using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MonoChrome.Systems.Combat;
using MonoChrome.Data;
using UnityEngine;

namespace MonoChrome.AI
{
    /// <summary>
    /// 적 AI를 관리하는 싱글톤 매니저 클래스
    /// 몬스터의 행동 패턴과 전략적 결정을 담당한다.
    /// PatternDataManager와 연동하여 MonsterPatternSO를 활용한다.
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
            
            InitializeAISystem();
        }
        #endregion
        
        #region Fields
        [Header("AI 시스템 설정")]
        [SerializeField] private bool enableAdvancedAI = true;
        [SerializeField] private float decisionDelayTime = 0.5f;
        
        // 턴 관리
        private Dictionary<Character, int> monsterTurnCounts = new Dictionary<Character, int>();
        private Dictionary<Character, MonsterPatternSO> currentIntents = new Dictionary<Character, MonsterPatternSO>();
        
        // 패턴 데이터 캐시
        private Dictionary<string, List<MonsterPatternSO>> monsterPatternCache = new Dictionary<string, List<MonsterPatternSO>>();
        #endregion
        
        #region Initialization
        /// <summary>
        /// AI 시스템 초기화
        /// </summary>
        private void InitializeAISystem()
        {
            Debug.Log("AIManager: AI 시스템 초기화 완료");
            
            // PatternDataManager가 있다면 패턴 데이터 캐시 구성
            if (PatternDataManager.Instance != null)
            {
                LoadPatternDataCache();
            }
        }
        
        /// <summary>
        /// 패턴 데이터 캐시 로드
        /// </summary>
        private void LoadPatternDataCache()
        {
            monsterPatternCache.Clear();
            
            var allMonsterPatterns = PatternDataManager.Instance.GetAllMonsterPatterns();
            
            // 몬스터 타입별로 패턴 그룹화
            foreach (var pattern in allMonsterPatterns)
            {
                if (pattern == null) continue;
                
                string monsterType = pattern.MonsterType;
                if (!monsterPatternCache.ContainsKey(monsterType))
                {
                    monsterPatternCache[monsterType] = new List<MonsterPatternSO>();
                }
                
                monsterPatternCache[monsterType].Add(pattern);
            }
            
            Debug.Log($"AIManager: {monsterPatternCache.Count}개의 몬스터 타입에 대한 패턴 캐시 로드 완료");
        }
        #endregion
        
        #region Pattern Selection
        /// <summary>
        /// 몬스터의 다음 행동 의도를 결정하고 캐시한다.
        /// 의도 표시 시스템에서 사용할 수 있도록 한다.
        /// </summary>
        /// <param name="monster">몬스터 캐릭터</param>
        /// <param name="player">플레이어 캐릭터</param>
        /// <returns>선택된 몬스터 패턴</returns>
        public MonsterPatternSO DetermineIntent(Character monster, Character player)
        {
            if (monster == null || player == null)
            {
                Debug.LogError("AIManager: Cannot determine intent - null monster or player");
                return null;
            }
            
            MonsterPatternSO selectedPattern = SelectMonsterPattern(monster, player);
            
            // 의도 캐시에 저장
            if (selectedPattern != null)
            {
                currentIntents[monster] = selectedPattern;
                Debug.Log($"AIManager: {monster.CharacterName}의 다음 의도 결정 - {selectedPattern.PatternName}");
            }
            
            return selectedPattern;
        }
        
        /// <summary>
        /// 몬스터의 현재 의도 가져오기 (의도 표시 시스템용)
        /// </summary>
        /// <param name="monster">몬스터 캐릭터</param>
        /// <returns>현재 의도된 패턴</returns>
        public MonsterPatternSO GetCurrentIntent(Character monster)
        {
            if (monster == null) return null;
            
            currentIntents.TryGetValue(monster, out MonsterPatternSO intent);
            return intent;
        }
        
        /// <summary>
        /// 몬스터 패턴 선택 로직 (MonsterPatternSO 기반)
        /// </summary>
        /// <param name="monster">몬스터 캐릭터</param>
        /// <param name="player">플레이어 캐릭터</param>
        /// <returns>선택된 몬스터 패턴</returns>
        public MonsterPatternSO SelectMonsterPattern(Character monster, Character player)
        {
            if (monster == null || player == null)
            {
                Debug.LogError("AIManager: Cannot select pattern - null monster or player");
                return null;
            }
            
            // 몬스터 타입에 맞는 패턴 목록 가져오기
            List<MonsterPatternSO> availablePatterns = GetAvailableMonsterPatterns(monster);
            
            if (availablePatterns == null || availablePatterns.Count == 0)
            {
                Debug.LogWarning($"AIManager: {monster.CharacterName}에 사용 가능한 패턴이 없습니다");
                return null;
            }
            
            // 턴 카운트 증가
            IncrementTurnCount(monster);
            
            // AI 유형에 따라 다른 선택 로직 적용
            switch (monster.Type)
            {
                case CharacterType.Normal:
                    return SelectNormalMonsterPattern(monster, player, availablePatterns);
                    
                case CharacterType.Elite:
                    return SelectEliteMonsterPattern(monster, player, availablePatterns);
                    
                case CharacterType.MiniBoss:
                    return SelectMiniBossMonsterPattern(monster, player, availablePatterns);
                    
                case CharacterType.Boss:
                    return SelectBossMonsterPattern(monster, player, availablePatterns);
                    
                default:
                    return SelectRandomMonsterPattern(availablePatterns);
            }
        }
        
        /// <summary>
        /// 몬스터 타입에 맞는 사용 가능한 패턴 목록 가져오기
        /// </summary>
        private List<MonsterPatternSO> GetAvailableMonsterPatterns(Character monster)
        {
            // 우선 몬스터 이름으로 패턴 찾기
            if (monsterPatternCache.ContainsKey(monster.CharacterName))
            {
                return monsterPatternCache[monster.CharacterName];
            }
            
            // 몬스터 타입으로 패턴 찾기
            string typeKey = monster.Type.ToString();
            if (monsterPatternCache.ContainsKey(typeKey))
            {
                return monsterPatternCache[typeKey];
            }
            
            // 기본 패턴 반환 (전체 패턴 중 랜덤)
            var allPatterns = PatternDataManager.Instance?.GetAllMonsterPatterns();
            if (allPatterns != null && allPatterns.Count > 0)
            {
                Debug.LogWarning($"AIManager: {monster.CharacterName}에 특화된 패턴을 찾지 못했습니다. 기본 패턴 사용");
                return allPatterns.Take(3).ToList(); // 처음 3개만 사용
            }
            
            return new List<MonsterPatternSO>();
        }
        #endregion
        
        #region AI Logic by Monster Type
        /// <summary>
        /// 일반 몬스터 패턴 선택 - 단순한 결정 로직
        /// </summary>
        private MonsterPatternSO SelectNormalMonsterPattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns)
        {
            // 체력이 낮으면 방어/회복 패턴 우선
            if (monster.CurrentHealth < monster.MaxHealth * 0.3f)
            {
                var defensivePattern = FindPatternByIntent(availablePatterns, "방어", "회복", "치유");
                if (defensivePattern != null) return defensivePattern;
            }
            
            // 70% 확률로 공격 패턴, 30% 확률로 기타 패턴
            if (Random.value < 0.7f)
            {
                var attackPattern = FindPatternByIntent(availablePatterns, "공격", "타격", "피해");
                if (attackPattern != null) return attackPattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        
        /// <summary>
        /// 엘리트 몬스터 패턴 선택 - 플레이어 상태 고려
        /// </summary>
        private MonsterPatternSO SelectEliteMonsterPattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns)
        {
            int turnCount = GetTurnCount(monster);
            
            // 특정 턴에 특수 패턴 사용
            if (turnCount % 4 == 0)
            {
                var specialPattern = FindPatternByIntent(availablePatterns, "특수", "강화", "상태");
                if (specialPattern != null) return specialPattern;
            }
            
            // 플레이어 체력이 낮으면 공격 집중
            if (player.CurrentHealth < player.MaxHealth * 0.4f)
            {
                var strongAttackPattern = FindStrongestPattern(availablePatterns, true);
                if (strongAttackPattern != null) return strongAttackPattern;
            }
            
            // 플레이어 방어력이 높으면 상태이상 우선
            if (player.CurrentDefense > 5)
            {
                var statusPattern = FindPatternByIntent(availablePatterns, "저주", "독", "출혈", "봉인");
                if (statusPattern != null) return statusPattern;
            }
            
            // 기본 전략: 60% 공격, 20% 방어, 20% 상태이상
            float rand = Random.value;
            if (rand < 0.6f)
            {
                var attackPattern = FindPatternByIntent(availablePatterns, "공격", "타격");
                if (attackPattern != null) return attackPattern;
            }
            else if (rand < 0.8f)
            {
                var defensePattern = FindPatternByIntent(availablePatterns, "방어", "보호");
                if (defensePattern != null) return defensePattern;
            }
            else
            {
                var statusPattern = FindPatternByIntent(availablePatterns, "상태", "저주", "독");
                if (statusPattern != null) return statusPattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        
        /// <summary>
        /// 미니보스 패턴 선택 - 페이즈 기반 로직
        /// </summary>
        private MonsterPatternSO SelectMiniBossMonsterPattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns)
        {
            int turnCount = GetTurnCount(monster);
            float healthRatio = (float)monster.CurrentHealth / monster.MaxHealth;
            
            // 첫 번째 턴에는 특수 패턴 사용
            if (turnCount == 1)
            {
                var openingPattern = FindPatternByIntent(availablePatterns, "등장", "시작", "특수");
                if (openingPattern != null) return openingPattern;
            }
            
            // 3턴마다 강력한 공격
            if (turnCount % 3 == 0)
            {
                var powerfulPattern = FindStrongestPattern(availablePatterns, true);
                if (powerfulPattern != null) return powerfulPattern;
            }
            
            // 체력 기반 전략
            if (healthRatio < 0.3f)
            {
                // 위험 상황: 방어 또는 회복
                var emergencyPattern = FindPatternByIntent(availablePatterns, "방어", "회복", "보호");
                if (emergencyPattern != null) return emergencyPattern;
            }
            else if (healthRatio < 0.6f)
            {
                // 중간 상황: 균형잡힌 전략
                if (Random.value < 0.5f)
                {
                    var attackPattern = FindPatternByIntent(availablePatterns, "공격");
                    if (attackPattern != null) return attackPattern;
                }
                else
                {
                    var statusPattern = FindPatternByIntent(availablePatterns, "상태");
                    if (statusPattern != null) return statusPattern;
                }
            }
            else
            {
                // 건강한 상황: 공격적 전략
                var aggressivePattern = FindPatternByIntent(availablePatterns, "공격", "타격");
                if (aggressivePattern != null) return aggressivePattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        
        /// <summary>
        /// 보스 패턴 선택 - 고급 페이즈 기반 AI
        /// </summary>
        private MonsterPatternSO SelectBossMonsterPattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns)
        {
            int turnCount = GetTurnCount(monster);
            float healthRatio = (float)monster.CurrentHealth / monster.MaxHealth;
            
            // 첫 턴에는 반드시 등장 패턴
            if (turnCount == 1)
            {
                var openingPattern = FindPatternByIntent(availablePatterns, "등장", "시작");
                if (openingPattern != null) return openingPattern;
            }
            
            // 페이즈 1: 체력 70% 이상
            if (healthRatio > 0.7f)
            {
                return SelectPhase1Pattern(monster, player, availablePatterns, turnCount);
            }
            // 페이즈 2: 체력 30%~70%
            else if (healthRatio > 0.3f)
            {
                return SelectPhase2Pattern(monster, player, availablePatterns, turnCount);
            }
            // 페이즈 3: 체력 30% 미만 (분노 페이즈)
            else
            {
                return SelectPhase3Pattern(monster, player, availablePatterns, turnCount);
            }
        }
        
        private MonsterPatternSO SelectPhase1Pattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns, int turnCount)
        {
            // 5턴마다 특수 공격
            if (turnCount % 5 == 0)
            {
                var specialPattern = FindPatternByIntent(availablePatterns, "특수", "강화");
                if (specialPattern != null) return specialPattern;
            }
            
            // 70% 상태이상, 30% 공격
            if (Random.value < 0.7f)
            {
                var statusPattern = FindPatternByIntent(availablePatterns, "저주", "독", "봉인");
                if (statusPattern != null) return statusPattern;
            }
            else
            {
                var attackPattern = FindPatternByIntent(availablePatterns, "공격");
                if (attackPattern != null) return attackPattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        
        private MonsterPatternSO SelectPhase2Pattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns, int turnCount)
        {
            // 3턴마다 강력한 공격
            if (turnCount % 3 == 0)
            {
                var powerfulPattern = FindStrongestPattern(availablePatterns, true);
                if (powerfulPattern != null) return powerfulPattern;
            }
            
            // 60% 공격, 25% 상태이상, 15% 방어
            float rand = Random.value;
            if (rand < 0.6f)
            {
                var attackPattern = FindPatternByIntent(availablePatterns, "공격", "타격");
                if (attackPattern != null) return attackPattern;
            }
            else if (rand < 0.85f)
            {
                var statusPattern = FindPatternByIntent(availablePatterns, "상태", "저주");
                if (statusPattern != null) return statusPattern;
            }
            else
            {
                var defensePattern = FindPatternByIntent(availablePatterns, "방어", "보호");
                if (defensePattern != null) return defensePattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        
        private MonsterPatternSO SelectPhase3Pattern(Character monster, Character player, List<MonsterPatternSO> availablePatterns, int turnCount)
        {
            // 매 2턴마다 강력한 공격
            if (turnCount % 2 == 0)
            {
                var desperatePattern = FindStrongestPattern(availablePatterns, true);
                if (desperatePattern != null) return desperatePattern;
            }
            
            // 80% 공격, 20% 특수 기술
            if (Random.value < 0.8f)
            {
                var attackPattern = FindStrongestPattern(availablePatterns, true);
                if (attackPattern != null) return attackPattern;
            }
            else
            {
                var specialPattern = FindPatternByIntent(availablePatterns, "특수", "분노", "절망");
                if (specialPattern != null) return specialPattern;
            }
            
            return SelectRandomMonsterPattern(availablePatterns);
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// 특정 의도를 가진 패턴 찾기
        /// </summary>
        private MonsterPatternSO FindPatternByIntent(List<MonsterPatternSO> patterns, params string[] keywords)
        {
            var matchingPatterns = new List<MonsterPatternSO>();
            
            foreach (var pattern in patterns)
            {
                foreach (string keyword in keywords)
                {
                    if (pattern.PatternName.Contains(keyword) || 
                        pattern.Description.Contains(keyword) ||
                        pattern.IntentType.Contains(keyword))
                    {
                        matchingPatterns.Add(pattern);
                        break;
                    }
                }
            }
            
            return matchingPatterns.Count > 0 ? SelectRandomMonsterPattern(matchingPatterns) : null;
        }
        
        /// <summary>
        /// 가장 강력한 패턴 찾기
        /// </summary>
        private MonsterPatternSO FindStrongestPattern(List<MonsterPatternSO> patterns, bool attackOnly = false)
        {
            MonsterPatternSO strongest = null;
            int highestValue = -1;
            
            foreach (var pattern in patterns)
            {
                if (attackOnly && !pattern.IntentType.Contains("공격")) continue;
                
                int totalValue = pattern.AttackBonus + pattern.DefenseBonus + 
                               (pattern.StatusEffects?.Length ?? 0) * 2;
                
                if (totalValue > highestValue)
                {
                    strongest = pattern;
                    highestValue = totalValue;
                }
            }
            
            return strongest;
        }
        
        /// <summary>
        /// 랜덤 몬스터 패턴 선택
        /// </summary>
        private MonsterPatternSO SelectRandomMonsterPattern(List<MonsterPatternSO> patterns)
        {
            if (patterns == null || patterns.Count == 0) return null;
            
            int randomIndex = Random.Range(0, patterns.Count);
            return patterns[randomIndex];
        }
        
        /// <summary>
        /// 몬스터 턴 카운트 증가
        /// </summary>
        private void IncrementTurnCount(Character monster)
        {
            if (!monsterTurnCounts.ContainsKey(monster))
            {
                monsterTurnCounts[monster] = 0;
            }
            
            monsterTurnCounts[monster]++;
        }
        
        /// <summary>
        /// 몬스터 턴 카운트 가져오기
        /// </summary>
        private int GetTurnCount(Character monster)
        {
            monsterTurnCounts.TryGetValue(monster, out int count);
            return count;
        }
        
        /// <summary>
        /// 몬스터 전투 종료 시 정리
        /// </summary>
        public void CleanupMonster(Character monster)
        {
            if (monster == null) return;
            
            monsterTurnCounts.Remove(monster);
            currentIntents.Remove(monster);
            
            Debug.Log($"AIManager: {monster.CharacterName} 전투 데이터 정리 완료");
        }
        
        /// <summary>
        /// 전투 종료 시 모든 데이터 정리
        /// </summary>
        public void CleanupBattleData()
        {
            monsterTurnCounts.Clear();
            currentIntents.Clear();
            
            Debug.Log("AIManager: 전투 종료 - 모든 AI 데이터 정리 완료");
        }
        #endregion
    }
}