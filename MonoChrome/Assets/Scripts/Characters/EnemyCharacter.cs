using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
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
        /// 유형에 따른 패턴 초기화
        /// </summary>
        private void InitializePatterns()
        {
            if (Type.IsNormal())
            {
                CreateNormalEnemyPatterns();
            }
            else if (Type.IsElite())
            {
                CreateEliteEnemyPatterns();
            }
            else if (Type.IsMiniBoss())
            {
                CreateMiniBossPatterns();
            }
            else if (Type.IsBoss())
            {
                CreateBossPatterns();
            }
            else
            {
                CreateDefaultPatterns();
            }
            
            Debug.Log($"Initialized {_availablePatterns.Count} patterns for enemy: {CharacterName}");
        }
        #endregion
        
        #region Pattern Methods
        /// <summary>
        /// 일반 적 패턴 생성
        /// </summary>
        private void CreateNormalEnemyPatterns()
        {
            // 기본 공격 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "기본 공격",
                Description = $"적에게 기본 공격을 가한다",
                ID = 1001,
                IsAttack = true,
                PatternType = PatternType.Consecutive2,
                PatternValue = true, // 앞면
                AttackBonus = 1
            });
            
            // 메인 효과 기반 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = $"{_primaryEffectType} 부여",
                Description = $"적에게 {_primaryEffectType} 상태 부여",
                ID = 1002,
                IsAttack = true,
                PatternType = PatternType.Consecutive3,
                PatternValue = true, // 앞면
                AttackBonus = 2,
                StatusEffects = new StatusEffect.StatusEffectData[] 
                { 
                    new StatusEffect.StatusEffectData(_primaryEffectType, 2, 2) 
                }
            });
            
            // 방어 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "방어 태세",
                Description = "방어 자세를 취해 방어력 증가",
                ID = 1003,
                IsAttack = false,
                PatternType = PatternType.Consecutive2,
                PatternValue = false, // 뒷면
                DefenseBonus = 2
            });
            
            // 루멘 리퍼 등 특정 적에 대한 특수 패턴 추가
            if (CharacterName == "루멘 리퍼" && _primaryEffectType == StatusEffectType.Mark)
            {
                _availablePatterns.Add(new Pattern
                {
                    Name = "수확",
                    Description = "표식 수치만큼 연속 공격",
                    ID = 1004,
                    IsAttack = true,
                    PatternType = PatternType.Consecutive4,
                    PatternValue = true, // 앞면
                    AttackBonus = 3,
                    SpecialEffect = "표식 수치만큼 추가 공격"
                });
            }
        }
        
        /// <summary>
        /// 엘리트 적 패턴 생성
        /// </summary>
        private void CreateEliteEnemyPatterns()
        {
            // 일반 적 패턴 포함
            CreateNormalEnemyPatterns();
            
            // 강력한 공격 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "강화 공격",
                Description = "강력한 일격을 가한다",
                ID = 2001,
                IsAttack = true,
                PatternType = PatternType.Consecutive4,
                PatternValue = true, // 앞면
                AttackBonus = 4
            });
            
            // 보조 효과 기반 패턴
            if (_secondaryEffectType != StatusEffectType.None)
            {
                _availablePatterns.Add(new Pattern
                {
                    Name = $"{_secondaryEffectType} 강화",
                    Description = $"적에게 {_secondaryEffectType} 강화 효과 부여",
                    ID = 2002,
                    IsAttack = true,
                    PatternType = PatternType.Consecutive3,
                    PatternValue = true, // 앞면
                    AttackBonus = 3,
                    StatusEffects = new StatusEffect.StatusEffectData[] 
                    { 
                        new StatusEffect.StatusEffectData(_secondaryEffectType, 3, 2) 
                    }
                });
            }
            
            // 강화 방어 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "견고한 방어",
                Description = "강력한 방어 자세로 방어력 크게 증가",
                ID = 2003,
                IsAttack = false,
                PatternType = PatternType.Consecutive3,
                PatternValue = false, // 뒷면
                DefenseBonus = 4
            });
        }
        
        /// <summary>
        /// 미니보스 패턴 생성
        /// </summary>
        private void CreateMiniBossPatterns()
        {
            // 엘리트 패턴 포함
            CreateEliteEnemyPatterns();
            
            // 특수 공격 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "특수 공격",
                Description = "강력한 특수 공격",
                ID = 3001,
                IsAttack = true,
                PatternType = PatternType.Consecutive5,
                PatternValue = true, // 앞면
                AttackBonus = 6,
                StatusEffects = new StatusEffect.StatusEffectData[] 
                { 
                    new StatusEffect.StatusEffectData(_primaryEffectType, 3, 3),
                    new StatusEffect.StatusEffectData(_secondaryEffectType, 2, 2)
                }
            });
            
            // 매우 강력한 방어 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "절대 방어",
                Description = "거의 모든 공격을 막아내는 방어 태세",
                ID = 3002,
                IsAttack = false,
                PatternType = PatternType.AllOfOne,
                PatternValue = false, // 뒷면
                DefenseBonus = 6
            });
        }
        
        /// <summary>
        /// 보스 패턴 생성
        /// </summary>
        private void CreateBossPatterns()
        {
            // 기본 공격 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "압도적 일격",
                Description = "매우 강력한 공격",
                ID = 4001,
                IsAttack = true,
                PatternType = PatternType.Consecutive3,
                PatternValue = true, // 앞면
                AttackBonus = 5
            });
            
            // 다단 히트 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "연쇄 공격",
                Description = "여러 번의 연속 공격",
                ID = 4002,
                IsAttack = true,
                PatternType = PatternType.Consecutive4,
                PatternValue = true, // 앞면
                AttackBonus = 4,
                SpecialEffect = "2 연속 공격"
            });
            
            // 강력한 주 효과
            _availablePatterns.Add(new Pattern
            {
                Name = $"강력한 {_primaryEffectType}",
                Description = $"적에게 강력한 {_primaryEffectType} 상태 부여",
                ID = 4003,
                IsAttack = true,
                PatternType = PatternType.Consecutive3,
                PatternValue = true, // 앞면
                AttackBonus = 3,
                StatusEffects = new StatusEffect.StatusEffectData[] 
                { 
                    new StatusEffect.StatusEffectData(_primaryEffectType, 4, 3) 
                }
            });
            
            // 강력한 보조 효과
            if (_secondaryEffectType != StatusEffectType.None)
            {
                _availablePatterns.Add(new Pattern
                {
                    Name = $"강력한 {_secondaryEffectType}",
                    Description = $"적에게 강력한 {_secondaryEffectType} 상태 부여",
                    ID = 4004,
                    IsAttack = true,
                    PatternType = PatternType.Consecutive4,
                    PatternValue = true, // 앞면
                    AttackBonus = 3,
                    StatusEffects = new StatusEffect.StatusEffectData[] 
                    { 
                        new StatusEffect.StatusEffectData(_secondaryEffectType, 4, 3) 
                    }
                });
            }
            
            // 궁극의 방어
            _availablePatterns.Add(new Pattern
            {
                Name = "불가침 장벽",
                Description = "거의 완벽한 방어 상태가 된다",
                ID = 4005,
                IsAttack = false,
                PatternType = PatternType.AllOfOne,
                PatternValue = false, // 뒷면
                DefenseBonus = 8
            });
            
            // 필살기
            _availablePatterns.Add(new Pattern
            {
                Name = "멸망의 심판",
                Description = "엄청난 파괴력의 궁극기",
                ID = 4006,
                IsAttack = true,
                PatternType = PatternType.Alternating,
                PatternValue = true, // 앞면
                AttackBonus = 10,
                StatusEffects = new StatusEffect.StatusEffectData[] 
                { 
                    new StatusEffect.StatusEffectData(_primaryEffectType, 5, 3),
                    new StatusEffect.StatusEffectData(_secondaryEffectType, 5, 3),
                    new StatusEffect.StatusEffectData(StatusEffectType.Bleed, 3, 2)
                }
            });
        }
        
        /// <summary>
        /// 기본 패턴 생성 (기본 대비책)
        /// </summary>
        private void CreateDefaultPatterns()
        {
            // 기본 공격 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "약한 공격",
                Description = "힘없는 공격",
                ID = 1,
                IsAttack = true,
                PatternType = PatternType.Consecutive2,
                PatternValue = true, // 앞면
                AttackBonus = 1
            });
            
            // 기본 방어 패턴
            _availablePatterns.Add(new Pattern
            {
                Name = "약한 방어",
                Description = "허술한 방어 자세",
                ID = 2,
                IsAttack = false,
                PatternType = PatternType.Consecutive2,
                PatternValue = false, // 뒷면
                DefenseBonus = 1
            });
        }
        #endregion
        
        #region Override Methods
        /// <summary>
        /// 액티브 스킬 사용 (적은 코인 매니저를 직접 조작하지 않음)
        /// </summary>
        public override void UseActiveSkill(CoinManager coinManager)
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
