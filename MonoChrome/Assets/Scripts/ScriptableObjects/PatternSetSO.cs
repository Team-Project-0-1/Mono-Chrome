using UnityEngine;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;

namespace MonoChrome
{
    /// <summary>
    /// 패턴(족보) 세트를 정의하는 ScriptableObject
    /// 특정 캐릭터 타입이나 감각 타입에 따른 패턴 그룹을 관리
    /// </summary>
    [CreateAssetMenu(fileName = "New Pattern Set", menuName = "MonoChrome/Combat/Pattern Set")]
    public class PatternSetSO : ScriptableObject
    {
        [Header("기본 정보")]
        public string setName;
        [TextArea(2, 4)]
        public string description;
        
        [Header("대상 캐릭터")]
        public CharacterType targetCharacterType;
        public SenseType targetSenseType; // 플레이어 캐릭터만 해당
        
        [Header("패턴 목록")]
        public PatternSO[] patterns;
        
        /// <summary>
        /// 이 패턴 세트에서 Pattern 객체 목록 가져오기
        /// </summary>
        public List<Pattern> GetPatterns()
        {
            List<Pattern> result = new List<Pattern>();
            
            if (patterns != null)
            {
                foreach (PatternSO patternSO in patterns)
                {
                    if (patternSO != null)
                    {
                        result.Add(patternSO.ToPattern());
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 이 패턴 세트가 특정 캐릭터 타입과 감각 타입에 적용 가능한지 확인
        /// </summary>
        public bool IsApplicableTo(CharacterType characterType, SenseType senseType = SenseType.Auditory)
        {
            // 캐릭터 타입 확인
            if (targetCharacterType != characterType)
            {
                return false;
            }
            
            // 플레이어 캐릭터인 경우 감각 타입도 확인
            if (characterType == CharacterType.Player && targetSenseType != senseType)
            {
                return false;
            }
            
            return true;
        }
    }
}
