using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 개별 동전 UI 컴포넌트
    /// Prefab에 붙여서 사용하는 전용 컴포넌트
    /// </summary>
    public class CoinUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image coinImage;
        [SerializeField] private Text coinText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Visual Settings")]
        [SerializeField] private Color headsColor = Color.red;
        [SerializeField] private Color tailsColor = Color.blue;
        [SerializeField] private Color selectedColor = Color.yellow;
        
        private bool _isHeads;
        private int _coinIndex;
        private bool _isSelected;
        
        private void Awake()
        {
            ValidateReferences();
        }
        
        /// <summary>
        /// 참조 검증
        /// </summary>
        private void ValidateReferences()
        {
            if (coinImage == null)
                coinImage = GetComponent<Image>();
            
            if (coinText == null)
                coinText = GetComponentInChildren<Text>();
                
            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }
        
        /// <summary>
        /// 동전 상태 설정
        /// </summary>
        public void SetCoinState(bool isHeads, int index)
        {
            _isHeads = isHeads;
            _coinIndex = index;
            
            UpdateVisuals();
        }
        
        /// <summary>
        /// 동전 선택 상태 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisuals();
        }
        
        /// <summary>
        /// 동전 뒤집기 (액티브 스킬용)
        /// </summary>
        public void FlipCoin()
        {
            _isHeads = !_isHeads;
            UpdateVisuals();
        }
        
        /// <summary>
        /// 비주얼 업데이트
        /// </summary>
        private void UpdateVisuals()
        {
            // 배경 색상
            if (backgroundImage != null)
            {
                if (_isSelected)
                {
                    backgroundImage.color = selectedColor;
                }
                else
                {
                    backgroundImage.color = _isHeads ? headsColor : tailsColor;
                }
            }
            
            // 메인 이미지 (동전 이미지가 있다면)
            if (coinImage != null && coinImage != backgroundImage)
            {
                coinImage.color = _isHeads ? headsColor : tailsColor;
            }
            
            // 텍스트
            if (coinText != null)
            {
                coinText.text = _isHeads ? "공격" : "방어";
                coinText.color = Color.white;
            }
        }
        
        /// <summary>
        /// 동전 값 반환
        /// </summary>
        public bool GetCoinValue() => _isHeads;
        
        /// <summary>
        /// 동전 인덱스 반환
        /// </summary>
        public int GetCoinIndex() => _coinIndex;
    }
}
