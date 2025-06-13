using UnityEngine;
using UnityEngine.UI;
using MonoChrome.Core;

namespace MonoChrome
{
    /// <summary>
    /// 캐릭터 선택 UI 전용 스크립트
    /// 간단하고 직관적인 버튼 이벤트 처리
    /// </summary>
    public class CharacterSelectionUI : MonoBehaviour
    {
        [Header("선택된 캐릭터")]
        public SenseType selectedSenseType = SenseType.Auditory;
        
        private void Start()
        {
            Debug.Log("[CharacterSelectionUI] 초기화 시작");
            SetupButtonEvents();
            SetDefaultSelection();
            Debug.Log("[CharacterSelectionUI] 초기화 완료");
        }
        
        private void SetupButtonEvents()
        {
            // 각 버튼 찾아서 이벤트 연결
            SetupCharacterButton("AuditoryCharButton", SenseType.Auditory);
            SetupCharacterButton("OlfactoryCharButton", SenseType.Olfactory);
            SetupCharacterButton("TactileCharButton", SenseType.Tactile);
            SetupCharacterButton("SpiritualCharButton", SenseType.Spiritual);
            
            // 게임 시작 버튼
            Button startButton = transform.Find("StartGameButton")?.GetComponent<Button>();
            if (startButton != null)
            {
                startButton.onClick.RemoveAllListeners();
                startButton.onClick.AddListener(StartGame);
                Debug.Log("[CharacterSelectionUI] StartGameButton 이벤트 연결 완료");
            }
            else
            {
                Debug.LogError("[CharacterSelectionUI] StartGameButton을 찾을 수 없습니다!");
            }
        }
        
        private void SetupCharacterButton(string buttonName, SenseType senseType)
        {
            Button button = transform.Find(buttonName)?.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => SelectCharacter(senseType));
                Debug.Log($"[CharacterSelectionUI] {buttonName} 이벤트 연결 완료");
            }
            else
            {
                Debug.LogWarning($"[CharacterSelectionUI] {buttonName}을 찾을 수 없습니다!");
            }
        }
        
        public void SelectCharacter(SenseType senseType)
        {
            selectedSenseType = senseType;
            UpdateButtonHighlight();
            Debug.Log($"[CharacterSelectionUI] 캐릭터 선택됨: {senseType}");
        }
        
        public void StartGame()
        {
            Debug.Log($"[CharacterSelectionUI] 게임 시작! 선택된 캐릭터: {selectedSenseType}");
            
            try
            {
                if (MasterGameManager.Instance != null)
                {
                    string characterName = GetCharacterName(selectedSenseType);
                    MasterGameManager.Instance.SelectCharacter(characterName);
                    Debug.Log($"[CharacterSelectionUI] MasterGameManager.SelectCharacter() 호출: {characterName}");
                }
                else
                {
                    Debug.LogError("[CharacterSelectionUI] MasterGameManager.Instance가 null입니다!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CharacterSelectionUI] 오류: {ex.Message}");
            }
        }
        
        private void SetDefaultSelection()
        {
            // UI 기본 하이라이트만 설정 (실제 선택은 하지 않음)
            selectedSenseType = SenseType.Auditory;
            UpdateButtonHighlight();
            Debug.Log("[CharacterSelectionUI] 기본 UI 하이라이트 설정: Auditory (실제 선택 아님)");
        }
        
        private void UpdateButtonHighlight()
        {
            // 모든 버튼 색상 리셋
            ResetButtonColor("AuditoryCharButton");
            ResetButtonColor("OlfactoryCharButton");
            ResetButtonColor("TactileCharButton");
            ResetButtonColor("SpiritualCharButton");
            
            // 선택된 버튼 강조
            string selectedButtonName = selectedSenseType switch
            {
                SenseType.Auditory => "AuditoryCharButton",
                SenseType.Olfactory => "OlfactoryCharButton", 
                SenseType.Tactile => "TactileCharButton",
                SenseType.Spiritual => "SpiritualCharButton",
                _ => "AuditoryCharButton"
            };
            
            HighlightButton(selectedButtonName);
        }
        
        private void ResetButtonColor(string buttonName)
        {
            Button button = transform.Find(buttonName)?.GetComponent<Button>();
            if (button != null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Color.white;
                }
            }
        }
        
        private void HighlightButton(string buttonName)
        {
            Button button = transform.Find(buttonName)?.GetComponent<Button>();
            if (button != null)
            {
                Image image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = new Color(0.8f, 0.8f, 1f, 1f); // 밝은 파란색
                }
            }
        }
        
        private string GetCharacterName(SenseType senseType)
        {
            return senseType switch
            {
                SenseType.Auditory => "김훈희",
                SenseType.Olfactory => "신제우",
                SenseType.Tactile => "곽장환", 
                SenseType.Spiritual => "박재석",
                _ => "김훈희"
            };
        }
    }
}