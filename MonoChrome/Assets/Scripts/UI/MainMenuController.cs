using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MonoChrome
{
    /// <summary>
    /// 메인 메뉴 UI 및 기능을 제어하는 클래스
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _quitButton;
        
        private void Start()
        {
            Debug.Log("MainMenuController: Start() called!");
            InitializeUI();
            
            // GameManager가 없으면 생성
            if (GameManager.Instance == null)
            {
                Debug.LogWarning("GameManager not found, creating one...");
                _ = GameManager.Instance;
            }
        }
        
        private void InitializeUI()
        {
            // 버튼 참조 가져오기
            if (_startButton == null)
            {
                _startButton = GameObject.Find("StartButton")?.GetComponent<Button>();
            }
            
            if (_quitButton == null)
            {
                _quitButton = GameObject.Find("QuitButton")?.GetComponent<Button>();
            }
            
            // 버튼 이벤트 등록
            if (_startButton != null)
            {
                _startButton.onClick.AddListener(OnStartButtonClicked);
            }
            else
            {
                Debug.LogError("Start Button not found!");
            }
            
            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
            else
            {
                Debug.LogError("Quit Button not found!");
            }
        }
        
        /// <summary>
        /// 시작 버튼 클릭 처리
        /// </summary>
        private void OnStartButtonClicked()
        {
            Debug.Log("MainMenuController: Start button clicked, loading GameScene");
            
            // 시작 버튼 클릭시 바로 GameScene으로 이동
            // 이때 GameManager의 상태는 업데이트하지 않음 - 씬 로드 후 처리
            SceneManager.LoadScene("GameScene");
        }
        
        /// <summary>
        /// 종료 버튼 클릭 처리
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("Quit button clicked, exiting application...");
            
#if UNITY_EDITOR
            // 에디터에서는 플레이 모드 중지
            UnityEditor.EditorApplication.isPlaying = false;
#else
            // 빌드된 게임에서는 애플리케이션 종료
            Application.Quit();
#endif
        }
    }
}