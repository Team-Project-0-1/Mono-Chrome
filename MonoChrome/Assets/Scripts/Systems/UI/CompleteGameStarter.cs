using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MonoChrome;
using MonoChrome;

namespace MonoChrome
{
    /// <summary>
    /// 완전한 게임 시작 및 던전 생성 솔루션
    /// 모든 UI 전환과 던전 생성을 한 번에 처리
    /// </summary>
    public class CompleteGameStarter : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject dungeonPanel;
        [SerializeField] private GameObject combatPanel;
        
        [Header("Test Settings")]
        [SerializeField] private bool autoStartGame = false;
        [SerializeField] private bool debugMode = true;
        [SerializeField] private float delayBetweenSteps = 0.5f;
        
        private void Start()
        {
            LogDebug("CompleteGameStarter: Starting initialization...");
            
            // UI 참조 찾기
            FindAllUIReferences();
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            // 자동 시작 옵션
            if (autoStartGame)
            {
                StartCoroutine(AutoStartGameAfterDelay(2f));
            }
            
            LogDebug("CompleteGameStarter: Initialization completed");
        }
        
        /// <summary>
        /// 모든 UI 참조 찾기
        /// </summary>
        private void FindAllUIReferences()
        {
            // Canvas 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                LogError("Canvas not found!");
                return;
            }
            
            // StartGameButton 찾기
            if (startGameButton == null)
            {
                startGameButton = canvas.GetComponentInChildren<Button>();
                if (startGameButton == null)
                {
                    Transform startButtonTransform = canvas.transform.Find("CharacterSelectionPanel/StartGameButton");
                    if (startButtonTransform != null)
                    {
                        startGameButton = startButtonTransform.GetComponent<Button>();
                    }
                }
            }
            
            // UI 패널들 찾기
            if (characterSelectionPanel == null)
            {
                characterSelectionPanel = canvas.transform.Find("CharacterSelectionPanel")?.gameObject;
            }
            
            if (dungeonPanel == null)
            {
                dungeonPanel = canvas.transform.Find("DungeonPanel")?.gameObject;
            }
            
            if (combatPanel == null)
            {
                combatPanel = canvas.transform.Find("CombatPanel")?.gameObject;
            }
            
            // 로그 출력
            LogDebug($"StartGameButton found: {startGameButton != null}");
            LogDebug($"CharacterSelectionPanel found: {characterSelectionPanel != null}");
            LogDebug($"DungeonPanel found: {dungeonPanel != null}");
            LogDebug($"CombatPanel found: {combatPanel != null}");
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtonEvents()
        {
            if (startGameButton != null)
            {
                startGameButton.onClick.RemoveAllListeners();
                startGameButton.onClick.AddListener(OnStartGameClicked);
                LogDebug("StartGameButton event handler set");
            }
            else
            {
                LogError("StartGameButton not found - cannot setup event handler");
            }
        }
        
        /// <summary>
        /// 게임 시작 버튼 클릭 핸들러
        /// </summary>
        public void OnStartGameClicked()
        {
            LogDebug("CompleteGameStarter: Start Game Button clicked!");
            StartCoroutine(CompleteGameStartSequence());
        }
        
        /// <summary>
        /// 완전한 게임 시작 시퀀스
        /// </summary>
        private IEnumerator CompleteGameStartSequence()
        {
            LogDebug("=== Starting Complete Game Start Sequence ===");
            
            // 1단계: 캐릭터 생성
            yield return StartCoroutine(Step1_CreateCharacter());
            
            // 2단계: UI 패널 전환
            yield return StartCoroutine(Step2_SwitchToDungeonPanel());
            
            // 3단계: 게임 매니저 상태 변경
            yield return StartCoroutine(Step3_ChangeGameState());
            
            // 4단계: 던전 생성
            yield return StartCoroutine(Step4_GenerateDungeon());
            
            // 5단계: 테스트 노드 생성
            yield return StartCoroutine(Step5_CreateTestNodes());
            
            LogDebug("=== Complete Game Start Sequence Finished ===");
        }
        
