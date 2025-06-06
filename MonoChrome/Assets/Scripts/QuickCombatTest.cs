using UnityEngine;
using System.Collections.Generic;
using MonoChrome.Combat;

namespace MonoChrome
{
    public class QuickCombatTest : MonoBehaviour
    {
        private void Start()
        {
            // 1초 후 테스트 실행
            Invoke("RunTest", 1f);
        }
        
        public void RunTest()
        {
            Debug.Log("QuickCombatTest: Starting combat UI test");
            
            // CombatPanel 찾기 및 활성화
            GameObject combatPanel = FindCombatPanel();
            if (combatPanel != null)
            {
                Debug.Log("QuickCombatTest: Found CombatPanel, activating...");
                combatPanel.SetActive(true);
                
                // CombatUI 컴포넌트 가져오기
                CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
                if (combatUI != null)
                {
                    Debug.Log("QuickCombatTest: Found CombatUI, initializing...");
                    combatUI.InitializeCombatUI();
                    
                    // 2초 후 UI 테스트
                    Invoke("TestUI", 2f);
                }
            }
        }
        
        private void TestUI()
        {
            GameObject combatPanel = FindCombatPanel();
            if (combatPanel != null && combatPanel.activeSelf)
            {
                CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
                if (combatUI != null)
                {
                    Debug.Log("QuickCombatTest: Testing health bars...");
                    combatUI.UpdateHealthBars(80f, 100f, 45f, 60f);
                    
                    Debug.Log("QuickCombatTest: Testing turn counter...");
                    combatUI.UpdateTurnCounter(3);
                    
                    Debug.Log("QuickCombatTest: Testing coin UI...");
                    List<bool> testCoins = new List<bool> { true, false, true, true, false };
                    combatUI.UpdateCoinUI(testCoins);
                    
                    Debug.Log("QuickCombatTest: Combat UI test completed!");
                }
            }
        }
        
        private GameObject FindCombatPanel()
        {
            GameObject combatPanel = GameObject.Find("CombatPanel");
            if (combatPanel == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform panelTransform = canvas.transform.Find("CombatPanel");
                    if (panelTransform != null)
                    {
                        combatPanel = panelTransform.gameObject;
                        break;
                    }
                }
            }
            return combatPanel;
        }
    }
}