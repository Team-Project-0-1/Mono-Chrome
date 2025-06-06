using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 적 AI를 관리하는 싱글톤 매니저 클래스
    /// 몬스터의 행동 패턴과 전략적 결정을 담당한다.
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        #region Singleton
        private static AIManager _instance;
        
        public static AIManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("AIManager");
                    _instance = go.AddComponent<AIManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        #endregion
        
        #region Pattern Selection
        /// <summary>
        /// 적 캐릭터의 패턴 선택 로직
        /// </summary>
        /// <param name="enemy">적 캐릭터</param>
        /// <param name="player">플레이어 캐릭터</param>
        /// <returns>선택된 패턴</returns>
        public Pattern SelectPattern(Character enemy, Character player)
        {
            if (enemy == null || player == null)
            {
                Debug.LogError("Cannot select pattern: null enemy or player");
                return null;
            }
            
            List<Pattern> availablePatterns = enemy.GetAvailablePatterns();
            
            if (availablePatterns == null || availablePatterns.Count == 0)
            {
                Debug.LogWarning($"Enemy {enemy.CharacterName} has no available patterns");
                return null;
            }
            
            // AI 유형에 따라 다른 선택 로직 적용
            switch (enemy.Type)
            {
                case CharacterType.Normal:
                    return SelectNormalEnemyPattern(enemy, player, availablePatterns);
                    
                case CharacterType.Elite:
                    return SelectEliteEnemyPattern(enemy, player, availablePatterns);
                    
                case CharacterType.MiniBoss:
                    return SelectMiniBossPattern(enemy, player, availablePatterns);
                    
                case CharacterType.Boss:
                    return SelectBossPattern(enemy, player, availablePatterns);
                    
                default:
                    // 기본: 랜덤 선택
                    return SelectRandomPattern(availablePatterns);
            }
        }
        
        /// <summary>
        /// 일반 적의 패턴 선택 - 단순한 결정 로직
        /// </summary>
        private Pattern SelectNormalEnemyPattern(Character enemy, Character player, List<Pattern> availablePatterns)
        {
            // 체력이 낮으면 방어 우선
            if (enemy.CurrentHealth < enemy.MaxHealth * 0.3f)
            {
                // 방어 패턴 우선 찾기
                Pattern defensePattern = FindPatternByType(availablePatterns, false);
                if (defensePattern != null)
                {
                    return defensePattern;
                }
            }
            
            // 기본적으로 공격 선호
            Pattern attackPattern = FindPatternByType(availablePatterns, true);
            if (attackPattern != null)
            {
                return attackPattern;
            }
            
            // 찾지 못했으면 랜덤 선택
            return SelectRandomPattern(availablePatterns);
        }
        
        /// <summary>
        /// 엘리트 적의 패턴 선택 - 플레이어 상태 고려
        /// </summary>
        private Pattern SelectEliteEnemyPattern(Character enemy, Character player, List<Pattern> availablePatterns)
        {
            // 플레이어 체력이 낮으면 공격 우선
            if (player.CurrentHealth < player.MaxHealth * 0.4f)
            {
                // 공격 패턴 찾기
                Pattern attackPattern = FindBestAttackPattern(availablePatterns);
                if (attackPattern != null)
                {
                    return attackPattern;
                }
            }
            
            // 적 체력이 낮으면 방어 우선
            if (enemy.CurrentHealth < enemy.MaxHealth * 0.3f)
            {
                // 방어 패턴 찾기
                Pattern defensePattern = FindBestDefensePattern(availablePatterns);
                if (defensePattern != null)
                {
                    return defensePattern;
                }
            }
            
            // 플레이어의 방어력이 높으면 상태이상 우선
            if (player.CurrentDefense > 5)
            {
                // 상태이상 중심 패턴 찾기
                Pattern statusPattern = FindPatternWithStatusEffect(availablePatterns);
                if (statusPattern != null)
                {
                    return statusPattern;
                }
            }
            
            // 40% 확률로 공격, 30% 확률로 방어, 30% 확률로 랜덤
            float rand = Random.value;
            if (rand < 0.4f)
            {
                Pattern attackPattern = FindPatternByType(availablePatterns, true);
                if (attackPattern != null)
                {
                    return attackPattern;
                }
            }
            else if (rand < 0.7f)
            {
                Pattern defensePattern = FindPatternByType(availablePatterns, false);
                if (defensePattern != null)
                {
                    return defensePattern;
                }
            }
            
            // 찾지 못했으면 랜덤 선택
            return SelectRandomPattern(availablePatterns);
        }
        
        /// <summary>
        /// 미니보스의 패턴 선택 - 고급 결정 로직
        /// </summary>
        private Pattern SelectMiniBossPattern(Character enemy, Character player, List<Pattern> availablePatterns)
        {
            // 턴 숫자를 사용한 특수 패턴 (3턴마다 특수 공격 등)
            int turnCount = 0; // 실제 구현 시 턴 카운터를 활용
            if (turnCount % 3 == 0)
            {
                // 특수 턴에는 상태이상 부여 패턴 우선
                Pattern statusPattern = FindPatternWithStatusEffect(availablePatterns);
                if (statusPattern != null)
                {
                    return statusPattern;
                }
            }
            
            // 플레이어가 상태이상을 가지고 있으면 공격 우선
            if (HasAnyStatusEffect(player))
            {
                Pattern attackPattern = FindBestAttackPattern(availablePatterns);
                if (attackPattern != null)
                {
                    return attackPattern;
                }
            }
            
            // 체력 비율에 따른 전략 선택
            float healthRatio = (float)enemy.CurrentHealth / enemy.MaxHealth;
            
            if (healthRatio < 0.3f)
            {
                // 체력이 위험 수준이면 방어 우선
                Pattern defensePattern = FindBestDefensePattern(availablePatterns);
                if (defensePattern != null)
                {
                    return defensePattern;
                }
            }
            else if (healthRatio < 0.5f)
            {
                // 체력이 중간 수준이면 균형 잡힌 전략
                if (Random.value < 0.5f)
                {
                    Pattern attackPattern = FindPatternByType(availablePatterns, true);
                    if (attackPattern != null)
                    {
                        return attackPattern;
                    }
                }
                else
                {
                    Pattern defensePattern = FindPatternByType(availablePatterns, false);
                    if (defensePattern != null)
                    {
                        return defensePattern;
                    }
                }
            }
            else
            {
                // 체력이 건강하면 공격적 전략
                Pattern attackPattern = FindBestAttackPattern(availablePatterns);
                if (attackPattern != null)
                {
                    return attackPattern;
                }
            }
            
            // 기본 선택
            return SelectRandomPattern(availablePatterns);
        }
        
        /// <summary>
        /// 보스의 패턴 선택 - 페이즈 기반 고급 AI
        /// </summary>
        private Pattern SelectBossPattern(Character enemy, Character player, List<Pattern> availablePatterns)
        {
            // 보스 체력 비율 기반 페이즈 설정
            float healthRatio = (float)enemy.CurrentHealth / enemy.MaxHealth;
            
            // 페이즈 1: 체력 70% 이상
            if (healthRatio > 0.7f)
            {
                // 페이즈 1 전략: 플레이어 탐색, 상태이상 부여 중심
                if (Random.value < 0.7f)
                {
                    // 70% 확률로 상태이상 부여 패턴 우선
                    Pattern statusPattern = FindPatternWithStatusEffect(availablePatterns);
                    if (statusPattern != null)
                    {
                        return statusPattern;
                    }
                }
                else
                {
                    // 30% 확률로 공격 패턴
                    Pattern attackPattern = FindPatternByType(availablePatterns, true);
                    if (attackPattern != null)
                    {
                        return attackPattern;
                    }
                }
            }
            // 페이즈 2: 체력 30%~70%
            else if (healthRatio > 0.3f)
            {
                // 페이즈 2 전략: 공격성 증가, 강력한 패턴 사용
                if (Random.value < 0.6f)
                {
                    // 60% 확률로 최고 공격력 패턴
                    Pattern strongAttackPattern = FindStrongestPattern(availablePatterns, true);
                    if (strongAttackPattern != null)
                    {
                        return strongAttackPattern;
                    }
                }
                else if (Random.value < 0.3f)
                {
                    // 30% 확률로 상태이상 패턴
                    Pattern statusPattern = FindPatternWithStatusEffect(availablePatterns);
                    if (statusPattern != null)
                    {
                        return statusPattern;
                    }
                }
                else
                {
                    // 10% 확률로 방어 패턴
                    Pattern defensePattern = FindPatternByType(availablePatterns, false);
                    if (defensePattern != null)
                    {
                        return defensePattern;
                    }
                }
            }
            // 페이즈 3: 체력 30% 미만 (분노 페이즈)
            else
            {
                // 페이즈 3 전략: 극도로 공격적인 패턴, 상태이상 커버
                
                // 체력이 10% 미만이면 특수 기술 사용 (실제 구현 시 추가적인 로직 필요)
                if (healthRatio < 0.1f && Random.value < 0.3f)
                {
                    // 특수 디스페어 기술 가정
                    Debug.Log("Boss is using desperate special attack!");
                    // 특수 기술 설정 로직 (추후 구현)
                }
                
                // 80% 확률로 최고 공격 패턴
                if (Random.value < 0.8f)
                {
                    Pattern bestAttackPattern = FindStrongestPattern(availablePatterns, true);
                    if (bestAttackPattern != null)
                    {
                        return bestAttackPattern;
                    }
                }
                else
                {
                    // 20% 확률로 강력한 상태이상 패턴
                    Pattern statusPattern = FindPatternWithStatusEffect(availablePatterns);
                    if (statusPattern != null)
                    {
                        return statusPattern;
                    }
                }
            }
            
            // 어떤 조건도 매칭되지 않으면 랜덤 선택
            return SelectRandomPattern(availablePatterns);
        }
        #endregion
        
        #region Pattern Utility Methods
        /// <summary>
        /// 랜덤 패턴 선택
        /// </summary>
        private Pattern SelectRandomPattern(List<Pattern> patterns)
        {
            if (patterns == null || patterns.Count == 0)
            {
                return null;
            }
            
            int randomIndex = Random.Range(0, patterns.Count);
            return patterns[randomIndex];
        }
        
        /// <summary>
        /// 특정 타입(공격/방어)의 패턴 찾기
        /// </summary>
        private Pattern FindPatternByType(List<Pattern> patterns, bool isAttack)
        {
            List<Pattern> matchingPatterns = new List<Pattern>();
            
            foreach (Pattern pattern in patterns)
            {
                if (pattern.IsAttack == isAttack)
                {
                    matchingPatterns.Add(pattern);
                }
            }
            
            if (matchingPatterns.Count > 0)
            {
                return SelectRandomPattern(matchingPatterns);
            }
            
            return null;
        }
        
        /// <summary>
        /// 가장 강력한 공격 패턴 찾기
        /// </summary>
        private Pattern FindBestAttackPattern(List<Pattern> patterns)
        {
            Pattern bestPattern = null;
            int highestAttackBonus = -1;
            
            foreach (Pattern pattern in patterns)
            {
                if (pattern.IsAttack && pattern.AttackBonus > highestAttackBonus)
                {
                    bestPattern = pattern;
                    highestAttackBonus = pattern.AttackBonus;
                }
            }
            
            return bestPattern;
        }
        
        /// <summary>
        /// 가장 강력한 방어 패턴 찾기
        /// </summary>
        private Pattern FindBestDefensePattern(List<Pattern> patterns)
        {
            Pattern bestPattern = null;
            int highestDefenseBonus = -1;
            
            foreach (Pattern pattern in patterns)
            {
                if (!pattern.IsAttack && pattern.DefenseBonus > highestDefenseBonus)
                {
                    bestPattern = pattern;
                    highestDefenseBonus = pattern.DefenseBonus;
                }
            }
            
            return bestPattern;
        }
        
        /// <summary>
        /// 상태이상 효과가 있는 패턴 찾기
        /// </summary>
        private Pattern FindPatternWithStatusEffect(List<Pattern> patterns)
        {
            List<Pattern> statusPatterns = new List<Pattern>();
            
            foreach (Pattern pattern in patterns)
            {
                if (pattern.StatusEffects != null && pattern.StatusEffects.Length > 0)
                {
                    statusPatterns.Add(pattern);
                }
            }
            
            if (statusPatterns.Count > 0)
            {
                return SelectRandomPattern(statusPatterns);
            }
            
            return null;
        }
        
        /// <summary>
        /// 특정 타입에서 가장 강력한 패턴 찾기
        /// </summary>
        private Pattern FindStrongestPattern(List<Pattern> patterns, bool isAttack)
        {
            if (isAttack)
            {
                return FindBestAttackPattern(patterns);
            }
            else
            {
                return FindBestDefensePattern(patterns);
            }
        }
        
        /// <summary>
        /// 캐릭터가 어떤 상태이상을 가지고 있는지 확인
        /// </summary>
        private bool HasAnyStatusEffect(Character character)
        {
            return character.GetAllStatusEffects().Count > 0;
        }
        #endregion
    }
}
