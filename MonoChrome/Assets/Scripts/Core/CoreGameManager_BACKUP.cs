using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonoChrome.Core
{
    /// <summary>
    /// 핵심 게임 매니저 - 단순하고 안정적인 구조
    /// 포트폴리오용: 명확한 의존성 관리와 이해하기 쉬운 구조
    /// </summary>
    public class CoreGameManager : MonoBehaviour
    {
        #region Singleton Pattern
        private static CoreGameManager _instance;
        private static readonly object _lock = new object();
        
        public static CoreGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            // 기존 인스턴스 찾기
                            _instance = FindObjectOfType<CoreGameManager>();
                            
                            if (_instance == null)
                            {
                                // 새 인스턴스 생성
                                GameObject go = new GameObject("CoreGameManager");
                                _instance = go.AddComponent<CoreGameManager>();
                                DontDestroyOnLoad(go);
                            }
                        }
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            // 중복 인스턴스 방지
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("CoreGameManager: Duplicate instance destroyed");
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("CoreGameManager: Singleton initialized");
        }
        #endregion
        
        #region Game State
        public enum GameState
        {
            MainMenu,
            CharacterSelection,
            Dungeon,
            Combat,
            Event,
            Shop,
            Rest,
            Victory,
            GameOver
        }
        
        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;
        
        public event Action<GameState> OnStateChanged;
        
        public void ChangeState(GameState newState)
        {
            if (_currentState != newState)
            {
                GameState oldState = _currentState;
                _currentState = newState;
                
                Debug.Log($"CoreGameManager: State changed from {oldState} to {newState}");
                OnStateChanged?.Invoke(newState);
            }
        }
        #endregion
        
        #region Character Management
        [Header("Current Game Data")]
        [SerializeField] private string _selectedCharacterName;
        [SerializeField] private int _currentStage = 0;
        [SerializeField] private int _currentRoom = 0;
        
        public string SelectedCharacterName 
        { 
            get => _selectedCharacterName; 
            set => _selectedCharacterName = value; 
        }
        
        public int CurrentStage 
        { 
            get => _currentStage; 
            set => _currentStage = value; 
        }
        
        public int CurrentRoom 
        { 
            get => _currentRoom; 
            set => _currentRoom = value; 
        }
        
        /// <summary>
        /// 캐릭터 선택 처리
        /// </summary>
        public void SelectCharacter(string characterName)
        {
            _selectedCharacterName = characterName;
            Debug.Log($"CoreGameManager: Character selected - {characterName}");
            
            // 캐릭터 선택 후 던전으로 진행
            StartCoroutine(TransitionToDungeon());
        }
        
        /// <summary>
        /// 던전으로 전환하는 코루틴
        /// </summary>
        private IEnumerator TransitionToDungeon()
        {
            ChangeState(GameState.Dungeon);
            
            // 프레임 대기 - UI 업데이트 시간 확보
            yield return null;
            yield return null;
            
            // DungeonManager 찾기 및 던전 생성
            var dungeonManager = FindObjectOfType<MonoChrome.Dungeon.DungeonManager>();
            if (dungeonManager != null)
            {
                Debug.Log("CoreGameManager: Starting dungeon generation");
                dungeonManager.GenerateNewDungeon(_currentStage);
            }
            else
            {
                Debug.LogError("CoreGameManager: DungeonManager not found!");
            }
        }
        #endregion
        
        #region Scene Management
        private void Start()
        {
            // 현재 씬에 따른 초기 상태 설정
            string currentScene = SceneManager.GetActiveScene().name;
            
            switch (currentScene)
            {
                case "MainMenu":
                    ChangeState(GameState.MainMenu);
                    break;
                    
                case "GameScene":
                    // GameScene에서 시작하면 캐릭터 선택으로
                    ChangeState(GameState.CharacterSelection);
                    break;
            }
            
            Debug.Log($"CoreGameManager: Initialized in scene {currentScene}");
        }
        
        /// <summary>
        /// 새 게임 시작
        /// </summary>
        public void StartNewGame()
        {
            Debug.Log("CoreGameManager: Starting new game");
            
            // 게임 데이터 초기화
            _selectedCharacterName = "";
            _currentStage = 0;
            _currentRoom = 0;
            
            // GameScene으로 이동
            if (SceneManager.GetActiveScene().name != "GameScene")
            {
                SceneManager.LoadScene("GameScene");
            }
            else
            {
                ChangeState(GameState.CharacterSelection);
            }
        }
        
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("CoreGameManager: Returning to main menu");
            
            ChangeState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }
        #endregion
        
        #region Game Flow Control
        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat()
        {
            Debug.Log("CoreGameManager: Starting combat");
            ChangeState(GameState.Combat);
            
            // CombatManager 초기화
            var combatManager = FindObjectOfType<MonoChrome.Combat.CombatManager>();
            if (combatManager != null)
            {
                // 전투 초기화 로직
                StartCoroutine(InitializeCombat(combatManager));
            }
            else
            {
                Debug.LogError("CoreGameManager: CombatManager not found!");
            }
        }
        
        /// <summary>
        /// 전투 초기화 코루틴
        /// </summary>
        private IEnumerator InitializeCombat(MonoChrome.Combat.CombatManager combatManager)
        {
            yield return null; // 프레임 대기
            
            try
            {
                combatManager.InitializeCombat();
                Debug.Log("CoreGameManager: Combat initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"CoreGameManager: Combat initialization failed - {ex.Message}");
            }
        }
        
        /// <summary>
        /// 전투 종료
        /// </summary>
        public void EndCombat(bool victory)
        {
            Debug.Log($"CoreGameManager: Combat ended - Victory: {victory}");
            
            if (victory)
            {
                // 승리 시 던전으로 복귀
                ChangeState(GameState.Dungeon);
            }
            else
            {
                // 패배 시 게임 오버
                ChangeState(GameState.GameOver);
            }
        }
        
        /// <summary>
        /// 이벤트 시작
        /// </summary>
        public void StartEvent()
        {
            Debug.Log("CoreGameManager: Starting event");
            ChangeState(GameState.Event);
        }
        
        /// <summary>
        /// 상점 열기
        /// </summary>
        public void OpenShop()
        {
            Debug.Log("CoreGameManager: Opening shop");
            ChangeState(GameState.Shop);
        }
        
        /// <summary>
        /// 휴식 시작
        /// </summary>
        public void StartRest()
        {
            Debug.Log("CoreGameManager: Starting rest");
            ChangeState(GameState.Rest);
        }
        
        /// <summary>
        /// 활동 완료 후 던전으로 복귀
        /// </summary>
        public void ReturnToDungeon()
        {
            Debug.Log("CoreGameManager: Returning to dungeon");
            ChangeState(GameState.Dungeon);
        }
        #endregion
        
        #region Debug Utilities
        [Header("Debug")]
        [SerializeField] private bool _showDebugInfo = true;
        
        private void OnGUI()
        {
            if (!_showDebugInfo) return;
            
            // 디버그 정보 표시
            GUI.Box(new Rect(10, 10, 200, 120), "Game Debug Info");
            GUI.Label(new Rect(15, 30, 190, 20), $"State: {_currentState}");
            GUI.Label(new Rect(15, 50, 190, 20), $"Character: {_selectedCharacterName}");
            GUI.Label(new Rect(15, 70, 190, 20), $"Stage: {_currentStage}");
            GUI.Label(new Rect(15, 90, 190, 20), $"Room: {_currentRoom}");
            GUI.Label(new Rect(15, 110, 190, 20), $"Scene: {SceneManager.GetActiveScene().name}");
        }
        #endregion
    }
}