using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoChrome.Combat;
using MonoChrome.Dungeon;
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
        /// </summary>
        private void RefreshManagerReferences()
        {
            Debug.Log("GameManager: Refreshing manager references...");
            
            // UIManager 찾기 (씬에 있는 것 우선)
            UIManager[] uiManagers = FindObjectsOfType<UIManager>();
            _uiManager = uiManagers.FirstOrDefault(ui => ui.gameObject.scene.isLoaded);
            
            // DungeonManager 찾기 (활성화된 것 우선)
            DungeonManager[] dungeonManagers = FindObjectsOfType<DungeonManager>();
            _dungeonManager = dungeonManagers.FirstOrDefault(dm => dm.gameObject.activeInHierarchy);
            
            // CombatManager 찾기 (활성화된 것 우선)
            CombatManager[] combatManagers = FindObjectsOfType<CombatManager>();
            _combatManager = combatManagers.FirstOrDefault(cm => cm.gameObject.activeInHierarchy);
            
            Debug.Log($"GameManager: References refreshed - UI:{_uiManager != null}, Dungeon:{_dungeonManager != null}, Combat:{_combatManager != null}");
        }
        
        /// <summary>
        /// 씬의 매니저들을 활성화하는 메서드
        /// </summary>
        private void ActivateSceneManagers()
        {
            // 비활성화된 매니저들 찾아서 활성화
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
                    Debug.Log($"GameManager: Activated {managerName}");
                }
            }
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
            Debug.Log("GameManager: Entering dungeon");
            ChangeState(GameState.Dungeon);
            
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
                return;
            }
            
            // 던전 생성
            StartCoroutine(GenerateDungeonSafely());
        }
        
        private IEnumerator GenerateDungeonSafely()
        {
            // UI가 준비될 때까지 대기
            while (UIManager == null)
            {
                yield return null;
            }
            
            // 던전 매니저가 준비될 때까지 대기
            while (DungeonManager == null)
            {
                yield return null;
            }
            
            try
            {
                DungeonManager.GenerateNewDungeon();
                Debug.Log("GameManager: Successfully generated dungeon");
            }
            catch (Exception ex)
            {
                Debug.LogError($"GameManager: Error generating dungeon: {ex.Message}");
            }
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