        /// <summary>
        /// 1단계: 캐릭터 생성
        /// </summary>
        private IEnumerator Step1_CreateCharacter()
        {
            LogDebug("Step 1: Creating player character...");
            
            try
            {
                var characterManager = CharacterManager.Instance;
                if (characterManager != null)
                {
                    characterManager.CreatePlayerCharacter(SenseType.Auditory);
                    LogDebug("Player character created successfully");
                }
                else
                {
                    LogError("CharacterManager.Instance is null");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error creating character: {ex.Message}");
            }
            
            yield return new WaitForSeconds(delayBetweenSteps);
        }
        
        /// <summary>
        /// 2단계: UI 패널 전환
        /// </summary>
        private IEnumerator Step2_SwitchToDungeonPanel()
        {
            LogDebug("Step 2: Switching to dungeon panel...");
            
            try
            {
                // 캐릭터 선택 패널 비활성화
                if (characterSelectionPanel != null)
                {
                    characterSelectionPanel.SetActive(false);
                    LogDebug("Character selection panel deactivated");
                }
                
                // 던전 패널 활성화
                if (dungeonPanel != null)
                {
                    dungeonPanel.SetActive(true);
                    LogDebug("Dungeon panel activated");
                }
                else
                {
                    LogError("DungeonPanel reference is null");
                }
                
                // 이벤트 기반 UI 전환
                MonoChrome.Events.UIEvents.RequestPanelShow("DungeonPanel");
                LogDebug("Event-based panel switch executed");
            }
            catch (System.Exception ex)
            {
                LogError($"Error switching panels: {ex.Message}");
            }
            
            yield return new WaitForSeconds(delayBetweenSteps);
        }
        
        /// <summary>
        /// 3단계: 게임 상태 변경
        /// </summary>
        private IEnumerator Step3_ChangeGameState()
        {
            LogDebug("Step 3: Changing game state...");
            
            try
            {
                var gameStateMachine = MonoChrome.Core.GameStateMachine.Instance;
                if (gameStateMachine != null)
                {
                    gameStateMachine.TryChangeState(MonoChrome.Core.GameStateMachine.GameState.Dungeon);
                    LogDebug("Game state changed to Dungeon");
                }
                else
                {
                    LogError("GameStateMachine.Instance is null");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error changing game state: {ex.Message}");
            }
            
            yield return new WaitForSeconds(delayBetweenSteps);
        }
        
        /// <summary>
        /// 4단계: 던전 생성
        /// </summary>
        private IEnumerator Step4_GenerateDungeon()
        {
            LogDebug("Step 4: Generating dungeon...");
            bool dungeonGenerated = false;

            try
            {
                var dungeonController = FindObjectOfType<MonoChrome.DungeonController>();

                if (dungeonController != null)
                {
                    MonoChrome.Events.DungeonEvents.RequestDungeonGeneration(0);
                    LogDebug("Dungeon generation requested");
                    dungeonGenerated = true;
                }
                else
                {
                    LogError("DungeonController not found - creating test nodes manually");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error generating dungeon: {ex.Message}");
            }

            if (dungeonGenerated)
            {
                yield return new WaitForSeconds(1f);
                LogDebug("Dungeon generation completed");
            }

            yield return new WaitForSeconds(delayBetweenSteps);
        }

        
        /// <summary>
        /// 5단계: 테스트 노드 생성 (던전 생성이 실패할 경우)
        /// </summary>
        private IEnumerator Step5_CreateTestNodes()
        {
            LogDebug("Step 5: Creating test nodes...");
            
            try
            {
                // DungeonUI 찾기
                var dungeonUI = FindObjectOfType<DungeonUI>();
                if (dungeonUI != null)
                {
                    // 테스트용 노드 데이터 생성
                    var testNodes = CreateTestDungeonNodes();
                    
                    // DungeonUI 업데이트
                    dungeonUI.UpdateDungeonMap(testNodes, 0);
                    LogDebug($"Test nodes created and UI updated: {testNodes.Count} nodes");
                }
                else
                {
                    LogError("DungeonUI not found");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error creating test nodes: {ex.Message}");
            }
            
            yield return new WaitForSeconds(delayBetweenSteps);
        }
        
        /// <summary>
        /// 테스트용 던전 노드 생성
        /// </summary>
        private System.Collections.Generic.List<DungeonNode> CreateTestDungeonNodes()
        {
            var nodes = new System.Collections.Generic.List<DungeonNode>();
            
            // 시작 노드 (전투)
            var startNode = new DungeonNode(0, NodeType.Combat, new Vector2(0, 0));
            startNode.IsAccessible = true;
            nodes.Add(startNode);
            
            // 중간 노드들
            nodes.Add(new DungeonNode(1, NodeType.Shop, new Vector2(100, 50)));
            nodes.Add(new DungeonNode(2, NodeType.Event, new Vector2(100, -50)));
            nodes.Add(new DungeonNode(3, NodeType.Rest, new Vector2(200, 0)));
            nodes.Add(new DungeonNode(4, NodeType.MiniBoss, new Vector2(300, 0)));
            nodes.Add(new DungeonNode(5, NodeType.Boss, new Vector2(400, 0)));
            
            // 연결 설정
            startNode.ConnectedNodes.Add(1);
            startNode.ConnectedNodes.Add(2);
            
            nodes[1].ConnectedNodes.Add(3);
            nodes[2].ConnectedNodes.Add(3);
            nodes[3].ConnectedNodes.Add(4);
            nodes[4].ConnectedNodes.Add(5);
            
            LogDebug("Test dungeon nodes created with connections");
            return nodes;
        }
        
        /// <summary>
        /// 자동 게임 시작 (테스트용)
        /// </summary>
        private IEnumerator AutoStartGameAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            LogDebug("Auto-starting game...");
            OnStartGameClicked();
        }
        
        /// <summary>
        /// 노드 클릭 테스트를 위한 공개 메서드
        /// </summary>
        public void TestNodeClick(int nodeIndex)
        {
            LogDebug($"Testing node click: {nodeIndex}");
            
            try
            {
                // 전투 패널 활성화
                if (combatPanel != null)
                {
                    if (dungeonPanel != null) dungeonPanel.SetActive(false);
                    combatPanel.SetActive(true);
                    LogDebug("Switched to combat panel");
                }
                
                // 게임 상태를 전투로 변경
                var gameStateMachine = MonoChrome.Core.GameStateMachine.Instance;
                if (gameStateMachine != null)
                {
                    gameStateMachine.TryChangeState(MonoChrome.Core.GameStateMachine.GameState.Combat);
                    LogDebug("Game state changed to Combat");
                    
                    // 전투는 이벤트 기반 시스템에서 자동으로 처리됨
                    MonoChrome.Events.CombatEvents.RequestCombatStart("들개", CharacterType.Normal);
                    LogDebug("Combat initialization requested via events");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"Error in TestNodeClick: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[CompleteGameStarter] {message}");
            }
        }
        
        /// <summary>
        /// 에러 로그
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[CompleteGameStarter] {message}");
        }
        
        /// <summary>
        /// Inspector에서 호출할 수 있는 테스트 메서드들
        /// </summary>
        [ContextMenu("Test Start Game")]
        public void TestStartGame()
        {
            OnStartGameClicked();
        }
        
        [ContextMenu("Test Combat Transition")]
        public void TestCombatTransition()
        {
            TestNodeClick(0);
        }
        
        [ContextMenu("Reset Panels")]
        public void ResetPanels()
        {
            if (characterSelectionPanel != null) characterSelectionPanel.SetActive(true);
            if (dungeonPanel != null) dungeonPanel.SetActive(false);
            if (combatPanel != null) combatPanel.SetActive(false);
            LogDebug("Panels reset to initial state");
        }
    }
}