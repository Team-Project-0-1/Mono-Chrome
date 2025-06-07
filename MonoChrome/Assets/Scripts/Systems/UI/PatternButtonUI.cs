using System;
using MonoChrome.Systems.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 패턴 버튼 UI 컴포넌트
    /// Prefab에 붙여서 사용하는 전용 컴포넌트
    /// </summary>
    public class PatternButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Text patternNameText;
        [SerializeField] private Text patternDescriptionText;
        [SerializeField] private Text patternEffectText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image typeIcon;
        
        [Header("Visual Settings")]
        [SerializeField] private Color attackColor = new Color(0.8f, 0.5f, 0.5f);
        [SerializeField] private Color defenseColor = new Color(0.5f, 0.5f, 0.8f);
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.8f);
        
        private Pattern _pattern;
        private Action<Pattern> _onPatternSelected;
        
        private void Awake()
        {
            ValidateReferences();
            SetupButton();
        }
        
        /// <summary>
        /// 참조 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (button == null)
                button = GetComponent<Button>();
            
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
                
            if (patternNameText == null)
            {
                Text[] texts = GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                    patternNameText = texts[0];
            }
        }
        
        /// <summary>
        /// 버튼 설정
        /// </summary>
        private void SetupButton()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        /// <summary>
        /// 패턴 설정
        /// </summary>
        public void Setup(Pattern pattern, Action<Pattern> onPatternSelected)
        {
            _pattern = pattern;
            _onPatternSelected = onPatternSelected;
            
            UpdateVisuals();
        }
        
        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            if (_pattern == null) return;
            
            // 패턴 이름
            if (patternNameText != null)
            {
                if (patternDescriptionText == null)
                {
                    // 단일 텍스트인 경우 이름과 설명을 함께 표시
                    patternNameText.text = $"{_pattern.Name}\n{_pattern.Description}";
                }
                else
                {
                    patternNameText.text = _pattern.Name;
                }
            }
            
            // 패턴 설명
            if (patternDescriptionText != null)
            {
                patternDescriptionText.text = _pattern.Description;
            }
            
            // 패턴 효과
            if (patternEffectText != null)
            {
                string effectText = "";
                if (_pattern.IsAttack)
                {
                    effectText = $"공격력 +{_pattern.AttackBonus}";
                }
                else
                {
                    effectText = $"방어력 +{_pattern.DefenseBonus}";
                }
                patternEffectText.text = effectText;
            }
            
            // 배경 색상
            if (backgroundImage != null)
            {
                backgroundImage.color = _pattern.IsAttack ? attackColor : defenseColor;
            }
            
            // 타입 아이콘 (있다면)
            if (typeIcon != null)
            {
                typeIcon.color = _pattern.IsAttack ? Color.red : Color.blue;
            }
        }
        
        /// <summary>
        /// 버튼 클릭 이벤트
        /// </summary>
        private void OnButtonClicked()
        {
            if (_pattern != null && _onPatternSelected != null)
            {
                _onPatternSelected.Invoke(_pattern);
                Debug.Log($"PatternButtonUI: Pattern selected - {_pattern.Name}");
            }
        }
        
        /// <summary>
        /// 버튼 활성화/비활성화
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
        
        /// <summary>
        /// 패턴 정보 반환
        /// </summary>
        public Pattern GetPattern() => _pattern;
    }
}
