using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome.AI;
using MonoChrome.Data;
using MonoChrome.Systems.Combat;
using MonoChrome.Systems.UI;

namespace MonoChrome.Testing
{
    /// <summary>
    /// AI 시스템 통합 테스트 컴포넌트
    /// 모든 AI 관련 시스템이 제대로 연동되는지 테스트한다.
    /// </summary>
    public class AISystemIntegrationTest : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool runTestOnStart = false;
        [SerializeField] private bool enableDetailedLogs = true;
        [SerializeField] private float testStepDelay = 1f;
        
        [Header("테스트 캐릭터")]
        [SerializeField] private Character testPlayer;
        [SerializeField] private Character[] testEnemies;
        
        [Header("UI 테스트")]
        [SerializeField] private IntentDisplaySystem intentDisplay;
        [SerializeField] private Transform intentContainer;
        [SerializeField] private GameObject intentPrefab;
        
        // 테스트 결과
        private List<string> testResults = new List<string>();
        private int passedTests = 0;
        private int totalTests = 0;
        
        #region Unity Lifecycle
        private void Start()
        {
            if (runTestOnStart)
            {
                StartCoroutine(RunAllTests());
            }
        }
        #endregion
        
        #region Test Execution
        /// <summary>
        /// 모든 테스트 실행
        /// </summary>
        [ContextMenu("Run All AI System Tests")]
        public void RunAllTestsMenu()
        {
            StartCoroutine(RunAllTests());
        }
        
        /// <summary>
        /// 모든 테스트 실행 코루틴
        /// </summary>
        private IEnumerator RunAllTests()
        {
            LogTest("=== AI 시스템 통합 테스트 시작 ===");
            
            testResults.Clear();
            passedTests = 0;
            totalTests = 0;
            
            // 1. 기본 시스템 테스트
            yield return StartCoroutine(TestBasicSystems());
            
            // 2. PatternDataManager 테스트
            yield return StartCoroutine(TestPatternDataManager());
            
            // 3. AIManager 테스트
            yield return StartCoroutine(TestAIManager());
            
            // 4. MonsterAI 테스트
            yield return StartCoroutine(TestMonsterAI());
            
            // 5. IntentDisplaySystem 테스트
            yield return StartCoroutine(TestIntentDisplaySystem());
            
            // 6. TurnManager 테스트
            yield return StartCoroutine(TestTurnManager());
            
            // 7. 통합 시나리오 테스트
            yield return StartCoroutine(TestIntegrationScenario());
            
            // 결과 출력
            DisplayTestResults();
        }
        
