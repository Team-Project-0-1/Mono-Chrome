using UnityEngine;
using MonoChrome.Core;
using MonoChrome.UI;
using MonoChrome.Events;
using MonoChrome.Dungeon;
using System.Collections.Generic;

namespace MonoChrome.Core
{
    /// <summary>
    /// 통합 UI 브릿지 - CoreUIManager와 새로운 UI 시스템들을 통합
    /// 포트폴리오 품질: 명확한 UI 시스템 통합과 이벤트 기반 아키텍처
    /// </summary>
    public class UnifiedUIBridge : MonoBehaviour
    {
        [Header("UI 시스템 설정")]
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _useNewUIController = false; // 기본적으로 CoreUIManager 사용
        [SerializeField] private bool _maintainLegacySupport = true;

        // UI 시스템 참조들
        private UIController _newUIController;
        private CoreUIManager _coreUIManager;  // CoreUIManager로 수정
        private ImprovedUIManager _improvedUIManager;
        
        // 현재 활성 UI 시스템 상태
        private bool _isInitialized = false;

        private void Awake()
        {
            InitializeUIBridge();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// UI 브릿지 초기화
        /// </summary>
        private void InitializeUIBridge()
        {
            LogDebug("UI 브릿지 초기화 시작...");

            // 기존 UI 시스템들 찾기
            FindUIControllers();

            // 우선순위에 따른 UI 시스템 설정
            SetupUISystemPriority();

            _isInitialized = true;
            LogDebug("UI 브릿지 초기화 완료");
        }

        /// <summary>
        /// UI 컨트롤러들 찾기
        /// </summary>
        private void FindUIControllers()
        {
            // 새로운 UIController 찾기
            _newUIController = FindFirstObjectByType<UIController>();
            if (_newUIController != null)
            {
                LogDebug("새로운 UIController 발견");
            }

            // CoreUIManager 찾기
            _coreUIManager = FindFirstObjectByType<CoreUIManager>();
            if (_coreUIManager != null)
            {
                LogDebug("CoreUIManager 발견");
            }

            // ImprovedUIManager 찾기
            _improvedUIManager = FindFirstObjectByType<ImprovedUIManager>();
            if (_improvedUIManager != null)
            {
                LogDebug("ImprovedUIManager 발견");
            }
        }

        /// <summary>
        /// UI 시스템 우선순위 설정
        /// </summary>
        private void SetupUISystemPriority()
        {
            if (_useNewUIController && _newUIController != null)
            {
                LogDebug("새로운 UIController를 기본 UI 시스템으로 사용");
            }
            else if (_coreUIManager != null)
            {
                LogDebug("CoreUIManager를 기본 UI 시스템으로 사용");
            }
            else if (_improvedUIManager != null)
            {
                LogDebug("ImprovedUIManager를 기본 UI 시스템으로 사용");
            }
            else
            {
                LogDebug("경고: 사용 가능한 UI 컨트롤러가 없습니다!");
            }
        }

        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
        {
            // UI 이벤트들
            UIEvents.OnPanelShowRequested += OnPanelShowRequested;
            UIEvents.OnDungeonMapUpdateRequested += OnDungeonMapUpdateRequested;
            UIEvents.OnPlayerStatusUpdateRequested += OnPlayerStatusUpdateRequested;
            UIEvents.OnDungeonUIUpdateRequested += OnDungeonUIUpdateRequested;

            // 게임 상태 변경 이벤트
            GameStateMachine.OnStateChanged += OnGameStateChanged;

            LogDebug("UI 이벤트 구독 완료");
        }

        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // UI 이벤트들
            UIEvents.OnPanelShowRequested -= OnPanelShowRequested;
            UIEvents.OnDungeonMapUpdateRequested -= OnDungeonMapUpdateRequested;
            UIEvents.OnPlayerStatusUpdateRequested -= OnPlayerStatusUpdateRequested;
            UIEvents.OnDungeonUIUpdateRequested -= OnDungeonUIUpdateRequested;

            // 게임 상태 변경 이벤트
            GameStateMachine.OnStateChanged -= OnGameStateChanged;

            LogDebug("UI 이벤트 구독 해제 완료");
        }

        #region Event Handlers
        /// <summary>
        /// 패널 표시 요청 처리
        /// </summary>
        private void OnPanelShowRequested(string panelName)
        {
            LogDebug($"패널 표시 요청: {panelName}");

            // 우선순위에 따라 UI 시스템 선택
            if (_useNewUIController && _newUIController != null)
            {
                _newUIController.ShowPanel(panelName);
            }
            else
            {
                // CoreUIManager는 패널별 직접 처리가 아닌 게임 상태 기반으로 작동
                ShowPanelThroughGameState(panelName);
            }
        }

