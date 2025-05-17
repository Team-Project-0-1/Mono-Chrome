using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 게임의 모든 UI 관련 기능을 관리하는 매니저 클래스
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

        // 현재 활성화된 패널
        private GameObject _currentPanel;
        
        // UI 컴포넌트 참조
        private CombatUI _combatUI;
        #endregion

        #region Initialization
        private void Start()
        {
            InitializeUIReferences();
            SubscribeToEvents();
            
            Debug.Log("UIManager: Started initialization");
        }

        private void InitializeUIReferences()
        {
            Debug.Log("UIManager: Initializing UI references");
            
            // Canvas 찾기
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("UIManager: Canvas not found!");
                Debug.Log("UIManager: Attempting to create Canvas");
                
                // Canvas 생성 시도
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
                
                Debug.Log("UIManager: Created new Canvas");
            }
            
            Transform canvasTransform = canvas.transform;
            Debug.Log($"UIManager: Canvas found: {canvas.name}");
            
            // 캐리어스 하위 객체 수 로그
            Debug.Log($"UIManager: Canvas has {canvasTransform.childCount} children");
            for (int i = 0; i < canvasTransform.childCount; i++)
            {
                Debug.Log($"UIManager: Canvas child {i}: {canvasTransform.GetChild(i).name}");
            }
            
            // 패널 참조 찾기
            _mainMenuPanel = canvasTransform.Find("MainMenuPanel")?.gameObject;
            _characterSelectionPanel = canvasTransform.Find("CharacterSelectionPanel")?.gameObject;
            _dungeonPanel = canvasTransform.Find("DungeonPanel")?.gameObject;
            _combatPanel = canvasTransform.Find("CombatPanel")?.gameObject;
            _gameOverPanel = canvasTransform.Find("GameOverPanel")?.gameObject;
            _victoryPanel = canvasTransform.Find("VictoryPanel")?.gameObject;
            _optionsPanel = canvasTransform.Find("OptionsPanel")?.gameObject;
            
            // 패널 참조 확인
            Debug.Log($"UIManager: Panel references - MainMenu: {(_mainMenuPanel != null ? "Found" : "Not Found")}, "
                  + $"CharacterSelection: {(_characterSelectionPanel != null ? "Found" : "Not Found")}, "
                  + $"Dungeon: {(_dungeonPanel != null ? "Found" : "Not Found")}, "
                  + $"Combat: {(_combatPanel != null ? "Found" : "Not Found")}, "
                  + $"GameOver: {(_gameOverPanel != null ? "Found" : "Not Found")}, "
                  + $"Victory: {(_victoryPanel != null ? "Found" : "Not Found")}, "
                  + $"Options: {(_optionsPanel != null ? "Found" : "Not Found")}");
            
            // 패널 비활성화 (GameManager 상태에 따라 인적 패널만 활성화되도록)
            if (_mainMenuPanel != null) _mainMenuPanel.SetActive(false);
            if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(false);
            if (_dungeonPanel != null) _dungeonPanel.SetActive(false);
            if (_combatPanel != null) _combatPanel.SetActive(false);
            if (_gameOverPanel != null) _gameOverPanel.SetActive(false);
            if (_victoryPanel != null) _victoryPanel.SetActive(false);
            if (_optionsPanel != null) _optionsPanel.SetActive(false);
            
            // CombatUI 참조 찾기
            if (_combatPanel != null)
            {
                _combatUI = _combatPanel.GetComponent<CombatUI>();
                if (_combatUI == null)
                {
                    _combatUI = _combatPanel.AddComponent<CombatUI>();
                    Debug.Log("UIManager: Added CombatUI component to CombatPanel");
                }
            }
            
            // 현재 GameManager 상태에 맞는 패널 활성화
            if (GameManager.Instance != null)
            {
                OnGameStateChanged(GameManager.Instance.CurrentState);
            }
            
            Debug.Log("UIManager: UI references initialized");
        }

        private void SubscribeToEvents()
        {
            // GameManager의 상태 변경 이벤트 구독
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            Debug.Log("UIManager: Subscribed to game state changes");
        }
        #endregion

        #region UI Panel Management
        /// <summary>
        /// 패널이 전환될 때 호출되는 메서드
        /// </summary>
        /// <param name="panelName">전환된 패널 이름</param>
        public void OnPanelSwitched(string panelName)
        {
            Debug.Log($"UIManager: Panel switched to {panelName}");
            
            // 패널에 따른 특별한 처리
            switch (panelName)
            {
                case "CombatPanel":
                    // 전투 UI 초기화 작업
                    if (_combatUI != null)
                    {
                        _combatUI.InitializeCombatUI();
                    }
                    break;
                    
                case "DungeonPanel":
                    // 던전 UI 초기화 작업
                    UpdateDungeonUI();
                    break;
                
                case "CharacterSelectionPanel":
                    // 캐릭터 선택 화면 초기화
                    InitializeCharacterSelection();
                    break;
                    
                // 다른 패널에 대한 추가 처리 가능
            }
        }
        
        /// <summary>
        /// 던전 UI 초기화 및 업데이트
        /// </summary>
        private void UpdateDungeonUI()
        {
            Debug.Log("UIManager: Initializing Dungeon UI");
            // 던전 UI 초기화 로직 구현
        }
        
        /// <summary>
        /// 캐릭터 선택 화면 초기화
        /// </summary>
        private void InitializeCharacterSelection()
        {
            Debug.Log("UIManager: Initializing Character Selection UI");
            // 캐릭터 선택 UI 초기화 로직 구현
        }
        
        // 게임 상태 변경에 따른 UI 패널 전환
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            // 현재 패널 비활성화
            if (_currentPanel != null)
            {
                _currentPanel.SetActive(false);
            }

            // 새 상태에 따른 패널 활성화
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    _currentPanel = _mainMenuPanel;
                    break;
                    
                case GameManager.GameState.CharacterSelection:
                    _currentPanel = _characterSelectionPanel;
                    break;
                    
                case GameManager.GameState.Dungeon:
                    _currentPanel = _dungeonPanel;
                    break;
                    
                case GameManager.GameState.Combat:
                    _currentPanel = _combatPanel;
                    break;
                    
                case GameManager.GameState.GameOver:
                    _currentPanel = _gameOverPanel;
                    break;
                    
                case GameManager.GameState.Victory:
                    _currentPanel = _victoryPanel;
                    break;
                    
                case GameManager.GameState.Paused:
                    _optionsPanel.SetActive(true);
                    return; // 일시정지는 이전 패널 위에 추가로 표시
            }

            // 새 패널 활성화
            if (_currentPanel != null)
            {
                _currentPanel.SetActive(true);
                Debug.Log($"UIManager: Activated panel for state {newState}");
            }
            else
            {
                Debug.LogWarning($"UIManager: No panel found for state {newState}");
            }
        }
        #endregion

        #region Combat UI Methods
        /// <summary>
        /// 전투 UI 초기화
        /// </summary>
        public void InitializeCombatUI()
        {
            if (_combatUI != null)
            {
                _combatUI.InitializeCombatUI();
            }
            else
            {
                Debug.LogError("UIManager: Cannot initialize combat UI - CombatUI reference is missing");
            }
        }
        
        // 동전 UI 업데이트
        public void UpdateCoinUI(List<bool> coinResults)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdateCoinUI(coinResults);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update coin UI - CombatUI reference is missing");
            }
        }
        
        // 패턴(족보) UI 업데이트
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdatePatternUI(availablePatterns);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update pattern UI - CombatUI reference is missing");
            }
        }
        
        // 적 의도 표시
        public void ShowEnemyIntention(Pattern pattern)
        {
            if (_combatUI != null)
            {
                _combatUI.ShowEnemyIntention(pattern);
            }
            else
            {
                Debug.LogError("UIManager: Cannot show enemy intention - CombatUI reference is missing");
            }
        }
        
        // 체력바 업데이트
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update health bars - CombatUI reference is missing");
            }
        }

        // 턴 카운터 업데이트
        public void UpdateTurnCounter(int turnCount)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdateTurnCounter(turnCount);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update turn counter - CombatUI reference is missing");
            }
        }

        // 액티브 스킬 버튼 업데이트
        public void UpdateActiveSkillButton(bool isAvailable)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdateActiveSkillButton(isAvailable);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update active skill button - CombatUI reference is missing");
            }
        }
        
        // 상태 효과 업데이트
        public void UpdateStatusEffects(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            if (_combatUI != null)
            {
                _combatUI.UpdateStatusEffectsUI(playerEffects, enemyEffects);
            }
            else
            {
                Debug.LogError("UIManager: Cannot update status effects - CombatUI reference is missing");
            }
        }
        #endregion

        #region Game Flow UI Methods
        // 게임오버 화면 표시
        public void ShowGameOverScreen()
        {
            Debug.Log("UIManager: Showing Game Over screen");
            
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("UIManager: Game over panel not found");
            }
        }

        // 승리 화면 표시
        public void ShowVictoryScreen()
        {
            Debug.Log("UIManager: Showing Victory screen");
            
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
            }
            else
            {
                Debug.LogError("UIManager: Victory panel not found");
            }
        }

        // 던전 맵 업데이트
        public void UpdateDungeonMap(List<MonoChrome.DungeonNode> nodes, int currentNodeIndex)
        {
            Debug.Log($"UIManager: Updating dungeon map with {nodes.Count} nodes, current position: {currentNodeIndex}");
            
            // 던전 UI 참조 찾기
            GameObject dungeonPanel = GameObject.Find("DungeonPanel");
            if (dungeonPanel != null)
            {
                DungeonUI dungeonUI = dungeonPanel.GetComponent<DungeonUI>();
                if (dungeonUI != null)
                {
                    dungeonUI.UpdateDungeonMap(nodes, currentNodeIndex);
                }
                else
                {
                    Debug.LogError("UIManager: DungeonUI component not found on DungeonPanel");
                }
            }
            else
            {
                Debug.LogError("UIManager: DungeonPanel not found");
            }
        }
        #endregion
    }
}
