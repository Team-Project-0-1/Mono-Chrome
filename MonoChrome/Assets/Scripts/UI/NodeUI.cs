using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DungeonManager = MonoChrome.Dungeon.DungeonManager;

namespace MonoChrome
{
    public class NodeUI : MonoBehaviour
    {
        [SerializeField] private Image nodeIcon;
        [SerializeField] private Image nodeBackground;
        [SerializeField] private Button nodeButton;
        [SerializeField] private Color visitedColor = Color.gray;
        [SerializeField] private Color currentColor = Color.yellow;
        [SerializeField] private Color accessibleColor = Color.white;
        [SerializeField] private Color inaccessibleColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        
        private DungeonNode nodeData;
        private DungeonManager dungeonManager;
        
        private void Awake()
        {
            // Find reference once at start
            dungeonManager = FindObjectOfType<DungeonManager>();
            if (dungeonManager == null)
            {
                Debug.LogError("DungeonManager not found in the scene!");
            }
        }
        
        public void SetupNode(DungeonNode data, bool isCurrent)
        {
            if (data == null)
            {
                Debug.LogError("NodeUI: Cannot setup node with null data");
                return;
            }
            
            nodeData = data;
            
            // 노드 UI 컴포넌트 참조 확인
            if (nodeBackground == null)
            {
                nodeBackground = GetComponent<Image>();
                if (nodeBackground == null && gameObject != null)
                {
                    nodeBackground = gameObject.AddComponent<Image>();
                }
            }
            
            if (nodeIcon == null)
            {
                // 아이콘용 컴포넌트 찾거나 생성
                Transform iconTransform = transform.Find("Icon");
                if (iconTransform == null)
                {
                    GameObject iconObject = new GameObject("Icon");
                    iconObject.transform.SetParent(transform, false);
                    
                    RectTransform rectTransform = iconObject.AddComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchoredPosition = Vector2.zero;
                    rectTransform.sizeDelta = new Vector2(30, 30);
                    
                    nodeIcon = iconObject.AddComponent<Image>();
                }
                else
                {
                    nodeIcon = iconTransform.GetComponent<Image>();
                    if (nodeIcon == null)
                    {
                        nodeIcon = iconTransform.gameObject.AddComponent<Image>();
                    }
                }
            }
            
            if (nodeButton == null)
            {
                nodeButton = GetComponent<Button>();
                if (nodeButton == null && gameObject != null)
                {
                    nodeButton = gameObject.AddComponent<Button>();
                    nodeButton.targetGraphic = nodeBackground;
                }
            }
            
            // Set node appearance based on state
            if (nodeBackground != null)
            {
                if (isCurrent)
                {
                    nodeBackground.color = currentColor;
                }
                else if (data.IsVisited)
                {
                    nodeBackground.color = visitedColor;
                }
                else if (data.IsAccessible)
                {
                    nodeBackground.color = accessibleColor;
                }
                else
                {
                    nodeBackground.color = inaccessibleColor;
                }
            }
            
            // Set icon based on room type
            if (nodeIcon != null)
            {
                nodeIcon.sprite = GetNodeIcon(data.Type);
            }
            
            // Set interactability based on accessibility
            if (nodeButton != null)
            {
                nodeButton.interactable = data.IsAccessible && !data.IsVisited && !isCurrent;
                
                // Set click handler
                nodeButton.onClick.RemoveAllListeners();
                if (data.IsAccessible && !data.IsVisited && !isCurrent)
                {
                    nodeButton.onClick.AddListener(() => OnNodeSelected());
                }
            }
        }
        
        private Sprite GetNodeIcon(NodeType type)
        {
            try
            {
                // Try to load icon from resources
                string iconName = type.ToString() + "Icon";
                Sprite icon = Resources.Load<Sprite>($"Icons/{iconName}");
                
                if (icon == null)
                {
                    Debug.LogWarning($"NodeUI: Icon not found: Icons/{iconName}");
                    
                    // 디버그용 가상 아이콘 생성
                    icon = CreateDebugIcon(type);
                }
                
                return icon;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"NodeUI: Error loading icon: {ex.Message}");
                return null;
            }
        }
        
        // 디버그용 색상 아이콘 생성
        private Sprite CreateDebugIcon(NodeType type)
        {
            Color iconColor = Color.gray;
            
            // 노드 타입에 따라 다른 색상 사용
            switch (type)
            {
                case NodeType.Combat:
                    iconColor = Color.red;
                    break;
                case NodeType.Shop:
                    iconColor = Color.green;
                    break;
                case NodeType.Rest:
                    iconColor = Color.blue;
                    break;
                case NodeType.Event:
                    iconColor = Color.yellow;
                    break;
                case NodeType.MiniBoss:
                    iconColor = new Color(1f, 0.5f, 0f); // 주황색
                    break;
                case NodeType.Boss:
                    iconColor = new Color(0.5f, 0f, 0.5f); // 보라색
                    break;
            }
            
            // 32x32 크기의 단색 텍스처 생성
            Texture2D texture = new Texture2D(32, 32);
            Color[] colors = new Color[32 * 32];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = iconColor;
            }
            texture.SetPixels(colors);
            texture.Apply();
            
            // 텍스처로부터 스프라이트 생성
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            return sprite;
        }
        
        // 노드 클릭 이벤트 핸들러 메서드
        private void OnNodeSelected()
        {
            // 캐시된 던전 매니저 참조 사용
            if (dungeonManager != null)
            {
                dungeonManager.MoveToNode(nodeData.ID);
            }
            else
            {
                // 컴포넌트가 생성된 후 던전 매니저가 다시 찾아지는 경우
                dungeonManager = FindObjectOfType<DungeonManager>();
                if (dungeonManager != null)
                {
                    dungeonManager.MoveToNode(nodeData.ID);
                }
                else
                {
                    Debug.LogError("DungeonManager not found!");
                }
            }
        }
    }
}