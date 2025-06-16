using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome.AI;
using MonoChrome.Data;
using MonoChrome.Systems.Combat;
using MonoChrome.Systems.UI;

namespace MonoChrome.AI
{
    /// <summary>
    /// 개별 몬스터에 부착되는 AI 컴포넌트
    /// 해당 몬스터의 고유한 AI 특성, 행동 패턴, 상태를 관리한다.
    /// AIManager와 연동하여 전술적 결정을 수행한다.
    /// </summary>
    [RequireComponent(typeof(Character))]
    public class MonsterAI : MonoBehaviour
    {
        [Header("AI 설정")]
        [SerializeField] private string monsterTypeOverride;
        [SerializeField] private float decisionThinkTime = 1f;
        [SerializeField] private bool showDebugLogs = true;
        
        [Header("AI 특성")]
        [SerializeField] private AIPersonality personality = AIPersonality.Balanced;
        [SerializeField] private int aggressionLevel = 5; // 1-10
        [SerializeField] private int cautionLevel = 5; // 1-10
        [SerializeField] private int intelligenceLevel = 5; // 1-10
        
        [Header("특수 행동 설정")]
        [SerializeField] private bool hasOpeningMove = false;
        [SerializeField] private bool hasPhaseTransitions = false;
        [SerializeField] private float[] phaseHealthThresholds = { 0.7f, 0.3f };
        [SerializeField] private bool hasEnrageMode = false;
        [SerializeField] private float enrageHealthThreshold = 0.25f;
        
        // 내부 상태
        private Character character;
        private MonsterPatternSO currentIntendedPattern;
        private int currentPhase = 0;
        private bool isEnraged = false;
        private bool hasUsedOpeningMove = false;
        
        // AI 상태 데이터
        private AIStateData aiState;
        
        #region Unity Lifecycle
        private void Awake()
        {
            InitializeAI();
        }
        
        private void Start()
        {
            SetupAICharacteristics();
        }
        
        private void OnEnable()
        {
            RegisterEventListeners();
        }
        
