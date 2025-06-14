using System.Collections;
using System.Collections.Generic;
using MonoChrome.Core;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome
{
    /// <summary>
    /// 게임 시작 시 매니저 간의 참조를 초기화하는 클래스
    /// </summary>
    public class InitializeReferences : MonoBehaviour
    {
        [Header("필수 UI 프리팹")]
        [SerializeField] private GameObject _mainCanvasPrefab;
        [SerializeField] private GameObject _combatUIPrefab;
        
        private void Awake()
        {
            // UI 요소가 존재하는지 확인하고 없으면 생성
            EnsureUIExists();
        }
        
        private void Start()
        {
            // MasterGameManager와 다른 매니저들이 먼저 초기화되도록 대기
            StartCoroutine(InitializeWithDelay());
        }
        
        private IEnumerator InitializeWithDelay()
        {
            // UI Canvas가 생성되고 초기화될 시간을 주기 위해 짧게 대기
            yield return new WaitForSeconds(0.1f);
            
            InitializeManagerReferences();
            yield return new WaitForSeconds(0.2f);
            
            // 씬이 로드되었을 때 기본 UI 상태 설정
            ActivateCorrectPanel();
            
            // 이미 게임 진행중이라면, 처리
            var gameStateMachine = MonoChrome.Core.GameStateMachine.Instance;
            if (gameStateMachine != null)
            {
                // 던전 상태라면 던전 생성
                if (gameStateMachine.CurrentState == MonoChrome.Core.GameStateMachine.GameState.Dungeon)
                {
                    var masterGameManager = FindObjectOfType<MasterGameManager>();
                    if (masterGameManager != null && masterGameManager.IsInitialized)
                    {
                        Debug.Log("InitializeReferences: System initialized with MasterGameManager");
                        // 이벤트 기반 시스템에서는 자동으로 처리됨
                    }
                }
                // 전투 상태라면 전투 시작  
                else if (gameStateMachine.CurrentState == MonoChrome.Core.GameStateMachine.GameState.Combat)
                {
                    var masterGameManager = FindObjectOfType<MasterGameManager>();
                    if (masterGameManager != null && masterGameManager.IsInitialized)
                    {
                        Debug.Log("InitializeReferences: Combat system ready via MasterGameManager");
                        // 이벤트 기반 시스템에서는 자동으로 처리됨
                    }
                }
            }
        }
        
        // UI 패널 활성화/비활성화 처리
        private void ActivateCorrectPanel()
        {
            try
            {
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    Debug.LogError("InitializeReferences: Canvas not found!");
                    return;
                }
                
                // 각 패널 참조 찾기
                GameObject characterSelectionPanel = GameObject.Find("CharacterSelectionPanel");
                GameObject dungeonPanel = GameObject.Find("DungeonPanel");
                GameObject combatPanel = GameObject.Find("CombatPanel");
                
                // 기본적으로 모든 패널 비활성화
                if (characterSelectionPanel != null) characterSelectionPanel.SetActive(false);
                if (dungeonPanel != null) dungeonPanel.SetActive(false);
                if (combatPanel != null) combatPanel.SetActive(false);
                
                // 현재 게임 상태에 따라 적절한 패널 활성화
                var gameStateMachine = MonoChrome.Core.GameStateMachine.Instance;
                if (gameStateMachine != null)
                {
                    switch (gameStateMachine.CurrentState)
                    {
                        case MonoChrome.Core.GameStateMachine.GameState.CharacterSelection:
                            if (characterSelectionPanel != null)
                            {
                                characterSelectionPanel.SetActive(true);
                                Debug.Log("InitializeReferences: Activated Character Selection Panel");
                            }
                            break;
                            
                        case MonoChrome.Core.GameStateMachine.GameState.Dungeon:
                            if (dungeonPanel != null)
                            {
                                dungeonPanel.SetActive(true);
                                Debug.Log("InitializeReferences: Activated Dungeon Panel");
                            }
                            break;
                            
                        case MonoChrome.Core.GameStateMachine.GameState.Combat:
                            if (combatPanel != null)
                            {
                                combatPanel.SetActive(true);
                                Debug.Log("InitializeReferences: Activated Combat Panel");
                            }
                            break;
                            
                        default:
                            // 아무 상태도 없을 때, 기본적으로 캐릭터 선택 패널 활성화
                            if (characterSelectionPanel != null)
                            {
                                characterSelectionPanel.SetActive(true);
                                Debug.Log("InitializeReferences: Activated Character Selection Panel (default)");
                                // 새로운 아키텍처에서는 이벤트 기반으로 상태 변경
                                gameStateMachine.TryChangeState(MonoChrome.Core.GameStateMachine.GameState.CharacterSelection);
                            }
                            break;
                    }
                }
                else
                {
                    // GameStateMachine이 없는 경우, 기본적으로 캐릭터 선택 패널 활성화
                    if (characterSelectionPanel != null)
                    {
                        characterSelectionPanel.SetActive(true);
                        Debug.Log("InitializeReferences: Activated Character Selection Panel (no GameStateMachine)");
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"InitializeReferences: Error in ActivateCorrectPanel: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private void EnsureUIExists()
        {
            // 메인 캔버스 확인/생성
            Canvas mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null && _mainCanvasPrefab != null)
            {
                Debug.Log("Creating main canvas");
                Instantiate(_mainCanvasPrefab);
            }
            else if (mainCanvas == null)
            {
                Debug.LogError("No Canvas found and no Canvas prefab assigned!");
                // 기본 캔버스 생성
                GameObject canvasObj = new GameObject("Canvas");
                Canvas canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // 기본 UI 요소 생성
                CreateBasicUIElements(canvasObj.transform);
            }
            else
            {
                // 기존 캔버스에 필요한 UI 요소가 있는지 확인
                CheckUIElementsExist(mainCanvas.transform);
            }
        }
        
        private void CreateBasicUIElements(Transform canvasTransform)
        {
            if (_combatUIPrefab != null)
            {
                Instantiate(_combatUIPrefab, canvasTransform);
                Debug.Log("Created Combat UI prefab");
            }
            else
            {
                // 기본 전투 UI 패널 생성
                GameObject combatPanel = new GameObject("CombatPanel");
                combatPanel.transform.SetParent(canvasTransform, false);
                RectTransform rectTransform = combatPanel.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
                
                // 필수 UI 요소 생성
                CreateHealthBars(combatPanel.transform);
                CreateCoinArea(combatPanel.transform);
                CreatePatternArea(combatPanel.transform);
                CreateStatusEffectAreas(combatPanel.transform);
                CreateActionButtons(combatPanel.transform);
                
                combatPanel.AddComponent<MonoChrome.CombatUI>();
                Debug.Log("Created basic Combat UI elements");
            }
        }
        
        private void CheckUIElementsExist(Transform canvasTransform)
        {
            // CombatPanel 확인
            Transform combatPanel = canvasTransform.Find("CombatPanel");
            if (combatPanel == null)
            {
                if (_combatUIPrefab != null)
                {
                    Instantiate(_combatUIPrefab, canvasTransform);
                    Debug.Log("Added Combat UI prefab to existing canvas");
                }
                else
                {
                    // 기본 전투 UI 패널 생성
                    GameObject combatPanelObj = new GameObject("CombatPanel");
                    combatPanelObj.transform.SetParent(canvasTransform, false);
                    RectTransform rectTransform = combatPanelObj.AddComponent<RectTransform>();
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    
                    // 필수 UI 요소 생성
                    CreateHealthBars(combatPanelObj.transform);
                    CreateCoinArea(combatPanelObj.transform);
                    CreatePatternArea(combatPanelObj.transform);
                    CreateStatusEffectAreas(combatPanelObj.transform);
                    CreateActionButtons(combatPanelObj.transform);
                    
                    combatPanelObj.AddComponent<MonoChrome.CombatUI>();
                    Debug.Log("Created basic Combat UI elements on existing canvas");
                }
            }
            else
            {
                // CombatUI 컴포넌트 확인
                MonoChrome.CombatUI combatUI = combatPanel.GetComponent<MonoChrome.CombatUI>();
                if (combatUI == null)
                {
                    combatUI = combatPanel.gameObject.AddComponent<MonoChrome.CombatUI>();
                    Debug.Log("Added CombatUI component to existing CombatPanel");
                }
                
                // 필수 UI 요소 확인
                if (combatPanel.Find("PlayerHealthBar") == null)
                    CreateHealthBars(combatPanel);
                    
                if (combatPanel.Find("CoinArea") == null)
                    CreateCoinArea(combatPanel);
                    
                if (combatPanel.Find("PatternArea") == null)
                    CreatePatternArea(combatPanel);
                    
                if (combatPanel.Find("PlayerStatusEffects") == null)
                    CreateStatusEffectAreas(combatPanel);
                    
                if (combatPanel.Find("ActiveSkillButton") == null)
                    CreateActionButtons(combatPanel);
            }
        }
        
        private void CreateHealthBars(Transform parent)
        {
            // 플레이어 체력바
            GameObject playerHealthBar = new GameObject("PlayerHealthBar");
            playerHealthBar.transform.SetParent(parent, false);
            RectTransform playerHealthRect = playerHealthBar.AddComponent<RectTransform>();
            playerHealthRect.anchoredPosition = new Vector2(-200, 150);
            playerHealthRect.sizeDelta = new Vector2(200, 30);
            Slider playerSlider = playerHealthBar.AddComponent<Slider>();
            playerSlider.minValue = 0f;
            playerSlider.maxValue = 100f;
            playerSlider.value = 100f;
            
            // 플레이어 체력 텍스트
            GameObject playerHealthText = new GameObject("PlayerHealthText");
            playerHealthText.transform.SetParent(playerHealthBar.transform, false);
            Text playerText = playerHealthText.AddComponent<Text>();
            playerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerText.alignment = TextAnchor.MiddleCenter;
            playerText.text = "100/100";
            RectTransform playerTextRect = playerHealthText.GetComponent<RectTransform>();
            playerTextRect.anchorMin = Vector2.zero;
            playerTextRect.anchorMax = Vector2.one;
            playerTextRect.offsetMin = Vector2.zero;
            playerTextRect.offsetMax = Vector2.zero;
            
            // 적 체력바
            GameObject enemyHealthBar = new GameObject("EnemyHealthBar");
            enemyHealthBar.transform.SetParent(parent, false);
            RectTransform enemyHealthRect = enemyHealthBar.AddComponent<RectTransform>();
            enemyHealthRect.anchoredPosition = new Vector2(200, 150);
            enemyHealthRect.sizeDelta = new Vector2(200, 30);
            Slider enemySlider = enemyHealthBar.AddComponent<Slider>();
            enemySlider.minValue = 0f;
            enemySlider.maxValue = 100f;
            enemySlider.value = 100f;
            
            // 적 체력 텍스트
            GameObject enemyHealthText = new GameObject("EnemyHealthText");
            enemyHealthText.transform.SetParent(enemyHealthBar.transform, false);
            Text enemyText = enemyHealthText.AddComponent<Text>();
            enemyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            enemyText.alignment = TextAnchor.MiddleCenter;
            enemyText.text = "100/100";
            RectTransform enemyTextRect = enemyHealthText.GetComponent<RectTransform>();
            enemyTextRect.anchorMin = Vector2.zero;
            enemyTextRect.anchorMax = Vector2.one;
            enemyTextRect.offsetMin = Vector2.zero;
            enemyTextRect.offsetMax = Vector2.zero;
        }
        
        private void CreateCoinArea(Transform parent)
        {
            GameObject coinArea = new GameObject("CoinArea");
            coinArea.transform.SetParent(parent, false);
            RectTransform coinRect = coinArea.AddComponent<RectTransform>();
            coinRect.anchoredPosition = new Vector2(0, 50);
            coinRect.sizeDelta = new Vector2(400, 100);
        }
        
        private void CreatePatternArea(Transform parent)
        {
            GameObject patternArea = new GameObject("PatternArea");
            patternArea.transform.SetParent(parent, false);
            RectTransform patternRect = patternArea.AddComponent<RectTransform>();
            patternRect.anchoredPosition = new Vector2(-200, -50);
            patternRect.sizeDelta = new Vector2(250, 300);
        }
        
        private void CreateStatusEffectAreas(Transform parent)
        {
            // 플레이어 상태효과 영역
            GameObject playerStatusEffects = new GameObject("PlayerStatusEffects");
            playerStatusEffects.transform.SetParent(parent, false);
            RectTransform playerStatusRect = playerStatusEffects.AddComponent<RectTransform>();
            playerStatusRect.anchoredPosition = new Vector2(-200, -200);
            playerStatusRect.sizeDelta = new Vector2(200, 40);
            
            // 적 상태효과 영역
            GameObject enemyStatusEffects = new GameObject("EnemyStatusEffects");
            enemyStatusEffects.transform.SetParent(parent, false);
            RectTransform enemyStatusRect = enemyStatusEffects.AddComponent<RectTransform>();
            enemyStatusRect.anchoredPosition = new Vector2(200, -200);
            enemyStatusRect.sizeDelta = new Vector2(200, 40);
        }
        
        private void CreateActionButtons(Transform parent)
        {
            // 액티브 스킬 버튼
            GameObject activeSkillButton = new GameObject("ActiveSkillButton");
            activeSkillButton.transform.SetParent(parent, false);
            RectTransform activeSkillRect = activeSkillButton.AddComponent<RectTransform>();
            activeSkillRect.anchoredPosition = new Vector2(200, 0);
            activeSkillRect.sizeDelta = new Vector2(150, 40);
            Image activeSkillBg = activeSkillButton.AddComponent<Image>();
            activeSkillBg.color = new Color(0.8f, 0.8f, 0.2f);
            Button activeSkillBtn = activeSkillButton.AddComponent<Button>();
            ColorBlock colors = activeSkillBtn.colors;
            colors.normalColor = new Color(0.8f, 0.8f, 0.2f);
            activeSkillBtn.colors = colors;
            
            // 액티브 스킬 버튼 텍스트
            GameObject activeSkillText = new GameObject("Text");
            activeSkillText.transform.SetParent(activeSkillButton.transform, false);
            Text activeBtnText = activeSkillText.AddComponent<Text>();
            activeBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            activeBtnText.alignment = TextAnchor.MiddleCenter;
            activeBtnText.text = "액티브 스킬";
            activeBtnText.color = Color.black;
            RectTransform activeTextRect = activeSkillText.GetComponent<RectTransform>();
            activeTextRect.anchorMin = Vector2.zero;
            activeTextRect.anchorMax = Vector2.one;
            activeTextRect.offsetMin = Vector2.zero;
            activeTextRect.offsetMax = Vector2.zero;
            
            // 턴 종료 버튼
            GameObject endTurnButton = new GameObject("EndTurnButton");
            endTurnButton.transform.SetParent(parent, false);
            RectTransform endTurnRect = endTurnButton.AddComponent<RectTransform>();
            endTurnRect.anchoredPosition = new Vector2(200, -100);
            endTurnRect.sizeDelta = new Vector2(150, 40);
            Image endTurnBg = endTurnButton.AddComponent<Image>();
            endTurnBg.color = new Color(0.8f, 0.2f, 0.2f);
            Button endTurnBtn = endTurnButton.AddComponent<Button>();
            ColorBlock endTurnColors = endTurnBtn.colors;
            endTurnColors.normalColor = new Color(0.8f, 0.2f, 0.2f);
            endTurnBtn.colors = endTurnColors;
            
            // 턴 종료 버튼 텍스트
            GameObject endTurnText = new GameObject("Text");
            endTurnText.transform.SetParent(endTurnButton.transform, false);
            Text endTurnBtnText = endTurnText.AddComponent<Text>();
            endTurnBtnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            endTurnBtnText.alignment = TextAnchor.MiddleCenter;
            endTurnBtnText.text = "턴 종료";
            endTurnBtnText.color = Color.black;
            RectTransform endTurnTextRect = endTurnText.GetComponent<RectTransform>();
            endTurnTextRect.anchorMin = Vector2.zero;
            endTurnTextRect.anchorMax = Vector2.one;
            endTurnTextRect.offsetMin = Vector2.zero;
            endTurnTextRect.offsetMax = Vector2.zero;
        }
        
        private void InitializeManagerReferences()
        {
            Debug.Log("Initializing manager references...");
            
            // 상태효과 매니저 참조
            StatusEffectManager statusEffectManager = FindObjectOfType<StatusEffectManager>();
            if (statusEffectManager == null)
            {
                Debug.LogError("StatusEffectManager not found!");
            }
            
            // 기타 매니저 참조
            MasterGameManager gameManager = MasterGameManager.Instance; // 싱글톤 생성 보장
            CharacterManager characterManager = CharacterManager.Instance; // 싱글톤 생성 보장
            AIManager aiManager = AIManager.Instance; // 싱글톤 생성 보장
            
            // 모든 매니저가 존재하는지 확인
            if (gameManager == null || characterManager == null || aiManager == null)
            {
                Debug.LogError("One or more core singleton managers are missing!");
                return;
            }
            
            Debug.Log("All singleton managers initialized successfully!");
            Debug.Log("Manager references initialized successfully!");
        }
    }
}