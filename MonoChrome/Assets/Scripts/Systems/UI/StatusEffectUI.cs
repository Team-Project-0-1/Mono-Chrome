using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 상태 효과 UI 컴포넌트
    /// Prefab에 붙여서 사용하는 전용 컴포넌트
    /// </summary>
    public class StatusEffectUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Text nameText;
        [SerializeField] private Text magnitudeText;
        [SerializeField] private Text durationText;
        
        private StatusEffect _statusEffect;
        
        private void Awake()
        {
            ValidateReferences();
        }
        
        /// <summary>
        /// 참조 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
                
            if (nameText == null)
            {
                Text[] texts = GetComponentsInChildren<Text>();
                if (texts.Length > 0)
                    nameText = texts[0];
            }
        }
        
        /// <summary>
        /// 상태 효과 설정
        /// </summary>
        public void Setup(StatusEffect statusEffect)
        {
            _statusEffect = statusEffect;
            UpdateVisuals();
        }
        
        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            if (_statusEffect == null) return;
            
            // 배경 색상
            if (backgroundImage != null)
            {
                backgroundImage.color = GetStatusEffectColor(_statusEffect.EffectType);
            }
            
            // 이름
            if (nameText != null)
            {
                if (magnitudeText == null && durationText == null)
                {
                    // 단일 텍스트인 경우 모든 정보를 표시
                    nameText.text = $"{GetStatusEffectShortName(_statusEffect.EffectType)}\n{_statusEffect.Magnitude}";
                }
                else
                {
                    nameText.text = GetStatusEffectShortName(_statusEffect.EffectType);
                }
            }
            
            // 수치
            if (magnitudeText != null)
            {
                magnitudeText.text = _statusEffect.Magnitude.ToString();
            }
            
            // 지속 시간
            if (durationText != null)
            {
                if (_statusEffect.RemainingDuration > 0)
                {
                    durationText.text = _statusEffect.RemainingDuration.ToString();
                }
                else
                {
                    durationText.text = "∞"; // 무한 지속
                }
            }
            
            // 아이콘 (있다면)
            if (iconImage != null)
            {
                iconImage.color = GetStatusEffectColor(_statusEffect.EffectType);
            }
        }
        
        /// <summary>
        /// 상태 효과 업데이트 (매 턴마다 호출)
        /// </summary>
        public void UpdateEffect()
        {
            if (_statusEffect != null)
            {
                UpdateVisuals();
            }
        }
        
        /// <summary>
        /// 상태 효과 색상 반환
        /// </summary>
        private Color GetStatusEffectColor(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify:
                    return new Color(0.8f, 0.5f, 0.0f); // 주황색
                case StatusEffectType.Resonance:
                    return new Color(0.8f, 0.8f, 0.2f); // 노란색
                case StatusEffectType.Mark:
                    return new Color(0.5f, 0.8f, 0.5f); // 녹색
                case StatusEffectType.Bleed:
                    return new Color(0.8f, 0.0f, 0.0f); // 빨간색
                case StatusEffectType.Counter:
                    return new Color(0.5f, 0.5f, 0.8f); // 파란색
                case StatusEffectType.Crush:
                    return new Color(0.5f, 0.2f, 0.0f); // 갈색
                case StatusEffectType.Curse:
                    return new Color(0.5f, 0.0f, 0.5f); // 보라색
                case StatusEffectType.Seal:
                    return new Color(0.2f, 0.2f, 0.2f); // 회색
                default:
                    return Color.gray;
            }
        }
        
        /// <summary>
        /// 상태 효과 짧은 이름 반환
        /// </summary>
        private string GetStatusEffectShortName(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify:
                    return "증폭";
                case StatusEffectType.Resonance:
                    return "공명";
                case StatusEffectType.Mark:
                    return "표식";
                case StatusEffectType.Bleed:
                    return "출혈";
                case StatusEffectType.Counter:
                    return "반격";
                case StatusEffectType.Crush:
                    return "분쇄";
                case StatusEffectType.Curse:
                    return "저주";
                case StatusEffectType.Seal:
                    return "봉인";
                default:
                    return "효과";
            }
        }
        
        /// <summary>
        /// 던전 상태 효과 설정 (DungeonStatusEffect 용)
        /// </summary>
        public void SetupDungeonEffect(DungeonStatusEffect dungeonEffect)
        {
            if (dungeonEffect == null) return;
            
            // DungeonStatusEffect의 실제 프로퍼티 사용
            StatusEffectType effectType = ParseEffectTypeFromName(dungeonEffect.name);
            
            // DungeonStatusEffect를 StatusEffect로 변환해서 기존 Setup 메서드 사용
            StatusEffect statusEffect = new StatusEffect(
                effectType,
                dungeonEffect.stacks,  // magnitude 대신 stacks 사용
                dungeonEffect.duration  // duration 사용
            );
            
            Setup(statusEffect);
        }
        
        /// <summary>
        /// 문자열 이름에서 StatusEffectType 파싱
        /// </summary>
        private StatusEffectType ParseEffectTypeFromName(string name)
        {
            if (string.IsNullOrEmpty(name)) return StatusEffectType.None;
            
            // enum 이름으로 파싱 시도
            if (System.Enum.TryParse<StatusEffectType>(name, true, out StatusEffectType result))
            {
                return result;
            }
            
            // 한글 이름으로 매칭
            switch (name.ToLower())
            {
                case "증폭": return StatusEffectType.Amplify;
                case "공명": return StatusEffectType.Resonance;
                case "표식": return StatusEffectType.Mark;
                case "출혈": return StatusEffectType.Bleed;
                case "반격": return StatusEffectType.Counter;
                case "분쇄": return StatusEffectType.Crush;
                case "저주": return StatusEffectType.Curse;
                case "봉인": return StatusEffectType.Seal;
                default: return StatusEffectType.None;
            }
        }
        
        /// <summary>
        /// 상태 효과 설정 (레거시 지원용 별칭)
        /// </summary>
        public void SetupEffect(StatusEffect statusEffect)
        {
            Setup(statusEffect);
        }
        
        /// <summary>
        /// 상태 효과 반환
        /// </summary>
        public StatusEffect GetStatusEffect() => _statusEffect;
    }
}
