using UnityEngine;
using UnityEditor;

namespace MonoChrome.Editor
{
    public class CharacterCategoryFixer : EditorWindow
    {
        [MenuItem("MonoChrome/Tools/Fix Character Categories")]
        public static void FixCharacterCategories()
        {
            var manager = Resources.Load<CharacterDataManager>("CharacterDataManager");
            if (manager == null)
            {
                Debug.LogError("CharacterDataManager not found!");
                return;
            }

            Debug.Log("Fixing character categories...");
            EditorUtility.SetDirty(manager);
            AssetDatabase.SaveAssets();
            Debug.Log("Character categories fixed!");
        }
    }
}