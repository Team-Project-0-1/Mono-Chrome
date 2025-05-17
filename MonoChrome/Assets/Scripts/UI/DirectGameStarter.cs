using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MonoChrome
{
    /// <summary>
    /// 게임 시작 버튼을 직접 처리하는 단순한 핸들러
    /// </summary>
    public class DirectGameStarter : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        
        private void Start()
        {
            FindButton();
            SetupEventHandler();
            
            // 디버그 로그
            Debug.Log("DirectGameStarter: Initialized");
        }
        
        private void FindButton()
        {
            if (startGameButton == null)
            {
                startGameButton = GetComponent<Button>();
                
                if (startGameButton == null)
                {
                    // 부모 객체에서 찾기
                    startGameButton = transform.parent.GetComponentInChildren<Button>();
                    
                    if (startGameButton == null)
                    {
                        // 계층 구조에서 직접 찾기
                        startGameButton = GameObject.Find("StartGameButton")?.GetComponent<Button>();
                    }
                }
            }
            
            if (startGameButton == null)
            {
                Debug.LogError("DirectGameStarter: Failed to find Start Game Button!");
            }
            else
            {
                Debug.Log("DirectGameStarter: Start Game Button found successfully");
            }
        }
        
        private void SetupEventHandler()
        {
            if (startGameButton != null)
            {
                // 기존 이벤트 리스너 제거
                startGameButton.onClick.RemoveAllListeners();
                
                // 새 이벤트 리스너 추가
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                Debug.Log("DirectGameStarter: Start Game Button click event handler set up");
            }
        }
        
        private void OnStartGameButtonClicked()
        {
            Debug.Log("DirectGameStarter: Start Game Button clicked!");
    
            try
            {
                // 캐릭터 선택 창에서 시작 버튼을 눌렀을 때
                if (GameManager.Instance != null)
                {
                    Debug.Log("DirectGameStarter: Entering dungeon from character selection");
                    GameManager.Instance.EnterDungeon();
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"DirectGameStarter: Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 참고: 씬 로드 후의 처리는 InitializeReferences.cs에서 수행함
    }
}