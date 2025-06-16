using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;


// 타입 변환 유틸리티

namespace MonoChrome
{
    /// <summary>
    /// 플레이어 캐릭터 클래스
    /// 감각 유형에 따른 고유한 능력과 패턴을 정의한다.
    /// </summary>
    [System.Serializable]
    public class PlayerCharacter : Character
    {
        #region Properties
        // 플레이어 고유 정보
        private SenseType _senseType;
        
        // 사용 가능한 패턴 목록
        private List<Pattern> _availablePatterns = new List<Pattern>();
        
        // 액티브 스킬 정보
        private ActiveSkill _activeSkill;
        
        // 획득한 족보 목록
        private List<Pattern> _unlockedPatterns = new List<Pattern>();
        
        // 감각 유형에 따른 고유 메인/보조 효과
        private StatusEffectType _mainStatusEffectType;
        private StatusEffectType _subStatusEffectType;
        
        // 감각 유형 프로퍼티
        public SenseType SenseType => _senseType;
        #endregion
        
        #region Initialization
        /// <summary>
        /// 플레이어 캐릭터 초기화
        /// </summary>
        public PlayerCharacter(string name, SenseType senseType, int maxHealth, int attackPower, int defensePower) 
            : base(name, CharacterType.Player, maxHealth, attackPower, defensePower)
        {
            _senseType = senseType;
            _maxActiveSkillCooldown = 3; // 모든 플레이어는 기본적으로 3턴 쿨다운
            
            // 감각 유형에 따른 효과 타입 설정
            SetEffectTypesBySense();
            
            // 기본 패턴 및 액티브 스킬 설정
            InitializePatterns();
            InitializeActiveSkill();
            
            Debug.Log($"Player character created: {name}, Sense: {senseType}");
        }
        
        /// <summary>
        /// 감각 유형에 따른 효과 타입 설정
        /// </summary>
        private void SetEffectTypesBySense()
        {
            switch (_senseType)
            {
                case SenseType.Auditory: // 청각
                    _mainStatusEffectType = StatusEffectType.Amplify;   // 메인: 증폭
                    _subStatusEffectType = StatusEffectType.Resonance;  // 보조: 공명
                    break;
                    
                case SenseType.Olfactory: // 후각
                    _mainStatusEffectType = StatusEffectType.Mark;      // 메인: 표식
                    _subStatusEffectType = StatusEffectType.Bleed;      // 보조: 출혈
                    break;
                    
                case SenseType.Tactile: // 촉각
                    _mainStatusEffectType = StatusEffectType.Counter;   // 메인: 반격
                    _subStatusEffectType = StatusEffectType.Crush;      // 보조: 분쇄
                    break;
                    
                case SenseType.Spiritual: // 영적
                    _mainStatusEffectType = StatusEffectType.Curse;     // 메인: 저주
                    _subStatusEffectType = StatusEffectType.Seal;       // 보조: 봉인
                    break;
                    
                default:
                    _mainStatusEffectType = StatusEffectType.Amplify;   // 기본값
                    _subStatusEffectType = StatusEffectType.Resonance;
                    break;
            }
            
            Debug.Log($"Player effects set - Main: {_mainStatusEffectType}, Sub: {_subStatusEffectType}");
        }
        
