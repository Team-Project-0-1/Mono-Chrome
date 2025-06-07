using UnityEngine;
using UnityEditor;

namespace MonoChrome.Editor
{
    public class ProjectQualityChecker : EditorWindow
    {
        [MenuItem("MonoChrome/Quality/Check Project")]
        public static void ShowWindow()
        {
            GetWindow<ProjectQualityChecker>("Quality Checker");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Project Quality Checker", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Run Quality Check"))
            {
                RunQualityCheck();
            }
        }
        
        private void RunQualityCheck()
        {
            Debug.Log("Running quality check...");
            
            // Check CharacterDataManager
            var characterManager = Resources.Load<CharacterDataManager>("CharacterDataManager");
            if (characterManager != null)
            {
                Debug.Log("✅ CharacterDataManager found");
            }
            else
            {
                Debug.LogError("❌ CharacterDataManager missing");
            }
            
            Debug.Log("Quality check complete");
        }
    }
}