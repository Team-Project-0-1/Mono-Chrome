using UnityEngine;
using UnityEngine.EventSystems;

namespace MonoChrome
{
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public string tooltipHeader;
        public string tooltipContent;
        
        private static TooltipSystem tooltipSystem;
        
        private void Awake()
        {
            if (tooltipSystem == null)
            {
                tooltipSystem = FindObjectOfType<TooltipSystem>();
            }
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            if (tooltipSystem != null)
            {
                tooltipSystem.Show(tooltipHeader, tooltipContent, Input.mousePosition);
            }
        }
        
        public void OnPointerExit(PointerEventData eventData)
        {
            if (tooltipSystem != null)
            {
                tooltipSystem.Hide();
            }
        }
    }
}