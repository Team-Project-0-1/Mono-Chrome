using UnityEngine;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 스테이지별 테마 데이터를 관리하는 ScriptableObject
    /// 포트폴리오급 데이터 관리 시스템으로 디자이너 친화적 구성
    /// </summary>
    [CreateAssetMenu(fileName = "New Stage Theme", menuName = "MONOCHROME/Dungeon/Stage Theme Data")]
    public class StageThemeDataAsset : ScriptableObject
    {
        [Header("== 스테이지 기본 정보 ==")]
        [SerializeField] private string stageDisplayName = "스테이지 1";
        [SerializeField] private string stageDescription = "벙커 외곽 - 돌연변이들과 자연재해가 기다리는 곳";
        [SerializeField] private Sprite stageIcon;
        [SerializeField] private Color stageColor = Color.white;
        
        [Header("== 던전 구조 설정 ==")]
        [SerializeField] private int totalTurns = 15;
        [SerializeField] private int branchesPerTurn = 3;
        [SerializeField] private int miniBossPosition = 6; // 6번째 턴에 미니보스
        
        [Header("== 노드 타입 확률 ==")]
        [Range(0, 1)] [SerializeField] private float combatChance = 0.4f;
        [Range(0, 1)] [SerializeField] private float eventChance = 0.3f;
        [Range(0, 1)] [SerializeField] private float shopChance = 0.15f;
        [Range(0, 1)] [SerializeField] private float restChance = 0.15f;
        
        [Header("== 보장 요소 ==")]
        [SerializeField] private int guaranteedShops = 1;
        [SerializeField] private int guaranteedRests = 1;
        [SerializeField] private int maxEvents = 3;
        [SerializeField] private int minCombatRooms = 5;
        
        [Header("== 특수 설정 ==")]
        [SerializeField] private bool allowMultipleMiniBosses = false;
        [SerializeField] private bool forceShopBeforeMiniBoss = true;
        [SerializeField] private bool forceRestAfterMiniBoss = true;
        
        [Header("== 감각 기반 필터 ==")]
        [SerializeField] private SenseFilterConfig[] senseFilters;
        
        [Header("== 적 & 이벤트 풀 ==")]
        [SerializeField] private EnemySpawnData[] enemyPool;
        [SerializeField] private EventSpawnData[] eventPool;
        
        #region Data Structures
        [System.Serializable]
        public class SenseFilterConfig
        {
            [Header("감각 필터 설정")]
            public SenseType targetSense;
            public NodeType[] applicableNodeTypes;
            [Range(0, 1)] public float bonusSuccessChance = 0.2f;
            public string hintText = "이상한 기운이 느껴진다...";
            public bool providesExtraReward = false;
        }
        
        [System.Serializable]
        public class EnemySpawnData
        {
            [Header("적 정보")]
            public string enemyName;
            public CharacterType enemyType;
            public int spawnWeight = 1; // 가중치 (높을수록 자주 등장)
            [Range(0, 1)] public float spawnChance = 1.0f;
            public int minStageLevel = 1; // 최소 등장 스테이지
            public string description;
        }
        
        [System.Serializable]
        public class EventSpawnData
        {
            [Header("이벤트 정보")]
            public string eventName;
            public int eventID;
            public int spawnWeight = 1;
            [Range(0, 1)] public float spawnChance = 1.0f;
            public SenseType[] requiredSenses; // 필요한 감각들
            public string eventDescription;
        }
        #endregion
        
        #region Properties
        public string StageDisplayName => stageDisplayName;
        public string StageDescription => stageDescription;
        public Sprite StageIcon => stageIcon;
        public Color StageColor => stageColor;
        
        public int TotalTurns => totalTurns;
        public int BranchesPerTurn => branchesPerTurn;
        public int MiniBossPosition => miniBossPosition;
        
        public float CombatChance => combatChance;
        public float EventChance => eventChance;
        public float ShopChance => shopChance;
        public float RestChance => restChance;
        
        public int GuaranteedShops => guaranteedShops;
        public int GuaranteedRests => guaranteedRests;
        public int MaxEvents => maxEvents;
        public int MinCombatRooms => minCombatRooms;
        
        public bool AllowMultipleMiniBosses => allowMultipleMiniBosses;
        public bool ForceShopBeforeMiniBoss => forceShopBeforeMiniBoss;
        public bool ForceRestAfterMiniBoss => forceRestAfterMiniBoss;
        
        public SenseFilterConfig[] SenseFilters => senseFilters;
        public EnemySpawnData[] EnemyPool => enemyPool;
        public EventSpawnData[] EventPool => eventPool;
        #endregion
        
        #region Validation
        /// <summary>
        /// 데이터 유효성 검증
        /// </summary>
        [ContextMenu("Validate Data")]
        public bool ValidateData()
        {
            bool isValid = true;
            
            // 확률 합계 검증
            float totalChance = combatChance + eventChance + shopChance + restChance;
            if (Mathf.Abs(totalChance - 1.0f) > 0.01f)
            {
                Debug.LogWarning($"{name}: 노드 타입 확률 합계가 1.0이 아닙니다! (현재: {totalChance:F2})");
                isValid = false;
            }
            
            // 미니보스 위치 검증
            if (miniBossPosition <= 0 || miniBossPosition >= totalTurns)
            {
                Debug.LogWarning($"{name}: 미니보스 위치가 올바르지 않습니다! (현재: {miniBossPosition}, 범위: 1-{totalTurns-1})");
                isValid = false;
            }
            
            // 보장 요소 검증
            int totalGuaranteed = guaranteedShops + guaranteedRests + 1; // +1은 미니보스
            if (totalGuaranteed > totalTurns * branchesPerTurn)
            {
                Debug.LogWarning($"{name}: 보장 요소가 너무 많습니다! (보장: {totalGuaranteed}, 총 슬롯: {totalTurns * branchesPerTurn})");
                isValid = false;
            }
            
            // 적 풀 검증
            if (enemyPool == null || enemyPool.Length == 0)
            {
                Debug.LogWarning($"{name}: 적 풀이 비어있습니다!");
                isValid = false;
            }
            
            // 이벤트 풀 검증
            if (eventPool == null || eventPool.Length == 0)
            {
                Debug.LogWarning($"{name}: 이벤트 풀이 비어있습니다!");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log($"{name}: 데이터 검증 통과!");
            }
            
            return isValid;
        }
        
        /// <summary>
        /// 확률 정규화
        /// </summary>
        [ContextMenu("Normalize Chances")]
        public void NormalizeChances()
        {
            float total = combatChance + eventChance + shopChance + restChance;
            if (total > 0)
            {
                combatChance /= total;
                eventChance /= total;
                shopChance /= total;
                restChance /= total;
                
                Debug.Log($"{name}: 확률 정규화 완료!");
            }
        }
        #endregion
        
        #region Helper Methods
        /// <summary>
        /// 가중치 기반 적 선택
        /// </summary>
        public EnemySpawnData GetRandomEnemy(int currentStageLevel = 1)
        {
            if (enemyPool == null || enemyPool.Length == 0) return null;
            
            // 현재 스테이지에서 등장 가능한 적들 필터링
            var availableEnemies = System.Array.FindAll(enemyPool, 
                e => e.minStageLevel <= currentStageLevel && Random.value < e.spawnChance);
            
            if (availableEnemies.Length == 0) return enemyPool[0]; // 폴백
            
            // 가중치 기반 선택
            int totalWeight = 0;
            foreach (var enemy in availableEnemies)
            {
                totalWeight += enemy.spawnWeight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var enemy in availableEnemies)
            {
                currentWeight += enemy.spawnWeight;
                if (randomValue < currentWeight)
                {
                    return enemy;
                }
            }
            
            return availableEnemies[0]; // 폴백
        }
        
        /// <summary>
        /// 감각 기반 이벤트 선택
        /// </summary>
        public EventSpawnData GetRandomEvent(SenseType playerSense = SenseType.None)
        {
            if (eventPool == null || eventPool.Length == 0) return null;
            
            // 플레이어 감각에 따른 필터링
            var availableEvents = System.Array.FindAll(eventPool, e => 
            {
                if (e.requiredSenses == null || e.requiredSenses.Length == 0) return true;
                return System.Array.Exists(e.requiredSenses, sense => sense == playerSense || sense == SenseType.None);
            });
            
            if (availableEvents.Length == 0) return eventPool[0]; // 폴백
            
            // 가중치 기반 선택
            int totalWeight = 0;
            foreach (var evt in availableEvents)
            {
                totalWeight += evt.spawnWeight;
            }
            
            int randomValue = Random.Range(0, totalWeight);
            int currentWeight = 0;
            
            foreach (var evt in availableEvents)
            {
                currentWeight += evt.spawnWeight;
                if (randomValue < currentWeight)
                {
                    return evt;
                }
            }
            
            return availableEvents[0]; // 폴백
        }
        #endregion
    }
}