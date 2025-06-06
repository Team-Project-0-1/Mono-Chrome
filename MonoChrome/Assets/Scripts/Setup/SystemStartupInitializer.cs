using UnityEngine;
using MonoChrome.Core;
using MonoChrome.Setup;

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
            if (_enableDebugLogs)
                Debug.Log("[SystemStartupInitializer] Running unified initialization");

            GameInitializer.Initialize(_createMasterGameManager, false);

            if (_destroyAfterInitialization)
            {
                Destroy(gameObject);
            }
        }

    }
}