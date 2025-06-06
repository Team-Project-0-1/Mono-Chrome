using System;

// 명시적 네임스페이스 참조를 위한 using alias
// 'CoreCharacterType'을 alias로 사용하지 않고 직접 CharacterType 사용
using CombatPatternType = MonoChrome.PatternType;
using SEStatusEffect = MonoChrome.StatusEffects.StatusEffect;
using SEStatusEffectType = MonoChrome.StatusEffectType;

namespace MonoChrome
{
    /// <summary>
    /// 타입 변환을 위한 인터페이스
    /// </summary>
    public interface ITypeConverter<T>
    {
        T Convert();
    }
    
    /// <summary>
    /// 기본 타입 변환기 클래스
    /// </summary>
    public static class TypeConverter
    {
        // CharacterType -> CombatSystem (updated to use new architecture)
        public static Systems.Combat.CombatSystem ToCombatSystem(this CharacterType type)
        {
            return UnityEngine.Object.FindObjectOfType<Systems.Combat.CombatSystem>();
        }
        
        // CharacterType -> StatusEffect
        public static SEStatusEffect ToStatusEffect(this CharacterType type, int magnitude = 1, int duration = 1)
        {
            return new SEStatusEffect(SEStatusEffectType.None, magnitude, duration);
        }
        
        // EffectType -> StatusEffect 
        public static SEStatusEffect ToStatusEffect(this StatusEffectType type, int magnitude = 1, int duration = 1)
        {
            SEStatusEffectType statusType = ToStatusEffectType(type);
            return new SEStatusEffect(statusType, magnitude, duration);
        }
        
        // StatusEffectType -> EffectType
        public static StatusEffectType ToEffectType(this SEStatusEffectType type)
        {
            switch (type)
            {
                case SEStatusEffectType.Amplify: return StatusEffectType.Amplify;
                case SEStatusEffectType.Resonance: return StatusEffectType.Resonance;
                case SEStatusEffectType.Mark: return StatusEffectType.Mark;
                case SEStatusEffectType.Bleed: return StatusEffectType.Bleed;
                case SEStatusEffectType.Counter: return StatusEffectType.Counter;
                case SEStatusEffectType.Crush: return StatusEffectType.Crush;
                case SEStatusEffectType.Curse: return StatusEffectType.Curse;
                case SEStatusEffectType.Seal: return StatusEffectType.Seal;
                case SEStatusEffectType.Poison: return StatusEffectType.Poison;
                case SEStatusEffectType.Burn: return StatusEffectType.Burn;
                case SEStatusEffectType.MultiAttack: return StatusEffectType.MultiAttack;
                default: return StatusEffectType.None;
            }
        }
        
        // EffectType -> StatusEffectType
        public static SEStatusEffectType ToStatusEffectType(this StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Amplify: return SEStatusEffectType.Amplify;
                case StatusEffectType.Resonance: return SEStatusEffectType.Resonance;
                case StatusEffectType.Mark: return SEStatusEffectType.Mark;
                case StatusEffectType.Bleed: return SEStatusEffectType.Bleed;
                case StatusEffectType.Counter: return SEStatusEffectType.Counter;
                case StatusEffectType.Crush: return SEStatusEffectType.Crush;
                case StatusEffectType.Curse: return SEStatusEffectType.Curse;
                case StatusEffectType.Seal: return SEStatusEffectType.Seal;
                case StatusEffectType.Poison: return SEStatusEffectType.Poison;
                case StatusEffectType.Burn: return SEStatusEffectType.Burn;
                case StatusEffectType.MultiAttack: return SEStatusEffectType.MultiAttack;
                default: return SEStatusEffectType.None;
            }
        }
        
        // PatternType -> CharacterType
        public static MonoChrome.CharacterType ToCharacterType(this CombatPatternType type)
        {
            switch (type)
            {
                case CombatPatternType.Consecutive2: return MonoChrome.CharacterType.Normal;
                case CombatPatternType.Consecutive3: return MonoChrome.CharacterType.Elite;
                case CombatPatternType.Consecutive4: return MonoChrome.CharacterType.MiniBoss;
                case CombatPatternType.Consecutive5: return MonoChrome.CharacterType.Boss;
                case CombatPatternType.AllOfOne: return MonoChrome.CharacterType.Boss;
                case CombatPatternType.Alternating: return MonoChrome.CharacterType.Boss;
                default: return MonoChrome.CharacterType.Normal;
            }
        }
        
        // CharacterType -> PatternType
        public static CombatPatternType ToPatternType(this MonoChrome.CharacterType type)
        {
            switch (type)
            {
                case CharacterType.Normal: return CombatPatternType.Consecutive2;
                case CharacterType.Elite: return CombatPatternType.Consecutive3;
                case CharacterType.MiniBoss: return CombatPatternType.Consecutive4;
                case CharacterType.Boss: return CombatPatternType.Consecutive5;
                default: return CombatPatternType.Consecutive2;
            }
        }
    }
}