using UnityEngine;
using MonoChrome.Core;
#if UNITY_EDITOR
using UnityEditor;
using MonoChrome.Editor;
#endif

namespace MonoChrome.Setup
{
    /// <summary>
    /// Unified initialization entry point for project setup.
    /// Handles data generation, essential manager creation and manager initialization.
    /// </summary>
    public static class GameInitializer
    {
        /// <summary>
        /// Run initialization logic.
        /// </summary>
        public static void Initialize(bool createMasterGameManager = true, bool initializeGameData = false)
        {
#if UNITY_EDITOR
            if (initializeGameData)
            {
                DataInitializer.GenerateGameData();
            }
#endif
            if (createMasterGameManager && MasterGameManager.Instance == null)
            {
                var go = new GameObject("[MasterGameManager]");
                go.AddComponent<MasterGameManager>();
            }

            ManagerInitializer.Initialize();
        }

#if UNITY_EDITOR
        [MenuItem("MonoChrome/Run Full Setup")]
        public static void RunFullSetup()
        {
            Initialize(createMasterGameManager: true, initializeGameData: true);
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            Initialize(createMasterGameManager: true, initializeGameData: false);
        }
    }
}
