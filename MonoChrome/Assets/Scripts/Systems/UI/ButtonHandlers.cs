using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// UI 버튼들의 이벤트 핸들러를 관리하는 클래스
    /// 브릿지 패턴과 호환되도록 수정됨
    /// </summary>
    public class ButtonHandlers : MonoBehaviour
    {
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _auditoryCharButton;
        [SerializeField] private Button _olfactoryCharButton;
        [SerializeField] private Button _tactileCharButton;
        [SerializeField] private Button _spiritualCharButton;
        
        // 캐릭터 선택 패널 참조
        [SerializeField] private GameObject _characterSelectionPanel;
        
        // 명시적으로 Core 네임스페이스의 SenseType 사용
        private MonoChrome.SenseType _selectedSenseType = MonoChrome.SenseType.Auditory; // 기본값
        
        private void Start()
        {
            FindButtons();
            SubscribeEvents();
            Debug.Log("ButtonHandlers: Initialized successfully with bridge pattern support");
        }
        
        private void FindButtons()
        {
            // 버튼 찾기
            if (_startGameButton == null)
                _startGameButton = transform.Find("StartGameButton")?.GetComponent<Button>();
                
            if (_auditoryCharButton == null)
                _auditoryCharButton = transform.Find("AuditoryCharButton")?.GetComponent<Button>();
                
            if (_olfactoryCharButton == null)
                _olfactoryCharButton = transform.Find("OlfactoryCharButton")?.GetComponent<Button>();
                
            if (_tactileCharButton == null)
                _tactileCharButton = transform.Find("TactileCharButton")?.GetComponent<Button>();
                
            if (_spiritualCharButton == null)
                _spiritualCharButton = transform.Find("SpiritualCharButton")?.GetComponent<Button>();
                
            // 캐릭터 선택 패널 찾기
            if (_characterSelectionPanel == null)
                _characterSelectionPanel = transform.Find("CharacterSelectionPanel")?.gameObject;
            
            // 버튼 찾기 결과 로그
            Debug.Log($"ButtonHandlers: Start Game Button found: {_startGameButton != null}");
            Debug.Log($"ButtonHandlers: Auditory Button found: {_auditoryCharButton != null}");
            Debug.Log($"ButtonHandlers: Olfactory Button found: {_olfactoryCharButton != null}");
            Debug.Log($"ButtonHandlers: Tactile Button found: {_tactileCharButton != null}");
            Debug.Log($"ButtonHandlers: Spiritual Button found: {_spiritualCharButton != null}");
            Debug.Log($"ButtonHandlers: Character Selection Panel found: {_characterSelectionPanel != null}");
        }
        
        private void SubscribeEvents()
        {
            // 이벤트 구독
            if (_startGameButton != null)
            {
                _startGameButton.onClick.RemoveAllListeners();
                _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                Debug.Log("ButtonHandlers: Start Game Button click event subscribed");
            }
                
            if (_auditoryCharButton != null)
            {
                _auditoryCharButton.onClick.RemoveAllListeners();
                _auditoryCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Auditory));
                Debug.Log("ButtonHandlers: Auditory Character Button click event subscribed");
            }
                
            if (_olfactoryCharButton != null)
            {
                _olfactoryCharButton.onClick.RemoveAllListeners();
                _olfactoryCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Olfactory));
                Debug.Log("ButtonHandlers: Olfactory Character Button click event subscribed");
            }
                
            if (_tactileCharButton != null)
            {
                _tactileCharButton.onClick.RemoveAllListeners();
                _tactileCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Tactile));
                Debug.Log("ButtonHandlers: Tactile Character Button click event subscribed");
            }
                
            if (_spiritualCharButton != null)
            {
                _spiritualCharButton.onClick.RemoveAllListeners();
                _spiritualCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(MonoChrome.SenseType.Spiritual));
                Debug.Log("ButtonHandlers: Spiritual Character Button click event subscribed");
            }
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
                
                // 캐릭터 생성
                Debug.Log($"ButtonHandlers: Creating player character with sense type: {_selectedSenseType}");
                CharacterManager.Instance.CreatePlayerCharacter(_selectedSenseType);
                
                // 던전 진입 - 브릿지 패턴을 통한 게임 시작
                Debug.Log("ButtonHandlers: Starting game by entering dungeon (using bridge pattern)");
                MasterGameManager.Instance.EnterDungeon();
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