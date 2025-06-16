using UnityEngine;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;

namespace MonoChrome
{
    /// <summary>
    /// 패턴(족보) 데이터를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Pattern", menuName = "MonoChrome/Combat/Pattern")]
    public class PatternSO : ScriptableObject
    {
        [Header("기본 정보")]
        public string patternName;
        [TextArea(2, 5)]
        public string description;
        public int id;
        public Sprite icon;
        
        [Header("패턴 속성")]
        public PatternType patternType;
        public bool patternValue; // true: 앞면, false: 뒷면
        public bool isAttack; // true: 공격, false: 방어
        
        [Header("패턴 조건")]
        public PatternConditionSO condition; // 패턴 조건 참조
        
        [Header("캐릭터 요구사항")]
        public CharacterType[] applicableCharacterTypes; // 어떤 캐릭터 타입이 사용할 수 있는지
        public SenseType[] applicableSenseTypes; // 어떤 감각 타입이 사용할 수 있는지
        
        [Header("효과")]
        public int attackBonus;
        public int defenseBonus;
        [TextArea(1, 3)]
        public string specialEffect;
        
        [Header("상태 효과")]
        public StatusEffectDataWrapper[] statusEffects;
        
        [Header("시각 효과")]
        public string animationTrigger;
        
        /// <summary>
        /// Pattern 클래스로 변환 (기존 코드와의 호환성 유지)
        /// </summary>
        public Pattern ToPattern()
        {
            return new Pattern
            {
                Name = patternName,
                Description = description,
                ID = id,
                PatternType = patternType,
                PatternValue = patternValue,
                IsAttack = isAttack,
                AttackBonus = attackBonus,
                DefenseBonus = defenseBonus,
                SpecialEffect = specialEffect,
                StatusEffects = ConvertStatusEffects()
            };
        }
        
        /// <summary>
        /// 상태 효과 변환
        /// </summary>
        private StatusEffect.StatusEffectData[] ConvertStatusEffects()
        {
            if (statusEffects == null || statusEffects.Length == 0)
                return new StatusEffect.StatusEffectData[0];
                
            StatusEffect.StatusEffectData[] result = new StatusEffect.StatusEffectData[statusEffects.Length];
            for (int i = 0; i < statusEffects.Length; i++)
            {
                result[i] = statusEffects[i].ToStatusEffectData();
            }
            return result;
        }
        
        /// <summary>
        /// 패턴이 현재 동전 상태와 일치하는지 검증
        /// </summary>
        public bool ValidatePattern(bool[] coinStates)
        {
            return ValidatePatternWithValue(coinStates, patternValue);
        }
        
        /// <summary>
        /// 패턴이 특정 값(앞면/뒷면)으로 현재 동전 상태와 일치하는지 검증
        /// </summary>
        public bool ValidatePatternWithValue(bool[] coinStates, bool testValue)
        {
            // PatternConditionSO가 있으면 해당 조건 사용
            if (condition != null)
            {
                return condition.Validate(coinStates, testValue);
            }
            
            // 조건이 없으면 기존 패턴 타입 기반 검증
            switch (patternType)
            {
                case PatternType.Consecutive2:
                    return HasConsecutiveCoins(coinStates, 2, testValue);
                    
                case PatternType.Consecutive3:
                    return HasConsecutiveCoins(coinStates, 3, testValue);
                    
                case PatternType.Consecutive4:
                    return HasConsecutiveCoins(coinStates, 4, testValue);
                    
                case PatternType.Consecutive5:
                    return HasConsecutiveCoins(coinStates, 5, testValue);
                    
                case PatternType.AllOfOne:
                    return HasAllSameCoins(coinStates, testValue);
                    
                case PatternType.Alternating:
                    return HasAlternatingCoins(coinStates);
                    
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 연속된 같은 면의 동전이 있는지 확인
        /// </summary>
        private bool HasConsecutiveCoins(bool[] coinStates, int count, bool isHeads)
        {
            if (count > coinStates.Length)
            {
                return false;
            }
            
            int currentCount = 0;
            int maxCount = 0;
            
            foreach (bool coin in coinStates)
            {
                if (coin == isHeads)
                {
                    currentCount++;
                    maxCount = Mathf.Max(maxCount, currentCount);
                }
                else
                {
                    currentCount = 0;
                }
            }
            
            return maxCount >= count;
        }
        
        /// <summary>
        /// 모든 동전이 같은 면인지 확인
        /// </summary>
        private bool HasAllSameCoins(bool[] coinStates, bool isHeads)
        {
            if (coinStates.Length == 0)
            {
                return false;
            }
            
            foreach (bool coin in coinStates)
            {
                if (coin != isHeads)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 동전이 교대로 나타나는지 확인 (앞뒤앞뒤... 또는 뒤앞뒤앞...)
        /// </summary>
        private bool HasAlternatingCoins(bool[] coinStates)
        {
            if (coinStates.Length <= 1)
            {
                return false;
            }
            
            bool expected = coinStates[0];
            
            for (int i = 0; i < coinStates.Length; i++)
            {
                if (i % 2 == 0 && coinStates[i] != expected)
                {
                    return false;
                }
                else if (i % 2 == 1 && coinStates[i] == expected)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 이 패턴이 특정 캐릭터 타입과 감각 타입에 적용 가능한지 확인
        /// </summary>
        public bool IsApplicableTo(CharacterType characterType, SenseType senseType = SenseType.Auditory)
        {
            // 캐릭터 타입 확인
            bool characterTypeMatch = false;
            if (applicableCharacterTypes == null || applicableCharacterTypes.Length == 0)
            {
                characterTypeMatch = true; // 제한이 없으면 모든 캐릭터 타입에 적용 가능
            }
            else
            {
                foreach (CharacterType type in applicableCharacterTypes)
                {
                    if (type == characterType)
                    {
                        characterTypeMatch = true;
                        break;
                    }
                }
            }
            
            // 감각 타입 확인 (플레이어 캐릭터인 경우만)
            bool senseTypeMatch = false;
            if (characterType != CharacterType.Player)
            {
                senseTypeMatch = true; // 플레이어가 아니면 감각 타입 무시
            }
            else
            {
                if (applicableSenseTypes == null || applicableSenseTypes.Length == 0)
                {
                    senseTypeMatch = true; // 제한이 없으면 모든 감각 타입에 적용 가능
                }
                else
                {
                    foreach (SenseType type in applicableSenseTypes)
                    {
                        if (type == senseType)
                        {
                            senseTypeMatch = true;
                            break;
                        }
                    }
                }
            }
            
            return characterTypeMatch && senseTypeMatch;
        }
    }
    
    /// <summary>
    /// 상태 효과 데이터 래퍼 (직렬화 가능)
    /// </summary>
    [System.Serializable]
    public struct StatusEffectDataWrapper
    {
        public StatusEffectType effectType;
        public int magnitude;
        public int duration;
        
        public StatusEffect.StatusEffectData ToStatusEffectData()
        {
            return new StatusEffect.StatusEffectData(effectType, magnitude, duration);
        }
    }
}
