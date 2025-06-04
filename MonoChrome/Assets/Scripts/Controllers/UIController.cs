using System.Collections.Generic;
using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;
using UnityEngine.UI;

namespace MonoChrome.UI
{
    /// <summary>
    /// UI 컨트롤러 - 순수하게 UI 표시만 담당 (뷰 레이어)
    /// 비즈니스 로직은 포함하지 않음 (단일 책임 원칙)
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("UI 패널들")]
        [SerializeField] private GameObject _mainMenuPanel;
        [SerializeField] private GameObject _characterSelectionPanel;
        [SerializeField] private GameObject _dungeonPanel;
        [SerializeField] private GameObject _combatPanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;

        private GameObject _currentActivePanel;
        private bool _isInitialized = false;

        private void Awake()
        {
            InitializeUIReferences();
        }

        private void Start()
        {
            InitializeUI();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeUIReferences()
        {
            Canvas canvas = FindOrCreateCanvas();
            FindUIComponents(canvas.transform);
            Debug.Log("[UIController] UI 참조 초기화 완료");
        }

        private Canvas FindOrCreateCanvas()
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }
            return canvas;
        }

        private void FindUIComponents(Transform canvasTransform)
        {
            _mainMenuPanel = FindPanel(canvasTransform, "MainMenuPanel");
            _characterSelectionPanel = FindPanel(canvasTransform, "CharacterSelectionPanel");
            _dungeonPanel = FindPanel(canvasTransform, "DungeonPanel");
            _combatPanel = FindPanel(canvasTransform, "CombatPanel");
            _gameOverPanel = FindPanel(canvasTransform, "GameOverPanel");
            _victoryPanel = FindPanel(canvasTransform, "VictoryPanel");
        }

        private GameObject FindPanel(Transform parent, string panelName)
        {
            Transform panelTransform = parent.Find(panelName);
            return panelTransform?.gameObject;
        }

        private void InitializeUI()
        {
            HideAllPanels();
            ShowPanelForCurrentState();
            _isInitialized = true;
            Debug.Log("[UIController] UI 초기화 완료");
        }

        private void SubscribeToEvents()
        {
            GameStateMachine.OnStateChanged += OnGameStateChanged;
            UIEvents.OnPanelShowRequested += OnPanelShowRequested;
            UIEvents.OnDungeonMapUpdateRequested += OnDungeonMapUpdateRequested;
            UIEvents.OnPlayerStatusUpdateRequested += OnPlayerStatusUpdateRequested;
        }

        private void UnsubscribeFromEvents()
        {
            GameStateMachine.OnStateChanged -= OnGameStateChanged;
            UIEvents.OnPanelShowRequested -= OnPanelShowRequested;
            UIEvents.OnDungeonMapUpdateRequested -= OnDungeonMapUpdateRequested;
            UIEvents.OnPlayerStatusUpdateRequested -= OnPlayerStatusUpdateRequested;
        }

        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            ShowPanelForState(newState);
        }

        private void OnPanelShowRequested(string panelName)
        {
            ShowPanel(panelName);
        }

        private void OnDungeonMapUpdateRequested(List<DungeonNode> nodes, int currentIndex)
        {
            UpdateDungeonMap(nodes, currentIndex);
        }

        private void OnPlayerStatusUpdateRequested(int currentHealth, int maxHealth)
        {
            UpdatePlayerStatus(currentHealth, maxHealth);
        }

        /// <summary>
        /// 특정 패널 표시 (외부에서 호출 가능)
        /// </summary>
        public void ShowPanel(string panelName)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"[UIController] UI가 아직 초기화되지 않음 - {panelName}");
                return;
            }

            GameObject targetPanel = GetPanelByName(panelName);
            
            if (targetPanel != null)
            {
                HideAllPanels();
                targetPanel.SetActive(true);
                _currentActivePanel = targetPanel;
                Debug.Log($"[UIController] 패널 표시됨 - {panelName}");
            }
            else
            {
                Debug.LogWarning($"[UIController] 패널을 찾을 수 없음 - {panelName}");
            }
        }

        private void ShowPanelForState(GameStateMachine.GameState state)
        {
            string panelName = GetPanelNameForState(state);
            if (!string.IsNullOrEmpty(panelName))
            {
                ShowPanel(panelName);
            }
        }

        private void ShowPanelForCurrentState()
        {
            if (GameStateMachine.Instance != null)
            {
                ShowPanelForState(GameStateMachine.Instance.CurrentState);
            }
        }

        private void HideAllPanels()
        {
            var allPanels = new[]
            {
                _mainMenuPanel, _characterSelectionPanel, _dungeonPanel,
                _combatPanel, _gameOverPanel, _victoryPanel
            };

            foreach (var panel in allPanels)
            {
                if (panel != null && panel.activeInHierarchy)
                {
                    panel.SetActive(false);
                }
            }
        }

        private GameObject GetPanelByName(string panelName)
        {
            return panelName switch
            {
                "MainMenuPanel" => _mainMenuPanel,
                "CharacterSelectionPanel" => _characterSelectionPanel,
                "DungeonPanel" => _dungeonPanel,
                "CombatPanel" => _combatPanel,
                "GameOverPanel" => _gameOverPanel,
                "VictoryPanel" => _victoryPanel,
                _ => null
            };
        }

        private string GetPanelNameForState(GameStateMachine.GameState state)
        {
            return state switch
            {
                GameStateMachine.GameState.MainMenu => "MainMenuPanel",
                GameStateMachine.GameState.CharacterSelection => "CharacterSelectionPanel",
                GameStateMachine.GameState.Dungeon => "DungeonPanel",
                GameStateMachine.GameState.Combat => "CombatPanel",
                GameStateMachine.GameState.Event => "DungeonPanel",
                GameStateMachine.GameState.Shop => "DungeonPanel",
                GameStateMachine.GameState.Rest => "DungeonPanel",
                GameStateMachine.GameState.GameOver => "GameOverPanel",
                GameStateMachine.GameState.Victory => "VictoryPanel",
                _ => null
            };
        }

        /// <summary>
        /// 던전 맵 업데이트 - 순수한 뷰 업데이트만 담당
        /// </summary>
        private void UpdateDungeonMap(List<DungeonNode> nodes, int currentIndex)
        {
            // 기존 DungeonUI 찾아서 업데이트
            if (_dungeonPanel != null)
            {
                DungeonUI dungeonUI = _dungeonPanel.GetComponent<DungeonUI>();
                if (dungeonUI != null)
                {
                    dungeonUI.UpdateDungeonMap(nodes, currentIndex);
                }
            }
        }

        /// <summary>
        /// 플레이어 상태 업데이트
        /// </summary>
        private void UpdatePlayerStatus(int currentHealth, int maxHealth)
        {
            if (_dungeonPanel != null)
            {
                DungeonUI dungeonUI = _dungeonPanel.GetComponent<DungeonUI>();
                if (dungeonUI != null)
                {
                    dungeonUI.UpdatePlayerStatus(currentHealth, maxHealth);
                }
            }
        }

        #region Input Handlers - 사용자 입력을 이벤트로 변환
        /// <summary>
        /// 노드 선택 입력 처리
        /// </summary>
        public void OnNodeSelected(int nodeIndex)
        {
            DungeonEvents.RequestNodeMove(nodeIndex);
        }

        /// <summary>
        /// 새 게임 시작 입력 처리
        /// </summary>
        public void OnNewGameRequested()
        {
            GameStateMachine.Instance.StartNewGame();
        }

        /// <summary>
        /// 던전 진입 입력 처리
        /// </summary>
        public void OnDungeonEnterRequested()
        {
            DungeonEvents.RequestDungeonGeneration(0);
        }

        /// <summary>
        /// 메인 메뉴 복귀 입력 처리
        /// </summary>
        public void OnMainMenuRequested()
        {
            GameStateMachine.Instance.ReturnToMainMenu();
        }

        /// <summary>
        /// 방 활동 완료 입력 처리
        /// </summary>
        public void OnRoomActivityCompleted()
        {
            DungeonEvents.NotifyRoomActivityCompleted();
        }
        #endregion
    }
}
