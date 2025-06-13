using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Core.Data;
using UnityEngine;
using MonoChrome.Events;
using MonoChrome.StatusEffects;
using Random = UnityEngine.Random;

namespace MonoChrome.Systems.Combat
{
    /// <summary>
    /// 개선된 전투 시스템 - 순수하게 전투 로직만 담당
    /// 
    /// 책임:
    /// - 전투 진행 로직 (턴 관리, 패턴 실행)
    /// - 전투 상태 추적
    /// - 승부 판정
    /// 
    /// 책임 제외:
    /// - UI 업데이트 (이벤트로 위임)
    /// - 캐릭터 생성 (팩토리로 위임)
    /// - 매니저 참조 관리 (의존성 최소화)
    /// </summary>
    public class CombatSystem : MonoBehaviour
    {
        #region Singleton
        private static CombatSystem _instance;
        public static CombatSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CombatSystem>();
                    if (_instance == null)
                    {
                        var go = new GameObject("[CombatSystem]");
                        _instance = go.AddComponent<CombatSystem>();
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
                InitializeSubSystems();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        #endregion

        #region Combat State (단순화된 상태)
        [Header("전투 상태")]
        [SerializeField] private bool _isCombatActive = false;
        [SerializeField] private bool _isPlayerTurn = true;
        [SerializeField] private int _turnCount = 1;

        // 전투 참가자 (외부에서 설정)
        private PlayerCharacter _player;
        private EnemyCharacter _enemy;

        // 통합된 전투 시스템 - 직접 관리
        [Header("Coin System")]
        [SerializeField] private int _coinCount = 5;
        private bool[] _coinStates;
        private bool[] _lockedCoins;
        
        [Header("Pattern System")]
        [SerializeField] private List<PatternSO> _availablePatterns;
        #endregion

        #region Events (UI와 분리를 위한 이벤트들)
        public static event Action<PlayerCharacter, EnemyCharacter> OnCombatStarted;
        public static event Action<bool> OnCombatEnded; // 승리 여부
        public static event Action<int> OnTurnChanged;
        public static event Action<List<Pattern>, List<bool>> OnPlayerTurnStarted; // 패턴, 코인
        public static event Action<Pattern> OnPatternExecuted;
        public static event Action<Character, int> OnDamageDealt; // 대상, 피해량
        public static event Action<Character, int> OnDefenseAdded; // 대상, 방어량
        #endregion

        #region Initialization
        private void InitializeSubSystems()
        {
            // 코인 시스템 초기화
            InitializeCoinSystem();
            
            // 패턴 시스템 초기화
            InitializePatternSystem();

            // 이벤트 구독
            SubscribeToEvents();
        }
        
        /// <summary>
        /// 전투 시스템 공개 초기화 (MasterGameManager용)
        /// </summary>
        public void InitializeCombat()
        {
            Debug.Log("CombatSystem: Public initialization called");
            
            // 서브시스템이 이미 초기화되었는지 확인
            if (_coinStates == null || _lockedCoins == null)
            {
                InitializeSubSystems();
            }
            
            // 전투 시스템이 준비되었음을 알림
            Debug.Log("CombatSystem: Ready for combat");
        }
        
        private void InitializeCoinSystem()
        {
            _coinStates = new bool[_coinCount];
            _lockedCoins = new bool[_coinCount];
            
            for (int i = 0; i < _coinCount; i++)
            {
                _lockedCoins[i] = false;
            }
            
            Debug.Log($"CombatSystem: Coin system initialized with {_coinCount} coins");
        }
        
        private void InitializePatternSystem()
        {
            if (_availablePatterns == null)
            {
                _availablePatterns = new List<PatternSO>();
            }
            
            Debug.Log("CombatSystem: Pattern system initialized");
        }
        
        #region Coin Management
        /// <summary>
        /// 모든 동전 던지기
        /// </summary>
        public void FlipCoins()
        {
            Debug.Log("CombatSystem: Flipping coins");
            
            for (int i = 0; i < _coinCount; i++)
            {
                if (!_lockedCoins[i])
                {
                    _coinStates[i] = Random.Range(0, 2) == 0; // 50% 확률
                }
            }
        }
        
        /// <summary>
        /// 현재 코인 결과 반환
        /// </summary>
        public List<bool> GetCoinResults()
        {
            var results = new List<bool>();
            for (int i = 0; i < _coinCount; i++)
            {
                results.Add(_coinStates[i]);
            }
            return results;
        }
        
        /// <summary>
        /// 특정 코인 고정/해제
        /// </summary>
        public void SetCoinLocked(int index, bool locked)
        {
            if (index >= 0 && index < _coinCount)
            {
                _lockedCoins[index] = locked;
            }
        }
        
        /// <summary>
        /// 특정 코인 상태 변경
        /// </summary>
        public void SetCoinState(int index, bool state)
        {
            if (index >= 0 && index < _coinCount)
            {
                _coinStates[index] = state;
            }
        }
        
        /// <summary>
        /// 모든 코인 다시 던지기 (액티브 스킬용)
        /// </summary>
        public void RethrowAllCoins()
        {
            Debug.Log("CombatSystem: Rethrowing all coins");
            
            for (int i = 0; i < _coinCount; i++)
            {
                if (!_lockedCoins[i])
                {
                    _coinStates[i] = Random.Range(0, 2) == 0;
                }
            }
        }
        
        /// <summary>
        /// 특정 코인 뒤집기 (액티브 스킬용)
        /// </summary>
        public void FlipCoin(int index)
        {
            if (index >= 0 && index < _coinCount && !_lockedCoins[index])
            {
                _coinStates[index] = !_coinStates[index];
                Debug.Log($"CombatSystem: Flipped coin {index} to {(_coinStates[index] ? "앞면" : "뒷면")}");
            }
        }
        
        /// <summary>
        /// 특정 코인 고정 (액티브 스킬용)
        /// </summary>
        public void LockCoin(int index)
        {
            if (index >= 0 && index < _coinCount)
            {
                _lockedCoins[index] = true;
                Debug.Log($"CombatSystem: Locked coin {index}");
            }
        }
        
        /// <summary>
        /// 두 코인 위치 교체 (액티브 스킬용)
        /// </summary>
        public void SwapCoins(int index1, int index2)
        {
            if (index1 >= 0 && index1 < _coinCount && index2 >= 0 && index2 < _coinCount)
            {
                if (!_lockedCoins[index1] && !_lockedCoins[index2])
                {
                    bool temp = _coinStates[index1];
                    _coinStates[index1] = _coinStates[index2];
                    _coinStates[index2] = temp;
                    Debug.Log($"CombatSystem: Swapped coins {index1} and {index2}");
                }
            }
        }
        #endregion

        private void SubscribeToEvents()
        {
            // 외부 전투 시작 요청 이벤트 구독
            DungeonEvents.CombatEvents.OnCombatStartRequested += HandleCombatStartRequest;
        }

        private void OnDestroy()
        {
            // 이벤트 구독 해제
            DungeonEvents.CombatEvents.OnCombatStartRequested -= HandleCombatStartRequest;
        }
        #endregion

        #region Combat Flow (핵심 로직만)
        /// <summary>
        /// 전투 시작 요청 처리
        /// </summary>
        private void HandleCombatStartRequest(string enemyType, CharacterType characterType)
        {
            Debug.Log($"[CombatSystem] 전투 시작 요청: {enemyType}, {characterType}");

            // 캐릭터들을 외부에서 받아옴
            var characterManager = CharacterManager.Instance;
            if (characterManager == null)
            {
                Debug.LogError("[CombatSystem] CharacterManager를 찾을 수 없습니다");
                return;
            }

            _player = characterManager.GetCurrentPlayer();
            _enemy = characterManager.CreateEnemy(enemyType, characterType);

            if (_player == null || _enemy == null)
            {
                Debug.LogError("[CombatSystem] 캐릭터 생성 실패");
                return;
            }

            StartCombat();
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        private void StartCombat()
        {
            Debug.Log("[CombatSystem] 전투 시작");
            
            _isCombatActive = true;
            _isPlayerTurn = true;
            _turnCount = 1;

            // 캐릭터 초기화
            _player.ResetForCombat();
            _enemy.ResetForCombat();

            // 전투 시작 이벤트 발행
            OnCombatStarted?.Invoke(_player, _enemy);
            DungeonEvents.CombatEvents.NotifyCombatInitialized();

            // 첫 턴 시작
            StartTurn();
        }

        /// <summary>
        /// 턴 시작 (핵심 로직)
        /// </summary>
        private void StartTurn()
        {
            Debug.Log($"[CombatSystem] 턴 {_turnCount} 시작 - 플레이어 턴: {_isPlayerTurn}");

            OnTurnChanged?.Invoke(_turnCount);

            // 동전 던지기
            FlipCoins();
            var coinResults = GetCoinResults();

            if (_isPlayerTurn)
            {
                // 사용 가능한 패턴 계산
                var availablePatterns = DataConnector.Instance.GetAvailablePatterns(
                    _player.SenseType, 
                    coinResults.ToArray()
                );

                // 플레이어 턴 시작 이벤트 발행
                OnPlayerTurnStarted?.Invoke(availablePatterns, coinResults);
            }
            else
            {
                // AI 턴 처리
                StartCoroutine(ProcessAITurn(coinResults));
            }
        }

        /// <summary>
        /// AI 턴 처리
        /// </summary>
        private IEnumerator ProcessAITurn(List<bool> coinResults)
        {
            Debug.Log("[CombatSystem] AI 턴 처리");

            // AI 패턴 선택
            var aiManager = AIManager.Instance;
            var selectedPattern = aiManager?.SelectPattern(_enemy, _player);

            if (selectedPattern != null)
            {
                yield return new WaitForSeconds(1f); // AI 사고 시간
                ExecutePattern(selectedPattern, _enemy, _player);
            }

            // 턴 종료
            EndTurn();
        }

        /// <summary>
        /// 플레이어 패턴 실행 (외부에서 호출)
        /// </summary>
        public void ExecutePlayerPattern(Pattern pattern)
        {
            if (!_isCombatActive || !_isPlayerTurn)
            {
                Debug.LogWarning("[CombatSystem] 플레이어 턴이 아님");
                return;
            }

            ExecutePattern(pattern, _player, _enemy);
            EndTurn();
        }

        /// <summary>
        /// 패턴 실행 (순수 로직)
        /// </summary>
        private void ExecutePattern(Pattern pattern, Character attacker, Character target)
        {
            Debug.Log($"[CombatSystem] 패턴 실행: {pattern.Name}");

            OnPatternExecuted?.Invoke(pattern);

            // 공격/방어 처리
            if (pattern.IsAttack)
            {
                int damage = attacker.AttackPower + pattern.AttackBonus;
                target.TakeDamage(damage);
                OnDamageDealt?.Invoke(target, damage);
            }
            else
            {
                int defense = pattern.DefenseBonus;
                attacker.AddDefense(defense);
                OnDefenseAdded?.Invoke(attacker, defense);
            }

            // 상태 효과 적용
            ApplyStatusEffects(pattern, attacker, target);

            // 전투 종료 체크
            CheckCombatEnd();
        }

        /// <summary>
        /// 상태 효과 적용
        /// </summary>
        private void ApplyStatusEffects(Pattern pattern, Character source, Character target)
        {
            if (pattern.StatusEffects == null) return;

            foreach (var effectData in pattern.StatusEffects)
            {
                var effect = new StatusEffect(
                    effectData.EffectType,
                    effectData.Magnitude,
                    effectData.Duration,
                    source
                );

                var effectTarget = pattern.IsAttack ? target : source;
                effectTarget.AddStatusEffect(effect);
            }
        }

        /// <summary>
        /// 턴 종료
        /// </summary>
        private void EndTurn()
        {
            // 상태 효과 업데이트
            _player.UpdateStatusEffects();
            _enemy.UpdateStatusEffects();

            // 스킬 쿨다운 업데이트
            if (_isPlayerTurn)
            {
                _player.UpdateSkillCooldown();
            }

            // 턴 전환
            if (_isPlayerTurn)
            {
                _isPlayerTurn = false;
                // AI 턴은 즉시 처리됨
                StartTurn();
            }
            else
            {
                _isPlayerTurn = true;
                _turnCount++;
                StartTurn();
            }
        }

        /// <summary>
        /// 전투 종료 체크
        /// </summary>
        private void CheckCombatEnd()
        {
            if (_player.CurrentHealth <= 0)
            {
                EndCombat(false); // 패배
            }
            else if (_enemy.CurrentHealth <= 0)
            {
                EndCombat(true); // 승리
            }
        }

        /// <summary>
        /// 전투 종료
        /// </summary>
        private void EndCombat(bool playerVictory)
        {
            Debug.Log($"[CombatSystem] 전투 종료 - 플레이어 승리: {playerVictory}");
            
            _isCombatActive = false;
            
            OnCombatEnded?.Invoke(playerVictory);
            DungeonEvents.CombatEvents.NotifyCombatEnded(playerVictory);
        }

        /// <summary>
        /// 액티브 스킬 사용 (외부에서 호출)
        /// </summary>
        public void UseActiveSkill()
        {
            if (!_isCombatActive || !_isPlayerTurn)
            {
                Debug.LogWarning("[CombatSystem] 액티브 스킬 사용 불가");
                return;
            }

            if (!_player.IsActiveSkillAvailable())
            {
                Debug.LogWarning("[CombatSystem] 액티브 스킬 쿨다운 중");
                return;
            }

            // 스킬 사용 (CombatSystem 자체를 전달)
            _player.UseActiveSkill(this);

            // 코인 상태 변경 후 새로운 패턴 계산
            var coinResults = GetCoinResults();
            var availablePatterns = DataConnector.Instance.GetAvailablePatterns(
                _player.SenseType, 
                coinResults.ToArray()
            );

            // 업데이트된 정보를 이벤트로 전달
            OnPlayerTurnStarted?.Invoke(availablePatterns, coinResults);
        }
        #endregion

        #region Public Properties (읽기 전용)
        public bool IsCombatActive => _isCombatActive;
        public bool IsPlayerTurn => _isPlayerTurn;
        public int TurnCount => _turnCount;
        public PlayerCharacter CurrentPlayer => _player;
        public EnemyCharacter CurrentEnemy => _enemy;
        #endregion
    }

    /// <summary>
    /// 전투 이벤트 확장 (기존 CombatEvents와 분리)
    /// </summary>
    public static class CombatSystemEvents
    {
        public static event Action<List<Pattern>, List<bool>> OnPlayerTurnReady;
        public static event Action<Character, int> OnCharacterDamaged;
        public static event Action<Character, int> OnCharacterHealed;
        
        public static void NotifyPlayerTurnReady(List<Pattern> patterns, List<bool> coins)
        {
            OnPlayerTurnReady?.Invoke(patterns, coins);
        }
        
        public static void NotifyCharacterDamaged(Character character, int damage)
        {
            OnCharacterDamaged?.Invoke(character, damage);
        }
        
        public static void NotifyCharacterHealed(Character character, int healing)
        {
            OnCharacterHealed?.Invoke(character, healing);
        }
    }
}
