using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoChrome.Combat;
using MonoChrome.Dungeon;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonoChrome
{
    /// <summary>
    /// 개선된 게임 매니저 - 씬 전환 시 안정성 보장
    /// 싱글톤 패턴과 매니저 참조 관리 최적화
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static GameManager _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;
        
        public static GameManager Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning("GameManager: Application is quitting, returning null");
                    return null;
                }
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<GameManager>();
                        
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("GameManager");
                            _instance = go.AddComponent<GameManager>();
                        }
                        
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    return _instance;
                }
            }
        }
        
        private void Awake()
        {
            // 인스턴스 중복 체크
            if (_instance != null && _instance != this)
            {
                Debug.Log("GameManager: Duplicate instance detected, destroying...");
                Destroy(gameObject);
                return;
            }
            
            // 싱글톤 설정
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            Debug.Log("GameManager: Singleton instance created and initialized");
        }
        
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
        
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            if (_instance == this)
            {
                _instance = null;
            }
        }
        #endregion
        
        #region Game State Management
        public enum GameState
        {
            MainMenu,
            CharacterSelection,
            Dungeon,
            Combat,
            Event,
            Shop,
            Rest,
            GameOver,
            Victory,
            Paused
        }
        
        private GameState _currentState = GameState.MainMenu;
        public event Action<GameState> OnGameStateChanged;
        
        public GameState CurrentState 
        { 
            get => _currentState;
            private set
            {
                if (_currentState != value)
                {
                    GameState oldState = _currentState;
                    _currentState = value;
                    OnGameStateChanged?.Invoke(_currentState);
                    Debug.Log($"GameManager: State changed from {oldState} to {_currentState}");
                }
            }
        }
        
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
        }
        #endregion
        
        #region Manager References - 개선된 참조 관리
        private UIManager _uiManager;
        private DungeonManager _dungeonManager;
        private CombatManager _combatManager;
        private FontManager _fontManager;
        
        // 프로퍼티로 안전한 접근 제공
        public UIManager UIManager 
        { 
            get 
            { 
                if (_uiManager == null)
                    RefreshManagerReferences();
                return _uiManager; 
            } 
        }
        
        public DungeonManager DungeonManager 
        { 
            get 
            { 
                if (_dungeonManager == null)
                    RefreshManagerReferences();
                return _dungeonManager; 
            } 
        }
        
        public CombatManager CombatManager 
        { 
            get 
            { 
                if (_combatManager == null)
                    RefreshManagerReferences();
                return _combatManager; 
            } 
        }
        
        /// <summary>
        /// 매니저 참조를 새로고침 - 씬 전환 시 호출
        /// 비활성화된 매니저도 찾고 필요시 활성화
        /// </summary>
        private void RefreshManagerReferences()
        {
            Debug.Log("GameManager: Refreshing manager references...");
            
            // UIManager 찾기 (씬에 있는 것 우선)
            UIManager[] uiManagers = FindObjectsOfType<UIManager>(true); // includeInactive: true
            _uiManager = uiManagers.FirstOrDefault(ui => ui.gameObject.scene.isLoaded);
            
            // DungeonManager 찾기 (비활성화된 것도 포함)
            DungeonManager[] dungeonManagers = FindObjectsOfType<DungeonManager>(true);
            _dungeonManager = dungeonManagers.FirstOrDefault();
            
            // 찾은 DungeonManager가 비활성화 상태면 활성화
            if (_dungeonManager != null && !_dungeonManager.gameObject.activeInHierarchy)
            {
                _dungeonManager.gameObject.SetActive(true);
                Debug.Log("GameManager: Activated DungeonManager");
            }
            
            // CombatManager 찾기 (비활성화된 것도 포함)
            CombatManager[] combatManagers = FindObjectsOfType<CombatManager>(true);
            _combatManager = combatManagers.FirstOrDefault();
            
            // 찾은 CombatManager가 비활성화 상태면 활성화
            if (_combatManager != null && !_combatManager.gameObject.activeInHierarchy)
            {
                _combatManager.gameObject.SetActive(true);
                Debug.Log("GameManager: Activated CombatManager");
            }
            
            Debug.Log($"GameManager: References refreshed - UI:{_uiManager != null}, Dungeon:{_dungeonManager != null}, Combat:{_combatManager != null}");
        }
        
        /// <summary>
        /// 씬의 매니저들을 활성화하는 메서드 (강화된 버전)
        /// </summary>
        private void ActivateSceneManagers()
        {
            Debug.Log("GameManager: Activating scene managers...");
            
            // 비활성화된 매니저들 찾아서 활성화
            var inactiveManagers = new[]
            {
                "CombatManager", "CoinManager", "PatternManager", "StatusEffectManager", "DungeonManager", "ManagerInitializer"
            };
            
            int activatedCount = 0;
            
            foreach (string managerName in inactiveManagers)
            {
                // 이름으로 직접 찾기
                GameObject managerObj = GameObject.Find(managerName);
                
                if (managerObj != null)
                {
                    if (!managerObj.activeInHierarchy)
                    {
                        managerObj.SetActive(true);
                        activatedCount++;
                        Debug.Log($"GameManager: Activated {managerName}");
                    }
                    else
                    {
                        Debug.Log($"GameManager: {managerName} already active");
                    }
                }
                else
                {
                    // 이름으로 찾지 못한 경우 타입으로 찾기
                    var managerComponent = FindManagerByType(managerName);
                    if (managerComponent != null)
                    {
                        if (!managerComponent.gameObject.activeInHierarchy)
                        {
                            managerComponent.gameObject.SetActive(true);
                            activatedCount++;
                            Debug.Log($"GameManager: Activated {managerName} (found by type)");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"GameManager: {managerName} not found in scene");
                    }
                }
            }
            
            Debug.Log($"GameManager: Activated {activatedCount} managers");
            
            // 매니저 활성화 후 참조 갱신
            RefreshManagerReferences();
        }
        
        /// <summary>
        /// 매니저 이름에 따라 타입으로 찾기
        /// </summary>
        private MonoBehaviour FindManagerByType(string managerName)
        {
            return managerName switch
            {
                "CombatManager" => FindObjectOfType<CombatManager>(true),
                "CoinManager" => FindObjectOfType<CoinManager>(true),
                "PatternManager" => FindObjectOfType<PatternManager>(true),
                "StatusEffectManager" => FindObjectOfType<StatusEffectManager>(true),
                "DungeonManager" => FindObjectOfType<DungeonManager>(true),
                _ => null
            };
        }
        #endregion
        
        #region Scene Event Handlers
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"GameManager: Scene loaded - {scene.name}");
            
            // 코루틴으로 안전한 초기화 수행
            StartCoroutine(SafeSceneInitialization(scene.name));
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            Debug.Log($"GameManager: Scene unloaded - {scene.name}");
            
            // 매니저 참조 정리
            if (scene.name == "GameScene")
            {
                _uiManager = null;
                _dungeonManager = null;
                _combatManager = null;
            }
        }
        
        /// <summary>
        /// 안전한 씬 초기화 - 프레임 대기 후 수행
        /// </summary>
        private IEnumerator SafeSceneInitialization(string sceneName)
        {
            // 2프레임 대기 (모든 오브젝트 초기화 완료 보장)
            yield return null;
            yield return null;
            
            // 매니저 참조 새로고침
            RefreshManagerReferences();
            
            // 씬별 초기화
            switch (sceneName)
            {
                case "MainMenu":
                    InitializeMainMenuScene();
                    break;
                    
                case "GameScene":
                    InitializeGameScene();
                    break;
            }
        }
        
        private void InitializeMainMenuScene()
        {
            Debug.Log("GameManager: Initializing MainMenu scene");
            ChangeState(GameState.MainMenu);
        }
        
        private void InitializeGameScene()
        {
            Debug.Log("GameManager: Initializing GameScene");
            
            // 씬의 매니저들 활성화
            ActivateSceneManagers();
            
            // 매니저 참조 다시 확인
            RefreshManagerReferences();
            
            // 폰트 매니저 초기화
            InitializeFontManager();
            
            // 현재 상태에 따른 처리
            switch (_currentState)
            {
                case GameState.MainMenu:
                    ChangeState(GameState.CharacterSelection);
                    break;
                    
                case GameState.CharacterSelection:
                    // 상태 유지
                    break;
                    
                case GameState.Dungeon:
                    // 던전 상태 복원
                    break;
                    
                case GameState.Combat:
                    // 전투 상태 복원
                    break;
                    
                default:
                    ChangeState(GameState.CharacterSelection);
                    break;
            }
        }
        
        private void InitializeFontManager()
        {
            _fontManager = FindObjectOfType<FontManager>();
            if (_fontManager == null)
            {
                GameObject fontManagerObject = new GameObject("FontManager");
                _fontManager = fontManagerObject.AddComponent<FontManager>();
                fontManagerObject.transform.SetParent(transform);
                Debug.Log("GameManager: Created new FontManager");
            }
        }
        #endregion
        
        #region Game Flow Methods
        public void StartNewGame()
        {
            Debug.Log("GameManager: Starting new game");
            ChangeState(GameState.CharacterSelection);
            
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
        }
        
        public void EnterDungeon()
        {
            Debug.Log("GameManager: Entering dungeon - Starting comprehensive initialization");
            
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
                return;
            }
            
            // 직접 던전 초기화 프로세스 시작
            StartCoroutine(InitializeDungeonCompletely());
        }
        
        /// <summary>
        /// 던전 초기화 전체 프로세스를 GameManager가 직접 제어
        /// </summary>
        private IEnumerator InitializeDungeonCompletely()
        {
            Debug.Log("GameManager: Starting complete dungeon initialization process");

            // 예외 처리를 위한 상태 변수
            bool hasError = false;
            string errorMessage = null;

            // 나중에 쓸 임시 참조들
            GameObject dungeonPanel = null;
            DungeonUI dungeonUI = null;

            // 예외 처리 부분은 yield 없이 try/catch만 사용
            try
            {
                // 1단계: 상태 변경
                ChangeState(GameState.Dungeon);

                // 2단계: DungeonPanel 활성화 대기
                float timeoutTime = Time.time + 5f;
                while (dungeonPanel == null && Time.time < timeoutTime)
                {
                    dungeonPanel = GameObject.Find("DungeonPanel");
                    if (dungeonPanel != null && dungeonPanel.activeInHierarchy)
                        break;
                }

                if (dungeonPanel == null || !dungeonPanel.activeInHierarchy)
                {
                    throw new Exception("DungeonPanel not activated within timeout.");
                }

                // 3단계: DungeonUI 존재 확인
                dungeonUI = dungeonPanel.GetComponent<DungeonUI>();
                if (dungeonUI == null)
                {
                    throw new Exception("DungeonUI component not found on DungeonPanel.");
                }

                // 4단계: DungeonManager 준비
                while (DungeonManager == null)
                {
                    RefreshManagerReferences();
                }

                // 5단계: 던전 생성 요청
                DungeonManager.GenerateNewDungeon(0);
            }
            catch (Exception ex)
            {
                hasError = true;
                errorMessage = $"GameManager: Error in dungeon initialization: {ex.Message}\n{ex.StackTrace}";
            }

            // 이제부터 yield return 실행 — try/catch 바깥
            yield return null;

            if (hasError)
            {
                Debug.LogError(errorMessage);
                yield break;
            }

            Debug.Log("GameManager: DungeonPanel is now active");
            yield return new WaitForSeconds(0.5f);
            Debug.Log("GameManager: DungeonUI initialization time provided");

            yield return new WaitForSeconds(0.2f);
            Debug.Log("GameManager: DungeonManager is ready");

            if (UIManager != null)
            {
                Debug.Log("GameManager: Requesting UI update...");
                // UIManager.UpdateDungeonUI(); // 필요 시
            }

            Debug.Log("GameManager: Dungeon initialization completed successfully!");
        }

        
        public void StartCombat()
        {
            Debug.Log("GameManager: Starting combat");
            ChangeState(GameState.Combat);
            
            StartCoroutine(InitializeCombatSafely());
        }
        
        private IEnumerator InitializeCombatSafely()
        {
            // 전투 매니저가 준비될 때까지 대기
            while (CombatManager == null)
            {
                yield return null;
            }
            
            try
            {
                CombatManager.InitializeCombat();
                Debug.Log("GameManager: Successfully initialized combat");
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error initializing combat: {ex.Message}");
            }
        }
        
        public void StartEvent()
        {
            ChangeState(GameState.Event);
        }
        
        public void StartShop()
        {
            ChangeState(GameState.Shop);
        }
        
        public void StartRest()
        {
            ChangeState(GameState.Rest);
        }
        
        public void EndCombat(bool victory)
        {
            if (victory)
            {
                ChangeState(GameState.Dungeon);
            }
            else
            {
                ChangeState(GameState.GameOver);
            }
        }
        
        public void EndEvent()
        {
            ChangeState(GameState.Dungeon);
        }
        
        public void EndShop()
        {
            ChangeState(GameState.Dungeon);
        }
        
        public void EndRest()
        {
            ChangeState(GameState.Dungeon);
        }
        
        public void GameOver()
        {
            ChangeState(GameState.GameOver);
            UIManager?.ShowGameOverScreen();
        }
        
        public void Victory()
        {
            ChangeState(GameState.Victory);
            UIManager?.ShowVictoryScreen();
        }
        
        public void ReturnToMainMenu()
        {
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }
        #endregion
        
        #region UI Panel Management
        public void SwitchPanel(string panelName)
        {
            UIManager?.OnPanelSwitched(panelName);
        }
        #endregion
    }
}