        private void OnDisable()
        {
            UnregisterEventListeners();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// AI 초기화
        /// </summary>
        private void InitializeAI()
        {
            character = GetComponent<Character>();
            
            if (character == null)
            {
                Debug.LogError($"MonsterAI: {gameObject.name}에 Character 컴포넌트가 없습니다!");
                enabled = false;
                return;
            }
            
            // AI 상태 데이터 초기화
            aiState = new AIStateData();
            
            // 몬스터 타입 결정
            if (string.IsNullOrEmpty(monsterTypeOverride))
            {
                monsterTypeOverride = character.CharacterName;
            }
            
            LogDebug($"MonsterAI 초기화 완료: {character.CharacterName}");
        }
        
        /// <summary>
        /// AI 특성 설정
        /// </summary>
        private void SetupAICharacteristics()
        {
            // 캐릭터 타입에 따른 기본 특성 설정
            switch (character.Type)
            {
                case CharacterType.Normal:
                    SetupNormalAI();
                    break;
                    
                case CharacterType.Elite:
                    SetupEliteAI();
                    break;
                    
                case CharacterType.MiniBoss:
                    SetupMiniBossAI();
                    break;
                    
                case CharacterType.Boss:
                    SetupBossAI();
                    break;
            }
            
            LogDebug($"AI 특성 설정 완료 - 성격: {personality}, 공격성: {aggressionLevel}, 신중함: {cautionLevel}");
        }
        
        private void SetupNormalAI()
        {
            personality = AIPersonality.Balanced;
            aggressionLevel = Random.Range(3, 7);
            cautionLevel = Random.Range(3, 7);
            intelligenceLevel = Random.Range(2, 5);
            hasOpeningMove = false;
            hasPhaseTransitions = false;
            hasEnrageMode = false;
        }
        
        private void SetupEliteAI()
        {
            personality = (AIPersonality)Random.Range(0, 4);
            aggressionLevel = Random.Range(4, 8);
            cautionLevel = Random.Range(4, 8);
            intelligenceLevel = Random.Range(4, 7);
            hasOpeningMove = Random.value < 0.3f;
            hasPhaseTransitions = false;
            hasEnrageMode = Random.value < 0.5f;
        }
        
        private void SetupMiniBossAI()
        {
            personality = (AIPersonality)Random.Range(0, 4);
            aggressionLevel = Random.Range(6, 9);
            cautionLevel = Random.Range(4, 8);
            intelligenceLevel = Random.Range(6, 9);
            hasOpeningMove = true;
            hasPhaseTransitions = true;
            hasEnrageMode = true;
            phaseHealthThresholds = new float[] { 0.6f, 0.3f };
        }
        
        private void SetupBossAI()
        {
            personality = AIPersonality.Strategic;
            aggressionLevel = Random.Range(7, 10);
            cautionLevel = Random.Range(6, 9);
            intelligenceLevel = Random.Range(8, 10);
            hasOpeningMove = true;
            hasPhaseTransitions = true;
            hasEnrageMode = true;
            phaseHealthThresholds = new float[] { 0.7f, 0.4f, 0.15f };
        }
        
        /// <summary>
        /// 이벤트 리스너 등록
        /// </summary>
        private void RegisterEventListeners()
        {
            if (character != null)
            {
                character.OnHealthChanged += OnHealthChanged;
                character.OnTurnStart += OnTurnStart;
                character.OnTurnEnd += OnTurnEnd;
            }
        }
        
        /// <summary>
        /// 이벤트 리스너 해제
        /// </summary>
        private void UnregisterEventListeners()
        {
            if (character != null)
            {
                character.OnHealthChanged -= OnHealthChanged;
                character.OnTurnStart -= OnTurnStart;
                character.OnTurnEnd -= OnTurnEnd;
            }
        }
        #endregion
        
        #region AI Decision Making
        /// <summary>
        /// 몬스터의 다음 행동을 결정한다
        /// </summary>
        /// <param name="player">플레이어 캐릭터</param>
        /// <returns>결정된 패턴</returns>
        public MonsterPatternSO DecideAction(Character player)
        {
            if (character == null || player == null)
            {
                LogDebug("DecideAction: character 또는 player가 null입니다");
                return null;
            }
            
            // 상태 업데이트
            UpdateAIState(player);
            
            // 특수 조건 확인
            MonsterPatternSO specialPattern = CheckSpecialConditions(player);
            if (specialPattern != null)
            {
                currentIntendedPattern = specialPattern;
                LogDebug($"특수 패턴 선택: {specialPattern.PatternName}");
                return specialPattern;
            }
            
            // 일반 AI 로직으로 패턴 선택
            MonsterPatternSO selectedPattern = AIManager.Instance.SelectMonsterPattern(character, player);
            
            // AI 성격에 따른 후처리
            selectedPattern = ApplyPersonalityModification(selectedPattern, player);
            
            currentIntendedPattern = selectedPattern;
            
            if (selectedPattern != null)
            {
                LogDebug($"AI 결정 완료: {selectedPattern.PatternName}");
            }
            
            return selectedPattern;
        }
        
        /// <summary>
        /// AI 상태 업데이트
        /// </summary>
        private void UpdateAIState(Character player)
        {
            aiState.turnCount++;
            aiState.healthRatio = (float)character.CurrentHealth / character.MaxHealth;
            aiState.playerHealthRatio = (float)player.CurrentHealth / player.MaxHealth;
            aiState.wasPlayerHurtLastTurn = player.CurrentHealth < aiState.lastPlayerHealth;
            aiState.lastPlayerHealth = player.CurrentHealth;
            
            // 페이즈 전환 확인
            CheckPhaseTransition();
            
            // 분노 모드 확인
            CheckEnrageMode();
        }
        
        /// <summary>
        /// 특수 조건 확인 (오프닝, 페이즈 전환 등)
        /// </summary>
        private MonsterPatternSO CheckSpecialConditions(Character player)
        {
            // 첫 턴 오프닝 무브
            if (hasOpeningMove && !hasUsedOpeningMove && aiState.turnCount == 1)
            {
                hasUsedOpeningMove = true;
                return FindSpecialPattern("등장", "오프닝", "시작");
            }
            
            // 페이즈 전환 특수 패턴
            if (hasPhaseTransitions && currentPhase < phaseHealthThresholds.Length)
            {
                if (aiState.healthRatio <= phaseHealthThresholds[currentPhase])
                {
                    currentPhase++;
                    LogDebug($"페이즈 {currentPhase} 전환!");
                    return FindSpecialPattern("페이즈", "전환", "변화");
                }
            }
            
            // 분노 모드 특수 패턴
            if (isEnraged && Random.value < 0.3f) // 30% 확률로 분노 패턴
            {
                return FindSpecialPattern("분노", "광분", "절망");
            }
            
            return null;
        }
        
        /// <summary>
        /// 특수 패턴 찾기
        /// </summary>
        private MonsterPatternSO FindSpecialPattern(params string[] keywords)
        {
            var allPatterns = PatternDataManager.Instance?.GetMonsterPatternsForType(monsterTypeOverride);
            if (allPatterns == null || allPatterns.Count == 0) return null;
            
            foreach (var pattern in allPatterns)
            {
                foreach (string keyword in keywords)
                {
                    if (pattern.PatternName.Contains(keyword) || 
                        pattern.Description.Contains(keyword) ||
                        pattern.IntentType.Contains(keyword))
                    {
                        return pattern;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// AI 성격에 따른 패턴 수정
        /// </summary>
        private MonsterPatternSO ApplyPersonalityModification(MonsterPatternSO pattern, Character player)
        {
            if (pattern == null) return pattern;
            
            // 성격별 행동 수정 로직
            switch (personality)
            {
                case AIPersonality.Aggressive:
                    return ModifyForAggressive(pattern, player);
                    
                case AIPersonality.Defensive:
                    return ModifyForDefensive(pattern, player);
                    
                case AIPersonality.Strategic:
                    return ModifyForStrategic(pattern, player);
                    
                case AIPersonality.Chaotic:
                    return ModifyForChaotic(pattern, player);
                    
                default:
                    return pattern;
            }
        }
        
        private MonsterPatternSO ModifyForAggressive(MonsterPatternSO pattern, Character player)
        {
            // 공격적 성격: 공격 패턴 선호, 방어 패턴 회피
            if (pattern.IntentType.Contains("방어") && aiState.healthRatio > 0.2f)
            {
                // 체력이 충분하면 방어 대신 공격 패턴 찾기
                var attackPattern = FindSpecialPattern("공격", "타격");
                if (attackPattern != null) return attackPattern;
            }
            
            return pattern;
        }
        
        private MonsterPatternSO ModifyForDefensive(MonsterPatternSO pattern, Character player)
        {
            // 방어적 성격: 체력이 낮을 때 더 신중하게
            if (aiState.healthRatio < 0.5f && pattern.IntentType.Contains("공격"))
            {
                // 방어 패턴으로 변경 시도
                var defensePattern = FindSpecialPattern("방어", "보호", "회복");
                if (defensePattern != null) return defensePattern;
            }
            
            return pattern;
        }
        
        private MonsterPatternSO ModifyForStrategic(MonsterPatternSO pattern, Character player)
        {
            // 전략적 성격: 플레이어 상태에 따른 최적화
            if (player.GetAllStatusEffects().Count == 0 && pattern.IntentType.Contains("공격"))
            {
                // 플레이어에게 상태이상이 없으면 상태이상 부여 우선
                var statusPattern = FindSpecialPattern("상태", "저주", "독", "봉인");
                if (statusPattern != null) return statusPattern;
            }
            
            return pattern;
        }
        
        private MonsterPatternSO ModifyForChaotic(MonsterPatternSO pattern, Character player)
        {
            // 혼돈적 성격: 랜덤하게 패턴 변경
            if (Random.value < 0.2f) // 20% 확률로 완전히 다른 패턴
            {
                var allPatterns = PatternDataManager.Instance?.GetMonsterPatternsForType(monsterTypeOverride);
                if (allPatterns != null && allPatterns.Count > 1)
                {
                    var randomPattern = allPatterns[Random.Range(0, allPatterns.Count)];
                    if (randomPattern != pattern) return randomPattern;
                }
            }
            
            return pattern;
        }
        #endregion
        
        #region State Management
        /// <summary>
        /// 페이즈 전환 확인
        /// </summary>
        private void CheckPhaseTransition()
        {
            if (!hasPhaseTransitions) return;
            
            int newPhase = 0;
            for (int i = 0; i < phaseHealthThresholds.Length; i++)
            {
                if (aiState.healthRatio <= phaseHealthThresholds[i])
                {
                    newPhase = i + 1;
                }
            }
            
            if (newPhase != currentPhase)
            {
                int oldPhase = currentPhase;
                currentPhase = newPhase;
                OnPhaseChanged(oldPhase, currentPhase);
            }
        }
        
        /// <summary>
        /// 분노 모드 확인
        /// </summary>
        private void CheckEnrageMode()
        {
            if (!hasEnrageMode) return;
            
            bool shouldBeEnraged = aiState.healthRatio <= enrageHealthThreshold;
            
            if (shouldBeEnraged && !isEnraged)
            {
                isEnraged = true;
                OnEnrageActivated();
            }
        }
        
        /// <summary>
        /// 페이즈 변경 시 호출
        /// </summary>
        private void OnPhaseChanged(int oldPhase, int newPhase)
        {
            LogDebug($"페이즈 변경: {oldPhase} → {newPhase}");
            
            // 페이즈별 특수 효과 적용 가능
            switch (newPhase)
            {
                case 1:
                    // 페이즈 1 효과
                    break;
                case 2:
                    // 페이즈 2 효과
                    aggressionLevel = Mathf.Min(10, aggressionLevel + 2);
                    break;
                case 3:
                    // 페이즈 3 효과 (최종 페이즈)
                    aggressionLevel = 10;
                    cautionLevel = Mathf.Max(1, cautionLevel - 3);
                    break;
            }
        }
        
        /// <summary>
        /// 분노 모드 활성화 시 호출
        /// </summary>
        private void OnEnrageActivated()
        {
            LogDebug("분노 모드 활성화!");
            
            // 분노 모드 효과
            aggressionLevel = 10;
            cautionLevel = 1;
            
            // 시각적 효과나 상태 변화 적용 가능
            // 예: 색상 변경, 이펙트 재생 등
        }
        #endregion
        
        #region Event Handlers
        /// <summary>
        /// 체력 변화 시 호출
        /// </summary>
        private void OnHealthChanged(int oldHealth, int newHealth)
        {
            // 체력 변화에 따른 AI 반응
            if (newHealth < oldHealth)
            {
                // 피해를 받았을 때
                OnTakeDamage(oldHealth - newHealth);
            }
        }
        
        /// <summary>
        /// 턴 시작 시 호출
        /// </summary>
        private void OnTurnStart()
        {
            LogDebug($"{character.CharacterName} 턴 시작 (턴 {aiState.turnCount + 1})");
        }
        
        /// <summary>
        /// 턴 종료 시 호출
        /// </summary>
        private void OnTurnEnd()
        {
            LogDebug($"{character.CharacterName} 턴 종료");
        }
        
        /// <summary>
        /// 피해를 받았을 때 호출
        /// </summary>
        private void OnTakeDamage(int damage)
        {
            // 피해량에 따른 AI 반응
            if (damage > character.MaxHealth * 0.2f) // 최대 체력의 20% 이상 피해
            {
                // 큰 피해를 받으면 더 공격적이 되거나 방어적이 될 수 있음
                if (personality == AIPersonality.Aggressive)
                {
                    aggressionLevel = Mathf.Min(10, aggressionLevel + 1);
                }
                else if (personality == AIPersonality.Defensive)
                {
                    cautionLevel = Mathf.Min(10, cautionLevel + 1);
                }
            }
        }
        #endregion
        
        #region Public API
        /// <summary>
        /// 현재 의도된 패턴 가져오기
        /// </summary>
        public MonsterPatternSO GetCurrentIntent()
        {
            return currentIntendedPattern;
        }
        
        /// <summary>
        /// 현재 페이즈 가져오기
        /// </summary>
        public int GetCurrentPhase()
        {
            return currentPhase;
        }
        
        /// <summary>
        /// 분노 상태 확인
        /// </summary>
        public bool IsEnraged()
        {
            return isEnraged;
        }
        
        /// <summary>
        /// AI 특성 수정 (런타임)
        /// </summary>
        public void ModifyCharacteristics(int aggressionDelta, int cautionDelta, int intelligenceDelta)
        {
            aggressionLevel = Mathf.Clamp(aggressionLevel + aggressionDelta, 1, 10);
            cautionLevel = Mathf.Clamp(cautionLevel + cautionDelta, 1, 10);
            intelligenceLevel = Mathf.Clamp(intelligenceLevel + intelligenceDelta, 1, 10);
            
            LogDebug($"AI 특성 수정됨 - 공격성: {aggressionLevel}, 신중함: {cautionLevel}, 지능: {intelligenceLevel}");
        }
        
        /// <summary>
        /// 강제로 분노 모드 활성화
        /// </summary>
        public void ForceEnrage()
        {
            if (!isEnraged)
            {
                isEnraged = true;
                OnEnrageActivated();
            }
        }
        #endregion
        
        #region Utility
        /// <summary>
        /// 디버그 로그 출력
        /// </summary>
        private void LogDebug(string message)
        {
            if (showDebugLogs)
            {
                Debug.Log($"[MonsterAI - {character?.CharacterName}] {message}");
            }
        }
        #endregion
    }
    
    #region Data Structures
    /// <summary>
    /// AI 상태 데이터
    /// </summary>
    [System.Serializable]
    public class AIStateData
    {
        public int turnCount = 0;
        public float healthRatio = 1f;
        public float playerHealthRatio = 1f;
        public bool wasPlayerHurtLastTurn = false;
        public int lastPlayerHealth = 0;
    }
    
    // AIPersonality enum은 AllEnums.cs에 정의되어 있습니다.
    #endregion
}