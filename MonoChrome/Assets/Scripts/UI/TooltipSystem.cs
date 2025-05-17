using UnityEngine;

namespace MonoChrome
{
    public class TooltipSystem : MonoBehaviour
    {
        [SerializeField] private Tooltip tooltip;
        
        private static TooltipSystem current;
        
        private void Awake()
        {
            current = this;
            tooltip.gameObject.SetActive(false);
        }
        
        public void Show(string header, string content, Vector3 position)
        {
            tooltip.SetText(header, content);
            tooltip.gameObject.SetActive(true);
            
            // Position the tooltip
            tooltip.SetPosition(position);
        }
        
        public void Hide()
        {
            tooltip.gameObject.SetActive(false);
        }
    }
}