using System;
using UnityEngine;

namespace MonoChrome.StatusEffects
{
    /// <summary>
    /// StatusEffect 확장 메서드 모음
    /// </summary>
    public static class StatusEffectExtensions
    {
        /// <summary>
        /// 이름으로부터 StatusEffect 변환
        /// </summary>
        public static StatusEffect ConvertFromName(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return new StatusEffect(StatusEffectType.None, 1, 1);
            
            if (Enum.TryParse<StatusEffectType>(effectName, true, out StatusEffectType type))
                return new StatusEffect(type, 1, 1);
            
            return new StatusEffect(StatusEffectType.None, 1, 1);
        }
        
        /// <summary>
        /// 상태 효과의 한글 이름 가져오기
        /// </summary>
        public static string GetKoreanName(this StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.None:
                    return "없음";
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
                case StatusEffectType.Poison:
                    return "중독";
                case StatusEffectType.Burn:
                    return "화상";
                case StatusEffectType.MultiAttack:
                    return "연격";
                case StatusEffectType.Wound:
                    return "상처";
                case StatusEffectType.Fracture:
                    return "골절";
                case StatusEffectType.Regeneration:
                    return "재생";
                default:
                    return "알 수 없음";
            }
        }
        
        /// <summary>
        /// 상태 효과 설명 가져오기
        /// </summary>
        public static string GetDescription(this StatusEffectType effectType)
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
                    return "얻는 공격력 1.5배 감소";
                case StatusEffectType.Regeneration:
                    return "즉시 체력 회복";
                default:
                    return "알 수 없는 효과";
            }
        }
        
        /// <summary>
        /// 상태 효과 색상 가져오기
        /// </summary>
        public static Color GetColor(this StatusEffectType effectType)
        {
            switch (effectType)
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
                    return new Color(0.8f, 0.0f, 0.0f); // 피의 빨간색
                case StatusEffectType.Fracture:
                    return new Color(0.5f, 0.3f, 0.1f); // 갈색
                case StatusEffectType.Regeneration:
                    return new Color(0.0f, 0.9f, 0.4f); // 청록색
                default:
                    return Color.white;
            }
        }
        
        /// <summary>
        /// 효과 지속시간 문자열 가져오기
        /// </summary>
        public static string GetDurationText(int duration)
        {
            if (duration <= 0)
                return "만료됨";
            else if (duration == 1)
                return "1턴 남음";
            else
                return $"{duration}턴 남음";
        }
        
        /// <summary>
        /// 상태 효과가 버프인지 확인
        /// </summary>
        public static bool IsBuff(this StatusEffectType effectType)
        {
            return effectType == StatusEffectType.Amplify ||
                   effectType == StatusEffectType.Counter ||
                   effectType == StatusEffectType.Regeneration;
        }
        
        /// <summary>
        /// 상태 효과가 디버프인지 확인
        /// </summary>
        public static bool IsDebuff(this StatusEffectType effectType)
        {
            return effectType == StatusEffectType.Bleed ||
                   effectType == StatusEffectType.Poison ||
                   effectType == StatusEffectType.Curse ||
                   effectType == StatusEffectType.Crush ||
                   effectType == StatusEffectType.Seal ||
                   effectType == StatusEffectType.Wound ||
                   effectType == StatusEffectType.Fracture;
        }
    }
}