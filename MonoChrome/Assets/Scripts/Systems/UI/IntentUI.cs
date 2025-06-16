using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome.Systems.UI
{
    /// <summary>
    /// 개별 몬스터의 의도를 표시하는 UI 컴포넌트
    /// IntentDisplaySystem에서 관리되며, 의도 아이콘, 텍스트, 설명을 표시한다.
    /// </summary>
    public class IntentUI : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        
        [Header("애니메이션 설정")]
        [SerializeField] private float fadeDuration = 0.3f;
        [SerializeField] private float scalePulseDuration = 0.5f;
        [SerializeField] private Vector3 pulseScale = Vector3.one * 1.1f;
        
        // 내부 상태
        private CanvasGroup canvasGroup;
        private RectTransform rectTransform;
        private string currentDescription;
        private bool isVisible = false;
        
        #region Unity Lifecycle
        private void Awake()
        {
            InitializeComponents();
        }
        
        private void Start()
        {
            SetupEventListeners();
        }
        
        private void OnDestroy()
        {
            RemoveEventListeners();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// 컴포넌트 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // CanvasGroup 컴포넌트 확인/추가
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            // RectTransform 가져오기
            rectTransform = GetComponent<RectTransform>();
            
            // 자동으로 컴포넌트 찾기 (Inspector에서 설정되지 않은 경우)
            if (iconImage == null)
                iconImage = GetComponentInChildren<Image>();
            
            if (valueText == null)
            {
                var texts = GetComponentsInChildren<TextMeshProUGUI>();
                if (texts.Length > 0) valueText = texts[0];
                if (texts.Length > 1) nameText = texts[1];
            }
            
            // 초기 상태 설정
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 이벤트 리스너 설정
        /// </summary>
        private void SetupEventListeners()
        {
            // 마우스 호버 이벤트 (툴팁 표시용)
            var eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // 마우스 진입 시 툴팁 표시
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => ShowTooltip());
            eventTrigger.triggers.Add(pointerEnter);
            
            // 마우스 나갈 시 툴팁 숨기기
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => HideTooltip());
            eventTrigger.triggers.Add(pointerExit);
        }
        
        /// <summary>
        /// 이벤트 리스너 제거
        /// </summary>
        private void RemoveEventListeners()
        {
            var eventTrigger = GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger != null)
            {
                eventTrigger.triggers.Clear();
            }
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// 의도 아이콘 설정
        /// </summary>
        /// <param name="sprite">표시할 스프라이트</param>
        public void SetIcon(Sprite sprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
                iconImage.enabled = sprite != null;
            }
        }
        
        /// <summary>
        /// 의도 색상 설정
        /// </summary>
        /// <param name="color">표시할 색상</param>
        public void SetColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }
        
        /// <summary>
        /// 의도 수치 텍스트 설정
        /// </summary>
        /// <param name="text">표시할 텍스트</param>
        public void SetText(string text)
        {
            if (valueText != null)
            {
                valueText.text = text;
                valueText.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }
        
        /// <summary>
        /// 의도 이름 설정
        /// </summary>
        /// <param name="name">의도 이름</param>
        public void SetName(string name)
        {
            if (nameText != null)
            {
                nameText.text = name;
                nameText.gameObject.SetActive(!string.IsNullOrEmpty(name));
            }
        }
        
        /// <summary>
        /// 의도 설명 설정 (툴팁용)
        /// </summary>
        /// <param name="description">의도 설명</param>
        public void SetDescription(string description)
        {
            currentDescription = description;
            
            if (tooltipText != null)
            {
                tooltipText.text = description;
            }
        }
        
        /// <summary>
        /// 의도 UI 표시
        /// </summary>
        public void Show()
        {
            if (isVisible) return;
            
            gameObject.SetActive(true);
            isVisible = true;
            
            // 페이드 인 애니메이션
            StartCoroutine(FadeIn());
            
            // 펄스 애니메이션 (선택적)
            StartCoroutine(PulseAnimation());
        }
        
        /// <summary>
        /// 의도 UI 숨기기
        /// </summary>
        public void Hide()
        {
            if (!isVisible) return;
            
            isVisible = false;
            
            // 페이드 아웃 애니메이션
            StartCoroutine(FadeOut());
        }
        
        /// <summary>
        /// 즉시 숨기기 (애니메이션 없이)
        /// </summary>
        public void HideImmediate()
        {
            isVisible = false;
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            HideTooltip();
        }
        #endregion
        
        #region Tooltip
        /// <summary>
        /// 툴팁 표시
        /// </summary>
        private void ShowTooltip()
        {
            if (tooltipPanel != null && !string.IsNullOrEmpty(currentDescription))
            {
                tooltipPanel.SetActive(true);
                
                // 툴팁 위치 조정 (마우스 근처 또는 고정 위치)
                if (tooltipPanel.GetComponent<RectTransform>() != null)
                {
                    // 간단한 위치 조정 로직
                    var tooltipRect = tooltipPanel.GetComponent<RectTransform>();
                    tooltipRect.position = transform.position + Vector3.up * 50f;
                }
            }
        }
        
        /// <summary>
        /// 툴팁 숨기기
        /// </summary>
        private void HideTooltip()
        {
            if (tooltipPanel != null)
            {
                tooltipPanel.SetActive(false);
            }
        }
        #endregion
        
        #region Animations
        /// <summary>
        /// 페이드 인 애니메이션
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, progress);
                
                yield return null;
            }
            
            canvasGroup.alpha = 1f;
        }
        
        /// <summary>
        /// 페이드 아웃 애니메이션
        /// </summary>
        private System.Collections.IEnumerator FadeOut()
        {
            float elapsedTime = 0f;
            float startAlpha = canvasGroup.alpha;
            
            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / fadeDuration;
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, progress);
                
                yield return null;
            }
            
            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            HideTooltip();
        }
        
        /// <summary>
        /// 펄스 애니메이션 (주목도 향상)
        /// </summary>
        private System.Collections.IEnumerator PulseAnimation()
        {
            Vector3 originalScale = rectTransform.localScale;
            float elapsedTime = 0f;
            
            // 확대
            while (elapsedTime < scalePulseDuration / 2f)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (scalePulseDuration / 2f);
                
                rectTransform.localScale = Vector3.Lerp(originalScale, pulseScale, progress);
                
                yield return null;
            }
            
            // 축소
            elapsedTime = 0f;
            while (elapsedTime < scalePulseDuration / 2f)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / (scalePulseDuration / 2f);
                
                rectTransform.localScale = Vector3.Lerp(pulseScale, originalScale, progress);
                
                yield return null;
            }
            
            rectTransform.localScale = originalScale;
        }
        #endregion
        
        #region Utility
        /// <summary>
        /// UI가 현재 표시 중인지 확인
        /// </summary>
        public bool IsVisible => isVisible;
        
        /// <summary>
        /// 모든 설정을 한 번에 적용
        /// </summary>
        /// <param name="icon">아이콘</param>
        /// <param name="color">색상</param>
        /// <param name="text">텍스트</param>
        /// <param name="description">설명</param>
        public void SetupIntent(Sprite icon, Color color, string text, string description)
        {
            SetIcon(icon);
            SetColor(color);
            SetText(text);
            SetDescription(description);
        }
        #endregion
    }
}