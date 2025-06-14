using System;
using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// 게임 상태 머신 - 순수하게 상태 전환과 이벤트 발행만 담당
    /// UI나 구체적인 구현사항에 대해 알지 못함 (느슨한 결합)
    /// </summary>
    public class GameStateMachine : MonoBehaviour
    {
        #region Singleton Pattern
        private static GameStateMachine _instance;
        public static GameStateMachine Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameStateMachine>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[GameStateMachine]");
                        _instance = go.AddComponent<GameStateMachine>();
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
        #endregion

        #region State Definition
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

        [Header("Current State")]
        [SerializeField] private GameState _currentState = GameState.MainMenu;

        // 이벤트만 발행 - 구체적인 구현은 각 시스템에서 처리
        public static event Action<GameState, GameState> OnStateChanged;
        public static event Action<GameState> OnStateEntered;
        public static event Action<GameState> OnStateExited;

        public GameState CurrentState => _currentState;
        #endregion

        #region State Management
        /// <summary>
        /// 상태 전환 - 순수하게 상태만 관리
        /// </summary>
        public bool TryChangeState(GameState newState)
        {
            if (!IsValidStateTransition(_currentState, newState))
            {
                Debug.LogWarning($"[GameStateMachine] Invalid state transition: {_currentState} -> {newState}");
                return false;
            }

            GameState previousState = _currentState;
            
            // 이전 상태 종료 이벤트
            OnStateExited?.Invoke(previousState);
            
            // 상태 변경
            _currentState = newState;
            
            // 상태 변경 이벤트
            OnStateChanged?.Invoke(previousState, newState);
            
            // 새 상태 진입 이벤트
            OnStateEntered?.Invoke(newState);
            
            Debug.Log($"[GameStateMachine] State changed: {previousState} -> {newState}");
            return true;
        }

        /// <summary>
        /// 상태 전환 유효성 검사
        /// </summary>
        private bool IsValidStateTransition(GameState from, GameState to)
        {
            // 비즈니스 로직에 따른 유효성 검사
            return to switch
            {
                GameState.CharacterSelection => from == GameState.MainMenu,
                GameState.Dungeon => from is GameState.CharacterSelection or GameState.Combat 
                                           or GameState.Event or GameState.Shop or GameState.Rest,
                GameState.Combat => from == GameState.Dungeon,
                GameState.Event => from == GameState.Dungeon,
                GameState.Shop => from == GameState.Dungeon,
                GameState.Rest => from == GameState.Dungeon,
                GameState.GameOver => from is GameState.Combat or GameState.Event,
                GameState.Victory => from is GameState.Combat or GameState.Dungeon,
                GameState.MainMenu => true, // 언제든 메인메뉴로 복귀 가능
                GameState.Paused => from != GameState.MainMenu, // 메인메뉴 제외 모든 상태에서 일시정지 가능
                _ => false
            };
        }
        #endregion

        #region Public API - 게임 플로우 인터페이스
        public void StartNewGame() => TryChangeState(GameState.CharacterSelection);
        public void EnterDungeon() => TryChangeState(GameState.Dungeon);
        public void StartCombat() => TryChangeState(GameState.Combat);
        public void EnterEvent() => TryChangeState(GameState.Event);
        public void EnterShop() => TryChangeState(GameState.Shop);
        public void EnterRest() => TryChangeState(GameState.Rest);
        public void GameOver() => TryChangeState(GameState.GameOver);
        public void Victory() => TryChangeState(GameState.Victory);
        public void ReturnToMainMenu() => TryChangeState(GameState.MainMenu);
        public void Pause() => TryChangeState(GameState.Paused);

        // 이벤트 처리 완료 후 던전으로 복귀
        public void CompleteRoomActivity() => TryChangeState(GameState.Dungeon);
        
        // 던전 완료 처리 - 승리 또는 다음 스테이지로 이동
        public void CompleteDungeon() => TryChangeState(GameState.Victory);
        #endregion
    }
}