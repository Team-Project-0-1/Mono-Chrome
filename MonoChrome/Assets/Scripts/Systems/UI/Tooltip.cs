using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MonoChrome
{
    public class Tooltip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI headerText;
        [SerializeField] private TextMeshProUGUI contentText;
        [SerializeField] private LayoutElement layoutElement;
        [SerializeField] private int characterWrapLimit = 40;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Canvas canvas;
        [SerializeField] private float padding = 10f;
        
        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
                
            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();
        }
        
        public void SetText(string header, string content)
        {
            // Set header if provided, otherwise hide it
            if (string.IsNullOrEmpty(header))
            {
                headerText.gameObject.SetActive(false);
            }
            else
            {
                headerText.gameObject.SetActive(true);
                headerText.text = header;
            }
            
            // Set content
            contentText.text = content;
            
            // Adjust layout if text is too long
            int headerLength = headerText.text.Length;
            int contentLength = contentText.text.Length;
            
            layoutElement.enabled = (headerLength > characterWrapLimit || contentLength > characterWrapLimit);
        }
        
        public void SetPosition(Vector3 position)
        {
            if (canvas == null) return;
            
            // Convert screen position to canvas position
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                position,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            // Set tooltip position
            rectTransform.anchoredPosition = localPoint;
            
            // Adjust to prevent going off-screen
            AdjustPositionToStayOnScreen();
        }
        
        private void AdjustPositionToStayOnScreen()
        {
            Vector2 position = rectTransform.anchoredPosition;
            Vector2 size = rectTransform.sizeDelta;
            Vector2 canvasSize = canvas.GetComponent<RectTransform>().sizeDelta;
            
            // Check right edge
            if (position.x + size.x / 2 > canvasSize.x / 2 - padding)
            {
                position.x = canvasSize.x / 2 - size.x / 2 - padding;
            }
            
            // Check left edge
            if (position.x - size.x / 2 < -canvasSize.x / 2 + padding)
            {
                position.x = -canvasSize.x / 2 + size.x / 2 + padding;
            }
            
            // Check top edge
            if (position.y + size.y / 2 > canvasSize.y / 2 - padding)
            {
                position.y = canvasSize.y / 2 - size.y / 2 - padding;
            }
            
            // Check bottom edge
            if (position.y - size.y / 2 < -canvasSize.y / 2 + padding)
            {
                position.y = -canvasSize.y / 2 + size.y / 2 + padding;
            }
            
            rectTransform.anchoredPosition = position;
        }
    }
}