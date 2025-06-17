using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
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
        
        // Public accessor for enemy intention text
        public Text EnemyIntentionText => enemyIntentionText;
        
        // 기존 생성된 UI 요소 참조
        private List<GameObject> coinObjects = new List<GameObject>();
        private List<GameObject> patternObjects = new List<GameObject>();
        private List<GameObject> playerStatusEffectObjects = new List<GameObject>();
        private List<GameObject> enemyStatusEffectObjects = new List<GameObject>();
        
        // 매니저 참조
        private CombatSystem combatManager;
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
            
            // CombatSystem 참조 설정
            if (CombatSystem.Instance != null)
            {
                combatManager = CombatSystem.Instance;
                Debug.Log("CombatUI: Successfully got CombatSystem reference");
            }
            else
            {
                Debug.LogError("CombatUI: CombatSystem.Instance is null");
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
        
                // CombatSystem 참조 업데이트 확인
                if (CombatSystem.Instance != null && combatManager == null)
                {
                    combatManager = CombatSystem.Instance;
                    Debug.Log("CombatUI: Updated CombatSystem reference during initialization");
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
                    enemyIntentionText.text = "적이 준비 중..."; // 적 의도 초기화
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
            Debug.Log("CombatUI: Starting component validation");
            
            // 체력바 자동 생성 및 설정
            SetupHealthBars();
            
            // 기타 UI 요소 검증
            ValidateOtherComponents();
            
            Debug.Log("CombatUI: Component validation completed");
        }
        
        /// <summary>
        /// 체력바 자동 설정
        /// </summary>
        private void SetupHealthBars()
        {
            // 플레이어 체력바 설정
            GameObject playerHealthBarGO = transform.Find("PlayerHealthBar")?.gameObject;
            if (playerHealthBarGO != null)
            {
                playerHealthBar = playerHealthBarGO.GetComponent<Slider>();
                if (playerHealthBar == null)
                {
                    Debug.Log("CombatUI: Creating Slider component for PlayerHealthBar");
                    playerHealthBar = playerHealthBarGO.AddComponent<Slider>();
                    SetupSliderComponents(playerHealthBar, "Player");
                }
                else
                {
                    // 기존 슬라이더의 텍스트 찾기
                    playerHealthText = playerHealthBarGO.GetComponentInChildren<Text>();
                }
            }
            else
            {
                Debug.LogError("CombatUI: PlayerHealthBar GameObject not found!");
            }
            
            // 적 체력바 설정
            GameObject enemyHealthBarGO = transform.Find("EnemyHealthBar")?.gameObject;
            if (enemyHealthBarGO != null)
            {
                enemyHealthBar = enemyHealthBarGO.GetComponent<Slider>();
                if (enemyHealthBar == null)
                {
                    Debug.Log("CombatUI: Creating Slider component for EnemyHealthBar");
                    enemyHealthBar = enemyHealthBarGO.AddComponent<Slider>();
                    SetupSliderComponents(enemyHealthBar, "Enemy");
                }
                else
                {
                    // 기존 슬라이더의 텍스트 찾기
                    enemyHealthText = enemyHealthBarGO.GetComponentInChildren<Text>();
                }
            }
            else
            {
                Debug.LogError("CombatUI: EnemyHealthBar GameObject not found!");
            }
        }
        
        /// <summary>
        /// 슬라이더 컴포넌트 자동 설정
        /// </summary>
        private void SetupSliderComponents(Slider slider, string prefix)
        {
            Debug.Log($"CombatUI: Setting up slider components for {prefix}");
            
            RectTransform sliderRT = slider.GetComponent<RectTransform>();
            sliderRT.sizeDelta = new Vector2(200, 20);
            
            // Background 생성
            GameObject background = new GameObject("Background");
            background.transform.SetParent(slider.transform, false);
            RectTransform bgRT = background.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Fill Area 생성
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(slider.transform, false);
            RectTransform fillAreaRT = fillArea.AddComponent<RectTransform>();
            fillAreaRT.anchorMin = Vector2.zero;
            fillAreaRT.anchorMax = Vector2.one;
            fillAreaRT.sizeDelta = Vector2.zero;
            fillAreaRT.offsetMin = Vector2.zero;
            fillAreaRT.offsetMax = Vector2.zero;
            
            // Fill 생성
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            RectTransform fillRT = fill.AddComponent<RectTransform>();
            fillRT.anchorMin = Vector2.zero;
            fillRT.anchorMax = Vector2.one;
            fillRT.sizeDelta = Vector2.zero;
            fillRT.offsetMin = Vector2.zero;
            fillRT.offsetMax = Vector2.zero;
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = prefix == "Player" ? new Color(0.2f, 0.8f, 0.2f) : new Color(0.8f, 0.2f, 0.2f);
            
            // Slider 설정
            slider.fillRect = fillRT;
            slider.targetGraphic = fillImage;
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 100;
            slider.value = 100;
            slider.interactable = false;
            
            // 체력 텍스트 생성
            GameObject healthText = new GameObject($"{prefix}HealthText");
            healthText.transform.SetParent(slider.transform, false);
            RectTransform textRT = healthText.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            
            Text text = healthText.AddComponent<Text>();
            text.text = "100/100";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 14;
            text.fontStyle = FontStyle.Bold;
            
            // 폰트 설정 (기본 폰트 사용)
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            if (prefix == "Player")
                playerHealthText = text;
            else
                enemyHealthText = text;
                
            Debug.Log($"CombatUI: {prefix} health bar setup completed");
        }
        
        /// <summary>
        /// 기타 UI 컴포넌트 검증
        /// </summary>
        private void ValidateOtherComponents()
        {
            // 코인 컨테이너 검증
            if (coinContainer == null)
            {
                coinContainer = transform.Find("CoinArea");
                if (coinContainer == null)
                {
                    // 여러 가지 가능한 이름으로 찾기
                    coinContainer = transform.Find("CoinContainer") ?? 
                                   transform.Find("Coins");
                                   
                    if (coinContainer == null)
                    {
                        Debug.LogWarning("CombatUI: 정적 CoinArea를 찾을 수 없어 동적 생성합니다. 정적 UI 설정을 권장합니다.");
                        CreateCoinContainer();
                    }
                    else
                    {
                        Debug.Log("CombatUI: 대체 정적 CoinContainer 찾음");
                    }
                }
                else
                {
                    Debug.Log("CombatUI: 정적 CoinArea 찾음");
                }
            }
                
            // 패턴 컨테이너 검증
            if (patternContainer == null)
            {
                patternContainer = transform.Find("PatternArea");
                if (patternContainer == null)
                {
                    // 여러 가지 가능한 이름으로 찾기
                    patternContainer = transform.Find("PatternContainer") ?? 
                                      transform.Find("Patterns") ??
                                      transform.Find("PatternArea/Content"); // ScrollRect의 Content 영역
                                      
                    if (patternContainer == null)
                    {
                        Debug.LogWarning("CombatUI: 정적 PatternArea를 찾을 수 없어 동적 생성합니다. 정적 UI 설정을 권장합니다.");
                        CreatePatternContainer();
                    }
                    else
                    {
                        Debug.Log("CombatUI: 대체 정적 PatternContainer 찾음");
                    }
                }
                else
                {
                    Debug.Log("CombatUI: 정적 PatternArea 찾음");
                }
            }
                
            // 버튼 검증
            if (activeSkillButton == null)
            {
                activeSkillButton = transform.Find("ActiveSkillButton")?.GetComponent<Button>();
                if (activeSkillButton == null)
                    Debug.LogError("CombatUI: activeSkillButton is missing!");
                else
                    Debug.Log("CombatUI: ActiveSkillButton found and assigned");
            }
                
            if (endTurnButton == null)
            {
                endTurnButton = transform.Find("EndTurnButton")?.GetComponent<Button>();
                if (endTurnButton == null)
                    Debug.LogError("CombatUI: endTurnButton is missing!");
                else
                    Debug.Log("CombatUI: EndTurnButton found and assigned");
            }
                
            // 텍스트 검증
            if (turnInfoText == null)
            {
                turnInfoText = transform.Find("TurnInfoText")?.GetComponent<Text>();
                if (turnInfoText == null)
                    Debug.LogError("CombatUI: turnInfoText is missing!");
                else
                    Debug.Log("CombatUI: TurnInfoText found and assigned");
            }
                
            if (enemyIntentionText == null)
            {
                enemyIntentionText = transform.Find("EnemyIntentionText")?.GetComponent<Text>();
                if (enemyIntentionText == null)
                {
                    Debug.LogWarning("CombatUI: EnemyIntentionText not found, creating one");
                    CreateEnemyIntentionText();
                }
                else
                {
                    Debug.Log("CombatUI: EnemyIntentionText found and assigned");
                }
            }
            
            // 상태 효과 컨테이너 검증
            ValidateStatusEffectContainers();
        }
        
        /// <summary>
        /// 적 의도 텍스트 생성
        /// </summary>
        private void CreateEnemyIntentionText()
        {
            GameObject intentionTextGO = new GameObject("EnemyIntentionText");
            intentionTextGO.transform.SetParent(transform, false);
            
            RectTransform textRT = intentionTextGO.AddComponent<RectTransform>();
            textRT.anchorMin = new Vector2(0.5f, 0.7f);
            textRT.anchorMax = new Vector2(0.5f, 0.7f);
            textRT.sizeDelta = new Vector2(300, 30);
            textRT.anchoredPosition = Vector2.zero;
            
            enemyIntentionText = intentionTextGO.AddComponent<Text>();
            enemyIntentionText.text = "적이 준비 중...";
            enemyIntentionText.alignment = TextAnchor.MiddleCenter;
            enemyIntentionText.color = Color.yellow;
            enemyIntentionText.fontSize = 16;
            enemyIntentionText.fontStyle = FontStyle.Bold;
            enemyIntentionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            Debug.Log("CombatUI: EnemyIntentionText created");
        }
        
        /// <summary>
        /// 상태 효과 컨테이너 검증 및 생성
        /// </summary>
        private void ValidateStatusEffectContainers()
        {
            // 플레이어 상태 효과 컨테이너
            if (playerStatusEffectContainer == null)
            {
                playerStatusEffectContainer = transform.Find("PlayerStatusEffectContainer");
                if (playerStatusEffectContainer == null)
                {
                    Debug.Log("CombatUI: Creating PlayerStatusEffectContainer");
                    GameObject playerContainer = new GameObject("PlayerStatusEffectContainer");
                    playerContainer.transform.SetParent(transform, false);
                    
                    RectTransform playerRT = playerContainer.AddComponent<RectTransform>();
                    playerRT.anchorMin = new Vector2(0.15f, 0.85f);
                    playerRT.anchorMax = new Vector2(0.45f, 0.95f);
                    playerRT.sizeDelta = Vector2.zero;
                    playerRT.offsetMin = Vector2.zero;
                    playerRT.offsetMax = Vector2.zero;
                    
                    playerStatusEffectContainer = playerContainer.transform;
                    Debug.Log("CombatUI: PlayerStatusEffectContainer created");
                }
                else
                {
                    Debug.Log("CombatUI: PlayerStatusEffectContainer found and assigned");
                }
            }
            
            // 적 상태 효과 컨테이너
            if (enemyStatusEffectContainer == null)
            {
                enemyStatusEffectContainer = transform.Find("EnemyStatusEffectContainer");
                if (enemyStatusEffectContainer == null)
                {
                    Debug.Log("CombatUI: Creating EnemyStatusEffectContainer");
                    GameObject enemyContainer = new GameObject("EnemyStatusEffectContainer");
                    enemyContainer.transform.SetParent(transform, false);
                    
                    RectTransform enemyRT = enemyContainer.AddComponent<RectTransform>();
                    enemyRT.anchorMin = new Vector2(0.55f, 0.85f);
                    enemyRT.anchorMax = new Vector2(0.85f, 0.95f);
                    enemyRT.sizeDelta = Vector2.zero;
                    enemyRT.offsetMin = Vector2.zero;
                    enemyRT.offsetMax = Vector2.zero;
                    
                    enemyStatusEffectContainer = enemyContainer.transform;
                    Debug.Log("CombatUI: EnemyStatusEffectContainer created");
                }
                else
                {
                    Debug.Log("CombatUI: EnemyStatusEffectContainer found and assigned");
                }
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
            
            // 동전 프리팹 찾기 (정적 우선)
            if (coinPrefab == null)
            {
                // Resources 폴더에서 정적 프리팹 찾기
                coinPrefab = Resources.Load<GameObject>("UI/CoinPrefab") ?? 
                            Resources.Load<GameObject>("CoinPrefab") ??
                            Resources.Load<GameObject>("Prefabs/CoinPrefab");
                            
                if (coinPrefab == null)
                {
                    Debug.LogWarning("CombatUI: 정적 CoinPrefab을 찾을 수 없어 동적 생성합니다.");
                    CreateImprovedCoinPrefab();
                }
                else
                {
                    Debug.Log("CombatUI: 정적 CoinPrefab 로드 완료");
                }
            }
            
            // 새 동전 UI 생성
            for (int i = 0; i < coinResults.Count; i++)
            {
                GameObject coinObj = Instantiate(coinPrefab, coinContainer);
                
                // 동전 이미지 및 텍스트 설정
                Image coinImage = coinObj.GetComponent<Image>();
                Text coinText = coinObj.GetComponentInChildren<Text>();
                
                if (coinResults[i]) // 앞면
                {
                    if (coinImage != null)
                        coinImage.color = new Color(0.8f, 0.2f, 0.2f, 0.9f); // 앞면 - 빨간색
                    if (coinText != null)
                        coinText.text = "앞면";
                }
                else // 뒷면
                {
                    if (coinImage != null)
                        coinImage.color = new Color(0.2f, 0.3f, 0.8f, 0.9f); // 뒷면 - 파란색
                    if (coinText != null)
                        coinText.text = "뒷면";
                }
                
                // 위치 설정
                RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float horizontalSpacing = 80f;
                    float xPos = (i - (coinResults.Count - 1) / 2.0f) * horizontalSpacing;
                    rectTransform.anchoredPosition = new Vector2(xPos, 0);
                }
                
                coinObjects.Add(coinObj);
            }
            
            Debug.Log($"CombatUI: Updated coin UI with {coinResults.Count} coins");
        }
        
        /// <summary>
        /// 개선된 동전 프리팹 생성
        /// </summary>
        private void CreateImprovedCoinPrefab()
        {
            Debug.Log("CombatUI: Creating improved coin prefab");
            
            // 동전 프리팹 생성
            coinPrefab = new GameObject("CoinPrefab");
            RectTransform rectTransform = coinPrefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 60);
            
            // 동전 이미지
            Image coinImage = coinPrefab.AddComponent<Image>();
            coinImage.color = new Color(0.5f, 0.5f, 0.5f, 0.9f);
            
            // 동전 텍스트
            GameObject textObj = new GameObject("CoinText");
            textObj.transform.SetParent(coinPrefab.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 18;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            
            Debug.Log("CombatUI: Improved coin prefab created");
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
            
            // 패턴 프리팹 찾기 (정적 우선)
            if (patternButtonPrefab == null)
            {
                // Resources 폴더에서 정적 프리팹 찾기
                patternButtonPrefab = Resources.Load<GameObject>("UI/PatternButtonPrefab") ?? 
                                     Resources.Load<GameObject>("PatternButtonPrefab") ??
                                     Resources.Load<GameObject>("Prefabs/PatternButtonPrefab");
                                     
                if (patternButtonPrefab == null)
                {
                    Debug.LogWarning("CombatUI: 정적 PatternButtonPrefab을 찾을 수 없어 동적 생성합니다.");
                    CreateImprovedPatternPrefab();
                }
                else
                {
                    Debug.Log("CombatUI: 정적 PatternButtonPrefab 로드 완료");
                }
            }
            
            // 패턴 버튼 생성
            for (int i = 0; i < availablePatterns.Count; i++)
            {
                Pattern pattern = availablePatterns[i];
                GameObject patternObj = Instantiate(patternButtonPrefab, patternContainer);
                
                // 패턴 이름 설정 (족보명 (패턴정보) 형식)
                Text nameText = patternObj.transform.Find("PatternName")?.GetComponent<Text>();
                if (nameText != null)
                {
                    string patternInfo = pattern.GetPatternTypeString();
                    nameText.text = $"{pattern.Name} ({patternInfo})";
                }
                
                // 패턴 효과 설명 설정
                Text effectText = patternObj.transform.Find("PatternEffect")?.GetComponent<Text>();
                if (effectText != null)
                {
                    if (pattern.IsAttack)
                    {
                        effectText.text = $"공격 +{pattern.AttackBonus}";
                    }
                    else
                    {
                        effectText.text = $"방어 +{pattern.DefenseBonus}";
                    }
                }
                
                // 공격력/방어력 표시
                Text powerText = patternObj.transform.Find("PowerText")?.GetComponent<Text>();
                if (powerText != null)
                {
                    if (pattern.IsAttack)
                    {
                        powerText.text = $"공격\n+{pattern.AttackBonus}";
                        powerText.color = new Color(1f, 0.3f, 0.3f);
                    }
                    else
                    {
                        powerText.text = $"방어\n+{pattern.DefenseBonus}";
                        powerText.color = new Color(0.3f, 0.3f, 1f);
                    }
                }
                
                // 버튼 색상 설정 (앞면 빨간색, 뒷면 파란색 통일)
                Image buttonImage = patternObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    if (pattern.PatternValue) // 앞면 패턴
                    {
                        buttonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // 빨간색
                    }
                    else // 뒷면 패턴
                    {
                        buttonImage.color = new Color(0.2f, 0.3f, 0.8f, 0.8f); // 파란색
                    }
                }
                
                // 버튼 이벤트 설정
                Button patternButton = patternObj.GetComponent<Button>();
                if (patternButton != null)
                {
                    Pattern localPattern = pattern;
                    patternButton.onClick.AddListener(() => OnPatternButtonClicked(localPattern));
                }
                
                // 위치 설정 (세로 배열)
                RectTransform rectTransform = patternObj.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    float yPos = -i * 60f;
                    rectTransform.anchoredPosition = new Vector2(0, yPos);
                }
                
                patternObjects.Add(patternObj);
            }
            
            Debug.Log($"CombatUI: Updated pattern UI with {availablePatterns.Count} patterns with detailed info");
        }
        
        /// <summary>
        /// 개선된 패턴 버튼 프리팹 생성
        /// </summary>
        private void CreateImprovedPatternPrefab()
        {
            Debug.Log("CombatUI: Creating improved pattern button prefab");
            
            // 패턴 버튼 프리팹 생성
            patternButtonPrefab = new GameObject("PatternButtonPrefab");
            RectTransform rectTransform = patternButtonPrefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 55);
            
            // 버튼 컴포넌트
            Image buttonImage = patternButtonPrefab.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            Button button = patternButtonPrefab.AddComponent<Button>();
            
            // 패턴 이름 텍스트
            GameObject nameTextObj = new GameObject("PatternName");
            nameTextObj.transform.SetParent(patternButtonPrefab.transform, false);
            RectTransform nameRT = nameTextObj.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.5f);
            nameRT.anchorMax = new Vector2(0.65f, 1);
            nameRT.offsetMin = new Vector2(10, -15);
            nameRT.offsetMax = new Vector2(0, 15);
            
            Text nameText = nameTextObj.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.fontSize = 16;
            nameText.color = Color.white;
            nameText.fontStyle = FontStyle.Bold;
            nameText.alignment = TextAnchor.MiddleLeft;
            
            // 패턴 효과 텍스트
            GameObject effectTextObj = new GameObject("PatternEffect");
            effectTextObj.transform.SetParent(patternButtonPrefab.transform, false);
            RectTransform effectRT = effectTextObj.AddComponent<RectTransform>();
            effectRT.anchorMin = new Vector2(0, 0);
            effectRT.anchorMax = new Vector2(0.65f, 0.5f);
            effectRT.offsetMin = new Vector2(10, -15);
            effectRT.offsetMax = new Vector2(0, 15);
            
            Text effectText = effectTextObj.AddComponent<Text>();
            effectText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            effectText.fontSize = 11;
            effectText.color = new Color(0.8f, 0.8f, 0.8f);
            effectText.alignment = TextAnchor.MiddleLeft;
            
            // 공격력/방어력 표시
            GameObject powerTextObj = new GameObject("PowerText");
            powerTextObj.transform.SetParent(patternButtonPrefab.transform, false);
            RectTransform powerRT = powerTextObj.AddComponent<RectTransform>();
            powerRT.anchorMin = new Vector2(0.65f, 0);
            powerRT.anchorMax = new Vector2(1, 1);
            powerRT.offsetMin = Vector2.zero;
            powerRT.offsetMax = new Vector2(-10, 0);
            
            Text powerText = powerTextObj.AddComponent<Text>();
            powerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            powerText.fontSize = 12;
            powerText.color = Color.yellow;
            powerText.alignment = TextAnchor.MiddleCenter;
            powerText.fontStyle = FontStyle.Bold;
            
            Debug.Log("CombatUI: Improved pattern button prefab created");
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
                combatManager.ExecutePlayerPattern(pattern);
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
                    Image buttonImage = button.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                    buttonImage.color = selectedPattern.IsAttack ? 
                        new Color(1.0f, 0.2f, 0.2f, 1.0f) : 
                        new Color(0.2f, 0.2f, 1.0f, 1.0f);
                }
                    
                    // 선택 효과 (옵션)
                    button.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                }
                else
                {
                    // 다른 패턴 기본 색상으로 복원
                    Image buttonImage = button.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        bool isAttack = buttonImage.color.r > buttonImage.color.b;
                        buttonImage.color = isAttack ? 
                            new Color(0.8f, 0.3f, 0.3f, 0.8f) : 
                            new Color(0.3f, 0.3f, 0.8f, 0.8f);
                    }
                    
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
                combatManager.EndPlayerTurn();
                Debug.Log("CombatUI: Player turn ended via end turn button");
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
            
            // 상태 효과 프리팹 찾기 (정적 우선)
            if (statusEffectPrefab == null)
            {
                // Resources 폴더에서 정적 프리팹 찾기
                statusEffectPrefab = Resources.Load<GameObject>("UI/StatusEffectPrefab") ?? 
                                    Resources.Load<GameObject>("StatusEffectPrefab") ??
                                    Resources.Load<GameObject>("Prefabs/StatusEffectPrefab");
                                    
                if (statusEffectPrefab == null)
                {
                    Debug.LogWarning("CombatUI: 정적 StatusEffectPrefab을 찾을 수 없어 동적 생성합니다.");
                    CreateImprovedStatusEffectPrefab();
                }
                else
                {
                    Debug.Log("CombatUI: 정적 StatusEffectPrefab 로드 완료");
                }
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
        /// 개선된 상태 효과 프리팹 생성
        /// </summary>
        private void CreateImprovedStatusEffectPrefab()
        {
            Debug.Log("CombatUI: Creating improved status effect prefab");
            
            // 상태 효과 프리팹 생성
            statusEffectPrefab = new GameObject("StatusEffectPrefab");
            RectTransform rectTransform = statusEffectPrefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(40, 40);
            
            // 배경 이미지
            Image effectImage = statusEffectPrefab.AddComponent<Image>();
            effectImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // 효과 텍스트
            GameObject textObj = new GameObject("EffectText");
            textObj.transform.SetParent(statusEffectPrefab.transform, false);
            RectTransform textRT = textObj.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.fontSize = 8;
            text.color = Color.white;
            text.fontStyle = FontStyle.Bold;
            
            Debug.Log("CombatUI: Improved status effect prefab created");
        }
        
        /// <summary>
        /// 상태 효과 아이콘 생성 헬퍼 메서드
        /// </summary>
        private void CreateStatusEffectIcons(List<StatusEffect> effects, Transform container, List<GameObject> objectsList)
        {
            float spacing = 45f;
            
            for (int i = 0; i < effects.Count; i++)
            {
                StatusEffect effect = effects[i];
                GameObject effectObj = Instantiate(statusEffectPrefab, container);
                
                // 효과 텍스트 설정
                Text effectText = effectObj.GetComponentInChildren<Text>();
                if (effectText != null)
                {
                    // 상태 효과 이름의 한글 줄임말 사용
                    string shortName = GetShortNameForStatusEffect(effect.EffectType);
                    effectText.text = $"{shortName}\n{effect.Magnitude}\n({effect.RemainingDuration})";
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
                case StatusEffectType.Amplify:   return new Color(0.8f, 0.5f, 0.0f, 0.9f); // 황색
                case StatusEffectType.Resonance: return new Color(0.8f, 0.8f, 0.2f, 0.9f); // 금색
                case StatusEffectType.Mark:      return new Color(0.5f, 0.8f, 0.5f, 0.9f); // 연두색
                case StatusEffectType.Bleed:     return new Color(0.8f, 0.0f, 0.0f, 0.9f); // 적색
                case StatusEffectType.Counter:   return new Color(0.5f, 0.5f, 0.8f, 0.9f); // 청색
                case StatusEffectType.Crush:     return new Color(0.5f, 0.2f, 0.0f, 0.9f); // 갈색
                case StatusEffectType.Curse:     return new Color(0.5f, 0.0f, 0.5f, 0.9f); // 보라색
                case StatusEffectType.Seal:      return new Color(0.2f, 0.2f, 0.2f, 0.9f); // 회색
                case StatusEffectType.Poison:    return new Color(0.0f, 0.5f, 0.0f, 0.9f); // 녹색
                case StatusEffectType.Burn:      return new Color(0.8f, 0.4f, 0.0f, 0.9f); // 주황색
                default:                   return new Color(0.3f, 0.3f, 0.3f, 0.9f); // 회색
            }
        }
    
        /// <summary>
        /// 상태 효과 유형에 따른 줄임말 반환
        /// </summary>
        private string GetShortNameForStatusEffect(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify:   return "증폭";
                case StatusEffectType.Resonance: return "공명";
                case StatusEffectType.Mark:      return "표식";
                case StatusEffectType.Bleed:     return "출혈";
                case StatusEffectType.Counter:   return "반격";
                case StatusEffectType.Crush:     return "분쇄";
                case StatusEffectType.Curse:     return "저주";
                case StatusEffectType.Seal:      return "봉인";
                case StatusEffectType.Poison:    return "독";
                case StatusEffectType.Burn:      return "화상";
                default:                   return "효과";
            }
        }
        #endregion
        
        #region Container Creation Methods
        /// <summary>
        /// 코인 컨테이너 자동 생성
        /// </summary>
        private void CreateCoinContainer()
        {
            GameObject coinAreaGO = new GameObject("CoinArea");
            coinAreaGO.transform.SetParent(transform, false);
            
            RectTransform coinRT = coinAreaGO.AddComponent<RectTransform>();
            coinRT.anchorMin = new Vector2(0.2f, 0.1f);
            coinRT.anchorMax = new Vector2(0.8f, 0.3f);
            coinRT.sizeDelta = Vector2.zero;
            coinRT.offsetMin = Vector2.zero;
            coinRT.offsetMax = Vector2.zero;
            
            coinContainer = coinAreaGO.transform;
            Debug.Log("CombatUI: CoinArea container created automatically");
        }
        
        /// <summary>
        /// 패턴 컨테이너 자동 생성
        /// </summary>
        private void CreatePatternContainer()
        {
            GameObject patternAreaGO = new GameObject("PatternArea");
            patternAreaGO.transform.SetParent(transform, false);
            
            RectTransform patternRT = patternAreaGO.AddComponent<RectTransform>();
            patternRT.anchorMin = new Vector2(0.05f, 0.35f);
            patternRT.anchorMax = new Vector2(0.45f, 0.85f);
            patternRT.sizeDelta = Vector2.zero;
            patternRT.offsetMin = Vector2.zero;
            patternRT.offsetMax = Vector2.zero;
            
            // 스크롤 가능하도록 ScrollRect 추가
            ScrollRect scrollRect = patternAreaGO.AddComponent<ScrollRect>();
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            
            // Content 영역 생성
            GameObject contentGO = new GameObject("Content");
            contentGO.transform.SetParent(patternAreaGO.transform, false);
            RectTransform contentRT = contentGO.AddComponent<RectTransform>();
            contentRT.anchorMin = new Vector2(0, 0);
            contentRT.anchorMax = new Vector2(1, 1);
            contentRT.sizeDelta = Vector2.zero;
            contentRT.offsetMin = Vector2.zero;
            contentRT.offsetMax = Vector2.zero;
            
            scrollRect.content = contentRT;
            
            patternContainer = contentGO.transform;
            Debug.Log("CombatUI: PatternArea container created automatically with scroll support");
        }
        #endregion
    }
}