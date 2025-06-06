using System.Collections.Generic;
using System.Linq;
using MonoChrome.Systems.Combat;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 패턴(족보) 데이터를 중앙에서 관리하는 매니저
    /// ScriptableObject 기반으로 데이터를 로드하고 캐시합니다.
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

        // 캐시된 데이터
        private Dictionary<int, PatternSO> _patternCache;
        private Dictionary<SenseType, List<PatternSO>> _patternsBySense;
        private Dictionary<CharacterType, List<PatternSO>> _patternsByCharacterType;

        #region Public Methods
        /// <summary>
        /// 매니저 초기화
        /// </summary>
        public void Initialize()
        {
            BuildCache();
            Debug.Log($"PatternDataManager initialized with {_patternCache.Count} patterns");
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

            // 기본 패턴 검사
            if (_basicPatterns != null)
            {
                foreach (PatternSO pattern in _basicPatterns)
                {
                    if (pattern != null && pattern.IsApplicableTo(CharacterType.Player, senseType) 
                        && pattern.ValidatePattern(coinStates))
                    {
                        availablePatterns.Add(pattern);
                    }
                }
            }

            // 감각별 특수 패턴 검사
            if (_patternsBySense.TryGetValue(senseType, out List<PatternSO> sensePatterns))
            {
                foreach (PatternSO pattern in sensePatterns)
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
        }
        #endif
        #endregion
    }
}
