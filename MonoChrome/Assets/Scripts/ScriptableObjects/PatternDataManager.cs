using System.Collections.Generic;
using System.Linq;
using MonoChrome.Data;
using MonoChrome.Systems.Combat;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 패턴(족보) 데이터를 중앙에서 관리하는 매니저
    /// ScriptableObject 기반으로 데이터를 로드하고 캐시합니다.
    /// PatternSO와 MonsterPatternSO를 모두 지원합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "PatternDataManager", menuName = "MonoChrome/Managers/Pattern Data Manager")]
    public class PatternDataManager : ScriptableObject
    {
        #region Singleton Pattern for Runtime
        private static PatternDataManager _instance;
        public static PatternDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<PatternDataManager>("PatternDataManager");
                    if (_instance == null)
                    {
                        Debug.LogError("PatternDataManager not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }
        #endregion

        [Header("기본 패턴")]
        [SerializeField] private PatternSO[] _basicPatterns;

        [Header("감각별 패턴")]
        [SerializeField] private PatternSO[] _auditoryPatterns;    // 청각
        [SerializeField] private PatternSO[] _olfactoryPatterns;   // 후각
        [SerializeField] private PatternSO[] _tactilePatterns;     // 촉각
        [SerializeField] private PatternSO[] _spiritualPatterns;   // 영혼

        [Header("적 전용 패턴")]
        [SerializeField] private PatternSO[] _enemyPatterns;
        [SerializeField] private PatternSO[] _bossPatterns;
        
        [Header("몬스터 패턴 (AI용)")]
        [SerializeField] private MonsterPatternSO[] _monsterPatterns;

        // 캐시된 데이터
        private Dictionary<int, PatternSO> _patternCache;
        private Dictionary<SenseType, List<PatternSO>> _patternsBySense;
        private Dictionary<CharacterType, List<PatternSO>> _patternsByCharacterType;
        
        // 몬스터 패턴 캐시
        private Dictionary<string, List<MonsterPatternSO>> _monsterPatternsByType;
        private List<MonsterPatternSO> _allMonsterPatterns;

        #region Public Methods
        /// <summary>
        /// 매니저 초기화
        /// </summary>
        public void Initialize()
        {
            BuildCache();
            BuildMonsterPatternCache();
            Debug.Log($"PatternDataManager initialized with {_patternCache.Count} patterns and {_allMonsterPatterns?.Count ?? 0} monster patterns");
        }

        /// <summary>
        /// ID로 패턴 가져오기
        /// </summary>
        public PatternSO GetPattern(int patternId)
        {
            if (_patternCache == null)
                BuildCache();

            if (_patternCache.TryGetValue(patternId, out PatternSO pattern))
            {
                return pattern;
            }

            Debug.LogWarning($"Pattern not found with ID: {patternId}");
            return null;
        }

        /// <summary>
        /// 기본 패턴들 가져오기
        /// </summary>
        public List<PatternSO> GetBasicPatterns()
        {
            if (_patternCache == null)
                BuildCache();

            return _basicPatterns?.ToList() ?? new List<PatternSO>();
        }

        /// <summary>
        /// 감각 유형별 패턴 가져오기
        /// </summary>
        public List<PatternSO> GetPatternsBySense(SenseType senseType)
        {
            if (_patternsBySense == null)
                BuildCache();

            if (_patternsBySense.TryGetValue(senseType, out List<PatternSO> patterns))
            {
                return patterns.ToList();
            }

            return new List<PatternSO>();
        }

        /// <summary>
        /// 캐릭터 타입별 패턴 가져오기
        /// </summary>
        public List<PatternSO> GetPatternsByCharacterType(CharacterType characterType)
        {
            if (_patternsByCharacterType == null)
                BuildCache();

            if (_patternsByCharacterType.TryGetValue(characterType, out List<PatternSO> patterns))
            {
                return patterns.ToList();
            }

            return new List<PatternSO>();
        }

        /// <summary>
        /// 이름으로 패턴 가져오기
        /// </summary>
        public PatternSO GetPattern(string patternName)
        {
            if (_patternCache == null)
                BuildCache();

            return _patternCache.Values.FirstOrDefault(p => p.patternName == patternName);
        }

        /// <summary>
        /// 플레이어의 감각 타입에 맞는 사용 가능한 패턴들 가져오기
        /// </summary>
        public List<PatternSO> GetAvailablePatterns(SenseType senseType, bool[] coinStates)
        {
            if (_patternsBySense == null)
                BuildCache();

            List<PatternSO> availablePatterns = new List<PatternSO>();

            // 기본 패턴 검사 - 앞면과 뒷면 버전 모두 확인
            if (_basicPatterns != null)
            {
                foreach (PatternSO pattern in _basicPatterns)
                {
                    if (pattern != null && pattern.IsApplicableTo(CharacterType.Player, senseType))
                    {
                        // 앞면 패턴 확인
                        if (pattern.ValidatePatternWithValue(coinStates, true))
                        {
                            var frontPattern = CreatePatternVariant(pattern, true);
                            availablePatterns.Add(frontPattern);
                        }
                        
                        // 뒷면 패턴 확인
                        if (pattern.ValidatePatternWithValue(coinStates, false))
                        {
                            var backPattern = CreatePatternVariant(pattern, false);
                            availablePatterns.Add(backPattern);
                        }
                    }
                }
            }

            // 감각별 특수 패턴 검사 - 앞면과 뒷면 버전 모두 확인
            if (_patternsBySense.TryGetValue(senseType, out List<PatternSO> sensePatterns))
            {
                foreach (PatternSO pattern in sensePatterns)
                {
                    if (pattern != null)
                    {
                        // 앞면 패턴 확인
                        if (pattern.ValidatePatternWithValue(coinStates, true))
                        {
                            var frontPattern = CreatePatternVariant(pattern, true);
                            availablePatterns.Add(frontPattern);
                        }
                        
                        // 뒷면 패턴 확인
                        if (pattern.ValidatePatternWithValue(coinStates, false))
                        {
                            var backPattern = CreatePatternVariant(pattern, false);
                            availablePatterns.Add(backPattern);
                        }
                    }
                }
            }

            Debug.Log($"PatternDataManager: Found {availablePatterns.Count} available patterns for {senseType} with coin states: [{string.Join(", ", coinStates)}]");
            return availablePatterns;
        }
        
        /// <summary>
        /// 패턴의 앞면/뒷면 변형 생성
        /// </summary>
        private PatternSO CreatePatternVariant(PatternSO basePattern, bool isHeads)
        {
            // 런타임에서만 사용하는 임시 PatternSO 생성
            var variant = CreateInstance<PatternSO>();
            
            // 기본 속성 복사
            // 기본 족보 표기법 사용
            string suffix = isHeads ? " (앞면)" : " (뒷면)";
            variant.patternName = basePattern.patternName + suffix;
            variant.description = basePattern.description;
            // ID 중복 방지: 뒷면 패턴은 음수 ID 사용
            variant.id = isHeads ? basePattern.id : -basePattern.id;
            variant.icon = basePattern.icon;
            variant.patternType = basePattern.patternType;
            variant.patternValue = isHeads;
            variant.isAttack = basePattern.isAttack;
            variant.condition = basePattern.condition;
            variant.applicableCharacterTypes = basePattern.applicableCharacterTypes;
            variant.applicableSenseTypes = basePattern.applicableSenseTypes;
            variant.attackBonus = basePattern.attackBonus;
            variant.defenseBonus = basePattern.defenseBonus;
            variant.specialEffect = basePattern.specialEffect;
            variant.statusEffects = basePattern.statusEffects;
            variant.animationTrigger = basePattern.animationTrigger;
            
            return variant;
        }

        /// <summary>
        /// 적 캐릭터용 패턴 가져오기
        /// </summary>
        public List<PatternSO> GetEnemyPatterns(CharacterType enemyType, bool[] coinStates)
        {
            if (_patternsByCharacterType == null)
                BuildCache();

            List<PatternSO> availablePatterns = new List<PatternSO>();

            // 기본 패턴도 사용 가능
            if (_basicPatterns != null)
            {
                foreach (PatternSO pattern in _basicPatterns)
                {
                    if (pattern != null && pattern.IsApplicableTo(enemyType) 
                        && pattern.ValidatePattern(coinStates))
                    {
                        availablePatterns.Add(pattern);
                    }
                }
            }

            // 적 전용 패턴 검사
            if (_patternsByCharacterType.TryGetValue(enemyType, out List<PatternSO> enemyPatterns))
            {
                foreach (PatternSO pattern in enemyPatterns)
                {
                    if (pattern != null && pattern.ValidatePattern(coinStates))
                    {
                        availablePatterns.Add(pattern);
                    }
                }
            }

            return availablePatterns;
        }

        /// <summary>
        /// Pattern 클래스 리스트로 변환 (기존 코드와의 호환성)
        /// </summary>
        public List<Pattern> ConvertToPatterns(List<PatternSO> patternSOs)
        {
            List<Pattern> patterns = new List<Pattern>();
            
            foreach (PatternSO patternSO in patternSOs)
            {
                if (patternSO != null)
                {
                    patterns.Add(patternSO.ToPattern());
                }
            }

            return patterns;
        }

        /// <summary>
        /// 기본 공격/방어 패턴 가져오기 (응급용)
        /// </summary>
        public Pattern GetDefaultPattern(bool isAttack)
        {
            Pattern defaultPattern = new Pattern
            {
                ID = isAttack ? -1 : -2,
                Name = isAttack ? "기본 공격" : "기본 방어",
                Description = isAttack ? "간단한 공격" : "간단한 방어",
                PatternType = PatternType.Consecutive2,
                PatternValue = isAttack,
                IsAttack = isAttack,
                AttackBonus = isAttack ? 2 : 0,
                DefenseBonus = isAttack ? 0 : 2,
                StatusEffects = new StatusEffects.StatusEffect.StatusEffectData[0]
            };

            return defaultPattern;
        }
        #endregion

        #region Monster Pattern Methods
        /// <summary>
        /// 모든 몬스터 패턴 가져오기 (AI Manager용)
        /// </summary>
        public List<MonsterPatternSO> GetAllMonsterPatterns()
        {
            if (_allMonsterPatterns == null)
                BuildMonsterPatternCache();

            return _allMonsterPatterns ?? new List<MonsterPatternSO>();
        }

        /// <summary>
        /// 특정 몬스터 타입의 패턴들 가져오기
        /// </summary>
        /// <param name="monsterType">몬스터 타입 (이름 또는 타입)</param>
        /// <returns>해당 타입의 몬스터 패턴 목록</returns>
        public List<MonsterPatternSO> GetMonsterPatternsForType(string monsterType)
        {
            if (_monsterPatternsByType == null)
                BuildMonsterPatternCache();

            if (string.IsNullOrEmpty(monsterType))
                return new List<MonsterPatternSO>();

            // 정확한 일치 먼저 확인
            if (_monsterPatternsByType.TryGetValue(monsterType, out List<MonsterPatternSO> exactMatch))
            {
                return exactMatch;
            }

            // 부분 일치 검색
            var partialMatches = new List<MonsterPatternSO>();
            foreach (var kvp in _monsterPatternsByType)
            {
                if (kvp.Key.Contains(monsterType) || monsterType.Contains(kvp.Key))
                {
                    partialMatches.AddRange(kvp.Value);
                }
            }

            if (partialMatches.Count > 0)
            {
                Debug.Log($"PatternDataManager: {monsterType}에 대해 부분 일치 패턴 {partialMatches.Count}개 찾음");
                return partialMatches;
            }

            // 찾지 못했으면 기본 패턴 반환
            Debug.LogWarning($"PatternDataManager: {monsterType}에 해당하는 패턴을 찾지 못했습니다. 기본 패턴 반환");
            return GetDefaultMonsterPatterns();
        }

        /// <summary>
        /// 몬스터 이름으로 패턴 검색
        /// </summary>
        /// <param name="monsterName">몬스터 이름</param>
        /// <returns>해당 몬스터의 패턴 목록</returns>
        public List<MonsterPatternSO> GetMonsterPatternsByName(string monsterName)
        {
            if (_allMonsterPatterns == null)
                BuildMonsterPatternCache();

            return _allMonsterPatterns.Where(p => p.MonsterType.Equals(monsterName, System.StringComparison.OrdinalIgnoreCase))
                                     .ToList();
        }

        /// <summary>
        /// 특정 의도 타입의 몬스터 패턴 검색
        /// </summary>
        /// <param name="intentType">의도 타입</param>
        /// <returns>해당 의도 타입의 패턴 목록</returns>
        public List<MonsterPatternSO> GetMonsterPatternsByIntent(string intentType)
        {
            if (_allMonsterPatterns == null)
                BuildMonsterPatternCache();

            return _allMonsterPatterns.Where(p => p.IntentType.Contains(intentType))
                                     .ToList();
        }

        /// <summary>
        /// 기본 몬스터 패턴 가져오기 (패턴을 찾지 못했을 때 사용)
        /// </summary>
        private List<MonsterPatternSO> GetDefaultMonsterPatterns()
        {
            var defaultPatterns = new List<MonsterPatternSO>();

            // 가장 기본적인 패턴 몇 개 반환
            if (_allMonsterPatterns != null && _allMonsterPatterns.Count > 0)
            {
                defaultPatterns.AddRange(_allMonsterPatterns.Take(3));
            }

            return defaultPatterns;
        }

        /// <summary>
        /// 몬스터 패턴 추가 (런타임)
        /// </summary>
        /// <param name="pattern">추가할 패턴</param>
        public void AddMonsterPattern(MonsterPatternSO pattern)
        {
            if (pattern == null) return;

            if (_allMonsterPatterns == null)
                BuildMonsterPatternCache();

            // 중복 체크
            if (_allMonsterPatterns.Any(p => p.PatternName == pattern.PatternName))
            {
                Debug.LogWarning($"PatternDataManager: 같은 이름의 패턴이 이미 존재합니다 - {pattern.PatternName}");
                return;
            }

            _allMonsterPatterns.Add(pattern);

            // 타입별 캐시에도 추가
            string monsterType = pattern.MonsterType;
            if (!_monsterPatternsByType.ContainsKey(monsterType))
            {
                _monsterPatternsByType[monsterType] = new List<MonsterPatternSO>();
            }
            _monsterPatternsByType[monsterType].Add(pattern);

            Debug.Log($"PatternDataManager: 몬스터 패턴 추가됨 - {pattern.PatternName} ({monsterType})");
        }

        /// <summary>
        /// 몬스터 패턴 캐시 새로고침 (동적 로딩 후 호출)
        /// </summary>
        public void RefreshMonsterPatternCache()
        {
            BuildMonsterPatternCache();
            Debug.Log("PatternDataManager: 몬스터 패턴 캐시 새로고침 완료");
        }

        /// <summary>
        /// Resources 폴더에서 몬스터 패턴 동적 로딩
        /// </summary>
        public void LoadMonsterPatternsFromResources()
        {
            var loadedPatterns = Resources.LoadAll<MonsterPatternSO>("Patterns/Enemy");
            
            if (loadedPatterns != null && loadedPatterns.Length > 0)
            {
                var currentList = _monsterPatterns?.ToList() ?? new List<MonsterPatternSO>();
                
                foreach (var pattern in loadedPatterns)
                {
                    if (!currentList.Any(p => p.PatternName == pattern.PatternName))
                    {
                        currentList.Add(pattern);
                    }
                }
                
                _monsterPatterns = currentList.ToArray();
                RefreshMonsterPatternCache();
                
                Debug.Log($"PatternDataManager: Resources에서 {loadedPatterns.Length}개의 몬스터 패턴 로드됨");
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 캐시 구축
        /// </summary>
        private void BuildCache()
        {
            _patternCache = new Dictionary<int, PatternSO>();
            _patternsBySense = new Dictionary<SenseType, List<PatternSO>>();
            _patternsByCharacterType = new Dictionary<CharacterType, List<PatternSO>>();

            // 감각별 리스트 초기화
            _patternsBySense[SenseType.Auditory] = new List<PatternSO>();
            _patternsBySense[SenseType.Olfactory] = new List<PatternSO>();
            _patternsBySense[SenseType.Tactile] = new List<PatternSO>();
            _patternsBySense[SenseType.Spiritual] = new List<PatternSO>();

            // 캐릭터 타입별 리스트 초기화
            _patternsByCharacterType[CharacterType.Normal] = new List<PatternSO>();
            _patternsByCharacterType[CharacterType.MiniBoss] = new List<PatternSO>();
            _patternsByCharacterType[CharacterType.Boss] = new List<PatternSO>();

            // 기본 패턴 등록
            RegisterPatterns(_basicPatterns);

            // 감각별 패턴 등록
            RegisterSensePatterns(_auditoryPatterns, SenseType.Auditory);
            RegisterSensePatterns(_olfactoryPatterns, SenseType.Olfactory);
            RegisterSensePatterns(_tactilePatterns, SenseType.Tactile);
            RegisterSensePatterns(_spiritualPatterns, SenseType.Spiritual);

            // 적 패턴 등록
            RegisterEnemyPatterns(_enemyPatterns, CharacterType.Normal);
            RegisterEnemyPatterns(_bossPatterns, CharacterType.Boss);
            RegisterEnemyPatterns(_bossPatterns, CharacterType.MiniBoss); // 미니보스도 보스 패턴 사용 가능
        }

        /// <summary>
        /// 몬스터 패턴 캐시 구축
        /// </summary>
        private void BuildMonsterPatternCache()
        {
            _monsterPatternsByType = new Dictionary<string, List<MonsterPatternSO>>();
            _allMonsterPatterns = new List<MonsterPatternSO>();

            if (_monsterPatterns == null || _monsterPatterns.Length == 0)
            {
                Debug.LogWarning("PatternDataManager: 몬스터 패턴이 설정되지 않았습니다. Resources에서 로드를 시도합니다.");
                LoadMonsterPatternsFromResources();
                return;
            }

            foreach (var pattern in _monsterPatterns)
            {
                if (pattern == null) continue;

                _allMonsterPatterns.Add(pattern);

                string monsterType = pattern.MonsterType;
                if (string.IsNullOrEmpty(monsterType))
                {
                    monsterType = "Default";
                }

                if (!_monsterPatternsByType.ContainsKey(monsterType))
                {
                    _monsterPatternsByType[monsterType] = new List<MonsterPatternSO>();
                }

                _monsterPatternsByType[monsterType].Add(pattern);
            }

            Debug.Log($"PatternDataManager: 몬스터 패턴 캐시 구축 완료 - {_allMonsterPatterns.Count}개 패턴, {_monsterPatternsByType.Count}개 타입");
        }

        /// <summary>
        /// 패턴 배열을 등록
        /// </summary>
        private void RegisterPatterns(PatternSO[] patterns)
        {
            if (patterns == null) return;

            foreach (PatternSO pattern in patterns)
            {
                if (pattern == null) continue;

                if (!_patternCache.ContainsKey(pattern.id))
                {
                    _patternCache[pattern.id] = pattern;
                }
                else
                {
                    Debug.LogWarning($"Duplicate pattern ID found: {pattern.id} ({pattern.patternName})");
                }
            }
        }

        /// <summary>
        /// 감각별 패턴 등록
        /// </summary>
        private void RegisterSensePatterns(PatternSO[] patterns, SenseType senseType)
        {
            if (patterns == null) return;

            RegisterPatterns(patterns); // 기본 등록도 수행

            foreach (PatternSO pattern in patterns)
            {
                if (pattern != null)
                {
                    _patternsBySense[senseType].Add(pattern);
                }
            }
        }

        /// <summary>
        /// 적 패턴 등록
        /// </summary>
        private void RegisterEnemyPatterns(PatternSO[] patterns, CharacterType characterType)
        {
            if (patterns == null) return;

            RegisterPatterns(patterns); // 기본 등록도 수행

            foreach (PatternSO pattern in patterns)
            {
                if (pattern != null)
                {
                    _patternsByCharacterType[characterType].Add(pattern);
                }
            }
        }
        #endregion

        #region Editor Validation
        #if UNITY_EDITOR
        private void OnValidate()
        {
            ValidatePatternData();
        }

        private void ValidatePatternData()
        {
            // ID 중복 검사
            HashSet<int> usedIds = new HashSet<int>();
            List<PatternSO[]> allPatternArrays = new List<PatternSO[]>
            {
                _basicPatterns, _auditoryPatterns, _olfactoryPatterns, 
                _tactilePatterns, _spiritualPatterns, _enemyPatterns, _bossPatterns
            };

            foreach (var patternArray in allPatternArrays)
            {
                if (patternArray == null) continue;

                foreach (var pattern in patternArray)
                {
                    if (pattern == null) continue;

                    if (usedIds.Contains(pattern.id))
                    {
                        Debug.LogWarning($"Duplicate pattern ID: {pattern.id} ({pattern.patternName})");
                    }
                    else
                    {
                        usedIds.Add(pattern.id);
                    }
                }
            }

            // 몬스터 패턴 검증
            if (_monsterPatterns != null)
            {
                HashSet<string> usedNames = new HashSet<string>();
                
                foreach (var pattern in _monsterPatterns)
                {
                    if (pattern == null) continue;

                    if (usedNames.Contains(pattern.PatternName))
                    {
                        Debug.LogWarning($"Duplicate monster pattern name: {pattern.PatternName}");
                    }
                    else
                    {
                        usedNames.Add(pattern.PatternName);
                    }
                }
            }
        }
        #endif
        #endregion
    }
}