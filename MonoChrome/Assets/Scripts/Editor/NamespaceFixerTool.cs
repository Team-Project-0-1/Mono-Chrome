#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MonoChrome
{
    /// <summary>
    /// 네임스페이스 충돌 문제를 자동으로 해결하는 에디터 도구
    /// </summary>
    public class NamespaceFixerTool : EditorWindow
    {
        private Vector2 scrollPosition;
        private List<string> logMessages = new List<string>();
        private bool autoBackup = true;
        private bool addNamespaceUsings = true;
        private bool useFullNamespace = true;
        private bool updateAllFiles = false;
        
        [MenuItem("MONOCHROME/Tools/Namespace Fixer Tool")]
        private static void ShowWindow()
        {
            var window = GetWindow<NamespaceFixerTool>();
            window.titleContent = new GUIContent("Namespace Fixer");
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("MONOCHROME 네임스페이스 충돌 해결 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 설정 옵션
            EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
            
            autoBackup = EditorGUILayout.Toggle("파일 백업 생성", autoBackup);
            addNamespaceUsings = EditorGUILayout.Toggle("네임스페이스 별칭 추가", addNamespaceUsings);
            useFullNamespace = EditorGUILayout.Toggle("전체 네임스페이스 사용", useFullNamespace);
            updateAllFiles = EditorGUILayout.Toggle("모든 파일 처리", updateAllFiles);
            
            EditorGUILayout.Space();
            
            // 작업 버튼
            if (GUILayout.Button("충돌하는 파일 검색"))
            {
                FindConflictingFiles();
            }
            
            if (GUILayout.Button("자동 수정 적용"))
            {
                ApplyAutomaticFixes();
            }
            
            EditorGUILayout.Space();
            
            // 로그 출력
            EditorGUILayout.LabelField("로그", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            foreach (string message in logMessages)
            {
                EditorGUILayout.HelpBox(message, MessageType.None);
            }
            EditorGUILayout.EndScrollView();
            
            // 로그 지우기 버튼
            if (GUILayout.Button("로그 지우기"))
            {
                logMessages.Clear();
            }
        }
        
        /// <summary>
        /// 충돌이 발생하는 파일 검색
        /// </summary>
        private void FindConflictingFiles()
        {
            logMessages.Clear();
            logMessages.Add("네임스페이스 충돌이 발생하는 파일 검색 중...");
            
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int conflictCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                string content = File.ReadAllText(file);
                bool hasConflict = false;
                
                // 모호한 참조 패턴 검색
                if (HasAmbiguousReferences(content))
                {
                    hasConflict = true;
                    conflictCount++;
                    logMessages.Add($"모호한 참조 발견: {Path.GetFileName(file)}");
                }
                
                // 네임스페이스 충돌 패턴 검색
                if (HasNamespaceConflicts(content))
                {
                    if (!hasConflict)
                    {
                        hasConflict = true;
                        conflictCount++;
                        logMessages.Add($"네임스페이스 충돌 발견: {Path.GetFileName(file)}");
                    }
                }
            }
            
            logMessages.Add($"검색 완료: {conflictCount}개의 파일에서 충돌 발견");
        }
        
        /// <summary>
        /// 모호한 참조가 있는지 확인
        /// </summary>
        private bool HasAmbiguousReferences(string content)
        {
            // CharacterType, SenseType, PatternType, StatusEffectType 등 모호한 타입 참조 확인
            string pattern = @"(?<!\w)(CharacterType|SenseType|PatternType|StatusEffectType|ActiveSkillType)(?!\.)";
            Regex regex = new Regex(pattern);
            
            return regex.IsMatch(content);
        }
        
        /// <summary>
        /// 네임스페이스 충돌이 있는지 확인
        /// </summary>
        private bool HasNamespaceConflicts(string content)
        {
            // 여러 네임스페이스 사용 확인
            bool hasMultipleNamespaces = content.Contains("MonoChrome.Core") && 
                                         (content.Contains("MonoChrome.Characters") || 
                                          content.Contains("MonoChrome.Combat") || 
                                          content.Contains("MonoChrome.StatusEffects"));
                                          
            // 명시적 네임스페이스 사용 확인
            bool hasExplicitNamespaceUsage = content.Contains("MonoChrome.Core.CharacterType") || 
                                            content.Contains("MonoChrome.Characters.CharacterType") ||
                                            content.Contains("MonoChrome.Core.SenseType") || 
                                            content.Contains("MonoChrome.Characters.SenseType") ||
                                            content.Contains("MonoChrome.Core.PatternType") || 
                                            content.Contains("MonoChrome.Combat.PatternType") ||
                                            content.Contains("MonoChrome.Core.StatusEffectType") || 
                                            content.Contains("MonoChrome.StatusEffects.StatusEffectType");
                                            
            // 네임스페이스 충돌이 있지만 해결책이 없는 경우
            return hasMultipleNamespaces && !hasExplicitNamespaceUsage && !HasTypeAliases(content);
        }
        
        /// <summary>
        /// 타입 별칭이 사용되었는지 확인
        /// </summary>
        private bool HasTypeAliases(string content)
        {
            return content.Contains("using CoreCharacterType =") || 
                   content.Contains("using CoreSenseType =") || 
                   content.Contains("using CorePatternType =") || 
                   content.Contains("using CoreStatusEffectType =") ||
                   content.Contains("using CharactersCharacterType =") ||
                   content.Contains("using CharactersSenseType =") ||
                   content.Contains("using CombatPatternType =") ||
                   content.Contains("using StatusEffectsStatusEffectType =");
        }
        
        /// <summary>
        /// 자동 수정 적용
        /// </summary>
        private void ApplyAutomaticFixes()
        {
            logMessages.Clear();
            logMessages.Add("자동 수정 적용 중...");
            
            string[] cSharpFiles = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
            int fixedCount = 0;
            
            foreach (string file in cSharpFiles)
            {
                // 에디터 스크립트 제외
                if (file.Contains("NamespaceFixerTool.cs") || file.Contains("TypeValidationHelper.cs"))
                {
                    continue;
                }
                
                string content = File.ReadAllText(file);
                bool hasConflict = false;
                
                if (updateAllFiles || HasAmbiguousReferences(content) || HasNamespaceConflicts(content))
                {
                    hasConflict = true;
                }
                
                if (hasConflict)
                {
                    // 파일 백업 생성
                    if (autoBackup)
                    {
                        string backupPath = file + ".bak";
                        File.WriteAllText(backupPath, content);
                    }
                    
                    // 수정된 내용
                    string modifiedContent = ApplyFixesToContent(content);
                    
                    // 변경된 경우에만 저장
                    if (modifiedContent != content)
                    {
                        File.WriteAllText(file, modifiedContent);
                        fixedCount++;
                        logMessages.Add($"수정 완료: {Path.GetFileName(file)}");
                    }
                }
            }
            
            logMessages.Add($"자동 수정 완료: {fixedCount}개의 파일 수정됨");
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 파일 내용에 자동 수정 적용
        /// </summary>
        private string ApplyFixesToContent(string content)
        {
            // 이미 타입 별칭을 사용 중인 경우 건너뜀
            if (HasTypeAliases(content))
            {
                return content;
            }
            
            // 네임스페이스 추가
            bool addedUsings = false;
            
            // 여러 네임스페이스를 사용 중인 경우에만 추가
            if (addNamespaceUsings && 
                ((content.Contains("MonoChrome.Core") && content.Contains("MonoChrome.Characters")) ||
                 (content.Contains("MonoChrome.Core") && content.Contains("MonoChrome.Combat")) ||
                 (content.Contains("MonoChrome.Core") && content.Contains("MonoChrome.StatusEffects"))))
            {
                System.Text.StringBuilder usingStatements = new System.Text.StringBuilder();
                
                // 기존 using 문 뒤에 추가
                if (content.Contains("using MonoChrome.Core;"))
                {
                    usingStatements.AppendLine("\n// 타입 별칭 정의");
                    
                    if (content.Contains("CharacterType") && 
                        content.Contains("MonoChrome.Core") && 
                        content.Contains("MonoChrome.Characters"))
                    {
                        usingStatements.AppendLine("using CoreCharacterType = MonoChrome.Core.CharacterType;");
                        usingStatements.AppendLine("using CharactersCharacterType = MonoChrome.Characters.CharacterType;");
                    }
                    
                    if (content.Contains("SenseType") && 
                        content.Contains("MonoChrome.Core") && 
                        content.Contains("MonoChrome.Characters"))
                    {
                        usingStatements.AppendLine("using CoreSenseType = MonoChrome.Core.SenseType;");
                        usingStatements.AppendLine("using CharactersSenseType = MonoChrome.Characters.SenseType;");
                    }
                    
                    if (content.Contains("PatternType") && 
                        content.Contains("MonoChrome.Core") && 
                        content.Contains("MonoChrome.Combat"))
                    {
                        usingStatements.AppendLine("using CorePatternType = MonoChrome.Core.PatternType;");
                        usingStatements.AppendLine("using CombatPatternType = MonoChrome.Combat.PatternType;");
                    }
                    
                    if (content.Contains("StatusEffectType") && 
                        content.Contains("MonoChrome.Core") && 
                        content.Contains("MonoChrome.StatusEffects"))
                    {
                        usingStatements.AppendLine("using CoreStatusEffectType = MonoChrome.Core.StatusEffectType;");
                        usingStatements.AppendLine("using StatusEffectsStatusEffectType = MonoChrome.StatusEffects.StatusEffectType;");
                    }
                }
                
                if (usingStatements.Length > 0)
                {
                    addedUsings = true;
                    
                    // using 구문 위치 찾기
                    Regex usingRegex = new Regex(@"using [^;]+;");
                    MatchCollection matches = usingRegex.Matches(content);
                    
                    if (matches.Count > 0)
                    {
                        Match lastMatch = matches[matches.Count - 1];
                        int lastUsingIndex = lastMatch.Index + lastMatch.Length;
                        
                        content = content.Insert(lastUsingIndex, usingStatements.ToString());
                    }
                    else
                    {
                        // using 구문이 없는 경우 파일 상단에 추가
                        content = usingStatements.ToString() + content;
                    }
                }
            }
            
            // 전체 네임스페이스 사용으로 수정
            if (useFullNamespace && !addedUsings)
            {
                // 모호한 타입 참조를 전체 네임스페이스로 변경
                content = Regex.Replace(content, @"(?<!\w)CharacterType(?!\.)", "MonoChrome.Core.CharacterType");
                content = Regex.Replace(content, @"(?<!\w)SenseType(?!\.)", "MonoChrome.Core.SenseType");
                content = Regex.Replace(content, @"(?<!\w)PatternType(?!\.)", "MonoChrome.Core.PatternType");
                content = Regex.Replace(content, @"(?<!\w)StatusEffectType(?!\.)", "MonoChrome.Core.StatusEffectType");
                content = Regex.Replace(content, @"(?<!\w)ActiveSkillType(?!\.)", "MonoChrome.Core.ActiveSkillType");
            }
            
            return content;
        }
    }
}
#endif
