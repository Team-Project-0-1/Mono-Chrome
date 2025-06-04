using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// CombatUI 설정을 위한 헬퍼 스크립트
    /// </summary>
    public class CombatUISetup : MonoBehaviour
    {
        [Header("Setup Components")]
        [SerializeField] private bool autoSetup = true;
        
        private void Start()
        {
            if (autoSetup)
            {
                SetupCombatUI();
            }
        }
        
        [ContextMenu("Setup Combat UI")]
        public void SetupCombatUI()
        {
            // CombatPanel 찾기
            GameObject combatPanel = GameObject.Find("CombatPanel");
            if (combatPanel == null)
            {
                // UI_Canvas 하위에서 찾기
                Canvas uiCanvas = FindObjectOfType<Canvas>();
                if (uiCanvas != null)
                {
                    combatPanel = uiCanvas.transform.Find("CombatPanel")?.gameObject;
                }
            }
            
            if (combatPanel == null)
            {
                Debug.LogError("CombatUISetup: CombatPanel not found!");
                return;
            }
            
            // CombatUI 컴포넌트 확인 및 추가
            CombatUI combatUI = combatPanel.GetComponent<CombatUI>();
            if (combatUI == null)
            {
                Debug.Log("CombatUISetup: Adding CombatUI component to CombatPanel");
                combatUI = combatPanel.AddComponent<CombatUI>();
            }
            
            // 체력바 설정
            SetupHealthBars(combatPanel);
            
            Debug.Log("CombatUISetup: Setup completed successfully!");
        }
        
        private void SetupHealthBars(GameObject combatPanel)
        {
            // PlayerHealthBar 설정
            Transform playerHealthBar = combatPanel.transform.Find("PlayerHealthBar");
            if (playerHealthBar != null)
            {
                Slider playerSlider = playerHealthBar.GetComponent<Slider>();
                if (playerSlider != null)
                {
                    playerSlider.minValue = 0;
                    playerSlider.maxValue = 100;
                    playerSlider.value = 100;
                    playerSlider.interactable = false;
                    Debug.Log("CombatUISetup: PlayerHealthBar configured");
                }
            }
            
            // EnemyHealthBar 설정
            Transform enemyHealthBar = combatPanel.transform.Find("EnemyHealthBar");
            if (enemyHealthBar != null)
            {
                Slider enemySlider = enemyHealthBar.GetComponent<Slider>();
                if (enemySlider != null)
                {
                    enemySlider.minValue = 0;
                    enemySlider.maxValue = 100;
                    enemySlider.value = 100;
                    enemySlider.interactable = false;
                    Debug.Log("CombatUISetup: EnemyHealthBar configured");
                }
            }
        }
    }
}