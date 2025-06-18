using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.Extensions;
using MonoChrome.StatusEffects;
using UnityEngine;


namespace MonoChrome
{
    /// <summary>
    /// 적 캐릭터 클래스
    /// 적 유형에 따른 고유 패턴과 행동을 정의한다.
    /// </summary>
    [System.Serializable]
    public class EnemyCharacter : Character
    {
        #region Properties
        // 적 고유 특수 효과 유형
        private StatusEffectType _primaryEffectType;
        private StatusEffectType _secondaryEffectType;
        
        // 사용 가능한 패턴 목록
        private List<Pattern> _availablePatterns = new List<Pattern>();
        
        // 전투 중 사용한 패턴 기록 (AI가 전략적 선택에 활용)
        private List<Pattern> _usedPatterns = new List<Pattern>();
        
        // 턴 기반 행동 카운터
        private int _actionCounter = 0;
        
        // Public accessors
        public List<Pattern> Patterns => _availablePatterns;
        public StatusEffectType PrimaryEffectType => _primaryEffectType;
        public StatusEffectType SecondaryEffectType => _secondaryEffectType;
        #endregion
        
        #region Initialization
        /// <summary>
        /// 적 캐릭터 초기화
        /// </summary>
        public EnemyCharacter(string name, CharacterType type, int maxHealth, int attackPower, int defensePower, 
                             StatusEffectType primaryEffect, StatusEffectType secondaryEffect) 
            : base(name, type, maxHealth, attackPower, defensePower)
        {
            _primaryEffectType = primaryEffect;
            _secondaryEffectType = secondaryEffect;
            
            // 유형에 따른 패턴 초기화
            InitializePatterns();
            
            Debug.Log($"Enemy character created: {name}, Type: {type}");
        }
        
        /// <summary>
        /// 유형에 따른 패턴 초기화 - ScriptableObject에서 로드
        /// </summary>
        private void InitializePatterns()
        {
            var patternManager = PatternDataManager.Instance;
            if (patternManager == null)
            {
                Debug.LogError("PatternDataManager not found! Using fallback patterns.");
                CreateDefaultPatterns();
                return;
            }

            // 기본 패턴 로드
            var basicPatterns = patternManager.GetBasicPatterns();
            foreach (var patternSO in basicPatterns)
            {
                if (patternSO != null)
                {
                    _availablePatterns.Add(patternSO.ToPattern());
                }
            }

            // 적 유형별 패턴
            List<PatternSO> enemyPatterns = new List<PatternSO>();
            
            if (Type.IsNormal())
            {
                enemyPatterns = patternManager.GetPatternsByCharacterType(CharacterType.Normal);
            }
            else if (Type.IsElite())
            {
                enemyPatterns = patternManager.GetPatternsByCharacterType(CharacterType.Elite);
            }
            else if (Type.IsMiniBoss())
            {
                enemyPatterns = patternManager.GetPatternsByCharacterType(CharacterType.MiniBoss);
            }
            else if (Type.IsBoss())
            {
                enemyPatterns = patternManager.GetPatternsByCharacterType(CharacterType.Boss);
            }

            foreach (var patternSO in enemyPatterns)
            {
                if (patternSO != null)
                {
                    _availablePatterns.Add(patternSO.ToPattern());
                }
            }
            
            Debug.Log($"Initialized {_availablePatterns.Count} patterns for {Type} enemy: {CharacterName} from ScriptableObject data");
        }
        #endregion
        
        #region Legacy Pattern Methods (DEPRECATED)
        // 하드코딩된 패턴 생성 메서드들 제거됨
        // 현재는 ScriptableObject 기반 패턴 시스템 사용
        
        /// <summary>
        /// 기본 패턴 생성 (폴백용)
        /// </summary>
        private void CreateDefaultPatterns()
        {
            Debug.LogWarning("Using fallback pattern creation for enemy - ScriptableObject system not available");
            
            // 최소한의 기본 패턴만 생성
            _availablePatterns.Add(new Pattern
            {
                Name = "기본 공격",
                Description = "일반적인 공격",
                ID = 1,
                IsAttack = true,
                PatternType = PatternType.Consecutive2,
                PatternValue = true,
                AttackBonus = 1
            });
            
            _availablePatterns.Add(new Pattern
            {
                Name = "기본 방어",
                Description = "일반적인 방어",
                ID = 2,
                IsAttack = false,
                PatternType = PatternType.Consecutive2,
                PatternValue = false,
                DefenseBonus = 1
            });
        }
        #endregion
        
        #region Override Methods
        /// <summary>
        /// 액티브 스킬 사용 (적은 전투 시스템을 직접 조작하지 않음)
        /// </summary>
        public override void UseActiveSkill(MonoChrome.Systems.Combat.CombatSystem combatSystem)
        {
            // 적 캐릭터의 액티브 스킬은 AI가 패턴 결정 시 자동으로 선택
            // 별도 구현 없음
            Debug.Log($"Enemy {CharacterName} attempted to use active skill directly - not supported");
        }
        
        /// <summary>
        /// 사용 가능한 패턴 목록 가져오기
        /// </summary>
        public override List<Pattern> GetAvailablePatterns()
        {
            return _availablePatterns;
        }
        
        /// <summary>
        /// 전투 초기화 시 추가 작업
        /// </summary>
        public override void ResetForCombat()
        {
            base.ResetForCombat();
            
            // 사용한 패턴 기록 초기화
            _usedPatterns.Clear();
            
            // 행동 카운터 초기화
            _actionCounter = 0;
        }
        #endregion
        
        #region Enemy Specific Methods
        /// <summary>
        /// 패턴 사용 기록
        /// </summary>
        public void RecordPatternUse(Pattern pattern)
        {
            if (pattern != null)
            {
                _usedPatterns.Add(pattern);
                _actionCounter++;
                
                Debug.Log($"Enemy {CharacterName} used pattern: {pattern.Name} (Action #{_actionCounter})");
            }
        }
        
        /// <summary>
        /// 현재 진행 중인 전투 턴 수 설정
        /// </summary>
        public void SetActionCounter(int counter)
        {
            _actionCounter = counter;
        }
        
        /// <summary>
        /// 현재 진행 중인 전투 턴 수 가져오기
        /// </summary>
        public int GetActionCounter()
        {
            return _actionCounter;
        }
        
        /// <summary>
        /// 사용한 패턴 목록 가져오기
        /// </summary>
        public List<Pattern> GetUsedPatterns()
        {
            return _usedPatterns;
        }
        
        /// <summary>
        /// 메인 효과 유형 가져오기
        /// </summary>
        public StatusEffectType GetPrimaryEffectType()
        {
            return _primaryEffectType;
        }
        
        /// <summary>
        /// 보조 효과 유형 가져오기
        /// </summary>
        public StatusEffectType GetSecondaryEffectType()
        {
            return _secondaryEffectType;
        }
        #endregion
    }
}
