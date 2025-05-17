using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 캐릭터 데이터를 정의하는 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "New Character Data", menuName = "MonoChrome/Characters/Character Data")]
    public class CharacterDataSO : ScriptableObject
    {
        [Header("기본 정보")]
        public string characterName;
        [TextArea(2, 4)]
        public string description;
        public CharacterType characterType;
        public SenseType senseType; // 플레이어 캐릭터만 해당
        public Sprite portrait;
        
        [Header("기본 스탯")]
        public int maxHealth = 100;
        public int baseAttackPower = 10;
        public int baseDefensePower = 5;
        public int skillCooldown = 3;
        
        [Header("상태 효과")]
        public StatusEffectType primaryEffectType;
        public StatusEffectType secondaryEffectType;
        
        [Header("패턴")]
        public PatternSetSO patternSet;
        
        [Header("액티브 스킬")]
        public string skillName;
        [TextArea(2, 4)]
        public string skillDescription;
        public ActiveSkillType skillType;
        
        [Header("적 특성")]
        public bool isEnemyBoss = false;
        public int behaviorPriority = 0; // 적 AI가 행동 결정 시 우선순위
        
        /// <summary>
        /// PlayerCharacter 객체 생성
        /// </summary>
        public PlayerCharacter CreatePlayerCharacter()
        {
            if (characterType != CharacterType.Player)
            {
                Debug.LogError($"CharacterDataSO: Cannot create player character from non-player data '{name}'");
                return null;
            }
            
            return new PlayerCharacter(
                characterName,
                senseType,
                maxHealth,
                baseAttackPower,
                baseDefensePower
            );
        }
        
        /// <summary>
        /// EnemyCharacter 객체 생성
        /// </summary>
        public EnemyCharacter CreateEnemyCharacter()
        {
            if (characterType == CharacterType.Player)
            {
                Debug.LogError($"CharacterDataSO: Cannot create enemy character from player data '{name}'");
                return null;
            }
            
            return new EnemyCharacter(
                characterName,
                characterType,
                maxHealth,
                baseAttackPower,
                baseDefensePower,
                primaryEffectType,
                secondaryEffectType
            );
        }
    }
}
