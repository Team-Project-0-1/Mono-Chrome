using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using MonoChrome.StatusEffects;
using MonoChrome.UI;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 개선된 UI 매니저 - 정적 UI 시스템 통합
    /// 포트폴리오 품질을 위한 완전한 UI 관리 시스템
    /// </summary>
    public class ImprovedUIManager : MonoBehaviour
    {
        #region UI Controllers
        [Header("UI 컨트롤러들")]
        [SerializeField] private CombatUIController combatUIController;
        [SerializeField] private DungeonUI dungeonUI;
        
        [Header("패널들")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject dungeonPanel;
        [SerializeField] private GameObject combatPanel;
        [SerializeField] private GameObject gameOverPanel;
        #endregion
        
        #region Manager References
        private GameManager _gameManager;
        private bool _isInitialized = false;
        #endregion
        
        #region Initialization
        private void Awake()
        {
            ValidateUIReferences();
        }
        
        private void Start()
        {
            StartCoroutine(DelayedInitialization());
        }
        
        /// <summary>
        /// UI 참조 검증
        /// </summary>
        private void ValidateUIReferences()
        {
            // CombatUIController 자동 검색
            if (combatUIController == null)
            {
                combatUIController = FindObjectOfType<CombatUIController>();
                if (combatUIController == null)
                {
                    Debug.LogWarning("ImprovedUIManager: CombatUIController를 찾을 수 없습니다. Scene에 추가하거나 인스펙터에서 할당하세요.");
                }
            }
            
            // DungeonUI 자동 검색
            if (dungeonUI == null)
            {
                dungeonUI = FindObjectOfType<DungeonUI>();
            }
            
            // 패널들 자동 검색
            if (mainMenuPanel == null)
            {
                GameObject found = GameObject.Find("MainMenuPanel");
                if (found != null) mainMenuPanel = found;
            }
            
            if (characterSelectionPanel == null)
            {
                GameObject found = GameObject.Find("CharacterSelectionPanel");
                if (found != null) characterSelectionPanel = found;
            }
            
            if (dungeonPanel == null)
            {
                GameObject found = GameObject.Find("DungeonPanel");
                if (found != null) dungeonPanel = found;
            }
            
            if (combatPanel == null)
            {
                GameObject found = GameObject.Find("CombatPanel");
                if (found != null) combatPanel = found;
            }
        }
        
        /// <summary>
        /// 지연된 초기화
        /// </summary>
        private IEnumerator DelayedInitialization()
        {
            // GameManager 대기
            while (GameManager.Instance == null)
            {
                yield return null;
            }
            
            _gameManager = GameManager.Instance;
            
            // UI 컨트롤러들 초기화
            InitializeUIControllers();
            
            // 게임 상태 이벤트 구독
            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged += OnGameStateChanged;
            }
            
            // 초기 UI 상태 설정
            SetInitialUIState();
            
            _isInitialized = true;
            Debug.Log("ImprovedUIManager: 초기화 완료");
        }
        
        /// <summary>
        /// UI 컨트롤러들 초기화
        /// </summary>
        private void InitializeUIControllers()
        {
            if (combatUIController != null)
            {
                combatUIController.ResetUI();
            }
            
            if (dungeonUI != null)
            {
                dungeonUI.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// 초기 UI 상태 설정
        /// </summary>
        private void SetInitialUIState()
        {
            // 모든 패널 비활성화
            SetAllPanelsActive(false);
            
            // 메인 메뉴만 활성화
            if (mainMenuPanel != null)
            {
                mainMenuPanel.SetActive(true);
            }
        }
        
        /// <summary>
        /// 모든 패널 활성화/비활성화
        /// </summary>
        private void SetAllPanelsActive(bool active)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(active);
            if (characterSelectionPanel != null) characterSelectionPanel.SetActive(active);
            if (dungeonPanel != null) dungeonPanel.SetActive(active);
            if (combatPanel != null) combatPanel.SetActive(active);
            if (gameOverPanel != null) gameOverPanel.SetActive(active);
        }
        #endregion
        
        #region Game State Management
        /// <summary>
        /// 게임 상태 변경 이벤트
        /// </summary>
        private void OnGameStateChanged(GameManager.GameState newState)
        {
            if (!_isInitialized) return;
            
            Debug.Log($"ImprovedUIManager: 게임 상태 변경 - {newState}");
            
            // 모든 패널 비활성화
            SetAllPanelsActive(false);
            
            // 상태에 따른 UI 활성화
            switch (newState)
            {
                case GameManager.GameState.MainMenu:
                    ShowMainMenu();
                    break;
                    
                case GameManager.GameState.CharacterSelection:
                    ShowCharacterSelection();
                    break;
                    
                case GameManager.GameState.Dungeon:
                    ShowDungeon();
                    break;
                    
                case GameManager.GameState.Combat:
                    ShowCombat();
                    break;
                    
                case GameManager.GameState.GameOver:
                    ShowGameOver();
                    break;
            }
        }
        
        private void ShowMainMenu()
        {
            if (mainMenuPanel != null)
                mainMenuPanel.SetActive(true);
        }
        
        private void ShowCharacterSelection()
        {
            if (characterSelectionPanel != null)
                characterSelectionPanel.SetActive(true);
        }
        
        private void ShowDungeon()
        {
            if (dungeonPanel != null)
                dungeonPanel.SetActive(true);
                
            if (dungeonUI != null)
                dungeonUI.gameObject.SetActive(true);
        }
        
        private void ShowCombat()
        {
            if (combatPanel != null)
                combatPanel.SetActive(true);
                
            if (combatUIController != null)
                combatUIController.gameObject.SetActive(true);
        }
        
        private void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);
        }
        #endregion
        
        #region Combat UI Interface
        /// <summary>
        /// 전투 UI 초기화
        /// </summary>
        public void InitializeCombatUI()
        {
            if (combatUIController != null)
            {
                combatUIController.ResetUI();
                Debug.Log("ImprovedUIManager: 전투 UI 초기화");
            }
            else
            {
                Debug.LogError("ImprovedUIManager: CombatUIController가 할당되지 않았습니다.");
            }
        }
        
        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            if (combatUIController != null)
            {
                combatUIController.UpdateHealthBars(playerHealth, playerMaxHealth, enemyHealth, enemyMaxHealth);
            }
        }
        
        /// <summary>
        /// 동전 UI 업데이트
        /// </summary>
        public void UpdateCoinUI(List<bool> coinResults)
        {
            if (combatUIController != null)
            {
                combatUIController.UpdateCoins(coinResults);
            }
        }
        
        /// <summary>
        /// 패턴 UI 업데이트
        /// </summary>
        public void UpdatePatternUI(List<Pattern> availablePatterns)
        {
            if (combatUIController != null)
            {
                combatUIController.UpdatePatterns(availablePatterns);
            }
        }
        
        /// <summary>
        /// 턴 카운터 업데이트
        /// </summary>
        public void UpdateTurnCounter(int turnCount)
        {
            if (combatUIController != null)
            {
                combatUIController.UpdateTurnCounter(turnCount);
            }
        }
        
        /// <summary>
        /// 액티브 스킬 버튼 업데이트
        /// </summary>
        public void UpdateActiveSkillButton(bool isAvailable, string skillName = "액티브 스킬")
        {
            if (combatUIController != null)
            {
                combatUIController.UpdateActiveSkillButton(isAvailable, skillName);
            }
        }
        
        /// <summary>
        /// 적 의도 표시
        /// </summary>
        public void ShowEnemyIntention(Pattern pattern)
        {
            if (combatUIController != null && pattern != null)
            {
                string intention = $"{pattern.Name}: {pattern.Description}";
                combatUIController.ShowEnemyIntention(intention);
            }
        }
        
        /// <summary>
        /// 상태 효과 UI 업데이트
        /// </summary>
        public void UpdateStatusEffects(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            if (combatUIController != null)
            {
                combatUIController.UpdateStatusEffects(playerEffects, enemyEffects);
            }
        }
        #endregion
        
        #region Dungeon UI Interface
        /// <summary>
        /// 던전 맵 업데이트
        /// </summary>
        public void UpdateDungeonMap(List<DungeonNode> nodes, int currentNodeId)
        {
            if (dungeonUI != null)
            {
                dungeonUI.UpdateDungeonMap(nodes, currentNodeId);
            }
        }
        
        /// <summary>
        /// 던전 UI 초기화
        /// </summary>
        public void InitializeDungeonUI()
        {
            if (dungeonUI != null)
            {
                dungeonUI.gameObject.SetActive(true);
                Debug.Log("ImprovedUIManager: 던전 UI 초기화");
            }
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// UI 컨트롤러 수동 할당 (에디터 또는 코드에서 호출)
        /// </summary>
        public void AssignUIController(CombatUIController controller)
        {
            combatUIController = controller;
            Debug.Log("ImprovedUIManager: CombatUIController 할당됨");
        }
        
        /// <summary>
        /// 현재 활성화된 UI 상태 반환
        /// </summary>
        public string GetCurrentActiveUI()
        {
            if (mainMenuPanel != null && mainMenuPanel.activeSelf) return "MainMenu";
            if (characterSelectionPanel != null && characterSelectionPanel.activeSelf) return "CharacterSelection";
            if (dungeonPanel != null && dungeonPanel.activeSelf) return "Dungeon";
            if (combatPanel != null && combatPanel.activeSelf) return "Combat";
            if (gameOverPanel != null && gameOverPanel.activeSelf) return "GameOver";
            return "None";
        }
        #endregion
        
        #region Cleanup
        private void OnDestroy()
        {
            // 이벤트 구독 해제
            if (_gameManager != null)
            {
                _gameManager.OnGameStateChanged -= OnGameStateChanged;
            }
        }
        #endregion
    }
}
