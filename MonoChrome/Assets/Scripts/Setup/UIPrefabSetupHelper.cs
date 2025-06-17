using UnityEngine;

namespace MonoChrome.Setup
{
    /// <summary>
    /// UI 프리팹을 수동으로 생성하기 위한 헬퍼 컴포넌트
    /// </summary>
    public class UIPrefabSetupHelper : MonoBehaviour
    {
        [Header("UI 프리팹 생성 도구")]
        [Tooltip("이 컴포넌트는 UI 프리팹 생성을 도와주는 유틸리티입니다.")]
        public bool showInstructions = true;

        void Start()
        {
            if (showInstructions)
            {
                Debug.Log("=== MONOCHROME UI 프리팹 생성 가이드 ===");
                Debug.Log("1. Hierarchy에서 'PatternButtonPrefab' GameObject를 선택하세요");
                Debug.Log("2. Project 창에서 Assets/Resources/UI 폴더로 드래그하세요");
                Debug.Log("3. Hierarchy에서 'StatusEffectPrefab' GameObject를 선택하세요");
                Debug.Log("4. Project 창에서 Assets/Resources/UI 폴더로 드래그하세요");
                Debug.Log("5. 완료되면 이 GameObject를 삭제하세요");
                Debug.Log("=============================================");
            }
        }

        void OnValidate()
        {
            // Inspector에서 변경사항이 있을 때 실행
            if (Application.isPlaying && showInstructions)
            {
                CheckPrefabStatus();
            }
        }

        private void CheckPrefabStatus()
        {
            bool patternPrefabExists = Resources.Load("UI/PatternButtonPrefab") != null;
            bool statusPrefabExists = Resources.Load("UI/StatusEffectPrefab") != null;

            Debug.Log($"PatternButtonPrefab 존재: {patternPrefabExists}");
            Debug.Log($"StatusEffectPrefab 존재: {statusPrefabExists}");

            if (patternPrefabExists && statusPrefabExists)
            {
                Debug.Log("✅ 모든 UI 프리팹이 준비되었습니다!");
            }
        }
    }
}