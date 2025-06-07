using UnityEngine;
using UnityEngine.UI;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// UI 버튼들의 이벤트 핸들러를 관리하는 클래스
    /// 브릿지 패턴과 호환되도록 수정됨
    /// </summary>
    public class ButtonHandlers : MonoBehaviour
    {
        [Header("캐릭터 선택 버튼들 (Inspector에서 할당)")]
        [SerializeField] private Button _auditoryCharButton;
        [SerializeField] private Button _olfactoryCharButton;
        [SerializeField] private Button _tactileCharButton;
        [SerializeField] private Button _spiritualCharButton;
        [SerializeField] private Button _startGameButton;

        [SerializeField] private GameObject _characterSelectionPanel;
        
        // 명시적으로 Core 네임스페이스의 SenseType 사용
        private MonoChrome.SenseType _selectedSenseType = MonoChrome.SenseType.Auditory; // 기본값
        
        private void Start()
        {
            SetupButtonEvents();
            Debug.Log("ButtonHandlers: Initialized and events wired");
        }

        private void SetupButtonEvents()
        {
            _auditoryCharButton?.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Auditory));
            _olfactoryCharButton?.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Olfactory));
            _tactileCharButton?.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Tactile));
            _spiritualCharButton?.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Spiritual));
            _startGameButton?.onClick.AddListener(OnStartGameButtonClicked);
        }
        
        private void OnStartGameButtonClicked()
        {
            Debug.Log("ButtonHandlers: Start Game Button clicked!");
            
            try
            {
                // MasterGameManager 참조 확인
                if (MasterGameManager.Instance == null)
                {
                    Debug.LogError("ButtonHandlers: MasterGameManager.Instance is null!");
                    return;
                }
                
                // 캐릭터 선택 패널 명시적 비활성화 (중요!)
                if (_characterSelectionPanel != null)
                {
                    _characterSelectionPanel.SetActive(false);
                    Debug.Log("ButtonHandlers: Character selection panel deactivated");
                }
                else
                {
                    // 패널 참조가 없으면 전체 화면에서 찾기
                    GameObject panel = GameObject.Find("CharacterSelectionPanel");
                    if (panel != null)
                    {
                        panel.SetActive(false);
                        Debug.Log("ButtonHandlers: Found and deactivated CharacterSelectionPanel by name");
                    }
                    
                    // UI Manager를 통한 패널 비활성화 시도
                    var uiManager = FindObjectOfType<CoreUIManager>();
                    if (uiManager != null)
                    {
                        uiManager.OnPanelSwitched("DungeonPanel");
                        Debug.Log("ButtonHandlers: Switched to DungeonPanel via UIManager");
                    }
                }
                
                Debug.Log($"ButtonHandlers: Requesting game start with {_selectedSenseType}");
                MasterGameManager.Instance.SelectCharacter(_selectedSenseType.ToString());
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ButtonHandlers: Error in OnStartGameButtonClicked: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // 참고: 씬 로드 후의 처리는 InitializeReferences.cs에서 수행함
        
        private void OnCharacterTypeButtonClicked(MonoChrome.SenseType senseType)
        {
            // 선택한 캐릭터 타입 저장
            _selectedSenseType = senseType;
            
            // UI 강조 효과
            HighlightSelectedCharacterButton(senseType);
            
            Debug.Log($"ButtonHandlers: Selected character type: {senseType}");
        }
        
        private void HighlightSelectedCharacterButton(MonoChrome.SenseType senseType)
        {
            try
            {
                // 모든 버튼 강조 제거
                if (_auditoryCharButton != null && _auditoryCharButton.GetComponent<Image>() != null) 
                    _auditoryCharButton.GetComponent<Image>().color = Color.white;
                    
                if (_olfactoryCharButton != null && _olfactoryCharButton.GetComponent<Image>() != null) 
                    _olfactoryCharButton.GetComponent<Image>().color = Color.white;
                    
                if (_tactileCharButton != null && _tactileCharButton.GetComponent<Image>() != null) 
                    _tactileCharButton.GetComponent<Image>().color = Color.white;
                    
                if (_spiritualCharButton != null && _spiritualCharButton.GetComponent<Image>() != null) 
                    _spiritualCharButton.GetComponent<Image>().color = Color.white;
                
                // 선택된 버튼 강조
                Color highlightColor = new Color(0.8f, 0.8f, 1f); // 밝은 파란색
                
                switch(senseType)
                {
                    case MonoChrome.SenseType.Auditory:
                        if (_auditoryCharButton != null && _auditoryCharButton.GetComponent<Image>() != null) 
                            _auditoryCharButton.GetComponent<Image>().color = highlightColor;
                        break;
                    case MonoChrome.SenseType.Olfactory:
                        if (_olfactoryCharButton != null && _olfactoryCharButton.GetComponent<Image>() != null) 
                            _olfactoryCharButton.GetComponent<Image>().color = highlightColor;
                        break;
                    case MonoChrome.SenseType.Tactile:
                        if (_tactileCharButton != null && _tactileCharButton.GetComponent<Image>() != null) 
                            _tactileCharButton.GetComponent<Image>().color = highlightColor;
                        break;
                    case MonoChrome.SenseType.Spiritual:
                        if (_spiritualCharButton != null && _spiritualCharButton.GetComponent<Image>() != null) 
                            _spiritualCharButton.GetComponent<Image>().color = highlightColor;
                        break;
                }
                
                Debug.Log($"ButtonHandlers: Highlighted {senseType} character button");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"ButtonHandlers: Error in HighlightSelectedCharacterButton: {ex.Message}");
            }
        }
    }
}