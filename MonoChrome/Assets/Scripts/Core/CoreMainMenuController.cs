using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace MonoChrome.Core
{
    /// <summary>
    /// 핵심 메인 메뉴 컨트롤러 - 단순하고 안정적인 메뉴 관리
    /// 포트폴리오용: 명확한 UI 구조와 안정적인 씬 전환
    /// </summary>
    public class CoreMainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _creditsButton;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _versionText;
        
        [Header("Settings")]
        [SerializeField] private string _gameVersion = "v1.0.0";
        [SerializeField] private bool _enableDebugMode = true;
        
        private bool _isInitialized = false;
        
        #region Initialization
        private void Start()
        {
            Debug.Log("CoreMainMenuController: Starting initialization");
            InitializeUI();
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // UI 참조 자동 찾기
            FindUIReferences();
            
            // 버튼 이벤트 설정
            SetupButtonEvents();
            
            // UI 텍스트 설정
            SetupUITexts();
            
            // 초기화 완료
            _isInitialized = true;
            
            Debug.Log("CoreMainMenuController: Initialization completed");
        }
        
        /// <summary>
        /// UI 참조 자동 찾기
        /// </summary>
        private void FindUIReferences()
        {
            // 버튼 찾기
            if (_startButton == null)
                _startButton = FindButtonByName("StartButton") ?? FindButtonByName("시작");
                
            if (_quitButton == null)
                _quitButton = FindButtonByName("QuitButton") ?? FindButtonByName("종료");
                
            if (_creditsButton == null)
                _creditsButton = FindButtonByName("CreditsButton") ?? FindButtonByName("크레딧");
            
            // 텍스트 찾기
            if (_titleText == null)
                _titleText = FindTextByName("TitleText") ?? FindTextByName("GameTitle");
                
            if (_versionText == null)
                _versionText = FindTextByName("VersionText") ?? FindTextByName("Version");
            
            LogFoundReferences();
        }
        
        /// <summary>
        /// 이름으로 버튼 찾기
        /// </summary>
        private Button FindButtonByName(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj?.GetComponent<Button>();
        }
        
        /// <summary>
        /// 이름으로 텍스트 찾기
        /// </summary>
        private TextMeshProUGUI FindTextByName(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj?.GetComponent<TextMeshProUGUI>();
        }
        
        /// <summary>
        /// 찾은 참조들 로그
        /// </summary>
        private void LogFoundReferences()
        {
            Debug.Log($"CoreMainMenuController: UI References Found:");
            Debug.Log($"  Start Button: {(_startButton != null ? "✓" : "✗")}");
            Debug.Log($"  Quit Button: {(_quitButton != null ? "✓" : "✗")}");
            Debug.Log($"  Credits Button: {(_creditsButton != null ? "✓" : "✗")}");
            Debug.Log($"  Title Text: {(_titleText != null ? "✓" : "✗")}");
            Debug.Log($"  Version Text: {(_versionText != null ? "✓" : "✗")}");
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtonEvents()
        {
            // 시작 버튼
            if (_startButton != null)
            {
                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(OnStartButtonClicked);
                Debug.Log("CoreMainMenuController: Start button event configured");
            }
            else
            {
                Debug.LogError("CoreMainMenuController: Start button not found!");
                CreateEmergencyStartButton();
            }
            
            // 종료 버튼
            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveAllListeners();
                _quitButton.onClick.AddListener(OnQuitButtonClicked);
                Debug.Log("CoreMainMenuController: Quit button event configured");
            }
            else
            {
                Debug.LogWarning("CoreMainMenuController: Quit button not found");
            }
            
            // 크레딧 버튼
            if (_creditsButton != null)
            {
                _creditsButton.onClick.RemoveAllListeners();
                _creditsButton.onClick.AddListener(OnCreditsButtonClicked);
                Debug.Log("CoreMainMenuController: Credits button event configured");
            }
        }
        
        /// <summary>
        /// 긴급 시작 버튼 생성 (UI가 없을 경우)
        /// </summary>
        private void CreateEmergencyStartButton()
        {
            Debug.Log("CoreMainMenuController: Creating emergency start button");
            
            // 캔버스 찾기
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("CoreMainMenuController: No Canvas found for emergency button");
                return;
            }
            
            // 버튼 생성
            GameObject buttonObj = new GameObject("EmergencyStartButton");
            buttonObj.transform.SetParent(canvas.transform, false);
            
            // RectTransform 설정
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(200, 50);
            
            // Button 컴포넌트 추가
            Button button = buttonObj.AddComponent<Button>();
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;
            button.targetGraphic = image;
            
            // 텍스트 추가
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "게임 시작";
            text.fontSize = 18;
            text.color = Color.black;
            text.alignment = TextAlignmentOptions.Center;
            
            // 이벤트 연결
            button.onClick.AddListener(OnStartButtonClicked);
            _startButton = button;
            
            Debug.Log("CoreMainMenuController: Emergency start button created");
        }
        
        /// <summary>
        /// UI 텍스트 설정
        /// </summary>
        private void SetupUITexts()
        {
            if (_titleText != null)
            {
                _titleText.text = "MONOCHROME: the Eclipse";
            }
            
            if (_versionText != null)
            {
                _versionText.text = _gameVersion;
            }
        }
        #endregion
        
        #region Button Event Handlers
        /// <summary>
        /// 시작 버튼 클릭 처리
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("CoreMainMenuController: Start button clicked");
            
            if (!_isInitialized)
            {
                Debug.LogWarning("CoreMainMenuController: Not initialized, but proceeding anyway");
            }
            
            // 버튼 비활성화 (중복 클릭 방지)
            if (_startButton != null)
                _startButton.interactable = false;
            
            // CoreGameManager를 통한 게임 시작
            if (CoreGameManager.Instance != null)
            {
                CoreGameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogWarning("CoreMainMenuController: CoreGameManager not found, loading scene directly");
                SceneManager.LoadScene("GameScene");
            }
        }
        
        /// <summary>
        /// 종료 버튼 클릭 처리
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("CoreMainMenuController: Quit button clicked");
            
            // 애플리케이션 종료
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("CoreMainMenuController: Stopping play mode in editor");
#else
            Application.Quit();
            Debug.Log("CoreMainMenuController: Quitting application");
