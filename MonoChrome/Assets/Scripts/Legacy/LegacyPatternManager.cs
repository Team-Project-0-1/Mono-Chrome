using System;
using UnityEngine;

namespace MonoChrome.Legacy
{
    /// <summary>
    /// 레거시 호환성을 위한 더미 PatternManager
    /// 실제 패턴 로직은 CombatSystem과 DataConnector에서 처리됨
    /// 포트폴리오 정리 시 제거 예정
    /// </summary>
    [System.Obsolete("This class is deprecated. Use CombatSystem and DataConnector instead.")]
    public class LegacyPatternManager : MonoBehaviour
    {
        [Header("레거시 호환성 유지용")]
        [SerializeField] private bool _showWarning = true;
        
        private void Awake()
        {
            if (_showWarning)
            {
                Debug.LogWarning($"[{name}] 레거시 PatternManager가 활성화되어 있습니다. DataConnector 사용을 권장합니다.");
            }
            
            // 자동으로 비활성화
            gameObject.SetActive(false);
        }
        
        /// <summary>
        /// 현재 패턴 시스템으로 리다이렉트
        /// </summary>
        public void GetAvailablePatterns() => Debug.LogWarning("Use DataConnector.Instance instead");
        public void ExecutePattern() => Debug.LogWarning("Use CombatSystem.Instance instead");
    }
}