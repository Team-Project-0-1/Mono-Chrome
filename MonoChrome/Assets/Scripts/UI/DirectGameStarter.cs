using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection;

namespace MonoChrome.UI
{
    /// <summary>
    /// 개선된 게임 시작 버튼 핸들러
    /// 던전 생성 및 UI 전환을 확실히 처리하도록 강화
    /// </summary>
    public class DirectGameStarter : MonoBehaviour
    {
        [SerializeField] private Button startGameButton;
        [SerializeField] private bool enableDebugMode = true;
        [SerializeField] private float uiTransitionDelay = 0.5f;
        
        // UI 패널 참조들
        [SerializeField] private GameObject characterSelectionPanel;
        [SerializeField] private GameObject dungeonPanel;
        
        private void Start()
        {
            LogDebug("DirectGameStarter: Initializing...");
            
            FindUIReferences();
            SetupEventHandler();
            
            LogDebug("DirectGameStarter: Initialization completed");
        }
        
        /// <summary>
        /// UI 참조 찾기
        /// </summary>
        private void FindUIReferences()
        {
            // 버튼 참조 찾기
            if (startGameButton == null)
            {
                startGameButton = GetComponent<Button>();
                
                if (startGameButton == null)
                {
                    startGameButton = transform.parent?.GetComponentInChildren<Button>();
                    
                    if (startGameButton == null)
                    {
                        startGameButton = GameObject.Find("StartGameButton")?.GetComponent<Button>();
                    }
                }
            }
            
            // UI 패널 참조 찾기
            if (characterSelectionPanel == null)
            {
                // Canvas에서 CharacterSelectionPanel 찾기
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    characterSelectionPanel = canvas.transform.Find("CharacterSelectionPanel")?.gameObject;
                }
                
                if (characterSelectionPanel == null)
                {
                    characterSelectionPanel = GameObject.Find("CharacterSelectionPanel");
                }
            }
            
            if (dungeonPanel == null)
            {
                // Canvas에서 DungeonPanel 찾기
                Canvas canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    dungeonPanel = canvas.transform.Find("DungeonPanel")?.gameObject;
                }
                
                if (dungeonPanel == null)
                {
                    dungeonPanel = GameObject.Find("DungeonPanel");
                }
            }
            
