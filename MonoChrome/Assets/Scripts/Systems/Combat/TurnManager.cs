using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome.AI;
using MonoChrome.Data;
using MonoChrome.Systems.Combat;
using MonoChrome.Systems.UI;

namespace MonoChrome.Systems.Combat
{
    /// <summary>
    /// 전투 턴을 관리하는 시스템
    /// 몬스터 AI의 의도 결정, 페이즈 전환, 특수 패턴 타이밍 등을 관리한다.
    /// AIManager, IntentDisplaySystem과 연동하여 전투 흐름을 제어한다.
    /// </summary>
    public class TurnManager : MonoBehaviour
    {
        [Header("턴 관리 설정")]
        [SerializeField] private float turnTransitionDelay = 1f;
        [SerializeField] private float intentDisplayDelay = 0.5f;
        [SerializeField] private bool showTurnDebugLogs = true;
        
        [Header("페이즈 관리")]
        [SerializeField] private bool enablePhaseSystem = true;
        [SerializeField] private float phaseTransitionEffectDuration = 2f;
        
        // 전투 참여자
        private Character playerCharacter;
        private List<Character> enemyCharacters = new List<Character>();
        private List<MonsterAI> enemyAIs = new List<MonsterAI>();
        
        // 턴 상태
        private int currentTurnNumber = 0;
        private TurnPhase currentPhase = TurnPhase.Planning;
        private bool isTurnInProgress = false;
        private bool isBattleActive = false;
        
        // 의도 및 패턴 관리
        private Dictionary<Character, MonsterPatternSO> currentIntents = new Dictionary<Character, MonsterPatternSO>();
        private Dictionary<Character, int> enemyTurnCounts = new Dictionary<Character, int>();
        
        // 시스템 참조
        private IntentDisplaySystem intentDisplaySystem;
        
        // 이벤트
        public event Action<int> OnTurnStart;
        public event Action<int> OnTurnEnd;
        public event Action<TurnPhase, TurnPhase> OnPhaseChanged;
        public event Action<Character, MonsterPatternSO> OnIntentDetermined;
        public event Action<Character, int, int> OnEnemyPhaseChanged;
        
        #region Unity Lifecycle
        private void Awake()
        {
            InitializeTurnManager();
        }
        
        private void Start()
        {
            SetupSystemReferences();
        }
        