        /// <summary>
        /// 게임 상태를 통한 패널 표시
        /// </summary>
        private void ShowPanelThroughGameState(string panelName)
        {
            // CoreUIManager는 게임 상태에 따라 자동으로 패널을 표시하므로
            // 해당 게임 상태로 전환하는 방식을 사용
            var gameStateMachine = GameStateMachine.Instance;
            if (gameStateMachine == null) return;

            switch (panelName)
            {
                case "CharacterSelectionPanel":
                    gameStateMachine.TryChangeState(GameStateMachine.GameState.CharacterSelection);
                    break;
                case "DungeonPanel":
                    gameStateMachine.TryChangeState(GameStateMachine.GameState.Dungeon);
                    break;
                case "CombatPanel":
                    gameStateMachine.TryChangeState(GameStateMachine.GameState.Combat);
                    break;
                case "GameOverPanel":
                    gameStateMachine.TryChangeState(GameStateMachine.GameState.GameOver);
                    break;
                case "VictoryPanel":
                    gameStateMachine.TryChangeState(GameStateMachine.GameState.Victory);
                    break;
                default:
                    LogDebug($"알 수 없는 패널: {panelName}");
                    break;
            }
        }

        /// <summary>
        /// 던전 맵 업데이트 요청 처리
        /// </summary>
        private void OnDungeonMapUpdateRequested(List<DungeonNode> nodes, int currentIndex)
        {
            LogDebug($"던전 맵 업데이트 요청: {nodes.Count}개 노드, 현재 인덱스 {currentIndex}");

            // ImprovedUIManager가 있다면 사용
            if (_improvedUIManager != null)
            {
                _improvedUIManager.UpdateDungeonMap(nodes, currentIndex);
            }
            else
            {
                LogDebug("던전 맵 업데이트: 적절한 UI 매니저를 찾을 수 없음");
            }
        }

        /// <summary>
        /// 플레이어 상태 업데이트 요청 처리
        /// </summary>
        private void OnPlayerStatusUpdateRequested(int currentHealth, int maxHealth)
        {
            LogDebug($"플레이어 상태 업데이트 요청: {currentHealth}/{maxHealth}");

            // 여기에 플레이어 상태 업데이트 로직 추가
            // 현재는 로그만 출력
        }

        /// <summary>
        /// 던전 UI 업데이트 요청 처리
        /// </summary>
        private void OnDungeonUIUpdateRequested()
        {
            LogDebug("던전 UI 업데이트 요청");

            // CoreUIManager를 통해 던전 UI 업데이트
            if (_coreUIManager != null)
            {
                _coreUIManager.UpdateDungeonUI();
            }
            else if (_improvedUIManager != null)
            {
                _improvedUIManager.InitializeDungeonUI();
            }
            else
            {
                LogDebug("경고: 던전 UI를 업데이트할 UI 매니저가 없습니다!");
            }
        }

        /// <summary>
        /// 게임 상태 변경 처리
        /// </summary>
        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            LogDebug($"게임 상태 변경 감지: {previousState} -> {newState}");

            // CoreUIManager가 자동으로 상태 변경을 처리하므로 추가 작업은 최소화
            // 필요한 경우에만 특별한 UI 작업 수행
            switch (newState)
            {
                case GameStateMachine.GameState.Dungeon:
                    // 던전 상태일 때 UI 업데이트
                    if (_coreUIManager != null)
                    {
                        _coreUIManager.UpdateDungeonUI();
                    }
                    break;
            }
        }
        #endregion

        #region Public API
        /// <summary>
        /// 수동으로 패널 표시
        /// </summary>
        public void ShowPanel(string panelName)
        {
            UIEvents.RequestPanelShow(panelName);
        }

        /// <summary>
        /// 던전 맵 업데이트
        /// </summary>
        public void UpdateDungeonMap(List<DungeonNode> nodes, int currentIndex)
        {
            UIEvents.RequestDungeonMapUpdate(nodes, currentIndex);
        }

        /// <summary>
        /// 플레이어 상태 업데이트
        /// </summary>
        public void UpdatePlayerStatus(int currentHealth, int maxHealth)
        {
            UIEvents.RequestPlayerStatusUpdate(currentHealth, maxHealth);
        }

        /// <summary>
        /// UI 시스템 전환 (런타임에서 변경 가능)
        /// </summary>
        public void SwitchUISystem(bool useNewController)
        {
            _useNewUIController = useNewController;
            SetupUISystemPriority();
            LogDebug($"UI 시스템 전환됨: {(useNewController ? "새로운 UIController" : "CoreUIManager")}");
        }
        #endregion

        #region Status & Debug
        /// <summary>
        /// 현재 활성 UI 시스템 반환
        /// </summary>
        public string GetActiveUISystem()
        {
            if (_useNewUIController && _newUIController != null)
                return "새로운 UIController";
            else if (_coreUIManager != null)
                return "CoreUIManager";
            else if (_improvedUIManager != null)
                return "ImprovedUIManager";
            else
                return "없음";
        }

        /// <summary>
        /// UI 브릿지 상태 리포트
        /// </summary>
        [ContextMenu("Generate UI Bridge Status Report")]
        public void GenerateStatusReport()
        {
            LogDebug("=== UI 브릿지 상태 리포트 ===");
            LogDebug($"초기화됨: {_isInitialized}");
            LogDebug($"새로운 UIController: {(_newUIController != null ? "존재" : "없음")}");
            LogDebug($"CoreUIManager: {(_coreUIManager != null ? "존재" : "없음")}");
            LogDebug($"ImprovedUIManager: {(_improvedUIManager != null ? "존재" : "없음")}");
            LogDebug($"현재 활성 UI 시스템: {GetActiveUISystem()}");
            LogDebug($"레거시 지원: {_maintainLegacySupport}");
            LogDebug("===========================");
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[UnifiedUIBridge] {message}");
            }
        }
        #endregion
    }
}
