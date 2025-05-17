using UnityEngine;
using System;
using System.Collections.Generic;

namespace MonoChrome
{
    /// <summary>
    /// 런타임 에러를 안전하게 처리하는 유틸리티 클래스
    /// </summary>
    public static class ErrorHandler
    {
        // 타입 변환 오류 방지
        public static T SafeConvert<T>(object value, T defaultValue)
        {
            try
            {
                return (T)value;
            }
            catch
            {
                Debug.LogWarning($"Failed to convert {value} to {typeof(T).Name}. Using default value.");
                return defaultValue;
            }
        }
        
        // 메서드 호출 오류 방지
        public static void SafeInvoke(Action action)
        {
            try
            {
                action?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error invoking action: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 속성 접근 오류 방지
        public static T SafeGetProperty<T>(Func<T> propertyGetter, T defaultValue)
        {
            try
            {
                return propertyGetter();
            }
            catch
            {
                Debug.LogWarning($"Failed to get property of type {typeof(T).Name}. Using default value.");
                return defaultValue;
            }
        }
        
        // 캐스팅 안전 처리
        public static T SafeCast<T>(object obj) where T : class
        {
            try
            {
                return obj as T;
            }
            catch
            {
                Debug.LogWarning($"Failed to cast object to {typeof(T).Name}.");
                return null;
            }
        }
        
        // 열거형 파싱 안전 처리
        public static T SafeParseEnum<T>(string value, T defaultValue) where T : struct
        {
            if (Enum.TryParse<T>(value, true, out T result))
            {
                return result;
            }
            else
            {
                Debug.LogWarning($"Failed to parse '{value}' to {typeof(T).Name}. Using default value.");
                return defaultValue;
            }
        }
    }
}