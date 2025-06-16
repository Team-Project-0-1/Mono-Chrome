using UnityEngine;
using MonoChrome.StatusEffects;
using MonoChrome.Systems.Combat;
using System.Collections.Generic;

namespace MonoChrome.Data
{
    /// <summary>
    /// 몬스터 전용 패턴 데이터를 정의하는 ScriptableObject
    /// AI 시스템에서 사용하며, 몬스터의 행동 패턴과 의도를 표현한다.
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterPattern_", menuName = "MonoChrome/Combat/Monster Pattern")]
    public class MonsterPatternSO : ScriptableObject
    {
        [Header("기본 정보")]
        [SerializeField] private string patternName;
        [SerializeField] private string monsterType;
        [SerializeField] private string intentType;
        [SerializeField] [TextArea(3, 5)] private string description;
        
        [Header("전투 효과")]
        [SerializeField] private int attackBonus = 0;
        [SerializeField] private int defenseBonus = 0;
        [SerializeField] private int additionalDamage = 0;
        
        [Header("상태 효과")]
        [SerializeField] private StatusEffectSO[] statusEffects;
        
        [Header("특수 효과")]
        [SerializeField] private bool ignoreDefense = false;
        [SerializeField] private int attackCount = 1;
        [SerializeField] private bool isAreaAttack = false;
        
        [Header("AI 설정")]
        [SerializeField] private int priority = 1;
        [SerializeField] private bool isSpecialPattern = false;
        [SerializeField] private string[] requiredConditions;
        
        // 프로퍼티
        public string PatternName => patternName;
        public string MonsterType => monsterType;
        public string IntentType => intentType;
        public string Description => description;
        public int AttackBonus => attackBonus;
        public int DefenseBonus => defenseBonus;
        public int AdditionalDamage => additionalDamage;
        public StatusEffectSO[] StatusEffects => statusEffects;
        public bool IgnoreDefense => ignoreDefense;
        public int AttackCount => attackCount;
        public bool IsAreaAttack => isAreaAttack;
        public int Priority => priority;
        public bool IsSpecialPattern => isSpecialPattern;
        public string[] RequiredConditions => requiredConditions;
        