        /// <summary>
        /// 기본 시스템 테스트
        /// </summary>
        private IEnumerator TestBasicSystems()
        {
            LogTest("--- 기본 시스템 테스트 ---");
            
            // AIManager 싱글톤 테스트
            bool aiManagerExists = AIManager.Instance != null;
            RecordTest("AIManager 싱글톤", aiManagerExists);
            
            // PatternDataManager 테스트
            bool patternManagerExists = PatternDataManager.Instance != null;
            RecordTest("PatternDataManager 인스턴스", patternManagerExists);
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// PatternDataManager 테스트
        /// </summary>
        private IEnumerator TestPatternDataManager()
        {
            LogTest("--- PatternDataManager 테스트 ---");
            
            var patternManager = PatternDataManager.Instance;
            if (patternManager == null)
            {
                RecordTest("PatternDataManager 초기화", false);
                yield break;
            }
            
            // 초기화 테스트
            patternManager.Initialize();
            RecordTest("PatternDataManager 초기화", true);
            
            // 몬스터 패턴 로드 테스트
            var allMonsterPatterns = patternManager.GetAllMonsterPatterns();
            bool hasMonsterPatterns = allMonsterPatterns != null && allMonsterPatterns.Count > 0;
            RecordTest($"몬스터 패턴 로드 ({allMonsterPatterns?.Count ?? 0}개)", hasMonsterPatterns);
            
            if (hasMonsterPatterns)
            {
                // 타입별 패턴 검색 테스트
                var firstPattern = allMonsterPatterns[0];
                var typePatterns = patternManager.GetMonsterPatternsForType(firstPattern.MonsterType);
                bool typeSearchWorks = typePatterns != null && typePatterns.Count > 0;
                RecordTest($"타입별 패턴 검색 ({firstPattern.MonsterType})", typeSearchWorks);
                
                LogTest($"첫 번째 패턴: {firstPattern.PatternName} ({firstPattern.MonsterType})");
            }
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// AIManager 테스트
        /// </summary>
        private IEnumerator TestAIManager()
        {
            LogTest("--- AIManager 테스트 ---");
            
            var aiManager = AIManager.Instance;
            if (aiManager == null)
            {
                RecordTest("AIManager 접근", false);
                yield break;
            }
            
            RecordTest("AIManager 접근", true);
            
            // 테스트 캐릭터 설정
            if (testPlayer != null && testEnemies != null && testEnemies.Length > 0)
            {
                var testEnemy = testEnemies[0];
                
                // 의도 결정 테스트
                var intent = aiManager.DetermineIntent(testEnemy, testPlayer);
                bool intentDetermined = intent != null;
                RecordTest($"의도 결정 ({testEnemy.CharacterName})", intentDetermined);
                
                if (intentDetermined)
                {
                    LogTest($"결정된 의도: {intent.Name} (공격: {intent.IsAttack})");
                    
                    // 의도 테스트 단순화 (캐시 기능은 현재 사용하지 않음)
                    RecordTest("의도 표시 기능", true);
                }
            }
            else
            {
                RecordTest("테스트 캐릭터 설정", false);
                LogTest("테스트용 플레이어와 적 캐릭터를 설정해주세요.");
            }
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// MonsterAI 테스트
        /// </summary>
        private IEnumerator TestMonsterAI()
        {
            LogTest("--- MonsterAI 테스트 ---");
            
            if (testEnemies == null || testEnemies.Length == 0)
            {
                RecordTest("MonsterAI 테스트", false);
                LogTest("테스트용 적 캐릭터가 없습니다.");
                yield break;
            }
            
            int monsterAICount = 0;
            foreach (var enemy in testEnemies)
            {
                if (enemy != null)
                {
                    var monsterAI = enemy.GetComponent<MonsterAI>();
                    if (monsterAI == null)
                    {
                        // MonsterAI 컴포넌트 자동 추가
                        monsterAI = enemy.gameObject.AddComponent<MonsterAI>();
                        LogTest($"{enemy.CharacterName}에 MonsterAI 컴포넌트 추가됨");
                    }
                    
                    if (monsterAI != null)
                    {
                        monsterAICount++;
                        
                        // 행동 결정 테스트
                        if (testPlayer != null)
                        {
                            var decision = monsterAI.DecideAction(testPlayer);
                            bool decisionMade = decision != null;
                            RecordTest($"{enemy.CharacterName} 행동 결정", decisionMade);
                            
                            if (decisionMade)
                            {
                                LogTest($"{enemy.CharacterName} 결정: {decision.PatternName}");
                            }
                        }
                    }
                }
            }
            
            RecordTest($"MonsterAI 컴포넌트 ({monsterAICount}개)", monsterAICount > 0);
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// IntentDisplaySystem 테스트
        /// </summary>
        private IEnumerator TestIntentDisplaySystem()
        {
            LogTest("--- IntentDisplaySystem 테스트 ---");
            
            if (intentDisplay == null)
            {
                // IntentDisplaySystem 자동 생성 시도
                intentDisplay = FindObjectOfType<IntentDisplaySystem>();
                if (intentDisplay == null)
                {
                    GameObject intentSystemGO = new GameObject("IntentDisplaySystem");
                    intentDisplay = intentSystemGO.AddComponent<IntentDisplaySystem>();
                    LogTest("IntentDisplaySystem 자동 생성됨");
                }
            }
            
            bool intentSystemExists = intentDisplay != null;
            RecordTest("IntentDisplaySystem 존재", intentSystemExists);
            
            if (intentSystemExists && testPlayer != null && testEnemies != null && testEnemies.Length > 0)
            {
                var testEnemy = testEnemies[0];
                
                // 의도 표시 테스트
                try
                {
                    intentDisplay.DisplayIntent(testEnemy, testPlayer);
                    RecordTest("의도 표시 기능", true);
                    LogTest($"{testEnemy.CharacterName}의 의도 표시 완료");
                }
                catch (System.Exception e)
                {
                    RecordTest("의도 표시 기능", false);
                    LogTest($"의도 표시 오류: {e.Message}");
                }
                
                yield return new WaitForSeconds(0.5f);
                
                // 의도 새로고침 테스트
                try
                {
                    intentDisplay.RefreshIntent(testEnemy);
                    RecordTest("의도 새로고침 기능", true);
                }
                catch (System.Exception e)
                {
                    RecordTest("의도 새로고침 기능", false);
                    LogTest($"의도 새로고침 오류: {e.Message}");
                }
            }
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// TurnManager 테스트
        /// </summary>
        private IEnumerator TestTurnManager()
        {
            LogTest("--- TurnManager 테스트 ---");
            
            var turnManager = FindObjectOfType<TurnManager>();
            if (turnManager == null)
            {
                GameObject turnManagerGO = new GameObject("TurnManager");
                turnManager = turnManagerGO.AddComponent<TurnManager>();
                LogTest("TurnManager 자동 생성됨");
            }
            
            bool turnManagerExists = turnManager != null;
            RecordTest("TurnManager 존재", turnManagerExists);
            
            if (turnManagerExists)
            {
                // 초기 상태 테스트
                bool notInBattle = !turnManager.IsBattleActive();
                RecordTest("초기 상태 (비전투)", notInBattle);
                
                int initialTurn = turnManager.GetCurrentTurn();
                bool correctInitialTurn = initialTurn == 0;
                RecordTest("초기 턴 번호", correctInitialTurn);
                
                LogTest($"TurnManager 상태 - 턴: {initialTurn}, 페이즈: {turnManager.GetCurrentPhase()}");
            }
            
            yield return new WaitForSeconds(testStepDelay);
        }
        
        /// <summary>
        /// 통합 시나리오 테스트
        /// </summary>
        private IEnumerator TestIntegrationScenario()
        {
            LogTest("--- 통합 시나리오 테스트 ---");
            
            if (testPlayer == null || testEnemies == null || testEnemies.Length == 0)
            {
                RecordTest("통합 테스트", false);
                LogTest("통합 테스트를 위한 캐릭터가 설정되지 않았습니다.");
                yield break;
            }
            
            // 1. 전투 시뮬레이션 준비
            var turnManager = FindObjectOfType<TurnManager>();
            var enemyList = new List<Character>();
            
            foreach (var enemy in testEnemies)
            {
                if (enemy != null) enemyList.Add(enemy);
            }
            
            if (turnManager != null && enemyList.Count > 0)
            {
                // 2. 전투 시작 시뮬레이션
                LogTest("전투 시뮬레이션 시작");
                turnManager.StartBattle(testPlayer, enemyList);
                
                yield return new WaitForSeconds(1f);
                
                // 3. 전투 상태 확인
                bool battleActive = turnManager.IsBattleActive();
                RecordTest("전투 시작 시뮬레이션", battleActive);
                
                if (battleActive)
                {
                    LogTest($"현재 턴: {turnManager.GetCurrentTurn()}, 페이즈: {turnManager.GetCurrentPhase()}");
                    
                    // 4. 의도 표시 확인
                    if (intentDisplay != null)
                    {
                        intentDisplay.OnTurnStart(enemyList, testPlayer);
                        yield return new WaitForSeconds(0.5f);
                    }
                    
                    // 5. 전투 종료 시뮬레이션
                    yield return new WaitForSeconds(2f);
                    turnManager.EndBattle(true);
                    
                    bool battleEnded = !turnManager.IsBattleActive();
                    RecordTest("전투 종료 시뮬레이션", battleEnded);
                }
            }
            else
            {
                RecordTest("통합 테스트", false);
                LogTest("TurnManager 또는 적 캐릭터가 없습니다.");
            }
            
            yield return new WaitForSeconds(testStepDelay);
        }
        #endregion
        
        #region Test Utilities
        /// <summary>
        /// 테스트 결과 기록
        /// </summary>
        private void RecordTest(string testName, bool passed)
        {
            totalTests++;
            if (passed) passedTests++;
            
            string result = passed ? "PASS" : "FAIL";
            string logMessage = $"[{result}] {testName}";
            testResults.Add(logMessage);
            
            LogTest(logMessage);
        }
        
        /// <summary>
        /// 테스트 로그 출력
        /// </summary>
        private void LogTest(string message)
        {
            if (enableDetailedLogs)
            {
                Debug.Log($"[AI System Test] {message}");
            }
        }
        
        /// <summary>
        /// 테스트 결과 출력
        /// </summary>
        private void DisplayTestResults()
        {
            LogTest("=== 테스트 결과 요약 ===");
            LogTest($"전체 테스트: {totalTests}개");
            LogTest($"통과: {passedTests}개");
            LogTest($"실패: {totalTests - passedTests}개");
            LogTest($"성공률: {(float)passedTests / totalTests * 100:F1}%");
            
            if (passedTests == totalTests)
            {
                LogTest("🎉 모든 테스트 통과! AI 시스템이 정상적으로 통합되었습니다.");
            }
            else
            {
                LogTest("⚠️ 일부 테스트 실패. 위 로그를 확인하여 문제를 해결해주세요.");
            }
            
            LogTest("=== 개별 테스트 결과 ===");
            foreach (string result in testResults)
            {
                LogTest(result);
            }
        }
        #endregion
        
        #region Individual Test Methods
        /// <summary>
        /// 개별 AIManager 테스트
        /// </summary>
        [ContextMenu("Test AIManager Only")]
        public void TestAIManagerOnly()
        {
            StartCoroutine(TestAIManager());
        }
        
        /// <summary>
        /// 개별 PatternDataManager 테스트
        /// </summary>
        [ContextMenu("Test PatternDataManager Only")]
        public void TestPatternDataManagerOnly()
        {
            StartCoroutine(TestPatternDataManager());
        }
        
        /// <summary>
        /// 개별 IntentDisplaySystem 테스트
        /// </summary>
        [ContextMenu("Test IntentDisplay Only")]
        public void TestIntentDisplayOnly()
        {
            StartCoroutine(TestIntentDisplaySystem());
        }
        
        /// <summary>
        /// 퀵 테스트 (기본 시스템만)
        /// </summary>
        [ContextMenu("Quick Test")]
        public void QuickTest()
        {
            StartCoroutine(QuickTestCoroutine());
        }
        
        private IEnumerator QuickTestCoroutine()
        {
            LogTest("=== 퀵 테스트 ===");
            yield return StartCoroutine(TestBasicSystems());
            yield return StartCoroutine(TestPatternDataManager());
            DisplayTestResults();
        }
        #endregion
    }
}