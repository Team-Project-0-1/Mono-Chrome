using System.Collections;
using MonoChrome.Core;
using MonoChrome.Systems.UI;
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
                "UIController",
                "DungeonController", 
                "CharacterManager",
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
            // 현재 시스템에서는 MasterGameManager가 모든 의존성을 관리하므로
            // 개별 매니저 의존성 체크 대신 통합 시스템 확인
            var masterGameManager = FindObjectOfType<MasterGameManager>();
            if (masterGameManager != null)
            {
                yield return StartCoroutine(EnsureMasterGameManagerDependencies(masterGameManager));
            }
        }
        
        /// <summary>
        /// MasterGameManager 의존성 확인 및 설정
        /// </summary>
        private IEnumerator EnsureMasterGameManagerDependencies(MasterGameManager masterGameManager)
        {
            if (_debugMode)
                Debug.Log("ManagerInitializer: Checking MasterGameManager dependencies");
            
            // MasterGameManager가 자체적으로 모든 의존성을 관리하므로
            // 초기화 완료 여부만 확인
            if (masterGameManager.IsInitialized)
            {
                if (_debugMode)
                    Debug.Log("ManagerInitializer: MasterGameManager already initialized");
            }
            else
            {
                if (_debugMode)
                    Debug.Log("ManagerInitializer: Waiting for MasterGameManager initialization");
                
                // MasterGameManager 초기화 대기
                yield return new WaitUntil(() => masterGameManager.IsInitialized);
            }
            
            yield return null;
        }
        
        /// <summary>
        /// UI 매니저 참조 업데이트
        /// </summary>
        private IEnumerator UpdateUIManagerReferences()
        {
            // 새로운 UI 시스템 확인
            var uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                if (_debugMode)
                    Debug.Log("ManagerInitializer: UIController found and active");
            }
            
            // CoreUIManager 확인
            var coreUIManager = FindObjectOfType<CoreUIManager>();
            if (coreUIManager != null)
            {
                if (_debugMode)
                    Debug.Log("ManagerInitializer: CoreUIManager found and active");
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
            
            // MasterGameManager 상태
            var masterGameManager = FindObjectOfType<MasterGameManager>();
            Debug.Log($"MasterGameManager: {(masterGameManager != null ? "Active" : "Missing")}");
            if (masterGameManager != null)
            {
                Debug.Log($"  Initialization Status: {(masterGameManager.IsInitialized ? "Initialized" : "Not Initialized")}");
            }
            
            // GameStateMachine 상태
            var gameStateMachine = MonoChrome.Core.GameStateMachine.Instance;
            Debug.Log($"GameStateMachine: {(gameStateMachine != null ? "Active" : "Missing")}");
            if (gameStateMachine != null)
            {
                Debug.Log($"  Current State: {gameStateMachine.CurrentState}");
            }
            
            // 씬의 시스템들 상태
            var sceneManagers = new[]
            {
                "UIController", "DungeonController", "CharacterManager", 
                "PlayerManager", "ShopManager", "StatusEffectManager"
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

        /// <summary>
        /// Static helper to run initialization using the singleton instance.
        /// </summary>
        public static void Initialize()
        {
            Instance.ForceReinitializeManagers();
        }
    }
}