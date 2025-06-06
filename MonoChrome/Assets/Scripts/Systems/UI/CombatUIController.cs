using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome.Systems.UI
{
    /// <summary>
    /// 정적 UI 기반의 전투 UI 컨트롤러
    /// 포트폴리오 품질을 위한 올바른 UI 아키텍처 구현
    /// </summary>
    public class CombatUIController : MonoBehaviour
    {
        #region Static UI References (인스펙터에서 할당)
        [Header("체력바 UI")]
        [SerializeField] private Slider playerHealthSlider;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private Slider enemyHealthSlider;
        [SerializeField] private Text enemyHealthText;
        
        [Header("동전 UI")]
        [SerializeField] private Transform coinContainer;
        [SerializeField] private GameObject coinPrefab;
        
        [Header("패턴 UI")]
        [SerializeField] private Transform patternContainer;
        [SerializeField] private GameObject patternButtonPrefab;
        [SerializeField] private ScrollRect patternScrollRect;
        
        [Header("전투 제어 UI")]
        [SerializeField] private Button activeSkillButton;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Text activeSkillText;
        [SerializeField] private Text turnCounterText;
        [SerializeField] private Text enemyIntentionText;
        
        [Header("상태 효과 UI")]
        [SerializeField] private Transform playerStatusContainer;
        [SerializeField] private Transform enemyStatusContainer;
        [SerializeField] private GameObject statusEffectPrefab;
        #endregion
        
        #region Dynamic UI Cache
        private List<GameObject> _activeCoinObjects = new List<GameObject>();
        private List<GameObject> _activePatternObjects = new List<GameObject>();
        private List<GameObject> _activePlayerStatusObjects = new List<GameObject>();
        private List<GameObject> _activeEnemyStatusObjects = new List<GameObject>();
        #endregion
        
        #region Manager References
        private CombatSystem _combatManager;
        #endregion
        
        #region Initialization
        private void Awake()
        {
            ValidateUIReferences();
        }
        
        private void Start()
        {
            InitializeUI();
        }
        
        /// <summary>
        /// UI 참조 검증
        /// </summary>
        private void ValidateUIReferences()
        {
            bool hasErrors = false;
            
            if (playerHealthSlider == null)
            {
                Debug.LogError("CombatUIController: playerHealthSlider가 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (enemyHealthSlider == null)
            {
                Debug.LogError("CombatUIController: enemyHealthSlider가 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (coinContainer == null)
            {
                Debug.LogError("CombatUIController: coinContainer가 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (coinPrefab == null)
            {
                Debug.LogError("CombatUIController: coinPrefab이 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (patternContainer == null)
            {
                Debug.LogError("CombatUIController: patternContainer가 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (patternButtonPrefab == null)
            {
                Debug.LogError("CombatUIController: patternButtonPrefab이 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (hasErrors)
            {
                Debug.LogWarning("CombatUIController: UI 참조가 완전하지 않습니다. 인스펙터에서 참조를 할당하세요.");
            }
        }
        
        /// <summary>
        /// UI 초기화
        /// </summary>
        private void InitializeUI()
        {
            // CombatSystem 참조 설정
            _combatManager = CombatSystem.Instance;
            
            // 버튼 이벤트 연결
            SetupButtonEvents();
            
            // 초기 UI 상태 설정
            ResetUI();
            
            Debug.Log("CombatUIController: UI 초기화 완료");
        }
        
        /// <summary>
        /// 버튼 이벤트 설정
        /// </summary>
        private void SetupButtonEvents()
        {
            if (activeSkillButton != null)
            {
                activeSkillButton.onClick.RemoveAllListeners();
                activeSkillButton.onClick.AddListener(OnActiveSkillButtonClicked);
            }
            
            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveAllListeners();
                endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
            }
        }
        
        /// <summary>
        /// UI 리셋
        /// </summary>
        public void ResetUI()
        {
            // 동적 오브젝트 정리
            ClearDynamicObjects();
            
            // 초기 상태 설정
            if (playerHealthSlider != null)
                playerHealthSlider.value = 1f;
            if (enemyHealthSlider != null)
                enemyHealthSlider.value = 1f;
            
            if (turnCounterText != null)
                turnCounterText.text = "Turn: 1";
            
            if (enemyIntentionText != null)
                enemyIntentionText.text = "";
                
            if (activeSkillButton != null)
                activeSkillButton.interactable = false;
        }
        #endregion
        
        #region Public Update Methods
        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            // 플레이어 체력바
            if (playerHealthSlider != null)
            {
                playerHealthSlider.maxValue = playerMaxHealth;
                playerHealthSlider.value = playerHealth;
            }
            
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{Mathf.RoundToInt(playerHealth)}/{Mathf.RoundToInt(playerMaxHealth)}";
            }
            
            // 적 체력바
            if (enemyHealthSlider != null)
            {
                enemyHealthSlider.maxValue = enemyMaxHealth;
                enemyHealthSlider.value = enemyHealth;
            }
            
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{Mathf.RoundToInt(enemyHealth)}/{Mathf.RoundToInt(enemyMaxHealth)}";
            }
        }
        
        /// <summary>
        /// 동전 UI 업데이트
        /// </summary>
        public void UpdateCoins(List<bool> coinResults)
        {
            if (coinContainer == null || coinPrefab == null) return;
            
            // 기존 동전 정리
            ClearCoinObjects();
            
            // 새 동전 생성
            for (int i = 0; i < coinResults.Count; i++)
            {
                GameObject coinObj = Instantiate(coinPrefab, coinContainer);
                
                // CoinUI 컴포넌트 설정
                CoinUI coinUI = coinObj.GetComponent<CoinUI>();
                if (coinUI != null)
                {
                    coinUI.SetCoinState(coinResults[i], i);
                }
                else
                {
                    // Fallback: 직접 설정
                    SetupCoinDisplay(coinObj, coinResults[i]);
                }
                
                _activeCoinObjects.Add(coinObj);
            }
        }
        
        /// <summary>
        /// 패턴 UI 업데이트
        /// </summary>
        public void UpdatePatterns(List<Pattern> availablePatterns)
        {
            if (patternContainer == null || patternButtonPrefab == null) return;
            
            // 기존 패턴 정리
            ClearPatternObjects();
            
            // 새 패턴 버튼 생성
            for (int i = 0; i < availablePatterns.Count; i++)
            {
                Pattern pattern = availablePatterns[i];
                GameObject patternObj = Instantiate(patternButtonPrefab, patternContainer);
                
                // PatternButtonUI 컴포넌트 설정
                PatternButtonUI patternUI = patternObj.GetComponent<PatternButtonUI>();
                if (patternUI != null)
                {
                    patternUI.Setup(pattern, OnPatternSelected);
                }
                else
                {
                    // Fallback: 직접 설정
                    SetupPatternButton(patternObj, pattern);
                }
                
                _activePatternObjects.Add(patternObj);
            }
        }
        
        /// <summary>
        /// 턴 카운터 업데이트
        /// </summary>
        public void UpdateTurnCounter(int turnCount)
        {
            if (turnCounterText != null)
            {
                turnCounterText.text = $"Turn: {turnCount}";
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 업데이트
        /// </summary>
        public void UpdateActiveSkillButton(bool isAvailable, string skillName = "액티브 스킬")
        {
            if (activeSkillButton != null)
            {
                activeSkillButton.interactable = isAvailable;
            }
            
            if (activeSkillText != null)
            {
                activeSkillText.text = skillName;
                activeSkillText.color = isAvailable ? Color.white : Color.gray;
            }
        }
        
        /// <summary>
        /// 적 의도 표시
        /// </summary>
        public void ShowEnemyIntention(string intention)
        {
            if (enemyIntentionText != null)
            {
                enemyIntentionText.text = intention;
            }
        }
        
        /// <summary>
        /// 상태 효과 UI 업데이트
        /// </summary>
        public void UpdateStatusEffects(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            // 기존 상태 효과 정리
            ClearStatusEffectObjects();
            
            // 플레이어 상태 효과 생성
            if (playerStatusContainer != null)
            {
                CreateStatusEffectIcons(playerEffects, playerStatusContainer, _activePlayerStatusObjects);
            }
            
            // 적 상태 효과 생성
            if (enemyStatusContainer != null)
            {
                CreateStatusEffectIcons(enemyEffects, enemyStatusContainer, _activeEnemyStatusObjects);
            }
        }
        #endregion
        
        #region Private Helper Methods
        /// <summary>
        /// 동적 오브젝트 전체 정리
        /// </summary>
        private void ClearDynamicObjects()
        {
            ClearCoinObjects();
            ClearPatternObjects();
            ClearStatusEffectObjects();
        }
        
        /// <summary>
        /// 동전 오브젝트 정리
        /// </summary>
        private void ClearCoinObjects()
        {
            foreach (GameObject coin in _activeCoinObjects)
            {
                if (coin != null)
                    DestroyImmediate(coin);
            }
            _activeCoinObjects.Clear();
        }
        
        /// <summary>
        /// 패턴 오브젝트 정리
        /// </summary>
        private void ClearPatternObjects()
        {
            foreach (GameObject pattern in _activePatternObjects)
            {
                if (pattern != null)
                    DestroyImmediate(pattern);
            }
            _activePatternObjects.Clear();
        }
        
        /// <summary>
        /// 상태 효과 오브젝트 정리
        /// </summary>
        private void ClearStatusEffectObjects()
        {
            foreach (GameObject effect in _activePlayerStatusObjects)
            {
                if (effect != null)
                    DestroyImmediate(effect);
            }
            _activePlayerStatusObjects.Clear();
            
            foreach (GameObject effect in _activeEnemyStatusObjects)
            {
                if (effect != null)
                    DestroyImmediate(effect);
            }
            _activeEnemyStatusObjects.Clear();
        }
        
        /// <summary>
        /// 동전 디스플레이 설정 (Fallback)
        /// </summary>
        private void SetupCoinDisplay(GameObject coinObj, bool isHeads)
        {
            Image coinImage = coinObj.GetComponent<Image>();
            if (coinImage != null)
            {
                coinImage.color = isHeads ? Color.red : Color.blue;
            }
            
            Text coinText = coinObj.GetComponentInChildren<Text>();
            if (coinText != null)
            {
                coinText.text = isHeads ? "공격" : "방어";
            }
        }
        
        /// <summary>
        /// 패턴 버튼 설정 (Fallback)
        /// </summary>
        private void SetupPatternButton(GameObject patternObj, Pattern pattern)
        {
            Button button = patternObj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnPatternSelected(pattern));
            }
            
            Text buttonText = patternObj.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = $"{pattern.Name}\n{pattern.Description}";
            }
        }
        
        /// <summary>
        /// 상태 효과 아이콘 생성
        /// </summary>
        private void CreateStatusEffectIcons(List<StatusEffect> effects, Transform container, List<GameObject> objectsList)
        {
            if (statusEffectPrefab == null) return;
            
            for (int i = 0; i < effects.Count; i++)
            {
                StatusEffect effect = effects[i];
                GameObject effectObj = Instantiate(statusEffectPrefab, container);
                
                // StatusEffectUI 컴포넌트 설정
                StatusEffectUI effectUI = effectObj.GetComponent<StatusEffectUI>();
                if (effectUI != null)
                {
                    effectUI.Setup(effect);
                }
                
                objectsList.Add(effectObj);
            }
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// 패턴 선택 이벤트
        /// </summary>
        private void OnPatternSelected(Pattern pattern)
        {
            if (_combatManager != null)
            {
                _combatManager.ExecutePlayerPattern(pattern);
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 클릭
        /// </summary>
        private void OnActiveSkillButtonClicked()
        {
            if (_combatManager != null)
            {
                _combatManager.UseActiveSkill();
            }
        }
        
        /// <summary>
        /// 턴 종료 버튼 클릭
        /// </summary>
        private void OnEndTurnButtonClicked()
        {
            if (_combatManager != null)
            {
                // CombatSystem에 턴 종료 메서드가 존재하지 않습니다.
                Debug.Log("CombatUIController: End turn requested - not implemented");
            }
        }
        #endregion
    }
}
