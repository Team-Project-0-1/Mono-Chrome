#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome.Dungeon.Editor
{
    /// <summary>
    /// 스테이지 테마 데이터 생성 및 관리 도구
    /// 포트폴리오급 에디터 도구로 디자이너 워크플로우 지원
    /// </summary>
    public class StageThemeCreator : EditorWindow
    {
        private const string STAGE_THEMES_PATH = "Assets/Data/StageThemes";
        private const string SCRIPTABLE_OBJECTS_PATH = "Assets/ScriptableObjects/StageThemes";
        
        private Vector2 scrollPosition;
        private StageThemeDataAsset[] existingThemes;
        
        [MenuItem("MONOCHROME/Dungeon/Stage Theme Creator")]
        public static void ShowWindow()
        {
            StageThemeCreator window = GetWindow<StageThemeCreator>("Stage Theme Creator");
            window.minSize = new Vector2(600, 700);
            window.Show();
        }
        
        private void OnEnable()
        {
            RefreshExistingThemes();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("MONOCHROME: Stage Theme Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            DrawCreateDefaultThemesSection();
            GUILayout.Space(20);
            
            DrawExistingThemesSection();
            GUILayout.Space(20);
            
            DrawUtilitySection();
        }
        
        private void DrawCreateDefaultThemesSection()
        {
            EditorGUILayout.LabelField("기본 스테이지 테마 생성", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("MONOCHROME 게임의 기본 3개 스테이지 테마를 자동 생성합니다.", MessageType.Info);
            
            if (GUILayout.Button("모든 기본 테마 생성", GUILayout.Height(30)))
            {
                CreateAllDefaultThemes();
            }
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("스테이지 1: 벙커 외곽"))
            {
                CreateStage1Theme();
            }
            if (GUILayout.Button("스테이지 2: 중립 지대"))
            {
                CreateStage2Theme();
            }
            if (GUILayout.Button("스테이지 3: 최종 심층"))
            {
                CreateStage3Theme();
            }
            GUILayout.EndHorizontal();
        }
        
        private void DrawExistingThemesSection()
        {
            EditorGUILayout.LabelField("기존 스테이지 테마", EditorStyles.boldLabel);
            
            if (GUILayout.Button("새로고침"))
            {
                RefreshExistingThemes();
            }
            
            if (existingThemes == null || existingThemes.Length == 0)
            {
                EditorGUILayout.HelpBox("생성된 스테이지 테마가 없습니다.", MessageType.Warning);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            foreach (var theme in existingThemes)
            {
                if (theme == null) continue;
                
                GUILayout.BeginHorizontal("box");
                
                EditorGUILayout.LabelField(theme.StageDisplayName, GUILayout.Width(150));
                EditorGUILayout.LabelField($"턴:{theme.TotalTurns} 분기:{theme.BranchesPerTurn}", GUILayout.Width(100));
                
                if (GUILayout.Button("선택", GUILayout.Width(50)))
                {
                    Selection.activeObject = theme;
                    EditorGUIUtility.PingObject(theme);
                }
                
                if (GUILayout.Button("검증", GUILayout.Width(50)))
                {
                    theme.ValidateData();
                }
                
                if (GUILayout.Button("정규화", GUILayout.Width(60)))
                {
                    theme.NormalizeChances();
                    EditorUtility.SetDirty(theme);
                }
                
                GUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawUtilitySection()
        {
            EditorGUILayout.LabelField("유틸리티", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("폴더 열기"))
            {
                string path = Path.Combine(Application.dataPath, "Data/StageThemes");
                if (Directory.Exists(path))
                {
                    EditorUtility.RevealInFinder(path);
                }
                else
                {
                    Directory.CreateDirectory(path);
                    AssetDatabase.Refresh();
                    Debug.Log("StageThemes 폴더가 생성되었습니다: " + path);
                }
            }
            
            if (GUILayout.Button("에셋 새로고침"))
            {
                AssetDatabase.Refresh();
                RefreshExistingThemes();
            }
            GUILayout.EndHorizontal();
            
            GUILayout.Space(10);
            EditorGUILayout.HelpBox("생성된 스테이지 테마는 DungeonManager의 ProceduralDungeonGenerator에서 사용됩니다.", MessageType.Info);
        }
        
        private void CreateAllDefaultThemes()
        {
            EnsureDirectoryExists();
            
            CreateStage1Theme();
            CreateStage2Theme();
            CreateStage3Theme();
            
            AssetDatabase.Refresh();
            RefreshExistingThemes();
            
            Debug.Log("모든 기본 스테이지 테마가 생성되었습니다!");
        }
        
        private void CreateStage1Theme()
        {
            var theme = CreateThemeAsset("Stage1_BunkerOutskirts");
            
            // 스테이지 1: 벙커 외곽 설정
            SetPrivateField(theme, "stageDisplayName", "스테이지 1: 벙커 외곽");
            SetPrivateField(theme, "stageDescription", "벙커 밖의 위험한 외곽 지역. 돌연변이와 폐허가 기다리고 있다.");
            SetPrivateField(theme, "stageColor", new Color(0.7f, 0.4f, 0.2f)); // 갈색톤
            
            // 기본 던전 구조
            SetPrivateField(theme, "totalTurns", 15);
            SetPrivateField(theme, "branchesPerTurn", 3);
            SetPrivateField(theme, "miniBossPosition", 6);
            
            // 노드 타입 확률 (초보자 친화적)
            SetPrivateField(theme, "combatChance", 0.5f);
            SetPrivateField(theme, "eventChance", 0.25f);
            SetPrivateField(theme, "shopChance", 0.15f);
            SetPrivateField(theme, "restChance", 0.1f);
            
            // 보장 요소
            SetPrivateField(theme, "guaranteedShops", 2);
            SetPrivateField(theme, "guaranteedRests", 2);
            SetPrivateField(theme, "maxEvents", 3);
            SetPrivateField(theme, "minCombatRooms", 6);
            
            // 특수 설정
            SetPrivateField(theme, "forceShopBeforeMiniBoss", true);
            SetPrivateField(theme, "forceRestAfterMiniBoss", true);
            
            // 적 풀 설정
            var enemyPool = new StageThemeDataAsset.EnemySpawnData[]
            {
                new StageThemeDataAsset.EnemySpawnData
                {
                    enemyName = "들개",
                    enemyType = CharacterType.Normal,
                    spawnWeight = 3,
                    spawnChance = 0.8f,
                    minStageLevel = 1,
                    description = "굶주린 야생견"
                },
                new StageThemeDataAsset.EnemySpawnData
                {
                    enemyName = "약탈자",
                    enemyType = CharacterType.Normal,
                    spawnWeight = 2,
                    spawnChance = 0.6f,
                    minStageLevel = 1,
                    description = "폐허를 배회하는 생존자"
                }
            };
            SetPrivateField(theme, "enemyPool", enemyPool);
            
            // 이벤트 풀 설정
            var eventPool = new StageThemeDataAsset.EventSpawnData[]
            {
                new StageThemeDataAsset.EventSpawnData
                {
                    eventName = "폐허 탐색",
                    eventID = 1,
                    spawnWeight = 2,
                    spawnChance = 1.0f,
                    requiredSenses = new SenseType[] { SenseType.None },
                    eventDescription = "버려진 건물을 조사한다"
                },
                new StageThemeDataAsset.EventSpawnData
                {
                    eventName = "수상한 소리",
                    eventID = 2,
                    spawnWeight = 1,
                    spawnChance = 1.0f,
                    requiredSenses = new SenseType[] { SenseType.Auditory },
                    eventDescription = "이상한 소리가 들린다"
                }
            };
            SetPrivateField(theme, "eventPool", eventPool);
            
            EditorUtility.SetDirty(theme);
            Debug.Log("스테이지 1 테마 생성 완료!");
        }
        
        private void CreateStage2Theme()
        {
            var theme = CreateThemeAsset("Stage2_NeutralZone");
            
            // 스테이지 2: 중립 지대 설정
            SetPrivateField(theme, "stageDisplayName", "스테이지 2: 중립 지대");
            SetPrivateField(theme, "stageDescription", "빛을 먹는 포식자들이 활동하는 위험한 중간 지역.");
            SetPrivateField(theme, "stageColor", new Color(0.3f, 0.3f, 0.6f)); // 푸른톤
            
            // 기본 던전 구조 (난이도 증가)
            SetPrivateField(theme, "totalTurns", 15);
            SetPrivateField(theme, "branchesPerTurn", 3);
            SetPrivateField(theme, "miniBossPosition", 7);
            
            // 노드 타입 확률 (전투 비중 증가)
            SetPrivateField(theme, "combatChance", 0.6f);
            SetPrivateField(theme, "eventChance", 0.2f);
            SetPrivateField(theme, "shopChance", 0.1f);
            SetPrivateField(theme, "restChance", 0.1f);
            
            // 보장 요소 (자원 부족)
            SetPrivateField(theme, "guaranteedShops", 1);
            SetPrivateField(theme, "guaranteedRests", 1);
            SetPrivateField(theme, "maxEvents", 2);
            SetPrivateField(theme, "minCombatRooms", 8);
            
            EditorUtility.SetDirty(theme);
            Debug.Log("스테이지 2 테마 생성 완료!");
        }
        
        private void CreateStage3Theme()
        {
            var theme = CreateThemeAsset("Stage3_FinalDepths");
            
            // 스테이지 3: 최종 심층 설정
            SetPrivateField(theme, "stageDisplayName", "스테이지 3: 최종 심층");
            SetPrivateField(theme, "stageDescription", "빛의 근원에 가까워질수록 더욱 강력한 적들이 기다린다.");
            SetPrivateField(theme, "stageColor", new Color(0.6f, 0.2f, 0.2f)); // 붉은톤
            
            // 기본 던전 구조 (최고 난이도)
            SetPrivateField(theme, "totalTurns", 15);
            SetPrivateField(theme, "branchesPerTurn", 3);
            SetPrivateField(theme, "miniBossPosition", 8);
            
            // 노드 타입 확률 (전투 위주)
            SetPrivateField(theme, "combatChance", 0.7f);
            SetPrivateField(theme, "eventChance", 0.15f);
            SetPrivateField(theme, "shopChance", 0.1f);
            SetPrivateField(theme, "restChance", 0.05f);
            
            // 보장 요소 (최소한의 지원)
            SetPrivateField(theme, "guaranteedShops", 1);
            SetPrivateField(theme, "guaranteedRests", 1);
            SetPrivateField(theme, "maxEvents", 1);
            SetPrivateField(theme, "minCombatRooms", 10);
            
            EditorUtility.SetDirty(theme);
            Debug.Log("스테이지 3 테마 생성 완료!");
        }
        
        private StageThemeDataAsset CreateThemeAsset(string fileName)
        {
            string assetPath = Path.Combine(STAGE_THEMES_PATH, fileName + ".asset");
            
            // 기존 에셋이 있으면 로드, 없으면 새로 생성
            StageThemeDataAsset theme = AssetDatabase.LoadAssetAtPath<StageThemeDataAsset>(assetPath);
            
            if (theme == null)
            {
                theme = CreateInstance<StageThemeDataAsset>();
                AssetDatabase.CreateAsset(theme, assetPath);
            }
            
            return theme;
        }
        
        private void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName, 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"필드를 찾을 수 없습니다: {fieldName}");
            }
        }
        
        private void EnsureDirectoryExists()
        {
            if (!Directory.Exists(STAGE_THEMES_PATH))
            {
                Directory.CreateDirectory(STAGE_THEMES_PATH);
                AssetDatabase.Refresh();
            }
        }
        
        private void RefreshExistingThemes()
        {
            string[] guids = AssetDatabase.FindAssets("t:StageThemeDataAsset", new[] { STAGE_THEMES_PATH });
            existingThemes = new StageThemeDataAsset[guids.Length];
            
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                existingThemes[i] = AssetDatabase.LoadAssetAtPath<StageThemeDataAsset>(path);
            }
        }
    }
}
#endif