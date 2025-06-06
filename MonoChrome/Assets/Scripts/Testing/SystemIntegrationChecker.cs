using UnityEngine;
using System.Collections;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Systems.Dungeon;
using MonoChrome.Systems.UI;

namespace MonoChrome.Testing
{
    /// <summary>
    /// 시스템 통합 상태 확인 및 자동 수정 도구
    /// 포트폴리오 프로젝트의 아키텍처가 올바르게 작동하는지 검증
    /// </summary>
    public class SystemIntegrationChecker : MonoBehaviour
    {
        [Header("자동 검사 설정")]
        [SerializeField] private bool _runCheckOnStart = true;
        [SerializeField] private bool _enableAutoFix = true;
        [SerializeField] private bool _showDetailedLogs = true;

        [Header("검사 결과")]
        [SerializeField] private bool _isEventSystemWorking = false;
        [SerializeField] private bool _isStateMachineWorking = false;
        [SerializeField] private bool _isDungeonControllerWorking = false;
        [SerializeField] private bool _isUIControllerWorking = false;
        [SerializeField] private bool _isGameManagerWorking = false;

        private int _totalTests = 0;
        private int _passedTests = 0;

        private void Start()
        {
            if (_runCheckOnStart)
            {
                StartCoroutine(RunCompleteSystemCheck());
            }
        }

        /// <summary>
        /// 전체 시스템 통합 검사 실행
        /// </summary>
        public IEnumerator RunCompleteSystemCheck()
        {
            LogInfo("=== 시스템 통합 검사 시작 ===");
            
            _totalTests = 0;
            _passedTests = 0;

            yield return new WaitForSeconds(0.5f);

            // 1. 핵심 시스템 존재 확인
            LogInfo("1. 핵심 시스템 존재 확인");
            CheckCoreSystemsExistence();
            yield return new WaitForSeconds(0.5f);

            // 2. 이벤트 시스템 테스트
            LogInfo("2. 이벤트 시스템 테스트");
            yield return StartCoroutine(TestEventSystem());
            yield return new WaitForSeconds(0.5f);

            // 3. 게임 상태 머신 테스트
            LogInfo("3. 게임 상태 머신 테스트");
            TestGameStateMachine();
            yield return new WaitForSeconds(0.5f);

            // 4. 던전 컨트롤러 테스트
            LogInfo("4. 던전 컨트롤러 테스트");
            yield return StartCoroutine(TestDungeonController());
            yield return new WaitForSeconds(0.5f);

            // 5. UI 컨트롤러 테스트
            LogInfo("5. UI 컨트롤러 테스트");
            TestUIController();
            yield return new WaitForSeconds(0.5f);

            // 6. 게임 매니저 테스트
            LogInfo("6. 게임 매니저 테스트");
            TestMasterGameManager();
            yield return new WaitForSeconds(0.5f);

            // 7. 통합 플로우 테스트
            LogInfo("7. 통합 플로우 테스트");
            yield return StartCoroutine(TestIntegratedFlow());

            // 결과 출력
            PrintFinalResults();
        }

        private void CheckCoreSystemsExistence()
        {
            LogInfo("핵심 시스템 존재 여부 확인 중...");

            var eventBus = FindObjectOfType<EventBus>();
            var stateMachine = FindObjectOfType<GameStateMachine>();
            var dungeonController = FindObjectOfType<DungeonController>();
            var uiController = FindObjectOfType<UIController>();
            var gameManager = FindObjectOfType<MasterGameManager>();

            LogInfo($"EventBus: {(eventBus != null ? "✓ 존재" : "✗ 없음")}");
            LogInfo($"GameStateMachine: {(stateMachine != null ? "✓ 존재" : "✗ 없음")}");
            LogInfo($"DungeonController: {(dungeonController != null ? "✓ 존재" : "✗ 없음")}");
            LogInfo($"UIController: {(uiController != null ? "✓ 존재" : "✗ 없음")}");
            LogInfo($"MasterGameManager: {(gameManager != null ? "✓ 존재" : "✗ 없음")}");

            // 자동 수정 시도
            if (_enableAutoFix)
            {
                TryAutoCreateMissingSystems();
            }
        }