        /// <summary>
        /// 기본 패턴 초기화 - ScriptableObject에서 로드
        /// </summary>
        private void InitializePatterns()
        {
            // PatternDataManager에서 감각 유형별 패턴 로드
            var patternManager = PatternDataManager.Instance;
            if (patternManager == null)
            {
                Debug.LogError("PatternDataManager not found! Loading fallback patterns.");
                CreateDefaultPatterns();
                return;
            }

            // 기본 패턴 (모든 감각 유형 공통)
            var basicPatterns = patternManager.GetBasicPatterns();
            foreach (var patternSO in basicPatterns)
            {
                if (patternSO != null)
                {
                    _availablePatterns.Add(patternSO.ToPattern());
                }
            }

            // 감각 유형별 전용 패턴
            var sensePatterns = patternManager.GetPatternsBySense(_senseType);
            foreach (var patternSO in sensePatterns)
            {
                if (patternSO != null)
                {
                    _availablePatterns.Add(patternSO.ToPattern());
                }
            }
            
            // 기본 패턴 해금
            foreach (Pattern pattern in _availablePatterns)
            {
                _unlockedPatterns.Add(pattern);
            }
            
            Debug.Log($"Initialized {_availablePatterns.Count} patterns for {_senseType} player from ScriptableObject data");
        }
        
        /// <summary>
        /// 액티브 스킬 초기화
        /// </summary>
        private void InitializeActiveSkill()
        {
            switch (_senseType)
            {
                case SenseType.Auditory: // 청각
                    // 기합: 모든 동전 재던지기 + 증폭 스택 +1
                    _activeSkill = new ActiveSkill(
                        "기합",
                        "모든 동전을 다시 던지고 증폭 스택을 1 증가시킵니다.",
                        ActiveSkillType.RethrowAll,
                        (MonoChrome.Systems.Combat.CombatSystem combatSystem) => 
                        {
                            combatSystem.RethrowAllCoins();
                            AddStatusEffect(new StatusEffect(_mainStatusEffectType, 1, 3, this));
                        });
                    break;
                    
                case SenseType.Olfactory: // 후각
                    // 속임수: 동전 1개 뒤집기 + 표식 +3
                    _activeSkill = new ActiveSkill(
                        "속임수",
                        "동전 1개를 선택해 뒤집고 표식 스택을 3 증가시킵니다.",
                        ActiveSkillType.FlipOne,
                        (MonoChrome.Systems.Combat.CombatSystem combatSystem) => 
                        {
                            // 첫 번째 동전 뒤집기 (나중에 선택 로직 추가)
                            combatSystem.FlipCoin(0);
                            AddStatusEffect(new StatusEffect(_mainStatusEffectType, 3, 2, this));
                        });
                    break;
                    
                case SenseType.Tactile: // 촉각
                    // 불괴: 동전 1개 고정 + 반격 +3
                    _activeSkill = new ActiveSkill(
                        "불괴",
                        "동전 1개를 선택해 고정하고 반격 스택을 3 증가시킵니다.",
                        ActiveSkillType.LockOne,
                        (MonoChrome.Systems.Combat.CombatSystem combatSystem) => 
                        {
                            // 첫 번째 동전 고정 (나중에 선택 로직 추가)
                            combatSystem.LockCoin(0);
                            AddStatusEffect(new StatusEffect(_mainStatusEffectType, 3, 2, this));
                        });
                    break;
                    
                case SenseType.Spiritual: // 영적
                    // 주문 배치: 동전 2개 위치 교환 + 저주 +2
                    _activeSkill = new ActiveSkill(
                        "주문 배치",
                        "동전 2개의 위치를 교환하고 저주 스택을 2 증가시킵니다.",
                        ActiveSkillType.SwapTwo,
                        (MonoChrome.Systems.Combat.CombatSystem combatSystem) => 
                        {
                            // 첫 번째와 두 번째 동전 교환 (나중에 선택 로직 추가)
                            combatSystem.SwapCoins(0, 1);
                            AddStatusEffect(new StatusEffect(_mainStatusEffectType, 2, 2, this));
                        });
                    break;
                    
                default:
                    // 기본 스킬: 모든 동전 재던지기
                    _activeSkill = new ActiveSkill(
                        "재시도",
                        "모든 동전을 다시 던집니다.",
                        ActiveSkillType.RethrowAll,
                        (MonoChrome.Systems.Combat.CombatSystem combatSystem) => 
                        {
                            combatSystem.RethrowAllCoins();
                        });
                    break;
            }
            
            Debug.Log($"Initialized active skill: {_activeSkill.Name}");
        }
        #endregion
        
