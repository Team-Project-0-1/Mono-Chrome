using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// 기존 매니저 시스템을 비활성화하고 새 Core 시스템을 활성화하는 도구
    /// 포트폴리오용: 복잡한 시스템을 간단하게 전환하기 위한 마이그레이션 도구
    /// </summary>
    public class SystemMigrator : MonoBehaviour
    {
        [Header("Migration Settings")]
        [SerializeField] private bool _disableLegacyManagers = true;
        [SerializeField] private bool _enableCoreManagers = true;
        [SerializeField] private bool _createMissingCoreComponents = true;
        [SerializeField] private bool _autoFixReferences = true;
        
        [Header("Debug")]
        [SerializeField] private bool _showMigrationLog = true;
        
        [ContextMenu("🚀 Migrate to Core System")]
        public void MigrateToCoreSystem()
        {
            LogMessage("=== 시스템 마이그레이션 시작 ===");
            
            // 1단계: 기존 매니저들 비활성화
            if (_disableLegacyManagers)
            {
                DisableLegacyManagers();
            }
            
            // 2단계: Core 매니저들 활성화
            if (_enableCoreManagers)
            {
                EnableCoreManagers();
            }
            
            // 3단계: 누락된 Core 컴포넌트 생성
            if (_createMissingCoreComponents)
            {
                CreateMissingCoreComponents();
            }
            
            // 4단계: 참조 자동 수정
            if (_autoFixReferences)
            {
                AutoFixReferences();
            }
            
            LogMessage("=== 시스템 마이그레이션 완료 ===");
            LogMessage("이제 Play 버튼을 눌러 테스트해보세요!");
        }
        
        /// <summary>
        /// 기존 매니저들 비활성화
        /// </summary>
        private void DisableLegacyManagers()
        {
            LogMessage("1단계: 기존 매니저들 비활성화...");
            
            string[] legacyManagerNames = {
                "GameManager",
                "ImprovedGameManager", 
                "UnifiedGameManager",
                "UIManager",
                "ManagerInitializer"
            };
            
            int disabledCount = 0;
            
            foreach (string managerName in legacyManagerNames)
            {
                // 이름으로 찾기
                GameObject managerObj = GameObject.Find(managerName);
                if (managerObj != null && managerObj.activeInHierarchy)
                {
                    managerObj.SetActive(false);
                    disabledCount++;
                    LogMessage($"  ✓ {managerName} 비활성화됨");
                }
                
                // 컴포넌트로 찾기
                var managerComponents = FindObjectsOfType<MonoBehaviour>();
                foreach (var component in managerComponents)
                {
                    if (component.GetType().Name == managerName && component.gameObject.activeInHierarchy)
                    {
                        component.gameObject.SetActive(false);
                        disabledCount++;
                        LogMessage($"  ✓ {managerName} (컴포넌트) 비활성화됨");
                        break;
                    }
                }
            }
            
            LogMessage($"총 {disabledCount}개의 기존 매니저가 비활성화되었습니다.");
        }
        
        /// <summary>
        /// Core 매니저들 활성화
        /// </summary>
        private void EnableCoreManagers()
        {
            LogMessage("2단계: Core 매니저들 활성화...");
            
            // CoreGameManager 찾기 및 활성화
            CoreGameManager coreGameManager = FindObjectOfType<CoreGameManager>();
            if (coreGameManager != null)
            {
                if (!coreGameManager.gameObject.activeInHierarchy)
                {
                    coreGameManager.gameObject.SetActive(true);
                    LogMessage("  ✓ CoreGameManager 활성화됨");
                }
                else
                {
                    LogMessage("  ✓ CoreGameManager 이미 활성화됨");
                }
            }
            else
            {
                LogMessage("  ⚠ CoreGameManager를 찾을 수 없음 - 3단계에서 생성됩니다");
            }
            
            // CoreUIManager 찾기 및 활성화
            CoreUIManager coreUIManager = FindObjectOfType<CoreUIManager>();
            if (coreUIManager != null)
            {
                if (!coreUIManager.gameObject.activeInHierarchy)
                {
                    coreUIManager.gameObject.SetActive(true);
                    LogMessage("  ✓ CoreUIManager 활성화됨");
                }
                else
                {
                    LogMessage("  ✓ CoreUIManager 이미 활성화됨");
                }
            }
            else
            {
                LogMessage("  ⚠ CoreUIManager를 찾을 수 없음 - 3단계에서 생성됩니다");
            }
        }
        
        /// <summary>
        /// 누락된 Core 컴포넌트 생성
        /// </summary>
        private void CreateMissingCoreComponents()
        {
            LogMessage("3단계: 누락된 Core 컴포넌트 생성...");
            
            // CoreGameManager 생성
            if (FindObjectOfType<CoreGameManager>() == null)
            {
                GameObject coreGameManagerObj = new GameObject("CoreGameManager");
                coreGameManagerObj.AddComponent<CoreGameManager>();
                LogMessage("  ✓ CoreGameManager 생성됨");
            }
            
            // CoreUIManager 생성 (Canvas 하위에)
            if (FindObjectOfType<CoreUIManager>() == null)
            {
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject coreUIManagerObj = new GameObject("CoreUIManager");
                    coreUIManagerObj.transform.SetParent(canvas.transform, false);
                    coreUIManagerObj.AddComponent<CoreUIManager>();
                    LogMessage("  ✓ CoreUIManager 생성됨 (Canvas 하위)");
                }
                else
                {
                    LogMessage("  ⚠ Canvas를 찾을 수 없어 CoreUIManager를 생성할 수 없음");
                }
            }
            
            // CoreMainMenuController 생성 (MainMenu 씬에서만)
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainMenu")
            {
                if (FindObjectOfType<CoreMainMenuController>() == null)
                {
                    GameObject coreMainMenuObj = new GameObject("CoreMainMenuController");
                    coreMainMenuObj.AddComponent<CoreMainMenuController>();
                    LogMessage("  ✓ CoreMainMenuController 생성됨");
                }
            }
            
            // QuickUIBuilder 생성 (GameScene에서만)
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "GameScene")
            {
                if (FindObjectOfType<QuickUIBuilder>() == null)
                {
                    GameObject quickUIBuilderObj = new GameObject("QuickUIBuilder");
                    quickUIBuilderObj.AddComponent<QuickUIBuilder>();
                    LogMessage("  ✓ QuickUIBuilder 생성됨");
                }
            }
        }
        
        /// <summary>
        /// 참조 자동 수정
        /// </summary>
        private void AutoFixReferences()
        {
            LogMessage("4단계: 참조 자동 수정...");
            
            // Canvas 및 EventSystem 확인
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                LogMessage("  ⚠ Canvas가 없습니다. UI가 표시되지 않을 수 있습니다.");
            }
            else
            {
                // GraphicRaycaster 확인
                if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
                {
                    canvas.gameObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                    LogMessage("  ✓ GraphicRaycaster 추가됨");
                }
                
                // CanvasScaler 확인
                if (canvas.GetComponent<UnityEngine.UI.CanvasScaler>() == null)
                {
                    var scaler = canvas.gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                    scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    LogMessage("  ✓ CanvasScaler 추가됨");
                }
            }
            
            // EventSystem 확인
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                LogMessage("  ✓ EventSystem 생성됨");
            }
            
            // 기존 DungeonManager와 CombatManager 활성화 (호환성을 위해)
            MonoChrome.Dungeon.DungeonManager dungeonManager = FindObjectOfType<MonoChrome.Dungeon.DungeonManager>();
            if (dungeonManager != null && !dungeonManager.gameObject.activeInHierarchy)
            {
                dungeonManager.gameObject.SetActive(true);
                LogMessage("  ✓ DungeonManager 활성화됨 (호환성)");
            }
            
            MonoChrome.Combat.CombatManager combatManager = FindObjectOfType<MonoChrome.Combat.CombatManager>();
            if (combatManager != null && !combatManager.gameObject.activeInHierarchy)
            {
                combatManager.gameObject.SetActive(true);
                LogMessage("  ✓ CombatManager 활성화됨 (호환성)");
            }
        }
        
        /// <summary>
        /// 메시지 로그
        /// </summary>
        private void LogMessage(string message)
        {
            if (_showMigrationLog)
            {
                Debug.Log($"[SystemMigrator] {message}");
            }
        }
        
        #region Context Menu Actions
        [ContextMenu("🔍 Analyze Current System")]
        public void AnalyzeCurrentSystem()
        {
            LogMessage("=== 현재 시스템 분석 ===");
            
            // 기존 매니저들 확인
            string[] legacyManagers = { "GameManager", "ImprovedGameManager", "UnifiedGameManager", "UIManager" };
            LogMessage("기존 매니저들:");
            foreach (string manager in legacyManagers)
            {
                GameObject obj = GameObject.Find(manager);
                if (obj != null)
                {
                    LogMessage($"  {manager}: {(obj.activeInHierarchy ? "활성화됨" : "비활성화됨")}");
                }
                else
                {
                    LogMessage($"  {manager}: 없음");
                }
            }
            
            // Core 매니저들 확인
            LogMessage("Core 매니저들:");
            LogMessage($"  CoreGameManager: {(FindObjectOfType<CoreGameManager>() != null ? "있음" : "없음")}");
            LogMessage($"  CoreUIManager: {(FindObjectOfType<CoreUIManager>() != null ? "있음" : "없음")}");
            LogMessage($"  CoreMainMenuController: {(FindObjectOfType<CoreMainMenuController>() != null ? "있음" : "없음")}");
            
            // UI 구조 확인
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                LogMessage("UI 구조:");
                LogMessage($"  Canvas: 있음");
                LogMessage($"  CharacterSelectionPanel: {(canvas.transform.Find("CharacterSelectionPanel") != null ? "있음" : "없음")}");
                LogMessage($"  DungeonPanel: {(canvas.transform.Find("DungeonPanel") != null ? "있음" : "없음")}");
                LogMessage($"  CombatPanel: {(canvas.transform.Find("CombatPanel") != null ? "있음" : "없음")}");
            }
            else
            {
                LogMessage("  Canvas: 없음");
            }
            
            LogMessage("===================");
        }
        
        [ContextMenu("🧹 Clean Legacy Managers")]
        public void CleanLegacyManagers()
        {
            DisableLegacyManagers();
            LogMessage("기존 매니저들이 정리되었습니다.");
        }
        
        [ContextMenu("⚡ Quick Setup")]
        public void QuickSetup()
        {
            LogMessage("=== 빠른 설정 시작 ===");
            
            // 마이그레이션 실행
            MigrateToCoreSystem();
            
            // UI 구조 생성
            QuickUIBuilder uiBuilder = FindObjectOfType<QuickUIBuilder>();
            if (uiBuilder != null)
            {
                uiBuilder.CreateBasicUIStructure();
                LogMessage("UI 구조가 생성되었습니다.");
            }
            
            LogMessage("=== 빠른 설정 완료 ===");
            LogMessage("이제 바로 게임을 테스트할 수 있습니다!");
        }
        #endregion
    }
}