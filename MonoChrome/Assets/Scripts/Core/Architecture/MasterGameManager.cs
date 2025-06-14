using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using MonoChrome.Systems.Combat;
using MonoChrome;
using MonoChrome.Events;
using MonoChrome.StatusEffects;
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
        private bool _isCombatInitializing = false;
        
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

            // 던전 이벤트 - 생성/이동 요청은 DungeonController가 직접 처리하도록 수정
            // DungeonEvents.OnDungeonGenerationRequested += OnDungeonGenerationRequested; // 제거됨
            // DungeonEvents.OnNodeMoveRequested += OnNodeMoveRequested; // 제거됨
            DungeonEvents.OnRoomActivityCompleted += OnRoomActivityCompleted;

            // 전투 이벤트
            DungeonEvents.CombatEvents.OnCombatStartRequested += OnCombatStartRequested;
            DungeonEvents.CombatEvents.OnCombatEndRequested += OnCombatEndRequested;

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
            // DungeonEvents.OnDungeonGenerationRequested -= OnDungeonGenerationRequested; // 제거됨
            // DungeonEvents.OnNodeMoveRequested -= OnNodeMoveRequested; // 제거됨
            DungeonEvents.OnRoomActivityCompleted -= OnRoomActivityCompleted;

            // 전투 이벤트
            DungeonEvents.CombatEvents.OnCombatStartRequested -= OnCombatStartRequested;
            DungeonEvents.CombatEvents.OnCombatEndRequested -= OnCombatEndRequested;

            LogDebug("게임 이벤트 구독 해제 완료");
        }

        private void OnDestroy()
        {
            // 씬 이벤트 구독 해제
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            // 게임 이벤트 구독 해제
            UnsubscribeFromGameEvents();

            // 중복 인스턴스가 파괴될 때 이벤트를 모두 정리하면
            // 이미 구독을 마친 다른 시스템들이 동작하지 않게 된다.
            // 따라서 싱글턴 본인일 때만 이벤트 버스를 정리한다.
            if (_instance == this)
            {
                _eventBus?.ClearAllEvents();
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
                "StatusEffectManager"
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
                "StatusEffectManager" => FindFirstObjectByType<StatusEffectManager>(FindObjectsInactive.Include)?.gameObject,
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
                    // GameScene 내에서는 이벤트를 정리하지 않음 (던전 생성 등을 위해 필요)
                    LogDebug("GameScene 데이터 정리 완료 (이벤트는 유지)");
                    break;
                case "MainMenu":
                    _eventBus?.ClearAllEvents(); // 메인 메뉴로 돌아갈 때만 이벤트 정리
                    LogDebug("메인 메뉴 데이터 정리 완료");
                    break;
            }
        }
        #endregion

        #region Game State Event Handlers
        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            LogDebug($"게임 상태 변경: {previousState} -> {newState}");
            
            // UI 패널 전환은 UIController가 GameStateMachine.OnStateChanged를 통해 직접 처리하므로
            // 여기서는 상태별 비즈니스 로직만 처리
        }

        private void OnGameStateEntered(GameStateMachine.GameState state)
        {
            LogDebug($"게임 상태 진입: {state}");
            
            // 던전 상태 진입 시 이벤트 재구독 (ClearAllEvents 호출로 인한 구독 해제 대응)
            if (state == GameStateMachine.GameState.Dungeon)
            {
                LogDebug("던전 진입 - 전투 이벤트 재구독 시도");
                // 중복 구독 방지를 위해 먼저 해제
                DungeonEvents.CombatEvents.OnCombatStartRequested -= OnCombatStartRequested;
                DungeonEvents.CombatEvents.OnCombatEndRequested -= OnCombatEndRequested;
                // 재구독
                DungeonEvents.CombatEvents.OnCombatStartRequested += OnCombatStartRequested;
                DungeonEvents.CombatEvents.OnCombatEndRequested += OnCombatEndRequested;
                LogDebug("전투 이벤트 재구독 완료");
            }
            
            // 상태 진입 시 특수 로직
            switch (state)
            {
                case GameStateMachine.GameState.Dungeon:
                    // 던전 상태 진입 시 던전 UI 업데이트 요청
                    StartCoroutine(RequestDungeonUIUpdateDelayed());
                    break;
                case GameStateMachine.GameState.Event:
                    DungeonEvents.UIEvents.RequestDungeonSubPanelShow("Event");
                    break;
                case GameStateMachine.GameState.Shop:
                    DungeonEvents.UIEvents.RequestDungeonSubPanelShow("Shop");
                    break;
                case GameStateMachine.GameState.Rest:
                    DungeonEvents.UIEvents.RequestDungeonSubPanelShow("Rest");
                    break;
            }
        }

        /// <summary>
        /// 지연된 던전 UI 업데이트 요청
        /// </summary>
        private IEnumerator RequestDungeonUIUpdateDelayed()
        {
            yield return new WaitForSeconds(0.2f);
            DungeonEvents.UIEvents.RequestDungeonUIUpdate();
        }
        #endregion

        #region Game Event Handlers


        private void OnRoomActivityCompleted()
        {
            LogDebug("방 활동 완료");
            
            // 던전으로 복귀
            _stateMachine?.TryChangeState(GameStateMachine.GameState.Dungeon);
        }

        private void OnCombatStartRequested(string enemyType, CharacterType characterType)
        {
            LogDebug($"[MasterGameManager] 전투 시작 요청 수신: {enemyType}, {characterType}");
            
            // 이미 전투 초기화 중인지 확인
            if (_isCombatInitializing)
            {
                LogDebug("이미 전투 초기화 중 - 요청 무시");
                return;
            }
            
            // 이미 Combat 상태인지 확인
            if (_stateMachine.CurrentState != GameStateMachine.GameState.Combat)
            {
                _stateMachine?.TryChangeState(GameStateMachine.GameState.Combat);
            }
            else
            {
                LogDebug("이미 Combat 상태임 - 상태 전환 생략");
            }
            
            // CombatSystem에게 전투 시작 요청
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
            _isCombatInitializing = true;
            
            try
            {
                // CombatSystem 준비 대기
                var combatSystem = FindFirstObjectByType<CombatSystem>();
                while (combatSystem == null)
                {
                    yield return null;
                    combatSystem = FindFirstObjectByType<CombatSystem>();
                }

                bool initializationSucceeded = false;

                try
                {
                    // 전투 초기화
                    combatSystem.InitializeCombat();
                    LogDebug("전투 시스템 초기화 완료");
                    initializationSucceeded = true;
                }
                catch (Exception ex)
                {
                    LogDebug($"전투 초기화 실패: {ex.Message}");
                }

                if (initializationSucceeded)
                {
                    yield return new WaitForSeconds(0.1f); // 짧은 지연 후 전투 시작
                    // 직접 전투 시작 - 이벤트 루프 방지
                    combatSystem.StartCombat(enemyType, characterType);
                    LogDebug($"전투 시작: {enemyType}, {characterType}");
                }
            }
            finally
            {
                _isCombatInitializing = false;
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
            yield return new WaitForSeconds(0.2f);
            
            LogDebug("캐릭터 선택 후 던전 전환 시작");
            
            // 1. 게임 상태 변경
            _stateMachine?.EnterDungeon();
            
            // 2. 던전 생성 이벤트 발행 (DungeonController가 직접 받아서 처리)
            LogDebug($"던전 생성 요청: 스테이지 {_currentStage}");
            
            // 이벤트 발행 전 구독 상태 확인
            var dungeonController = FindFirstObjectByType<DungeonController>();
            if (dungeonController == null)
            {
                LogDebug("오류: DungeonController를 찾을 수 없음!");
                yield break;
            }
            
            if (!dungeonController.gameObject.activeInHierarchy)
            {
                LogDebug("오류: DungeonController가 비활성 상태!");
                yield break;
            }
            
            LogDebug($"DungeonController 상태: 활성={dungeonController.gameObject.activeInHierarchy}, 이름={dungeonController.gameObject.name}");
            
            // 이벤트 발행
            DungeonEvents.RequestDungeonGeneration(_currentStage);
            
            // 0.3초 대기 후 구독자 수 확인
            yield return new WaitForSeconds(0.3f);
            
            // Fallback: 이벤트가 실패한 경우 직접 호출
            var currentSubscriberCount = DungeonEvents.GetSubscriberCount();
            if (currentSubscriberCount == 0)
            {
                LogDebug("이벤트 방식 실패 - 직접 DungeonController 호출로 전환");
                dungeonController.GenerateNewDungeon(_currentStage);
                LogDebug("직접 호출 방식으로 던전 생성 완료");
            }
            else
            {
                LogDebug($"이벤트 방식 성공 - 구독자 수: {currentSubscriberCount}");
            }
            
            // 3. 던전 생성 완료 대기
            yield return new WaitForSeconds(0.5f);
            
            LogDebug("던전 전환 완료");
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
            
            DungeonEvents.CombatEvents.RequestCombatStart(enemyType, characterType);
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
        
        #region Force UI Control (강제 UI 제어)
        /// <summary>
        /// 이벤트 기반 방식이 실패할 경우를 대비한 강제 패널 표시
        /// </summary>
        public void ForceShowCharacterSelection()
        {
            LogDebug("강제 캠릭터 선택 패널 표시");
            
            GameObject charPanel = GameObject.Find("CharacterSelectionPanel");
            GameObject dungeonPanel = GameObject.Find("DungeonPanel");
            GameObject combatPanel = GameObject.Find("CombatPanel");
            
            // 모든 패널 비활성화
            if (dungeonPanel != null) dungeonPanel.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);
            
            // 캠릭터 선택 패널 활성화
            if (charPanel != null)
            {
                charPanel.SetActive(true);
                LogDebug("캠릭터 선택 패널 강제 표시 완료");
            }
            else
            {
                LogDebug("경고: CharacterSelectionPanel을 찾을 수 없습니다!");
            }
        }
        
        /// <summary>
        /// 강제 던전 패널 표시
        /// </summary>
        public void ForceShowDungeon()
        {
            LogDebug("강제 던전 패널 표시");
            
            GameObject charPanel = GameObject.Find("CharacterSelectionPanel");
            GameObject dungeonPanel = GameObject.Find("DungeonPanel");
            GameObject combatPanel = GameObject.Find("CombatPanel");
            
            // 모든 패널 비활성화
            if (charPanel != null) charPanel.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);
            
            // 던전 패널 활성화
            if (dungeonPanel != null)
            {
                dungeonPanel.SetActive(true);
                LogDebug("던전 패널 강제 표시 완료");
                
                // 3개 방 버튼 생성
                StartCoroutine(CreateDungeonRoomButtonsDelayed());
            }
            else
            {
                LogDebug("경고: DungeonPanel을 찾을 수 없습니다!");
            }
        }
        
        /// <summary>
        /// 던전 방 버튼 생성 (지연 실행)
        /// </summary>
        private IEnumerator CreateDungeonRoomButtonsDelayed()
        {
            yield return new WaitForSeconds(0.2f);
            
            // 던전 UI 버튼 생성 로직
            LogDebug("던전 방 버튼 생성 시도");
            
            // DungeonController에게 던전 생성 요청
            var dungeonController = FindFirstObjectByType<DungeonController>();
            if (dungeonController != null)
            {
                dungeonController.GenerateNewDungeon(_currentStage);
                LogDebug("던전 생성 요청 완료");
            }
            else
            {
                LogDebug("경고: DungeonController를 찾을 수 없습니다");
            }
        }
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