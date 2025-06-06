using System;
using UnityEngine;
using MonoChrome;
using MonoChrome.StatusEffects;

namespace MonoChrome.Extensions
{
    /// <summary>
    /// 열거형 확장 메서드 모음
    /// 열거형 간의 변환 및 유용한 도우미 메서드를 제공합니다.
    /// </summary>
    public static class EnumExtensions
    {
        #region CharacterType 관련 확장 메서드
        
        /// <summary>
        /// 일반 몬스터 타입인지 확인합니다.
        /// </summary>
        public static bool IsNormal(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Normal;
        }
        
        /// <summary>
        /// 엘리트 몬스터 타입인지 확인합니다.
        /// </summary>
        public static bool IsElite(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Elite;
        }
        
        /// <summary>
        /// 미니보스 타입인지 확인합니다.
        /// </summary>
        public static bool IsMiniBoss(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.MiniBoss;
        }
        
        /// <summary>
        /// 보스 타입인지 확인합니다.
        /// </summary>
        public static bool IsBoss(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Boss;
        }
        
        /// <summary>
        /// 플레이어 타입인지 확인합니다.
        /// </summary>
        public static bool IsPlayer(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Player;
        }
        
        /// <summary>
        /// 중립 NPC 타입인지 확인합니다.
        /// </summary>
        public static bool IsNeutral(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Neutral;
        }
        
        /// <summary>
        /// 상인 타입인지 확인합니다.
        /// </summary>
        public static bool IsVendor(this MonoChrome.CharacterType type)
        {
            return type == MonoChrome.CharacterType.Vendor;
        }
        
        /// <summary>
        /// Core.CharacterType을 Characters.CharacterType으로 변환합니다.
        /// </summary>
        public static MonoChrome.CharacterType ToCharactersType(this MonoChrome.CharacterType type)
        {
            return (MonoChrome.CharacterType)((int)type);
        }
        
        /// <summary>
        /// Characters.CharacterType을 Core.CharacterType으로 변환합니다.
        /// </summary>
        public static MonoChrome.CharacterType ToCoreType(this MonoChrome.CharacterType type)
        {
            return (MonoChrome.CharacterType)((int)type);
        }
        
        #endregion
        
        #region SenseType 관련 확장 메서드
        
        /// <summary>
        /// Core.SenseType을 Characters.SenseType으로 변환합니다.
        /// </summary>
        public static MonoChrome.SenseType ToCharactersSenseType(this MonoChrome.SenseType type)
        {
            return (MonoChrome.SenseType)((int)type);
        }
        
        /// <summary>
        /// Characters.SenseType을 Core.SenseType으로 변환합니다.
        /// </summary>
        public static MonoChrome.SenseType ToCoreSenseType(this MonoChrome.SenseType type)
        {
            return (MonoChrome.SenseType)((int)type);
        }
        
        #endregion
        
        #region PatternType 관련 확장 메서드
        
        /// <summary>
        /// Core.PatternType을 Combat.PatternType으로 변환합니다.
        /// </summary>
        public static MonoChrome.PatternType ToCombatPatternType(this MonoChrome.PatternType type)
        {
            return (MonoChrome.PatternType)((int)type);
        }
        
        /// <summary>
        /// Combat.PatternType을 Core.PatternType으로 변환합니다.
        /// </summary>
        public static MonoChrome.PatternType ToCorePatternType(this MonoChrome.PatternType type)
        {
            return (MonoChrome.PatternType)((int)type);
        }
        
        #endregion
        
        #region StatusEffectType 관련 확장 메서드
        
        /// <summary>
        /// Core.StatusEffectType을 StatusEffects.StatusEffectType으로 변환합니다.
        /// </summary>
        public static MonoChrome.StatusEffectType ToStatusEffectsType(this MonoChrome.StatusEffectType type)
        {
            return (MonoChrome.StatusEffectType)((int)type);
        }
        
        /// <summary>
        /// StatusEffects.StatusEffectType을 Core.StatusEffectType으로 변환합니다.
        /// </summary>
        public static MonoChrome.StatusEffectType ToCoreStatusEffectType(this MonoChrome.StatusEffectType type)
        {
            return (MonoChrome.StatusEffectType)((int)type);
        }
        
        /// <summary>
        /// StatusEffectType에서 StatusEffect 객체를 생성합니다.
        /// </summary>
        public static StatusEffect ToStatusEffect(this StatusEffectType type, int magnitude = 1, int duration = 1)
        {
            return new StatusEffect(type, magnitude, duration);
        }
        
        /// <summary>
        /// EffectType에서 StatusEffect 객체를 생성합니다.
        /// </summary>
        // public static StatusEffect ToStatusEffect(this EffectType type, int magnitude = 1, int duration = 1)
        // {
        //     StatusEffectType effectType = (StatusEffectType)((int)type);
        //     return new StatusEffect(effectType, magnitude, duration);
        // }
        
        #endregion
    }
}
