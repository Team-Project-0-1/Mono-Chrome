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
                // GameDataGenerator 통합으로 DataInitializer 대체
                Debug.Log("GameInitializer: 데이터 초기화는 GameDataGenerator를 사용해주세요.");
                Debug.Log("메뉴: MonoChrome → Game Data Utility");
            }
#endif
            if (createMasterGameManager && MasterGameManager.Instance == null)
            {
                var go = new GameObject("[MasterGameManager]");
                go.AddComponent<MasterGameManager>();
            }

            // MasterGameManager가 자체 초기화를 담당하므로 별도 초기화 불필요
            Debug.Log("GameInitializer: MasterGameManager 생성 완료");
        }

#if UNITY_EDITOR
        [MenuItem("MonoChrome/Run Full Setup")]
        public static void RunFullSetup()
        {
            // 이제 MasterGameManager만 생성하고, 데이터 생성은 GameDataGenerator 사용
            Initialize(createMasterGameManager: true, initializeGameData: false);
            
            // GameDataGenerator UI 창 열기
            EditorApplication.delayCall += () => {
                GameDataGenerator.ShowWindow();
            };
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void RuntimeInitialize()
        {
            Initialize(createMasterGameManager: true, initializeGameData: false);
        }
    }
}
