using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 던전에서 사용되는 상태 효과 클래스
    /// </summary>
    [System.Serializable]
    public class DungeonStatusEffect
    {
        public string name;
        public int stacks;
        public int duration;
        public Sprite icon;

        public DungeonStatusEffect(string name, int stacks, int duration)
        {
            this.name = name;
            this.stacks = stacks;
            this.duration = duration;
            this.icon = null;
        }

        // StatusEffect에서 변환 생성자
        public DungeonStatusEffect(StatusEffects.StatusEffect effect)
        {
            if (effect != null)
            {
                this.name = effect.EffectType.ToString();
                this.stacks = effect.Magnitude;
                this.duration = effect.RemainingDuration;
                // 아이콘은 나중에 별도로 로드해야 함
            }
            else
            {
                this.name = "Unknown";
                this.stacks = 0;
                this.duration = 0;
            }
        }
    }
}