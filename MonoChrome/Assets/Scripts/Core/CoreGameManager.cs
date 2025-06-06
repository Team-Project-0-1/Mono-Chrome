using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// Lightweight bridge for legacy references.
    /// Forwards calls to <see cref="MasterGameManager"/> and <see cref="GameStateMachine"/>.
    /// </summary>
    public class CoreGameManager : MonoBehaviour
    {
        private static CoreGameManager _instance;
        public static CoreGameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CoreGameManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("CoreGameManager");
                        _instance = go.AddComponent<CoreGameManager>();
                        DontDestroyOnLoad(go);
                    }
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
        }

        // Legacy GameState for compatibility
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

        public void ChangeState(GameState state)
        {
            var target = state switch
            {
                GameState.MainMenu => GameStateMachine.GameState.MainMenu,
                GameState.CharacterSelection => GameStateMachine.GameState.CharacterSelection,
                GameState.Dungeon => GameStateMachine.GameState.Dungeon,
                GameState.Combat => GameStateMachine.GameState.Combat,
                GameState.Event => GameStateMachine.GameState.Event,
                GameState.Shop => GameStateMachine.GameState.Shop,
                GameState.Rest => GameStateMachine.GameState.Rest,
                GameState.GameOver => GameStateMachine.GameState.GameOver,
                GameState.Victory => GameStateMachine.GameState.Victory,
                GameState.Paused => GameStateMachine.GameState.Paused,
                _ => GameStateMachine.GameState.MainMenu
            };
            GameStateMachine.Instance?.TryChangeState(target);
        }

        public void StartNewGame()
        {
            MasterGameManager.Instance?.StartNewGame();
        }

        public void EnterDungeon()
        {
            MasterGameManager.Instance?.EnterDungeon();
        }

        public void StartCombat(string enemyType = "약탈자1", CharacterType characterType = CharacterType.Normal)
        {
            MasterGameManager.Instance?.StartCombat(enemyType, characterType);
        }
    }
}