        /// <summary>
        /// 패턴이 특정 조건을 만족하는지 확인
        /// </summary>
        /// <param name="condition">확인할 조건</param>
        /// <returns>조건 만족 여부</returns>
        public bool MeetsCondition(string condition)
        {
            if (requiredConditions == null) return true;
            
            foreach (string requiredCondition in requiredConditions)
            {
                if (requiredCondition.Equals(condition, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            
            return requiredConditions.Length == 0; // 조건이 없으면 항상 만족
        }
        
        /// <summary>
        /// 패턴의 총 위력 계산 (AI 선택 시 참고용)
        /// </summary>
        /// <returns>패턴의 총 위력</returns>
        public int CalculateTotalPower()
        {
            int totalPower = attackBonus + additionalDamage;
            
            // 상태 효과도 위력에 포함
            if (statusEffects != null)
            {
                totalPower += statusEffects.Length * 2;
            }
            
            // 공격 횟수 반영
            totalPower *= attackCount;
            
            // 방어 무시 보너스
            if (ignoreDefense)
            {
                totalPower = (int)(totalPower * 1.5f);
            }
            
            return totalPower;
        }
        
        /// <summary>
        /// 패턴이 공격 타입인지 확인
        /// </summary>
        /// <returns>공격 타입 여부</returns>
        public bool IsAttackPattern()
        {
            return intentType.Contains("공격") || intentType.Contains("타격") || intentType.Contains("피해") ||
                   attackBonus > 0 || additionalDamage > 0;
        }
        
        /// <summary>
        /// 패턴이 방어 타입인지 확인
        /// </summary>
        /// <returns>방어 타입 여부</returns>
        public bool IsDefensePattern()
        {
            return intentType.Contains("방어") || intentType.Contains("보호") || intentType.Contains("회복") ||
                   defenseBonus > 0;
        }
        
        /// <summary>
        /// 패턴이 상태 효과 타입인지 확인
        /// </summary>
        /// <returns>상태 효과 타입 여부</returns>
        public bool IsStatusPattern()
        {
            return intentType.Contains("상태") || intentType.Contains("저주") || intentType.Contains("독") ||
                   intentType.Contains("출혈") || intentType.Contains("봉인") ||
                   (statusEffects != null && statusEffects.Length > 0);
        }
        
        /// <summary>
        /// 패턴을 실행한다 (CombatManager에서 호출)
        /// </summary>
        /// <param name="caster">시전자</param>
        /// <param name="target">대상</param>
        public void ExecutePattern(Character caster, Character target)
        {
            if (caster == null || target == null)
            {
                Debug.LogError($"MonsterPatternSO: ExecutePattern - caster 또는 target이 null입니다");
                return;
            }
            
            Debug.Log($"{caster.CharacterName}이 {patternName} 패턴을 {target.CharacterName}에게 사용");
            
            // 공격 처리
            if (IsAttackPattern())
            {
                int totalDamage = caster.CurrentAttack + attackBonus + additionalDamage;
                
                for (int i = 0; i < attackCount; i++)
                {
                    target.TakeDamage(totalDamage, ignoreDefense);
                    
                    if (!target.IsAlive) break; // 대상이 죽으면 추가 공격 중단
                }
            }
            
            // 방어 처리
            if (IsDefensePattern())
            {
                caster.AddDefense(defenseBonus);
            }
            
            // 상태 효과 적용
            if (statusEffects != null && statusEffects.Length > 0)
            {
                foreach (StatusEffectSO statusEffect in statusEffects)
                {
                    if (statusEffect != null)
                    {
                        target.AddStatusEffect(statusEffect);
                    }
                }
            }
        }
        
        /// <summary>
        /// MonsterPatternSO를 Pattern 클래스로 변환
        /// </summary>
        /// <returns>변환된 Pattern 객체</returns>
        public Pattern ToPattern()
        {
            var pattern = new Pattern();
            pattern.Name = patternName;
            pattern.Description = description;
            pattern.ID = priority; // priority를 ID로 사용
            pattern.IsAttack = IsAttackPattern();
            pattern.AttackBonus = attackBonus;
            pattern.DefenseBonus = defenseBonus;
            pattern.SpecialEffect = description; // 설명을 특수 효과로 사용
            
            // PatternType은 의도 기반으로 결정
            pattern.PatternType = DeterminePatternType();
            pattern.PatternValue = IsAttackPattern(); // 공격이면 앞면(true), 방어면 뒷면(false)
            
            // 상태 효과 변환 (StatusEffectSO[] → StatusEffect.StatusEffectData[])
            if (statusEffects != null && statusEffects.Length > 0)
            {
                var statusEffectList = new List<StatusEffects.StatusEffect.StatusEffectData>();
                foreach (var effectSO in statusEffects)
                {
                    if (effectSO != null)
                    {
                        // StatusEffectSO에서 StatusEffectData로 변환
                        var effectData = new StatusEffects.StatusEffect.StatusEffectData(
                            effectSO.effectType, 
                            effectSO.defaultMagnitude, 
                            effectSO.defaultDuration
                        );
                        statusEffectList.Add(effectData);
                    }
                }
                pattern.StatusEffects = statusEffectList.ToArray();
            }
            else
            {
                pattern.StatusEffects = new StatusEffects.StatusEffect.StatusEffectData[0];
            }
            
            return pattern;
        }
        
        /// <summary>
        /// 의도 타입에 따른 PatternType 결정
        /// </summary>
        private PatternType DeterminePatternType()
        {
            switch (intentType)
            {
                case "Attack":
                    return PatternType.Consecutive2;
                case "Defense":
                    return PatternType.Consecutive2;
                case "Buff":
                case "Debuff":
                    return PatternType.Consecutive3;
                case "Special":
                    return PatternType.Consecutive4;
                case "Ultimate":
                    return PatternType.Consecutive5;
                default:
                    return PatternType.Consecutive2;
            }
        }
        
        /// <summary>
        /// 디버그 정보 출력
        /// </summary>
        /// <returns>패턴 정보 문자열</returns>
        public override string ToString()
        {
            return $"MonsterPattern[{patternName}] Type:{intentType} Monster:{monsterType} " +
                   $"Atk:{attackBonus} Def:{defenseBonus} Priority:{priority}";
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 에디터에서 유효성 검사
        /// </summary>
        private void OnValidate()
        {
            // 패턴 이름이 비어있으면 파일 이름으로 설정
            if (string.IsNullOrEmpty(patternName))
            {
                patternName = name;
            }
            
            // 몬스터 타입이 비어있으면 경고
            if (string.IsNullOrEmpty(monsterType))
            {
                Debug.LogWarning($"MonsterPatternSO '{name}': MonsterType이 설정되지 않았습니다.");
            }
            
            // 의도 타입이 비어있으면 경고
            if (string.IsNullOrEmpty(intentType))
            {
                Debug.LogWarning($"MonsterPatternSO '{name}': IntentType이 설정되지 않았습니다.");
            }
            
            // 공격과 방어가 모두 0이면 경고
            if (attackBonus == 0 && defenseBonus == 0 && additionalDamage == 0 && 
                (statusEffects == null || statusEffects.Length == 0))
            {
                Debug.LogWarning($"MonsterPatternSO '{name}': 효과가 없는 패턴입니다.");
            }
        }
        #endif
    }
}