            // 로그 출력
            LogDebug($"Start Game Button found: {startGameButton != null}");
            LogDebug($"Character Selection Panel found: {characterSelectionPanel != null}");
            LogDebug($"Dungeon Panel found: {dungeonPanel != null}");
        }
        
        /// <summary>
        /// 이벤트 핸들러 설정
        /// </summary>
        private void SetupEventHandler()
        {
            if (startGameButton != null)
            {
                // 기존 이벤트 리스너 제거
                startGameButton.onClick.RemoveAllListeners();
                
                // 새 이벤트 리스너 추가
                startGameButton.onClick.AddListener(OnStartGameButtonClicked);
                LogDebug("DirectGameStarter: Start Game Button event handler configured");
            }
            else
            {
                LogError("DirectGameStarter: Failed to find Start Game Button!");
            }
        }
        
        /// <summary>
        /// 게임 시작 버튼 클릭 처리
        /// </summary>
        private void OnStartGameButtonClicked()
        {
            LogDebug("DirectGameStarter: Start Game Button clicked!");
            
            try
            {
                // 코루틴으로 순차적 처리
                StartCoroutine(StartGameSequence());
            }
            catch (System.Exception ex)
            {
                LogError($"DirectGameStarter: Error in OnStartGameButtonClicked: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 게임 시작 시퀀스 (코루틴) - 개선된 버전
        /// </summary>
        private IEnumerator StartGameSequence()
        {
            LogDebug("DirectGameStarter: Starting improved game sequence...");
            
            // 1. GameManager 확인
            if (GameManager.Instance == null)
            {
                LogError("DirectGameStarter: GameManager.Instance is null!");
                yield break;
            }
            
            // 2. 매니저 활성화 선행 처리
            LogDebug("DirectGameStarter: Ensuring all managers are active...");
            yield return StartCoroutine(EnsureManagersAreActive());
            
            // 3. 매니저 참조 강제 갱신
            LogDebug("DirectGameStarter: Forcing manager reference refresh...");
            GameManager.Instance.GetType().GetMethod("RefreshManagerReferences", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(GameManager.Instance, null);
            
            // 4. 캐릭터 생성 (기본 청각 타입)
            LogDebug("DirectGameStarter: Creating player character...");
            if (CharacterManager.Instance != null)
            {
                CharacterManager.Instance.CreatePlayerCharacter(SenseType.Auditory);
                LogDebug("DirectGameStarter: Player character created (Auditory type)");
            }
            else
            {
                LogError("DirectGameStarter: CharacterManager.Instance is null!");
            }
            
            // 5. UI 패널 전환
            LogDebug("DirectGameStarter: Switching UI panels...");
            SwitchUIToDungeonPanel();
            
            // UI 전환 완료 대기
            yield return new WaitForSeconds(uiTransitionDelay);
            
            // 6. 게임 상태 변경
            LogDebug("DirectGameStarter: Changing game state to Dungeon...");
            GameManager.Instance.ChangeState(GameManager.GameState.Dungeon);
            
            // 7. 던전 생성 강제 실행
            LogDebug("DirectGameStarter: Forcing dungeon generation...");
            yield return StartCoroutine(ForceDungeonGeneration());
            
            LogDebug("DirectGameStarter: Game start sequence completed successfully!");
        }
        
        /// <summary>
        /// 모든 매니저가 활성화되어 있는지 확인하고 필요시 활성화
        /// </summary>
        private IEnumerator EnsureManagersAreActive()
        {
            var managersToActivate = new[]
            {
                "CombatManager", "CoinManager", "PatternManager", "StatusEffectManager", "DungeonManager"
            };
            
            int activatedCount = 0;
            
            foreach (string managerName in managersToActivate)
            {
                GameObject managerObj = GameObject.Find(managerName);
                
                if (managerObj != null)
                {
                    if (!managerObj.activeInHierarchy)
                    {
                        managerObj.SetActive(true);
                        activatedCount++;
                        LogDebug($"Activated {managerName}");
                        
                        // 약간의 대기 시간 제공
                        yield return new WaitForSeconds(0.1f);
                    }
                    else
                    {
                        LogDebug($"{managerName} already active");
                    }
                }
                else
                {
                    LogWarning($"{managerName} not found in scene");
                }
            }
            
            LogDebug($"Ensured {activatedCount} managers are active");
            
            // 매니저 활성화 후 추가 대기
            yield return new WaitForSeconds(0.2f);
        }
        
        /// <summary>
        /// UI 패널을 던전 패널로 전환
        /// </summary>
        private void SwitchUIToDungeonPanel()
        {
            try
            {
                // 캐릭터 선택 패널 비활성화
                if (characterSelectionPanel != null)
                {
                    characterSelectionPanel.SetActive(false);
                    LogDebug("DirectGameStarter: Character selection panel deactivated");
                }
                
                // 던전 패널 활성화
                if (dungeonPanel != null)
                {
                    dungeonPanel.SetActive(true);
                    LogDebug("DirectGameStarter: Dungeon panel activated");
                }
                
                // UIManager를 통한 패널 전환도 시도
                if (GameManager.Instance?.UIManager != null)
                {
                    GameManager.Instance.UIManager.OnPanelSwitched("DungeonPanel");
                    LogDebug("DirectGameStarter: UIManager panel switch executed");
                }
            }
            catch (System.Exception ex)
            {
                LogError($"DirectGameStarter: Error switching UI panels: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 던전 생성 강제 실행
        /// </summary>
        private IEnumerator ForceDungeonGeneration()
        {
            Dungeon.DungeonManager dungeonManager = null;
            float timeout = 5f;
            float elapsed = 0f;

            while (dungeonManager == null && elapsed < timeout)
            {
                dungeonManager = Dungeon.DungeonManager.Instance ?? 
                                 FindObjectOfType<Dungeon.DungeonManager>();

                if (dungeonManager == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    elapsed += 0.1f;
                }
            }

            if (dungeonManager == null)
            {
                LogError("DirectGameStarter: DungeonManager not found after timeout!");
                yield break;
            }

            LogDebug("DirectGameStarter: DungeonManager found, generating dungeon...");

            bool hasError = false;
            string errorMessage = null;

            try
            {
                dungeonManager.GenerateNewDungeon(0);
                LogDebug("DirectGameStarter: Dungeon generation requested successfully");
            }
            catch (System.Exception ex)
            {
                hasError = true;
                errorMessage = $"DirectGameStarter: Error during dungeon generation: {ex.Message}\n{ex.StackTrace}";
            }

            if (hasError)
            {
                LogError(errorMessage);
                yield break; // 혹은 다른 후속 처리
            }

            yield return new WaitForSeconds(1f);

            var dungeonUI = FindObjectOfType<DungeonUI>();
            if (dungeonUI != null)
            {
                LogDebug("DirectGameStarter: DungeonUI found and ready");
            }
            else
            {
                LogWarning("DirectGameStarter: DungeonUI not found");
            }
        }

        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugMode)
            {
                Debug.Log($"[DirectGameStarter] {message}");
            }
        }
        
        /// <summary>
        /// 에러 로그
        /// </summary>
        private void LogError(string message)
        {
            Debug.LogError($"[DirectGameStarter] {message}");
        }
        
        /// <summary>
        /// 경고 로그
        /// </summary>
        private void LogWarning(string message)
        {
            Debug.LogWarning($"[DirectGameStarter] {message}");
        }
    }
}