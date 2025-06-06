using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// 레거시 GameManager 호환성 클래스
    /// 기존 코드와의 호환성을 위해 GameManager 패턴을 MasterGameManager로 포워딩
    /// </summary>
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
            GameOver,
            Victory
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
        public Systems.Dungeon.DungeonController DungeonManager => FindObjectOfType<Systems.Dungeon.DungeonController>();
        
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
        
        public bool IsInitialized => _masterGameManager?.IsInitialized ?? false;
    }
}