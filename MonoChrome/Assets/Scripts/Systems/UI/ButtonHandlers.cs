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
    /// CharacterSelectionPanel에 직접 붙어서 자식 버튼들을 관리
    /// </summary>
    public class ButtonHandlers : MonoBehaviour
    {
        [Header("버튼 참조 (자동으로 찾아짐)")]
        [SerializeField] private Button _startGameButton;
        [SerializeField] private Button _auditoryCharButton;
        [SerializeField] private Button _olfactoryCharButton;
        [SerializeField] private Button _tactileCharButton;
        [SerializeField] private Button _spiritualCharButton;
        
        [Header("현재 선택된 캐릭터")]
        [SerializeField] private SenseType _selectedSenseType = SenseType.Auditory; // 기본값
        
        private bool _isInitialized = false;
        
        private void Start()
        {
            Debug.Log("[ButtonHandlers] 초기화 시작...");
            StartCoroutine(InitializeAfterFrame());
        }
        
        /// <summary>
        /// 한 프레임 후 초기화 (다른 시스템들의 초기화 완료 대기)
        /// </summary>
        private IEnumerator InitializeAfterFrame()
        {
            yield return null; // 한 프레임 대기
            
            FindButtons();
            SubscribeEvents();
            SetDefaultSelection();
            
            _isInitialized = true;
            Debug.Log("[ButtonHandlers] 초기화 완료!");
        }
        
        private void FindButtons()
        {
            Debug.Log("[ButtonHandlers] 버튼들을 찾는 중...");
            
            // CharacterSelectionPanel의 직접 자식들에서 버튼 찾기
            Transform myTransform = transform;
            
            // 각 버튼을 이름으로 찾기
            _startGameButton = FindChildButton("StartGameButton");
            _auditoryCharButton = FindChildButton("AuditoryCharButton");
            _olfactoryCharButton = FindChildButton("OlfactoryCharButton");
            _tactileCharButton = FindChildButton("TactileCharButton");
            _spiritualCharButton = FindChildButton("SpiritualCharButton");
            
            // 찾기 결과 로그
            Debug.Log($"[ButtonHandlers] StartGameButton: {(_startGameButton != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonHandlers] AuditoryCharButton: {(_auditoryCharButton != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonHandlers] OlfactoryCharButton: {(_olfactoryCharButton != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonHandlers] TactileCharButton: {(_tactileCharButton != null ? "✓" : "✗")}");
            Debug.Log($"[ButtonHandlers] SpiritualCharButton: {(_spiritualCharButton != null ? "✓" : "✗")}");
        }
        
        private Button FindChildButton(string buttonName)
        {
            Transform child = transform.Find(buttonName);
            if (child != null)
            {
                Button button = child.GetComponent<Button>();
                if (button != null)
                {
                    Debug.Log($"[ButtonHandlers] {buttonName} 버튼을 찾았습니다!");
                    return button;
                }
                else
                {
                    Debug.LogWarning($"[ButtonHandlers] {buttonName} 오브젝트는 있지만 Button 컴포넌트가 없습니다!");
                }
            }
            else
            {
                Debug.LogWarning($"[ButtonHandlers] {buttonName} 오브젝트를 찾을 수 없습니다!");
            }
            return null;
        }
        
        private void SubscribeEvents()
        {
            Debug.Log("[ButtonHandlers] 버튼 이벤트 구독 중...");
            
            // 기존 이벤트 제거 후 새로 등록
            if (_startGameButton != null)
            {
                _startGameButton.onClick.RemoveAllListeners();
                _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                Debug.Log("[ButtonHandlers] StartGameButton 이벤트 구독 완료");
            }
                
            if (_auditoryCharButton != null)
            {
                _auditoryCharButton.onClick.RemoveAllListeners();
                _auditoryCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(SenseType.Auditory));
                Debug.Log("[ButtonHandlers] AuditoryCharButton 이벤트 구독 완료");
            }
                
            if (_olfactoryCharButton != null)
            {
                _olfactoryCharButton.onClick.RemoveAllListeners();
                _olfactoryCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(SenseType.Olfactory));
                Debug.Log("[ButtonHandlers] OlfactoryCharButton 이벤트 구독 완료");
            }
                
            if (_tactileCharButton != null)
            {
                _tactileCharButton.onClick.RemoveAllListeners();
                _tactileCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(SenseType.Tactile));
                Debug.Log("[ButtonHandlers] TactileCharButton 이벤트 구독 완료");
            }
                
            if (_spiritualCharButton != null)
            {
                _spiritualCharButton.onClick.RemoveAllListeners();
                _spiritualCharButton.onClick.AddListener(() => OnCharacterTypeButtonClicked(SenseType.Spiritual));
                Debug.Log("[ButtonHandlers] SpiritualCharButton 이벤트 구독 완료");
            }
        }
        
        private void SetDefaultSelection()
        {
            // 기본값만 설정, UI 하이라이트는 사용자 선택 시에만 적용
            _selectedSenseType = SenseType.Auditory;
            Debug.Log("[ButtonHandlers] 기본값 설정: Auditory (하이라이트 없음, 사용자 선택 대기)");
        }
        
        private void OnStartGameButtonClicked()
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[ButtonHandlers] 아직 초기화가 완료되지 않았습니다!");
                return;
            }
            
            Debug.Log($"[ButtonHandlers] 게임 시작! 선택된 캐릭터: {_selectedSenseType}");
            
            try
            {
                // MasterGameManager를 통한 캐릭터 선택 및 게임 시작
                if (MasterGameManager.Instance != null)
                {
                    // 캐릭터 선택
                    string characterName = GetCharacterNameBySenseType(_selectedSenseType);
                    MasterGameManager.Instance.SelectCharacter(characterName);
                    
                    Debug.Log($"[ButtonHandlers] MasterGameManager.SelectCharacter() 호출 완료: {characterName}");
                }
                else
                {
                    Debug.LogError("[ButtonHandlers] MasterGameManager.Instance가 null입니다!");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonHandlers] 게임 시작 중 오류 발생: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private void OnCharacterTypeButtonClicked(SenseType senseType)
        {
            // 선택한 캐릭터 타입 저장
            _selectedSenseType = senseType;
            
            // UI 피드백
            HighlightSelectedCharacterButton(senseType);
            
            Debug.Log($"[ButtonHandlers] 캐릭터 선택됨: {senseType}");
        }
        
        private void HighlightSelectedCharacterButton(SenseType senseType)
        {
            try
            {
                // 모든 버튼 기본 색상으로 초기화
                ResetButtonColor(_auditoryCharButton);
                ResetButtonColor(_olfactoryCharButton);
                ResetButtonColor(_tactileCharButton);
                ResetButtonColor(_spiritualCharButton);
                
                // 선택된 버튼 강조
                Color highlightColor = new Color(0.8f, 0.8f, 1f, 1f); // 밝은 파란색
                
                Button selectedButton = senseType switch
                {
                    SenseType.Auditory => _auditoryCharButton,
                    SenseType.Olfactory => _olfactoryCharButton,
                    SenseType.Tactile => _tactileCharButton,
                    SenseType.Spiritual => _spiritualCharButton,
                    _ => _auditoryCharButton
                };
                
                if (selectedButton != null)
                {
                    Image buttonImage = selectedButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = highlightColor;
                        Debug.Log($"[ButtonHandlers] {senseType} 버튼이 강조되었습니다");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[ButtonHandlers] 버튼 강조 중 오류: {ex.Message}");
            }
        }
        
        private void ResetButtonColor(Button button)
        {
            if (button != null)
            {
                Image buttonImage = button.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = Color.white;
                }
            }
        }
        
        private string GetCharacterNameBySenseType(SenseType senseType)
        {
            return senseType switch
            {
                SenseType.Auditory => "김훈희",  // 청각 캐릭터
                SenseType.Olfactory => "신제우", // 후각 캐릭터  
                SenseType.Tactile => "곽장환",   // 촉각 캐릭터
                SenseType.Spiritual => "박재석", // 영적 캐릭터
                _ => "김훈희" // 기본값
            };
        }
        
        private void OnDestroy()
        {
            // 이벤트 정리
            _startGameButton?.onClick.RemoveAllListeners();
            _auditoryCharButton?.onClick.RemoveAllListeners();
            _olfactoryCharButton?.onClick.RemoveAllListeners();
            _tactileCharButton?.onClick.RemoveAllListeners();
            _spiritualCharButton?.onClick.RemoveAllListeners();
        }
    }
}