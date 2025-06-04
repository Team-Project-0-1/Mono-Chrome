#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonoChrome.Editor
{
    public class QuickDataLinker
    {
        [MenuItem("Tools/MonoChrome/Quick Link All Data")]
        public static void QuickLinkAllData()
        {
            LinkCharacterData();
            LinkPatternData();
            Debug.Log("Quick data linking completed!");
        }
        
        private static void LinkCharacterData()
        {
            var manager = Resources.Load<CharacterDataManager>("CharacterDataManager");
            if (manager == null) return;
            
            var so = new SerializedObject(manager);
            
            var players = FindByType(CharacterType.Player);
            var normal = FindByType(CharacterType.Normal);
            var mini = FindByType(CharacterType.MiniBoss);
            var boss = FindByType(CharacterType.Boss);
            
            SetArray(so, "_playerCharacters", players);
            SetArray(so, "_normalEnemies", normal);
            SetArray(so, "_miniBosses", mini);
            SetArray(so, "_bosses", boss);
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            
            Debug.Log($"Characters linked: P{players.Count} N{normal.Count} M{mini.Count} B{boss.Count}");
        }
        
        private static void LinkPatternData()
        {
            var manager = Resources.Load<PatternDataManager>("PatternDataManager");
            if (manager == null) return;
            
            var patterns = FindAllPatterns();
            var so = new SerializedObject(manager);
            SetArray(so, "_patterns", patterns);
            
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(manager);
            
            Debug.Log($"Patterns linked: {patterns.Count}");
        }
        
        private static List<CharacterDataSO> FindByType(CharacterType type)
        {
            var result = new List<CharacterDataSO>();
            var guids = AssetDatabase.FindAssets("t:CharacterDataSO");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
                if (asset != null && asset.characterType == type)
                    result.Add(asset);
            }
            
            return result;
        }
        
        private static List<PatternSO> FindAllPatterns()
        {
            var result = new List<PatternSO>();
            var guids = AssetDatabase.FindAssets("t:PatternSO");
            
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<PatternSO>(path);
                if (asset != null)
                    result.Add(asset);
            }
            
            return result;
        }
        
        private static void SetArray<T>(SerializedObject so, string propName, List<T> items) where T : Object
        {
            var prop = so.FindProperty(propName);
            if (prop != null && prop.isArray)
            {
                prop.arraySize = items.Count;
                for (int i = 0; i < items.Count; i++)
                    prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
            }
        }
    }
}
#endif