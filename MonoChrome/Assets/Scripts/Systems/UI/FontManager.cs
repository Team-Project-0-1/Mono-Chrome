using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MonoChrome.Systems.UI
{
    /// <summary>
    /// 폰트 관리 및 한글 텍스트 렌더링을 위한 매니저 클래스
    /// </summary>
    public class FontManager : MonoBehaviour
    {
        // 싱글톤 인스턴스
        private static FontManager _instance;
        
        public static FontManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("FontManager");
                    _instance = go.AddComponent<FontManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        // 기본 한글 폰트
        [SerializeField] private Font koreanFont;
        [SerializeField] private TMP_FontAsset koreanTMPFont;
        
        // 현재 폰트 설정
        private bool _initialized = false;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 폰트 초기화
            InitializeFonts();
        }
        
        /// <summary>
        /// 폰트 초기화
        /// </summary>
        private void InitializeFonts()
        {
            if (_initialized)
                return;
                
            // 폰트 로드
            LoadFonts();
            
            // 기존 텍스트 컴포넌트에 폰트 적용
            StartCoroutine(ApplyFontsToAllTexts());
            
            _initialized = true;
            Debug.Log("FontManager: Fonts initialized");
        }
        
        /// <summary>
        /// 폰트 로드
        /// </summary>
        private void LoadFonts()
        {
            // 리소스에서 폰트 로드
            if (koreanFont == null)
            {
                koreanFont = Resources.Load<Font>("Fonts/NanumGothic");
                if (koreanFont == null)
                {
                    Debug.LogWarning("FontManager: Korean font not found in Resources/Fonts/");
                }
                else
                {
                    Debug.Log("FontManager: Korean font loaded");
                }
            }
            
            // TMP 폰트 에셋 로드
            if (koreanTMPFont == null)
            {
                koreanTMPFont = Resources.Load<TMP_FontAsset>("Fonts/NanumGothic SDF");
                if (koreanTMPFont == null)
                {
                    Debug.LogWarning("FontManager: Korean TMP font not found in Resources/Fonts/");
                }
                else
                {
                    Debug.Log("FontManager: Korean TMP font asset loaded");
                }
            }
        }
        
        /// <summary>
        /// 씬의 모든 텍스트 컴포넌트에 폰트 적용
        /// </summary>
        private IEnumerator ApplyFontsToAllTexts()
        {
            // UI Text 컴포넌트 검색 및 폰트 적용
            Text[] texts = FindObjectsOfType<Text>(true); // 비활성화된 텍스트도 포함
            foreach (Text text in texts)
            {
                if (koreanFont != null)
                {
                    text.font = koreanFont;
                    Debug.Log($"FontManager: Applied Korean font to Text component on {text.gameObject.name}");
                }
            }
            
            yield return null;
            
            // TextMeshPro 컴포넌트 검색 및 폰트 적용
            TextMeshProUGUI[] tmpTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (TextMeshProUGUI tmpText in tmpTexts)
            {
                if (koreanTMPFont != null)
                {
                    tmpText.font = koreanTMPFont;
                    Debug.Log($"FontManager: Applied Korean TMP font to TextMeshProUGUI component on {tmpText.gameObject.name}");
                }
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 특정 텍스트 컴포넌트에 한글 폰트 적용
        /// </summary>
        public void ApplyKoreanFont(Text text)
        {
            if (text != null && koreanFont != null)
            {
                text.font = koreanFont;
            }
        }
        
        /// <summary>
        /// 특정 TextMeshPro 컴포넌트에 한글 폰트 적용
        /// </summary>
        public void ApplyKoreanFont(TextMeshProUGUI text)
        {
            if (text != null && koreanTMPFont != null)
            {
                text.font = koreanTMPFont;
            }
        }
    }
}
