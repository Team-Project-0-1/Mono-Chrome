using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 완전히 개선된 전투 UI 시스템
    /// 포트폴리오 품질을 위한 완전한 UI 관리
    /// </summary>
    public class ImprovedCombatUI : MonoBehaviour
    {
        #region UI References
        [Header("체력바 시스템")]
        [SerializeField] private GameObject playerHealthBarParent;
        [SerializeField] private GameObject enemyHealthBarParent;
        [SerializeField] private Slider playerHealthBar;
        [SerializeField] private Slider enemyHealthBar;
        [SerializeField] private Text playerHealthText;
        [SerializeField] private Text enemyHealthText;
        
        [Header("동전 UI 시스템")]
        [SerializeField] private GameObject coinUIParent;
        [SerializeField] private Transform coinContainer;
        [SerializeField] private GameObject coinPrefab;
        
        [Header("패턴 UI 시스템")]
        [SerializeField] private GameObject patternUIParent;
        [SerializeField] private Transform patternContainer;
        [SerializeField] private GameObject patternButtonPrefab;
        [SerializeField] private ScrollRect patternScrollRect;
        
        [Header("전투 제어")]
        [SerializeField] private Button activeSkillButton;
        [SerializeField] private Button endTurnButton;
        [SerializeField] private Text activeSkillText;
        [SerializeField] private Text turnInfoText;
        [SerializeField] private Text enemyIntentionText;
        
        [Header("상태 효과 UI")]
        [SerializeField] private Transform playerStatusContainer;
        [SerializeField] private Transform enemyStatusContainer;
        [SerializeField] private GameObject statusEffectPrefab;
        
        // 동적 생성된 UI 요소들
        private List<GameObject> dynamicCoinObjects = new List<GameObject>();
        private List<GameObject> dynamicPatternObjects = new List<GameObject>();
        private List<GameObject> dynamicPlayerStatusObjects = new List<GameObject>();
        private List<GameObject> dynamicEnemyStatusObjects = new List<GameObject>();
        
        // 매니저 참조
        private CombatSystem combatManager;
        private bool isInitialized = false;
        #endregion
        
        #region Initialization
        private void Awake()
        {
            Debug.Log("ImprovedCombatUI: Awake called");
            CreateUIElements();
        }
        
        private void Start()
        {
            StartCoroutine(DelayedInitialization());
        }
        
        /// <summary>
        /// 지연된 초기화
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // CombatSystem 대기
            while (CombatSystem.Instance == null)
                yield return null;
                
            // CombatSystem 참조 설정
            combatManager = CombatSystem.Instance;
            
            // UI 요소 검증 및 생성
            ValidateAndCreateUI();
            
            // 이벤트 구독
            SubscribeToEvents();
            
            isInitialized = true;
            Debug.Log("ImprovedCombatUI: Initialization completed");
        }
        
        /// <summary>
        /// UI 요소 생성
        /// </summary>
        private void CreateUIElements()
        {
            if (transform.childCount == 0)
            {
                CreateCompleteUIStructure();
            }
        }
        
        /// <summary>
        /// 완전한 UI 구조 생성
        /// </summary>
        private void CreateCompleteUIStructure()
        {
            Debug.Log("ImprovedCombatUI: Creating complete UI structure");
            
            // 루트 Canvas 설정
            Canvas canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                gameObject.AddComponent<CanvasScaler>();
                gameObject.AddComponent<GraphicRaycaster>();
            }
            
            // 체력바 영역 생성
            CreateHealthBarArea();
            
            // 동전 영역 생성
            CreateCoinArea();
            
            // 패턴 영역 생성
            CreatePatternArea();
            
            // 제어 버튼 영역 생성
            CreateControlArea();
            
            // 상태 효과 영역 생성
            CreateStatusEffectArea();
            
            Debug.Log("ImprovedCombatUI: Complete UI structure created");
        }
        
        /// <summary>
        /// 체력바 영역 생성
        /// </summary>
        private void CreateHealthBarArea()
        {
            // 플레이어 체력바
            playerHealthBarParent = CreateUIPanel("PlayerHealthBarArea", new Vector2(300, 60), new Vector2(-400, 350));
            playerHealthBar = CreateHealthBar(playerHealthBarParent, "PlayerHealthBar", Color.green);
            playerHealthText = CreateHealthText(playerHealthBarParent, "PlayerHealthText");
            
            // 적 체력바
            enemyHealthBarParent = CreateUIPanel("EnemyHealthBarArea", new Vector2(300, 60), new Vector2(400, 350));
            enemyHealthBar = CreateHealthBar(enemyHealthBarParent, "EnemyHealthBar", Color.red);
            enemyHealthText = CreateHealthText(enemyHealthBarParent, "EnemyHealthText");
            
            Debug.Log("ImprovedCombatUI: Health bar area created");
        }
        
        /// <summary>
        /// 체력바 생성 헬퍼
        /// </summary>
        private Slider CreateHealthBar(GameObject parent, string name, Color color)
        {
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent.transform, false);
            
            // RectTransform 설정
            RectTransform rectTransform = sliderObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Slider 컴포넌트
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.maxValue = 100;
            slider.value = 100;
            
            // 배경
            GameObject background = new GameObject("Background");
            background.transform.SetParent(sliderObj.transform, false);
            Image bgImage = background.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            RectTransform bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Fill Area
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Fill
            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = color;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Slider 설정
            slider.fillRect = fillRect;
            
            return slider;
        }
        
        /// <summary>
        /// 체력 텍스트 생성 헬퍼
        /// </summary>
        private Text CreateHealthText(GameObject parent, string name)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Text text = textObj.AddComponent<Text>();
            text.text = "100/100";
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.fontSize = 14;
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            return text;
        }
        
        /// <summary>
        /// 동전 영역 생성
        /// </summary>
        private void CreateCoinArea()
        {
            coinUIParent = CreateUIPanel("CoinArea", new Vector2(500, 100), new Vector2(0, 200));
            
            // 동전 컨테이너
            GameObject container = new GameObject("CoinContainer");
            container.transform.SetParent(coinUIParent.transform, false);
            coinContainer = container.transform;
            
            // 수평 레이아웃 그룹
            HorizontalLayoutGroup layoutGroup = container.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 20f;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            
            RectTransform containerRect = container.GetComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = Vector2.zero;
            containerRect.offsetMax = Vector2.zero;
            
            Debug.Log("ImprovedCombatUI: Coin area created");
        }
        
        /// <summary>
        /// 패턴 영역 생성
        /// </summary>
        private void CreatePatternArea()
        {
            patternUIParent = CreateUIPanel("PatternArea", new Vector2(300, 400), new Vector2(-400, -50));
            
            // 스크롤 뷰 생성
            GameObject scrollView = new GameObject("PatternScrollView");
            scrollView.transform.SetParent(patternUIParent.transform, false);
            
            RectTransform scrollRect = scrollView.AddComponent<RectTransform>();
            scrollRect.anchorMin = Vector2.zero;
            scrollRect.anchorMax = Vector2.one;
            scrollRect.offsetMin = Vector2.zero;
            scrollRect.offsetMax = Vector2.zero;
            
            // ScrollRect 컴포넌트
            patternScrollRect = scrollView.AddComponent<ScrollRect>();
            patternScrollRect.horizontal = false;
            patternScrollRect.vertical = true;
            
            // Viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(scrollView.transform, false);
            viewport.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            // Content
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            patternContainer = content.transform;
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            // 수직 레이아웃 그룹
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.childAlignment = TextAnchor.UpperCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = false;
            
            // Content Size Fitter
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            // ScrollRect 설정
            patternScrollRect.viewport = viewportRect;
            patternScrollRect.content = contentRect;
            
            Debug.Log("ImprovedCombatUI: Pattern area created");
        }
        
        /// <summary>
        /// 제어 영역 생성
        /// </summary>
        private void CreateControlArea()
        {
            GameObject controlArea = CreateUIPanel("ControlArea", new Vector2(400, 100), new Vector2(0, -350));
            
            // 액티브 스킬 버튼
            activeSkillButton = CreateButton(controlArea, "ActiveSkillButton", "액티브 스킬", new Vector2(-100, 0));
            
            // 턴 종료 버튼
            endTurnButton = CreateButton(controlArea, "EndTurnButton", "턴 종료", new Vector2(100, 0));
            
            // 턴 정보 텍스트
            GameObject turnInfoObj = new GameObject("TurnInfoText");
            turnInfoObj.transform.SetParent(controlArea.transform, false);
            turnInfoText = turnInfoObj.AddComponent<Text>();
            turnInfoText.text = "Turn: 1";
            turnInfoText.alignment = TextAnchor.MiddleCenter;
            turnInfoText.color = Color.white;
            turnInfoText.fontSize = 18;
            turnInfoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform turnInfoRect = turnInfoObj.GetComponent<RectTransform>();
            turnInfoRect.anchoredPosition = new Vector2(0, 30);
            turnInfoRect.sizeDelta = new Vector2(200, 30);
            
            // 적 의도 텍스트
            GameObject enemyIntentionObj = new GameObject("EnemyIntentionText");
            enemyIntentionObj.transform.SetParent(controlArea.transform, false);
            enemyIntentionText = enemyIntentionObj.AddComponent<Text>();
            enemyIntentionText.text = "Enemy is thinking...";
            enemyIntentionText.alignment = TextAnchor.MiddleCenter;
            enemyIntentionText.color = Color.yellow;
            enemyIntentionText.fontSize = 14;
            enemyIntentionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform enemyIntentionRect = enemyIntentionObj.GetComponent<RectTransform>();
            enemyIntentionRect.anchoredPosition = new Vector2(0, -30);
            enemyIntentionRect.sizeDelta = new Vector2(300, 25);
            
            Debug.Log("ImprovedCombatUI: Control area created");
        }
        
        /// <summary>
        /// 상태 효과 영역 생성
        /// </summary>
        private void CreateStatusEffectArea()
        {
            // 플레이어 상태 효과
            GameObject playerStatusArea = CreateUIPanel("PlayerStatusArea", new Vector2(300, 60), new Vector2(-400, 280));
            playerStatusContainer = playerStatusArea.transform;
            
            // 적 상태 효과
            GameObject enemyStatusArea = CreateUIPanel("EnemyStatusArea", new Vector2(300, 60), new Vector2(400, 280));
            enemyStatusContainer = enemyStatusArea.transform;
            
            Debug.Log("ImprovedCombatUI: Status effect area created");
        }
        
        /// <summary>
        /// UI 패널 생성 헬퍼
        /// </summary>
        private GameObject CreateUIPanel(string name, Vector2 size, Vector2 position)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(transform, false);
            
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = position;
            
            return panel;
        }
        
        /// <summary>
        /// 버튼 생성 헬퍼
        /// </summary>
        private Button CreateButton(GameObject parent, string name, string text, Vector2 position)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent.transform, false);
            
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(120, 40);
            rectTransform.anchoredPosition = position;
            
            // 버튼 이미지
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // 버튼 컴포넌트
            Button button = buttonObj.AddComponent<Button>();
            
            // 버튼 텍스트
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = text;
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = 12;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            if (name == "ActiveSkillButton")
                activeSkillText = buttonText;
            
            return button;
        }
        #endregion
        
        #region UI Validation and Updates
        /// <summary>
        /// UI 검증 및 생성
        /// </summary>
        private void ValidateAndCreateUI()
        {
            if (playerHealthBar == null || enemyHealthBar == null)
            {
                Debug.LogWarning("ImprovedCombatUI: Health bars missing, recreating...");
                CreateHealthBarArea();
            }
            
            if (coinContainer == null)
            {
                Debug.LogWarning("ImprovedCombatUI: Coin container missing, recreating...");
                CreateCoinArea();
            }
            
            if (patternContainer == null)
            {
                Debug.LogWarning("ImprovedCombatUI: Pattern container missing, recreating...");
                CreatePatternArea();
            }
        }
        
        /// <summary>
        /// 전투 UI 초기화 (외부 호출용)
        /// </summary>
        public void InitializeCombatUI()
        {
            if (!isInitialized)
            {
                StartCoroutine(DelayedInitialization());
                return;
            }
            
            // 초기 상태 설정
            UpdateHealthBars(80, 80, 60, 60); // 테스트 값
            UpdateTurnCounter(1);
            UpdateActiveSkillButton(true);
            
            // 기존 동적 요소 정리
            ClearDynamicElements();
            
            Debug.Log("ImprovedCombatUI: Combat UI initialized");
        }
        
        /// <summary>
        /// 동적 요소 정리
        /// </summary>
        private void ClearDynamicElements()
        {
            ClearDynamicObjects(dynamicCoinObjects);
            ClearDynamicObjects(dynamicPatternObjects);
            ClearDynamicObjects(dynamicPlayerStatusObjects);
            ClearDynamicObjects(dynamicEnemyStatusObjects);
        }
        
        /// <summary>
        /// 동적 오브젝트 리스트 정리
        /// </summary>
        private void ClearDynamicObjects(List<GameObject> objects)
        {
            foreach (GameObject obj in objects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            objects.Clear();
        }
        #endregion
        
        #region Public UI Update Methods
        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.maxValue = playerMaxHealth;
                playerHealthBar.value = playerHealth;
                
                if (playerHealthText != null)
                    playerHealthText.text = $"{Mathf.RoundToInt(playerHealth)}/{Mathf.RoundToInt(playerMaxHealth)}";
            }
            
            if (enemyHealthBar != null)
            {
                enemyHealthBar.maxValue = enemyMaxHealth;
                enemyHealthBar.value = enemyHealth;
                
                if (enemyHealthText != null)
                    enemyHealthText.text = $"{Mathf.RoundToInt(enemyHealth)}/{Mathf.RoundToInt(enemyMaxHealth)}";
            }
            
            Debug.Log($"ImprovedCombatUI: Health bars updated - Player: {playerHealth}/{playerMaxHealth}, Enemy: {enemyHealth}/{enemyMaxHealth}");
        }
        
        /// <summary>
        /// 동전 UI 업데이트
        /// </summary>
        public void UpdateCoinUI(List<bool> coinResults)
        {
            if (coinContainer == null) return;
            
            // 기존 동전 정리
            ClearDynamicObjects(dynamicCoinObjects);
            
            // 새 동전 생성
            for (int i = 0; i < coinResults.Count; i++)
            {
                GameObject coinObj = CreateCoinObject(coinResults[i], i);
                dynamicCoinObjects.Add(coinObj);
            }
            
            Debug.Log($"ImprovedCombatUI: Updated {coinResults.Count} coins");
        }
        
        /// <summary>
        /// 동전 오브젝트 생성
        /// </summary>
        private GameObject CreateCoinObject(bool isHeads, int index)
        {
            GameObject coinObj = new GameObject($"Coin_{index}");
            coinObj.transform.SetParent(coinContainer, false);
            
            // 크기 설정
            RectTransform rectTransform = coinObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(60, 60);
            
            // 배경 이미지
            Image coinImage = coinObj.AddComponent<Image>();
            coinImage.color = isHeads ? new Color(0.8f, 0.3f, 0.3f) : new Color(0.3f, 0.3f, 0.8f);
            
            // 텍스트
            GameObject textObj = new GameObject("CoinText");
            textObj.transform.SetParent(coinObj.transform, false);
            
            Text coinText = textObj.AddComponent<Text>();
            coinText.text = isHeads ? "A" : "D"; // Attack/Defense
            coinText.alignment = TextAnchor.MiddleCenter;
            coinText.color = Color.white;
            coinText.fontSize = 20;
            coinText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return coinObj;
        }
        
        /// <summary>
        /// 패턴 UI 업데이트
        /// </summary>
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            if (patternContainer == null) return;
            
            // 기존 패턴 정리
            ClearDynamicObjects(dynamicPatternObjects);
            
            // 새 패턴 버튼 생성
            for (int i = 0; i < availablePatterns.Count; i++)
            {
                Pattern pattern = availablePatterns[i];
                GameObject patternObj = CreatePatternButton(pattern, i);
                dynamicPatternObjects.Add(patternObj);
            }
            
            Debug.Log($"ImprovedCombatUI: Updated {availablePatterns.Count} patterns");
        }
        
        /// <summary>
        /// 패턴 버튼 생성
        /// </summary>
        private GameObject CreatePatternButton(Pattern pattern, int index)
        {
            GameObject buttonObj = new GameObject($"Pattern_{index}");
            buttonObj.transform.SetParent(patternContainer, false);
            
            // 크기 설정
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 50);
            
            // 버튼 이미지
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = pattern.IsAttack ? new Color(0.8f, 0.5f, 0.5f) : new Color(0.5f, 0.5f, 0.8f);
            
            // 버튼 컴포넌트
            Button button = buttonObj.AddComponent<Button>();
            
            // 버튼 텍스트
            GameObject textObj = new GameObject("PatternText");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            Text buttonText = textObj.AddComponent<Text>();
            buttonText.text = $"{pattern.Name}\n{pattern.Description}";
            buttonText.alignment = TextAnchor.MiddleCenter;
            buttonText.color = Color.white;
            buttonText.fontSize = 11;
            buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 5);
            textRect.offsetMax = new Vector2(-5, -5);
            
            // 버튼 이벤트 설정
            button.onClick.AddListener(() => OnPatternButtonClicked(pattern));
            
            return buttonObj;
        }
        
        /// <summary>
        /// 턴 카운터 업데이트
        /// </summary>
        public void UpdateTurnCounter(int turnCount)
        {
            if (turnInfoText != null)
            {
                turnInfoText.text = $"Turn: {turnCount}";
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
                
                if (activeSkillText != null)
                {
                    activeSkillText.color = isAvailable ? Color.white : Color.gray;
                }
            }
        }
        
        /// <summary>
        /// 적 의도 표시
        /// </summary>
        public void ShowEnemyIntention(Pattern pattern)
        {
            if (enemyIntentionText != null && pattern != null)
            {
                string colorTag = pattern.IsAttack ? "red" : "blue";
                enemyIntentionText.text = $"<color={colorTag}>{pattern.Name}: {pattern.Description}</color>";
            }
        }
        
        /// <summary>
        /// 상태 효과 UI 업데이트
        /// </summary>
        public void UpdateStatusEffectsUI(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            // 기존 상태 효과 정리
            ClearDynamicObjects(dynamicPlayerStatusObjects);
            ClearDynamicObjects(dynamicEnemyStatusObjects);
            
            // 플레이어 상태 효과 생성
            if (playerStatusContainer != null)
            {
                CreateStatusEffectIcons(playerEffects, playerStatusContainer, dynamicPlayerStatusObjects);
            }
            
            // 적 상태 효과 생성
            if (enemyStatusContainer != null)
            {
                CreateStatusEffectIcons(enemyEffects, enemyStatusContainer, dynamicEnemyStatusObjects);
            }
        }
        
        /// <summary>
        /// 상태 효과 아이콘 생성
        /// </summary>
        private void CreateStatusEffectIcons(List<StatusEffect> effects, Transform container, List<GameObject> objectsList)
        {
            for (int i = 0; i < effects.Count; i++)
            {
                StatusEffect effect = effects[i];
                GameObject effectObj = CreateStatusEffectIcon(effect, i);
                effectObj.transform.SetParent(container, false);
                objectsList.Add(effectObj);
            }
        }
        
        /// <summary>
        /// 상태 효과 아이콘 생성
        /// </summary>
        private GameObject CreateStatusEffectIcon(StatusEffect effect, int index)
        {
            GameObject effectObj = new GameObject($"StatusEffect_{index}");
            
            RectTransform rectTransform = effectObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(35, 35);
            rectTransform.anchoredPosition = new Vector2(index * 40, 0);
            
            // 배경 이미지
            Image effectImage = effectObj.AddComponent<Image>();
            effectImage.color = GetStatusEffectColor(effect.EffectType);
            
            // 텍스트
            GameObject textObj = new GameObject("EffectText");
            textObj.transform.SetParent(effectObj.transform, false);
            
            Text effectText = textObj.AddComponent<Text>();
            effectText.text = $"{GetStatusEffectShortName(effect.EffectType)}\n{effect.Magnitude}";
            effectText.alignment = TextAnchor.MiddleCenter;
            effectText.color = Color.white;
            effectText.fontSize = 8;
            effectText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return effectObj;
        }
        #endregion
        
        #region Event Handlers and Subscriptions
        /// <summary>
        /// 이벤트 구독
        /// </summary>
        private void SubscribeToEvents()
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
        /// 패턴 버튼 클릭 이벤트
        /// </summary>
        private void OnPatternButtonClicked(Pattern pattern)
        {
            if (combatManager != null)
            {
                combatManager.ExecutePlayerPattern(pattern);
                Debug.Log($"ImprovedCombatUI: Pattern selected - {pattern.Name}");
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 클릭 이벤트
        /// </summary>
        private void OnActiveSkillButtonClicked()
        {
            if (combatManager != null)
            {
                combatManager.UseActiveSkill();
                Debug.Log("ImprovedCombatUI: Active skill button clicked");
            }
        }
        
        /// <summary>
        /// 턴 종료 버튼 클릭 이벤트
        /// </summary>
        private void OnEndTurnButtonClicked()
        {
            if (combatManager != null)
            {
                Debug.Log("ImprovedCombatUI: End turn button clicked - not implemented");
            }
        }
        #endregion
        
        #region Helper Methods
        /// <summary>
        /// 상태 효과 색상 반환
        /// </summary>
        private Color GetStatusEffectColor(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify: return new Color(0.8f, 0.5f, 0.0f);
                case StatusEffectType.Resonance: return new Color(0.8f, 0.8f, 0.2f);
                case StatusEffectType.Mark: return new Color(0.5f, 0.8f, 0.5f);
                case StatusEffectType.Bleed: return new Color(0.8f, 0.0f, 0.0f);
                case StatusEffectType.Counter: return new Color(0.5f, 0.5f, 0.8f);
                case StatusEffectType.Crush: return new Color(0.5f, 0.2f, 0.0f);
                case StatusEffectType.Curse: return new Color(0.5f, 0.0f, 0.5f);
                case StatusEffectType.Seal: return new Color(0.2f, 0.2f, 0.2f);
                default: return new Color(0.3f, 0.3f, 0.3f);
            }
        }
        
        /// <summary>
        /// 상태 효과 짧은 이름 반환
        /// </summary>
        private string GetStatusEffectShortName(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify: return "증폭";
                case StatusEffectType.Resonance: return "공명";
                case StatusEffectType.Mark: return "표식";
                case StatusEffectType.Bleed: return "출혈";
                case StatusEffectType.Counter: return "반격";
                case StatusEffectType.Crush: return "분쇄";
                case StatusEffectType.Curse: return "저주";
                case StatusEffectType.Seal: return "봉인";
                default: return "효과";
            }
        }
        #endregion
        
        #region Cleanup
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (activeSkillButton != null)
                activeSkillButton.onClick.RemoveAllListeners();
                
            if (endTurnButton != null)
                endTurnButton.onClick.RemoveAllListeners();
        }
        #endregion
    }
}
