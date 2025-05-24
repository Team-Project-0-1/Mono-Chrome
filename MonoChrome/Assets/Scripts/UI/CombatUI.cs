using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 전투 UI를 관리하는 클래스
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        #region UI References
        [Header("Health Bars")]
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private Slider enemyHealthBar;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private Text enemyHealthText;
        
        [Header("Coin UI")]
        [SerializeField] private Transform coinContainer;
        [SerializeField] private GameObject coinPrefab;
        
        [Header("Pattern UI")]
        [SerializeField] private Transform patternContainer;
        [SerializeField] private GameObject patternButtonPrefab;
        
        [Header("Status Effects")]
        [SerializeField] private Transform playerStatusEffectContainer;
        [SerializeField] private Transform enemyStatusEffectContainer;
        [SerializeField] private GameObject statusEffectPrefab;
        
        [Header("Combat Controls")]
        [SerializeField] private Button activeSkillButton;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Text turnInfoText;
        [SerializeField] private Text enemyIntentionText;
        
        // 기존 생성된 UI 요소 참조
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<GameObject> patternObjects = new List<GameObject>();
        private List<GameObject> playerStatusEffectObjects = new List<GameObject>();
        private List<GameObject> enemyStatusEffectObjects = new List<GameObject>();
        
        // 매니저 참조
        private CombatManager combatManager;
        #endregion
        
        #region Initialization
        private void Awake()
        {
            // 컴포넌트 참조 확인
            ValidateComponents();
        }
        
        private void Start()
        {
            // 이벤트 구독
            SubscribeToEvents();
            
            // 매니저 참조 설정
            if (GameManager.Instance != null)
            {
                // Core.GameManager에서 MonoChrome.Combat.CombatManager 참조 얻기
                // 주의: GameManager가 MonoChrome.Combat.CombatManager를 직접 참조하거나
                // MonoChrome.CombatManager를 통해 MonoChrome.Combat.CombatManager를 참조하는지 확인 필요
                CombatManager managerWrapper = GameManager.Instance.CombatManager;
                if (managerWrapper != null) {
                    combatManager = managerWrapper.GetComponent<CombatManager>();
                }
                
                if (combatManager == null)
                {
                    Debug.LogError("CombatUI: CombatManager reference is null from GameManager");
                }
                else
                {
                    Debug.Log("CombatUI: Successfully got CombatManager reference");
                }
            }
            else
            {
                Debug.LogError("CombatUI: GameManager.Instance is null");
            }
            
            Debug.Log("CombatUI initialized");
        }
        
        /// <summary>
        /// 전투 UI를 초기화하는 메서드
        /// </summary>
        public void InitializeCombatUI()
        {
            try
            {
                Debug.Log("CombatUI: Initializing combat UI");
        
                // 컴포넌트 참조 확인
                ValidateComponents();
        
                // 매니저 참조 업데이트 확인
                if (GameManager.Instance != null && combatManager == null)
                {
                    // Core.GameManager에서 MonoChrome.Combat.CombatManager 참조 다시 시도
                    CombatManager managerWrapper = GameManager.Instance.CombatManager;
                    if (managerWrapper != null) {
                        combatManager = managerWrapper;
                    }
                    Debug.Log("CombatUI: Updated CombatManager reference during initialization");
                }
        
                // 초기 UI 상태 설정
                if (activeSkillButton != null)
                {
                    UpdateActiveSkillButton(false); // 초기에는 액티브 스킬 비활성화
                }
        
                // 체력바 초기화 - 수정된 부분
                InitializeHealthBars();
        
                // 상태 초기화
                if (turnInfoText != null)
                {
                    turnInfoText.text = "Turn: 1"; // 첫 턴으로 초기화
                }
        
                if (enemyIntentionText != null)
                {
                    enemyIntentionText.text = "Enemy is preparing..."; // 적 의도 초기화
                }
        
                // 기존 UI 요소 정리
                ClearUIElements();
        
                Debug.Log("CombatUI: Combat UI initialized successfully");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CombatUI: Error initializing combat UI - {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 체력바 초기화 - 새로 추가된 메서드
        /// </summary>
        private void InitializeHealthBars()
        {
            // 체력바 참조 확인
            if (playerHealthBar == null || enemyHealthBar == null)
            {
                ValidateComponents();
        
                if (playerHealthBar == null || enemyHealthBar == null)
                {
                    Debug.LogError("CombatUI: Failed to find health bar references");
                    return;
                }
            }
    
            // 체력바 초기 값 설정
            if (playerHealthBar != null)
            {
                playerHealthBar.value = playerHealthBar.maxValue;
        
                if (playerHealthText != null)
                {
                    playerHealthText.text = $"{Mathf.RoundToInt(playerHealthBar.maxValue)}/{Mathf.RoundToInt(playerHealthBar.maxValue)}";
                }
            }
    
            if (enemyHealthBar != null)
            {
                enemyHealthBar.value = enemyHealthBar.maxValue;
        
                if (enemyHealthText != null)
                {
                    enemyHealthText.text = $"{Mathf.RoundToInt(enemyHealthBar.maxValue)}/{Mathf.RoundToInt(enemyHealthBar.maxValue)}";
                }
            }
    
            Debug.Log("CombatUI: Health bars initialized");
        }
        
        /// <summary>
        /// 기존 UI 요소 정리
        /// </summary>
        private void ClearUIElements()
        {
            // 동전 분석 요소 정리
            foreach (GameObject coinObj in coinObjects)
            {
                Destroy(coinObj);
            }
            coinObjects.Clear();
            
            // 패턴 분석 요소 정리
            foreach (GameObject patternObj in patternObjects)
            {
                Destroy(patternObj);
            }
            patternObjects.Clear();
            
            // 상태 효과 요소 정리
            foreach (GameObject effectObj in playerStatusEffectObjects)
            {
                Destroy(effectObj);
            }
            playerStatusEffectObjects.Clear();
            
            foreach (GameObject effectObj in enemyStatusEffectObjects)
            {
                Destroy(effectObj);
            }
            enemyStatusEffectObjects.Clear();
            
            Debug.Log("CombatUI: Cleared all UI elements");
        }
        
        private void ValidateComponents()
        {
            // 헬스바 검증
            if (playerHealthBar == null)
            {
                playerHealthBar = transform.Find("PlayerHealthBar")?.GetComponent<Slider>();
                if (playerHealthBar == null)
                    Debug.LogError("CombatUI: playerHealthBar is missing!");
            }
                
            if (enemyHealthBar == null)
            {
                enemyHealthBar = transform.Find("EnemyHealthBar")?.GetComponent<Slider>();
                if (enemyHealthBar == null)
                    Debug.LogError("CombatUI: enemyHealthBar is missing!");
            }
                
            // 헬스 텍스트 검증
            if (playerHealthText == null)
            {
                playerHealthText = playerHealthBar?.transform.Find("PlayerHealthText")?.GetComponent<Text>();
            }
                
            if (enemyHealthText == null)
            {
                enemyHealthText = enemyHealthBar?.transform.Find("EnemyHealthText")?.GetComponent<Text>();
            }
                
            // 코인 컨테이너 검증
            if (coinContainer == null)
            {
                coinContainer = transform.Find("CoinArea");
                if (coinContainer == null)
                    Debug.LogError("CombatUI: coinContainer is missing!");
            }
                
            // 패턴 컨테이너 검증
            if (patternContainer == null)
            {
                patternContainer = transform.Find("PatternArea");
                if (patternContainer == null)
                    Debug.LogError("CombatUI: patternContainer is missing!");
            }
                
            // 버튼 검증
            if (activeSkillButton == null)
            {
                activeSkillButton = transform.Find("ActiveSkillButton")?.GetComponent<Button>();
                if (activeSkillButton == null)
                    Debug.LogError("CombatUI: activeSkillButton is missing!");
            }
                
            if (endTurnButton == null)
            {
                endTurnButton = transform.Find("EndTurnButton")?.GetComponent<Button>();
                if (endTurnButton == null)
                    Debug.LogError("CombatUI: endTurnButton is missing!");
            }
                
            // 텍스트 검증
            if (turnInfoText == null)
            {
                turnInfoText = transform.Find("TurnInfoText")?.GetComponent<Text>();
                if (turnInfoText == null)
                    Debug.LogError("CombatUI: turnInfoText is missing!");
            }
                
            if (enemyIntentionText == null)
            {
                enemyIntentionText = transform.Find("EnemyIntentionText")?.GetComponent<Text>();
                if (enemyIntentionText == null)
                    Debug.LogWarning("CombatUI: enemyIntentionText is missing!");
            }
        }
        
        private void SubscribeToEvents()
        {
            // 버튼 이벤트 구독
            if (activeSkillButton != null)
            {
                activeSkillButton.onClick.RemoveAllListeners();
                activeSkillButton.onClick.AddListener(OnActiveSkillButtonClicked);
                Debug.Log("CombatUI: Subscribed to active skill button click");
            }
            
            if (endTurnButton != null)
            {
                endTurnButton.onClick.RemoveAllListeners();
                endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
                Debug.Log("CombatUI: Subscribed to end turn button click");
            }
        }
        #endregion
        
        #region UI Updates
        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            // 플레이어 체력바 업데이트
            if (playerHealthBar != null)
            {
                playerHealthBar.maxValue = playerMaxHealth;
                playerHealthBar.value = playerHealth;
                
                Debug.Log($"CombatUI: Updated player health bar: {playerHealth}/{playerMaxHealth}");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot update player health bar - reference is missing");
            }
            
            if (playerHealthText != null)
            {
                playerHealthText.text = $"{Mathf.RoundToInt(playerHealth)}/{Mathf.RoundToInt(playerMaxHealth)}";
            }
            
            // 적 체력바 업데이트
            if (enemyHealthBar != null)
            {
                enemyHealthBar.maxValue = enemyMaxHealth;
                enemyHealthBar.value = enemyHealth;
                
                Debug.Log($"CombatUI: Updated enemy health bar: {enemyHealth}/{enemyMaxHealth}");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot update enemy health bar - reference is missing");
            }
            
            if (enemyHealthText != null)
            {
                enemyHealthText.text = $"{Mathf.RoundToInt(enemyHealth)}/{Mathf.RoundToInt(enemyMaxHealth)}";
            }
        }
        
        /// <summary>
        /// 동전 UI 업데이트
        /// </summary>
        public void UpdateCoinUI(List<bool> coinResults)
        {
            if (coinContainer == null)
            {
                Debug.LogError("CombatUI: Cannot update coin UI - container reference is missing");
                return;
            }
            
            // 기존 동전 객체 정리
            foreach (GameObject coinObj in coinObjects)
            {
                Destroy(coinObj);
            }
            coinObjects.Clear();
            
            // 동전 프리팹이 없는 경우 임시 프리팹 생성
            if (coinPrefab == null)
            {
                // 임시 동전 프리팹 생성
                coinPrefab = new GameObject("CoinPrefab");
                coinPrefab.AddComponent<RectTransform>();
                coinPrefab.AddComponent<Image>();
                
                // 텍스트 추가
                GameObject textObj = new GameObject("CoinText");
                textObj.transform.SetParent(coinPrefab.transform);
                textObj.AddComponent<RectTransform>();
                Text text = textObj.AddComponent<Text>();
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 12;
                text.color = Color.white;
                
                Debug.LogWarning("CombatUI: Created temporary coin prefab");
            }
            
            // 새 동전 UI 생성
            for (int i = 0; i < coinResults.Count; i++)
            {
                GameObject coinObj = Instantiate(coinPrefab, coinContainer);
                
                // 동전 이미지 설정
                Image coinImage = coinObj.GetComponent<Image>();
                if (coinImage != null)
                {
                    // 앞면/뒷면에 따라 이미지 설정
                    coinImage.color = coinResults[i] ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.3f, 0.3f, 0.8f);
                }
                
                // 동전 텍스트 설정
                Text coinText = coinObj.GetComponentInChildren<Text>();
                if (coinText != null)
                {
                    coinText.text = coinResults[i] ? "A" : "D"; // Attack/Defense
                }
                
                // 위치 설정
                RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float horizontalSpacing = 80f;
                    float xPos = (i - (coinResults.Count - 1) / 2.0f) * horizontalSpacing;
                    rectTransform.anchoredPosition = new Vector2(xPos, 0);
                    
                    // 크기 설정
                    rectTransform.sizeDelta = new Vector2(60, 60);
                }
                
                coinObjects.Add(coinObj);
            }
            
            Debug.Log($"CombatUI: Updated coin UI with {coinResults.Count} coins");
        }
        
        /// <summary>
        /// 패턴(족보) UI 업데이트
        /// </summary>
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            if (patternContainer == null)
            {
                Debug.LogError("CombatUI: Cannot update pattern UI - container reference is missing");
                return;
            }
            
            // 기존 패턴 객체 정리
            foreach (GameObject patternObj in patternObjects)
            {
                Destroy(patternObj);
            }
            patternObjects.Clear();
            
            // 패턴 버튼 프리팹이 없는 경우 임시 프리팹 생성
            if (patternButtonPrefab == null)
            {
                // 임시 패턴 버튼 프리팹 생성
                patternButtonPrefab = new GameObject("PatternButtonPrefab");
                patternButtonPrefab.AddComponent<RectTransform>();
                patternButtonPrefab.AddComponent<Image>();
                patternButtonPrefab.AddComponent<Button>();
                
                // 텍스트 추가
                GameObject textObj = new GameObject("PatternText");
                textObj.transform.SetParent(patternButtonPrefab.transform);
                textObj.AddComponent<RectTransform>();
                Text text = textObj.AddComponent<Text>();
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 14;
                text.color = Color.white;
                
                Debug.LogWarning("CombatUI: Created temporary pattern button prefab");
            }
            
            // 패턴 버튼 생성
            for (int i = 0; i < availablePatterns.Count; i++)
            {
                Pattern pattern = availablePatterns[i];
                GameObject patternObj = Instantiate(patternButtonPrefab, patternContainer);
                
                // 패턴 이름 텍스트 설정
                Text patternText = patternObj.GetComponentInChildren<Text>();
                if (patternText != null)
                {
                    patternText.text = pattern.Name;
                }
                
                // 버튼 이벤트 설정
                Button patternButton = patternObj.GetComponent<Button>();
                if (patternButton != null)
                {
                    // 클로저를 위한 로컬 변수
                    Pattern localPattern = pattern;
                    patternButton.onClick.AddListener(() => OnPatternButtonClicked(localPattern));
                    
                    // 공격/방어 색상 구분
                    ColorBlock colors = patternButton.colors;
                    colors.normalColor = pattern.IsAttack ? new Color(0.8f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 0.8f);
                    patternButton.colors = colors;
                }
                
                // 위치 설정
                RectTransform rectTransform = patternObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float verticalSpacing = 70f;
                    float yPos = -i * verticalSpacing;
                    rectTransform.anchoredPosition = new Vector2(0, yPos);
                    
                    // 크기 설정
                    rectTransform.sizeDelta = new Vector2(200, 60);
                }
                
                patternObjects.Add(patternObj);
            }
            
            Debug.Log($"CombatUI: Updated pattern UI with {availablePatterns.Count} patterns");
        }
        
        /// <summary>
        /// 턴 카운터 업데이트
        /// </summary>
        public void UpdateTurnCounter(int turnCount)
        {
            if (turnInfoText != null)
            {
                turnInfoText.text = $"Turn: {turnCount}";
                Debug.Log($"CombatUI: Updated turn counter to {turnCount}");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot update turn counter - text reference is missing");
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 업데이트
        /// </summary>
        public void UpdateActiveSkillButton(bool isAvailable)
        {
            if (activeSkillButton != null)
            {
                activeSkillButton.interactable = isAvailable;
                
                // 시각적 피드백 (옵션)
                Image buttonImage = activeSkillButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = isAvailable ? new Color(0.8f, 0.8f, 0.2f) : new Color(0.5f, 0.5f, 0.5f);
                }
                
                Debug.Log($"CombatUI: Updated active skill button - Available: {isAvailable}");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot update active skill button - reference is missing");
            }
        }
        
        /// <summary>
        /// 적 의도 표시
        /// </summary>
        public void ShowEnemyIntention(Pattern pattern)
        {
            if (enemyIntentionText != null && pattern != null)
            {
                string intentionText = pattern.IsAttack 
                    ? $"<color=red>Attack: {pattern.Name}</color>" 
                    : $"<color=blue>Defense: {pattern.Name}</color>";
                    
                enemyIntentionText.text = intentionText;
                Debug.Log($"CombatUI: Showing enemy intention: {pattern.Name}");
            }
            else if (enemyIntentionText == null)
            {
                Debug.LogError("CombatUI: Cannot show enemy intention - text reference is missing");
            }
        }
        #endregion
        
        #region UI Event Handlers
        /// <summary>
        /// 패턴 버튼 클릭 처리
        /// </summary>
        private void OnPatternButtonClicked(Pattern pattern)
        {
            if (combatManager != null)
            {
                combatManager.SelectPattern(pattern);
                Debug.Log($"CombatUI: Pattern selected: {pattern.Name}");
                
                // 선택된 패턴 시각적 강조 처리
                HighlightSelectedPattern(pattern);
            }
            else
            {
                Debug.LogError("CombatUI: Cannot select pattern - CombatManager reference is missing");
            }
        }
        
        /// <summary>
        /// 선택된 패턴 강조 처리
        /// </summary>
        private void HighlightSelectedPattern(Pattern selectedPattern)
        {
            foreach (GameObject patternObj in patternObjects)
            {
                Button button = patternObj.GetComponent<Button>();
                Text text = patternObj.GetComponentInChildren<Text>();
                
                if (text != null && text.text == selectedPattern.Name)
                {
                    // 선택된 패턴 강조
                    ColorBlock colors = button.colors;
                    colors.normalColor = selectedPattern.IsAttack ? new Color(1.0f, 0.3f, 0.3f) : new Color(0.3f, 0.3f, 1.0f);
                    button.colors = colors;
                    
                    // 선택 효과 (옵션)
                    button.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }
                else
                {
                    // 다른 패턴 기본 색상
                    ColorBlock colors = button.colors;
                    bool isAttack = button.colors.normalColor.r > button.colors.normalColor.b;
                    colors.normalColor = isAttack ? new Color(0.8f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 0.8f);
                    button.colors = colors;
                    
                    // 기본 크기
                    button.transform.localScale = Vector3.one;
                }
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 클릭 처리
        /// </summary>
        private void OnActiveSkillButtonClicked()
        {
            if (combatManager != null)
            {
                combatManager.UseActiveSkill();
                Debug.Log("CombatUI: Active skill button clicked");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot use active skill - CombatManager reference is missing");
            }
        }
        
        /// <summary>
        /// 턴 종료 버튼 클릭 처리
        /// </summary>
        private void OnEndTurnButtonClicked()
        {
            if (combatManager != null)
            {
                combatManager.FinishPlayerTurn();
                Debug.Log("CombatUI: End turn button clicked");
            }
            else
            {
                Debug.LogError("CombatUI: Cannot finish turn - CombatManager reference is missing");
            }
        }
        #endregion
    
    #region Status Effects
    /// <summary>
    /// 상태 효과 UI 업데이트
    /// </summary>
    public void UpdateStatusEffectsUI(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
    {
        try
        {
            // 기존 상태 효과 UI 정리
            foreach (GameObject effectObj in playerStatusEffectObjects)
            {
                Destroy(effectObj);
            }
            playerStatusEffectObjects.Clear();
            
            foreach (GameObject effectObj in enemyStatusEffectObjects)
            {
                Destroy(effectObj);
            }
            enemyStatusEffectObjects.Clear();
            
            // 컨테이너 확인
            if (playerStatusEffectContainer == null || enemyStatusEffectContainer == null)
            {
                Debug.LogError("CombatUI: Cannot update status effects - containers missing");
                return;
            }
            
            // 상태 효과 프리팹 확인
            if (statusEffectPrefab == null)
            {
                // 임시 상태 효과 프리팹 생성
                statusEffectPrefab = new GameObject("StatusEffectPrefab");
                statusEffectPrefab.AddComponent<RectTransform>();
                statusEffectPrefab.AddComponent<Image>();
                
                // 텍스트 추가
                GameObject textObj = new GameObject("EffectText");
                textObj.transform.SetParent(statusEffectPrefab.transform);
                textObj.AddComponent<RectTransform>();
                Text text = textObj.AddComponent<Text>();
                text.alignment = TextAnchor.MiddleCenter;
                text.fontSize = 10;
                text.color = Color.white;
                
                Debug.LogWarning("CombatUI: Created temporary status effect prefab");
            }
            
            // 플레이어 상태 효과 UI 생성
            CreateStatusEffectIcons(playerEffects, playerStatusEffectContainer, playerStatusEffectObjects);
            
            // 적 상태 효과 UI 생성
            CreateStatusEffectIcons(enemyEffects, enemyStatusEffectContainer, enemyStatusEffectObjects);
            
            Debug.Log($"CombatUI: Updated status effects - Player: {playerEffects.Count}, Enemy: {enemyEffects.Count}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"CombatUI: Error updating status effects - {ex.Message}");
        }
    }
    
    /// <summary>
    /// 상태 효과 아이콘 생성 헬퍼 메서드
    /// </summary>
    private void CreateStatusEffectIcons(List<StatusEffect> effects, Transform container, List<GameObject> objectsList)
    {
        float spacing = 40f;
        
        for (int i = 0; i < effects.Count; i++)
        {
            StatusEffect effect = effects[i];
            GameObject effectObj = Instantiate(statusEffectPrefab, container);
            
            // 효과 텍스트 설정
            Text effectText = effectObj.GetComponentInChildren<Text>();
            if (effectText != null)
            {
                effectText.text = $"{effect.EffectType}\n{effect.Magnitude} ({effect.RemainingDuration}턴)";
            }
            
            // 효과 색상 설정
            Image effectImage = effectObj.GetComponent<Image>();
            if (effectImage != null)
            {
                // 효과 유형에 따른 색상 설정
                effectImage.color = GetColorForStatusEffect(effect.EffectType);
            }
            
            // 위치 설정
            RectTransform rectTransform = effectObj.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = new Vector2(i * spacing, 0);
                rectTransform.sizeDelta = new Vector2(35, 35);
            }
            
            objectsList.Add(effectObj);
        }
    }
    
    /// <summary>
    /// 상태 효과 유형에 따른 색상 반환
    /// </summary>
    private Color GetColorForStatusEffect(StatusEffectType effectType)
    {
        switch (effectType)
        {
            case StatusEffectType.Amplify:   return new Color(0.8f, 0.5f, 0.0f); // 황색
            case StatusEffectType.Resonance: return new Color(0.8f, 0.8f, 0.2f); // 금색
            case StatusEffectType.Mark:      return new Color(0.5f, 0.8f, 0.5f); // 연두색
            case StatusEffectType.Bleed:     return new Color(0.8f, 0.0f, 0.0f); // 적색
            case StatusEffectType.Counter:   return new Color(0.5f, 0.5f, 0.8f); // 청색
            case StatusEffectType.Crush:     return new Color(0.5f, 0.2f, 0.0f); // 갈색
            case StatusEffectType.Curse:     return new Color(0.5f, 0.0f, 0.5f); // 보라색
            case StatusEffectType.Seal:      return new Color(0.2f, 0.2f, 0.2f); // 회색
            case StatusEffectType.Poison:    return new Color(0.0f, 0.5f, 0.0f); // 녹색
            case StatusEffectType.Burn:      return new Color(0.8f, 0.4f, 0.0f); // 주황색
            default:                   return new Color(0.3f, 0.3f, 0.3f); // 회색
        }
    }
    #endregion
}
}