#endif
        }
        
        /// <summary>
        /// 크레딧 버튼 클릭 처리
        /// </summary>
        private void OnCreditsButtonClicked()
        {
            Debug.Log("CoreMainMenuController: Credits button clicked");
            
            // 크레딧 표시 (간단한 로그로 대체)
            string credits = @"
MONOCHROME: the Eclipse
개발자: [개발자명]
음악: [음악가명]
아트: [아티스트명]
특별히 감사합니다: Unity Technologies
";
            Debug.Log($"Credits:\n{credits}");
            
            // 실제 게임에서는 크레딧 UI를 표시하거나 새 씬으로 이동
        }
        #endregion
        
        #region Debug & Utilities
        /// <summary>
        /// 키보드 단축키 처리
        /// </summary>
        private void Update()
        {
            if (!_enableDebugMode) return;
            
            // 디버그 키보드 단축키
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnStartButtonClicked();
            }
            
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnQuitButtonClicked();
            }
            
            // F1 키로 직접 GameScene 이동 (디버그용)
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("CoreMainMenuController: F1 pressed - Loading GameScene directly");
                SceneManager.LoadScene("GameScene");
            }
        }
        
        /// <summary>
        /// 디버그 GUI
        /// </summary>
        private void OnGUI()
        {
            if (!_enableDebugMode) return;
            
            // 디버그 정보 표시
            GUI.Box(new Rect(10, 10, 250, 100), "Main Menu Debug");
            GUI.Label(new Rect(15, 30, 240, 20), $"Initialized: {_isInitialized}");
            GUI.Label(new Rect(15, 50, 240, 20), $"CoreGameManager: {(CoreGameManager.Instance != null ? "✓" : "✗")}");
            GUI.Label(new Rect(15, 70, 240, 20), "Space: Start Game, Esc: Quit");
            GUI.Label(new Rect(15, 90, 240, 20), "F1: Direct to GameScene");
        }
        #endregion
    }
}