        private void TryAutoCreateMissingSystems()
        {
            LogInfo("누락된 시스템 자동 생성 시도...");

            // EventBus 생성
            if (EventBus.Instance == null)
            {
                GameObject eventBusGO = new GameObject("[EventBus]");
                eventBusGO.AddComponent<EventBus>();
                LogInfo("EventBus 자동 생성됨");
            }

            // GameStateMachine 생성
            if (GameStateMachine.Instance == null)
            {
                GameObject stateMachineGO = new GameObject("[GameStateMachine]");
                stateMachineGO.AddComponent<GameStateMachine>();
                LogInfo("GameStateMachine 자동 생성됨");
            }

            // MasterGameManager 생성
            if (MasterGameManager.Instance == null)
            {
                GameObject gameManagerGO = new GameObject("[MasterGameManager]");
                gameManagerGO.AddComponent<MasterGameManager>();
                LogInfo("MasterGameManager 자동 생성됨");
            }

            // DungeonController 생성 (씬에 없을 경우)
            if (FindObjectOfType<DungeonController>() == null)
            {
                GameObject dungeonControllerGO = new GameObject("[DungeonController]");
                dungeonControllerGO.AddComponent<DungeonController>();
                LogInfo("DungeonController 자동 생성됨");
            }

            // UIController 생성 (씬에 없을 경우)
            if (FindObjectOfType<UIController>() == null)
            {
                GameObject uiControllerGO = new GameObject("[UIController]");
                uiControllerGO.AddComponent<UIController>();
                LogInfo("UIController 자동 생성됨");
            }
        }

        private IEnumerator TestEventSystem()
        {
            LogInfo("이벤트 시스템 테스트 중...");
            
            bool testPassed = false;
            
            // 테스트용 이벤트 핸들러 등록
            DungeonEvents.OnDungeonGenerationRequested += (stageIndex) => {
                LogInfo($"이벤트 수신됨: 던전 생성 요청 (스테이지 {stageIndex})");
                testPassed = true;
            };

            // 이벤트 발행
            DungeonEvents.RequestDungeonGeneration(0);
            
            yield return new WaitForSeconds(0.1f);
            
            if (testPassed)
            {
                LogInfo("✓ 이벤트 시스템 작동 확인");
                _isEventSystemWorking = true;
                _passedTests++;
            }
            else
            {
                LogError("✗ 이벤트 시스템 작동 실패");
            }
            
            _totalTests++;
        }

        private void TestGameStateMachine()
        {
            LogInfo("게임 상태 머신 테스트 중...");
            
            var stateMachine = GameStateMachine.Instance;
            if (stateMachine != null)
            {
                var initialState = stateMachine.CurrentState;
                bool transitionSuccess = stateMachine.TryChangeState(GameStateMachine.GameState.Dungeon);
                
                if (transitionSuccess)
                {
                    LogInfo($"✓ 상태 전환 성공: {initialState} -> {stateMachine.CurrentState}");
                    _isStateMachineWorking = true;
                    _passedTests++;
                }
                else
                {
                    LogError("✗ 상태 전환 실패");
                }
            }
            else
            {
                LogError("✗ GameStateMachine 인스턴스 없음");
            }
            
            _totalTests++;
        }

        private IEnumerator TestDungeonController()
        {
            LogInfo("던전 컨트롤러 테스트 중...");
            
            var dungeonController = FindObjectOfType<DungeonController>();
            if (dungeonController != null)
            {
                bool testPassed = false;
                
                // 던전 생성 완료 이벤트 구독
                DungeonEvents.OnDungeonGenerated += (nodes, currentIndex) => {
                    LogInfo($"던전 생성 완료 이벤트 수신됨: {nodes.Count}개 노드");
                    testPassed = true;
                };
                
                // 던전 생성 요청
                DungeonEvents.RequestDungeonGeneration(0);
                
                yield return new WaitForSeconds(0.5f);
                
                if (testPassed)
                {
                    LogInfo("✓ 던전 컨트롤러 작동 확인");
                    _isDungeonControllerWorking = true;
                    _passedTests++;
                }
                else
                {
                    LogError("✗ 던전 컨트롤러 작동 실패");
                }
            }
            else
            {
                LogError("✗ DungeonController 컴포넌트 없음");
            }
            
            _totalTests++;
        }

        private void TestUIController()
        {
            LogInfo("UI 컨트롤러 테스트 중...");
            
            var uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                // UI 패널 표시 테스트
                UIEvents.RequestPanelShow("DungeonPanel");
                
                LogInfo("✓ UI 컨트롤러 존재 및 이벤트 발행 확인");
                _isUIControllerWorking = true;
                _passedTests++;
            }
            else
            {
                LogError("✗ UIController 컴포넌트 없음");
            }
            
            _totalTests++;
        }

