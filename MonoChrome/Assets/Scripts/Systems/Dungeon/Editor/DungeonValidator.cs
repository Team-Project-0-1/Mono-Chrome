#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using MonoChrome.Systems.Dungeon;


namespace MonoChrome.Dungeon.Editor
{
    /// <summary>
    /// 던전 생성 검증 및 시각화 도구
    /// 포트폴리오 품질 확보를 위한 고급 디버깅 도구
    /// </summary>
    public class DungeonValidator : EditorWindow
    {
        private DungeonController dungeonController;
        private ProceduralDungeonGenerator procGenerator;
        private ConfigurableDungeonGenerator configGenerator;
        private Vector2 scrollPosition;
        private bool showDetailedStats = true;
        private bool showGenerationLog = true;
        private int testGenerationCount = 10;
        
        // 검증 결과
        private ValidationResult lastValidation;
        private List<string> generationLog = new List<string>();
        
        [MenuItem("MONOCHROME/Dungeon/Dungeon Validator")]
        public static void ShowWindow()
        {
            DungeonValidator window = GetWindow<DungeonValidator>("Dungeon Validator");
            window.minSize = new Vector2(700, 800);
            window.Show();
        }
        
        private void OnEnable()
        {
            FindDungeonManager();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("MONOCHROME: Dungeon Validator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            DrawManagerSection();
            GUILayout.Space(15);
            
            DrawValidationSection();
            GUILayout.Space(15);
            
            DrawGenerationTestSection();
            GUILayout.Space(15);
            
            DrawResultsSection();
        }
        
        private void DrawManagerSection()
        {
            EditorGUILayout.LabelField("던전 매니저", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            dungeonController = (DungeonController)EditorGUILayout.ObjectField("DungeonController:", dungeonController, typeof(DungeonController), true);
            
            if (GUILayout.Button("자동 찾기", GUILayout.Width(80)))
            {
                FindDungeonManager();
            }
            GUILayout.EndHorizontal();
            
            if (dungeonController == null)
            {
                EditorGUILayout.HelpBox("DungeonManager를 찾을 수 없습니다. 씬에 DungeonManager 오브젝트가 있는지 확인하세요.", MessageType.Warning);
                return;
            }
            
            // 생성기 컴포넌트 상태 표시
            procGenerator = dungeonController.GetComponent<ProceduralDungeonGenerator>();
            var improvedGen = dungeonController.GetComponent<ImprovedDungeonGenerator>();
            var advancedGen = dungeonController.GetComponent<AdvancedDungeonGenerator>();
            configGenerator = dungeonController.GetComponent<ConfigurableDungeonGenerator>();
            
            EditorGUILayout.LabelField("생성기 컴포넌트 상태:", EditorStyles.miniLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField($"• ProceduralDungeonGenerator: {(procGenerator != null ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"• ImprovedDungeonGenerator: {(improvedGen != null ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"• AdvancedDungeonGenerator: {(advancedGen != null ? "✓" : "✗")}");
            EditorGUILayout.LabelField($"• ConfigurableDungeonGenerator: {(configGenerator != null ? "✓" : "✗")}");
            EditorGUI.indentLevel--;
        }
        
        private void DrawValidationSection()
        {
            EditorGUILayout.LabelField("단일 던전 검증", EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("현재 설정으로 던전 생성 & 검증"))
            {
                ValidateSingleDungeon();
            }
            
            if (GUILayout.Button("스테이지별 검증"))
            {
                ValidateAllStages();
            }
            GUILayout.EndHorizontal();
            
            showDetailedStats = EditorGUILayout.Toggle("상세 통계 표시", showDetailedStats);
        }
        
        private void DrawGenerationTestSection()
        {
            EditorGUILayout.LabelField("반복 생성 테스트", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("여러 번 생성하여 일관성과 안정성을 테스트합니다.", MessageType.Info);
            
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("테스트 횟수:", GUILayout.Width(80));
            testGenerationCount = EditorGUILayout.IntSlider(testGenerationCount, 1, 50);
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button($"{testGenerationCount}회 반복 생성 테스트"))
            {
                RunRepeatedGenerationTest();
            }
            
            showGenerationLog = EditorGUILayout.Toggle("생성 로그 표시", showGenerationLog);
        }
        
        private void DrawResultsSection()
        {
            if (lastValidation == null && generationLog.Count == 0) return;
            
            EditorGUILayout.LabelField("검증 결과", EditorStyles.boldLabel);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // 마지막 검증 결과 표시
            if (lastValidation != null)
            {
                DrawValidationResult(lastValidation);
            }
            
            // 생성 로그 표시
            if (showGenerationLog && generationLog.Count > 0)
            {
                GUILayout.Space(10);
                EditorGUILayout.LabelField("생성 로그:", EditorStyles.boldLabel);
                
                foreach (string log in generationLog.TakeLast(20)) // 최근 20개만 표시
                {
                    EditorGUILayout.LabelField($"• {log}", EditorStyles.miniLabel);
                }
                
                if (GUILayout.Button("로그 지우기"))
                {
                    generationLog.Clear();
                }
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawValidationResult(ValidationResult result)
        {
            // 전체 상태
            Color originalColor = GUI.color;
            GUI.color = result.IsValid ? Color.green : Color.red;
            EditorGUILayout.LabelField($"검증 결과: {(result.IsValid ? "통과" : "실패")}", EditorStyles.boldLabel);
            GUI.color = originalColor;
            
            // 기본 정보
            EditorGUILayout.LabelField($"생성 시간: {result.GenerationTime:F3}초");
            EditorGUILayout.LabelField($"총 노드 수: {result.TotalNodes}개");
            
            if (showDetailedStats)
            {
                // 노드 분포
                GUILayout.Space(5);
                EditorGUILayout.LabelField("노드 분포:", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"전투: {result.CombatNodes}개");
                EditorGUILayout.LabelField($"이벤트: {result.EventNodes}개");
                EditorGUILayout.LabelField($"상점: {result.ShopNodes}개");
                EditorGUILayout.LabelField($"휴식: {result.RestNodes}개");
                EditorGUILayout.LabelField($"미니보스: {result.MiniBossNodes}개");
                EditorGUILayout.LabelField($"보스: {result.BossNodes}개");
                EditorGUI.indentLevel--;
            }
            
            // 경고 및 오류
            if (result.Warnings.Count > 0)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("경고:", EditorStyles.boldLabel);
                foreach (string warning in result.Warnings)
                {
                    EditorGUILayout.HelpBox(warning, MessageType.Warning);
                }
            }
            
            if (result.Errors.Count > 0)
            {
                GUILayout.Space(5);
                EditorGUILayout.LabelField("오류:", EditorStyles.boldLabel);
                foreach (string error in result.Errors)
                {
                    EditorGUILayout.HelpBox(error, MessageType.Error);
                }
            }
        }
        
        private void ValidateSingleDungeon()
        {
            if (dungeonController == null)
            {
                Debug.LogError("DungeonManager가 설정되지 않았습니다.");
                return;
            }
            
            generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] 단일 던전 검증 시작");
            
            float startTime = Time.realtimeSinceStartup;
            
            try
            {
                // 던전 생성
                List<DungeonNode> nodes = null;

                if (configGenerator != null)
                {
                    nodes = configGenerator.GenerateDungeon(0);
                    generationLog.Add($"ConfigurableDungeonGenerator 사용 ({configGenerator.name})");
                }
                else if (procGenerator != null)
                {
                    nodes = procGenerator.GenerateProceduralDungeon(0);
                    generationLog.Add("ProceduralDungeonGenerator 사용");
                }
                else
                {
                    // 이벤트 기반 던전 생성 요청
                    MonoChrome.Events.DungeonEvents.RequestDungeonGeneration(0);
                    generationLog.Add("DungeonController event-based generation 사용");
                }
                
                float generationTime = Time.realtimeSinceStartup - startTime;
                
                // 검증 수행
                lastValidation = ValidateDungeonNodes(nodes, generationTime);
                
                generationLog.Add($"검증 완료 - {(lastValidation.IsValid ? "성공" : "실패")}");
                
                Debug.Log($"던전 검증 완료: {(lastValidation.IsValid ? "통과" : "실패")} (생성시간: {generationTime:F3}초)");
            }
            catch (System.Exception e)
            {
                generationLog.Add($"오류 발생: {e.Message}");
                Debug.LogError($"던전 검증 중 오류: {e.Message}");
            }
        }
        
        private void ValidateAllStages()
        {
            if (dungeonController == null || (procGenerator == null && configGenerator == null))
            {
                Debug.LogError("Dungeon generator가 설정되지 않았습니다.");
                return;
            }
            
            generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] 전체 스테이지 검증 시작");
            
            bool allValid = true;
            
            for (int stage = 0; stage < 3; stage++)
            {
                try
                {
                    float startTime = Time.realtimeSinceStartup;
                    var nodes = configGenerator != null
                        ? configGenerator.GenerateDungeon(stage)
                        : procGenerator.GenerateProceduralDungeon(stage);
                    float generationTime = Time.realtimeSinceStartup - startTime;
                    
                    var validation = ValidateDungeonNodes(nodes, generationTime);
                    
                    generationLog.Add($"스테이지 {stage + 1}: {(validation.IsValid ? "통과" : "실패")} ({generationTime:F3}초)");
                    
                    if (!validation.IsValid)
                    {
                        allValid = false;
                        generationLog.Add($"  오류: {string.Join(", ", validation.Errors)}");
                    }
                    
                    if (stage == 0) // 첫 번째 결과를 상세 표시용으로 저장
                    {
                        lastValidation = validation;
                    }
                }
                catch (System.Exception e)
                {
                    allValid = false;
                    generationLog.Add($"스테이지 {stage + 1} 오류: {e.Message}");
                }
            }
            
            Debug.Log($"전체 스테이지 검증 {(allValid ? "성공" : "실패")}");
        }
        
        private void RunRepeatedGenerationTest()
        {
            if (configGenerator == null && procGenerator == null)
            {
                Debug.LogError("Dungeon generator가 설정되지 않았습니다.");
                return;
            }
            
            generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] {testGenerationCount}회 반복 테스트 시작");
            
            int successCount = 0;
            float totalTime = 0f;
            List<float> generationTimes = new List<float>();
            
            for (int i = 0; i < testGenerationCount; i++)
            {
                try
                {
                    float startTime = Time.realtimeSinceStartup;
                    var nodes = configGenerator != null
                        ? configGenerator.GenerateDungeon(0)
                        : procGenerator.GenerateProceduralDungeon(0);
                    float generationTime = Time.realtimeSinceStartup - startTime;
                    
                    var validation = ValidateDungeonNodes(nodes, generationTime);
                    
                    if (validation.IsValid)
                    {
                        successCount++;
                    }
                    
                    totalTime += generationTime;
                    generationTimes.Add(generationTime);
                    
                    if (i == 0) // 첫 번째 결과를 표시용으로 저장
                    {
                        lastValidation = validation;
                    }
                }
                catch (System.Exception e)
                {
                    generationLog.Add($"테스트 {i + 1} 실패: {e.Message}");
                }
            }
            
            // 통계 계산
            float avgTime = totalTime / testGenerationCount;
            float minTime = generationTimes.Min();
            float maxTime = generationTimes.Max();
            float successRate = (float)successCount / testGenerationCount * 100f;
            
            generationLog.Add($"반복 테스트 완료:");
            generationLog.Add($"  성공률: {successRate:F1}% ({successCount}/{testGenerationCount})");
            generationLog.Add($"  평균 시간: {avgTime:F3}초");
            generationLog.Add($"  최소/최대: {minTime:F3}초 / {maxTime:F3}초");
            
            Debug.Log($"반복 생성 테스트 완료 - 성공률: {successRate:F1}%, 평균 시간: {avgTime:F3}초");
        }
        
        private ValidationResult ValidateDungeonNodes(List<DungeonNode> nodes, float generationTime)
        {
            var result = new ValidationResult
            {
                GenerationTime = generationTime,
                TotalNodes = nodes?.Count ?? 0
            };
            
            if (nodes == null || nodes.Count == 0)
            {
                result.Errors.Add("생성된 노드가 없습니다.");
                return result;
            }
            
            // 노드 분포 계산
            foreach (var node in nodes)
            {
                switch (node.Type)
                {
                    case NodeType.Combat: result.CombatNodes++; break;
                    case NodeType.Event: result.EventNodes++; break;
                    case NodeType.Shop: result.ShopNodes++; break;
                    case NodeType.Rest: result.RestNodes++; break;
                    case NodeType.MiniBoss: result.MiniBossNodes++; break;
                    case NodeType.Boss: result.BossNodes++; break;
                }
            }
            
            // 필수 요소 검증
            if (result.MiniBossNodes < 1)
            {
                result.Errors.Add("미니보스가 없습니다.");
            }
            
            if (result.BossNodes < 1)
            {
                result.Errors.Add("보스가 없습니다.");
            }
            
            if (result.CombatNodes < 3)
            {
                result.Warnings.Add("전투 노드가 너무 적습니다.");
            }
            
            if (result.ShopNodes < 1)
            {
                result.Warnings.Add("상점이 없습니다.");
            }
            
            if (result.RestNodes < 1)
            {
                result.Warnings.Add("휴식 지점이 없습니다.");
            }
            
            // 연결성 검증
            ValidateNodeConnectivity(nodes, result);
            
            result.IsValid = result.Errors.Count == 0;
            return result;
        }
        
        private void ValidateNodeConnectivity(List<DungeonNode> nodes, ValidationResult result)
        {
            // 시작 노드 확인
            var accessibleNodes = nodes.Where(n => n.IsAccessible).ToList();
            if (accessibleNodes.Count == 0)
            {
                result.Errors.Add("접근 가능한 시작 노드가 없습니다.");
                return;
            }
            
            // 연결 검증 (간단한 형태)
            foreach (var node in nodes)
            {
                if (node.ConnectedNodes.Count == 0 && node.Type != NodeType.Boss)
                {
                    result.Warnings.Add($"연결되지 않은 노드가 있습니다: {node.Type} (ID: {node.ID})");
                }
            }
        }
        
        private void FindDungeonManager()
        {
            dungeonController = FindObjectOfType<DungeonController>();

            if (dungeonController != null)
            {
                procGenerator = dungeonController.GetComponent<ProceduralDungeonGenerator>();
                configGenerator = dungeonController.GetComponent<ConfigurableDungeonGenerator>();
                generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] DungeonController 발견: {dungeonController.name}");
            }
            else
            {
                generationLog.Add($"[{System.DateTime.Now:HH:mm:ss}] DungeonController를 찾을 수 없음");
            }
        }
        
        [System.Serializable]
        private class ValidationResult
        {
            public bool IsValid = false;
            public float GenerationTime = 0f;
            public int TotalNodes = 0;
            public int CombatNodes = 0;
            public int EventNodes = 0;
            public int ShopNodes = 0;
            public int RestNodes = 0;
            public int MiniBossNodes = 0;
            public int BossNodes = 0;
            public List<string> Warnings = new List<string>();
            public List<string> Errors = new List<string>();
        }
    }
}
#endif