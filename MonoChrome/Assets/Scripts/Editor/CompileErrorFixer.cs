#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MonoChrome
{
    public class CompileErrorFixer : EditorWindow
    {
        [MenuItem("Tools/Fix Compilation Errors")]
        private static void FixErrors()
        {
            var window = GetWindow<CompileErrorFixer>();
            window.titleContent = new GUIContent("Compile Error Fixer");
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("이 도구는 일반적인 컴파일 에러를 수정하려고 시도합니다", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Fix CharacterType References"))
            {
                FixCharacterTypeReferences();
            }
            
            if (GUILayout.Button("Fix StatusEffect Issues"))
            {
                FixStatusEffectIssues();
            }
            
            if (GUILayout.Button("Fix Namespace Issues"))
            {
                FixNamespaceIssues();
            }
            
            if (GUILayout.Button("Add Missing Usings"))
            {
                AddMissingUsings();
            }
            
            if (GUILayout.Button("Fix All Issues"))
            {
                FixCharacterTypeReferences();
                FixStatusEffectIssues();
                FixNamespaceIssues();
                AddMissingUsings();
            }
        }
        
        private void FixCharacterTypeReferences()
        {
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int fixedCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                string content = File.ReadAllText(file);
                bool modified = false;
                
                // CharacterType.Normal을 참조할 때 발생하는 에러 수정
                if (content.Contains("CharacterType.Normal") && !content.Contains("using MonoChrome.Core;"))
                {
                    content = "using MonoChrome.Core;\n" + content;
                    modified = true;
                }
                
                // switch (type) 문에서 CharacterType 관련 에러 수정
                if (content.Contains("switch (type)") && content.Contains("case CharacterType."))
                {
                    if (!content.Contains("using MonoChrome.Core;"))
                    {
                        content = "using MonoChrome.Core;\n" + content;
                        modified = true;
                    }
                    
                    // if-else로 변환
                    if (content.Contains("case CharacterType.Normal:") && !content.Contains("if (type.IsNormal())"))
                    {
                        content = content.Replace("case CharacterType.Normal:", "// case CharacterType.Normal:");
                        if (!content.Contains("if (type.IsNormal())") && !content.Contains("if (GlobalTypeConverter.IsNormal(type))"))
                        {
                            content = content.Replace("switch (type)", "// switch (type)");
                            modified = true;
                        }
                    }
                }
                
                if (modified)
                {
                    File.WriteAllText(file, content);
                    fixedCount++;
                    Debug.Log($"Fixed CharacterType reference in {file}");
                }
            }
            
            Debug.Log($"Fixed CharacterType references in {fixedCount} files");
            AssetDatabase.Refresh();
        }
        
        private void FixStatusEffectIssues()
        {
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int fixedCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                string content = File.ReadAllText(file);
                bool modified = false;
                
                // StatusEffect 관련 문제 수정
                if (content.Contains("StatusEffect") && !content.Contains("using MonoChrome.StatusEffects;"))
                {
                    content = "using MonoChrome.StatusEffects;\n" + content;
                    modified = true;
                }
                
                // EffectType 관련 문제 수정
                if (content.Contains("EffectType") && !content.Contains("using MonoChrome.StatusEffects;"))
                {
                    content = "using MonoChrome.StatusEffects;\n" + content;
                    modified = true;
                }
                
                // 명시적 변환 문제 수정
                if (content.Contains("(StatusEffect)") && (content.Contains("EffectType") || content.Contains("StatusEffectType")))
                {
                    content = content.Replace("(StatusEffect)", "GlobalTypeConverter.CreateStatusEffect");
                    modified = true;
                }
                
                if (modified)
                {
                    File.WriteAllText(file, content);
                    fixedCount++;
                    Debug.Log($"Fixed StatusEffect reference in {file}");
                }
            }
            
            Debug.Log($"Fixed StatusEffect issues in {fixedCount} files");
            AssetDatabase.Refresh();
        }
        
        private void FixNamespaceIssues()
        {
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int fixedCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                string content = File.ReadAllText(file);
                bool modified = false;
                
                // CombatManager 관련 문제 수정
                if (content.Contains("CombatManager") && 
                    !content.Contains("using MonoChrome.Combat;") && 
                    !content.Contains("using MonoChrome.Core;"))
                {
                    content = "using MonoChrome.Combat;\nusing MonoChrome.Core;\n" + content;
                    modified = true;
                }
                
                // CharacterType -> CombatManager 변환 관련 문제 수정
                if (content.Contains("(CombatManager)") && content.Contains("CharacterType"))
                {
                    content = content.Replace("(CombatManager)", "GlobalTypeConverter.CreateCombatManager");
                    modified = true;
                }
                
                if (modified)
                {
                    File.WriteAllText(file, content);
                    fixedCount++;
                    Debug.Log($"Fixed namespace issue in {file}");
                }
            }
            
            Debug.Log($"Fixed namespace issues in {fixedCount} files");
            AssetDatabase.Refresh();
        }
        
        private void AddMissingUsings()
        {
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int fixedCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                string content = File.ReadAllText(file);
                bool modified = false;
                
                // 공통적으로 필요한 네임스페이스 추가
                if (!content.Contains("using MonoChrome;") && !file.Contains("GlobalTypeConverter.cs"))
                {
                    content = "using MonoChrome;\n" + content;
                    modified = true;
                }
                
                if (!content.Contains("using MonoChrome.Extensions;") && !file.Contains("CharacterTypeExtensions.cs"))
                {
                    content = "using MonoChrome.Extensions;\n" + content;
                    modified = true;
                }
                
                if (!content.Contains("using MonoChrome.Core.Interfaces;") && 
                    (content.Contains("ITypeConverter") || content.Contains("TypeConverter")) && 
                    !file.Contains("ITypeConverter.cs"))
                {
                    content = "using MonoChrome.Core.Interfaces;\n" + content;
                    modified = true;
                }
                
                if (!content.Contains("using MonoChrome.Utils;") && 
                    (content.Contains("ErrorHandler") || content.Contains("PatternConverter")) && 
                    !file.Contains("ErrorHandler.cs") && 
                    !file.Contains("PatternConverter.cs"))
                {
                    content = "using MonoChrome.Utils;\n" + content;
                    modified = true;
                }
                
                if (modified)
                {
                    File.WriteAllText(file, content);
                    fixedCount++;
                    Debug.Log($"Added missing usings to {file}");
                }
            }
            
            Debug.Log($"Added missing usings to {fixedCount} files");
            AssetDatabase.Refresh();
        }
    }
}
#endif