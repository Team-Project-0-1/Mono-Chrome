using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome.Compatibility
{
    /// <summary>
    /// 기존 시스템과 새로운 이벤트 기반 시스템 사이의 브릿지
    /// 레거시 코드와의 호환성을 보장하면서 점진적 마이그레이션 지원
    /// </summary>
    public class LegacySystemBridge : MonoBehaviour
    {
        [Header("브릿지 설정")]
        [SerializeField] private bool _enableLegacySupport = true;
        [SerializeField] private bool _showBridgeLog = true;

        // 기존 시스템 참조 (호환성용)
        private GameManager _legacyGameManager;

        // 새 시스템 참조
        private GameStateMachine _newStateMachine;
        private DungeonController _newDungeonController;

        private void Awake()
        {
            if (!_enableLegacySupport) return;

            InitializeBridge();
        }

        private void OnEnable()
        {
            if (!_enableLegacySupport) return;

            SubscribeToEvents();
        }

        private void OnDisable()
        {
            if (!_enableLegacySupport) return;

            UnsubscribeFromEvents();
        }

        private void InitializeBridge()
        {
            LogBridge("브릿지 시스템 초기화 중...");

            // 기존 시스템 찾기
            _legacyGameManager = FindObjectOfType<GameManager>();
            _legacyDungeonManager = FindObjectOfType<DungeonManager>();

            // 새 시스템 참조
            _newStateMachine = GameStateMachine.Instance;
            _newDungeonController = FindObjectOfType<DungeonController>();

            LogBridge($"기존 시스템: GameManager={_legacyGameManager != null}, DungeonManager={_legacyDungeonManager != null}");
            LogBridge($"새 시스템: StateMachine={_newStateMachine != null}, DungeonController={_newDungeonController != null}");
        }

        private void SubscribeToEvents()
        {
            // 새 시스템 이벤트 구독
            DungeonEvents.OnDungeonGenerationRequested += OnNewSystemDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested += OnNewSystemNodeMoveRequested;
            
            // 기존 시스템 이벤트 구독 (있다면)
            if (_legacyGameManager != null)
            {
                _legacyGameManager.OnGameStateChanged += OnLegacyGameStateChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            // 새 시스템 이벤트 구독 해제
            DungeonEvents.OnDungeonGenerationRequested -= OnNewSystemDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested -= OnNewSystemNodeMoveRequested;
            
            // 기존 시스템 이벤트 구독 해제
            if (_legacyGameManager != null)
            {
                _legacyGameManager.OnGameStateChanged -= OnLegacyGameStateChanged;
            }
        }

        #region 새 시스템 -> 기존 시스템 브릿지

        /// <summary>
        /// 새 시스템의 던전 생성 요청을 기존 시스템으로 전달
        /// </summary>
        private void OnNewSystemDungeonGenerationRequested(int stageIndex)
        {
            LogBridge($"새 시스템에서 던전 생성 요청됨 (스테이지 {stageIndex})");

            if (_legacyDungeonManager != null)
            {
                LogBridge("기존 DungeonManager로 던전 생성 요청 전달");
                _legacyDungeonManager.GenerateNewDungeon(stageIndex);
            }
            else if (_newDungeonController != null)
            {
                LogBridge("새 DungeonController가 처리할 예정");
                // 새 DungeonController가 이미 처리했을 것임
            }
            else
            {
                LogBridge("⚠️ 던전 생성을 처리할 시스템이 없음");
            }
        }

        /// <summary>
        /// 새 시스템의 노드 이동 요청을 기존 시스템으로 전달
        /// </summary>
        private void OnNewSystemNodeMoveRequested(int nodeIndex)
        {
            LogBridge($"새 시스템에서 노드 이동 요청됨 (노드 {nodeIndex})");

            if (_legacyDungeonManager != null)
            {
                LogBridge("기존 DungeonManager로 노드 이동 요청 전달");
                _legacyDungeonManager.MoveToNode(nodeIndex);
            }
            else if (_newDungeonController != null)
            {
                LogBridge("새 DungeonController가 처리할 예정");
                // 새 DungeonController가 이미 처리했을 것임
            }
            else
            {
                LogBridge("⚠️ 노드 이동을 처리할 시스템이 없음");
            }
        }

        #endregion

        #region 기존 시스템 -> 새 시스템 브릿지

        /// <summary>
        /// 기존 시스템의 상태 변경을 새 시스템으로 동기화
        /// </summary>
        private void OnLegacyGameStateChanged(GameManager.GameState legacyState)
        {
            LogBridge($"기존 시스템 상태 변경됨: {legacyState}");

            if (_newStateMachine != null)
            {
                // 기존 상태를 새 상태로 매핑
                var newState = MapLegacyStateToNewState(legacyState);
                
                if (newState.HasValue)
                {
                    LogBridge($"새 시스템으로 상태 동기화: {newState.Value}");
                    _newStateMachine.TryChangeState(newState.Value);
                }
            }
        }

        /// <summary>
        /// 기존 상태를 새 상태로 매핑
        /// </summary>
        private GameStateMachine.GameState? MapLegacyStateToNewState(GameManager.GameState legacyState)
        {
            return legacyState switch
            {
                GameManager.GameState.MainMenu => GameStateMachine.GameState.MainMenu,
                GameManager.GameState.CharacterSelection => GameStateMachine.GameState.CharacterSelection,
                GameManager.GameState.Dungeon => GameStateMachine.GameState.Dungeon,
                GameManager.GameState.Combat => GameStateMachine.GameState.Combat,
                GameManager.GameState.Event => GameStateMachine.GameState.Event,
                GameManager.GameState.Shop => GameStateMachine.GameState.Shop,
                GameManager.GameState.Rest => GameStateMachine.GameState.Rest,
                GameManager.GameState.GameOver => GameStateMachine.GameState.GameOver,
                GameManager.GameState.Victory => GameStateMachine.GameState.Victory,
                GameManager.GameState.Paused => GameStateMachine.GameState.Paused,
                _ => null
            };
        }

        #endregion

        #region 공용 브릿지 메서드

        /// <summary>
        /// 던전 진입 - 두 시스템 모두 지원
        /// </summary>
        public void EnterDungeon()
        {
            LogBridge("던전 진입 요청");

            // 새 시스템 우선 사용
            if (_newStateMachine != null)
            {
                _newStateMachine.EnterDungeon();
                DungeonEvents.RequestDungeonGeneration(0);
            }
            // 기존 시스템 폴백
            else if (_legacyGameManager != null)
            {
                _legacyGameManager.EnterDungeon();
            }
            else
            {
                LogBridge("⚠️ 던전 진입을 처리할 시스템이 없음");
            }
        }

        /// <summary>
        /// 새 게임 시작 - 두 시스템 모두 지원
        /// </summary>
        public void StartNewGame()
        {
            LogBridge("새 게임 시작 요청");

            // 새 시스템 우선 사용
            if (_newStateMachine != null)
            {
                _newStateMachine.StartNewGame();
            }
            // 기존 시스템 폴백
            else if (_legacyGameManager != null)
            {
                _legacyGameManager.StartNewGame();
            }
            else
            {
                LogBridge("⚠️ 새 게임 시작을 처리할 시스템이 없음");
            }
        }

        /// <summary>
        /// 전투 시작 - 두 시스템 모두 지원
        /// </summary>
        public void StartCombat()
        {
            LogBridge("전투 시작 요청");

            // 새 시스템 우선 사용
            if (_newStateMachine != null)
            {
                _newStateMachine.StartCombat();
            }
            // 기존 시스템 폴백
            else if (_legacyGameManager != null)
            {
                _legacyGameManager.StartCombat();
            }
            else
            {
                LogBridge("⚠️ 전투 시작을 처리할 시스템이 없음");
            }
        }

        #endregion

        #region 유틸리티

        private void LogBridge(string message)
        {
            if (_showBridgeLog)
            {
                Debug.Log($"[LegacySystemBridge] {message}");
            }
        }

        /// <summary>
        /// 브릿지 상태 확인
        /// </summary>
        [ContextMenu("브릿지 상태 확인")]
        public void CheckBridgeStatus()
        {
            Debug.Log("=== 레거시 시스템 브릿지 상태 ===");
            Debug.Log($"기존 GameManager: {(_legacyGameManager != null ? "존재" : "없음")}");
            Debug.Log($"기존 DungeonManager: {(_legacyDungeonManager != null ? "존재" : "없음")}");
            Debug.Log($"새 GameStateMachine: {(_newStateMachine != null ? "존재" : "없음")}");
            Debug.Log($"새 DungeonController: {(_newDungeonController != null ? "존재" : "없음")}");
            Debug.Log($"브릿지 활성화: {_enableLegacySupport}");
        }

        /// <summary>
        /// 브릿지 활성화/비활성화 토글
        /// </summary>
        public void SetBridgeEnabled(bool enabled)
        {
            _enableLegacySupport = enabled;
            
            if (enabled)
            {
                InitializeBridge();
                SubscribeToEvents();
            }
            else
            {
                UnsubscribeFromEvents();
            }
            
            LogBridge($"브릿지 {(enabled ? "활성화" : "비활성화")}됨");
        }

        #endregion
    }
}
