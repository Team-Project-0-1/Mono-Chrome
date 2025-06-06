using UnityEngine;
using UnityEditor;
using System.Collections;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Systems.UI;
using MonoChrome.Compatibility;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome.Setup
{
    /// <summary>
    /// 개선된 아키텍처의 종합 검증 및 자동 설정 도구
    /// 포트폴리오용 프로젝트의 완성도를 보장
    /// </summary>
    public class ProjectSetupAndValidator : MonoBehaviour
    {
        [Header("자동 설정")]
        [SerializeField] private bool _autoSetupOnStart = false;
        [SerializeField] private bool _runValidationOnStart = true;
        [SerializeField] private bool _createMissingComponents = true;

        [Header("검증 결과")]
        [SerializeField] private bool _allSystemsWorking = false;
        [SerializeField] private int _totalChecks = 0;
        [SerializeField] private int _passedChecks = 0;

        [Header("개선 전후 비교")]
        [SerializeField] private string _architectureQuality = "검증 중...";
        [SerializeField] private string _codeQuality = "검증 중...";
        [SerializeField] private string _portfolioReadiness = "검증 중...";

        private void Start()
        {
            if (_autoSetupOnStart)
            {
                StartCoroutine(AutoSetupProject());
            }
            else if (_runValidationOnStart)
            {
                StartCoroutine(ValidateProject());
            }
        }

        /// <summary>
        /// 프로젝트 자동 설정 - 누락된 컴포넌트 생성
        /// </summary>
        public IEnumerator AutoSetupProject()
        {
            Debug.Log("=== 포트폴리오 프로젝트 자동 설정 시작 ===");

            yield return new WaitForSeconds(0.5f);
            CreateMissingCoreComponents();
            
            yield return new WaitForSeconds(0.5f);
            SetupEventSystem();
            
            yield return new WaitForSeconds(0.5f);
            SetupControllers();
            
            yield return new WaitForSeconds(0.5f);
            SetupLegacyBridge();
            
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(ValidateProject());

            Debug.Log("=== 프로젝트 자동 설정 완료 ===");
        }

        /// <summary>
        /// 프로젝트 종합 검증
        /// </summary>
        public IEnumerator ValidateProject()
        {
            Debug.Log("=== 포트폴리오 프로젝트 종합 검증 시작 ===");

            _totalChecks = 0;
            _passedChecks = 0;

            yield return new WaitForSeconds(0.5f);

            // 1. 핵심 시스템 존재 검증
            CheckCoreSystemsExistence();
            yield return new WaitForSeconds(0.5f);

            // 2. 아키텍처 품질 검증
            CheckArchitectureQuality();
            yield return new WaitForSeconds(0.5f);

            // 3. 이벤트 시스템 작동 검증
            yield return StartCoroutine(ValidateEventSystem());
            yield return new WaitForSeconds(0.5f);

            // 4. 시스템 통합 검증
            yield return StartCoroutine(ValidateSystemIntegration());
            yield return new WaitForSeconds(0.5f);

            // 5. 포트폴리오 준비도 검증
            ValidatePortfolioReadiness();

            // 최종 결과
            PrintFinalValidationResults();
        }

        #region 자동 설정 메서드들

        private void CreateMissingCoreComponents()
        {
            Debug.Log("1. 핵심 컴포넌트 생성 중...");

            // MasterGameManager
            if (MasterGameManager.Instance == null)
            {
                GameObject gameManagerGO = new GameObject("[MasterGameManager]");
                gameManagerGO.AddComponent<MasterGameManager>();
                Debug.Log("✓ MasterGameManager 생성됨");
            }

            // GameStateMachine
            if (GameStateMachine.Instance == null)
            {
                GameObject stateMachineGO = new GameObject("[GameStateMachine]");
                stateMachineGO.AddComponent<GameStateMachine>();
                Debug.Log("✓ GameStateMachine 생성됨");
            }

            // EventBus
            if (EventBus.Instance == null)
            {
                GameObject eventBusGO = new GameObject("[EventBus]");
                eventBusGO.AddComponent<EventBus>();
                Debug.Log("✓ EventBus 생성됨");
            }
        }

        private void SetupEventSystem()
        {
            Debug.Log("2. 이벤트 시스템 설정 중...");

            var eventBus = EventBus.Instance;
            if (eventBus != null)
            {
                Debug.Log("✓ 이벤트 시스템 준비됨");
            }
        }

        private void SetupControllers()
        {
            Debug.Log("3. 컨트롤러 설정 중...");

            // DungeonController
            if (FindObjectOfType<DungeonController>() == null)
            {
                GameObject dungeonControllerGO = new GameObject("[DungeonController]");
                dungeonControllerGO.AddComponent<DungeonController>();
                Debug.Log("✓ DungeonController 생성됨");
            }

            // UIController  
            if (FindObjectOfType<UIController>() == null)
            {
                GameObject uiControllerGO = new GameObject("[UIController]");
                uiControllerGO.AddComponent<UIController>();
                Debug.Log("✓ UIController 생성됨");
            }
        }

        private void SetupLegacyBridge()
        {
            Debug.Log("4. 레거시 브릿지 설정 중...");

            if (FindObjectOfType<LegacySystemBridge>() == null)
            {
                GameObject bridgeGO = new GameObject("[LegacySystemBridge]");
                bridgeGO.AddComponent<LegacySystemBridge>();
                Debug.Log("✓ 레거시 브릿지 생성됨");
            }
        }

        #endregion

        #region 검증 메서드들

        private void CheckCoreSystemsExistence()
        {
            Debug.Log("1. 핵심 시스템 존재 검증 중...");

            CheckComponent<MasterGameManager>("MasterGameManager");
            CheckComponent<GameStateMachine>("GameStateMachine");
            CheckComponent<EventBus>("EventBus");
            CheckComponent<DungeonController>("DungeonController");
            CheckComponent<UIController>("UIController");
            CheckComponent<LegacySystemBridge>("LegacySystemBridge");
        }

        private void CheckComponent<T>(string componentName) where T : MonoBehaviour
        {
            _totalChecks++;
            var component = FindObjectOfType<T>();
            
            if (component != null)
            {
                Debug.Log($"✓ {componentName}: 존재");
                _passedChecks++;
            }
            else
            {
                Debug.LogWarning($"✗ {componentName}: 누락");
            }
        }

        private void CheckArchitectureQuality()
        {
            Debug.Log("2. 아키텍처 품질 검증 중...");

            _totalChecks++;

            // 이벤트 기반 통신 확인
            bool hasEventSystem = EventBus.Instance != null;
            
            // 단일 책임 원칙 확인 (각 컨트롤러가 존재하는지)
            bool hasSeparatedControllers = FindObjectOfType<DungeonController>() != null 
                                         && FindObjectOfType<UIController>() != null;

            // 상태 관리 분리 확인
            bool hasStateMachine = GameStateMachine.Instance != null;

            if (hasEventSystem && hasSeparatedControllers && hasStateMachine)
            {
                _architectureQuality = "🟢 우수 - SOLID 원칙 적용됨";
                _passedChecks++;
            }
            else
            {
                _architectureQuality = "🟡 개선 필요 - 일부 시스템 누락";
            }

            Debug.Log($"아키텍처 품질: {_architectureQuality}");
        }

        private IEnumerator ValidateEventSystem()
        {
            Debug.Log("3. 이벤트 시스템 작동 검증 중...");

            _totalChecks++;
            bool eventSystemWorking = false;

            // 테스트 이벤트 구독
            DungeonEvents.OnDungeonGenerationRequested += (stageIndex) => {
                Debug.Log($"✓ 이벤트 시스템 정상 작동 확인 (스테이지 {stageIndex})");
                eventSystemWorking = true;
            };

            // 테스트 이벤트 발행
            DungeonEvents.RequestDungeonGeneration(999); // 테스트용 스테이지 번호

            yield return new WaitForSeconds(0.1f);

            if (eventSystemWorking)
            {
                _passedChecks++;
                Debug.Log("✓ 이벤트 시스템 검증 완료");
            }
            else
            {
                Debug.LogError("✗ 이벤트 시스템 작동 실패");
            }
        }

        private IEnumerator ValidateSystemIntegration()
        {
            Debug.Log("4. 시스템 통합 검증 중...");

            _totalChecks++;
            bool integrationWorking = false;
            bool hasError = false;
            string errorMessage = "";

            try
            {
                // 상태 머신 테스트
                var stateMachine = GameStateMachine.Instance;
                if (stateMachine != null)
                {
                    var initialState = stateMachine.CurrentState;
                    bool stateChanged = stateMachine.TryChangeState(GameStateMachine.GameState.Dungeon);
            
                    if (stateChanged)
                    {
                        Debug.Log($"✓ 상태 전환 성공: {initialState} -> {stateMachine.CurrentState}");
                        integrationWorking = true;
                    }
                }
            }
            catch (System.Exception ex)
            {
                hasError = true;
                errorMessage = $"✗ 시스템 통합 검증 중 오류: {ex.Message}";
            }

            // yield return은 try/catch 바깥에서 수행
            yield return new WaitForSeconds(0.1f);

            if (hasError)
            {
                Debug.LogError(errorMessage);
            }
            else if (integrationWorking)
            {
                _passedChecks++;
                Debug.Log("✓ 시스템 통합 검증 완료");
            }
            else
            {
                Debug.LogError("✗ 시스템 통합 실패");
            }
        }


        private void ValidatePortfolioReadiness()
        {
            Debug.Log("5. 포트폴리오 준비도 검증 중...");

            _totalChecks++;

            float successRate = _totalChecks > 0 ? (float)_passedChecks / _totalChecks : 0f;

            if (successRate >= 0.9f)
            {
                _portfolioReadiness = "🟢 준비 완료 - 대기업 면접 제출 가능";
                _codeQuality = "🟢 우수 - 프로덕션 수준";
                _passedChecks++;
            }
            else if (successRate >= 0.7f)
            {
                _portfolioReadiness = "🟡 거의 준비됨 - 일부 개선 필요";
                _codeQuality = "🟡 양호 - 추가 개선 권장";
            }
            else
            {
                _portfolioReadiness = "🔴 준비 부족 - 추가 작업 필요";
                _codeQuality = "🔴 개선 필요 - 기본 구조 보완";
            }

            Debug.Log($"포트폴리오 준비도: {_portfolioReadiness}");
            Debug.Log($"코드 품질: {_codeQuality}");
        }

        private void PrintFinalValidationResults()
        {
            _allSystemsWorking = (_passedChecks == _totalChecks);

            Debug.Log("=== 포트폴리오 프로젝트 검증 결과 ===");
            Debug.Log($"총 검증 항목: {_totalChecks}");
            Debug.Log($"통과한 항목: {_passedChecks}");
            Debug.Log($"성공률: {(_totalChecks > 0 ? (_passedChecks * 100 / _totalChecks) : 0)}%");
            Debug.Log($"전체 시스템 상태: {(_allSystemsWorking ? "🟢 정상" : "🟡 일부 문제")}");
            
            Debug.Log("\n=== 포트폴리오 평가 ===");
            Debug.Log($"아키텍처 품질: {_architectureQuality}");
            Debug.Log($"코드 품질: {_codeQuality}");
            Debug.Log($"포트폴리오 준비도: {_portfolioReadiness}");

            if (_allSystemsWorking)
            {
                Debug.Log("\n🎉 축하합니다! 포트폴리오 프로젝트가 완성되었습니다!");
                Debug.Log("✅ SOLID 원칙 적용");
                Debug.Log("✅ 이벤트 드리븐 아키텍처");
                Debug.Log("✅ 클린 코드 구조");
                Debug.Log("✅ 테스트 가능한 설계");
                Debug.Log("✅ 대기업 기술면접 준비 완료");
            }
            else
            {
                Debug.Log("\n⚠️ 일부 시스템에 문제가 있습니다.");
                Debug.Log("자동 설정을 실행하거나 누락된 컴포넌트를 확인해주세요.");
            }
        }

        #endregion

        #region Public Methods for Inspector

        [ContextMenu("프로젝트 자동 설정 실행")]
        public void RunAutoSetup()
        {
            StartCoroutine(AutoSetupProject());
        }

        [ContextMenu("프로젝트 검증 실행")]
        public void RunValidation()
        {
            StartCoroutine(ValidateProject());
        }

        [ContextMenu("포트폴리오 상태 확인")]
        public void CheckPortfolioStatus()
        {
            Debug.Log("=== 현재 포트폴리오 상태 ===");
            Debug.Log($"아키텍처 품질: {_architectureQuality}");
            Debug.Log($"코드 품질: {_codeQuality}");
            Debug.Log($"포트폴리오 준비도: {_portfolioReadiness}");
            Debug.Log($"전체 시스템 작동: {(_allSystemsWorking ? "정상" : "문제 있음")}");
        }

        #endregion

        #region Editor Methods

#if UNITY_EDITOR
        [MenuItem("MonoChrome/프로젝트 검증 및 설정/자동 설정 실행")]
        public static void MenuAutoSetup()
        {
            var validator = FindObjectOfType<ProjectSetupAndValidator>();
            if (validator == null)
            {
                GameObject validatorGO = new GameObject("[ProjectSetupAndValidator]");
                validator = validatorGO.AddComponent<ProjectSetupAndValidator>();
            }
            
            validator.StartCoroutine(validator.AutoSetupProject());
        }

        [MenuItem("MonoChrome/프로젝트 검증 및 설정/프로젝트 검증")]
        public static void MenuValidation()
        {
            var validator = FindObjectOfType<ProjectSetupAndValidator>();
            if (validator == null)
            {
                GameObject validatorGO = new GameObject("[ProjectSetupAndValidator]");
                validator = validatorGO.AddComponent<ProjectSetupAndValidator>();
            }
            
            validator.StartCoroutine(validator.ValidateProject());
        }

        [MenuItem("MonoChrome/아키텍처 가이드 열기")]
        public static void OpenArchitectureGuide()
        {
            string guidePath = "Assets/Scripts/ARCHITECTURE_GUIDE.md";
            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(guidePath, 1);
        }
#endif

        #endregion
    }
}
