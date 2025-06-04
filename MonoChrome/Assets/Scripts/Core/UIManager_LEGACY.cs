using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 개선된 UI 매니저 - 안전한 이벤트 관리와 초기화
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region UI Panel References
        [Header("UI Panels")]
        private GameObject _mainMenuPanel;
        private GameObject _characterSelectionPanel;
        private GameObject _dungeonPanel;
        private GameObject _combatPanel;
        private GameObject _gameOverPanel;
        private GameObject _victoryPanel;
        private GameObject _optionsPanel;

        private GameObject _currentPanel;
        
        private CombatUI _combatUI;
        private DungeonUI _dungeonUI;
        
        // 초기화 상태 추적
        private bool _isInitialized = false;
        private bool _isSubscribedToEvents = false;
        #endregion

        #region Initialization
        private void Awake()
        {
            Debug.Log("UIManager: Awake called");
        }
        
        private void Start()
        {
            Debug.Log("UIManager: Start called");
            StartCoroutine(DelayedInitialization());
        }
        
        /// <summary>
        /// 지연된 초기화 - GameManager가 준비될 때까지 대기
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // GameManager가 준비될 때까지 대기
            while (GameManager.Instance == null)
            {
                yield return null;
            }
            
            // 추가 프레임 대기 (안전성 확보)
            yield return null;
            
            InitializeUIReferences();
            SubscribeToEvents();
            
            _isInitialized = true;
            Debug.Log("UIManager: Delayed initialization completed");
        }

        private void InitializeUIReferences()
        {
            Debug.Log("UIManager: Initializing UI references");
            
            // Canvas 찾기 또는 생성
            Canvas canvas = FindCanvas();
            if (canvas == null)
            {
                Debug.LogError("UIManager: No Canvas found!");
                return;
            }
            
            Transform canvasTransform = canvas.transform;
            Debug.Log($"UIManager: Canvas found: {canvas.name} with {canvasTransform.childCount} children");
            
            // 패널 참조 찾기
            FindPanelReferences(canvasTransform);
            
            // UI 컴포넌트 초기화
            InitializeUIComponents();
            
            // 초기 패널 상태 설정
            SetInitialPanelState();
            
            Debug.Log("UIManager: UI references initialized successfully");
        }
        
        private Canvas FindCanvas()
        {
            // 씬에서 Canvas 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            
            if (canvas == null)
            {
                Debug.LogWarning("UIManager: Creating new Canvas");
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            
            return canvas;
        }
        
        private void FindPanelReferences(Transform canvasTransform)
        {
            _mainMenuPanel = canvasTransform.Find("MainMenuPanel")?.gameObject;
            _characterSelectionPanel = canvasTransform.Find("CharacterSelectionPanel")?.gameObject;
            _dungeonPanel = canvasTransform.Find("DungeonPanel")?.gameObject;
            _combatPanel = canvasTransform.Find("CombatPanel")?.gameObject;
            _gameOverPanel = canvasTransform.Find("GameOverPanel")?.gameObject;
            _victoryPanel = canvasTransform.Find("VictoryPanel")?.gameObject;
            _optionsPanel = canvasTransform.Find("OptionsPanel")?.gameObject;
            
            LogPanelReferences();
        }
        
        private void LogPanelReferences()
        {
            Debug.Log($"UIManager: Panel references - " +
                     $"MainMenu: {(_mainMenuPanel != null ? "Found" : "Not Found")}, " +
                     $"CharacterSelection: {(_characterSelectionPanel != null ? "Found" : "Not Found")}, " +
                     $"Dungeon: {(_dungeonPanel != null ? "Found" : "Not Found")}, " +
                     $"Combat: {(_combatPanel != null ? "Found" : "Not Found")}, " +
                     $"GameOver: {(_gameOverPanel != null ? "Found" : "Not Found")}, " +
                     $"Victory: {(_victoryPanel != null ? "Found" : "Not Found")}, " +
                     $"Options: {(_optionsPanel != null ? "Found" : "Not Found")}");
        }
        
        private void InitializeUIComponents()
        {
            // DungeonUI 초기화
            if (_dungeonPanel != null)
            {
                _dungeonUI = _dungeonPanel.GetComponent<DungeonUI>();
                if (_dungeonUI == null)
                {
                    _dungeonUI = _dungeonPanel.AddComponent<DungeonUI>();
                    Debug.Log("UIManager: Added DungeonUI component");
                }
            }
            
            // CombatUI 초기화
            if (_combatPanel != null)
            {
                _combatUI = _combatPanel.GetComponent<CombatUI>();
                if (_combatUI == null)
                {
                    _combatUI = _combatPanel.AddComponent<CombatUI>();
                    Debug.Log("UIManager: Added CombatUI component");
                }
            }
        }
        
        private void SetInitialPanelState()
        {
            // 모든 패널 비활성화
            DeactivateAllPanels();
            
            // GameManager 상태에 따른 패널 활성화
            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void SubscribeToEvents()
        {
            if (_isSubscribedToEvents || GameManager.Instance == null)
                return;
                
            // GameManager 이벤트 구독
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            _isSubscribedToEvents = true;
            
            Debug.Log("UIManager: Subscribed to GameManager events");
        }
        
        private void UnsubscribeFromEvents()
        {
            if (!_isSubscribedToEvents || GameManager.Instance == null)
                return;
                
            // GameManager 이벤트 구독 해제
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            _isSubscribedToEvents = false;
            
            Debug.Log("UIManager: Unsubscribed from GameManager events");
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        #endregion

        #region UI Panel Management
        public void OnPanelSwitched(string panelName)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"UIManager: Cannot switch to panel {panelName} - not initialized yet");
                return;
            }
            
            Debug.Log($"UIManager: Switching to panel {panelName}");
            
            DeactivateAllPanels();
            
            GameObject panelToActivate = GetPanelByName(panelName);
            
            if (panelToActivate != null)
            {
                panelToActivate.SetActive(true);
                _currentPanel = panelToActivate;
                
                // 패널별 초기화 로직
                OnPanelActivated(panelName);
                
                Debug.Log($"UIManager: Successfully activated panel: {panelName}");
            }
            else
            {
                Debug.LogError($"UIManager: Panel not found: {panelName}");
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
                "OptionsPanel" => _optionsPanel,
                "EventPanel" => _dungeonPanel?.transform.Find("EventPanel")?.gameObject,
                "ShopPanel" => _dungeonPanel?.transform.Find("ShopPanel")?.gameObject,
                "RestPanel" => _dungeonPanel?.transform.Find("RestPanel")?.gameObject,
                _ => null
            };
        }
        
        private void OnPanelActivated(string panelName)
        {
            switch (panelName)
            {
                case "CharacterSelectionPanel":
                    InitializeCharacterSelection();
                    break;
                    
                case "DungeonPanel":
                    UpdateDungeonUI();
                    break;
                    
                case "CombatPanel":
                    InitializeCombatUI();
                    break;
            }
        }
        
        private void DeactivateAllPanels()
        {
            var panels = new[]
            {
                _mainMenuPanel, _characterSelectionPanel, _dungeonPanel,
                _combatPanel, _gameOverPanel, _victoryPanel, _optionsPanel
            };
            
            foreach (var panel in panels)
            {
                if (panel != null && panel.activeInHierarchy)
                {
                    panel.SetActive(false);
                }
            }
            
            // 던전 내부 패널들 비활성화
            DeactivateDungeonSubPanels();
        }
        
        private void DeactivateDungeonSubPanels()
        {
            if (_dungeonPanel == null) return;
            
            var subPanelNames = new[] { "EventPanel", "ShopPanel", "RestPanel" };
            
            foreach (string subPanelName in subPanelNames)
            {
                Transform subPanel = _dungeonPanel.transform.Find(subPanelName);
                if (subPanel != null && subPanel.gameObject.activeInHierarchy)
                {
                    subPanel.gameObject.SetActive(false);
                }
            }
        }
        
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning($"UIManager: Game state changed to {newState} but UIManager not initialized yet");
                return;
            }
            
            Debug.Log($"UIManager: Handling game state change to {newState}");
            
            string panelName = GetPanelNameForState(newState);
            if (!string.IsNullOrEmpty(panelName))
            {
                OnPanelSwitched(panelName);
            }
        }
        
        private string GetPanelNameForState(GameManager.GameState state)
        {
            return state switch
            {
                GameManager.GameState.MainMenu => "MainMenuPanel",
                GameManager.GameState.CharacterSelection => "CharacterSelectionPanel",
                GameManager.GameState.Dungeon => "DungeonPanel",
                GameManager.GameState.Combat => "CombatPanel",
                GameManager.GameState.Event => "EventPanel",
                GameManager.GameState.Shop => "ShopPanel",
                GameManager.GameState.Rest => "RestPanel",
                GameManager.GameState.GameOver => "GameOverPanel",
                GameManager.GameState.Victory => "VictoryPanel",
                GameManager.GameState.Paused => "OptionsPanel",
                _ => null
            };
        }
        #endregion

        #region UI Component Initialization
        private void UpdateDungeonUI()
        {
            Debug.Log("UIManager: Updating Dungeon UI");
            
            if (_dungeonUI == null && _dungeonPanel != null)
            {
                _dungeonUI = _dungeonPanel.GetComponent<DungeonUI>();
                if (_dungeonUI == null)
                {
                    _dungeonUI = _dungeonPanel.AddComponent<DungeonUI>();
                }
            }
        }
        
        private void InitializeCharacterSelection()
        {
            Debug.Log("UIManager: Initializing Character Selection UI");
        }
        
        public void InitializeCombatUI()
        {
            if (_combatUI != null)
            {
                _combatUI.InitializeCombatUI();
                Debug.Log("UIManager: Combat UI initialized");
            }
            else
            {
                Debug.LogError("UIManager: Cannot initialize combat UI - CombatUI reference is missing");
            }
        }
        #endregion

        #region Combat UI Methods
        public void UpdateCoinUI(List<bool> coinResults)
        {
            _combatUI?.UpdateCoinUI(coinResults);
        }
        
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            _combatUI?.UpdatePatternUI(availablePatterns);
        }
        
        public void ShowEnemyIntention(Pattern pattern)
        {
            _combatUI?.ShowEnemyIntention(pattern);
        }
        
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            _combatUI?.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);
        }

        public void UpdateTurnCounter(int turnCount)
        {
            _combatUI?.UpdateTurnCounter(turnCount);
        }

        public void UpdateActiveSkillButton(bool isAvailable)
        {
            _combatUI?.UpdateActiveSkillButton(isAvailable);
        }
        
        public void UpdateStatusEffects(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            _combatUI?.UpdateStatusEffectsUI(playerEffects, enemyEffects);
        }
        #endregion

        #region Game Flow UI Methods
        public void ShowGameOverScreen()
        {
            Debug.Log("UIManager: Showing Game Over screen");
            OnPanelSwitched("GameOverPanel");
        }

        public void ShowVictoryScreen()
        {
            Debug.Log("UIManager: Showing Victory screen");
            OnPanelSwitched("VictoryPanel");
        }

        public void UpdateDungeonMap(List<DungeonNode> nodes, int currentNodeIndex)
        {
            Debug.Log($"UIManager: Updating dungeon map with {nodes.Count} nodes, current position: {currentNodeIndex}");
            
            if (_dungeonUI != null)
            {
                _dungeonUI.UpdateDungeonMap(nodes, currentNodeIndex);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update dungeon map - DungeonUI reference is missing");
            }
        }
        #endregion
    }
}