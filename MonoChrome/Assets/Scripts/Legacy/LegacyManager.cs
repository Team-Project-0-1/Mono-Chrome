using UnityEngine;

namespace MonoChrome.Legacy
{
    /// <summary>
    /// 범용 레거시 매니저 - Missing Script 문제 해결용
    /// 포트폴리오 정리 시 제거 예정
    /// </summary>
    [System.Obsolete("Legacy compatibility only. Use new architecture instead.")]
    public class LegacyManager : MonoBehaviour
    {
        [Header("레거시 호환성 정보")]
        [SerializeField] private string originalManagerName = "";
        [SerializeField] private bool autoDisable = true;
        [SerializeField] private bool showWarning = true;
        
        private void Awake()
        {
            // 원래 매니저 이름이 비어있으면 GameObject 이름 사용
            if (string.IsNullOrEmpty(originalManagerName))
            {
                originalManagerName = gameObject.name;
            }
            
            if (showWarning)
            {
                Debug.LogWarning($"[{originalManagerName}] 레거시 매니저가 활성화되어 있습니다. " +
                    "새로운 아키텍처(MasterGameManager, CombatSystem 등) 사용을 권장합니다.");
            }
            
            // 자동 비활성화 옵션
            if (autoDisable)
            {
                gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 레거시 기능들 - 모두 경고 메시지만 출력
        /// </summary>
        public void StartCombat() => LogDeprecationWarning("StartCombat", "CombatSystem.Instance");
        public void EndCombat() => LogDeprecationWarning("EndCombat", "CombatSystem.Instance");
        public void FlipCoins() => LogDeprecationWarning("FlipCoins", "CombatSystem.Instance");
        public void GetAvailablePatterns() => LogDeprecationWarning("GetAvailablePatterns", "DataConnector.Instance");
        public void ApplyStatusEffect() => LogDeprecationWarning("ApplyStatusEffect", "StatusEffectManager.Instance");
        public void UpdateUI() => LogDeprecationWarning("UpdateUI", "UIController.Instance");
        
        private void LogDeprecationWarning(string methodName, string newSystem)
        {
            Debug.LogWarning($"[{originalManagerName}] {methodName}() is deprecated. Use {newSystem} instead.");
        }
        
        /// <summary>
        /// 에디터용 컨텍스트 메뉴
        /// </summary>
        [ContextMenu("Set Original Manager Name")]
        private void SetOriginalManagerName()
        {
            originalManagerName = gameObject.name;
            Debug.Log($"Original manager name set to: {originalManagerName}");
        }
        
        [ContextMenu("Show Migration Guide")]
        private void ShowMigrationGuide()
        {
            Debug.Log($"=== {originalManagerName} 마이그레이션 가이드 ===");
            
            switch (originalManagerName)
            {
                case "CombatManager":
                    Debug.Log("→ CombatSystem.Instance 사용");
                    Debug.Log("→ StartCombat() 대신 CombatEvents.RequestCombatStart() 사용");
                    break;
                    
                case "CoinManager":
                    Debug.Log("→ CombatSystem.Instance의 코인 메서드들 사용");
                    Debug.Log("→ FlipCoins(), SetCoinState() 등 직접 호출 가능");
                    break;
                    
                case "PatternManager":
                    Debug.Log("→ DataConnector.Instance.GetAvailablePatterns() 사용");
                    Debug.Log("→ CombatSystem.ExecutePlayerPattern() 사용");
                    break;
                    
                case "UIManager":
                    Debug.Log("→ UIController.Instance 사용");
                    Debug.Log("→ UIEvents를 통한 이벤트 기반 UI 업데이트");
                    break;
                    
                default:
                    Debug.Log("→ MasterGameManager와 새로운 아키텍처 확인 필요");
                    break;
            }
            
            Debug.Log("===============================");
        }
    }
}