        private void OnDestroy()
        {
            CleanupTurnManager();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// 턴 매니저 초기화
        /// </summary>
        private void InitializeTurnManager()
        {
            currentTurnNumber = 0;
            currentPhase = TurnPhase.Planning;
            isTurnInProgress = false;
            isBattleActive = false;
            
            LogDebug("TurnManager 초기화 완료");
        }
        
        /// <summary>
        /// 시스템 참조 설정
        /// </summary>
        private void SetupSystemReferences()
        {
            // IntentDisplaySystem 찾기
            intentDisplaySystem = FindObjectOfType<IntentDisplaySystem>();
            if (intentDisplaySystem == null)
            {
                Debug.LogWarning("TurnManager: IntentDisplaySystem을 찾을 수 없습니다.");
            }
        }
        
        /// <summary>
        /// 턴 매니저 정리
        /// </summary>
        private void CleanupTurnManager()
        {
            StopAllCoroutines();
            
            // 이벤트 정리
            OnTurnStart = null;
            OnTurnEnd = null;
            OnPhaseChanged = null;
            OnIntentDetermined = null;
            OnEnemyPhaseChanged = null;
        }
        #endregion
        
        #region Battle Management
        /// <summary>
        /// 전투 시작
        /// </summary>
        /// <param name="player">플레이어 캐릭터</param>
        /// <param name="enemies">적 캐릭터 목록</param>
        public void StartBattle(Character player, List<Character> enemies)
        {
            if (player == null || enemies == null || enemies.Count == 0)
            {
                Debug.LogError("TurnManager: 잘못된 전투 참여자입니다.");
                return;
            }
            
            // 전투 참여자 설정
            playerCharacter = player;
            enemyCharacters.Clear();
            enemyAIs.Clear();
            enemyTurnCounts.Clear();
            currentIntents.Clear();
            
            foreach (Character enemy in enemies)
            {
                if (enemy != null)
                {
                    enemyCharacters.Add(enemy);
                    enemyTurnCounts[enemy] = 0;
                    
                    // MonsterAI 컴포넌트 찾기
                    MonsterAI ai = enemy.GetComponent<MonsterAI>();
                    if (ai != null)
                    {
                        enemyAIs.Add(ai);
                    }
                    else
                    {
                        Debug.LogWarning($"TurnManager: {enemy.CharacterName}에 MonsterAI 컴포넌트가 없습니다.");
                    }
                }
            }
            
            // 전투 상태 초기화
            currentTurnNumber = 0;
            currentPhase = TurnPhase.Planning;
            isBattleActive = true;
            
            LogDebug($"전투 시작 - 플레이어: {player.CharacterName}, 적: {enemies.Count}마리");
            
            // 첫 턴 시작
            StartNextTurn();
        }
        
        /// <summary>
        /// 전투 종료
        /// </summary>
        /// <param name="playerWon">플레이어 승리 여부</param>
        public void EndBattle(bool playerWon)
        {
            if (!isBattleActive) return;
            
            isBattleActive = false;
            isTurnInProgress = false;
            
            LogDebug($"전투 종료 - {(playerWon ? "플레이어 승리" : "플레이어 패배")}");
            
            // AI 매니저 정리
            if (AIManager.Instance != null)
            {
                AIManager.Instance.CleanupBattleData();
            }
            
            // 의도 표시 시스템 정리
            if (intentDisplaySystem != null)
            {
                intentDisplaySystem.OnBattleEnd();
            }
            
            // 개별 몬스터 AI 정리
            foreach (var enemy in enemyCharacters)
            {
                if (AIManager.Instance != null)
                {
                    AIManager.Instance.CleanupMonster(enemy);
                }
            }
            
            // 데이터 정리
            enemyCharacters.Clear();
            enemyAIs.Clear();
            enemyTurnCounts.Clear();
            currentIntents.Clear();
        }
        #endregion
        
        #region Turn Management
        /// <summary>
        /// 다음 턴 시작
        /// </summary>
        public void StartNextTurn()
        {
            if (!isBattleActive || isTurnInProgress) return;
            
            currentTurnNumber++;
            isTurnInProgress = true;
            
            LogDebug($"턴 {currentTurnNumber} 시작");
            
            OnTurnStart?.Invoke(currentTurnNumber);
            
            // 턴 처리 코루틴 시작
            StartCoroutine(ProcessTurnCoroutine());
        }
        
        /// <summary>
        /// 턴 처리 코루틴
        /// </summary>
        private IEnumerator ProcessTurnCoroutine()
        {
            // 1. 계획 단계 - 의도 결정
            yield return StartCoroutine(PlanningPhaseCoroutine());
            
            // 2. 실행 단계 - 행동 수행
            yield return StartCoroutine(ExecutionPhaseCoroutine());
            
            // 3. 해결 단계 - 결과 처리
            yield return StartCoroutine(ResolutionPhaseCoroutine());
            
            // 턴 종료
            EndCurrentTurn();
        }
        
        /// <summary>
        /// 계획 단계 - 모든 적의 의도 결정
        /// </summary>
        private IEnumerator PlanningPhaseCoroutine()
        {
            SetPhase(TurnPhase.Planning);
            
            LogDebug("계획 단계 시작 - 적 의도 결정");
            
            // 각 적의 의도 결정
            foreach (Character enemy in enemyCharacters)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                // 턴 카운트 증가
                enemyTurnCounts[enemy]++;
                
                // AI를 통해 의도 결정
                MonsterPatternSO intent = null;
                
                // MonsterAI가 있으면 우선 사용
                MonsterAI monsterAI = enemy.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    intent = monsterAI.DecideAction(playerCharacter);
                }
                else
                {
                    // AIManager 직접 사용
                    intent = AIManager.Instance?.DetermineIntent(enemy, playerCharacter);
                }
                
                if (intent != null)
                {
                    currentIntents[enemy] = intent;
                    OnIntentDetermined?.Invoke(enemy, intent);
                    
                    LogDebug($"{enemy.CharacterName}의 의도: {intent.PatternName}");
                }
                
                yield return new WaitForSeconds(0.1f); // 의도 결정 간격
            }
            
            // 의도 표시 시스템 업데이트
            if (intentDisplaySystem != null)
            {
                intentDisplaySystem.OnTurnStart(enemyCharacters, playerCharacter);
                yield return new WaitForSeconds(intentDisplayDelay);
            }
            
            LogDebug("계획 단계 완료");
        }
        
