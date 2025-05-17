using System;
using UnityEngine;

namespace MonoChrome.Utils
{
    /// <summary>
    /// CharacterType 열거형에 대한 확장 메서드
    /// 모든 CharacterType 관련 확장 메서드를 이 클래스로 통합했습니다.
    /// </summary>
    public static class CharacterTypeExtensions
    {
        /// <summary>
        /// 일반 몬스터 타입인지 확인
        /// </summary>
        public static bool IsNormal(this CharacterType type)
        {
            return type == CharacterType.Normal;
        }
        
        /// <summary>
        /// 엘리트 몬스터 타입인지 확인
        /// </summary>
        public static bool IsElite(this CharacterType type)
        {
            return type == CharacterType.Elite;
        }
        
        /// <summary>
        /// 보스 몬스터 타입인지 확인
        /// </summary>
        public static bool IsBoss(this CharacterType type)
        {
            return type == CharacterType.Boss;
        }
        
        /// <summary>
        /// 미니보스 타입인지 확인
        /// </summary>
        public static bool IsMiniBoss(this CharacterType type)
        {
            return type == CharacterType.MiniBoss;
        }
        
        /// <summary>
        /// Windows 타입인지 확인
        /// </summary>
        public static bool IsWindows(this CharacterType type)
        {
            return type == CharacterType.Windows;
        }
        
        /// <summary>
        /// 플레이어 타입인지 확인
        /// </summary>
        public static bool IsPlayer(this CharacterType type)
        {
            return type == CharacterType.Player;
        }
        
        /// <summary>
        /// Neutral 타입인지 확인
        /// </summary>
        public static bool IsNeutral(this CharacterType type)
        {
            return type == CharacterType.Neutral;
        }
        
        /// <summary>
        /// Vendor 타입인지 확인
        /// </summary>
        public static bool IsVendor(this CharacterType type)
        {
            return type == CharacterType.Vendor;
        }
        
        /// <summary>
        /// CharacterType을 PatternType으로 변환
        /// </summary>
        public static PatternType ToPatternType(this CharacterType type)
        {
            switch (type)
            {
                case CharacterType.Normal:
                    return PatternType.Consecutive2;
                case CharacterType.Elite:
                    return PatternType.Consecutive3;
                case CharacterType.MiniBoss:
                    return PatternType.Consecutive4;
                case CharacterType.Boss:
                    return PatternType.Consecutive5;
                default:
                    return PatternType.Consecutive2;
            }
        }
        
        /// <summary>
        /// 디버그용 출력 메서드
        /// </summary>
        public static string ToString(this CharacterType type, bool verbose = false)
        {
            string result = type.ToString();
            
            if (verbose)
            {
                switch (type)
                {
                    case CharacterType.Player:
                        result += " (플레이어)";
                        break;
                    case CharacterType.Normal:
                        result += " (일반 몬스터)";
                        break;
                    case CharacterType.Elite:
                        result += " (엘리트 몬스터)";
                        break;
                    case CharacterType.MiniBoss:
                        result += " (미니보스)";
                        break;
                    case CharacterType.Boss:
                        result += " (보스)";
                        break;
                }
            }
            
            return result;
        }
    }
}