        #region Legacy Pattern Creation Methods (DEPRECATED)
        // 하드코딩된 패턴 생성 메서드들 제거됨
        // 현재는 ScriptableObject 기반 패턴 시스템 사용
        
        /// <summary>
        /// 기본 패턴 생성 (폴백용)
        /// </summary>
        private void CreateDefaultPatterns()
        {
            Debug.LogWarning("Using fallback pattern creation - ScriptableObject system not available");
            
            // 최소한의 기본 패턴만 생성
            _availablePatterns.Add(new Pattern
            {
                Name = "기본 공격",
                Description = "공격력 +1",
                ID = 1,
                IsAttack = true,
                PatternType = PatternType.Consecutive2,
                PatternValue = true,
                AttackBonus = 1
            });
            
            _availablePatterns.Add(new Pattern
            {
                Name = "기본 방어",
                Description = "방어력 +1",
                ID = 2,
                IsAttack = false,
                PatternType = PatternType.Consecutive2,
                PatternValue = false,
                DefenseBonus = 1
            });
        }
        #endregion
        
        #region Combat Methods
        /// <summary>
        /// 액티브 스킬 사용
        /// </summary>
        public override void UseActiveSkill(MonoChrome.Systems.Combat.CombatSystem combatSystem)
        {
            if (_activeSkill != null && IsActiveSkillAvailable())
            {
                // 스킬 효과 적용
                _activeSkill.Execute(combatSystem);
                
                // 쿨다운 적용
                ApplyActiveSkillCooldown();
                
                Debug.Log($"Player used active skill: {_activeSkill.Name}");
            }
            else
            {
                Debug.LogWarning("Cannot use active skill: not available or null");
            }
        }
        
        /// <summary>
        /// 사용 가능한 패턴(족보) 목록 반환
        /// </summary>
        public override List<Pattern> GetAvailablePatterns()
        {
            // 현재 해금된 패턴만 반환
            return _unlockedPatterns;
        }
        
        /// <summary>
        /// 새로운 패턴(족보) 해금
        /// </summary>
        public void UnlockPattern(Pattern pattern)
        {
            if (pattern != null && !_unlockedPatterns.Contains(pattern))
            {
                _unlockedPatterns.Add(pattern);
                Debug.Log($"Player unlocked new pattern: {pattern.Name}");
            }
        }
        #endregion
    }
    
    #region Helper Classes and Enums
    /// <summary>
    /// 액티브 스킬 타입
    /// </summary>
    // 이 열거형은 AllEnums.cs로 이동되었습니다.
    // Enum definition moved to AllEnums.cs
    /*
    public enum ActiveSkillType
    {
        RethrowAll, // 모든 동전 재던지기
        FlipOne,    // 동전 1개 뒤집기
        LockOne,    // 동전 1개 고정
        SwapTwo     // 동전 2개 교환
    }
    */
    
    /// <summary>
    /// 액티브 스킬 클래스
    /// </summary>
    [System.Serializable]
    public class ActiveSkill
    {
        // 스킬 정보
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ActiveSkillType Type { get; private set; }
        
        // 스킬 실행 동작
        private Action<MonoChrome.Systems.Combat.CombatSystem> _skillAction;
        
        public ActiveSkill(string name, string description, ActiveSkillType type, Action<MonoChrome.Systems.Combat.CombatSystem> skillAction)
        {
            Name = name;
            Description = description;
            Type = type;
            _skillAction = skillAction;
        }
        
        public void Execute(MonoChrome.Systems.Combat.CombatSystem combatSystem)
        {
            if (_skillAction != null && combatSystem != null)
            {
                _skillAction.Invoke(combatSystem);
            }
            else
            {
                Debug.LogError("Cannot execute skill: null action or combat system");
            }
        }
    }
    #endregion
}
