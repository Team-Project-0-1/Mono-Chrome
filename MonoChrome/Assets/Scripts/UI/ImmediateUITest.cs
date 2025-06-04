using UnityEngine;
using System.Collections.Generic;
using MonoChrome.StatusEffects;

namespace MonoChrome
{
    public class ImmediateUITest : MonoBehaviour
    {
        private void Start()
        {
            Debug.Log("ImmediateUITest: Starting immediate UI test");
            Invoke("RunImmediateTest", 0.5f);
        }
        
        private void RunImmediateTest()
        {
            Debug.Log("ImmediateUITest: Running immediate test...");
            
            // CombatPanel 찾기
            GameObject combatPanel = GameObject.Find("CombatPanel");
            if (combatPanel == null)
            {
                Debug.LogError("ImmediateUITest: CombatPanel not found!");
                return;
            }
            
            Debug.Log("ImmediateUITest: CombatPanel found and active: " + combatPanel.activeSelf);
            
            // CombatUI 컴포넌트 찾기
            CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
            if (combatUI == null)
            {
                Debug.LogError("ImmediateUITest: CombatUI component not found!");
                return;
            }
            
            Debug.Log("ImmediateUITest: CombatUI component found, starting tests...");
            
            // 1. 체력바 테스트
            Debug.Log("ImmediateUITest: Testing health bars...");
            combatUI.UpdateHealthBars(75f, 100f, 30f, 50f);
            
            // 2. 턴 카운터 테스트
            Debug.Log("ImmediateUITest: Testing turn counter...");
            combatUI.UpdateTurnCounter(7);
            
            // 3. 동전 UI 테스트
            Debug.Log("ImmediateUITest: Testing coin UI...");
            List<bool> testCoins = new List<bool> { true, false, true, false, true };
            combatUI.UpdateCoinUI(testCoins);
            
            // 4. 액티브 스킬 버튼 테스트
            Debug.Log("ImmediateUITest: Testing active skill button...");
            combatUI.UpdateActiveSkillButton(true);
            
            // 5. 상태 효과 테스트
            Debug.Log("ImmediateUITest: Testing status effects...");
            List<StatusEffect> playerEffects = new List<StatusEffect>
            {
                new StatusEffect(StatusEffectType.Amplify, 5, 3),
                new StatusEffect(StatusEffectType.Resonance, 2, 1)
            };
            
            List<StatusEffect> enemyEffects = new List<StatusEffect>
            {
                new StatusEffect(StatusEffectType.Mark, 3, 2),
                new StatusEffect(StatusEffectType.Bleed, 4, 2)
            };
            
            combatUI.UpdateStatusEffectsUI(playerEffects, enemyEffects);
            
            Debug.Log("ImmediateUITest: All tests completed successfully!");
            Debug.Log("ImmediateUITest: Check the Game view to see the updated UI!");
        }
    }
}