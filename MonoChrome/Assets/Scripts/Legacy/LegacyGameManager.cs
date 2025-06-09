using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// Deprecated legacy GameManager bridge.
    /// 포워딩 로직을 통해 <see cref="MasterGameManager"/>와 <see cref="GameStateMachine"/>에 연결합니다.
    /// </summary>
    [System.Obsolete("GameManager is deprecated. Use MasterGameManager.Instance instead.")]
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        private MasterGameManager _masterGameManager;
        private GameStateMachine _gameStateMachine;
        
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager_Legacy_Bridge");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        // 레거시 상태 열거형 (호환성용)
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
        
        // 호환성 프로퍼티들
        public GameState CurrentState
        {
            get
            {
                if (_gameStateMachine == null)
                    _gameStateMachine = GameStateMachine.Instance;
                
                if (_gameStateMachine != null)
                {
                    return ConvertToLegacyState(_gameStateMachine.CurrentState);
                }
                return GameState.MainMenu;
            }
        }
        
        // 레거시 매니저 참조들 (호환성용)
        public CoreUIManager UIManager => FindObjectOfType<CoreUIManager>();
        public Systems.Combat.CombatSystem CombatManager => FindObjectOfType<Systems.Combat.CombatSystem>();
        public DungeonController DungeonManager => FindObjectOfType<DungeonController>();
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 실제 시스템들 참조
            _masterGameManager = FindObjectOfType<MasterGameManager>();
            _gameStateMachine = GameStateMachine.Instance;
            
            Debug.Log("Legacy GameManager Bridge initialized");
        }
        
        // 레거시 상태 변경 메서드
        public void ChangeState(GameState newState)
        {
            if (_gameStateMachine == null)
                _gameStateMachine = GameStateMachine.Instance;
                
            if (_gameStateMachine != null)
            {
                var modernState = ConvertToModernState(newState);
                _gameStateMachine.TryChangeState(modernState);
                Debug.Log($"Legacy state change: {newState} -> {modernState}");
            }
        }
        
        // 상태 변환 헬퍼 메서드들
        private GameState ConvertToLegacyState(GameStateMachine.GameState modernState)
        {
            switch (modernState)
            {
                case GameStateMachine.GameState.MainMenu:
                    return GameState.MainMenu;
                case GameStateMachine.GameState.CharacterSelection:
                    return GameState.CharacterSelection;
                case GameStateMachine.GameState.Dungeon:
                    return GameState.Dungeon;
                case GameStateMachine.GameState.Combat:
                    return GameState.Combat;
                case GameStateMachine.GameState.GameOver:
                    return GameState.GameOver;
                case GameStateMachine.GameState.Victory:
                    return GameState.Victory;
                default:
                    return GameState.MainMenu;
            }
        }
        
        private GameStateMachine.GameState ConvertToModernState(GameState legacyState)
        {
            switch (legacyState)
            {
                case GameState.MainMenu:
                    return GameStateMachine.GameState.MainMenu;
                case GameState.CharacterSelection:
                    return GameStateMachine.GameState.CharacterSelection;
                case GameState.Dungeon:
                    return GameStateMachine.GameState.Dungeon;
                case GameState.Combat:
                    return GameStateMachine.GameState.Combat;
                case GameState.GameOver:
                    return GameStateMachine.GameState.GameOver;
                case GameState.Victory:
                    return GameStateMachine.GameState.Victory;
                default:
                    return GameStateMachine.GameState.MainMenu;
            }
        }

        /// <summary>
        /// Legacy entry point for starting the game by entering the first dungeon.
        /// </summary>
        public void EnterDungeon()
        {
            _masterGameManager?.EnterDungeon();
        }

        public bool IsInitialized => _masterGameManager?.IsInitialized ?? false;
    }
}