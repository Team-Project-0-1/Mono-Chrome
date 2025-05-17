using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome
{
    public class StatusEffectUI : MonoBehaviour
    {
        [SerializeField] private Image effectIcon;
        [SerializeField] private TextMeshProUGUI stacksText;
        [SerializeField] private TextMeshProUGUI durationText;
        [SerializeField] private TooltipTrigger tooltipTrigger;

        // Dictionary of effect descriptions for common status effects
        private static readonly System.Collections.Generic.Dictionary<StatusEffectType, string> EffectDescriptions = 
            new System.Collections.Generic.Dictionary<StatusEffectType, string>
        {
            { StatusEffectType.Amplify, "공격력/방어력을 +1 획득하며 최대 10까지 누적된다. 증폭 1당 매 턴 1 자해 피해" },
            { StatusEffectType.Mark, "적에게 부여한 수치만큼 다음번 자신의 공격 횟수를 증가 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Curse, "턴마다 지속 고정 피해 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Counter, "피격 시 수치당 적에게 2 고정 피해 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Bleed, "방어력 무시 지속 피해 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Resonance, "누적된 후 2턴 뒤 공명 수치만큼 즉시 피해" },
            { StatusEffectType.Seal, "동전 n개 무작위 봉쇄 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Crush, "방어력 1.5배 감소 (지속형 상태 효과)" },
            { StatusEffectType.Poison, "턴마다 지속 피해 (턴 마다 누적 수치 1 감소)" },
            { StatusEffectType.Burn, "즉시 피해 후 소멸" },
            { StatusEffectType.MultiAttack, "여러번 연속 공격" }
        };

        // 전투 StatusEffect 설정
        public void SetupEffect(StatusEffect effect)
        {
            // Exit early if the passed effect is null
            if (effect == null)
            {
                Debug.LogError("Null StatusEffect passed to StatusEffectUI.SetupEffect");
                gameObject.SetActive(false);
                return;
            }
            
            // Set the icon
            string iconPath = $"Icons/StatusEffects/{effect.EffectType}Icon";
            Sprite icon = Resources.Load<Sprite>(iconPath);
            
            if (icon != null)
            {
                effectIcon.sprite = icon;
            }
            else
            {
                Debug.LogWarning($"Status effect icon not found: {iconPath}");
                // Load a default icon
                effectIcon.sprite = Resources.Load<Sprite>("Icons/StatusEffects/DefaultIcon");
            }
            
            // Set stacks text if more than 1, otherwise hide it
            if (effect.Magnitude > 1)
            {
                stacksText.gameObject.SetActive(true);
                stacksText.text = effect.Magnitude.ToString();
            }
            else
            {
                stacksText.gameObject.SetActive(false);
            }
            
            // Set duration text if has duration, otherwise hide it
            if (effect.RemainingDuration > 0)
            {
                durationText.gameObject.SetActive(true);
                durationText.text = effect.RemainingDuration.ToString();
            }
            else
            {
                durationText.gameObject.SetActive(false);
            }
            
            // Set up tooltip if available
            if (tooltipTrigger != null)
            {
                tooltipTrigger.tooltipHeader = effect.EffectType.ToString();
                tooltipTrigger.tooltipContent = GetEffectDescription(effect);
            }
        }
        
        // 던전 StatusEffect 설정
        public void SetupDungeonEffect(DungeonStatusEffect effect)
        {
            // Exit early if the passed effect is null
            if (effect == null)
            {
                Debug.LogError("Null DungeonStatusEffect passed to StatusEffectUI.SetupDungeonEffect");
                gameObject.SetActive(false);
                return;
            }
            
            // Set the icon
            if (effect.icon != null)
            {
                effectIcon.sprite = effect.icon;
            }
            else
            {
                // Try to load the icon if not already set
                string iconPath = $"Icons/StatusEffects/{effect.name}Icon";
                Sprite icon = Resources.Load<Sprite>(iconPath);
                
                if (icon != null)
                {
                    effectIcon.sprite = icon;
                    // Update the original effect's icon reference for future use
                    effect.icon = icon;
                }
                else
                {
                    Debug.LogWarning($"Status effect icon not found: {iconPath}");
                    // Load a default icon
                    effectIcon.sprite = Resources.Load<Sprite>("Icons/StatusEffects/DefaultIcon");
                }
            }
            
            // Set stacks text if more than 1, otherwise hide it
            if (effect.stacks > 1)
            {
                stacksText.gameObject.SetActive(true);
                stacksText.text = effect.stacks.ToString();
            }
            else
            {
                stacksText.gameObject.SetActive(false);
            }
            
            // Set duration text if has duration, otherwise hide it
            if (effect.duration > 0)
            {
                durationText.gameObject.SetActive(true);
                durationText.text = effect.duration.ToString();
            }
            else if (effect.duration == -1) // Permanent effect
            {
                durationText.gameObject.SetActive(true);
                durationText.text = "∞"; // Infinity symbol for permanent effects
            }
            else
            {
                durationText.gameObject.SetActive(false);
            }
            
            // Set up tooltip if available
            if (tooltipTrigger != null)
            {
                tooltipTrigger.tooltipHeader = effect.name;
                tooltipTrigger.tooltipContent = GetDungeonEffectDescription(effect);
            }
        }
        
        private string GetEffectDescription(StatusEffect effect)
        {
            // Check if we have a pre-defined description for this effect
            if (EffectDescriptions.TryGetValue(effect.EffectType, out string description))
            {
                return $"{description} ({effect.Magnitude} 중첩, {effect.RemainingDuration} 턴 남음)";
            }
            
            // Default description for unknown effects
            return $"{effect.EffectType}: 상태 효과, {effect.Magnitude} 중첩, {effect.RemainingDuration} 턴 남음";
        }
        
        private string GetDungeonEffectDescription(DungeonStatusEffect effect)
        {
            // More general description for dungeon effects
            string durationText = effect.duration == -1 ? "영구적" : effect.duration + " 턴 남음";
            return $"{effect.name}: {effect.stacks} 중첩, {durationText}";
        }
    }
}