using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Core
{
    /// <summary>
    /// 서비스 로케이터 패턴 - 의존성 관리 및 시스템 간 연결
    /// 포트폴리오 품질: SOLID 원칙 적용과 테스트 가능한 구조
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();
        private static bool _isInitialized = false;

        /// <summary>
        /// 서비스 등록
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Overwriting...");
                _services[type] = service;
            }
            else
            {
                _services.Add(type, service);
                Debug.Log($"[ServiceLocator] Service {type.Name} registered successfully");
            }
        }

        /// <summary>
        /// 서비스 해제
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.ContainsKey(type))
            {
                _services.Remove(type);
                Debug.Log($"[ServiceLocator] Service {type.Name} unregistered");
            }
        }

        /// <summary>
        /// 서비스 조회
        /// </summary>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            if (_services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogWarning($"[ServiceLocator] Service {type.Name} not found");
            return null;
        }

        /// <summary>
        /// 서비스 존재 확인
        /// </summary>
        public static bool Has<T>() where T : class
        {
            return _services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 모든 서비스 초기화 (씬 전환 시 사용)
        /// </summary>
        public static void Clear()
        {
            Debug.Log("[ServiceLocator] Clearing all services");
            _services.Clear();
            _isInitialized = false;
        }

        /// <summary>
        /// 서비스 로케이터 초기화 상태
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 초기화 완료 표시
        /// </summary>
        public static void MarkAsInitialized()
        {
            _isInitialized = true;
            Debug.Log("[ServiceLocator] Initialization completed");
        }

        /// <summary>
        /// 등록된 서비스 목록 출력 (디버그용)
        /// </summary>
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void LogRegisteredServices()
        {
            Debug.Log("=== ServiceLocator Registered Services ===");
            foreach (var service in _services)
            {
                Debug.Log($"- {service.Key.Name}: {service.Value.GetType().Name}");
            }
            Debug.Log("==========================================");
        }
    }
}