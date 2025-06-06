using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 모든 프로젝트에서 공통으로 사용할 수 있는 타입 변환 유틸리티
    /// </summary>
    public static class GlobalTypeHelper
    {
        /// <summary>
        /// StatusEffectType에서 이름 문자열로 변환
        /// </summary>
        public static string GetEffectName(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.None:
                    return "None";
                case StatusEffectType.Amplify:
                    return "Amplify";
                case StatusEffectType.Resonance:
                    return "Resonance";
                case StatusEffectType.Mark:
                    return "Mark";
                case StatusEffectType.Bleed:
                    return "Bleed";
                case StatusEffectType.Counter:
                    return "Counter";
                case StatusEffectType.Crush:
                    return "Crush";
                case StatusEffectType.Curse:
                    return "Curse";
                case StatusEffectType.Seal:
                    return "Seal";
                case StatusEffectType.Poison:
                    return "Poison";
                case StatusEffectType.Burn:
                    return "Burn";
                case StatusEffectType.MultiAttack:
                    return "MultiAttack";
                case StatusEffectType.Wound:
                    return "Wound";
                case StatusEffectType.Fracture:
                    return "Fracture";
                case StatusEffectType.Regeneration:
                    return "Regeneration";
                default:
                    return "Unknown";
            }
        }

        /// <summary>
        /// 문자열에서 StatusEffectType으로 변환
        /// </summary>
        public static StatusEffectType GetEffectTypeFromName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return StatusEffectType.None;

            if (Enum.TryParse<StatusEffectType>(name, true, out StatusEffectType type))
                return type;
            
            // 문자열 매칭으로 시도
            switch (name.ToLower())
            {
                case "amplify":
                    return StatusEffectType.Amplify;
                case "resonance":
                    return StatusEffectType.Resonance;
                case "mark":
                    return StatusEffectType.Mark;
                case "bleed":
                    return StatusEffectType.Bleed;
                case "counter":
                    return StatusEffectType.Counter;
                case "crush":
                    return StatusEffectType.Crush;
                case "curse":
                    return StatusEffectType.Curse;
                case "seal":
                    return StatusEffectType.Seal;
                case "poison":
                    return StatusEffectType.Poison;
                case "burn":
                    return StatusEffectType.Burn;
                case "multiattack":
                case "multi-attack":
                case "multi attack":
                    return StatusEffectType.MultiAttack;
                case "wound":
                    return StatusEffectType.Wound;
                case "fracture":
                    return StatusEffectType.Fracture;
                case "regeneration":
                case "regen":
                    return StatusEffectType.Regeneration;
                default:
                    Debug.LogWarning($"Unknown effect name: {name}");
                    return StatusEffectType.None;
            }
        }

        /// <summary>
        /// StatusEffectType에서 색상으로 변환
        /// </summary>
        public static Color GetEffectColor(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Amplify:
                    return new Color(0.2f, 0.8f, 0.2f); // 초록색
                case StatusEffectType.Resonance:
                    return new Color(0.2f, 0.6f, 1.0f); // 밝은 파랑
                case StatusEffectType.Mark:
                    return new Color(1.0f, 0.8f, 0.0f); // 노란색
                case StatusEffectType.Bleed:
                    return new Color(0.9f, 0.2f, 0.2f); // 빨간색
                case StatusEffectType.Counter:
                    return new Color(0.9f, 0.5f, 0.1f); // 주황색
                case StatusEffectType.Crush:
                    return new Color(0.5f, 0.5f, 0.5f); // 회색
                case StatusEffectType.Curse:
                    return new Color(0.5f, 0.0f, 0.5f); // 보라색
                case StatusEffectType.Seal:
                    return new Color(0.4f, 0.0f, 0.8f); // 어두운 보라
                case StatusEffectType.Poison:
                    return new Color(0.2f, 0.7f, 0.0f); // 어두운 초록
                case StatusEffectType.Burn:
                    return new Color(1.0f, 0.4f, 0.0f); // 주황색
                case StatusEffectType.MultiAttack:
                    return new Color(0.7f, 0.0f, 0.0f); // 어두운 빨강
                case StatusEffectType.Wound:
                    return new Color(0.8f, 0.1f, 0.1f); // 진한 빨강
                case StatusEffectType.Fracture:
                    return new Color(0.6f, 0.4f, 0.2f); // 갈색
                case StatusEffectType.Regeneration:
                    return new Color(0.0f, 0.8f, 0.4f); // 민트색
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 패턴 타입 문자열 변환
        /// </summary>
        public static string GetPatternName(PatternType type)
        {
            switch (type)
            {
                case PatternType.None:
                    return "None";
                case PatternType.Consecutive2:
                    return "Two Consecutive";
                case PatternType.Consecutive3:
                    return "Three Consecutive";
                case PatternType.Consecutive4:
                    return "Four Consecutive";
                case PatternType.Consecutive5:
                    return "Five Consecutive";
                case PatternType.AllOfOne:
                    return "All Same Face";
                case PatternType.Alternating:
                    return "Alternating";
                default:
                    return "Unknown Pattern";
            }
        }
        
        /// <summary>
        /// 상태 효과 설명 가져오기
        /// </summary>
        public static string GetEffectDescription(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.None:
                    return "효과 없음";
                case StatusEffectType.Amplify:
                    return "공격력/방어력을 +1 획득하며 최대 10까지 누적된다. 증폭 1당 매 턴 1 자해 피해";
                case StatusEffectType.Resonance:
                    return "누적된 후 2턴 뒤 공명 수치만큼 즉시 피해";
                case StatusEffectType.Mark:
                    return "적에게 부여한 수치만큼 다음번 자신의 공격 횟수를 증가 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Bleed:
                    return "방어력 무시 지속 피해 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Counter:
                    return "피격 시 수치당 적에게 2 고정 피해 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Crush:
                    return "방어력 1.5배 감소 (지속형 상태 효과)";
                case StatusEffectType.Curse:
                    return "턴마다 지속 고정 피해 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Seal:
                    return "동전 n개 무작위 봉쇄 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Poison:
                    return "턴마다 지속 피해 (턴 마다 누적 수치 1 감소)";
                case StatusEffectType.Burn:
                    return "즉시 피해 후 소멸";
                case StatusEffectType.MultiAttack:
                    return "여러번 연속 공격";
                case StatusEffectType.Wound:
                    return "받는 피해량 1.5배 증가";
                case StatusEffectType.Fracture:
                    return "공격력 1.5배 감소";
                case StatusEffectType.Regeneration:
                    return "턴마다 체력 회복";
                default:
                    return "알 수 없는 효과";
            }
        }
        
        /// <summary>
        /// 상태효과를 버프/디버프로 구분
        /// </summary>
        public static bool IsDebuff(StatusEffectType type)
        {
            switch (type)
            {
                case StatusEffectType.Bleed:
                case StatusEffectType.Crush:
                case StatusEffectType.Curse:
                case StatusEffectType.Seal:
                case StatusEffectType.Poison:
                case StatusEffectType.Burn:
                case StatusEffectType.Wound:
                case StatusEffectType.Fracture:
                    return true;
                
                case StatusEffectType.Amplify:
                case StatusEffectType.Resonance:
                case StatusEffectType.Mark:
                case StatusEffectType.Counter:
                case StatusEffectType.MultiAttack:
                case StatusEffectType.Regeneration:
                    return false;
                
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// 캐릭터 타입 이름 가져오기
        /// </summary>
        public static string GetCharacterTypeName(CharacterType type)
        {
            switch (type)
            {
                case CharacterType.None:
                    return "None";
                case CharacterType.Player:
                    return "Player";
                case CharacterType.Normal:
                    return "Normal Enemy";
                case CharacterType.Elite:
                    return "Elite Enemy";
                case CharacterType.MiniBoss:
                    return "Mini Boss";
                case CharacterType.Boss:
                    return "Boss";
                case CharacterType.Windows:
                    return "Environment";
                case CharacterType.Neutral:
                    return "Neutral NPC";
                case CharacterType.Vendor:
                    return "Vendor";
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// 감각 타입 이름 가져오기
        /// </summary>
        public static string GetSenseTypeName(SenseType type)
        {
            switch (type)
            {
                case SenseType.None:
                    return "None";
                case SenseType.Auditory:
                    return "Auditory (Hearing)";
                case SenseType.Olfactory:
                    return "Olfactory (Smell)";
                case SenseType.Tactile:
                    return "Tactile (Touch)";
                case SenseType.Spiritual:
                    return "Spiritual (Soul)";
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// 플레이어 캐릭터 타입 이름 가져오기
        /// </summary>
        public static string GetPlayerTypeName(PlayerCharacterType type)
        {
            switch (type)
            {
                case PlayerCharacterType.Warrior:
                    return "Warrior";
                case PlayerCharacterType.Rogue:
                    return "Rogue";
                case PlayerCharacterType.Mage:
                    return "Mage";
                case PlayerCharacterType.Tank:
                    return "Tank";
                default:
                    return "Unknown";
            }
        }
        
        /// <summary>
        /// 코인 면 이름 가져오기
        /// </summary>
        public static string GetCoinFaceName(CoinFace face)
        {
            switch (face)
            {
                case CoinFace.None:
                    return "None";
                case CoinFace.Head:
                    return "Head (Attack)";
                case CoinFace.Tail:
                    return "Tail (Defense)";
                default:
                    return "Unknown";
            }
        }
    }
}