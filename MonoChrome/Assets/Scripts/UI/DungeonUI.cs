using System.Collections;
using System.Collections.Generic;
using MonoChrome.Dungeon;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


namespace MonoChrome
{
    public class DungeonUI : MonoBehaviour
    {
        [Header("Room UI References")]
        [SerializeField] private GameObject roomSelectionPanel;
        [SerializeField] private Button[] roomButtons;
        [SerializeField] private Image[] roomTypeIcons;
        [SerializeField] private TextMeshProUGUI[] roomDescriptions;
        
        [Header("Mini Map")]
        [SerializeField] private GameObject miniMapPanel;
        [SerializeField] private Transform nodesContainer;
        [SerializeField] private GameObject nodeButtonPrefab;
        
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
                Debug.LogError("DungeonUI: roomSelectionPanel reference is missing");
            
            if (miniMapPanel == null)
                Debug.LogError("DungeonUI: miniMapPanel reference is missing");
                
            if (nodesContainer == null)
                Debug.LogError("DungeonUI: nodesContainer reference is missing");
                
            if (nodeButtonPrefab == null)
                Debug.LogError("DungeonUI: nodeButtonPrefab reference is missing");
                
            if (playerHealthText == null)
                Debug.LogError("DungeonUI: playerHealthText reference is missing");
                
            if (playerHealthSlider == null)
                Debug.LogError("DungeonUI: playerHealthSlider reference is missing");
                
            if (playerStageText == null)
                Debug.LogError("DungeonUI: playerStageText reference is missing");
                
            if (statusEffectsContainer == null)
                Debug.LogError("DungeonUI: statusEffectsContainer reference is missing");
                
            if (statusEffectPrefab == null)
                Debug.LogError("DungeonUI: statusEffectPrefab reference is missing");
                
            // Check for array references
            if (roomButtons == null || roomButtons.Length == 0)
                Debug.LogError("DungeonUI: roomButtons array is empty or null");
                
            if (roomTypeIcons == null || roomTypeIcons.Length == 0)
                Debug.LogError("DungeonUI: roomTypeIcons array is empty or null");
                
            if (roomDescriptions == null || roomDescriptions.Length == 0)
                Debug.LogError("DungeonUI: roomDescriptions array is empty or null");
                
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
        }
        
        private void InitializeUI()
        {
            // Initialize UI components that should be set up at the start
            UpdatePlayerStatus(100, 100); // Default values until player data is loaded
            HideAllPanels();
        }
        
        private void HideAllPanels()
        {
            if (roomSelectionPanel != null)
                roomSelectionPanel.SetActive(false);
                
            if (miniMapPanel != null)
                miniMapPanel.SetActive(false);
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
            
            roomSelectionPanel.SetActive(true);
            
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
            if (roomTypeIcons[index] != null)
            {
                roomTypeIcons[index].sprite = GetRoomIcon(node.Type);
            }
            
            // Set room description
            if (roomDescriptions[index] != null)
            {
                roomDescriptions[index].text = GetRoomDescription(node);
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
                    return $"Combat: Face an enemy";
                case NodeType.Shop:
                    return "Shop: Buy items and upgrades";
                case NodeType.Rest:
                    return "Rest: Recover health and prepare for battles ahead";
                case NodeType.Event:
                    return "Event: Encounter a mysterious situation";
                case NodeType.MiniBoss:
                    return $"Mini Boss: A powerful enemy blocks your path";
                case NodeType.Boss:
                    return $"Boss: The final challenge of this stage";
                default:
                    return "Unknown room type";
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
            if (nodesContainer == null || nodeButtonPrefab == null || mapNodes == null)
            {
                Debug.LogError("DungeonUI: Cannot update mini map due to missing references");
                return;
            }
            
            // Clear existing nodes if any
            foreach (Transform child in nodesContainer)
            {
                Destroy(child.gameObject);
            }
            
            // Create new nodes based on the dungeon map
            foreach (var node in mapNodes)
            {
                if (node == null)
                    continue;
                    
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
                playerStageText.text = $"Stage {stage} - Room {room}";
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