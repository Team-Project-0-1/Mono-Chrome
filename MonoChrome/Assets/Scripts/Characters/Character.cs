using System;
using System.Collections;
using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 모든 캐릭터(플레이어 및 적)의 기본 클래스
    /// 체력, 공격력, 방어력 등의 기본 속성과 상태 효과를 관리한다.
    /// </summary>
    [System.Serializable]
    public abstract class Character
    {
        #region Basic Properties
        // 기본 정보
        public string CharacterName { get; protected set; }
        public CharacterType Type { get; protected set; }
        
        // 기본 능력치
        [SerializeField] protected int _maxHealth;
        [SerializeField] protected int _baseAttackPower;
        [SerializeField] protected int _baseDefensePower;
        [SerializeField] protected int _currentHealth;
        
        // 현재 상태
        [SerializeField] protected int _currentDefense;
        
        // 스킬 쿨다운
        [SerializeField] protected int _activeSkillCooldown;
        [SerializeField] protected int _maxActiveSkillCooldown = 3; // 기본 3턴
        
        // 상태 효과 목록
        protected Dictionary<StatusEffectType, StatusEffect> _statusEffects = new Dictionary<StatusEffectType, StatusEffect>();
        
        // 프로퍼티
        public int MaxHealth => _maxHealth;
        public int CurrentHealth => _currentHealth;
        public int AttackPower => _baseAttackPower;
        public int DefensePower => _baseDefensePower;
        public int CurrentDefense => _currentDefense;
        #endregion
        
        #region Events
        // 체력 변화 이벤트
        public event Action<int, int> OnHealthChanged; // 현재 체력, 최대 체력
        
        // 방어력 변화 이벤트
        public event Action<int> OnDefenseChanged; // 현재 방어력
        
        // 피해 받음 이벤트
        public event Action<int, bool> OnDamageTaken; // 피해량, 방어력 무시 여부
        
        // 상태 효과 변화 이벤트
        public event Action<StatusEffect> OnStatusEffectAdded;
        public event Action<StatusEffectType> OnStatusEffectRemoved;
        
        // 사망 이벤트
        public event Action OnDeath;
        #endregion
        
        #region Initialization
        /// <summary>
        /// 캐릭터 초기화
        /// </summary>
        protected Character(string name, CharacterType type, int maxHealth, int attackPower, int defensePower)
        {
            CharacterName = name;
            Type = type;
            _maxHealth = maxHealth;
            _baseAttackPower = attackPower;
            _baseDefensePower = defensePower;
            _currentHealth = maxHealth;
            _currentDefense = 0;
            _activeSkillCooldown = 0;
        }
        
        /// <summary>
        /// 전투 시작 시 초기화
        /// </summary>
        public virtual void ResetForCombat()
        {
            _currentHealth = _maxHealth;
            _currentDefense = 0;
            _activeSkillCooldown = 0;
            _statusEffects.Clear();
            
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            OnDefenseChanged?.Invoke(_currentDefense);
            
            Debug.Log($"{CharacterName} reset for combat");
        }
        #endregion
        
        #region Combat Actions
        /// <summary>
        /// 피해를 받는다
        /// </summary>
        /// <param name="damage">피해량</param>
        /// <param name="ignoreDefense">방어력 무시 여부</param>
        public virtual void TakeDamage(int damage, bool ignoreDefense = false)
        {
            if (damage <= 0)
            {
                return;
            }
            
            int actualDamage = damage;
            
            // 상처(취약) 효과가 있으면 피해 증가
            if (HasStatusEffect(StatusEffectType.Bleed))
            {
                actualDamage = (int)(damage * 1.5f); // 50% 증가
                Debug.Log($"{CharacterName} takes increased damage due to Bleed: {damage} -> {actualDamage}");
            }
            
            // 방어력 적용 (방어력 무시가 아닌 경우)
            if (!ignoreDefense && _currentDefense > 0)
            {
                if (actualDamage <= _currentDefense)
                {
                    // 방어력이 피해보다 높으면 방어력만 감소
                    _currentDefense -= actualDamage;
                    actualDamage = 0;
                }
                else
                {
                    // 방어력이 피해보다 낮으면 초과 피해만 체력에 적용
                    actualDamage -= _currentDefense;
                    _currentDefense = 0;
                }
                
                OnDefenseChanged?.Invoke(_currentDefense);
            }
            
            if (actualDamage > 0)
            {
                // 체력 감소
                _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);
                OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
                
                Debug.Log($"{CharacterName} took {actualDamage} damage, HP: {_currentHealth}/{_maxHealth}");
                
                // 반격 효과 처리
                ProcessCounterEffects();
                
                // 피해 이벤트 발생
                OnDamageTaken?.Invoke(actualDamage, ignoreDefense);
                
                // 사망 체크
                if (_currentHealth <= 0)
                {
                    Die();
                }
            }
            else
            {
                Debug.Log($"{CharacterName} blocked all damage with defense");
            }
        }
        
        /// <summary>
        /// 반격 효과 처리
        /// </summary>
        private void ProcessCounterEffects()
        {
            // 반격 효과가 있으면 반격 피해 처리
            StatusEffect counterEffect = GetStatusEffect(StatusEffectType.Counter);
            if (counterEffect != null && counterEffect.Source != null)
            {
                int counterDamage = counterEffect.Magnitude;
                counterEffect.Source.TakeDamage(counterDamage, true); // 방어력 무시 피해
                Debug.Log($"{CharacterName} counters with {counterDamage} damage to {counterEffect.Source.CharacterName}");
            }
        }
        
        /// <summary>
        /// 체력을 회복한다
        /// </summary>
        /// <param name="amount">회복량</param>
        public virtual void Heal(int amount)
        {
            if (amount <= 0 || _currentHealth <= 0)
            {
                return;
            }
            
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
            
            Debug.Log($"{CharacterName} healed for {amount}, HP: {_currentHealth}/{_maxHealth}");
        }
        
        /// <summary>
        /// 방어력을 추가한다
        /// </summary>
        /// <param name="amount">방어력 증가량</param>
        public virtual void AddDefense(int amount)
        {
            if (amount <= 0)
            {
                return;
            }
            
            _currentDefense += amount;
            OnDefenseChanged?.Invoke(_currentDefense);
            
            Debug.Log($"{CharacterName} gained {amount} defense, total: {_currentDefense}");
        }
        
        /// <summary>
        /// 캐릭터가 사망한다
        /// </summary>
        protected virtual void Die()
        {
            Debug.Log($"{CharacterName} has died");
            OnDeath?.Invoke();
        }
        
        /// <summary>
        /// 액티브 스킬을 사용하여 동전을 조작한다
        /// </summary>
        /// <param name="coinManager">동전 관리자</param>
        public abstract void UseActiveSkill(CoinManager coinManager);
        
        /// <summary>
        /// 액티브 스킬 쿨다운을 업데이트한다
        /// </summary>
        public virtual void UpdateSkillCooldown()
        {
            if (_activeSkillCooldown > 0)
            {
                _activeSkillCooldown--;
                Debug.Log($"{CharacterName} skill cooldown reduced to {_activeSkillCooldown}");
            }
        }
        
        /// <summary>
        /// 액티브 스킬 사용 가능 여부
        /// </summary>
        public bool IsActiveSkillAvailable()
        {
            return _activeSkillCooldown <= 0;
        }
        
        /// <summary>
        /// 액티브 스킬 사용 후 쿨다운 적용
        /// </summary>
        protected void ApplyActiveSkillCooldown()
        {
            _activeSkillCooldown = _maxActiveSkillCooldown;
            Debug.Log($"{CharacterName} used active skill, cooldown set to {_activeSkillCooldown}");
        }
        #endregion
        
        #region Status Effect Methods
        /// <summary>
        /// 상태 효과를 추가한다
        /// </summary>
        public void AddStatusEffect(StatusEffect effect)
        {
            if (effect == null)
            {
                return;
            }
            
            _statusEffects[effect.EffectType] = effect;
            OnStatusEffectAdded?.Invoke(effect);
            
            Debug.Log($"{CharacterName} gained {effect.EffectType} effect, magnitude: {effect.Magnitude}, duration: {effect.RemainingDuration}");
        }
        
        /// <summary>
        /// 상태 효과를 제거한다
        /// </summary>
        public void RemoveStatusEffect(StatusEffectType effectType)
        {
            if (_statusEffects.ContainsKey(effectType))
            {
                _statusEffects.Remove(effectType);
                OnStatusEffectRemoved?.Invoke(effectType);
                
                Debug.Log($"{CharacterName} lost {effectType} effect");
            }
        }
        
        /// <summary>
        /// 상태 효과를 가져온다
        /// </summary>
        public StatusEffect GetStatusEffect(StatusEffectType effectType)
        {
            if (_statusEffects.TryGetValue(effectType, out StatusEffect effect))
            {
                return effect;
            }
            
            return null;
        }
        
        /// <summary>
        /// 특정 상태 효과가 있는지 확인한다
        /// </summary>
        public bool HasStatusEffect(StatusEffectType effectType)
        {
            return _statusEffects.ContainsKey(effectType);
        }
        
        /// <summary>
        /// 모든 상태 효과를 가져온다
        /// </summary>
        public List<StatusEffect> GetAllStatusEffects()
        {
            List<StatusEffect> effects = new List<StatusEffect>();
            
            foreach (var effect in _statusEffects.Values)
            {
                effects.Add(effect);
            }
            
            return effects;
        }
        
        /// <summary>
        /// 모든 상태 효과를 제거한다
        /// </summary>
        public void ClearAllStatusEffects()
        {
            List<StatusEffectType> effectTypes = new List<StatusEffectType>(_statusEffects.Keys);
            
            foreach (var effectType in effectTypes)
            {
                RemoveStatusEffect(effectType);
            }
            
            Debug.Log($"Cleared all status effects from {CharacterName}");
        }
        
        /// <summary>
        /// 상태 효과를 업데이트한다 (턴마다 호출)
        /// </summary>
        public virtual void UpdateStatusEffects()
        {
            List<StatusEffectType> expiredEffects = new List<StatusEffectType>();
            
            // 각 상태 효과 업데이트
            foreach (var kvp in _statusEffects)
            {
                StatusEffect effect = kvp.Value;
                
                // 지속 시간 감소
                effect.RemainingDuration--;
                
                // 지속 시간이 끝났으면 제거 목록에 추가
                if (effect.RemainingDuration <= 0)
                {
                    expiredEffects.Add(kvp.Key);
                }
            }
            
            // 만료된 효과 제거
            foreach (var effectType in expiredEffects)
            {
                RemoveStatusEffect(effectType);
            }
        }
        
        /// <summary>
        /// ScriptableObject 기반 상태 효과를 추가한다
        /// </summary>
        public void AddStatusEffect(StatusEffectSO effectSO, int magnitude = 1)
        {
            if (effectSO == null)
            {
                Debug.LogError("Null StatusEffectSO passed to AddStatusEffect");
                return;
            }
    
            // ScriptableObject에서 StatusEffect 인스턴스 생성
            StatusEffect effect = effectSO.CreateEffect(magnitude, -1, null);
    
            // 기존 AddStatusEffect 메서드 호출
            AddStatusEffect(effect);
        }
        #endregion
        
        #region Pattern Methods
        /// <summary>
        /// 사용 가능한 패턴(족보) 목록을 가져온다
        /// </summary>
        public abstract List<Pattern> GetAvailablePatterns();
        #endregion
    }
}