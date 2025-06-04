using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Setup
{
    /// <summary>
    /// 시스템 시작 초기화 - 모든 씬에서 MasterGameManager가 확실히 존재하도록 보장
    /// 이 컴포넌트는 씬의 빈 오브젝트에 추가하거나 자동으로 생성됨
    /// </summary>
    public class SystemStartupInitializer : MonoBehaviour
    {
        [Header("시작 설정")]
        [SerializeField] private bool _createMasterGameManager = true;
        [SerializeField] private bool _enableDebugLogs = true;
        [SerializeField] private bool _destroyAfterInitialization = false;

        private void Awake()
        {
            // 최우선으로 MasterGameManager 확보
            EnsureMasterGameManagerExists();
            
            // 기타 초기화 수행
            PerformAdditionalInitialization();
            
            // 임무 완료 후 자신을 파괴 (옵션)
            if (_destroyAfterInitialization)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// MasterGameManager 존재 보장
        /// </summary>
        private void EnsureMasterGameManagerExists()
        {
            if (!_createMasterGameManager) return;

            if (MasterGameManager.Instance == null)
            {
                LogDebug("MasterGameManager가 없어서 생성합니다.");
                
                // MasterGameManager를 강제로 생성
                GameObject go = new GameObject("[MasterGameManager]");
                go.AddComponent<MasterGameManager>();
                
                LogDebug("MasterGameManager 생성 완료");
            }
            else
            {
                LogDebug("MasterGameManager가 이미 존재합니다.");
            }
        }

        /// <summary>
        /// 추가 초기화 작업
        /// </summary>
        private void PerformAdditionalInitialization()
        {
            // 필요시 추가 초기화 로직 작성
            LogDebug("시스템 초기화 완료");
        }

        /// <summary>
        /// 수동으로 시스템 초기화 실행 (에디터용)
        /// </summary>
        [ContextMenu("Initialize Systems Now")]
        public void InitializeSystemsManually()
        {
            EnsureMasterGameManagerExists();
            PerformAdditionalInitialization();
            LogDebug("수동 시스템 초기화 완료");
        }

        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[SystemStartupInitializer] {message}");
            }
        }
    }
}