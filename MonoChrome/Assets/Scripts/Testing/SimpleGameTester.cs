using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Testing
{
    /// <summary>
    /// 간단한 게임 플로우 테스터
    /// 포트폴리오 시연용
    /// </summary>
    public class SimpleGameTester : MonoBehaviour
    {
        [Header("테스트 키 설정")]
        [SerializeField] private KeyCode testNewGameKey = KeyCode.F1;
        [SerializeField] private KeyCode testCharacterSelectKey = KeyCode.F2;
        [SerializeField] private KeyCode testCombatKey = KeyCode.F3;
        [SerializeField] private KeyCode testDungeonKey = KeyCode.F4;
        [SerializeField] private KeyCode testPanelFlowKey = KeyCode.F5;
        [SerializeField] private KeyCode testCombatFlowKey = KeyCode.F6;
        [SerializeField] private KeyCode testCombatUIKey = KeyCode.F7;
        
        private void Start()
        {
            Debug.Log("SimpleGameTester 초기화 완료");
            Debug.Log("F1: 새게임, F2: 캐릭터선택, F3: 전투, F4: 던전");
            Debug.Log("F5: 패널 플로우 테스트, F6: 전투 플로우 테스트, F7: 전투 UI 테스트");
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(testNewGameKey))
            {
                TestNewGame();
            }
            else if (Input.GetKeyDown(testCharacterSelectKey))
            {
                TestCharacterSelect();
            }
            else if (Input.GetKeyDown(testCombatKey))
            {
                TestCombat();
            }
            else if (Input.GetKeyDown(testDungeonKey))
            {
                TestDungeon();
            }
            else if (Input.GetKeyDown(testPanelFlowKey))
            {
                TestPanelFlow();
            }
            else if (Input.GetKeyDown(testCombatFlowKey))
            {
                TestCombatFlow();
            }
            else if (Input.GetKeyDown(testCombatUIKey))
            {
                TestCombatUI();
            }
        }
        
        private void TestNewGame()
        {
            Debug.Log("🎮 새 게임 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartNewGame();
            }
        }
        
        private void TestCharacterSelect()
        {
            Debug.Log("🎭 캐릭터 선택 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.SelectCharacter("김훈희");
            }
        }
        
        private void TestCombat()
        {
            Debug.Log("⚔️ 전투 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartCombat("약탈자1", CharacterType.Normal);
            }
        }
        
        private void TestDungeon()
        {
            Debug.Log("🏰 던전 테스트");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.EnterDungeon();
            }
        }
        
        private void TestPanelFlow()
        {
            Debug.Log("🖼️ 패널 플로우 테스트 시작");
            var stateMachine = GameStateMachine.Instance;
            if (stateMachine != null)
            {
                Debug.Log($"현재 상태: {stateMachine.CurrentState}");
                
                // 각 상태로 순차 전환 테스트
                StartCoroutine(TestPanelSequence());
            }
        }
        
        private void TestCombatFlow()
        {
            Debug.Log("⚔️ 전투 플로우 테스트 시작");
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                // 전투 상태로 전환 후 실제 전투 시작까지 테스트
                gameManager.StartCombat("루멘 리퍼", CharacterType.Normal);
                
                // 전투 시스템이 제대로 시작되는지 확인
                StartCoroutine(ValidateCombatStart());
            }
        }
        
        private System.Collections.IEnumerator TestPanelSequence()
        {
            var stateMachine = GameStateMachine.Instance;
            
            Debug.Log("📋 테스트: MainMenu → CharacterSelection");
            stateMachine.StartNewGame();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📋 테스트: CharacterSelection → Dungeon");
            stateMachine.EnterDungeon();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📋 테스트: Dungeon → Event");
            stateMachine.EnterEvent();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📋 테스트: Event → Shop");
            stateMachine.EnterShop();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📋 테스트: Shop → Rest");
            stateMachine.EnterRest();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("📋 테스트: Rest → Dungeon");
            stateMachine.EnterDungeon();
            yield return new WaitForSeconds(1f);
            
            Debug.Log("✅ 패널 플로우 테스트 완료");
        }
        
        private void TestCombatUI()
        {
            Debug.Log("🎨 전투 UI 테스트 시작");
            
            // 먼저 전투 시작
            var gameManager = MasterGameManager.Instance;
            if (gameManager != null)
            {
                gameManager.StartCombat("테스트 적", CharacterType.Normal);
                StartCoroutine(ValidateCombatUI());
            }
        }
        
        private System.Collections.IEnumerator ValidateCombatUI()
        {
            yield return new WaitForSeconds(1f); // 전투 초기화 대기
            
            // CombatPanel 확인
            var combatPanel = GameObject.Find("CombatUI");
            if (combatPanel != null)
            {
                Debug.Log("✅ CombatPanel 발견됨");
                
                var combatUI = combatPanel.GetComponent<CombatUI>();
                if (combatUI != null)
                {
                    Debug.Log("✅ CombatUI 컴포넌트 발견됨");
                    
                    // UI 요소들 확인
                    CheckUIElement(combatPanel, "PlayerHealthBar", "플레이어 체력바");
                    CheckUIElement(combatPanel, "EnemyHealthBar", "적 체력바");
                    CheckUIElement(combatPanel, "CoinArea", "동전 영역");
                    CheckUIElement(combatPanel, "PatternArea", "패턴 영역");
                    CheckUIElement(combatPanel, "ActiveSkillButton", "액티브 스킬 버튼");
                    CheckUIElement(combatPanel, "EndTurnButton", "턴 종료 버튼");
                    CheckUIElement(combatPanel, "TurnInfoText", "턴 정보 텍스트");
                    
                    Debug.Log("🎨 전투 UI 테스트 완료");
                }
                else
                {
                    Debug.LogError("❌ CombatUI 컴포넌트를 찾을 수 없음");
                }
            }
            else
            {
                Debug.LogError("❌ CombatPanel을 찾을 수 없음");
            }
        }
        
        private void CheckUIElement(GameObject parent, string elementName, string description)
        {
            var element = parent.transform.Find(elementName);
            if (element != null)
            {
                Debug.Log($"✅ {description} 발견됨: {elementName}");
            }
            else
            {
                Debug.LogWarning($"❌ {description} 누락됨: {elementName}");
            }
        }
        
        private System.Collections.IEnumerator ValidateCombatStart()
        {
            yield return new WaitForSeconds(0.5f);
            
            var combatSystem = FindFirstObjectByType<MonoChrome.Systems.Combat.CombatSystem>();
            if (combatSystem != null)
            {
                Debug.Log("✅ CombatSystem 발견됨");
                
                // 전투가 활성화되었는지 확인
                var stateMachine = GameStateMachine.Instance;
                if (stateMachine.CurrentState == GameStateMachine.GameState.Combat)
                {
                    Debug.Log("✅ 게임 상태가 Combat으로 변경됨");
                }
                else
                {
                    Debug.LogWarning($"❌ 게임 상태가 Combat이 아님: {stateMachine.CurrentState}");
                }
            }
            else
            {
                Debug.LogWarning("❌ CombatSystem을 찾을 수 없음");
            }
        }
    }
}
