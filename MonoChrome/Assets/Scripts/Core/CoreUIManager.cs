using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome.Core
{
    /// <summary>
    /// 핵심 UI 매니저 - 단순하고 안정적인 UI 관리
    /// 포트폴리오용: 명확한 UI 상태 관리와 패널 전환
    /// </summary>
    public class CoreUIManager : MonoBehaviour
    {
        #region UI Panel References
        [Header("Main UI Panels")]
        [SerializeField] private GameObject _characterSelectionPanel;
        [SerializeField] private GameObject _dungeonPanel;
        [SerializeField] private GameObject _combatPanel;
        [SerializeField] private GameObject _eventPanel;
        [SerializeField] private GameObject _shopPanel;
        [SerializeField] private GameObject _restPanel;
        [SerializeField] private GameObject _gameOverPanel;
        [SerializeField] private GameObject _victoryPanel;
        
        [Header("Character Selection UI")]
        [SerializeField] private Button[] _characterButtons;
        [SerializeField] private string[] _characterNames = { "김훈희", "신제우", "곽장환", "박재석" };
        
        [Header("Dungeon UI")]
        [SerializeField] private Button[] _roomChoiceButtons;
        [SerializeField] private TextMeshProUGUI[] _roomDescriptions;
        [SerializeField] private Image[] _roomIcons;
        
        [Header("Status UI")]
        [SerializeField] private TextMeshProUGUI _stageInfoText;
        [SerializeField] private TextMeshProUGUI _characterInfoText;
        
        private Canvas _mainCanvas;
        private bool _isInitialized = false;
        #endregion
        
        #region Initialization
        private void Awake()
        {
            Debug.Log("CoreUIManager: Awake");
            _mainCanvas = GetComponentInParent<Canvas>();
            if (_mainCanvas == null)
                _mainCanvas = FindObjectOfType<Canvas>();
        }
        
        private void Start()
        {
            Debug.Log("CoreUIManager: Start");
            StartCoroutine(DelayedInitialization());
        }
        
        /// <summary>
        /// 지연된 초기화 - 다른 매니저들이 준비될 때까지 대기
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // MasterGameManager가 준비될 때까지 대기
            while (MasterGameManager.Instance == null)
            {
                yield return null;
            }
            
            yield return null; // 추가 프레임 대기
            
            // UI 초기화
            InitializeUI();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            _isInitialized = true;
            Debug.Log("CoreUIManager: Initialization completed");
            
            // 초기 상태에 따른 UI 표시
            ShowUIForCurrentState();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // UI 참조 자동 찾기
            FindUIReferences();
            
            // 캐릭터 선택 버튼 설정
            SetupCharacterSelectionButtons();
            
            // 던전 UI 버튼 설정
            SetupDungeonButtons();
            
            // 모든 패널 비활성화
            HideAllPanels();
        }
        
        /// <summary>
        /// UI 참조 자동 찾기
        /// </summary>
        private void FindUIReferences()
        {
            if (_mainCanvas == null) return;
            
            Transform canvasTransform = _mainCanvas.transform;
            
            // 패널 참조 찾기
            _characterSelectionPanel = FindChildByName(canvasTransform, "CharacterSelectionPanel");
            _dungeonPanel = FindChildByName(canvasTransform, "DungeonPanel");
            _combatPanel = FindChildByName(canvasTransform, "CombatPanel");
            _eventPanel = FindChildByName(canvasTransform, "EventPanel");
            _shopPanel = FindChildByName(canvasTransform, "ShopPanel");
            _restPanel = FindChildByName(canvasTransform, "RestPanel");
            _gameOverPanel = FindChildByName(canvasTransform, "GameOverPanel");
            _victoryPanel = FindChildByName(canvasTransform, "VictoryPanel");
            
            // 상태 텍스트 찾기
            _stageInfoText = FindComponentInChildren<TextMeshProUGUI>(canvasTransform, "StageInfoText");
            _characterInfoText = FindComponentInChildren<TextMeshProUGUI>(canvasTransform, "CharacterInfoText");
            
            LogFoundReferences();
        }
        
        /// <summary>
        /// 자식 오브젝트를 이름으로 찾기
        /// </summary>
        private GameObject FindChildByName(Transform parent, string name)
        {
            Transform found = parent.Find(name);
            if (found == null)
            {
                // 재귀적으로 찾기
                found = FindInChildren(parent, name);
            }
            
            return found?.gameObject;
        }
        
        /// <summary>
        /// 재귀적으로 자식에서 찾기
        /// </summary>
        private Transform FindInChildren(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;
                    
                Transform found = FindInChildren(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }
        
        /// <summary>
        /// 컴포넌트를 이름으로 찾기
        /// </summary>
        private T FindComponentInChildren<T>(Transform parent, string name) where T : Component
        {
            GameObject obj = FindChildByName(parent, name);
            return obj?.GetComponent<T>();
        }
        
        /// <summary>
        /// 찾은 참조들 로그 출력
        /// </summary>
        private void LogFoundReferences()
        {
            Debug.Log($"CoreUIManager: Found UI References:");
            Debug.Log($"  CharacterSelection: {(_characterSelectionPanel != null ? "✓" : "✗")}");
            Debug.Log($"  Dungeon: {(_dungeonPanel != null ? "✓" : "✗")}");
            Debug.Log($"  Combat: {(_combatPanel != null ? "✓" : "✗")}");
            Debug.Log($"  Event: {(_eventPanel != null ? "✓" : "✗")}");
            Debug.Log($"  Shop: {(_shopPanel != null ? "✓" : "✗")}");
            Debug.Log($"  Rest: {(_restPanel != null ? "✓" : "✗")}");
        }
        #endregion
        
        #region Event Management
        /// <summary>
        /// 이벤트 구독 - GameStateMachine을 통해 상태 변경 감지
        /// </summary>
        private void SubscribeToEvents()
        {
            // GameStateMachine의 이벤트 구독
            if (GameStateMachine.Instance != null)
            {
                GameStateMachine.OnStateChanged += OnGameStateChanged;
            }
            else
            {
                Debug.LogWarning("CoreUIManager: GameStateMachine not found");
            }
        }
        
        /// <summary>
        /// 이벤트 구독 해제
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            // GameStateMachine 이벤트 구독 해제
            GameStateMachine.OnStateChanged -= OnGameStateChanged;
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// 게임 상태 변경 이벤트 처리
        /// </summary>
        private void OnGameStateChanged(GameStateMachine.GameState previousState, GameStateMachine.GameState newState)
        {
            Debug.Log($"CoreUIManager: Game state changed from {previousState} to {newState}");
            
            if (!_isInitialized)
            {
                Debug.LogWarning("CoreUIManager: State changed but not initialized yet");
                return;
            }
            
            ShowUIForState(newState);
        }
        #endregion
        
        #region Panel Management
        /// <summary>
        /// 모든 패널 숨기기
        /// </summary>
        private void HideAllPanels()
        {
            var panels = new GameObject[]
            {
                _characterSelectionPanel, _dungeonPanel, _combatPanel,
                _eventPanel, _shopPanel, _restPanel, _gameOverPanel, _victoryPanel
            };
            
            foreach (var panel in panels)
            {
                if (panel != null)
                    panel.SetActive(false);
            }
        }
        
        /// <summary>
        /// 현재 상태에 맞는 UI 표시
        /// </summary>
        private void ShowUIForCurrentState()
        {
            if (MasterGameManager.Instance != null)
            {
                ShowUIForState(MasterGameManager.Instance.CurrentState);
            }
        }
        
        /// <summary>
        /// 특정 상태에 맞는 UI 표시
        /// </summary>
        private void ShowUIForState(GameStateMachine.GameState state)
        {
            HideAllPanels();
            
            switch (state)
            {
                case GameStateMachine.GameState.CharacterSelection:
                    ShowCharacterSelectionPanel();
                    break;
                    
                case GameStateMachine.GameState.Dungeon:
                    ShowDungeonPanel();
                    break;
                    
                case GameStateMachine.GameState.Combat:
                    ShowCombatPanel();
                    break;
                    
                case GameStateMachine.GameState.Event:
                    ShowEventPanel();
                    break;
                    
                case GameStateMachine.GameState.Shop:
                    ShowShopPanel();
                    break;
                    
                case GameStateMachine.GameState.Rest:
                    ShowRestPanel();
                    break;
                    
                case GameStateMachine.GameState.GameOver:
                    ShowGameOverPanel();
                    break;
                    
                case GameStateMachine.GameState.Victory:
                    ShowVictoryPanel();
                    break;
            }
            
            // 상태 정보 업데이트
            UpdateStatusUI();
        }
        
        /// <summary>
        /// 캐릭터 선택 패널 표시
        /// </summary>
        private void ShowCharacterSelectionPanel()
        {
            if (_characterSelectionPanel != null)
            {
                _characterSelectionPanel.SetActive(true);
                Debug.Log("CoreUIManager: Character selection panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Character selection panel is null!");
            }
        }
        
        /// <summary>
        /// 던전 패널 표시
        /// </summary>
        private void ShowDungeonPanel()
        {
            if (_dungeonPanel != null)
            {
                _dungeonPanel.SetActive(true);
                Debug.Log("CoreUIManager: Dungeon panel shown");
                
                // 던전 UI 업데이트
                UpdateDungeonUI();
            }
            else
            {
                Debug.LogError("CoreUIManager: Dungeon panel is null!");
            }
        }
        
        /// <summary>
        /// 전투 패널 표시
        /// </summary>
        private void ShowCombatPanel()
        {
            if (_combatPanel != null)
            {
                _combatPanel.SetActive(true);
                Debug.Log("CoreUIManager: Combat panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Combat panel is null!");
            }
        }
        
        /// <summary>
        /// 이벤트 패널 표시
        /// </summary>
        private void ShowEventPanel()
        {
            if (_eventPanel != null)
            {
                _eventPanel.SetActive(true);
                Debug.Log("CoreUIManager: Event panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Event panel is null!");
            }
        }
        
        /// <summary>
        /// 상점 패널 표시
        /// </summary>
        private void ShowShopPanel()
        {
            if (_shopPanel != null)
            {
                _shopPanel.SetActive(true);
                Debug.Log("CoreUIManager: Shop panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Shop panel is null!");
            }
        }
        
        /// <summary>
        /// 휴식 패널 표시
        /// </summary>
        private void ShowRestPanel()
        {
            if (_restPanel != null)
            {
                _restPanel.SetActive(true);
                Debug.Log("CoreUIManager: Rest panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Rest panel is null!");
            }
        }
        
        /// <summary>
        /// 게임 오버 패널 표시
        /// </summary>
        private void ShowGameOverPanel()
        {
            if (_gameOverPanel != null)
            {
                _gameOverPanel.SetActive(true);
                Debug.Log("CoreUIManager: Game over panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Game over panel is null!");
            }
        }
        
        /// <summary>
        /// 승리 패널 표시
        /// </summary>
        private void ShowVictoryPanel()
        {
            if (_victoryPanel != null)
            {
                _victoryPanel.SetActive(true);
                Debug.Log("CoreUIManager: Victory panel shown");
            }
            else
            {
                Debug.LogError("CoreUIManager: Victory panel is null!");
            }
        }
        #endregion
        
        #region Character Selection
        /// <summary>
        /// 캐릭터 선택 버튼 설정
        /// </summary>
        private void SetupCharacterSelectionButtons()
        {
            if (_characterSelectionPanel == null) return;
            
            // 캐릭터 버튼 찾기
            if (_characterButtons == null || _characterButtons.Length == 0)
            {
                List<Button> buttons = new List<Button>();
                
                // 표준 버튼 이름들로 찾기
                string[] buttonNames = { "AuditoryCharButton", "OlfactoryCharButton", "TactileCharButton", "SpiritualCharButton" };
                
                for (int i = 0; i < buttonNames.Length; i++)
                {
                    Button button = FindComponentInChildren<Button>(_characterSelectionPanel.transform, buttonNames[i]);
                    if (button != null)
                        buttons.Add(button);
                }
                
                _characterButtons = buttons.ToArray();
            }
            
            // 버튼 이벤트 설정
            for (int i = 0; i < _characterButtons.Length && i < _characterNames.Length; i++)
            {
                if (_characterButtons[i] != null)
                {
                    int characterIndex = i; // 클로저용 변수
                    _characterButtons[i].onClick.RemoveAllListeners();
                    _characterButtons[i].onClick.AddListener(() => OnCharacterSelected(characterIndex));
                    
                    Debug.Log($"CoreUIManager: Character button {i} ({_characterNames[i]}) configured");
                }
            }
        }
        
        /// <summary>
        /// 캐릭터 선택 처리
        /// </summary>
        private void OnCharacterSelected(int characterIndex)
        {
            if (characterIndex >= 0 && characterIndex < _characterNames.Length)
            {
                string characterName = _characterNames[characterIndex];
                Debug.Log($"CoreUIManager: Character selected - {characterName}");
                
                if (MasterGameManager.Instance != null)
                {
                    MasterGameManager.Instance.SelectCharacter(characterName);
                }
            }
        }
        #endregion
        
        #region Dungeon UI
        /// <summary>
        /// 던전 버튼 설정
        /// </summary>
        private void SetupDungeonButtons()
        {
            if (_dungeonPanel == null) return;
            
            // 방 선택 버튼 찾기
            if (_roomChoiceButtons == null || _roomChoiceButtons.Length == 0)
            {
                List<Button> buttons = new List<Button>();
                
                for (int i = 1; i <= 3; i++) // 일반적으로 3개의 선택지
                {
                    Button button = FindComponentInChildren<Button>(_dungeonPanel.transform, $"RoomButton{i}");
                    if (button != null)
                        buttons.Add(button);
                }
                
                _roomChoiceButtons = buttons.ToArray();
            }
            
            // 방 설명 텍스트 찾기
            if (_roomDescriptions == null || _roomDescriptions.Length == 0)
            {
                List<TextMeshProUGUI> descriptions = new List<TextMeshProUGUI>();
                
                for (int i = 0; i < _roomChoiceButtons.Length; i++)
                {
                    if (_roomChoiceButtons[i] != null)
                    {
                        TextMeshProUGUI desc = _roomChoiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                        if (desc != null)
                            descriptions.Add(desc);
                    }
                }
                
                _roomDescriptions = descriptions.ToArray();
            }
            
            // 버튼 이벤트 설정
            for (int i = 0; i < _roomChoiceButtons.Length; i++)
            {
                if (_roomChoiceButtons[i] != null)
                {
                    int roomIndex = i; // 클로저용 변수
                    _roomChoiceButtons[i].onClick.RemoveAllListeners();
                    _roomChoiceButtons[i].onClick.AddListener(() => OnRoomSelected(roomIndex));
                    
                    Debug.Log($"CoreUIManager: Room button {i} configured");
                }
            }
        }
        
        /// <summary>
        /// 방 선택 처리
        /// </summary>
        private void OnRoomSelected(int roomIndex)
        {
            Debug.Log($"CoreUIManager: Room {roomIndex} selected");
            
            // MasterGameManager를 통해 전투 시작
            if (MasterGameManager.Instance != null)
            {
                MasterGameManager.Instance.StartCombat();
            }
        }
        
        /// <summary>
        /// 던전 UI 업데이트 (Public API)
        /// </summary>
        public void UpdateDungeonUI()
        {
            // 방 선택지 설정 (테스트용)
            var roomTypes = new string[] { "전투", "이벤트", "상점" };
            var roomDescs = new string[] 
            { 
                "적과 마주쳐 전투를 벌입니다.",
                "특별한 이벤트가 발생합니다.",
                "아이템을 구매할 수 있습니다."
            };
            
            for (int i = 0; i < _roomChoiceButtons.Length && i < roomTypes.Length; i++)
            {
                if (_roomChoiceButtons[i] != null)
                {
                    _roomChoiceButtons[i].gameObject.SetActive(true);
                    
                    if (i < _roomDescriptions.Length && _roomDescriptions[i] != null)
                    {
                        _roomDescriptions[i].text = $"{roomTypes[i]}: {roomDescs[i]}";
                    }
                }
            }
        }
        #endregion
        
        #region Status UI
        /// <summary>
        /// 상태 UI 업데이트
        /// </summary>
        private void UpdateStatusUI()
        {
            if (MasterGameManager.Instance == null) return;
            
            var gameManager = MasterGameManager.Instance;
            
            if (_stageInfoText != null)
            {
                _stageInfoText.text = $"스테이지 {gameManager.CurrentStage + 1}";
            }
            
            if (_characterInfoText != null && !string.IsNullOrEmpty(gameManager.SelectedCharacterName))
            {
                _characterInfoText.text = $"캐릭터: {gameManager.SelectedCharacterName}";
            }
        }
        #endregion
        
        #region Public Interface
        /// <summary>
        /// 메인 메뉴로 돌아가기
        /// </summary>
        public void ReturnToMainMenu()
        {
            Debug.Log("CoreUIManager: Return to main menu requested");
            
            if (MasterGameManager.Instance != null)
            {
                MasterGameManager.Instance.ReturnToMainMenu();
            }
        }
        
        /// <summary>
        /// 던전으로 돌아가기
        /// </summary>
        public void ReturnToDungeon()
        {
            Debug.Log("CoreUIManager: Return to dungeon requested");
            
            if (MasterGameManager.Instance != null)
            {
                MasterGameManager.Instance.EnterDungeon();
            }
        }
        #endregion
    }
}