using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Dungeon;
using MonoChrome.UI;

namespace MonoChrome.Testing
{
    /// <summary>
    /// 개선된 아키텍처 테스트 스크립트
    /// 새로운 이벤트 시스템과 상태 머신을 테스트
    /// </summary>
    public class ImprovedArchitectureTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool _enableAutoTest = false;
        [SerializeField] private float _testDelay = 2f;

        private void Start()
        {
            if (_enableAutoTest)
            {
                StartCoroutine(RunAutoTest());
            }
        }

        private System.Collections.IEnumerator RunAutoTest()
        {
            Debug.Log("=== 개선된 아키텍처 자동 테스트 시작 ===");

            yield return new WaitForSeconds(1f);

            // 1. 게임 상태 머신 테스트
            Debug.Log("1. 게임 상태 머신 테스트");
            TestGameStateMachine();

            yield return new WaitForSeconds(_testDelay);

            // 2. 이벤트 시스템 테스트
            Debug.Log("2. 이벤트 시스템 테스트");
            TestEventSystem();

            yield return new WaitForSeconds(_testDelay);

            // 3. 던전 생성 이벤트 테스트
            Debug.Log("3. 던전 생성 이벤트 테스트");
            TestDungeonGeneration();

            Debug.Log("=== 아키텍처 테스트 완료 ===");
        }

        /// <summary>
        /// 게임 상태 머신 테스트
        /// </summary>
        private void TestGameStateMachine()
        {
            var stateMachine = GameStateMachine.Instance;
            if (stateMachine != null)
            {
                Debug.Log($"현재 상태: {stateMachine.CurrentState}");
                
                // 상태 전환 테스트
                bool success = stateMachine.TryChangeState(GameStateMachine.GameState.Dungeon);
                Debug.Log($"던전 상태 전환: {(success ? "성공" : "실패")}");
                
                Debug.Log($"변경된 상태: {stateMachine.CurrentState}");
            }
            else
            {
                Debug.LogError("GameStateMachine 인스턴스를 찾을 수 없음");
            }
        }

        /// <summary>
        /// 이벤트 시스템 테스트
        /// </summary>
        private void TestEventSystem()
        {
            // 던전 이벤트 구독 테스트
            DungeonEvents.OnDungeonGenerationRequested += OnTestDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested += OnTestNodeMoveRequested;

            // 이벤트 발행 테스트
            DungeonEvents.RequestDungeonGeneration(0);
            DungeonEvents.RequestNodeMove(1);

            // 구독 해제
            DungeonEvents.OnDungeonGenerationRequested -= OnTestDungeonGenerationRequested;
            DungeonEvents.OnNodeMoveRequested -= OnTestNodeMoveRequested;
        }

        /// <summary>
        /// 던전 생성 테스트
        /// </summary>
        private void TestDungeonGeneration()
        {
            // DungeonController가 있는지 확인
            var dungeonController = FindObjectOfType<DungeonController>();
            if (dungeonController != null)
            {
                Debug.Log("DungeonController 발견됨");
                
                // 던전 생성 요청 이벤트 발행
                DungeonEvents.RequestDungeonGeneration(0);
            }
            else
            {
                Debug.LogWarning("DungeonController를 찾을 수 없음");
            }
        }

        // 이벤트 핸들러들
        private void OnTestDungeonGenerationRequested(int stageIndex)
        {
            Debug.Log($"[테스트] 던전 생성 요청 이벤트 수신: 스테이지 {stageIndex}");
        }

        private void OnTestNodeMoveRequested(int nodeIndex)
        {
            Debug.Log($"[테스트] 노드 이동 요청 이벤트 수신: 노드 {nodeIndex}");
        }

        #region UI 테스트 버튼들
        // [Header("수동 테스트 버튼들")]
        public void TestNewGameFlow()
        {
            Debug.Log("=== 새 게임 플로우 테스트 ===");
            
            // 1. 게임 매니저 통해 새 게임 시작
            var gameManager = ImprovedGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartNewGame();
            }
        }

        public void TestDungeonEntryFlow()
        {
            Debug.Log("=== 던전 진입 플로우 테스트 ===");
            
            // 1. 상태를 던전으로 변경
            GameStateMachine.Instance?.EnterDungeon();
            
            // 2. 던전 생성 이벤트 발행
            DungeonEvents.RequestDungeonGeneration(0);
        }

        public void TestNodeMoveFlow()
        {
            Debug.Log("=== 노드 이동 플로우 테스트 ===");
            
            // 임의의 노드 이동 요청
            DungeonEvents.RequestNodeMove(1);
        }

        public void LogCurrentSystemState()
        {
            Debug.Log("=== 현재 시스템 상태 ===");
            
            // 게임 상태
            var stateMachine = GameStateMachine.Instance;
            if (stateMachine != null)
            {
                Debug.Log($"현재 게임 상태: {stateMachine.CurrentState}");
            }
            
            // 시스템 존재 여부 확인
            var dungeonController = FindObjectOfType<DungeonController>();
            var uiController = FindObjectOfType<UIController>();
            var gameManager = ImprovedGameManager.Instance;
            var eventBus = EventBus.Instance;
            
            Debug.Log($"DungeonController: {(dungeonController != null ? "존재" : "없음")}");
            Debug.Log($"UIController: {(uiController != null ? "존재" : "없음")}");
            Debug.Log($"ImprovedGameManager: {(gameManager != null ? "존재" : "없음")}");
            Debug.Log($"EventBus: {(eventBus != null ? "존재" : "없음")}");
            
            if (gameManager != null)
            {
                Debug.Log($"GameManager 초기화 상태: {gameManager.IsInitialized}");
            }
        }
        #endregion
    }
}
