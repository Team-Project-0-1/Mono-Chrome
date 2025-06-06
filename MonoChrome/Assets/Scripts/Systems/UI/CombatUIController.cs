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
    public class CombatUIController : CombatUIBase
    {
        #region Static UI References (인스펙터에서 할당)
        [Header("체력바 UI")]
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private Slider enemyHealthBar;
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
        [SerializeField] private Text turnInfoText;
        [SerializeField] private Text enemyIntentionText;
        
        [Header("상태 효과 UI")]
        [SerializeField] private Transform playerStatusEffectContainer;
        [SerializeField] private Transform enemyStatusEffectContainer;
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
            
            if (playerHealthBar == null)
            {
                Debug.LogError("CombatUIController: playerHealthBar가 할당되지 않았습니다.");
                hasErrors = true;
            }
            
            if (enemyHealthBar == null)
            {
                Debug.LogError("CombatUIController: enemyHealthBar가 할당되지 않았습니다.");
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
            if (playerHealthBar != null)
                playerHealthBar.value = 1f;
            if (enemyHealthBar != null)
                enemyHealthBar.value = 1f;
            
            if (turnInfoText != null)
                turnInfoText.text = "Turn: 1";
            
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
            base.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);
        }
        
        /// <summary>
        /// 동전 UI 업데이트
        /// </summary>
        public void UpdateCoinUI(List<bool> coinResults)
        {
            base.UpdateCoinUI(coinResults);
        }
        
        /// <summary>
        /// 패턴 UI 업데이트
        /// </summary>
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            base.UpdatePatternUI(availablePatterns);
        }
        
        /// <summary>
        /// 턴 카운터 업데이트
        /// </summary>
        public void UpdateTurnCounter(int turnCount)
        {
            base.UpdateTurnCounter(turnCount);
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 업데이트
        /// </summary>
        public void UpdateActiveSkillButton(bool isAvailable, string skillName = "액티브 스킬")
        {
            base.UpdateActiveSkillButton(isAvailable, skillName);
        }
        
        /// <summary>
        /// 적 의도 표시
        /// </summary>
        public void ShowEnemyIntention(string intention)
        {
            base.ShowEnemyIntention(intention);
        }
        
        /// <summary>
        /// 상태 효과 UI 업데이트
        /// </summary>
        public void UpdateStatusEffectsUI(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            base.UpdateStatusEffectsUI(playerEffects, enemyEffects);
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
