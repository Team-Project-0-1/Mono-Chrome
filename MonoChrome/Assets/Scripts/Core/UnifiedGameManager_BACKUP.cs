using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoChrome.Events;

namespace MonoChrome.Core
{
    /// <summary>
    /// 통합 게임 매니저 - 기존과 신규 시스템의 장점을 결합
    /// 책임: 시스템 생명주기 관리 + 이벤트 기반 조정
    /// 
    /// 설계 원칙:
    /// - 단일 책임: 시스템 조정 및 생명주기 관리만 담당
    /// - 낮은 결합도: 직접 참조 최소화, 이벤트 기반 통신
    /// - 높은 응집도: 관련된 시스템 관리 기능들을 한 곳에 집중
    /// - 확장성: 새로운 시스템 추가 시 최소한의 변경
    /// </summary>
    public class UnifiedGameManager : MonoBehaviour
    {
        #region Singleton Pattern (Thread-Safe)
        private static UnifiedGameManager _instance;
        private static readonly object _lock = new object();
        private static bool _isApplicationQuitting = false;

        public static UnifiedGameManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<UnifiedGameManager>();
                        if (_instance == null)
                        {
                            var go = new GameObject("[UnifiedGameManager]");
                            _instance = go.AddComponent<UnifiedGameManager>();
                            DontDestroyOnLoad(go);
                        }
                    }
                    return _instance;
                }
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Debug.Log("[UnifiedGameManager] 중복 인스턴스 감지, 제거합니다.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeManager();
        }

        private void OnApplicationQuit() => _isApplicationQuitting = true;
        #endregion

        #region Core System References (최소한의 의존성)
        [Header("시스템 설정")]
        [SerializeField] private bool _enableDebugLogs = true;
        
        // 핵심 시스템만 직접 참조
        private GameStateMachine _stateMachine;
        private EventBus _eventBus;
        
        // 시스템 상태 추적
        private bool _isInitialized = false;
        #endregion

        #region Initialization
        private void InitializeManager()
        {
            if (_isInitialized) return;
            
            LogDebug("통합 게임 매니저 초기화 시작...");
            
            // 핵심 시스템 생성
            CreateCoreSystem();
            
            // 씬 관리 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            _isInitialized = true;
            LogDebug("통합 게임 매니저 초기화 완료");
        }

        private void CreateCoreSystem()
        {
            // 게임 상태 머신 초기화
            _stateMachine = GameStateMachine.Instance;
            if (_stateMachine == null)
            {
                LogDebug("경고: GameStateMachine을 찾을 수 없습니다.");
            }
            
            // 이벤트 버스 초기화
            _eventBus = EventBus.Instance;
            if (_eventBus == null)
            {
                LogDebug("경고: EventBus를 찾을 수 없습니다.");
            }
        }

        private void OnDestroy()
        {
            // 씬 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            // 이벤트 정리
            _eventBus?.ClearAllEvents();
            
            if (_instance == this)
            {
                _instance = null;
            }
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
            
            // 씬별 정리 작업
            CleanupSceneData(scene.name);
        }

        private IEnumerator InitializeSceneSystemsSafely(string sceneName)
        {
            // 시스템들이 자체 초기화될 시간 제공
            yield return new WaitForSeconds(0.1f);
            
            switch (sceneName)
            {
                case "MainMenu":
                    yield return StartCoroutine(InitializeMainMenuScene());
                    break;
                    
                case "GameScene":
                    yield return StartCoroutine(InitializeGameScene());
                    break;
                    
                default:
                    LogDebug($"알 수 없는 씬: {sceneName}");
                    break;
            }
        }

        private IEnumerator InitializeMainMenuScene()
        {
            LogDebug("메인 메뉴 씬 초기화");
            
            // 게임 상태 설정
            _stateMachine?.TryChangeState(GameStateMachine.GameState.MainMenu);
            
            yield return null;
        }

        private IEnumerator InitializeGameScene()
        {
            LogDebug("게임 씬 초기화 시작");
            
            // 1. 필수 시스템들이 준비될 때까지 대기
            yield return StartCoroutine(WaitForEssentialSystems());
            
            // 2. 레거시 매니저들 활성화 (호환성 유지)
            ActivateLegacyManagers();
            
            // 3. 상태 설정
            SetInitialGameState();
            
            LogDebug("게임 씬 초기화 완료");
        }

        private IEnumerator WaitForEssentialSystems()
        {
            float timeout = 5f;
            float elapsed = 0f;
            
            LogDebug("필수 시스템 준비 대기 중...");
            
            while (elapsed < timeout)
            {
                // 핵심 시스템들 확인 (직접 참조 없이 존재 여부만 확인)
                bool dungeonControllerReady = FindFirstObjectByType<MonoChrome.Dungeon.DungeonController>() != null;
                bool uiControllerReady = FindFirstObjectByType<MonoChrome.UI.UIController>() != null;
                bool dataConnectorReady = DataConnector.Instance != null && DataConnector.Instance.IsInitialized;
                
                if (dungeonControllerReady && uiControllerReady && dataConnectorReady)
                {
                    LogDebug("모든 필수 시스템 준비 완료");
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            LogDebug("시스템 준비 시간 초과 - 현재 상태로 진행");
        }

        private void ActivateLegacyManagers()
        {
            // 레거시 매니저들 활성화 (기존 시스템과의 호환성 유지)
            var inactiveManagers = new[]
            {
                "CombatManager", "CoinManager", "PatternManager", "StatusEffectManager"
            };
            
            foreach (string managerName in inactiveManagers)
            {
                GameObject managerObj = GameObject.Find(managerName);
                if (managerObj != null && !managerObj.activeInHierarchy)
                {
                    managerObj.SetActive(true);
                    LogDebug($"레거시 매니저 활성화: {managerName}");
                }
            }
        }

        private void SetInitialGameState()
        {
            if (_stateMachine == null) return;
            
            // 현재 상태에 따른 초기 설정
            var currentState = _stateMachine.CurrentState;
            
            switch (currentState)
            {
                case GameStateMachine.GameState.MainMenu:
                    _stateMachine.TryChangeState(GameStateMachine.GameState.CharacterSelection);
                    break;
                    
                case GameStateMachine.GameState.CharacterSelection:
                case GameStateMachine.GameState.Dungeon:
                case GameStateMachine.GameState.Combat:
                    // 상태 유지
                    break;
                    
                default:
                    _stateMachine.TryChangeState(GameStateMachine.GameState.CharacterSelection);
                    break;
            }
        }

        private void CleanupSceneData(string sceneName)
        {
            switch (sceneName)
            {
                case "GameScene":
                    // 게임 씬 데이터 정리
                    _eventBus?.ClearAllEvents();
                    break;
            }
        }
        #endregion

        #region Public API (간단한 중개자 인터페이스)
        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            LogDebug("새 게임 시작 요청");
            
            _stateMachine?.StartNewGame();
            
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// 던전 진입 - 이벤트 기반으로 처리
        /// </summary>
        public void EnterDungeon()
        {
            LogDebug("던전 진입 요청");
            
            _stateMachine?.EnterDungeon();
            
            // 이벤트 기반으로 던전 생성 요청
            DungeonEvents.RequestDungeonGeneration(0);
        }

        /// <summary>
        /// 전투 시작 - 이벤트 기반으로 처리
        /// </summary>
        public void StartCombat(string enemyType, CharacterType characterType)
        {
            LogDebug($"전투 시작 요청: {enemyType}, {characterType}");
            
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Combat);
            
            // 이벤트 기반으로 전투 시작 요청
            CombatEvents.RequestCombatStart(enemyType, characterType);
        }

        /// <summary>
        /// 메인 메뉴로 복귀
        /// </summary>
        public void ReturnToMainMenu()
        {
            LogDebug("메인 메뉴 복귀 요청");
            
            _stateMachine?.ReturnToMainMenu();
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

        #region Legacy System Bridge (호환성 유지)
        /// <summary>
        /// 레거시 시스템과의 호환성을 위한 브릿지 메서드들
        /// 점진적으로 제거할 예정
        /// </summary>
        public void EndCombat(bool victory)
        {
            LogDebug($"전투 종료: 승리 {victory}");
            
            if (victory)
            {
                _stateMachine?.TryChangeState(GameStateMachine.GameState.Dungeon);
            }
            else
            {
                _stateMachine?.TryChangeState(GameStateMachine.GameState.GameOver);
            }
        }

        public void StartEvent()
        {
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Event);
        }

        public void StartShop()
        {
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Shop);
        }

        public void StartRest()
        {
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Rest);
        }
        #endregion

        #region Status Properties
        public bool IsInitialized => _isInitialized;
        
        public GameStateMachine.GameState CurrentState => 
            _stateMachine?.CurrentState ?? GameStateMachine.GameState.MainMenu;
        #endregion

        #region Utility
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[UnifiedGameManager] {message}");
            }
        }

        /// <summary>
        /// 디버그 모드 토글 (에디터/테스트용)
        /// </summary>
        [ContextMenu("Toggle Debug Mode")]
        public void ToggleDebugMode()
        {
            _enableDebugLogs = !_enableDebugLogs;
            LogDebug($"디버그 모드: {_enableDebugLogs}");
        }
        #endregion
    }
}
