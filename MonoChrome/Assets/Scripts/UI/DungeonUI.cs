using System.Collections;
using System.Collections.Generic;
using MonoChrome.Dungeon;
using MonoChrome.StatusEffects;
using MonoChrome.UI;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome
{
    /// <summary>
    /// 던전 UI를 관리하는 클래스
    /// </summary>
    public class DungeonUI : MonoBehaviour
    {
        [Header("Room UI References")]
        [SerializeField] private GameObject roomSelectionPanel;
        [SerializeField] private Button[] roomButtons;
        [SerializeField] private Image[] roomTypeIcons;
        [SerializeField] private TextMeshProUGUI[] roomDescriptions;
        
        [Header("Event, Shop, and Rest Panels")]
        [SerializeField] private GameObject eventPanel;
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private GameObject restPanel;
        
        [Header("Mini Map")]
        [SerializeField] private GameObject miniMapPanel;
        [SerializeField] private Transform nodesContainer;
        [SerializeField] private GameObject nodeButtonPrefab;
        
        [Header("노드 배치 시스템")]
        [SerializeField] private NodePositionManager nodePositionManager;
        [SerializeField] private bool useAdvancedPositioning = true;
        
        [Header("Player Status")]
        [SerializeField] private TextMeshProUGUI playerHealthText;
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private TextMeshProUGUI playerStageText;
        
        [Header("Status Effects")]
        [SerializeField] private Transform statusEffectsContainer;
        [SerializeField] private GameObject statusEffectPrefab;
        
        [Header("Icons")]
        [SerializeField] private Sprite combatIcon;
        [SerializeField] private Sprite shopIcon;
        [SerializeField] private Sprite restIcon;
        [SerializeField] private Sprite eventIcon;
        [SerializeField] private Sprite miniBossIcon;
        [SerializeField] private Sprite bossIcon;
        [SerializeField] private Sprite defaultIcon;
        
        private DungeonManager dungeonManager;
        
        private void Awake()
        {
            // Find and cache reference to DungeonManager
            dungeonManager = FindObjectOfType<DungeonManager>();
            
            if (dungeonManager == null)
            {
                Debug.LogError("DungeonManager not found in the scene!");
            }
            
            // Ensure UI panels are properly referenced
            ValidateReferences();
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        private void ValidateReferences()
        {
            // Check for missing references and log errors
            if (roomSelectionPanel == null)
            {
                roomSelectionPanel = transform.Find("RoomSelectionPanel")?.gameObject;
                if (roomSelectionPanel == null)
                    Debug.LogError("DungeonUI: roomSelectionPanel reference is missing");
            }
            
            // Check for Event, Shop, and Rest panels
            if (eventPanel == null)
            {
                eventPanel = transform.Find("EventPanel")?.gameObject;
                if (eventPanel == null)
                    Debug.LogError("DungeonUI: eventPanel reference is missing");
            }
            
            if (shopPanel == null)
            {
                shopPanel = transform.Find("ShopPanel")?.gameObject;
                if (shopPanel == null)
                    Debug.LogError("DungeonUI: shopPanel reference is missing");
            }
            
            if (restPanel == null)
            {
                restPanel = transform.Find("RestPanel")?.gameObject;
                if (restPanel == null)
                    Debug.LogError("DungeonUI: restPanel reference is missing");
            }
            
            if (miniMapPanel == null)
            {
                miniMapPanel = transform.Find("MapPanel")?.gameObject;
                if (miniMapPanel == null)
                    Debug.LogError("DungeonUI: miniMapPanel reference is missing");
            }
                
            if (nodesContainer == null && miniMapPanel != null)
            {
                nodesContainer = miniMapPanel.transform;
                Debug.Log("DungeonUI: Using MapPanel as nodesContainer");
            }
                
            if (nodeButtonPrefab == null)
                Debug.LogWarning("DungeonUI: nodeButtonPrefab reference is missing");
                
            if (playerHealthText == null)
            {
                // 상태창에서 PlayerHealthText 찾기
                Transform statusPanel = transform.Find("StatusPanel");
                if (statusPanel != null)
                {
                    Transform playerStatusBar = statusPanel.Find("PlayerStatusBar");
                    if (playerStatusBar != null)
                    {
                        playerHealthText = playerStatusBar.Find("HealthText")?.GetComponent<TextMeshProUGUI>();
                    }
                }
                
                if (playerHealthText == null)
                    Debug.LogError("DungeonUI: playerHealthText reference is missing");
            }
                
            if (playerHealthSlider == null)
            {
                // 상태창에서 HealthSlider 찾기
                Transform statusPanel = transform.Find("StatusPanel");
                if (statusPanel != null)
                {
                    Transform playerStatusBar = statusPanel.Find("PlayerStatusBar");
                    if (playerStatusBar != null)
                    {
                        playerHealthSlider = playerStatusBar.Find("HealthSlider")?.GetComponent<Slider>();
                    }
                }
                
                if (playerHealthSlider == null)
                    Debug.LogError("DungeonUI: playerHealthSlider reference is missing");
            }
                
            if (playerStageText == null)
            {
                playerStageText = transform.Find("StageInfoText")?.GetComponent<TextMeshProUGUI>();
                if (playerStageText == null)
                    Debug.LogError("DungeonUI: playerStageText reference is missing");
            }
                
            if (statusEffectsContainer == null)
            {
                // 상태창에서 StatusEffectsContainer 찾기
                Transform statusPanel = transform.Find("StatusPanel");
                if (statusPanel != null)
                {
                    statusEffectsContainer = statusPanel.Find("StatusEffectsContainer");
                }
                
                if (statusEffectsContainer == null)
                    Debug.LogError("DungeonUI: statusEffectsContainer reference is missing");
            }
                
            if (statusEffectPrefab == null)
                Debug.LogWarning("DungeonUI: statusEffectPrefab reference is missing");
                
            // Check for array references
            if (roomButtons == null || roomButtons.Length == 0)
            {
                // 룸 선택 버튼 배열 초기화
                if (roomSelectionPanel != null)
                {
                    List<Button> buttons = new List<Button>();
                    Button roomButton1 = roomSelectionPanel.transform.Find("RoomButton1")?.GetComponent<Button>();
                    if (roomButton1 != null) buttons.Add(roomButton1);
                    
                    Button roomButton2 = roomSelectionPanel.transform.Find("RoomButton2")?.GetComponent<Button>();
                    if (roomButton2 != null) buttons.Add(roomButton2);
                    
                    Button roomButton3 = roomSelectionPanel.transform.Find("RoomButton3")?.GetComponent<Button>();
                    if (roomButton3 != null) buttons.Add(roomButton3);
                    
                    if (buttons.Count > 0)
                    {
                        roomButtons = buttons.ToArray();
                        Debug.Log($"DungeonUI: Found {roomButtons.Length} room buttons");
                    }
                    else
                    {
                        Debug.LogError("DungeonUI: No room buttons found in RoomSelectionPanel");
                    }
                }
                else
                {
                    Debug.LogError("DungeonUI: Cannot find room buttons - RoomSelectionPanel is missing");
                }
            }
                
            if (roomTypeIcons == null || roomTypeIcons.Length == 0)
            {
                // 룸 타입 아이콘 배열 초기화
                if (roomButtons != null && roomButtons.Length > 0)
                {
                    roomTypeIcons = new Image[roomButtons.Length];
                    for (int i = 0; i < roomButtons.Length; i++)
                    {
                        roomTypeIcons[i] = roomButtons[i].GetComponent<Image>();
                    }
                    Debug.Log($"DungeonUI: Initialized {roomTypeIcons.Length} room type icons");
                }
                else
                {
                    Debug.LogError("DungeonUI: Cannot initialize room type icons - roomButtons array is empty or null");
                }
            }
                
            if (roomDescriptions == null || roomDescriptions.Length == 0)
            {
                // 룸 설명 텍스트 배열 초기화
                if (roomButtons != null && roomButtons.Length > 0)
                {
                    List<TextMeshProUGUI> descriptions = new List<TextMeshProUGUI>();
                    for (int i = 0; i < roomButtons.Length; i++)
                    {
                        TextMeshProUGUI desc = roomButtons[i].transform.Find("RoomDescription")?.GetComponent<TextMeshProUGUI>();
                        if (desc != null)
                        {
                            descriptions.Add(desc);
                        }
                        else
                        {
                            Debug.LogError($"DungeonUI: RoomDescription not found on RoomButton{i+1}");
                        }
                    }
                    
                    if (descriptions.Count > 0)
                    {
                        roomDescriptions = descriptions.ToArray();
                        Debug.Log($"DungeonUI: Found {roomDescriptions.Length} room descriptions");
                    }
                }
                else
                {
                    Debug.LogError("DungeonUI: Cannot initialize room descriptions - roomButtons array is empty or null");
                }
            }
                
            // Ensure array lengths match
            if (roomButtons != null && roomTypeIcons != null && roomDescriptions != null && 
                (roomButtons.Length != roomTypeIcons.Length || roomButtons.Length != roomDescriptions.Length))
            {
                Debug.LogError("DungeonUI: roomButtons, roomTypeIcons, and roomDescriptions arrays must have the same length");
            }
            
            // If icons are not set, try to load them from resources
            LoadIconsIfNeeded();
        }
        
        private void LoadIconsIfNeeded()
        {
            if (combatIcon == null)
                combatIcon = Resources.Load<Sprite>("Icons/CombatIcon");
                
            if (shopIcon == null)
                shopIcon = Resources.Load<Sprite>("Icons/ShopIcon");
                
            if (restIcon == null)
                restIcon = Resources.Load<Sprite>("Icons/RestIcon");
                
            if (eventIcon == null)
                eventIcon = Resources.Load<Sprite>("Icons/EventIcon");
                
            if (miniBossIcon == null)
                miniBossIcon = Resources.Load<Sprite>("Icons/MiniBossIcon");
                
            if (bossIcon == null)
                bossIcon = Resources.Load<Sprite>("Icons/BossIcon");
                
            if (defaultIcon == null)
                defaultIcon = Resources.Load<Sprite>("Icons/DefaultIcon");
                
            // 아이콘이 여전히 없는 경우 기본 스프라이트로 설정
            CreateDefaultIcons();
        }
        
        private void CreateDefaultIcons()
        {
            // 기본 아이콘이 없을 경우 단색 이미지로 생성
            if (combatIcon == null) combatIcon = CreateDefaultSprite(Color.red);
            if (shopIcon == null) shopIcon = CreateDefaultSprite(Color.green);
            if (restIcon == null) restIcon = CreateDefaultSprite(Color.blue);
            if (eventIcon == null) eventIcon = CreateDefaultSprite(Color.yellow);
            if (miniBossIcon == null) miniBossIcon = CreateDefaultSprite(new Color(1f, 0.5f, 0f)); // Orange
            if (bossIcon == null) bossIcon = CreateDefaultSprite(new Color(0.5f, 0f, 0.5f)); // Purple
            if (defaultIcon == null) defaultIcon = CreateDefaultSprite(Color.gray);
        }
        
        private Sprite CreateDefaultSprite(Color color)
        {
            // 32x32 크기의 단색 텍스처 생성
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = color;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            // 텍스처로부터 스프라이트 생성
            return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
        }
        
        private void InitializeUI()
        {
            // Initialize UI components that should be set up at the start
            UpdatePlayerStatus(100, 100); // Default values until player data is loaded
            HideAllPanels();
            
            // Set RoomSelectionPanel as the default active panel when entering dungeon
            ShowRoomSelectionPanel();
        }
        
        private void HideAllPanels()
        {
            Debug.Log("DungeonUI: Hiding all panels");
            
            if (roomSelectionPanel != null)
                roomSelectionPanel.SetActive(false);
                
            if (eventPanel != null)
                eventPanel.SetActive(false);
                
            if (shopPanel != null)
                shopPanel.SetActive(false);
                
            if (restPanel != null)
                restPanel.SetActive(false);
                
            // MapPanel과 StatusPanel은 비활성화하지 않음
        }
        
        public void ShowRoomSelectionPanel()
        {
            Debug.Log("DungeonUI: Showing Room Selection Panel");
            HideAllPanels();
            if (roomSelectionPanel != null)
                roomSelectionPanel.SetActive(true);
        }
        
        public void ShowEventPanel()
        {
            Debug.Log("DungeonUI: Showing Event Panel");
            HideAllPanels();
            if (eventPanel != null)
                eventPanel.SetActive(true);
        }
        
        public void ShowShopPanel()
        {
            Debug.Log("DungeonUI: Showing Shop Panel");
            HideAllPanels();
            if (shopPanel != null)
                shopPanel.SetActive(true);
        }
        
        public void ShowRestPanel()
        {
            Debug.Log("DungeonUI: Showing Rest Panel");
            HideAllPanels();
            if (restPanel != null)
                restPanel.SetActive(true);
        }
        
        public void ShowRoomSelection(RoomData[] roomDataArray)
        {
            if (roomSelectionPanel == null || roomButtons == null || roomDataArray == null)
            {
                Debug.LogError("DungeonUI: Cannot show room selection due to missing references");
                return;
            }
            
            // RoomData 배열을 DungeonNode 배열로 변환
            DungeonNode[] availableNodes = new DungeonNode[roomDataArray.Length];
            for (int i = 0; i < roomDataArray.Length; i++)
            {
                if (roomDataArray[i] != null)
                {
                    availableNodes[i] = roomDataArray[i].ToDungeonNode();
                }
            }
            
            // DungeonNode 버전 호출
            ShowRoomSelection(availableNodes);
        }
        
        public void ShowRoomSelection(DungeonNode[] availableNodes)
        {
            if (roomSelectionPanel == null || roomButtons == null || availableNodes == null)
            {
                Debug.LogError("DungeonUI: Cannot show room selection due to missing references");
                return;
            }
            
            // Show room selection panel
            ShowRoomSelectionPanel();
            
            // Clear any previous data
            for (int i = 0; i < roomButtons.Length; i++)
            {
                if (roomButtons[i] == null)
                    continue;
                    
                if (i < availableNodes.Length)
                {
                    roomButtons[i].gameObject.SetActive(true);
                    SetupRoomButton(i, availableNodes[i]);
                }
                else
                {
                    roomButtons[i].gameObject.SetActive(false);
                }
            }
        }
        
        private void SetupRoomButton(int index, DungeonNode node)
        {
            if (index < 0 || index >= roomButtons.Length || node == null)
            {
                Debug.LogError("DungeonUI: Invalid parameters for SetupRoomButton");
                return;
            }
            
            // Set room icon based on type
            if (roomTypeIcons != null && index < roomTypeIcons.Length && roomTypeIcons[index] != null)
            {
                roomTypeIcons[index].sprite = GetRoomIcon(node.Type);
            }
            
            // Set room description
            if (roomDescriptions != null && index < roomDescriptions.Length && roomDescriptions[index] != null)
            {
                roomDescriptions[index].text = GetRoomDescription(node);
                
                // 한글 폰트 적용 시도
                if (FontManager.Instance != null)
                {
                    FontManager.Instance.ApplyKoreanFont(roomDescriptions[index]);
                }
            }
            
            // Set button click handler
            if (roomButtons[index] != null)
            {
                roomButtons[index].onClick.RemoveAllListeners();
                roomButtons[index].onClick.AddListener(() => OnRoomSelected(node));
            }
        }
        
        private Sprite GetRoomIcon(NodeType nodeType)
        {
            // Return the appropriate icon based on room type
            switch (nodeType)
            {
                case NodeType.Combat:
                    return combatIcon;
                case NodeType.Shop:
                    return shopIcon;
                case NodeType.Rest:
                    return restIcon;
                case NodeType.Event:
                    return eventIcon;
                case NodeType.MiniBoss:
                    return miniBossIcon;
                case NodeType.Boss:
                    return bossIcon;
                default:
                    return defaultIcon;
            }
        }
        
        private string GetRoomDescription(DungeonNode node)
        {
            // Generate a description based on node type
            switch (node.Type)
            {
                case NodeType.Combat:
                    return $"전투: 적과 마주치다";
                case NodeType.Shop:
                    return "상점: 아이템과 업그레이드 구매";
                case NodeType.Rest:
                    return "휴식: 체력을 회복하고 다음 전투를 준비";
                case NodeType.Event:
                    return "이벤트: 신비한 상황 발생";
                case NodeType.MiniBoss:
                    return $"미니 보스: 강력한 적이 길을 막고 있다";
                case NodeType.Boss:
                    return $"보스: 이 스테이지의 최종 도전";
                default:
                    return "알 수 없는 방";
            }
        }
        
        private void OnRoomSelected(DungeonNode node)
        {
            // Hide the selection panel
            if (roomSelectionPanel != null)
                roomSelectionPanel.SetActive(false);
            
            // Notify the dungeon manager about the selection
            if (dungeonManager != null && node != null)
            {
                dungeonManager.MoveToNode(node.ID);
            }
            else
            {
                Debug.LogError("DungeonUI: Cannot process room selection due to missing references");
            }
        }
        
        public void UpdateMiniMap(List<RoomNode> mapNodes, int currentNodeIndex)
        {
            if (nodesContainer == null || nodeButtonPrefab == null || mapNodes == null)
            {
                Debug.LogError("DungeonUI: Cannot update mini map due to missing references");
                return;
            }
            
            // RoomNode 리스트를 DungeonNode 리스트로 변환
            List<DungeonNode> dungeonNodes = new List<DungeonNode>();
            foreach (var node in mapNodes)
            {
                if (node != null)
                {
                    dungeonNodes.Add(node.ToDungeonNode());
                }
            }
            
            // DungeonNode 오버로드 호출
            UpdateDungeonMap(dungeonNodes, currentNodeIndex);
        }
        
        public void UpdateDungeonMap(List<DungeonNode> mapNodes, int currentNodeIndex)
        {
            Debug.Log($"DungeonUI: UpdateDungeonMap called with {(mapNodes != null ? mapNodes.Count : 0)} nodes");
            
            // 필요한 UI 컴포넌트 확인 및 생성
            EnsureUIComponentsExist();
            
            if (nodesContainer == null || nodeButtonPrefab == null || mapNodes == null)
            {
                Debug.LogError("DungeonUI: Cannot update mini map due to missing references");
                return;
            }
            
            // Show Room Selection Panel when dungeon map is updated
            ShowRoomSelectionPanel();
            
            // Clear existing nodes if any
            foreach (Transform child in nodesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // 고급 노드 배치 시스템 사용
            if (useAdvancedPositioning && nodePositionManager != null)
            {
                CreateNodesWithAdvancedPositioning(mapNodes, currentNodeIndex);
            }
            else
            {
                CreateNodesWithBasicPositioning(mapNodes, currentNodeIndex);
            }
        }
        
        /// <summary>
        /// 고급 노드 배치 시스템을 사용한 노드 생성
        /// </summary>
        private void CreateNodesWithAdvancedPositioning(List<DungeonNode> mapNodes, int currentNodeIndex)
        {
            // NodePositionManager 초기화
            if (nodePositionManager == null)
            {
                nodePositionManager = GetComponent<NodePositionManager>();
                if (nodePositionManager == null)
                {
                    nodePositionManager = gameObject.AddComponent<NodePositionManager>();
                }
            }
            
            nodePositionManager.Initialize(nodesContainer as RectTransform);
            
            // 최적 위치 계산
            List<Vector2> nodePositions = nodePositionManager.CalculateNodePositions(mapNodes.Count, true);
            
            // 노드 생성 및 배치
            for (int i = 0; i < mapNodes.Count; i++)
            {
                if (mapNodes[i] == null) continue;
                
                try
                {
                    GameObject nodeObj = Instantiate(nodeButtonPrefab, nodesContainer);
                    RectTransform nodeRect = nodeObj.GetComponent<RectTransform>();
                    NodeUI nodeUI = nodeObj.GetComponent<NodeUI>();
                    
                    // 위치 설정
                    if (nodeRect != null && i < nodePositions.Count)
                    {
                        nodeRect.anchoredPosition = nodePositions[i];
                    }
                    
                    // 노드 UI 설정
                    if (nodeUI != null)
                    {
                        nodeUI.SetupNode(mapNodes[i], mapNodes[i].ID == currentNodeIndex);
                    }
                    else
                    {
                        Debug.LogError("DungeonUI: NodeUI component missing on nodeButtonPrefab");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"DungeonUI: Error creating node UI with advanced positioning: {ex.Message}");
                }
            }
            
            Debug.Log($"DungeonUI: Created {mapNodes.Count} nodes with advanced positioning");
        }
        
        /// <summary>
        /// 기본 노드 배치 시스템 (레거시 지원)
        /// </summary>
        private void CreateNodesWithBasicPositioning(List<DungeonNode> mapNodes, int currentNodeIndex)
        {
            // Create new nodes based on the dungeon map
            foreach (var node in mapNodes)
            {
                if (node == null)
                    continue;
                    
                try
                {
                    // nodeButtonPrefab 생성
                    GameObject nodeObj = Instantiate(nodeButtonPrefab, nodesContainer);
                    NodeUI nodeUI = nodeObj.GetComponent<NodeUI>();
                    
                    if (nodeUI != null)
                    {
                        nodeUI.SetupNode(node, node.ID == currentNodeIndex);
                    }
                    else
                    {
                        Debug.LogError("DungeonUI: NodeUI component missing on nodeButtonPrefab");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"DungeonUI: Error creating node UI: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// UI 컴포넌트가 없을 경우 기본 구성 요소를 생성
        /// </summary>
        private void EnsureUIComponentsExist()
        {
            // nodeButtonPrefab이 없을 경우 생성
            if (nodeButtonPrefab == null)
            {
                Debug.Log("DungeonUI: Creating default node button prefab");
                nodeButtonPrefab = new GameObject("NodeButtonPrefab");
                
                // UI 컴포넌트 추가
                RectTransform rectTransform = nodeButtonPrefab.AddComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(40, 40);
                
                // Button 컴포넌트 추가
                Button button = nodeButtonPrefab.AddComponent<Button>();
                Image image = nodeButtonPrefab.AddComponent<Image>();
                image.color = Color.white;
                button.targetGraphic = image;
                
                // NodeUI 컴포넌트 추가 또는 탐색
                NodeUI nodeUI = nodeButtonPrefab.GetComponent<NodeUI>();
                if (nodeUI == null) nodeUI = nodeButtonPrefab.AddComponent<NodeUI>();
                
                // 오브젝트 활성화 유지
                nodeButtonPrefab.SetActive(true);
            }
            
            // nodesContainer가 없을 경우
            if (nodesContainer == null)
            {
                Debug.Log("DungeonUI: Creating nodes container");
                
                // 임시 컨테이너 생성
                GameObject containerObj = new GameObject("NodesContainer");
                containerObj.transform.SetParent(transform, false);
                
                RectTransform rectTransform = containerObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                nodesContainer = containerObj.transform;
            }
        }
        
        public void ShowMiniMap(bool show)
        {
            if (miniMapPanel != null)
                miniMapPanel.SetActive(show);
        }
        
        public void UpdatePlayerStatus(int currentHealth, int maxHealth)
        {
            if (playerHealthText != null)
                playerHealthText.text = $"{currentHealth}/{maxHealth}";
                
            if (playerHealthSlider != null)
                playerHealthSlider.value = maxHealth > 0 ? (float)currentHealth / maxHealth : 0;
        }
        
        public void UpdateStageInfo(int stage, int room)
        {
            if (playerStageText != null)
                playerStageText.text = $"스테이지 {stage} - 방 {room}";
                
            // 한글 폰트 적용
            if (FontManager.Instance != null && playerStageText != null)
            {
                FontManager.Instance.ApplyKoreanFont(playerStageText);
            }
        }
        
        // 던전 상태 효과 리스트 표시 (DungeonStatusEffect 사용)
        public void UpdateStatusEffects(List<DungeonStatusEffect> activeEffects)
        {
            if (statusEffectsContainer == null || statusEffectPrefab == null)
            {
                Debug.LogError("DungeonUI: Cannot update status effects due to missing references");
                return;
            }
            
            // Clear existing effects
            foreach (Transform child in statusEffectsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Skip if no active effects
            if (activeEffects == null || activeEffects.Count == 0)
                return;
                
            // Create new effect indicators
            foreach (var effect in activeEffects)
            {
                if (effect == null)
                    continue;
                    
                GameObject effectObj = Instantiate(statusEffectPrefab, statusEffectsContainer);
                StatusEffectUI effectUI = effectObj.GetComponent<StatusEffectUI>();
                
                if (effectUI != null)
                {
                    effectUI.SetupDungeonEffect(effect);
                }
                else
                {
                    Debug.LogError("DungeonUI: StatusEffectUI component missing on statusEffectPrefab");
                }
            }
        }
        
        // 기존 상태 효과 리스트 표시 (레거시 지원)
        public void UpdateStatusEffects(List<StatusEffect> activeEffects)
        {
            if (statusEffectsContainer == null || statusEffectPrefab == null)
            {
                Debug.LogError("DungeonUI: Cannot update status effects due to missing references");
                return;
            }
            
            // Clear existing effects
            foreach (Transform child in statusEffectsContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Skip if no active effects
            if (activeEffects == null || activeEffects.Count == 0)
                return;
                
            // Create new effect indicators
            foreach (var effect in activeEffects)
            {
                if (effect == null)
                    continue;
                    
                GameObject effectObj = Instantiate(statusEffectPrefab, statusEffectsContainer);
                StatusEffectUI effectUI = effectObj.GetComponent<StatusEffectUI>();
                
                if (effectUI != null)
                {
                    effectUI.SetupEffect(effect);
                }
                else
                {
                    Debug.LogError("DungeonUI: StatusEffectUI component missing on statusEffectPrefab");
                }
            }
        }
    }
}