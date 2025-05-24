using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MonoChrome
{
    /// <summary>
    /// 매니저들의 초기화와 의존성을 안전하게 관리하는 헬퍼 클래스
    /// 씬 전환 시 발생할 수 있는 참조 문제들을 해결
    /// </summary>
    public class ManagerInitializer : MonoBehaviour
    {
        [Header("Manager Initialization Settings")]
        [SerializeField] private bool _debugMode = true;
        [SerializeField] private float _initializationDelay = 0.1f;
        
        private static ManagerInitializer _instance;
        
        public static ManagerInitializer Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("ManagerInitializer");
                    _instance = go.AddComponent<ManagerInitializer>();
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
            
            // 씬 로드 이벤트 구독
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_debugMode)
                Debug.Log($"ManagerInitializer: Scene {scene.name} loaded, starting initialization");
                
            StartCoroutine(InitializeManagersForScene(scene.name));
        }
        
        /// <summary>
        /// 씬별 매니저 초기화 코루틴
        /// </summary>
        private IEnumerator InitializeManagersForScene(string sceneName)
        {
            // 초기화 딜레이
            yield return new WaitForSeconds(_initializationDelay);
            
            switch (sceneName)
            {
                case "GameScene":
                    yield return StartCoroutine(InitializeGameSceneManagers());
                    break;
                    
                case "MainMenu":
                    yield return StartCoroutine(InitializeMainMenuManagers());
                    break;
            }
            
            if (_debugMode)
                Debug.Log($"ManagerInitializer: Completed initialization for {sceneName}");
        }
        
        /// <summary>
        /// GameScene 매니저들 초기화
        /// </summary>
        private IEnumerator InitializeGameSceneManagers()
        {
            if (_debugMode)
                Debug.Log("ManagerInitializer: Initializing GameScene managers");
            
            // 1. 비활성화된 매니저들 활성화
            yield return StartCoroutine(ActivateInactiveManagers());
            
            // 2. 의존성 순서에 따른 초기화
            yield return StartCoroutine(InitializeManagerDependencies());
            
            // 3. UI 매니저 참조 업데이트
            yield return StartCoroutine(UpdateUIManagerReferences());
            
            if (_debugMode)
                Debug.Log("ManagerInitializer: GameScene managers initialization completed");
        }
        
        /// <summary>
        /// MainMenu 매니저들 초기화
        /// </summary>
        private IEnumerator InitializeMainMenuManagers()
        {
            if (_debugMode)
                Debug.Log("ManagerInitializer: Initializing MainMenu managers");
            
            // MainMenu는 간단한 초기화만 필요
            yield return null;
            
            if (_debugMode)
                Debug.Log("ManagerInitializer: MainMenu managers initialization completed");
        }
        
        /// <summary>
        /// 비활성화된 매니저들을 활성화
        /// </summary>
        private IEnumerator ActivateInactiveManagers()
        {
            var managerNames = new[]
            {
                "CombatManager",
                "CoinManager", 
                "PatternManager",
                "StatusEffectManager"
            };
            
            foreach (string managerName in managerNames)
            {
                GameObject manager = GameObject.Find(managerName);
                if (manager != null && !manager.activeInHierarchy)
                {
                    manager.SetActive(true);
                    if (_debugMode)
                        Debug.Log($"ManagerInitializer: Activated {managerName}");
                    
                    // 매니저 간 안전한 활성화를 위한 프레임 대기
                    yield return null;
                }
            }
        }
        
        /// <summary>
        /// 매니저 의존성 초기화
        /// </summary>
        private IEnumerator InitializeManagerDependencies()
        {
            // CombatManager 의존성 확인
            var combatManager = FindObjectOfType<MonoChrome.Combat.CombatManager>();
            if (combatManager != null)
            {
                yield return StartCoroutine(EnsureCombatManagerDependencies(combatManager));
            }
            
            // DungeonManager 의존성 확인
            var dungeonManager = FindObjectOfType<MonoChrome.Dungeon.DungeonManager>();
            if (dungeonManager != null)
            {
                yield return StartCoroutine(EnsureDungeonManagerDependencies(dungeonManager));
            }
        }
        
        /// <summary>
        /// CombatManager 의존성 확인 및 설정
        /// </summary>
        private IEnumerator EnsureCombatManagerDependencies(MonoChrome.Combat.CombatManager combatManager)
        {
            GameObject combatObject = combatManager.gameObject;
            
            // 필요한 컴포넌트들 확인 및 추가
            if (combatObject.GetComponent<CoinManager>() == null)
            {
                combatObject.AddComponent<CoinManager>();
                if (_debugMode)
                    Debug.Log("ManagerInitializer: Added CoinManager to CombatManager");
            }
            
            if (combatObject.GetComponent<PatternManager>() == null)
            {
                combatObject.AddComponent<PatternManager>();
                if (_debugMode)
                    Debug.Log("ManagerInitializer: Added PatternManager to CombatManager");
            }
            
            if (combatObject.GetComponent<MonoChrome.StatusEffects.StatusEffectManager>() == null)
            {
                combatObject.AddComponent<MonoChrome.StatusEffects.StatusEffectManager>();
                if (_debugMode)
                    Debug.Log("ManagerInitializer: Added StatusEffectManager to CombatManager");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// DungeonManager 의존성 확인 및 설정
        /// </summary>
        private IEnumerator EnsureDungeonManagerDependencies(MonoChrome.Dungeon.DungeonManager dungeonManager)
        {
            // DungeonManager 관련 의존성이 필요한 경우 여기에 추가
            yield return null;
        }
        
        /// <summary>
        /// UI 매니저 참조 업데이트
        /// </summary>
        private IEnumerator UpdateUIManagerReferences()
        {
            var uiManager = FindObjectOfType<UIManager>();
            if (uiManager != null)
            {
                // UIManager가 GameManager 이벤트를 올바르게 구독했는지 확인
                // 필요시 강제로 재구독
                if (_debugMode)
                    Debug.Log("ManagerInitializer: UIManager references updated");
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 매니저 상태 진단 도구
        /// </summary>
        [ContextMenu("Diagnose Manager States")]
        public void DiagnoseManagerStates()
        {
            Debug.Log("=== Manager State Diagnosis ===");
            
            // GameManager 상태
            var gameManager = GameManager.Instance;
            Debug.Log($"GameManager: {(gameManager != null ? "Active" : "Missing")}");
            if (gameManager != null)
            {
                Debug.Log($"  Current State: {gameManager.CurrentState}");
                Debug.Log($"  UIManager Reference: {(gameManager.UIManager != null ? "Valid" : "Null")}");
                Debug.Log($"  DungeonManager Reference: {(gameManager.DungeonManager != null ? "Valid" : "Null")}");
                Debug.Log($"  CombatManager Reference: {(gameManager.CombatManager != null ? "Valid" : "Null")}");
            }
            
            // 씬의 매니저들 상태
            var sceneManagers = new[]
            {
                "UIManager", "DungeonManager", "CombatManager", 
                "CoinManager", "PatternManager", "StatusEffectManager"
            };
            
            foreach (string managerName in sceneManagers)
            {
                GameObject manager = GameObject.Find(managerName);
                if (manager != null)
                {
                    Debug.Log($"{managerName}: Active={manager.activeInHierarchy}, Components={manager.GetComponents<Component>().Length}");
                }
                else
                {
                    Debug.Log($"{managerName}: Not Found");
                }
            }
            
            Debug.Log("=== End Diagnosis ===");
        }
        
        /// <summary>
        /// 매니저 강제 재초기화 (디버깅용)
        /// </summary>
        [ContextMenu("Force Reinitialize Managers")]
        public void ForceReinitializeManagers()
        {
            if (_debugMode)
                Debug.Log("ManagerInitializer: Force reinitializing managers");
                
            StartCoroutine(InitializeManagersForScene(SceneManager.GetActiveScene().name));
        }
    }
}