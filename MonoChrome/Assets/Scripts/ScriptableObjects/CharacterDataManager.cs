using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 캐릭터 데이터를 중앙에서 관리하는 매니저
    /// ScriptableObject 기반으로 데이터를 로드하고 캐시합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterDataManager", menuName = "MonoChrome/Managers/Character Data Manager")]
    public class CharacterDataManager : ScriptableObject
    {
        #region Singleton Pattern for Runtime
        private static CharacterDataManager _instance;
        public static CharacterDataManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CharacterDataManager>("CharacterDataManager");
                    if (_instance == null)
                    {
                        Debug.LogError("CharacterDataManager not found in Resources folder!");
                    }
                }
                return _instance;
            }
        }
        #endregion

        [Header("플레이어블 캐릭터")]
        [SerializeField] private CharacterDataSO[] _playerCharacters;
        
        [Header("적 캐릭터")]
        [SerializeField] private CharacterDataSO[] _normalEnemies;
        [SerializeField] private CharacterDataSO[] _miniBosses;
        [SerializeField] private CharacterDataSO[] _bosses;

        // 캐시된 데이터
        private Dictionary<string, CharacterDataSO> _characterCache;
        private Dictionary<CharacterType, List<CharacterDataSO>> _characterByType;

        #region Public Methods
        /// <summary>
        /// 매니저 초기화 (처음 로드 시 호출)
        /// </summary>
        public void Initialize()
        {
            BuildCache();
            Debug.Log($"CharacterDataManager initialized with {_characterCache.Count} characters");
        }

        /// <summary>
        /// 이름으로 캐릭터 데이터 가져오기
        /// </summary>
        public CharacterDataSO GetCharacterData(string characterName)
        {
            if (_characterCache == null)
                BuildCache();

            if (_characterCache.TryGetValue(characterName, out CharacterDataSO data))
            {
                return data;
            }

            Debug.LogWarning($"Character data not found: {characterName}");
            return null;
        }

        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        public PlayerCharacter CreatePlayerCharacter(string characterName)
        {
            CharacterDataSO data = GetCharacterData(characterName);
            if (data == null || data.characterType != CharacterType.Player)
            {
                Debug.LogError($"Invalid player character data: {characterName}");
                return null;
            }

            return data.CreatePlayerCharacter();
        }

        /// <summary>
        /// 적 캐릭터 생성
        /// </summary>
        public EnemyCharacter CreateEnemyCharacter(string characterName)
        {
            CharacterDataSO data = GetCharacterData(characterName);
            if (data == null || data.characterType == CharacterType.Player)
            {
                Debug.LogError($"Invalid enemy character data: {characterName}");
                return null;
            }

            return data.CreateEnemyCharacter();
        }

        /// <summary>
        /// 캐릭터 타입별 리스트 가져오기
        /// </summary>
        public List<CharacterDataSO> GetCharactersByType(CharacterType type)
        {
            if (_characterByType == null)
                BuildCache();

            if (_characterByType.TryGetValue(type, out List<CharacterDataSO> characters))
            {
                return new List<CharacterDataSO>(characters);
            }

            return new List<CharacterDataSO>();
        }

        /// <summary>
        /// 모든 플레이어블 캐릭터 가져오기
        /// </summary>
        public List<CharacterDataSO> GetPlayableCharacters()
        {
            return GetCharactersByType(CharacterType.Player);
        }

        /// <summary>
        /// 랜덤 적 캐릭터 가져오기
        /// </summary>
        public CharacterDataSO GetRandomEnemy(CharacterType enemyType = CharacterType.Normal)
        {
            List<CharacterDataSO> enemies = GetCharactersByType(enemyType);
            if (enemies.Count == 0)
            {
                Debug.LogWarning($"No enemies found for type: {enemyType}");
                return null;
            }

            return enemies[Random.Range(0, enemies.Count)];
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// 캐시 구축
        /// </summary>
        private void BuildCache()
        {
            _characterCache = new Dictionary<string, CharacterDataSO>();
            _characterByType = new Dictionary<CharacterType, List<CharacterDataSO>>();

            // 모든 캐릭터 데이터 수집
            List<CharacterDataSO> allCharacters = new List<CharacterDataSO>();
            
            if (_playerCharacters != null)
                allCharacters.AddRange(_playerCharacters);
            
            if (_normalEnemies != null)
                allCharacters.AddRange(_normalEnemies);
                
            if (_miniBosses != null)
                allCharacters.AddRange(_miniBosses);
                
            if (_bosses != null)
                allCharacters.AddRange(_bosses);

            // 캐시 구축
            foreach (CharacterDataSO character in allCharacters)
            {
                if (character == null) continue;

                // 이름별 캐시
                if (!_characterCache.ContainsKey(character.characterName))
                {
                    _characterCache[character.characterName] = character;
                }
                else
                {
                    Debug.LogWarning($"Duplicate character name found: {character.characterName}");
                }

                // 타입별 캐시
                if (!_characterByType.ContainsKey(character.characterType))
                {
                    _characterByType[character.characterType] = new List<CharacterDataSO>();
                }
                _characterByType[character.characterType].Add(character);
            }
        }
        #endregion

        #region Editor Validation
        #if UNITY_EDITOR
        private void OnValidate()
        {
            // 에디터에서 데이터 검증
            ValidateCharacterData();
        }

        private void ValidateCharacterData()
        {
            // 플레이어 캐릭터 검증
            if (_playerCharacters != null)
            {
                foreach (var character in _playerCharacters)
                {
                    if (character != null && character.characterType != CharacterType.Player)
                    {
                        Debug.LogWarning($"Player character array contains non-player character: {character.characterName}");
                    }
                }
            }

            // 적 캐릭터 검증
            if (_normalEnemies != null)
            {
                foreach (var enemy in _normalEnemies)
                {
                    if (enemy != null && enemy.characterType != CharacterType.Normal)
                    {
                        Debug.LogWarning($"Normal enemy array contains non-normal character: {enemy.characterName}");
                    }
                }
            }

            if (_miniBosses != null)
            {
                foreach (var boss in _miniBosses)
                {
                    if (boss != null && boss.characterType != CharacterType.MiniBoss)
                    {
                        Debug.LogWarning($"Mini boss array contains non-mini boss character: {boss.characterName}");
                    }
                }
            }

            if (_bosses != null)
            {
                foreach (var boss in _bosses)
                {
                    if (boss != null && boss.characterType != CharacterType.Boss)
                    {
                        Debug.LogWarning($"Boss array contains non-boss character: {boss.characterName}");
                    }
                }
            }
        }
        #endif
        #endregion
    }
}
