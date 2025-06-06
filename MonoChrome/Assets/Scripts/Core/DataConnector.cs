using System.Collections.Generic;
using MonoChrome.Combat;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 데이터 연결 매니저 - ScriptableObject와 런타임 시스템 연결
    /// 포트폴리오 품질을 위한 체계적인 데이터 관리
    /// </summary>
    public class DataConnector : MonoBehaviour
    {
        #region Singleton
        private static DataConnector _instance;
        public static DataConnector Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DataConnector");
                    _instance = go.AddComponent<DataConnector>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDataConnector();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion
        
        #region Data Managers
        private CharacterDataManager _characterDataManager;
        private PatternDataManager _patternDataManager;
        private bool _isInitialized = false;
        #endregion
        
        #region Initialization
        /// <summary>
        /// 데이터 커넥터 초기화
        /// </summary>
        private void InitializeDataConnector()
        {
            Debug.Log("DataConnector: 초기화 시작");
            
            // ScriptableObject 매니저들 로드
            LoadDataManagers();
            
            // 매니저들 초기화
            InitializeManagers();
            
            _isInitialized = true;
            Debug.Log("DataConnector: 초기화 완료");
        }
        
        /// <summary>
        /// 데이터 매니저들 로드
        /// </summary>
        private void LoadDataManagers()
        {
            // CharacterDataManager 로드
            _characterDataManager = Resources.Load<CharacterDataManager>("CharacterDataManager");
            if (_characterDataManager == null)
            {
                Debug.LogError("DataConnector: CharacterDataManager를 찾을 수 없습니다. Resources 폴더에 생성하세요.");
                CreateEmptyCharacterDataManager();
            }
            else
            {
                Debug.Log("DataConnector: CharacterDataManager 로드 완료");
            }
            
            // PatternDataManager 로드
            _patternDataManager = Resources.Load<PatternDataManager>("PatternDataManager");
            if (_patternDataManager == null)
            {
                Debug.LogError("DataConnector: PatternDataManager를 찾을 수 없습니다. Resources 폴더에 생성하세요.");
                CreateEmptyPatternDataManager();
            }
            else
            {
                Debug.Log("DataConnector: PatternDataManager 로드 완료");
            }
        }
        
        /// <summary>
        /// 매니저들 초기화
        /// </summary>
        private void InitializeManagers()
        {
            if (_characterDataManager != null)
            {
                _characterDataManager.Initialize();
            }
            
            if (_patternDataManager != null)
            {
                _patternDataManager.Initialize();
            }
        }
        
        /// <summary>
        /// 빈 CharacterDataManager 생성 (응급용)
        /// </summary>
        private void CreateEmptyCharacterDataManager()
        {
            Debug.LogWarning("DataConnector: 응급용 CharacterDataManager 생성");
            // 런타임에는 생성할 수 없으므로 경고만 출력
        }
        
        /// <summary>
        /// 빈 PatternDataManager 생성 (응급용)
        /// </summary>
        private void CreateEmptyPatternDataManager()
        {
            Debug.LogWarning("DataConnector: 응급용 PatternDataManager 생성");
            // 런타임에는 생성할 수 없으므로 경고만 출력
        }
        #endregion
        
        #region Character Data Interface
        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        public PlayerCharacter CreatePlayerCharacter(string characterName)
        {
            if (!_isInitialized || _characterDataManager == null)
            {
                Debug.LogError("DataConnector: CharacterDataManager가 초기화되지 않았습니다.");
                return CreateFallbackPlayerCharacter(characterName);
            }
            
            PlayerCharacter character = _characterDataManager.CreatePlayerCharacter(characterName);
            if (character == null)
            {
                Debug.LogWarning($"DataConnector: {characterName} 캐릭터 데이터를 찾을 수 없습니다. 기본 캐릭터를 생성합니다.");
                return CreateFallbackPlayerCharacter(characterName);
            }
            
            Debug.Log($"DataConnector: 플레이어 캐릭터 생성 완료 - {characterName}");
            return character;
        }
        
        /// <summary>
        /// 적 캐릭터 생성
        /// </summary>
        public EnemyCharacter CreateEnemyCharacter(string characterName)
        {
            if (!_isInitialized || _characterDataManager == null)
            {
                Debug.LogError("DataConnector: CharacterDataManager가 초기화되지 않았습니다.");
                return CreateFallbackEnemyCharacter(characterName);
            }
            
            EnemyCharacter character = _characterDataManager.CreateEnemyCharacter(characterName);
            if (character == null)
            {
                Debug.LogWarning($"DataConnector: {characterName} 적 캐릭터 데이터를 찾을 수 없습니다. 기본 적을 생성합니다.");
                return CreateFallbackEnemyCharacter(characterName);
            }
            
            Debug.Log($"DataConnector: 적 캐릭터 생성 완료 - {characterName}");
            return character;
        }
        
        /// <summary>
        /// 랜덤 적 캐릭터 생성
        /// </summary>
        public EnemyCharacter CreateRandomEnemyCharacter(CharacterType enemyType = CharacterType.Normal)
        {
            if (!_isInitialized || _characterDataManager == null)
            {
                return CreateFallbackEnemyCharacter("랜덤 적");
            }
            
            CharacterDataSO enemyData = _characterDataManager.GetRandomEnemy(enemyType);
            if (enemyData == null)
            {
                return CreateFallbackEnemyCharacter("랜덤 적");
            }
            
            return enemyData.CreateEnemyCharacter();
        }
        
        /// <summary>
        /// 플레이어블 캐릭터 리스트 가져오기
        /// </summary>
        public List<CharacterDataSO> GetPlayableCharacters()
        {
            if (!_isInitialized || _characterDataManager == null)
            {
                return new List<CharacterDataSO>();
            }
            
            return _characterDataManager.GetPlayableCharacters();
        }
        #endregion
        
        #region Pattern Data Interface
        /// <summary>
        /// 사용 가능한 패턴들 가져오기
        /// </summary>
        public List<Pattern> GetAvailablePatterns(SenseType senseType, bool[] coinStates)
        {
            if (!_isInitialized || _patternDataManager == null)
            {
                Debug.LogWarning("DataConnector: PatternDataManager가 초기화되지 않았습니다. 기본 패턴을 반환합니다.");
                return GetFallbackPatterns(coinStates);
            }
            
            List<PatternSO> patternSOs = _patternDataManager.GetAvailablePatterns(senseType, coinStates);
            List<Pattern> patterns = _patternDataManager.ConvertToPatterns(patternSOs);
            
            if (patterns.Count == 0)
            {
                Debug.LogWarning("DataConnector: 사용 가능한 패턴이 없습니다. 기본 패턴을 반환합니다.");
                return GetFallbackPatterns(coinStates);
            }
            
            Debug.Log($"DataConnector: {patterns.Count}개의 패턴을 찾았습니다.");
            return patterns;
        }
        
        /// <summary>
        /// 적용 패턴들 가져오기
        /// </summary>
        public List<Pattern> GetEnemyPatterns(CharacterType enemyType, bool[] coinStates)
        {
            if (!_isInitialized || _patternDataManager == null)
            {
                return GetFallbackPatterns(coinStates);
            }
            
            List<PatternSO> patternSOs = _patternDataManager.GetEnemyPatterns(enemyType, coinStates);
            List<Pattern> patterns = _patternDataManager.ConvertToPatterns(patternSOs);
            
            if (patterns.Count == 0)
            {
                return GetFallbackPatterns(coinStates);
            }
            
            return patterns;
        }
        
        /// <summary>
        /// ID로 패턴 가져오기
        /// </summary>
        public Pattern GetPatternById(int patternId)
        {
            if (!_isInitialized || _patternDataManager == null)
            {
                return null;
            }
            
            PatternSO patternSO = _patternDataManager.GetPattern(patternId);
            return patternSO?.ToPattern();
        }
        #endregion
        
        #region Fallback Methods
        /// <summary>
        /// 기본 플레이어 캐릭터 생성 (응급용)
        /// </summary>
        private PlayerCharacter CreateFallbackPlayerCharacter(string name)
        {
            Debug.LogWarning($"DataConnector: 기본 플레이어 캐릭터 생성 - {name}");
            return new PlayerCharacter(
                name,
                SenseType.Auditory,
                80, // maxHealth
                5,  // attackPower
                5   // defensePower
            );
        }
        
        /// <summary>
        /// 기본 적 캐릭터 생성 (응급용)
        /// </summary>
        private EnemyCharacter CreateFallbackEnemyCharacter(string name)
        {
            Debug.LogWarning($"DataConnector: 기본 적 캐릭터 생성 - {name}");
            return new EnemyCharacter(
                name,
                CharacterType.Normal,
                60, // maxHealth
                6,  // attackPower
                3,  // defensePower
                StatusEffectType.Mark,
                StatusEffectType.Bleed
            );
        }
        
        /// <summary>
        /// 기본 패턴들 생성 (응급용)
        /// </summary>
        private List<Pattern> GetFallbackPatterns(bool[] coinStates)
        {
            List<Pattern> patterns = new List<Pattern>();
            
            if (coinStates == null || coinStates.Length < 2)
            {
                // 기본 패턴 하나라도 반환
                patterns.Add(new Pattern
                {
                    ID = 1,
                    Name = "기본 공격",
                    Description = "간단한 공격",
                    IsAttack = true,
                    PatternType = PatternType.Consecutive2,
                    PatternValue = true,
                    AttackBonus = 2,
                    DefenseBonus = 0
                });
                return patterns;
            }
            
            // 동전 상태를 분석해서 가능한 기본 패턴들 생성
            if (HasConsecutivePattern(coinStates, true, 2))
            {
                patterns.Add(new Pattern
                {
                    ID = 1,
                    Name = "앞면 2연",
                    Description = "기본 공격 패턴",
                    IsAttack = true,
                    PatternType = PatternType.Consecutive2,
                    PatternValue = true,
                    AttackBonus = 2,
                    DefenseBonus = 0
                });
            }
            
            if (HasConsecutivePattern(coinStates, false, 2))
            {
                patterns.Add(new Pattern
                {
                    ID = 2,
                    Name = "뒷면 2연",
                    Description = "기본 방어 패턴",
                    IsAttack = false,
                    PatternType = PatternType.Consecutive2,
                    PatternValue = false,
                    AttackBonus = 0,
                    DefenseBonus = 2
                });
            }
            
            if (HasConsecutivePattern(coinStates, true, 3))
            {
                patterns.Add(new Pattern
                {
                    ID = 3,
                    Name = "앞면 3연",
                    Description = "강력한 공격",
                    IsAttack = true,
                    PatternType = PatternType.Consecutive3,
                    PatternValue = true,
                    AttackBonus = 4,
                    DefenseBonus = 0
                });
            }
            
            if (HasConsecutivePattern(coinStates, false, 3))
            {
                patterns.Add(new Pattern
                {
                    ID = 4,
                    Name = "뒷면 3연",
                    Description = "강력한 방어",
                    IsAttack = false,
                    PatternType = PatternType.Consecutive3,
                    PatternValue = false,
                    AttackBonus = 0,
                    DefenseBonus = 4
                });
            }
            
            Debug.Log($"DataConnector: {patterns.Count}개의 기본 패턴 생성");
            return patterns;
        }
        
        /// <summary>
        /// 연속 패턴 확인
        /// </summary>
        private bool HasConsecutivePattern(bool[] coinStates, bool targetValue, int length)
        {
            if (coinStates.Length < length) return false;
            
            int consecutiveCount = 0;
            foreach (bool coin in coinStates)
            {
                if (coin == targetValue)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= length)
                        return true;
                }
                else
                {
                    consecutiveCount = 0;
                }
            }
            
            return false;
        }
        #endregion
        
        #region Public Status Methods
        /// <summary>
        /// 초기화 상태 확인
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 데이터 매니저 상태 확인
        /// </summary>
        public bool HasValidManagers => _characterDataManager != null && _patternDataManager != null;
        
        /// <summary>
        /// 데이터 연결 상태 리포트
        /// </summary>
        public void GenerateStatusReport()
        {
            Debug.Log("=== DataConnector 상태 리포트 ===");
            Debug.Log($"초기화됨: {_isInitialized}");
            Debug.Log($"CharacterDataManager: {(_characterDataManager != null ? "로드됨" : "없음")}");
            Debug.Log($"PatternDataManager: {(_patternDataManager != null ? "로드됨" : "없음")}");
            
            if (_characterDataManager != null)
            {
                var players = _characterDataManager.GetPlayableCharacters();
                Debug.Log($"플레이어블 캐릭터: {players.Count}개");
            }
            
            Debug.Log("========================");
        }
        #endregion


    }
}
