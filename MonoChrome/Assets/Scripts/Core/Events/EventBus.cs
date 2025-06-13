using System;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome.Events;

namespace MonoChrome.Core
{
    /// <summary>
    /// 이벤트 버스 - 이벤트 시스템의 중앙 관리자
    /// 메모리 누수 방지와 이벤트 생명주기 관리 담당
    /// 성능 최적화: 구독자 수 제한, 약한 참조 사용
    /// </summary>
    public class EventBus : MonoBehaviour
    {
        #region Singleton Pattern
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
            
            // 정기적인 메모리 정리 시작
            InvokeRepeating(nameof(CleanupDeadReferences), 10f, 30f);
        }
        #endregion

        #region Memory Management
        // 약한 참조로 구독자들을 추적
        private readonly List<WeakReference> _subscribers = new();
        private readonly Dictionary<string, int> _eventCounts = new();
        
        // 성능 설정
        private const int MAX_SUBSCRIBERS_PER_EVENT = 50;
        private const int MAX_TOTAL_SUBSCRIBERS = 200;

        /// <summary>
        /// 구독자 등록 (메모리 누수 방지)
        /// </summary>
        public void RegisterSubscriber(object subscriber, string eventName)
        {
            if (subscriber == null) return;

            // 전체 구독자 수 제한
            if (_subscribers.Count >= MAX_TOTAL_SUBSCRIBERS)
            {
                Debug.LogWarning($"[EventBus] 최대 구독자 수 ({MAX_TOTAL_SUBSCRIBERS}) 초과. 새 구독 거부됨.");
                return;
            }

            // 이벤트별 구독자 수 제한
            if (!_eventCounts.ContainsKey(eventName))
                _eventCounts[eventName] = 0;

            if (_eventCounts[eventName] >= MAX_SUBSCRIBERS_PER_EVENT)
            {
                Debug.LogWarning($"[EventBus] 이벤트 '{eventName}'의 최대 구독자 수 ({MAX_SUBSCRIBERS_PER_EVENT}) 초과.");
                return;
            }

            _subscribers.Add(new WeakReference(subscriber));
            _eventCounts[eventName]++;
            
            LogDebug($"구독자 등록: {eventName} (총 {_eventCounts[eventName]}개)");
        }

        /// <summary>
        /// 죽은 참조 정리 (가비지 컬렉션된 객체들)
        /// </summary>
        private void CleanupDeadReferences()
        {
            int removedCount = 0;
            
            for (int i = _subscribers.Count - 1; i >= 0; i--)
            {
                if (!_subscribers[i].IsAlive)
                {
                    _subscribers.RemoveAt(i);
                    removedCount++;
                }
            }

            if (removedCount > 0)
            {
                LogDebug($"죽은 참조 {removedCount}개 정리됨. 남은 구독자: {_subscribers.Count}개");
            }
        }
        #endregion

        #region Event Management
        /// <summary>
        /// 모든 이벤트 리스너 정리 (씬 전환 시 호출)
        /// </summary>
        public void ClearAllEvents()
        {
            Debug.Log("[EventBus] ClearAllEvents() 호출됨 - 모든 이벤트 구독 해제 시작");
            
            // 각 이벤트 클래스의 Clear 메서드 호출
            DungeonEvents.ClearAllSubscriptions();
            DungeonEvents.CombatEvents.ClearAllSubscriptions();
            DungeonEvents.UIEvents.ClearAllSubscriptions();
            
            // 구독자 정보 초기화
            _subscribers.Clear();
            _eventCounts.Clear();
            
            Debug.Log("[EventBus] All events cleared - 모든 이벤트 구독 해제 완료");
        }

        /// <summary>
        /// 이벤트 시스템 상태 보고
        /// </summary>
        public void GenerateStatusReport()
        {
            LogDebug("=== EventBus 상태 보고 ===");
            LogDebug($"총 구독자 수: {_subscribers.Count}");
            LogDebug($"활성 이벤트 타입: {_eventCounts.Count}");
            
            foreach (var eventCount in _eventCounts)
            {
                LogDebug($"- {eventCount.Key}: {eventCount.Value}개 구독자");
            }
            
            LogDebug("========================");
        }
        #endregion

        #region Lifecycle
        private void OnDestroy()
        {
            CancelInvoke(); // 정기 정리 중단
            ClearAllEvents();
        }

        private void OnApplicationQuit()
        {
            ClearAllEvents();
        }
        #endregion

        #region Debug
        [SerializeField] private bool _enableDebugLogs = false;
        
        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[EventBus] {message}");
            }
        }
        #endregion
    }
}