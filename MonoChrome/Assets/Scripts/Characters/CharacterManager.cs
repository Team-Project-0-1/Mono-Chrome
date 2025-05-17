using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome;
using MonoChrome.Combat;
using MonoChrome.Extensions;


namespace MonoChrome
{
    /// <summary>
    /// 캐릭터 생성과 관리를 담당하는 싱글톤 매니저 클래스
    /// </summary>
    public class CharacterManager : MonoBehaviour
    {
        #region Singleton
        private static CharacterManager _instance;
        
        public static CharacterManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("CharacterManager");
                    _instance = go.AddComponent<CharacterManager>();
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
            
            // 초기화
            Initialize();
        }
        #endregion
        
        #region Character Management
        // 현재 플레이어 캐릭터
        private PlayerCharacter _playerCharacter;
        
        // 생성한 적 캐릭터 캐시
        private Dictionary<string, EnemyCharacter> _enemyCache = new Dictionary<string, EnemyCharacter>();
        
        // 프로퍼티
        public PlayerCharacter CurrentPlayer => _playerCharacter;
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            Debug.Log("CharacterManager: Initializing...");
        }
        
        /// <summary>
        /// 플레이어 캐릭터 생성
        /// </summary>
        /// <param name="senseType">감각 유형</param>
        /// <param name="name">캐릭터 이름 (옵션)</param>
        /// <returns>생성된 플레이어 캐릭터</returns>
        public PlayerCharacter CreatePlayerCharacter(SenseType senseType, string name = null)
        {
            // 캐릭터 이름 설정
            string characterName = string.IsNullOrEmpty(name) ? GetDefaultPlayerName(senseType) : name;
            
            // 유형에 따른 기본 능력치 설정
            int maxHealth = 100;
            int attackPower = 10;
            int defensePower = 5;
            
            // 유형에 따른 능력치 조정
            switch (senseType)
            {
                case SenseType.Auditory: // 청각 - 공/방 균형
                    maxHealth = 100;
                    attackPower = 10;
                    defensePower = 5;
                    break;
                    
                case SenseType.Olfactory: // 후각 - 공격 특화
                    maxHealth = 90;
                    attackPower = 12;
                    defensePower = 4;
                    break;
                    
                case SenseType.Tactile: // 촉각 - 방어 특화
                    maxHealth = 110;
                    attackPower = 8;
                    defensePower = 8;
                    break;
                    
                case SenseType.Spiritual: // 영적 - 특수 능력 특화
                    maxHealth = 80;
                    attackPower = 11;
                    defensePower = 3;
                    break;
            }
            
            // 플레이어 캐릭터 생성
            _playerCharacter = new PlayerCharacter(
                characterName,
                senseType,
                maxHealth,
                attackPower,
                defensePower
            );
            
            Debug.Log($"CharacterManager: Created player character: {characterName}, Sense: {senseType}");
            
            // CombatManager에 플레이어 캐릭터 설정
            SetPlayerInCombatManager();
            
            return _playerCharacter;
        }
        
        /// <summary>
        /// 적 캐릭터 생성
        /// </summary>
        /// <param name="enemyType">적 유형</param>
        /// <param name="type">캐릭터 등급 (일반/엘리트/보스 등)</param>
        /// <returns>생성된 적 캐릭터</returns>
        public EnemyCharacter CreateEnemyCharacter(string enemyType, CharacterType type = CharacterType.Normal)
        {
            // 캐시에 있는지 확인
            string cacheKey = $"{enemyType}_{type}";
            if (_enemyCache.ContainsKey(cacheKey))
            {
                Debug.Log($"CharacterManager: Returning cached enemy: {enemyType}");
                return _enemyCache[cacheKey];
            }
            
            // 적 정보 설정
            string name = enemyType;
            int maxHealth = 50;
            int attackPower = 5;
            int defensePower = 3;
            StatusEffectType primaryEffect = StatusEffectType.None;
            StatusEffectType secondaryEffect = StatusEffectType.None;
            
            // 유형에 따른 설정
            switch (enemyType)
            {
                case "루멘 리퍼":
                    maxHealth = 80;
                    attackPower = 8;
                    defensePower = 3;
                    primaryEffect = StatusEffectType.Mark;
                    secondaryEffect = StatusEffectType.Bleed;
                    break;
                    
                case "그림자 수호자":
                    maxHealth = 100;
                    attackPower = 6;
                    defensePower = 7;
                    primaryEffect = StatusEffectType.Counter;
                    secondaryEffect = StatusEffectType.Crush;
                    break;
                    
                case "암흑 마법사":
                    maxHealth = 70;
                    attackPower = 10;
                    defensePower = 2;
                    primaryEffect = StatusEffectType.Curse;
                    secondaryEffect = StatusEffectType.Seal;
                    break;
                    
                case "검은 심연":
                    // 보스 몬스터
                    maxHealth = 150;
                    attackPower = 12;
                    defensePower = 8;
                    primaryEffect = StatusEffectType.Curse;
                    secondaryEffect = StatusEffectType.Resonance;
                    break;
                    
                default:
                    // 기본 적
                    name = "알 수 없는 적";
                    maxHealth = 50;
                    attackPower = 5;
                    defensePower = 3;
                    primaryEffect = StatusEffectType.None;
                    secondaryEffect = StatusEffectType.None;
                    break;
            }
            
            // 적 등급에 따른 능력치 조정
            if (type.IsElite())
            {
                maxHealth = (int)(maxHealth * 1.3f);
                attackPower = (int)(attackPower * 1.2f);
                defensePower = (int)(defensePower * 1.2f);
            }
            else if (type.IsMiniBoss())
            {
                maxHealth = (int)(maxHealth * 1.5f);
                attackPower = (int)(attackPower * 1.3f);
                defensePower = (int)(defensePower * 1.3f);
            }
            else if (type.IsBoss())
            {
                maxHealth = (int)(maxHealth * 2.0f);
                attackPower = (int)(attackPower * 1.5f);
                defensePower = (int)(defensePower * 1.5f);
            }
            
            // 적 캐릭터 생성
            EnemyCharacter enemy = new EnemyCharacter(
                name,
                type,
                maxHealth,
                attackPower,
                defensePower,
                primaryEffect,
                secondaryEffect
            );
            
            // 캐시에 저장
            _enemyCache[cacheKey] = enemy;
            
            Debug.Log($"CharacterManager: Created enemy: {name}, Type: {type}");
            
            return enemy;
        }
        
        /// <summary>
        /// CombatManager에 플레이어 캐릭터 설정
        /// </summary>
        private void SetPlayerInCombatManager()
        {
            if (_playerCharacter == null)
            {
                Debug.LogError("CharacterManager: Cannot set player in CombatManager - player is null");
                return;
            }
            
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("CharacterManager: GameManager instance is null");
                return;
            }
            
            CombatManager combatManager = gameManager.CombatManager;
            if (combatManager == null)
            {
                Debug.LogError("CharacterManager: CombatManager reference is null from GameManager");
                return;
            }
            
            combatManager.SetPlayerCharacter(_playerCharacter);
            Debug.Log("CharacterManager: Set player character in CombatManager");
        }
        
        /// <summary>
        /// 감각 유형에 따른 기본 이름 가져오기
        /// </summary>
        private string GetDefaultPlayerName(SenseType senseType)
        {
            switch (senseType)
            {
                case SenseType.Auditory:
                    return "소리를 듣는 자";
                    
                case SenseType.Olfactory:
                    return "냄새를 맡는 자";
                    
                case SenseType.Tactile:
                    return "만지는 자";
                    
                case SenseType.Spiritual:
                    return "영혼을 보는 자";
                    
                default:
                    return "모험가";
            }
        }
        #endregion
    }
}
