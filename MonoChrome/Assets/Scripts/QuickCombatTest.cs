using UnityEngine;
using System.Collections.Generic;

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
            
            // CombatUI 찾기 및 활성화
            GameObject combatPanel = FindCombatUI();
            if (combatPanel != null)
            {
                Debug.Log("QuickCombatTest: Found CombatUI, activating...");
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
            GameObject combatPanel = FindCombatUI();
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
        
        private GameObject FindCombatUI()
        {
            GameObject combatPanel = GameObject.Find("CombatUI");
            if (combatPanel == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform panelTransform = canvas.transform.Find("CombatUI");
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