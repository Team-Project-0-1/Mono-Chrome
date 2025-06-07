using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoChrome.Systems.Combat;
using MonoChrome;
using MonoChrome.Events;
using MonoChrome.StatusEffects;
using MonoChrome;
using MonoChrome.Core.Data;

namespace MonoChrome.Core
{
    /// <summary>
    /// 마스터 게임 매니저 - 모든 시스템을 조정하는 중앙 컨트롤러
    /// 설계 원칙:
    /// - 단일 책임: 시스템 조정 및 생명주기 관리
    /// - 이벤트 기반: 직접 참조 최소화, 느슨한 결합
    /// - 확장 가능: 새 시스템 추가 시 최소 변경
    /// - 포트폴리오 품질: 명확한 구조와 코드 가독성
    /// </summary>
    public class MasterGameManager : MonoBehaviour, IGameManager
    {
        #region Singleton Pattern (Thread-Safe)
        private static MasterGameManager _instance;
        private static readonly object _lock = new object();
        private static bool _isApplicationQuitting = false;

        public static MasterGameManager Instance
        {
            get
            {
                if (_isApplicationQuitting) return null;
                
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<MasterGameManager>();
                        if (_instance == null)
                        {
                            GameObject go = new GameObject("[MasterGameManager]");
                            _instance = go.AddComponent<MasterGameManager>();
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
                LogDebug("중복 인스턴스 감지, 제거합니다.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeMasterSystem();
        }

        private void OnApplicationQuit() => _isApplicationQuitting = true;
        #endregion

        #region Core System Configuration
        [Header("시스템 설정")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _autoInitializeSystems = true;
        [SerializeField] private float _systemInitializationDelay = 0.1f;
        
        // 핵심 시스템 상태 추적
        private bool _isInitialized = false;
        private bool _systemsReady = false;
        
        // 현재 게임 데이터 (단순화)
        private string _selectedCharacterName = "";
        private int _currentStage = 0;
        #endregion

        #region Core System References (최소한의 직접 참조)
        private GameStateMachine _stateMachine;
        private EventBus _eventBus;
        private DataConnector _dataConnector;
        #endregion

        #region Initialization & Lifecycle
        /// <summary>
        /// 마스터 시스템 초기화
        /// </summary>
        private void InitializeMasterSystem()
        {
            if (_isInitialized) return;
            
            LogDebug("마스터 게임 매니저 초기화 시작...");

            // 핵심 시스템 생성 및 연결
            CreateAndConnectCoreSystem();
            
            // 씬 관리 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
            // 게임 이벤트 구독
            SubscribeToGameEvents();
            
            _isInitialized = true;
            LogDebug("마스터 게임 매니저 초기화 완료");
        }

        /// <summary>
        /// 핵심 시스템 생성 및 연결
        /// </summary>
        private void CreateAndConnectCoreSystem()
        {
            // ServiceLocator에 핵심 시스템들 등록
            
            // 게임 상태 머신 확보
            _stateMachine = GameStateMachine.Instance;
            if (_stateMachine == null)
            {
                LogDebug("경고: GameStateMachine을 찾을 수 없습니다.");
            }
            else
            {
                ServiceLocator.Register<GameStateMachine>(_stateMachine);
            }

            // 이벤트 버스 확보
            _eventBus = EventBus.Instance;
            if (_eventBus == null)
            {
                LogDebug("경고: EventBus를 찾을 수 없습니다.");
            }
            else
            {
                ServiceLocator.Register<EventBus>(_eventBus);
            }

            // 데이터 커넥터 확보
            _dataConnector = DataConnector.Instance;
            if (_dataConnector == null)
            {
                LogDebug("경고: DataConnector를 찾을 수 없습니다.");
            }
            else
            {
                ServiceLocator.Register<DataConnector>(_dataConnector);
            }

            // 자신을 IGameManager로 등록
            ServiceLocator.Register<IGameManager>(this);
            
            ServiceLocator.MarkAsInitialized();
            LogDebug("ServiceLocator에 핵심 시스템 등록 완료");
        }

        /// <summary>
        /// 게임 이벤트 구독
        /// </summary>
        private void SubscribeToGameEvents()
        {
            // 상태 머신 이벤트
            if (_stateMachine != null)
            {
                GameStateMachine.OnStateChanged += OnGameStateChanged;
                GameStateMachine.OnStateEntered += OnGameStateEntered;
            }

            // 던전 이벤트
            DungeonEvents.OnDungeonGenerationRequested += OnDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested += OnNodeMoveRequested;
            DungeonEvents.OnRoomActivityCompleted += OnRoomActivityCompleted;

            // 전투 이벤트
            CombatEvents.OnCombatStartRequested += OnCombatStartRequested;
            CombatEvents.OnCombatEndRequested += OnCombatEndRequested;

            LogDebug("게임 이벤트 구독 완료");
        }

        /// <summary>
        /// 게임 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromGameEvents()
        {
            // 상태 머신 이벤트
            GameStateMachine.OnStateChanged -= OnGameStateChanged;
            GameStateMachine.OnStateEntered -= OnGameStateEntered;

            // 던전 이벤트
            DungeonEvents.OnDungeonGenerationRequested -= OnDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested -= OnNodeMoveRequested;
            DungeonEvents.OnRoomActivityCompleted -= OnRoomActivityCompleted;

            // 전투 이벤트
            CombatEvents.OnCombatStartRequested -= OnCombatStartRequested;
            CombatEvents.OnCombatEndRequested -= OnCombatEndRequested;

            LogDebug("게임 이벤트 구독 해제 완료");
        }

        private void OnDestroy()
        {
            // 씬 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
            // 게임 이벤트 구독 해제
            UnsubscribeFromGameEvents();
            
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
            
            // 씬별 초기화 수행
            StartCoroutine(InitializeSceneSystemsSafely(scene.name));
        }

        private void OnSceneUnloaded(Scene scene)
        {
            LogDebug($"씬 언로드됨: {scene.name}");
            
            // 씬별 정리 작업
            CleanupSceneData(scene.name);
        }

        /// <summary>
        /// 씬별 시스템 안전한 초기화
        /// </summary>
        private IEnumerator InitializeSceneSystemsSafely(string sceneName)
        {
            // 시스템들이 자체 초기화될 시간 제공
            yield return new WaitForSeconds(_systemInitializationDelay);

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

        /// <summary>
        /// 메인 메뉴 씬 초기화
        /// </summary>
        private IEnumerator InitializeMainMenuScene()
        {
            LogDebug("메인 메뉴 씬 초기화");
            
            // 게임 상태 설정
            _stateMachine?.TryChangeState(GameStateMachine.GameState.MainMenu);
            
            yield return null;
        }

        /// <summary>
        /// 게임 씬 초기화
        /// </summary>
        private IEnumerator InitializeGameScene()
        {
            LogDebug("게임 씬 초기화 시작");
            
            // 1. 필수 시스템들이 준비될 때까지 대기
            yield return StartCoroutine(WaitForEssentialSystems());
            
            // 2. 레거시 매니저들 활성화 (호환성 유지)
            if (_autoInitializeSystems)
            {
                ActivateSceneManagers();
            }
            
            // 3. 초기 상태 설정
            SetInitialGameState();
            
            _systemsReady = true;
            LogDebug("게임 씬 초기화 완료");
        }

        /// <summary>
        /// 필수 시스템 준비 대기
        /// </summary>
        private IEnumerator WaitForEssentialSystems()
        {
            float timeout = 5f;
            float elapsed = 0f;
            
            LogDebug("필수 시스템 준비 대기 중...");
            
            while (elapsed < timeout)
            {
                // 필수 시스템들 확인
                bool dataConnectorReady = _dataConnector != null && _dataConnector.IsInitialized;
                bool uiControllerReady = FindFirstObjectByType<UIController>() != null;
                bool dungeonControllerReady = FindFirstObjectByType<DungeonController>() != null;
                
                if (dataConnectorReady && uiControllerReady && dungeonControllerReady)
                {
                    LogDebug("모든 필수 시스템 준비 완료");
                    yield break;
                }
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            LogDebug("시스템 준비 시간 초과 - 현재 상태로 진행");
        }

        /// <summary>
        /// 씬 매니저들 활성화
        /// </summary>
        private void ActivateSceneManagers()
        {
            LogDebug("씬 매니저들 활성화 중...");
            
            // 비활성화된 레거시 매니저들 찾아서 활성화
            var managerNames = new[]
            {
                "CombatManager", "CoinManager", "PatternManager", 
                "StatusEffectManager", "DungeonManager"
            };
            
            int activatedCount = 0;
            
            foreach (string managerName in managerNames)
            {
                GameObject managerObj = FindManagerObject(managerName);
                if (managerObj != null && !managerObj.activeInHierarchy)
                {
                    managerObj.SetActive(true);
                    activatedCount++;
                    LogDebug($"매니저 활성화: {managerName}");
                }
            }
            
            LogDebug($"총 {activatedCount}개 매니저 활성화 완료");
        }

        /// <summary>
        /// 매니저 오브젝트 찾기 (이름 또는 타입으로)
        /// </summary>
        private GameObject FindManagerObject(string managerName)
        {
            // 먼저 이름으로 찾기
            GameObject obj = GameObject.Find(managerName);
            if (obj != null) return obj;
            
            // 타입으로 찾기
            return managerName switch
            {
                "CombatManager" => FindFirstObjectByType<CombatManager>(FindObjectsInactive.Include)?.gameObject,
                "CoinManager" => FindFirstObjectByType<CoinManager>(FindObjectsInactive.Include)?.gameObject,
                "PatternManager" => FindFirstObjectByType<PatternManager>(FindObjectsInactive.Include)?.gameObject,
                "StatusEffectManager" => FindFirstObjectByType<StatusEffectManager>(FindObjectsInactive.Include)?.gameObject,
                "DungeonManager" => FindFirstObjectByType<DungeonManager>(FindObjectsInactive.Include)?.gameObject,
                _ => null
            };
        }

        /// <summary>
        /// 초기 게임 상태 설정
        /// </summary>
        private void SetInitialGameState()
        {
            if (_stateMachine == null) return;
            
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

        /// <summary>
        /// 씬 데이터 정리
        /// </summary>
        private void CleanupSceneData(string sceneName)
        {
            switch (sceneName)
            {
                case "GameScene":
                    _systemsReady = false;
                    _eventBus?.ClearAllEvents();
                    break;
            }
        }
        #endregion

        #region Game State Event Handlers
        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            LogDebug($"게임 상태 변경: {previousState} -> {newState}");
            
            // 상태별 특수 처리
            switch (newState)
            {
                case GameStateMachine.GameState.CharacterSelection:
                    UIEvents.RequestPanelShow("CharacterSelectionPanel");
                    break;
                    
                case GameStateMachine.GameState.Dungeon:
                    UIEvents.RequestPanelShow("DungeonPanel");
                    break;
                    
                case GameStateMachine.GameState.Combat:
                    UIEvents.RequestPanelShow("CombatPanel");
                    break;
                    
                case GameStateMachine.GameState.GameOver:
                    UIEvents.RequestPanelShow("GameOverPanel");
                    break;
                    
                case GameStateMachine.GameState.Victory:
                    UIEvents.RequestPanelShow("VictoryPanel");
                    break;
            }
        }

        private void OnGameStateEntered(GameStateMachine.GameState state)
        {
            LogDebug($"게임 상태 진입: {state}");
            
            // 상태 진입 시 특수 로직
            switch (state)
            {
                case GameStateMachine.GameState.Dungeon:
                    // 던전 상태 진입 시 던전 UI 업데이트 요청
                    StartCoroutine(RequestDungeonUIUpdateDelayed());
                    break;
            }
        }

        /// <summary>
        /// 지연된 던전 UI 업데이트 요청
        /// </summary>
        private IEnumerator RequestDungeonUIUpdateDelayed()
        {
            yield return new WaitForSeconds(0.2f);
            UIEvents.RequestDungeonUIUpdate();
        }
        #endregion

        #region Game Event Handlers
        private void OnDungeonGenerationRequested(int stageIndex)
        {
            LogDebug($"던전 생성 요청: 스테이지 {stageIndex}");
            
            _currentStage = stageIndex;
            
            // DungeonController에게 던전 생성 요청
            var dungeonController = FindFirstObjectByType<DungeonController>();
            if (dungeonController != null)
            {
                dungeonController.GenerateNewDungeon(stageIndex);
            }
            else
            {
                LogDebug("경고: DungeonController를 찾을 수 없습니다.");
            }
        }

        private void OnNodeMoveRequested(int nodeIndex)
        {
            LogDebug($"노드 이동 요청: {nodeIndex}");
            
            // DungeonController에게 노드 이동 요청
            var dungeonController = FindFirstObjectByType<DungeonController>();
            if (dungeonController != null)
            {
                dungeonController.MoveToNode(nodeIndex);
            }
        }

        private void OnRoomActivityCompleted()
        {
            LogDebug("방 활동 완료");
            
            // 던전으로 복귀
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Dungeon);
        }

        private void OnCombatStartRequested(string enemyType, CharacterType characterType)
        {
            LogDebug($"전투 시작 요청: {enemyType}, {characterType}");
            
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Combat);
            
            // CombatManager에게 전투 시작 요청
            StartCoroutine(InitializeCombatSafely(enemyType, characterType));
        }

        private void OnCombatEndRequested(bool victory)
        {
            LogDebug($"전투 종료 요청: 승리 {victory}");
            
            if (victory)
            {
                _stateMachine?.TryChangeState(GameStateMachine.GameState.Dungeon);
            }
            else
            {
                _stateMachine?.TryChangeState(GameStateMachine.GameState.GameOver);
            }
        }

        /// <summary>
        /// 안전한 전투 초기화
        /// </summary>
        private IEnumerator InitializeCombatSafely(string enemyType, CharacterType characterType)
        {
            // CombatManager 준비 대기
            var combatManager = FindFirstObjectByType<CombatManager>();
            while (combatManager == null)
            {
                yield return null;
                combatManager = FindFirstObjectByType<CombatManager>();
            }
            
            try
            {
                // 전투 초기화
                combatManager.InitializeCombat();
                LogDebug("전투 초기화 완료");
            }
            catch (Exception ex)
            {
                LogDebug($"전투 초기화 실패: {ex.Message}");
            }
        }
        #endregion

        #region Public API - 게임 플로우 제어
        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            LogDebug("새 게임 시작");
            
            // 게임 데이터 초기화
            _selectedCharacterName = "";
            _currentStage = 0;
            
            _stateMachine?.StartNewGame();
            
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        /// <summary>
        /// 캐릭터 선택
        /// </summary>
        public void SelectCharacter(string characterName)
        {
            LogDebug($"캐릭터 선택: {characterName}");
            
            _selectedCharacterName = characterName;
            
            // 던전 진입
            StartCoroutine(TransitionToDungeonAfterCharacterSelection());
        }

        /// <summary>
        /// 캐릭터 선택 후 던전 전환
        /// </summary>
        private IEnumerator TransitionToDungeonAfterCharacterSelection()
        {
            yield return new WaitForSeconds(0.1f);
            
            _stateMachine?.EnterDungeon();
            DungeonEvents.RequestDungeonGeneration(_currentStage);
        }

        /// <summary>
        /// 던전 진입
        /// </summary>
        public void EnterDungeon()
        {
            LogDebug("던전 진입");
            
            _stateMachine?.EnterDungeon();
            DungeonEvents.RequestDungeonGeneration(_currentStage);
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat(string enemyType = "약탈자1", CharacterType characterType = CharacterType.Normal)
        {
            LogDebug($"전투 시작: {enemyType}, {characterType}");
            
            CombatEvents.RequestCombatStart(enemyType, characterType);
        }

        /// <summary>
        /// 메인 메뉴로 복귀
        /// </summary>
        public void ReturnToMainMenu()
        {
            LogDebug("메인 메뉴 복귀");
            
            _stateMachine?.ReturnToMainMenu();
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>
        /// 게임 종료
        /// </summary>
        public void QuitGame()
        {
            LogDebug("게임 종료");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        #endregion

        #region Legacy Support Methods (호환성 유지)
        /// <summary>
        /// 레거시 시스템과의 호환성을 위한 메서드들
        /// 점진적으로 제거할 예정
        /// </summary>
        public void EndCombat(bool victory) => OnCombatEndRequested(victory);
        public void StartEvent() => _stateMachine?.TryChangeState(GameStateMachine.GameState.Event);
        public void StartShop() => _stateMachine?.TryChangeState(GameStateMachine.GameState.Shop);
        public void StartRest() => _stateMachine?.TryChangeState(GameStateMachine.GameState.Rest);
        public void GameOver() => _stateMachine?.TryChangeState(GameStateMachine.GameState.GameOver);
        public void Victory() => _stateMachine?.TryChangeState(GameStateMachine.GameState.Victory);
        #endregion

        #region Status & Debug
        /// <summary>
        /// 현재 게임 상태
        /// </summary>
        public GameStateMachine.GameState CurrentState => 
            _stateMachine?.CurrentState ?? GameStateMachine.GameState.MainMenu;

        /// <summary>
        /// 시스템 준비 상태
        /// </summary>
        public bool IsInitialized => _isInitialized;
        public bool SystemsReady => _systemsReady;
        public string SelectedCharacterName => _selectedCharacterName;
        public int CurrentStage => _currentStage;

        /// <summary>
        /// 디버깅 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[MasterGameManager] {message}");
            }
        }

        /// <summary>
        /// 시스템 상태 리포트 생성 (디버그용)
        /// </summary>
        [ContextMenu("Generate System Status Report")]
        public void GenerateSystemStatusReport()
        {
            LogDebug("=== 시스템 상태 리포트 ===");
            LogDebug($"초기화됨: {_isInitialized}");
            LogDebug($"시스템 준비됨: {_systemsReady}");
            LogDebug($"현재 상태: {CurrentState}");
            LogDebug($"선택된 캐릭터: {_selectedCharacterName}");
            LogDebug($"현재 스테이지: {_currentStage}");
            LogDebug($"GameStateMachine: {(_stateMachine != null ? "활성" : "비활성")}");
            LogDebug($"EventBus: {(_eventBus != null ? "활성" : "비활성")}");
            LogDebug($"DataConnector: {(_dataConnector != null ? "활성" : "비활성")}");
            LogDebug("========================");
        }
        #endregion
    }
}