using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Dungeon;
using MonoChrome.UI;

namespace MonoChrome
{
    /// <summary>
    /// 개선된 게임 매니저 - 시스템들의 조정자 역할만 담당
    /// 구체적인 게임 로직은 각 시스템에 위임 (조정자 패턴)
    /// </summary>
    public class ImprovedGameManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static ImprovedGameManager _instance;
        private static readonly object _lock = new object();
        private static bool _isApplicationQuitting = false;

        public static ImprovedGameManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<ImprovedGameManager>();
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("[ImprovedGameManager]");
                            _instance = go.AddComponent<ImprovedGameManager>();
                        }
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    return _instance;
                }
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Initialize();
        }

        private void OnApplicationQuit()
        {
            _isApplicationQuitting = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                Cleanup();
                _instance = null;
            }
        }
        #endregion

        #region System References
        [Header("핵심 시스템들")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        // 핵심 시스템들 - 초기화 시에만 사용
        private GameStateMachine _stateMachine;
        private EventBus _eventBus;
        
        // 시스템 상태 추적
        private bool _isInitialized = false;
        private bool _isInitializing = false;
        #endregion

        #region Initialization
        /// <summary>
        /// 시스템 초기화 - 생성과 연결만 담당
        /// </summary>
        private void Initialize()
        {
            if (_isInitialized || _isInitializing) return;
            
            _isInitializing = true;
            LogDebug("게임 매니저 초기화 시작");

            // 핵심 시스템들 생성
            CreateCoreSystem();
            
            // 씬 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            _isInitialized = true;
            _isInitializing = false;
            LogDebug("게임 매니저 초기화 완료");
        }

        /// <summary>
        /// 핵심 시스템들 생성
        /// </summary>
        private void CreateCoreSystem()
        {
            // 게임 상태 머신 생성
            if (_stateMachine == null)
            {
                _stateMachine = GameStateMachine.Instance;
                LogDebug("게임 상태 머신 생성됨");
            }

            // 이벤트 버스 생성
            if (_eventBus == null)
            {
                _eventBus = EventBus.Instance;
                LogDebug("이벤트 버스 생성됨");
            }
        }

        /// <summary>
        /// 시스템 정리
        /// </summary>
        private void Cleanup()
        {
            // 씬 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            // 이벤트 정리
            if (_eventBus != null)
            {
                _eventBus.ClearAllEvents();
            }
            
            LogDebug("게임 매니저 정리 완료");
        }
        #endregion

        #region Scene Management
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LogDebug($"씬 로드됨: {scene.name}");
            
            // 안전한 초기화를 위해 코루틴 사용
            StartCoroutine(InitializeSceneSystemsSafely(scene.name));
        }

        private void OnSceneUnloaded(Scene scene)
        {
            LogDebug($"씬 언로드됨: {scene.name}");
            
            // 이벤트 정리
            if (_eventBus != null)
            {
                _eventBus.ClearAllEvents();
            }
        }

        /// <summary>
        /// 씬별 시스템들 안전한 초기화
        /// </summary>
        private IEnumerator InitializeSceneSystemsSafely(string sceneName)
        {
            // 프레임 대기 (모든 오브젝트 초기화 완료 보장)
            yield return null;
            yield return null;

            switch (sceneName)
            {
                case "MainMenu":
                    InitializeMainMenuSystems();
                    break;
                    
                case "GameScene":
                    InitializeGameSystems();
                    break;
                    
                default:
                    LogDebug($"알 수 없는 씬: {sceneName}");
                    break;
            }
        }

        /// <summary>
        /// 메인 메뉴 시스템 초기화
        /// </summary>
        private void InitializeMainMenuSystems()
        {
            LogDebug("메인 메뉴 시스템 초기화");
            
            // 게임 상태를 메인 메뉴로 설정
            if (_stateMachine != null)
            {
                _stateMachine.TryChangeState(GameStateMachine.GameState.MainMenu);
            }
        }

        /// <summary>
        /// 게임 씬 시스템 초기화
        /// </summary>
        private void InitializeGameSystems()
        {
            LogDebug("게임 시스템 초기화 시작");
            
            // 시스템들이 스스로 초기화되도록 대기
            StartCoroutine(WaitForSystemsAndFinalize());
        }

        /// <summary>
        /// 모든 시스템이 준비될 때까지 대기 후 최종 설정
        /// </summary>
        private IEnumerator WaitForSystemsAndFinalize()
        {
            // 시스템들 준비 대기
            yield return WaitForSystemsReady();
            
            // 초기 상태 설정
            SetInitialGameState();
            
            LogDebug("게임 시스템 초기화 완료");
        }

        /// <summary>
        /// 핵심 시스템들이 준비될 때까지 대기
        /// </summary>
        private IEnumerator WaitForSystemsReady()
        {
            float timeout = 5f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                // 필수 시스템들 확인
                var dungeonController = FindObjectOfType<DungeonController>();
                var uiController = FindObjectOfType<UIController>();
                
                if (dungeonController != null && uiController != null)
                {
                    LogDebug("모든 핵심 시스템 준비됨");
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            LogDebug("시스템 준비 시간 초과 - 현재 상태로 진행");
        }

        /// <summary>
        /// 초기 게임 상태 설정
        /// </summary>
        private void SetInitialGameState()
        {
            if (_stateMachine != null)
            {
                // 현재 상태에 따라 적절한 초기 상태로 설정
                var currentState = _stateMachine.CurrentState;
                
                if (currentState == GameStateMachine.GameState.MainMenu)
                {
                    _stateMachine.TryChangeState(GameStateMachine.GameState.CharacterSelection);
                }
                // 이미 적절한 상태인 경우 그대로 유지
            }
        }
        #endregion

        #region Public API - 단순한 중개자 역할만
        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            LogDebug("새 게임 시작 요청");
            
            if (_stateMachine != null)
            {
                _stateMachine.StartNewGame();
            }
            
            // 게임 씬으로 전환이 필요한 경우
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// 던전 진입
        /// </summary>
        public void EnterDungeon()
        {
            LogDebug("던전 진입 요청");
            
            if (_stateMachine != null)
            {
                _stateMachine.EnterDungeon();
            }
            
            // 던전 생성 이벤트 발행
            DungeonEvents.RequestDungeonGeneration(0);
        }

        /// <summary>
        /// 메인 메뉴로 복귀
        /// </summary>
        public void ReturnToMainMenu()
        {
            LogDebug("메인 메뉴 복귀 요청");
            
            if (_stateMachine != null)
            {
                _stateMachine.ReturnToMainMenu();
            }
            
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            LogDebug("게임 종료 요청");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Utility
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[ImprovedGameManager] {message}");
            }
        }

        /// <summary>
        /// 디버그 모드 토글
        /// </summary>
        public void SetDebugMode(bool enabled)
        {
            _enableDebugLogs = enabled;
        }

        /// <summary>
        /// 현재 게임 상태 반환
        /// </summary>
        public GameStateMachine.GameState GetCurrentState()
        {
            return _stateMachine?.CurrentState ?? GameStateMachine.GameState.MainMenu;
        }

        /// <summary>
        /// 시스템 초기화 상태 확인
        /// </summary>
        public bool IsInitialized => _isInitialized;
        #endregion
    }
}