        /// <summary>
        /// 실행 단계 - 실제 행동 수행
        /// </summary>
        private IEnumerator ExecutionPhaseCoroutine()
        {
            SetPhase(TurnPhase.Execution);
            
            LogDebug("실행 단계 시작");
            
            // 플레이어 턴
            yield return StartCoroutine(ProcessPlayerTurn());
            
            // 적들의 턴
            foreach (Character enemy in enemyCharacters)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                yield return StartCoroutine(ProcessEnemyTurn(enemy));
                
                // 전투 종료 조건 확인
                if (!playerCharacter.IsAlive || enemyCharacters.TrueForAll(e => e == null || !e.IsAlive))
                {
                    break;
                }
            }
            
            LogDebug("실행 단계 완료");
        }
        
        /// <summary>
        /// 해결 단계 - 결과 처리 및 정리
        /// </summary>
        private IEnumerator ResolutionPhaseCoroutine()
        {
            SetPhase(TurnPhase.Resolution);
            
            LogDebug("해결 단계 시작");
            
            // 상태 효과 처리
            yield return StartCoroutine(ProcessStatusEffects());
            
            // 페이즈 전환 확인
            yield return StartCoroutine(CheckPhaseTransitions());
            
            // 전투 종료 조건 확인
            CheckBattleEndConditions();
            
            LogDebug("해결 단계 완료");
        }
        
        /// <summary>
        /// 플레이어 턴 처리
        /// </summary>
        private IEnumerator ProcessPlayerTurn()
        {
            LogDebug("플레이어 턴 시작");
            
            // 플레이어 턴 로직은 별도의 PlayerTurnManager에서 처리
            // 여기서는 턴 시작 알림만 처리
            
            yield return new WaitForSeconds(0.1f);
            
            LogDebug("플레이어 턴 완료");
        }
        
        /// <summary>
        /// 적 턴 처리
        /// </summary>
        private IEnumerator ProcessEnemyTurn(Character enemy)
        {
            if (enemy == null || !enemy.IsAlive) yield break;
            
            LogDebug($"{enemy.CharacterName} 턴 시작");
            
            // 의도된 패턴 실행
            if (currentIntents.TryGetValue(enemy, out MonsterPatternSO intent))
            {
                yield return StartCoroutine(ExecuteMonsterPattern(enemy, intent));
            }
            
            LogDebug($"{enemy.CharacterName} 턴 완료");
        }
        
        /// <summary>
        /// 몬스터 패턴 실행
        /// </summary>
        private IEnumerator ExecuteMonsterPattern(Character enemy, MonsterPatternSO pattern)
        {
            if (enemy == null || pattern == null) yield break;
            
            LogDebug($"{enemy.CharacterName}이 {pattern.PatternName} 사용");
            
            // 패턴 실행 로직
            // 실제 구현에서는 CombatManager를 통해 처리
            
            // 공격 보너스 적용
            if (pattern.AttackBonus > 0)
            {
                int damage = enemy.CurrentAttack + pattern.AttackBonus;
                LogDebug($"공격력 {damage}로 플레이어 공격");
                
                // 실제 피해 적용은 CombatManager에서 처리
                // playerCharacter.TakeDamage(damage);
            }
            
            // 방어 보너스 적용
            if (pattern.DefenseBonus > 0)
            {
                LogDebug($"방어력 {pattern.DefenseBonus} 증가");
                // enemy.AddDefense(pattern.DefenseBonus);
            }
            
            // 상태 효과 적용
            if (pattern.StatusEffects != null && pattern.StatusEffects.Length > 0)
            {
                foreach (var statusEffect in pattern.StatusEffects)
                {
                    LogDebug($"상태 효과 적용: {statusEffect}");
                    // StatusEffectManager를 통해 처리
                }
            }
            
            yield return new WaitForSeconds(0.5f); // 패턴 실행 시간
        }
        
        /// <summary>
        /// 상태 효과 처리
        /// </summary>
        private IEnumerator ProcessStatusEffects()
        {
            LogDebug("상태 효과 처리");
            
            // 플레이어 상태 효과
            if (playerCharacter != null && playerCharacter.IsAlive)
            {
                // StatusEffectManager를 통해 처리
                yield return new WaitForSeconds(0.1f);
            }
            
            // 적 상태 효과
            foreach (Character enemy in enemyCharacters)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    // StatusEffectManager를 통해 처리
                    yield return new WaitForSeconds(0.1f);
                }
            }
        }
        
        /// <summary>
        /// 페이즈 전환 확인
        /// </summary>
        private IEnumerator CheckPhaseTransitions()
        {
            if (!enablePhaseSystem) yield break;
            
            foreach (Character enemy in enemyCharacters)
            {
                if (enemy == null || !enemy.IsAlive) continue;
                
                MonsterAI monsterAI = enemy.GetComponent<MonsterAI>();
                if (monsterAI != null)
                {
                    int currentPhaseNum = monsterAI.GetCurrentPhase();
                    float healthRatio = (float)enemy.CurrentHealth / enemy.MaxHealth;
                    
                    // 페이즈 전환 체크 로직은 MonsterAI에서 처리됨
                    // 여기서는 페이즈 전환 이벤트 처리만
                }
            }
            
            yield return null;
        }
        
        /// <summary>
        /// 전투 종료 조건 확인
        /// </summary>
        private void CheckBattleEndConditions()
        {
            if (!isBattleActive) return;
            
            // 플레이어 사망
            if (playerCharacter == null || !playerCharacter.IsAlive)
            {
                EndBattle(false);
                return;
            }
            
            // 모든 적 사망
            bool anyEnemyAlive = false;
            foreach (Character enemy in enemyCharacters)
            {
                if (enemy != null && enemy.IsAlive)
                {
                    anyEnemyAlive = true;
                    break;
                }
            }
            
            if (!anyEnemyAlive)
            {
                EndBattle(true);
            }
        }
        
        /// <summary>
        /// 현재 턴 종료
        /// </summary>
        private void EndCurrentTurn()
        {
            LogDebug($"턴 {currentTurnNumber} 종료");
            
            OnTurnEnd?.Invoke(currentTurnNumber);
            
            isTurnInProgress = false;
            
            // 다음 턴 예약 (전투가 계속되는 경우)
            if (isBattleActive)
            {
                Invoke(nameof(StartNextTurn), turnTransitionDelay);
            }
        }
        #endregion
        
        #region Phase Management
        /// <summary>
        /// 턴 페이즈 설정
        /// </summary>
        private void SetPhase(TurnPhase newPhase)
        {
            if (currentPhase != newPhase)
            {
                TurnPhase oldPhase = currentPhase;
                currentPhase = newPhase;
                
                OnPhaseChanged?.Invoke(oldPhase, newPhase);
                LogDebug($"페이즈 변경: {oldPhase} → {newPhase}");
            }
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// 현재 턴 번호 가져오기
        /// </summary>
        public int GetCurrentTurn()
        {
            return currentTurnNumber;
        }
        
        /// <summary>
        /// 현재 턴 페이즈 가져오기
        /// </summary>
        public TurnPhase GetCurrentPhase()
        {
            return currentPhase;
        }
        
        /// <summary>
        /// 전투 진행 중인지 확인
        /// </summary>
        public bool IsBattleActive()
        {
            return isBattleActive;
        }
        
        /// <summary>
        /// 턴 진행 중인지 확인
        /// </summary>
        public bool IsTurnInProgress()
        {
            return isTurnInProgress;
        }
        
        /// <summary>
        /// 특정 적의 현재 의도 가져오기
        /// </summary>
        public MonsterPatternSO GetEnemyIntent(Character enemy)
        {
            currentIntents.TryGetValue(enemy, out MonsterPatternSO intent);
            return intent;
        }
        
        /// <summary>
        /// 특정 적의 턴 카운트 가져오기
        /// </summary>
        public int GetEnemyTurnCount(Character enemy)
        {
            enemyTurnCounts.TryGetValue(enemy, out int count);
            return count;
        }
        
        /// <summary>
        /// 강제로 턴 건너뛰기 (디버그용)
        /// </summary>
        public void SkipTurn()
        {
            if (isTurnInProgress)
            {
                StopAllCoroutines();
                EndCurrentTurn();
            }
        }
        
        /// <summary>
        /// 강제로 전투 종료 (디버그용)
        /// </summary>
        public void ForceBattleEnd(bool playerWon)
        {
            EndBattle(playerWon);
        }
        #endregion
        
        #region Utility
        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (showTurnDebugLogs)
            {
                Debug.Log($"[TurnManager] {message}");
            }
        }
        #endregion
    }
    
    #region Enums
    /// <summary>
    /// 턴 페이즈 열거형
    /// </summary>
    public enum TurnPhase
    {
        Planning,    // 계획 단계 (의도 결정)
        Execution,   // 실행 단계 (행동 수행)
        Resolution   // 해결 단계 (결과 처리)
    }
    #endregion
}