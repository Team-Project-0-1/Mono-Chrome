#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonoChrome.Editor
{
    public class DataManagerLinker : EditorWindow
    {
        [MenuItem("Tools/MonoChrome/Link Data Managers")]
        public static void ShowWindow()
        {
            GetWindow<DataManagerLinker>("Data Manager Linker");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("Data Manager Auto Linker", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Link Character Data Manager", GUILayout.Height(30)))
            {
                LinkCharacterDataManager();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Link Pattern Data Manager", GUILayout.Height(30)))
            {
                LinkPatternDataManager();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Link All Data Managers", GUILayout.Height(40)))
            {
                LinkCharacterDataManager();
                LinkPatternDataManager();
                Debug.Log("All data managers linked successfully!");
            }
        }
        
        private void LinkCharacterDataManager()
        {
            CharacterDataManager characterManager = Resources.Load<CharacterDataManager>("CharacterDataManager");
            if (characterManager == null)
            {
                Debug.LogError("CharacterDataManager not found!");
                return;
            }
            
            SerializedObject serializedManager = new SerializedObject(characterManager);
            
            List<CharacterDataSO> playerCharacters = FindCharactersByType(CharacterType.Player);
            List<CharacterDataSO> normalEnemies = FindCharactersByType(CharacterType.Normal);
            List<CharacterDataSO> miniBosses = FindCharactersByType(CharacterType.MiniBoss);
            List<CharacterDataSO> bosses = FindCharactersByType(CharacterType.Boss);
            
            SetSerializedArray(serializedManager, "_playerCharacters", playerCharacters);
            SetSerializedArray(serializedManager, "_normalEnemies", normalEnemies);
            SetSerializedArray(serializedManager, "_miniBosses", miniBosses);
            SetSerializedArray(serializedManager, "_bosses", bosses);
            
            serializedManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(characterManager);
            AssetDatabase.SaveAssets();
            
            Debug.Log("CharacterDataManager linked! Players: " + playerCharacters.Count + 
                     ", Normal: " + normalEnemies.Count + 
                     ", MiniBoss: " + miniBosses.Count + 
                     ", Boss: " + bosses.Count);
        }
        
        private void LinkPatternDataManager()
        {
            PatternDataManager patternManager = Resources.Load<PatternDataManager>("PatternDataManager");
            if (patternManager == null)
            {
                Debug.LogError("PatternDataManager not found!");
                return;
            }
            
            List<PatternSO> allPatterns = FindAllPatterns();
            
            SerializedObject serializedManager = new SerializedObject(patternManager);
            SetSerializedArray(serializedManager, "_patterns", allPatterns);
            
            serializedManager.ApplyModifiedProperties();
            EditorUtility.SetDirty(patternManager);
            AssetDatabase.SaveAssets();
            
            Debug.Log("PatternDataManager linked! Patterns: " + allPatterns.Count);
        }
        
        private List<CharacterDataSO> FindCharactersByType(CharacterType characterType)
        {
            string[] guids = AssetDatabase.FindAssets("t:CharacterDataSO");
            List<CharacterDataSO> characters = new List<CharacterDataSO>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                CharacterDataSO character = AssetDatabase.LoadAssetAtPath<CharacterDataSO>(path);
                
                if (character != null && character.characterType == characterType)
                {
                    characters.Add(character);
                }
            }
            
            return characters;
        }
        
        private List<PatternSO> FindAllPatterns()
        {
            string[] guids = AssetDatabase.FindAssets("t:PatternSO");
            List<PatternSO> patterns = new List<PatternSO>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PatternSO pattern = AssetDatabase.LoadAssetAtPath<PatternSO>(path);
                
                if (pattern != null)
                {
                    patterns.Add(pattern);
                }
            }
            
            return patterns;
        }
        
        private void SetSerializedArray<T>(SerializedObject serializedObject, string propertyName, List<T> items) where T : Object
        {
            SerializedProperty arrayProperty = serializedObject.FindProperty(propertyName);
            if (arrayProperty != null && arrayProperty.isArray)
            {
                arrayProperty.arraySize = items.Count;
                
                for (int i = 0; i < items.Count; i++)
                {
                    SerializedProperty elementProperty = arrayProperty.GetArrayElementAtIndex(i);
                    elementProperty.objectReferenceValue = items[i];
                }
            }
            else
            {
                Debug.LogWarning("Property not found or is not an array: " + propertyName);
            }
        }
    }
}
#endif