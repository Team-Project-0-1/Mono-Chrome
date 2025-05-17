using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.StatusEffects
{
    /// <summary>
    /// 상태 효과 클래스
    /// 캐릭터에게 적용되는 다양한 전투 효과를 정의한다.
    /// </summary>
    [System.Serializable]
    public class StatusEffect
    {
        // 효과 유형
        private StatusEffectType _effectType;
        
        // 효과 강도/수치
        private int _magnitude;
        
        // 남은 지속 시간 (턴 수)
        private int _remainingDuration;
        
        // 효과 생성자/적용자
        private Character _source;
        
        // 프로퍼티
        public StatusEffectType EffectType => _effectType;
        public int Magnitude { get => _magnitude; set => _magnitude = value; }
        public int RemainingDuration { get => _remainingDuration; set => _remainingDuration = value; }
        public Character Source => _source;
        
        /// <summary>
        /// 상태 효과 생성자
        /// </summary>
        /// <param name="type">효과 유형</param>
        /// <param name="magnitude">효과 강도</param>
        /// <param name="duration">지속 시간</param>
        /// <param name="source">효과 생성자</param>
        public StatusEffect(StatusEffectType type, int magnitude, int duration, Character source = null)
        {
            _effectType = type;
            _magnitude = magnitude;
            _remainingDuration = duration;
            _source = source;
        }
        
        /// <summary>
        /// 보조 생성자 - 문자열 이름으로 생성
        /// </summary>
        public StatusEffect(string name, int magnitude, int duration, Character source = null)
            : this(ParseEffectType(name), magnitude, duration, source)
        {
        }
        
        /// <summary>
        /// 문자열에서 상태 효과 타입 파싱
        /// </summary>
        private static StatusEffectType ParseEffectType(string name)
        {
            if (string.IsNullOrEmpty(name)) return StatusEffectType.None;
            
            if (System.Enum.TryParse<StatusEffectType>(name, true, out StatusEffectType result))
            {
                return result;
            }
            
            // 기본값
            return StatusEffectType.None;
        }
        
        /// <summary>
        /// StatusEffectType으로부터의 명시적 변환 연산자
        /// </summary>
        public static explicit operator StatusEffect(StatusEffectType effectType)
        {
            return new StatusEffect(effectType, 1, 1);
        }
        
        /// <summary>
        /// 문자열로부터의 명시적 변환 연산자
        /// </summary>
        public static explicit operator StatusEffect(string effectName)
        {
            return new StatusEffect(effectName, 1, 1);
        }
        
        /// <summary>
        /// CharacterType으로부터의 명시적 변환 연산자
        /// </summary>
        public static explicit operator StatusEffect(CharacterType characterType)
        {
            // 캐릭터 타입에 따른 기본 상태 효과 반환
            switch (characterType)
            {
                case CharacterType.Normal:
                    return new StatusEffect(StatusEffectType.None, 1, 1);
                case CharacterType.Elite:
                    return new StatusEffect(StatusEffectType.Amplify, 2, 2);
                case CharacterType.MiniBoss:
                    return new StatusEffect(StatusEffectType.Resonance, 2, 3);
                case CharacterType.Boss:
                    return new StatusEffect(StatusEffectType.Curse, 3, 3);
                default:
                    return new StatusEffect(StatusEffectType.None, 1, 1);
            }
        }
        
        /// <summary>
        /// 효과의 문자열 표현
        /// </summary>
        public override string ToString()
        {
            return $"{_effectType} ({_magnitude}) - {_remainingDuration}턴 남음";
        }
        
        /// <summary>
        /// 상태 효과를 복제합니다.
        /// </summary>
        public StatusEffect Clone()
        {
            return new StatusEffect(_effectType, _magnitude, _remainingDuration, _source);
        }
        
        /// <summary>
        /// 상태 효과의 지속 시간을 감소시킵니다.
        /// </summary>
        public void DecreaseDuration()
        {
            if (_remainingDuration > 0)
            {
                _remainingDuration--;
            }
        }
        
        /// <summary>
        /// 상태 효과의 강도/수치를 증가시킵니다.
        /// </summary>
        public void IncreaseMagnitude(int amount)
        {
            _magnitude += amount;
        }
        
        /// <summary>
        /// 상태 효과가 만료되었는지 확인합니다.
        /// </summary>
        public bool IsExpired()
        {
            return _remainingDuration <= 0;
        }
        
        /// <summary>
        /// 상태 효과 데이터를 나타내는 구조체
        /// </summary>
        [System.Serializable]
        public struct StatusEffectData
        {
            public StatusEffectType EffectType;
            public int Magnitude;
            public int Duration;
            
            public StatusEffectData(StatusEffectType type, int magnitude, int duration)
            {
                EffectType = type;
                Magnitude = magnitude;
                Duration = duration;
            }
        }
    }
}