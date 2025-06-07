using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR

namespace MonoChrome
{
    public class CombatUITester : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                ActivateCombatPanel();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestCombatUI();
            }
        }

        public void ActivateCombatPanel()
        {
            GameObject combatPanel = GameObject.Find("CombatPanel");
            if (combatPanel == null)
            {
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform combatPanelTransform = canvas.transform.Find("CombatPanel");
                    if (combatPanelTransform != null)
                    {
                        combatPanel = combatPanelTransform.gameObject;
                        break;
                    }
                }
            }

            if (combatPanel != null)
            {
                combatPanel.SetActive(true);
                Debug.Log("CombatUITester: CombatPanel activated!");

                CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
                if (combatUI != null)
                {
                    combatUI.InitializeCombatUI();
                    Debug.Log("CombatUITester: CombatUI initialized!");
                }
            }
        }

        public void TestCombatUI()
        {
            GameObject combatPanel = GameObject.Find("CombatPanel");
            if (combatPanel != null && combatPanel.activeSelf)
            {
                CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
                if (combatUI != null)
                {
                    combatUI.UpdateHealthBars(80f, 100f, 60f, 80f);
                    combatUI.UpdateTurnCounter(5);
                    Debug.Log("CombatUITester: UI test completed!");
                }
            }
        }
    }
}
#endif

