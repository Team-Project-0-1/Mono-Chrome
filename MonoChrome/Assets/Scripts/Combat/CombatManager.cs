using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.StatusEffects;
using UnityEngine;


namespace MonoChrome.Combat
{
    /// <summary>
    /// 전투 시스템을 관리하는 클래스
    /// MonoChrome.Combat 네임스페이스로 이동된 최종 구현
    /// </summary>
    public class CombatManager : MonoBehaviour
    {
        #region Singleton
        private static CombatManager _instance;
        public static CombatManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<CombatManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("CombatManager");
                        _instance = obj.AddComponent<CombatManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            InitializeComponents();
        }
        #endregion
        
        [Header("Combat Parameters")]
        [SerializeField] private int _turnCount = 0;
        [SerializeField] private bool _isPlayerTurn = true;
        [SerializeField] private bool _isCombatActive = false;
        
        [Header("References")]
        private GameManager _gameManager;
        private UIManager _uiManager;
        private CoinManager _coinManager;
        private PatternManager _patternManager;
        private AIManager _aiManager;
        private StatusEffects.StatusEffectManager _statusEffectManager;
        
        [Header("Combat State")]
        private PlayerCharacter _playerCharacter;
        private EnemyCharacter _enemyCharacter;
        
        // Properties
        public int TurnCount => _turnCount;
        public bool IsPlayerTurn => _isPlayerTurn;
        public bool IsCombatActive => _isCombatActive;
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void InitializeComponents()
        {
            // CoinManager 확인/생성
            _coinManager = GetComponent<CoinManager>();
            if (_coinManager == null)
            {
                _coinManager = gameObject.AddComponent<CoinManager>();
                Debug.Log("CombatManager: Created CoinManager component");
            }
            
            // PatternManager 확인/생성
            _patternManager = GetComponent<PatternManager>();
            if (_patternManager == null)
            {
                _patternManager = gameObject.AddComponent<PatternManager>();
                Debug.Log("CombatManager: Created PatternManager component");
            }
            
            // StatusEffectManager 확인/생성
            _statusEffectManager = GetComponent<StatusEffects.StatusEffectManager>();
            if (_statusEffectManager == null)
            {
                _statusEffectManager = gameObject.AddComponent<StatusEffects.StatusEffectManager>();
                Debug.Log("CombatManager: Created StatusEffectManager component");
            }
        }
        
        private void Start()
        {
            GetManagerReferences();
        }
        
        /// <summary>
        /// 매니저 참조 가져오기
        /// </summary>
        private void GetManagerReferences()
        {
            // GameManager 참조
            _gameManager = GameManager.Instance;
            if (_gameManager == null)
            {
                Debug.LogError("CombatManager: GameManager instance is null");
                return;
            }
            
            // UIManager 참조
            _uiManager = FindObjectOfType<UIManager>();
            if (_uiManager == null && _gameManager != null)
            {
                _uiManager = _gameManager.UIManager;
                if (_uiManager == null)
                {
                    Debug.LogError("CombatManager: UIManager reference is null");
                }
            }
            
            // AIManager 참조
            _aiManager = AIManager.Instance;
            if (_aiManager == null)
            {
                Debug.LogError("CombatManager: AIManager instance is null");
            }
            
            Debug.Log("CombatManager: Manager references initialized");
        }
        
