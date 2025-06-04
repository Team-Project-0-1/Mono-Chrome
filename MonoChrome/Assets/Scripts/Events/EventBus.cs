using UnityEngine;
using MonoChrome.Events;

namespace MonoChrome.Core
{
    /// <summary>
    /// 이벤트 버스 - 이벤트 시스템의 중앙 관리자
    /// 메모리 누수 방지와 이벤트 생명주기 관리 담당
    /// </summary>
    public class EventBus : MonoBehaviour
    {
        private static EventBus _instance;
        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EventBus>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("[EventBus]");
                        _instance = go.AddComponent<EventBus>();
                        DontDestroyOnLoad(go);
                    }
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// 모든 이벤트 리스너 정리 (씬 전환 시 호출)
        /// </summary>
        public void ClearAllEvents()
        {
            // 각 이벤트 클래스의 Clear 메서드 호출
            DungeonEvents.ClearAllSubscriptions();
            CombatEvents.ClearAllSubscriptions();
            UIEvents.ClearAllSubscriptions();
            
            Debug.Log("[EventBus] All events cleared");
        }

        private void OnDestroy()
        {
            ClearAllEvents();
        }
    }
}