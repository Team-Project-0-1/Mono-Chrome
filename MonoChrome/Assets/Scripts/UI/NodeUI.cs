using MonoChrome.Dungeon;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
            nodeData = data;
            
            // Set node appearance based on state
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
            
            // Set icon based on room type
            nodeIcon.sprite = GetNodeIcon(data.Type);
            
            // Set interactability based on accessibility
            nodeButton.interactable = data.IsAccessible && !data.IsVisited && !isCurrent;
            
            // Set click handler
            nodeButton.onClick.RemoveAllListeners();
            if (data.IsAccessible && !data.IsVisited && !isCurrent)
            {
                nodeButton.onClick.AddListener(() => OnNodeSelected());
            }
        }
        
        private Sprite GetNodeIcon(NodeType type)
        {
            // Try to load icon from resources
            string iconName = type.ToString() + "Icon";
            Sprite icon = Resources.Load<Sprite>($"Icons/{iconName}");
            
            if (icon == null)
            {
                Debug.LogWarning($"Icon not found: Icons/{iconName}");
                // Return a default icon
                return Resources.Load<Sprite>("Icons/DefaultIcon");
            }
            
            return icon;
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