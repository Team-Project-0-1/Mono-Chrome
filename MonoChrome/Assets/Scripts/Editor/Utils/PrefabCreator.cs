using UnityEngine;
using UnityEditor;
using System.IO;

namespace MonoChrome.Editor.Utils
{
    public static class PrefabCreator
    {
        [MenuItem("Tools/MONOCHROME/Create Pattern Button Prefab")]
        public static void CreatePatternButtonPrefab()
        {
            // Scene에서 PatternButtonPrefab GameObject 찾기
            GameObject patternButtonGO = GameObject.Find("PatternButtonPrefab");
            if (patternButtonGO == null)
            {
                Debug.LogError("PatternButtonPrefab GameObject를 Scene에서 찾을 수 없습니다.");
                return;
            }

            // Resources/UI 폴더 확인 및 생성
            string folderPath = "Assets/Resources/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "UI");
            }

            // 프리팹 경로
            string prefabPath = "Assets/Resources/UI/PatternButtonPrefab.prefab";

            // 기존 프리팹이 있다면 삭제
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // 프리팹 생성
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(patternButtonGO, prefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"PatternButtonPrefab 프리팹이 {prefabPath}에 성공적으로 생성되었습니다.");
                
                // 생성된 프리팹을 선택
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError("PatternButtonPrefab 프리팹 생성에 실패했습니다.");
            }
        }

        [MenuItem("Tools/MONOCHROME/Create Status Effect Prefab")]
        public static void CreateStatusEffectPrefab()
        {
            // StatusEffectPrefab GameObject 찾기
            GameObject statusEffectGO = GameObject.Find("StatusEffectPrefab");
            if (statusEffectGO == null)
            {
                Debug.LogError("StatusEffectPrefab GameObject를 Scene에서 찾을 수 없습니다.");
                return;
            }

            // Resources/UI 폴더 확인 및 생성
            string folderPath = "Assets/Resources/UI";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                {
                    AssetDatabase.CreateFolder("Assets", "Resources");
                }
                AssetDatabase.CreateFolder("Assets/Resources", "UI");
            }

            // 프리팹 경로
            string prefabPath = "Assets/Resources/UI/StatusEffectPrefab.prefab";

            // 기존 프리팹이 있다면 삭제
            if (File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // 프리팹 생성
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(statusEffectGO, prefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"StatusEffectPrefab 프리팹이 {prefabPath}에 성공적으로 생성되었습니다.");
                
                // 생성된 프리팹을 선택
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
            }
            else
            {
                Debug.LogError("StatusEffectPrefab 프리팹 생성에 실패했습니다.");
            }
        }

        [MenuItem("Tools/MONOCHROME/Create All UI Prefabs")]
        public static void CreateAllUIPrefabs()
        {
            CreatePatternButtonPrefab();
            CreateStatusEffectPrefab();
        }
    }
}