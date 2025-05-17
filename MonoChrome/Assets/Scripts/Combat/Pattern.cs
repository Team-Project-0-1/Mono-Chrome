using System.Collections;
using System.Collections.Generic;
using MonoChrome.StatusEffects;
using UnityEngine;
using MonoChrome.Extensions;

namespace MonoChrome.Combat
{
    /// <summary>
    /// 패턴(족보) 클래스
    /// 동전 조합으로 만들어지는 전투 패턴을 정의합니다.
    /// </summary>
    [System.Serializable]
    public class Pattern
    {
        // 기본 정보
        public string Name;
        public string Description;
        public int ID;
        
        // 패턴 유형
        public PatternType PatternType;
        public bool PatternValue; // true: 앞면, false: 뒷면
        public bool IsAttack; // true: 공격, false: 방어
        
        // 효과
        public int AttackBonus;
        public int DefenseBonus;
        public string SpecialEffect; // 특수 효과 설명
        
        // 상태 효과
        public StatusEffects.StatusEffect.StatusEffectData[] StatusEffects;
        
        /// <summary>
        /// 기본 생성자
        /// </summary>
        public Pattern()
        {
            Name = "기본 패턴";
            Description = "설명 없음";
            ID = 0;
            PatternType = PatternType.None;
            PatternValue = true;
            IsAttack = true;
            AttackBonus = 0;
            DefenseBonus = 0;
            StatusEffects = new StatusEffects.StatusEffect.StatusEffectData[0];
        }
        
        /// <summary>
        /// 복사 생성자
        /// </summary>
        public Pattern(Pattern other)
        {
            if (other == null)
            {
                Debug.LogError("Pattern: Cannot copy from null pattern");
                return;
            }
            
            Name = other.Name;
            Description = other.Description;
            ID = other.ID;
            PatternType = other.PatternType;
            PatternValue = other.PatternValue;
            IsAttack = other.IsAttack;
            AttackBonus = other.AttackBonus;
            DefenseBonus = other.DefenseBonus;
            SpecialEffect = other.SpecialEffect;
            
            // 상태 효과 복사
            if (other.StatusEffects != null)
            {
                StatusEffects = new StatusEffects.StatusEffect.StatusEffectData[other.StatusEffects.Length];
                for (int i = 0; i < other.StatusEffects.Length; i++)
                {
                    StatusEffects[i] = other.StatusEffects[i];
                }
            }
            else
            {
                StatusEffects = new StatusEffects.StatusEffect.StatusEffectData[0];
            }
        }
        
        /// <summary>
        /// Pattern 문자열 표현 (디버깅용)
        /// </summary>
        public override string ToString()
        {
            return $"{Name} (ID: {ID}, Type: {PatternType}, {(IsAttack ? "공격" : "방어")})";
        }
        
        /// <summary>
        /// 패턴 유형을 한글로 변환
        /// </summary>
        public string GetPatternTypeString()
        {
            switch (PatternType)
            {
                case PatternType.Consecutive2:
                    return $"{(PatternValue ? "앞면" : "뒷면")} 2연속";
                case PatternType.Consecutive3:
                    return $"{(PatternValue ? "앞면" : "뒷면")} 3연속";
                case PatternType.Consecutive4:
                    return $"{(PatternValue ? "앞면" : "뒷면")} 4연속";
                case PatternType.Consecutive5:
                    return $"{(PatternValue ? "앞면" : "뒷면")} 5연속";
                case PatternType.AllOfOne:
                    return $"모두 {(PatternValue ? "앞면" : "뒷면")}";
                case PatternType.Alternating:
                    return "앞뒤 교차";
                default:
                    return "알 수 없음";
            }
        }
        
        /// <summary>
        /// 효과 요약 문자열 (UI 표시용)
        /// </summary>
        public string GetEffectSummary()
        {
            string result = "";
            
            if (IsAttack)
            {
                result += $"공격력 +{AttackBonus}";
            }
            else
            {
                result += $"방어력 +{DefenseBonus}";
            }
            
            if (!string.IsNullOrEmpty(SpecialEffect))
            {
                result += $", {SpecialEffect}";
            }
            
            if (StatusEffects != null && StatusEffects.Length > 0)
            {
                result += "\n효과: ";
                for (int i = 0; i < StatusEffects.Length; i++)
                {
                    var effect = StatusEffects[i];
                    result += $"{effect.EffectType.GetKoreanName()} {effect.Magnitude} ({effect.Duration}턴)";
                    
                    if (i < StatusEffects.Length - 1)
                    {
                        result += ", ";
                    }
                }
            }
            
            return result;
        }
    }
}