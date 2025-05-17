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
            }
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
                    
                    // 상태 변경 시 UI 자동 업데이트
                    UpdateUIBasedOnGameState();
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
            CurrentState = GameState.MainMenu;
            
            Debug.Log("All managers initialized successfully");
        }
        #endregion
        
        #region Game Flow Methods
        // 게임 시작
        public void StartNewGame()
        {
            Debug.Log("GameManager: Starting new game");
            
            // 캐릭터 선택 화면으로 전환
            ChangeState(GameState.CharacterSelection);
            
            // 메인메뉴 씬이라면 캠릭터 선택 패널 활성화, 그렇지 않으면 씬 전환
            string currentScene = SceneManager.GetActiveScene().name;
            if (currentScene == "MainMenu")
            {
                Debug.Log("GameManager: Showing character selection panel in main menu");
                SwitchToPanel("CharacterSelectionPanel");
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
                // 씬 전환 - OnSceneLoaded 채가 던전 생성 처리
                SceneManager.LoadScene("GameScene");
                return;
            }
            
            // 이미 GameScene인 경우 던전 생성 직접 호출
            ChangeState(GameState.Dungeon);
            
            Debug.Log("GameManager: DungeonManager reference: " + (DungeonManager != null ? "Valid" : "NULL"));

            
            try
            {
                if (DungeonManager != null)
                {
                    Debug.Log("GameManager: Calling DungeonManager.GenerateNewDungeon()");
                    DungeonManager.GenerateNewDungeon();
                }
                else
                {
                    Debug.LogError("GameManager: DungeonManager is null! Reinitializing...");
                    InitializeManagers();
                    
                    // 재초기화 후 다시 확인
                    Debug.Log("GameManager: DungeonManager after reinit: " + (DungeonManager != null ? "Valid" : "NULL"));
                    if (DungeonManager != null)
                    {
                        DungeonManager.GenerateNewDungeon();
                    }
                    else
                    {
                        Debug.LogError("GameManager: Still cannot find DungeonManager after reinitializing!");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error in EnterDungeon: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 전투 시작
        public void StartCombat()
        {
            Debug.Log("GameManager: Starting combat - transitioning to Combat state");
            ChangeState(GameState.Combat);
            
            // 패널 전환 처리
            SwitchToPanel("CombatPanel");
            
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
        
        #region UI Panel Management
        /// <summary>
        /// UI 패널을 전환하는 메서드
        /// </summary>
        /// <param name="panelName">활성화할 패널의 이름</param>
        public void SwitchToPanel(string panelName)
        {
            try
            {
                Debug.Log($"GameManager: Attempting to switch to panel: {panelName}");
        
                // 캔버스 찾기
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("GameManager: No Canvas found in the scene!");
                    return;
                }
        
                // 모든 패널 비활성화 (Transform.Find를 사용하여 직접 자식 찾기)
                foreach (Transform child in mainCanvas.transform)
                {
                    // 패널인지 확인 (이름이 "Panel"로 끝나는지)
                    if (child.name.EndsWith("Panel"))
                    {
                        Debug.Log($"Found panel: {child.name}, setting inactive");
                        child.gameObject.SetActive(false);
                    }
                }
        
                // 지정된 패널 활성화
                Transform targetPanel = mainCanvas.transform.Find(panelName);
                if (targetPanel != null)
                {
                    targetPanel.gameObject.SetActive(true);
                    Debug.Log($"GameManager: Successfully switched to {panelName}");
            
                    // UI Manager 알림
                    if (UIManager != null)
                    {
                        UIManager.OnPanelSwitched(panelName);
                    }
                }
                else
                {
                    Debug.LogError($"GameManager: Could not find panel {panelName} as child of Canvas");
                    // 모든 자식 로깅
                    foreach (Transform child in mainCanvas.transform)
                    {
                        Debug.Log($"Canvas child: {child.name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error switching to panel {panelName}: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 게임 상태에 따라 적절한 UI 패널로 자동 전환
        /// </summary>
        private void UpdateUIBasedOnGameState()
        {
            UpdateUIBasedOnGameState(CurrentState);
        }

        /// <summary>
        /// 지정된 게임 상태에 따라 UI 패널 전환
        /// </summary>
        /// <param name="state">활성화할 패널에 해당하는 게임 상태</param> 
        private void UpdateUIBasedOnGameState(GameState state)
        {
            Debug.Log($"GameManager: Updating UI based on game state: {state}");
            
            switch (state)
            {
                case GameState.MainMenu:
                    SwitchToPanel("MainMenuPanel");
                    break;
                case GameState.CharacterSelection:
                    SwitchToPanel("CharacterSelectionPanel");
                    break;
                case GameState.Dungeon:
                    SwitchToPanel("DungeonPanel");
                    break;
                case GameState.Combat:
                    SwitchToPanel("CombatPanel");
                    break;
                case GameState.Event:
                    SwitchToPanel("EventPanel");
                    break;
                case GameState.GameOver:
                    SwitchToPanel("GameOverPanel");
                    break;
                case GameState.Victory:
                    SwitchToPanel("VictoryPanel");
                    break;
                case GameState.Paused:
                    SwitchToPanel("SettingsPanel");
                    break;
            }
        }
        #endregion
    }
}