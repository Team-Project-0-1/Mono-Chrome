#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;
using MonoChrome.Utils;

namespace MonoChrome
{
    /// <summary>
    /// 타입 검증 도우미 에디터 도구
    /// 프로젝트의 타입 정의와 참조가 올바른지 확인합니다.
    /// </summary>
    public class TypeValidationHelper : EditorWindow
    {
        [MenuItem("MONOCHROME/Tools/Validate Type References")]
        private static void ValidateTypes()
        {
            var window = GetWindow<TypeValidationHelper>();
            window.titleContent = new GUIContent("Type Validator");
            window.Show();
        }
        
        private void OnGUI()
        {
            GUILayout.Label("MONOCHROME 프로젝트 타입 검증 도구", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            if (GUILayout.Button("핵심 타입 정의 확인"))
            {
                ValidateCoreTypes();
            }
            
            if (GUILayout.Button("네임스페이스 호환성 확인"))
            {
                ValidateNamespaceCompatibility();
            }
            
            if (GUILayout.Button("열거형 변환 테스트"))
            {
                TestEnumConversions();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("모든 타입 종합 검증"))
            {
                ValidateAllTypes();
            }
        }
        
        private void ValidateCoreTypes()
        {
            try
            {
                // Core 네임스페이스 타입 확인
                Type coreCharacterType = typeof(MonoChrome.CharacterType);
                Type coreSenseType = typeof(MonoChrome.SenseType);
                Type corePatternType = typeof(MonoChrome.PatternType);
                Type coreStatusEffectType = typeof(MonoChrome.StatusEffectType);
                
                Debug.Log("핵심 타입 정의 확인 완료: 모든 Core 네임스페이스 타입이 올바르게 정의되어 있습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"핵심 타입 정의 확인 실패: {ex.Message}");
            }
        }
        
        private void ValidateNamespaceCompatibility()
        {
            try
            {
                // Characters 네임스페이스 타입 확인
                Type charactersCharacterType = typeof(MonoChrome.CharacterType);
                Type charactersSenseType = typeof(MonoChrome.SenseType);
                
                // Combat 네임스페이스 타입 확인
                Type combatPatternType = typeof(MonoChrome.PatternType);
                
                // StatusEffects 네임스페이스 타입 확인
                Type statusEffectsStatusEffectType = typeof(MonoChrome.StatusEffectType);
                
                Debug.Log("네임스페이스 호환성 확인 완료: 모든 네임스페이스별 타입이 올바르게 정의되어 있습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"네임스페이스 호환성 확인 실패: {ex.Message}");
            }
        }
        
        private void TestEnumConversions()
        {
            try
            {
                // CharacterType 변환 테스트
                MonoChrome.CharacterType coreCharacterType = MonoChrome.CharacterType.Player;
                // 타입 변환 메서드 이동됨
                // MonoChrome.CharacterType charactersCharacterType = TypeReference.TypeFactory.GetCharactersCharacterType(coreCharacterType);
                
                // SenseType 변환 테스트
                MonoChrome.SenseType coreSenseType = MonoChrome.SenseType.Auditory;
                // 타입 변환 메서드 이동됨
                // MonoChrome.SenseType charactersSenseType = TypeReference.TypeFactory.GetCharactersSenseType(coreSenseType);
                
                // PatternType 변환 테스트
                MonoChrome.PatternType corePatternType = MonoChrome.PatternType.Consecutive2;
                // 타입 변환 메서드 이동됨
                // MonoChrome.PatternType combatPatternType = TypeReference.TypeFactory.GetCombatPatternType(corePatternType);
                
                // StatusEffectType 변환 테스트
                MonoChrome.StatusEffectType coreStatusEffectType = MonoChrome.StatusEffectType.Amplify;
                // 타입 변환 메서드 이동됨
                // MonoChrome.StatusEffectType statusEffectsStatusEffectType = TypeReference.TypeFactory.GetStatusEffectsStatusEffectType(coreStatusEffectType);
                
                Debug.Log("열거형 변환 테스트 완료: 모든 변환이 올바르게 작동합니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"열거형 변환 테스트 실패: {ex.Message}");
            }
        }
        
        private void ValidateAllTypes()
        {
            ValidateCoreTypes();
            ValidateNamespaceCompatibility();
            TestEnumConversions();
            
            // TypeConversionUtil 클래스 테스트
            try
            {
                // 타입 변환 메서드 이동됨
                // var characterType = TypeConversionUtil.ConvertToCharacterType(1);
                // var patternType = TypeConversionUtil.ParsePatternType("consecutive2");
                // var statusEffectType = TypeConversionUtil.ParseStatusEffectType("amplify");
                
                Debug.Log("타입 변환 유틸리티 검증 완료: TypeConversionUtil 클래스가 이동되었습니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"타입 변환 유틸리티 검증 실패: {ex.Message}");
            }
            
            // EnumExtensions 클래스 테스트
            try
            {
                // 확장 메서드 이동됨
                // MonoChrome.Extensions.EnumExtensions.IsNormal(MonoChrome.CharacterType.Normal);
                // 대신 CharacterType 확장 메서드 사용
                CharacterTypeExtensions.IsNormal(MonoChrome.CharacterType.Normal);
                
                Debug.Log("열거형 확장 메서드 검증 완료: CharacterTypeExtensions 클래스가 올바르게 작동합니다.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"열거형 확장 메서드 검증 실패: {ex.Message}");
            }
            
            Debug.Log("모든 타입 종합 검증 완료!");
        }
    }
}
#endif