using MonoChrome.StatusEffects;
using UnityEngine;


namespace MonoChrome
{
    /// <summary>
    /// 상태 효과 데이터를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Status Effect", menuName = "MonoChrome/Combat/Status Effect")]
    public class StatusEffectSO : ScriptableObject
    {
        [Header("기본 정보")]
        public string effectName;
        [TextArea(2, 4)]
        public string description;
        public MonoChrome.StatusEffectType effectType;
        public Sprite icon;
        
        [Header("시각 효과")]
        public Color effectColor = Color.white;
        
        [Header("기본값")]
        public int defaultMagnitude = 1;
        public int defaultDuration = 1;
        
        [Header("효과 동작")]
        public bool stackable = true;
        public bool refreshDuration = true;
        public bool ignoreDefense = false;
        
        [Header("특수 효과")]
        [TextArea(1, 3)]
        public string specialEffectDescription;
        
        /// <summary>
        /// 효과 생성 헬퍼 메서드
        /// </summary>
        public StatusEffect CreateEffect(int magnitude = -1, int duration = -1, Character source = null)
        {
            int finalMagnitude = magnitude > 0 ? magnitude : defaultMagnitude;
            int finalDuration = duration > 0 ? duration : defaultDuration;
            
            return new StatusEffect(effectType, finalMagnitude, finalDuration, source);
        }
    }
}