using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using MonoChrome.Dungeon;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonoChrome
{
    /// <summary>
    /// 게임의 전체적인 상태를 관리하는 싱글톤 매니저 클래스
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton
        private static GameManager _instance;
        
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
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
            
            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            InitializeManagers();
        }
        
        // 씬 로드 이벤트 핸들러
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"GameManager: Scene loaded - {scene.name}");
    
            // 현재 씬에 따른 적절한 상태 설정
            if (scene.name == "MainMenu")
            {
                if (_currentState != GameState.MainMenu)
                {
                    ChangeState(GameState.MainMenu);
                }
            }
            else if (scene.name == "GameScene")
            {
                Debug.Log("GameManager: GameScene loaded - checking state");
        
                // MainMenu → GameScene으로 바로 이동한 경우에만 CharacterSelection으로 설정
                if (_currentState == GameState.MainMenu)
                {
                    Debug.Log("GameManager: Showing character selection panel");
                    ChangeState(GameState.CharacterSelection);
                }
                // 이미 캐릭터 선택 중이었다면 상태 유지
                else if (_currentState != GameState.CharacterSelection)
                {
                    Debug.Log($"GameManager: Current state is {_currentState}, maintaining current state");
                }
                
                // 관리자 참조 다시 초기화
                InitializeManagers();
                
                // 폰트 매니저 초기화
                InitializeFontManager();
            }
        }
        
        // 씬 로드 이벤트 구독 해제
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        #endregion
        
        #region Game State Management
        
        // 게임 상태 열거형
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
        
        // 현재 게임 상태
        private GameState _currentState;
        
        // 게임 상태 변경 이벤트
        public event Action<GameState> OnGameStateChanged;
        
        // 현재 게임 상태 프로퍼티
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
                    Debug.Log($"Game State changed from {oldState} to {_currentState}");
                }
            }
        }
        
        // 게임 상태 변경 메서드
        public void ChangeState(GameState newState)
        {
            CurrentState = newState;
        }
        #endregion
        
        #region Manager References
        // 다른 매니저 참조
        public UIManager UIManager { get; private set; }
        public DungeonManager DungeonManager { get; private set; }
        public CombatManager CombatManager { get; private set; } // 네임스페이스 명시
        private FontManager FontManager { get; set; }
        
        // 매니저 초기화
        private void InitializeManagers()
        {
            // UI 매니저 참조 찾기 또는 생성
            UIManager = FindObjectOfType<UIManager>();
            if (UIManager == null)
            {
                GameObject uiManagerObject = new GameObject("UIManager");
                UIManager = uiManagerObject.AddComponent<UIManager>();
                uiManagerObject.transform.SetParent(transform);
                Debug.Log("Created new UIManager");
            }
            else
            {
                Debug.Log("Found existing UIManager");
            }
            
            // 던전 매니저 초기화
            DungeonManager = FindObjectOfType<DungeonManager>();
            if (DungeonManager == null)
            {
                GameObject dungeonManagerObject = new GameObject("DungeonManager");
                DungeonManager = dungeonManagerObject.AddComponent<DungeonManager>();
                dungeonManagerObject.transform.SetParent(transform);
                Debug.Log("Created new DungeonManager");
            }
            else
            {
                Debug.Log("Found existing DungeonManager");
            }
            
            // 전투 매니저 초기화
            CombatManager = FindObjectOfType<CombatManager>();
            if (CombatManager == null)
            {
                GameObject combatManagerObject = new GameObject("CombatManager");
                CombatManager = combatManagerObject.AddComponent<CombatManager>();
                combatManagerObject.transform.SetParent(transform);
                Debug.Log("Created new CombatManager");
            }
            else
            {
                Debug.Log("Found existing CombatManager");
            }
            
            // 초기 게임 상태 설정
            if (_currentState.ToString() == "0")
            {
                CurrentState = GameState.MainMenu;
            }
            
            Debug.Log("All managers initialized successfully");
        }
        
        // 폰트 매니저 초기화
        private void InitializeFontManager()
        {
            FontManager = FindObjectOfType<FontManager>();
            if (FontManager == null)
            {
                GameObject fontManagerObject = new GameObject("FontManager");
                FontManager = fontManagerObject.AddComponent<FontManager>();
                fontManagerObject.transform.SetParent(transform);
                Debug.Log("Created new FontManager");
            }
            else
            {
                Debug.Log("Found existing FontManager");
            }
        }
        #endregion
        
        #region UI Panel Management
        /// <summary>
        /// UI 패널을 전환하는 메서드
        /// </summary>
        public void SwitchPanel(string panelName)
        {
            if (UIManager != null)
            {
                UIManager.OnPanelSwitched(panelName);
            }
            else
            {
                Debug.LogError($"GameManager: Cannot switch to panel {panelName} - UIManager is null");
            }
        }
        #endregion
        
        #region Game Flow Methods
        // 게임 시작
        public void StartNewGame()
        {
            Debug.Log("GameManager: Starting new game");
            
            // 캐릭터 선택 화면으로 전환
            ChangeState(GameState.CharacterSelection);
            
            // 메인메뉴 씬이라면 캐릭터 선택 패널 활성화, 그렇지 않으면 씬 전환
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "MainMenu")
            {
                Debug.Log("GameManager: Showing character selection panel in main menu");
                SwitchPanel("CharacterSelectionPanel");
            }
            else
            {
                Debug.Log("GameManager: Current scene is not MainMenu. Transitioning to GameScene");
                SceneManager.LoadScene("MainMenu");
            }
        }
        
        // 캐릭터 선택 후 던전 진입
        public void EnterDungeon()
        {
            Debug.Log("GameManager: Entering dungeon - transitioning to Dungeon state");
            
            // 현재 씬이 GameScene인지 확인
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                Debug.Log("GameManager: Loading GameScene");
                // 씬 전환 - OnSceneLoaded 콜백이 던전 생성 처리
                SceneManager.LoadScene("GameScene");
                return;
            }
            
            // 이미 GameScene인 경우 던전 생성 직접 호출
            ChangeState(GameState.Dungeon);
            
            Debug.Log("GameManager: DungeonManager reference: " + (DungeonManager != null ? "Valid" : "NULL"));

            try
            {
                // UIManager 참조 확인 및 초기화 확인
                if (UIManager == null)
                {
                    Debug.LogError("GameManager: UIManager is null! Reinitializing...");
                    InitializeManagers();
                }
                
                // UI가 준비되도록 UIManager에게 우선 패널 설정 요청
                if (UIManager != null)
                {
                    // DungeonPanel 준비 확인
                    UIManager.GetType().GetMethod("EnsureDungeonPanelExists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(UIManager, null);
                }
                
                // DungeonManager 준비 확인
                if (DungeonManager == null)
                {
                    Debug.LogError("GameManager: DungeonManager is still null! Creating new instance...");
                    GameObject dmObj = new GameObject("DungeonManager");
                    DungeonManager = dmObj.AddComponent<DungeonManager>();
                    dmObj.transform.SetParent(transform);
                }
                
                // UI와 DungeonManager가 모두 준비되었으면 던전 생성
                if (DungeonManager != null)
                {
                    Debug.Log("GameManager: Calling DungeonManager.GenerateNewDungeon()");
                    // 코루틴으로 약간의 딜레이 후 던전 생성 시도 (UI가 준비될 시간 확보)
                    StartCoroutine(GenerateDungeonWithDelay());
                }
                else
                {
                    Debug.LogError("GameManager: Failed to initialize DungeonManager!");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error in EnterDungeon: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 약간의 딜레이 후 던전 생성 시도 (UI 초기화 시간 확보)
        /// </summary>
        private IEnumerator GenerateDungeonWithDelay()
        {
            // 1프레임 대기하여 UI 초기화 시간 확보
            yield return null;
            
            if (DungeonManager != null)
            {
                try
                {
                    DungeonManager.GenerateNewDungeon();
                    Debug.Log("GameManager: Successfully generated dungeon after delay");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"GameManager: Error generating dungeon: {ex.Message}\n{ex.StackTrace}");
                }
            }
            else
            {
                Debug.LogError("GameManager: DungeonManager is null in delayed dungeon generation!");
            }
        }
        
        // 전투 시작
        public void StartCombat()
        {
            Debug.Log("GameManager: Starting combat - transitioning to Combat state");
            ChangeState(GameState.Combat);
            
            // 패널 전환 처리
            SwitchPanel("CombatPanel");
            
            try
            {
                if (CombatManager != null)
                {
                    Debug.Log("GameManager: Calling CombatManager.InitializeCombat()");
                    CombatManager.InitializeCombat();
                }
                else
                {
                    Debug.LogError("GameManager: CombatManager is null! Reinitializing...");
                    InitializeManagers();
                    CombatManager.InitializeCombat();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error in StartCombat: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 이벤트 시작
        public void StartEvent()
        {
            Debug.Log("GameManager: Starting event - transitioning to Event state");
            ChangeState(GameState.Event);
            
            // 패널 전환 처리
            SwitchPanel("EventPanel");
        }
        
        // 상점 시작
        public void StartShop()
        {
            Debug.Log("GameManager: Starting shop - transitioning to Shop state");
            ChangeState(GameState.Shop);
            
            // 패널 전환 처리
            SwitchPanel("ShopPanel");
        }
        
        // 휴식 시작
        public void StartRest()
        {
            Debug.Log("GameManager: Starting rest - transitioning to Rest state");
            ChangeState(GameState.Rest);
            
            // 패널 전환 처리
            SwitchPanel("RestPanel");
        }
        
        // 전투 종료
        public void EndCombat(bool victory)
        {
            Debug.Log($"Combat ended - Victory: {victory}");
            
            if (victory)
            {
                ChangeState(GameState.Dungeon);
                // 보상 처리 등...
                Debug.Log("Returning to dungeon after victory");
            }
            else
            {
                ChangeState(GameState.GameOver);
                Debug.Log("Game over after defeat");
            }
        }
        
        // 이벤트 종료
        public void EndEvent()
        {
            Debug.Log("Event ended - returning to dungeon");
            ChangeState(GameState.Dungeon);
        }
        
        // 상점 종료
        public void EndShop()
        {
            Debug.Log("Shop ended - returning to dungeon");
            ChangeState(GameState.Dungeon);
        }
        
        // 휴식 종료
        public void EndRest()
        {
            Debug.Log("Rest ended - returning to dungeon");
            ChangeState(GameState.Dungeon);
        }
        
        // 게임 오버
        public void GameOver()
        {
            Debug.Log("Game over - transitioning to GameOver state");
            ChangeState(GameState.GameOver);
            
            try
            {
                if (UIManager != null)
                {
                    Debug.Log("Calling UIManager.ShowGameOverScreen()");
                    UIManager.ShowGameOverScreen();
                }
                else
                {
                    Debug.LogError("UIManager is null! Reinitializing...");
                    InitializeManagers();
                    UIManager.ShowGameOverScreen();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in GameOver: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 게임 승리
        public void Victory()
        {
            Debug.Log("Victory - transitioning to Victory state");
            ChangeState(GameState.Victory);
            
            try
            {
                if (UIManager != null)
                {
                    Debug.Log("Calling UIManager.ShowVictoryScreen()");
                    UIManager.ShowVictoryScreen();
                }
                else
                {
                    Debug.LogError("UIManager is null! Reinitializing...");
                    InitializeManagers();
                    UIManager.ShowVictoryScreen();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error in Victory: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 메인 메뉴로 돌아가기
        public void ReturnToMainMenu()
        {
            Debug.Log("Returning to main menu - transitioning to MainMenu state");
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }
        #endregion
    }
}