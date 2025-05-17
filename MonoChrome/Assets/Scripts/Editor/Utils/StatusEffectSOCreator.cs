#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace MonoChrome
{
    /// <summary>
    /// 기본 상태 효과 ScriptableObject 생성 편의 도구
    /// </summary>
    public class StatusEffectSOCreator : EditorWindow
    {
        private string baseFolderPath = "Assets/Resources/StatusEffects";
        
        [MenuItem("Tools/MonoChrome/Create Status Effect ScriptableObjects")]
        public static void ShowWindow()
        {
            GetWindow<StatusEffectSOCreator>("상태 효과 ScriptableObject 생성기");
        }
        
        private void OnGUI()
        {
            GUILayout.Label("상태 효과 ScriptableObject 생성 도구", EditorStyles.boldLabel);
            
            EditorGUILayout.Space();
            
            baseFolderPath = EditorGUILayout.TextField("상태 효과 저장 경로", baseFolderPath);
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("리소스 폴더 생성"))
            {
                CreateResourceFolder();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("기본 상태 효과 생성"))
            {
                CreateBasicStatusEffects();
            }
            
            EditorGUILayout.Space();
            
            GUILayout.Label("특수 상태 효과 생성", EditorStyles.boldLabel);
            
            if (GUILayout.Button("캐릭터별 특수 상태 효과 생성"))
            {
                CreateCharacterSpecificStatusEffects();
            }
        }
        
        private void CreateResourceFolder()
        {
            System.IO.Directory.CreateDirectory(baseFolderPath);
            AssetDatabase.Refresh();
            
            Debug.Log("상태 효과 리소스 폴더가 생성되었습니다.");
        }
        
        private void CreateBasicStatusEffects()
        {
            // 증폭 (공격력/방어력 증가)
            CreateStatusEffectSO(
                "Amplify", 
                "증폭", 
                "공격력과 방어력을 증가시킵니다.",
                StatusEffectType.Amplify, 
                Color.cyan, 
                1, 2, 
                true, true, false);
            
            // 공명 (축적 후 폭발)
            CreateStatusEffectSO(
                "Resonance", 
                "공명", 
                "지정된 턴 후에 누적된 피해를 입힙니다.",
                StatusEffectType.Resonance,
                Color.blue, 
                1, 2, 
                true, false, false, 
                "2턴 후 누적된 공명 수치만큼 즉시 피해를 입힙니다.");
            
            // 표식 (추가 공격)
            CreateStatusEffectSO(
                "Mark", 
                "표식", 
                "대상에게 표식을 남겨 추가 공격을 유발합니다.",
                StatusEffectType.Mark, 
                Color.yellow, 
                2, 2, 
                true, true, false);
            
            // 출혈 (방어력 무시 피해)
            CreateStatusEffectSO(
                "Bleed", 
                "출혈", 
                "매 턴마다 방어력을 무시하는 피해를 입힙니다.",
                StatusEffectType.Bleed, 
                Color.red, 
                2, 2, 
                true, true, true);
            
            // 반격 (피격 시 피해 반환)
            CreateStatusEffectSO(
                "Counter", 
                "반격", 
                "피격 시 공격자에게 피해를 되돌려줍니다.",
                StatusEffectType.Counter, 
                Color.magenta, 
                1, 2, 
                true, true, true);
            
            // 분쇄 (방어력 감소)
            CreateStatusEffectSO(
                "Crush", 
                "분쇄", 
                "대상의 방어력을 감소시킵니다.",
                StatusEffectType.Crush, 
                new Color(0.5f, 0.3f, 0.1f), 
                1, 2, 
                true, true, false);
            
            // 저주 (디버프 효과 증가)
            CreateStatusEffectSO(
                "Curse", 
                "저주", 
                "매 턴마다 고정 피해를 입히고 다른 디버프 효과가 강화됩니다.",
                StatusEffectType.Curse, 
                new Color(0.5f, 0, 0.5f), 
                2, 3, 
                true, true, true);
            
            // 봉인 (동전 봉인)
            CreateStatusEffectSO(
                "Seal", 
                "봉인", 
                "대상의 동전을 봉인하여 사용할 수 없게 만듭니다.",
                StatusEffectType.Seal, 
                new Color(0.3f, 0, 0.3f), 
                1, 1, 
                false, false, false,
                "봉인된 동전은 패턴 판정에 포함되지 않습니다.");
            
            // 중독 (지속 피해)
            CreateStatusEffectSO(
                "Poison", 
                "중독", 
                "매 턴마다 지속적인 피해를 입힙니다.",
                StatusEffectType.Poison, 
                Color.green, 
                2, 3, 
                true, true, false);
            
            // 화상 (즉시 피해)
            CreateStatusEffectSO(
                "Burn", 
                "화상", 
                "즉시 피해를 입히고 사라집니다.",
                StatusEffectType.Burn, 
                new Color(1, 0.5f, 0), 
                3, 1, 
                false, false, false);
            
            // 연격 (연속 공격)
            CreateStatusEffectSO(
                "MultiAttack", 
                "연격", 
                "수치만큼 연속 공격을 가합니다.",
                StatusEffectType.MultiAttack, 
                new Color(1, 0.7f, 0.2f), 
                2, 1, 
                false, false, false);
            
            Debug.Log("기본 상태 효과가 생성되었습니다.");
        }
        
        private void CreateCharacterSpecificStatusEffects()
        {
            // 청각 캐릭터 특수 효과
            CreateStatusEffectSO(
                "AmplifyResonance", 
                "증폭 공명", 
                "증폭 수치에 비례하여 공명 효과가 강화됩니다.",
                StatusEffectType.Amplify, 
                new Color(0, 0.8f, 1), 
                2, 2, 
                true, true, false,
                "증폭 수치가 높을수록 공명 효과가 더 강력해집니다.");
            
            // 후각 캐릭터 특수 효과
            CreateStatusEffectSO(
                "DeepBleed", 
                "심층 출혈", 
                "표식에 비례하여 출혈 효과가 강화됩니다.",
                StatusEffectType.Bleed, 
                new Color(0.8f, 0.1f, 0.1f), 
                3, 3, 
                true, true, true,
                "표식 수치가 높을수록 출혈 피해가 증가합니다.");
            
            // 촉각 캐릭터 특수 효과
            CreateStatusEffectSO(
                "CounterStrike", 
                "반격 타격", 
                "반격 성공 시 분쇄 효과가 함께 적용됩니다.",
                StatusEffectType.Counter, 
                new Color(0.8f, 0.3f, 0.8f), 
                2, 2, 
                true, true, true,
                "반격으로 피해를 입힐 때마다 분쇄 효과를 1 부여합니다.");
            
            // 영적 캐릭터 특수 효과
            CreateStatusEffectSO(
                "CursedSeal", 
                "저주 봉인", 
                "저주 상태일 때 봉인 효과가 강화됩니다.",
                StatusEffectType.Seal, 
                new Color(0.4f, 0.1f, 0.4f), 
                2, 2, 
                true, false, false,
                "저주 수치에 비례하여 봉인되는 동전 수가 증가합니다.");
            
            Debug.Log("캐릭터별 특수 상태 효과가 생성되었습니다.");
        }
        
        private StatusEffectSO CreateStatusEffectSO(string assetName, string effectName, string description,
            StatusEffectType effectType, Color color, int defaultMagnitude, int defaultDuration,
            bool stackable, bool refreshDuration, bool ignoreDefense, 
            string specialEffectDescription = null)
        {
            string fullPath = System.IO.Path.Combine(baseFolderPath, $"{assetName}.asset");
            
            // 에셋 생성
            StatusEffectSO statusEffectSO = ScriptableObject.CreateInstance<StatusEffectSO>();
            
            // 기본 정보 설정
            statusEffectSO.effectName = effectName;
            statusEffectSO.description = description;
            statusEffectSO.effectType = effectType;
            
            // 시각 효과 설정
            statusEffectSO.effectColor = color;
            
            // 기본값 설정
            statusEffectSO.defaultMagnitude = defaultMagnitude;
            statusEffectSO.defaultDuration = defaultDuration;
            
            // 효과 동작 설정
            statusEffectSO.stackable = stackable;
            statusEffectSO.refreshDuration = refreshDuration;
            statusEffectSO.ignoreDefense = ignoreDefense;
            
            // 특수 효과 설정
            if (!string.IsNullOrEmpty(specialEffectDescription))
            {
                statusEffectSO.specialEffectDescription = specialEffectDescription;
            }
            
            // 에셋 저장
            AssetDatabase.CreateAsset(statusEffectSO, fullPath);
            AssetDatabase.SaveAssets();
            
            Debug.Log($"상태 효과 에셋 생성: {fullPath}");
            
            return statusEffectSO;
        }
    }
}
#endif