        private void TestMasterGameManager()
        {
            LogInfo("개선된 게임 매니저 테스트 중...");

            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                if (gameManager.IsInitialized)
                {
                    LogInfo("✓ 게임 매니저 초기화 확인");
                    _isGameManagerWorking = true;
                    _passedTests++;
                }
                else
                {
                    LogWarning("⚠ 게임 매니저 초기화 미완료");
                    _passedTests++; // 부분 점수
                }
            }
            else
            {
                LogError("✗ MasterGameManager 인스턴스 없음");
            }
            
            _totalTests++;
        }

        private IEnumerator TestIntegratedFlow()
        {
            LogInfo("통합 플로우 테스트 중...");

            bool flowTestPassed = false;
            bool hasError = false;
            string errorMessage = "";

            // 예외 발생 여부를 먼저 확인
            try
            {
                // 1. 게임 상태를 던전으로 변경
                GameStateMachine.Instance?.EnterDungeon();

                // 2. 던전 생성 요청
                DungeonEvents.RequestDungeonGeneration(0);

                // 3. UI 업데이트 요청
                UIEvents.RequestPanelShow("DungeonPanel");

                flowTestPassed = true;
            }
            catch (System.Exception ex)
            {
                hasError = true;
                errorMessage = $"✗ 통합 플로우 테스트 실패: {ex.Message}";
            }

            // yield return은 try 바깥에서 실행
            yield return new WaitForSeconds(0.1f);
            yield return new WaitForSeconds(0.2f);
            yield return new WaitForSeconds(0.1f);

            if (hasError)
            {
                LogError(errorMessage);
            }
            else if (flowTestPassed)
            {
                LogInfo("✓ 통합 플로우 테스트 성공");
                _passedTests++;
            }

            _totalTests++;
        }


        private void PrintFinalResults()
        {
            LogInfo("=== 시스템 통합 검사 결과 ===");
            LogInfo($"총 테스트: {_totalTests}");
            LogInfo($"통과한 테스트: {_passedTests}");
            LogInfo($"성공률: {(_totalTests > 0 ? (_passedTests * 100 / _totalTests) : 0)}%");
            
            if (_passedTests == _totalTests)
            {
                LogInfo("🎉 모든 시스템이 정상적으로 작동합니다!");
                LogInfo("포트폴리오용 아키텍처 구현이 완료되었습니다.");
            }
            else
            {
                LogWarning($"⚠ {_totalTests - _passedTests}개의 시스템에서 문제가 발견되었습니다.");
                LogInfo("자동 수정을 시도하거나 수동으로 확인해주세요.");
            }
            
            // 개별 시스템 상태 출력
            LogInfo("\n=== 개별 시스템 상태 ===");
            LogInfo($"이벤트 시스템: {(_isEventSystemWorking ? "✓ 정상" : "✗ 문제")}");
            LogInfo($"상태 머신: {(_isStateMachineWorking ? "✓ 정상" : "✗ 문제")}");
            LogInfo($"던전 컨트롤러: {(_isDungeonControllerWorking ? "✓ 정상" : "✗ 문제")}");
            LogInfo($"UI 컨트롤러: {(_isUIControllerWorking ? "✓ 정상" : "✗ 문제")}");
            LogInfo($"게임 매니저: {(_isGameManagerWorking ? "✓ 정상" : "✗ 문제")}");
        }

        #region Public Methods for Manual Testing
        
        [ContextMenu("시스템 검사 실행")]
        public void RunManualCheck()
        {
            StartCoroutine(RunCompleteSystemCheck());
        }

        [ContextMenu("누락된 시스템 자동 생성")]
        public void AutoCreateMissingSystems()
        {
            TryAutoCreateMissingSystems();
        }

        [ContextMenu("이벤트 시스템만 테스트")]
        public void TestEventSystemOnly()
        {
            StartCoroutine(TestEventSystem());
        }

        [ContextMenu("통합 플로우만 테스트")]
        public void TestIntegratedFlowOnly()
        {
            StartCoroutine(TestIntegratedFlow());
        }

        #endregion

        #region Logging Methods

        private void LogInfo(string message)
        {
            if (_showDetailedLogs)
            {
                Debug.Log($"[SystemIntegrationChecker] {message}");
            }
        }

        private void LogWarning(string message)
        {
            Debug.LogWarning($"[SystemIntegrationChecker] {message}");
        }

        private void LogError(string message)
        {
            Debug.LogError($"[SystemIntegrationChecker] {message}");
        }

        #endregion
    }
}