        /// <summary>
        /// 전투 초기화 - 게임 메니저에서 호출
        /// </summary>
        public void InitializeCombat()
        {
            try
            {
                Debug.Log("CombatManager: Initializing combat");
                
                // 전투 상태 초기화
                _turnCount = 1;
                _isPlayerTurn = true;
                _isCombatActive = true;
                
                // 캐릭터 생성 (임시 - 추후 캐릭터 매니저에서 관리)
                CreateTempCharacters();
                
                // 캐릭터 초기화
                if (_playerCharacter != null)
                {
                    _playerCharacter.ResetForCombat();
                }
                
                if (_enemyCharacter != null)
                {
                    _enemyCharacter.ResetForCombat();
                }
                
                // UI 업데이트
                UpdateCombatUI();
                
                // 첫 턴 시작
                StartCoroutine(StartTurn());
                
                Debug.Log("CombatManager: Combat initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.LogError($"CombatManager: Error initializing combat - {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 임시 캐릭터 생성 (추후 캐릭터 매니저로 대체)
        /// </summary>
        private void CreateTempCharacters()
        {
            // 플레이어 캐릭터 생성
            _playerCharacter = new PlayerCharacter(
                "모험가",
                SenseType.Auditory,
                100, // maxHealth
                10,  // attackPower
                5    // defensePower
            );
            
            // 적 캐릭터 생성
            _enemyCharacter = new EnemyCharacter(
                "루멘 리퍼",
                CharacterType.Normal,
                80,  // maxHealth
                8,   // attackPower
                3,   // defensePower
                StatusEffectType.Mark,      // primaryEffect
                StatusEffectType.Bleed      // secondaryEffect
            );
            
            Debug.Log("CombatManager: Temporary characters created");
        }
        
        /// <summary>
        /// 턴 시작
        /// </summary>
        private IEnumerator StartTurn()
        {
            if (!_isCombatActive)
            {
                Debug.LogWarning("CombatManager: Cannot start turn - combat is not active");
                yield break;
            }
            
            Debug.Log($"CombatManager: Starting turn {_turnCount} - Player turn: {_isPlayerTurn}");
            
            // UI 업데이트
            if (_uiManager != null)
            {
                _uiManager.UpdateTurnCounter(_turnCount);
            }
            
            // 동전 던지기
            _coinManager.FlipCoins();
            List<bool> coinResults = _coinManager.GetCoinResults();
            
            // UI에 코인 결과 표시
            if (_uiManager != null)
            {
                _uiManager.UpdateCoinUI(coinResults);
            }
            
            yield return new WaitForSeconds(0.5f);
            
            // 패턴 확인
            if (_isPlayerTurn)
            {
                // 플레이어 턴
                List<Pattern> availablePatterns = _patternManager.DetermineAvailablePatterns(
                    coinResults, _playerCharacter.GetAvailablePatterns()
                );
                
                // UI에 패턴 선택지 표시
                if (_uiManager != null)
                {
                    _uiManager.UpdatePatternUI(availablePatterns);
                }
                
                // 액티브 스킬 버튼 업데이트
                if (_uiManager != null)
                {
                    _uiManager.UpdateActiveSkillButton(_playerCharacter.IsActiveSkillAvailable());
                }
                
                Debug.Log($"CombatManager: Player's turn - awaiting pattern selection. {availablePatterns.Count} patterns available");
                
                // 플레이어의 선택 대기 (UI 버튼 클릭으로 SelectPattern이 호출됨)
            }
            else
            {
                // 적 턴
                yield return StartCoroutine(ExecuteEnemyTurn());
            }
        }
        
        /// <summary>
        /// 패턴 선택 (플레이어가 UI를 통해 호출)
        /// </summary>
        public void SelectPattern(Pattern pattern)
        {
            if (!_isCombatActive || !_isPlayerTurn)
            {
                Debug.LogWarning("CombatManager: Cannot select pattern - not player's turn or combat inactive");
                return;
            }
            
            if (pattern == null)
            {
                Debug.LogError("CombatManager: Selected pattern is null");
                return;
            }
            
            Debug.Log($"CombatManager: Player selected pattern: {pattern.Name}");
            
            StartCoroutine(ExecutePlayerPattern(pattern));
        }
        
        /// <summary>
        /// 플레이어 패턴 실행
        /// </summary>
        private IEnumerator ExecutePlayerPattern(Pattern pattern)
        {
            // 패턴 효과 적용
            ApplyPatternEffects(pattern, _playerCharacter, _enemyCharacter);
            
            // 약간의 지연
            yield return new WaitForSeconds(0.5f);
            
            // 전투 결과 확인
            if (CheckCombatResult())
            {
                yield break;
            }
            
            // 플레이어 턴 종료
            FinishPlayerTurn();
        }
        
        /// <summary>
        /// 적 턴 실행
        /// </summary>
        private IEnumerator ExecuteEnemyTurn()
        {
            Debug.Log("CombatManager: Executing enemy turn");
            
            // AI가 패턴 선택
            Pattern selectedPattern = _aiManager.SelectPattern(_enemyCharacter, _playerCharacter);
            
            if (selectedPattern == null)
            {
                Debug.LogError("CombatManager: AI selected null pattern");
                // 기본 패턴 생성
                selectedPattern = new Pattern
                {
                    Name = "기본 공격",
                    Description = "약한 공격",
                    ID = 1,
                    IsAttack = true,
                    PatternType = PatternType.Consecutive2,
                    PatternValue = true,
                    AttackBonus = 1
                };
            }
            
            // 의도 표시
            if (_uiManager != null)
            {
                _uiManager.ShowEnemyIntention(selectedPattern);
            }
            
            // 지연
            yield return new WaitForSeconds(1.0f);
            
            // 패턴 적용
            ApplyPatternEffects(selectedPattern, _enemyCharacter, _playerCharacter);
            
            // 패턴 사용 기록
            _enemyCharacter.RecordPatternUse(selectedPattern);
            
            // 전투 결과 확인
            if (CheckCombatResult())
            {
                yield break;
            }
            
            // 지연
            yield return new WaitForSeconds(0.5f);
            
            // 턴 종료
            FinishEnemyTurn();
        }
        
        /// <summary>
        /// 패턴 효과 적용
        /// </summary>
        private void ApplyPatternEffects(Pattern pattern, Character source, Character target)
        {
            try
            {
                if (pattern == null || source == null || target == null)
                {
                    Debug.LogError("CombatManager: Cannot apply pattern effects - null parameters");
                    return;
                }
                
                Debug.Log($"CombatManager: Applying pattern '{pattern.Name}' effects from {source.CharacterName} to {target.CharacterName}");
                
                if (pattern.IsAttack)
                {
                    // 공격 패턴
                    int damage = source.AttackPower + pattern.AttackBonus;
                    target.TakeDamage(damage);
                    
                    Debug.Log($"CombatManager: Applied {damage} damage to {target.CharacterName}");
                }
                else
                {
                    // 방어 패턴
                    int defense = pattern.DefenseBonus;
                    source.AddDefense(defense);
                    
                    Debug.Log($"CombatManager: Added {defense} defense to {source.CharacterName}");
                }
                
                // 상태 효과 적용
                if (pattern.StatusEffects != null && pattern.StatusEffects.Length > 0)
                {
                    foreach (StatusEffect.StatusEffectData effectData in pattern.StatusEffects)
                    {
                        // 공격 패턴이면 대상에게, 방어 패턴이면 자신에게 효과 적용
                        Character effectTarget = pattern.IsAttack ? target : source;
                        
                        StatusEffects.StatusEffect effect = new StatusEffects.StatusEffect(
                            effectData.EffectType,
                            effectData.Magnitude,
                            effectData.Duration,
                            source
                        );
                        
                        effectTarget.AddStatusEffect(effect);
                        
                        Debug.Log($"CombatManager: Applied {effect.EffectType} effect to {effectTarget.CharacterName}");
                    }
                }
                
                // UI 업데이트
                UpdateCombatUI();
            }
            catch (Exception ex)
            {
                Debug.LogError($"CombatManager: Error applying pattern effects - {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 액티브 스킬 사용 (플레이어 UI에서 호출)
        /// </summary>
        public void UseActiveSkill()
        {
            if (!_isCombatActive || !_isPlayerTurn)
            {
                Debug.LogWarning("CombatManager: Cannot use active skill - not player's turn or combat inactive");
                return;
            }
            
            if (_playerCharacter == null)
            {
                Debug.LogError("CombatManager: Player character is null");
                return;
            }
            
            if (!_playerCharacter.IsActiveSkillAvailable())
            {
                Debug.LogWarning("CombatManager: Active skill is not available");
                return;
            }
            
            // 액티브 스킬 사용
            _playerCharacter.UseActiveSkill(_coinManager);
            
            // UI 업데이트
            _coinManager.FlipCoins(); // 스킬에 따라 코인 상태가 변경될 수 있음
            List<bool> coinResults = _coinManager.GetCoinResults();
            
            if (_uiManager != null)
            {
                _uiManager.UpdateCoinUI(coinResults);
                _uiManager.UpdateActiveSkillButton(false); // 스킬 사용 후 버튼 비활성화
            }
            
            // 새로운 패턴 계산
            List<Pattern> availablePatterns = _patternManager.DetermineAvailablePatterns(
                coinResults, _playerCharacter.GetAvailablePatterns()
            );
            
            // UI에 새 패턴 선택지 표시
            if (_uiManager != null)
            {
                _uiManager.UpdatePatternUI(availablePatterns);
            }
            
            Debug.Log("CombatManager: Player used active skill");
        }
        
        /// <summary>
        /// 플레이어 턴 종료 (UI 버튼에서 호출하거나 패턴 선택 후 자동 호출)
        /// </summary>
        public void FinishPlayerTurn()
        {
            if (!_isCombatActive || !_isPlayerTurn)
            {
                Debug.LogWarning("CombatManager: Cannot finish player's turn - not player's turn or combat inactive");
                return;
            }
            
            Debug.Log("CombatManager: Finishing player's turn");
            
            // 플레이어 상태 효과 업데이트
            if (_playerCharacter != null)
            {
                _playerCharacter.UpdateStatusEffects();
                _playerCharacter.UpdateSkillCooldown();
            }
            
            // 적 상태 효과 업데이트
            if (_enemyCharacter != null)
            {
                _enemyCharacter.UpdateStatusEffects();
            }
            
            // 전투 결과 확인
            if (CheckCombatResult())
            {
                return;
            }
            
            // 턴 전환
            _isPlayerTurn = false;
            
            // UI 업데이트
            UpdateCombatUI();
            
            // 적 턴 시작
            StartCoroutine(StartTurn());
        }
        
        /// <summary>
        /// 적 턴 종료
        /// </summary>
        private void FinishEnemyTurn()
        {
            if (!_isCombatActive || _isPlayerTurn)
            {
                Debug.LogWarning("CombatManager: Cannot finish enemy's turn - not enemy's turn or combat inactive");
                return;
            }
            
            Debug.Log("CombatManager: Finishing enemy's turn");
            
            // 턴 카운터 증가
            _turnCount++;
            
            // 턴 전환
            _isPlayerTurn = true;
            
            // UI 업데이트
            UpdateCombatUI();
            
            // 플레이어 턴 시작
            StartCoroutine(StartTurn());
        }
        
        /// <summary>
        /// 전투 결과 확인 (승리/패배)
        /// </summary>
        private bool CheckCombatResult()
        {
            if (_playerCharacter == null || _enemyCharacter == null)
            {
                Debug.LogError("CombatManager: Cannot check combat result - null characters");
                return false;
            }
            
            if (_playerCharacter.CurrentHealth <= 0)
            {
                // 플레이어 패배
                EndCombat(false);
                return true;
            }
            
            if (_enemyCharacter.CurrentHealth <= 0)
            {
                // 플레이어 승리
                EndCombat(true);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 전투 종료
        /// </summary>
        private void EndCombat(bool victory)
        {
            _isCombatActive = false;
            
            Debug.Log($"CombatManager: Combat ended - Player victory: {victory}");
            
            // 게임 매니저에 결과 알림
            if (_gameManager != null)
            {
                _gameManager.EndCombat(victory);
            }
            else
            {
                Debug.LogError("CombatManager: Cannot end combat - GameManager is null");
            }
        }
        
        /// <summary>
        /// UI 업데이트
        /// </summary>
        private void UpdateCombatUI()
        {
            if (_uiManager == null)
            {
                Debug.LogError("CombatManager: Cannot update UI - UIManager is null");
                return;
            }
            
            try
            {
                // 체력바 업데이트
                if (_playerCharacter != null && _enemyCharacter != null)
                {
                    _uiManager.UpdateHealthBars(
                        _playerCharacter.CurrentHealth,
                        _playerCharacter.MaxHealth,
                        _enemyCharacter.CurrentHealth,
                        _enemyCharacter.MaxHealth
                    );
                }
                
                // 상태 효과 업데이트
                if (_playerCharacter != null && _enemyCharacter != null)
                {
                    _uiManager.UpdateStatusEffects(
                        _playerCharacter.GetAllStatusEffects(),
                        _enemyCharacter.GetAllStatusEffects()
                    );
                }
                
                // 턴 카운터 업데이트
                _uiManager.UpdateTurnCounter(_turnCount);
                
                // 액티브 스킬 버튼 업데이트
                if (_playerCharacter != null)
                {
                    _uiManager.UpdateActiveSkillButton(_playerCharacter.IsActiveSkillAvailable() && _isPlayerTurn);
                }
                
                Debug.Log("CombatManager: Combat UI updated");
            }
            catch (Exception ex)
            {
                Debug.LogError($"CombatManager: Error updating UI - {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// 플레이어 캐릭터 설정 (CharacterManager에서 호출)
        /// </summary>
        public void SetPlayerCharacter(PlayerCharacter character)
        {
            _playerCharacter = character;
            Debug.Log($"CombatManager: Player character set to {character?.CharacterName ?? "null"}");
        }
        
        /// <summary>
        /// 적 캐릭터 설정 (DungeonManager에서 호출)
        /// </summary>
        public void SetEnemyCharacter(EnemyCharacter character)
        {
            _enemyCharacter = character;
            Debug.Log($"CombatManager: Enemy character set to {character?.CharacterName ?? "null"}");
        }
        
        /// <summary>
        /// 전투 시작 (RoomData 활용)
        /// </summary>
        public void StartCombat(RoomData roomData, bool isMiniBoss = false, bool isBoss = false)
        {
            Debug.Log($"Starting combat. Room difficulty: {roomData.difficulty}, MiniBoss: {isMiniBoss}, Boss: {isBoss}");
            
            // RoomData를 DungeonNode로 변환
            DungeonNode node = roomData.ToDungeonNode();
            
            // DungeonNode 오버로드 호출
            StartCombat(node, isMiniBoss, isBoss);
        }

        /// <summary>
        /// 전투 시작 (DungeonNode 활용)
        /// </summary>
        public void StartCombat(DungeonNode node, bool isMiniBoss = false, bool isBoss = false)
        {
            Debug.Log($"Starting combat. Node type: {node.Type}, MiniBoss: {isMiniBoss}, Boss: {isBoss}");
            
            // 적 생성
            string enemyName = "루멘 리퍼";
            CharacterType characterType = CharacterType.Normal;
            
            if (isBoss)
            {
                enemyName = "검은 심연";
                characterType = CharacterType.Boss;
            }
            else if (isMiniBoss)
            {
                enemyName = "그림자 수호자";
                characterType = CharacterType.MiniBoss;
            }
            
            // 캐릭터 매니저를 통해 적 생성
            var characterManager = CharacterManager.Instance;
            if (characterManager != null)
            {
                var enemy = characterManager.CreateEnemyCharacter(enemyName, characterType);
                
                // 실제 CombatManager에 적 설정
                SetEnemyCharacter(enemy);
            }
            
            // 전투 시작
            InitializeCombat();
        }
    }
}