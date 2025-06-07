using System;
using UnityEngine;

namespace MonoChrome.Legacy
{
    /// <summary>
    /// 레거시 호환성을 위한 더미 CombatManager
    /// 실제 전투 로직은 CombatSystem에서 처리됨
    /// 포트폴리오 정리 시 제거 예정
    /// </summary>
    [System.Obsolete("This class is deprecated. Use CombatSystem instead.")]
    public class LegacyCombatManager : MonoBehaviour
    {
        [Header("레거시 호환성 유지용")]
        [SerializeField] private bool _showWarning = true;
        
        private void Awake()
        {
            if (_showWarning)
            {
                Debug.LogWarning($"[{name}] 레거시 CombatManager가 활성화되어 있습니다. CombatSystem 사용을 권장합니다.");
            }
            
            // 자동으로 비활성화
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 현재 전투 시스템으로 리다이렉트
        /// </summary>
        public void StartCombat() => Debug.LogWarning("Use CombatSystem.Instance instead");
        public void EndCombat() => Debug.LogWarning("Use CombatSystem.Instance instead");
    }
}