using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// Handles main menu interactions including start, quit, and credits.
    /// Combines functionality from the previous CoreMainMenuController.
    /// </summary>
    public class MainMenuController : MonoBehaviour
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

        private void Start()
        {
            Debug.Log("MainMenuController: Starting initialization");
            InitializeUI();
        }

        /// <summary>
        /// Set up UI references and button events.
        /// </summary>
        private void InitializeUI()
        {
            FindUIReferences();
            SetupButtonEvents();
            SetupUITexts();
            _isInitialized = true;
            Debug.Log("MainMenuController: Initialization completed");
        }

        private void FindUIReferences()
        {
            if (_startButton == null)
                _startButton = FindButtonByName("StartButton") ?? FindButtonByName("시작");

            if (_quitButton == null)
                _quitButton = FindButtonByName("QuitButton") ?? FindButtonByName("종료");

            if (_creditsButton == null)
                _creditsButton = FindButtonByName("CreditsButton") ?? FindButtonByName("크레딧");

            if (_titleText == null)
                _titleText = FindTextByName("TitleText") ?? FindTextByName("GameTitle");

            if (_versionText == null)
                _versionText = FindTextByName("VersionText") ?? FindTextByName("Version");

            LogFoundReferences();
        }

        private Button FindButtonByName(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj ? obj.GetComponent<Button>() : null;
        }

        private TextMeshProUGUI FindTextByName(string name)
        {
            GameObject obj = GameObject.Find(name);
            return obj ? obj.GetComponent<TextMeshProUGUI>() : null;
        }

        private void LogFoundReferences()
        {
            Debug.Log("MainMenuController: UI References Found:");
            Debug.Log($"  Start Button: {(_startButton != null ? "✓" : "✗")}");
            Debug.Log($"  Quit Button: {(_quitButton != null ? "✓" : "✗")}");
            Debug.Log($"  Credits Button: {(_creditsButton != null ? "✓" : "✗")}");
            Debug.Log($"  Title Text: {(_titleText != null ? "✓" : "✗")}");
            Debug.Log($"  Version Text: {(_versionText != null ? "✓" : "✗")}");
        }

        private void SetupButtonEvents()
        {
            if (_startButton != null)
            {
                _startButton.onClick.RemoveAllListeners();
                _startButton.onClick.AddListener(OnStartButtonClicked);
                Debug.Log("MainMenuController: Start button event configured");
            }
            else
            {
                Debug.LogError("MainMenuController: Start button not found!");
                CreateEmergencyStartButton();
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveAllListeners();
                _quitButton.onClick.AddListener(OnQuitButtonClicked);
                Debug.Log("MainMenuController: Quit button event configured");
            }
            else
            {
                Debug.LogWarning("MainMenuController: Quit button not found");
            }

            if (_creditsButton != null)
            {
                _creditsButton.onClick.RemoveAllListeners();
                _creditsButton.onClick.AddListener(OnCreditsButtonClicked);
                Debug.Log("MainMenuController: Credits button event configured");
            }
        }

        private void CreateEmergencyStartButton()
        {
            Debug.Log("MainMenuController: Creating emergency start button");

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("MainMenuController: No Canvas found for emergency button");
                return;
            }

            GameObject buttonObj = new GameObject("EmergencyStartButton");
            buttonObj.transform.SetParent(canvas.transform, false);

            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.3f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.3f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(200, 50);

            Button button = buttonObj.AddComponent<Button>();
            Image image = buttonObj.AddComponent<Image>();
            image.color = Color.white;
            button.targetGraphic = image;

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

            button.onClick.AddListener(OnStartButtonClicked);
            _startButton = button;

            Debug.Log("MainMenuController: Emergency start button created");
        }

        private void SetupUITexts()
        {
            if (_titleText != null)
                _titleText.text = "MONOCHROME: the Eclipse";

            if (_versionText != null)
                _versionText.text = _gameVersion;
        }

        private void OnStartButtonClicked()
        {
            Debug.Log("MainMenuController: Start button clicked");

            if (!_isInitialized)
                Debug.LogWarning("MainMenuController: Not initialized, but proceeding anyway");

            if (_startButton != null)
                _startButton.interactable = false;

            if (MasterGameManager.Instance != null)
            {
                MasterGameManager.Instance.StartNewGame();
            }
            else
            {
                Debug.LogWarning("MainMenuController: MasterGameManager not found, loading scene directly");
                SceneManager.LoadScene("GameScene");
            }
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("MainMenuController: Quit button clicked");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            Debug.Log("MainMenuController: Stopping play mode in editor");
#else
            Application.Quit();
            Debug.Log("MainMenuController: Quitting application");
#endif
        }

        private void OnCreditsButtonClicked()
        {
            Debug.Log("MainMenuController: Credits button clicked");

            string credits = @"MONOCHROME: the Eclipse
개발자: [개발자명]
음악: [음악가명]
아트: [아티스트명]
특별히 감사합니다: Unity Technologies";
            Debug.Log($"Credits:\n{credits}");
        }

        private void Update()
        {
            if (!_enableDebugMode) return;

            if (Input.GetKeyDown(KeyCode.Space))
                OnStartButtonClicked();

            if (Input.GetKeyDown(KeyCode.Escape))
                OnQuitButtonClicked();

            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log("MainMenuController: F1 pressed - Loading GameScene directly");
                SceneManager.LoadScene("GameScene");
            }
        }

        private void OnGUI()
        {
            if (!_enableDebugMode) return;

            GUI.Box(new Rect(10, 10, 250, 100), "Main Menu Debug");
            GUI.Label(new Rect(15, 30, 240, 20), $"Initialized: {_isInitialized}");
            GUI.Label(new Rect(15, 50, 240, 20), $"MasterGameManager: {(MasterGameManager.Instance != null ? "✓" : "✗")}");
            GUI.Label(new Rect(15, 70, 240, 20), "Space: Start Game, Esc: Quit");
            GUI.Label(new Rect(15, 90, 240, 20), "F1: Direct to GameScene");
        }
    }
}
