using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.StatusEffects
{
    /// <summary>
    /// 상태 효과 관리 시스템
    /// 상태 효과 적용, 갱신, 만료 등을 관리
    /// </summary>
    public class StatusEffectManager : MonoBehaviour
    {
        #region Singleton
        private static StatusEffectManager _instance;
        
        public static StatusEffectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("StatusEffectManager");
                    _instance = go.AddComponent<StatusEffectManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 초기화
            Initialize();
        }
        #endregion
        
        [Header("ScriptableObject 설정")]
        [SerializeField] private string statusEffectResourcePath = "StatusEffects";
        [SerializeField] private bool useScriptableObjects = true;
        
        // 상태 효과 캐시
        private Dictionary<string, StatusEffectSO> effectCache = new Dictionary<string, StatusEffectSO>();
        
        // 프로퍼티
        public bool UseScriptableObjects { get => useScriptableObjects; set => useScriptableObjects = value; }
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            // ScriptableObject 모드인 경우 리소스에서 효과 로드
            if (useScriptableObjects)
            {
                LoadStatusEffectsFromResources();
            }
            
            Debug.Log("StatusEffectManager: Initialized");
        }
        
        /// <summary>
        /// Resources 폴더에서 상태 효과 ScriptableObject 로드
        /// </summary>
        private void LoadStatusEffectsFromResources()
        {
            StatusEffectSO[] effects = Resources.LoadAll<StatusEffectSO>(statusEffectResourcePath);
            
            if (effects != null && effects.Length > 0)
            {
                effectCache.Clear();
                
                foreach (StatusEffectSO effect in effects)
                {
                    // 유효한 ID가 있는 경우에만 캐시에 추가
                    if (!string.IsNullOrEmpty(effect.effectName))
                    {
                        effectCache[effect.effectName] = effect;
                        Debug.Log($"StatusEffectManager: Loaded effect '{effect.effectName}'");
                    }
                    else
                    {
                        Debug.LogWarning($"StatusEffectManager: Effect has no name: {effect.name}");
                    }
                }
                
                Debug.Log($"StatusEffectManager: Loaded {effectCache.Count} effects from resources");
            }
            else
            {
                Debug.LogWarning($"StatusEffectManager: No effects found in resources path: {statusEffectResourcePath}");
            }
        }
        
        /// <summary>
        /// 대상에게 상태 효과 적용
        /// </summary>
        public void ApplyEffect(Character target, StatusEffectType effectType, int magnitude = 1, int duration = -1)
        {
            if (target == null)
            {
                Debug.LogError("StatusEffectManager: Cannot apply effect to null target");
                return;
            }
            
            if (useScriptableObjects)
            {
                // ScriptableObject 방식 사용
                StatusEffectSO effectSO = GetStatusEffectByType(effectType);
                
                if (effectSO != null)
                {
                    Debug.Log($"StatusEffectManager: Applying effect {effectType} to {target.CharacterName}");
                    
                    // 기본 지속 시간 사용
                    if (duration < 0)
                    {
                        duration = effectSO.defaultDuration;
                    }
                    
                    // 대상에게 효과 적용
                    target.AddStatusEffect(effectSO, magnitude);
                }
                else
                {
                    Debug.LogWarning($"StatusEffectManager: Effect {effectType} not found");
                }
            }
            else
            {
                // 직접 상태 효과 생성 방식 사용
                Debug.Log($"StatusEffectManager: Applying direct effect {effectType} to {target.CharacterName}");
                
                // 기본 지속 시간 설정
                if (duration < 0)
                {
                    duration = GetDefaultDuration(effectType);
                }
                
                // 상태 효과 생성 및 적용
                StatusEffect effect = new StatusEffect(effectType, magnitude, duration);
                target.AddStatusEffect(effect);
            }
        }
        
        /// <summary>
        /// 이름으로 상태 효과 가져오기
        /// </summary>
        public StatusEffectSO GetStatusEffectByName(string effectName)
        {
            if (string.IsNullOrEmpty(effectName))
                return null;
                
            if (effectCache.TryGetValue(effectName, out StatusEffectSO effect))
            {
                return effect;
            }
            
            Debug.LogWarning($"StatusEffectManager: Effect '{effectName}' not found in cache");
            return null;
        }
        
        /// <summary>
        /// 타입으로 상태 효과 가져오기
        /// </summary>
        public StatusEffectSO GetStatusEffectByType(StatusEffectType effectType)
        {
            foreach (StatusEffectSO effect in effectCache.Values)
            {
                if (effect.effectType == effectType)
                {
                    return effect;
                }
            }
            
            Debug.LogWarning($"StatusEffectManager: Effect type {effectType} not found in cache");
            return null;
        }
        
        /// <summary>
        /// 모든 상태 효과 가져오기
        /// </summary>
        public List<StatusEffectSO> GetAllStatusEffects()
        {
            return new List<StatusEffectSO>(effectCache.Values);
        }
        
        /// <summary>
        /// 상태 효과의 기본 지속 시간 가져오기
        /// </summary>
        private int GetDefaultDuration(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify:
                    return 3;
                case StatusEffectType.Resonance:
                    return 2;
                case StatusEffectType.Mark:
                    return 3;
                case StatusEffectType.Bleed:
                    return 3;
                case StatusEffectType.Counter:
                    return 2;
                case StatusEffectType.Crush:
                    return 3;
                case StatusEffectType.Curse:
                    return 3;
                case StatusEffectType.Seal:
                    return 2;
                case StatusEffectType.Poison:
                    return 3;
                case StatusEffectType.Burn:
                    return 1;
                case StatusEffectType.MultiAttack:
                    return 1;
                case StatusEffectType.Wound:
                    return 2;
                case StatusEffectType.Fracture:
                    return 2;
                case StatusEffectType.Regeneration:
                    return 1;
                default:
                    return 1;
            }
        }
        
        /// <summary>
        /// 상태 효과 생성
        /// </summary>
        public StatusEffect CreateEffect(StatusEffectType effectType, int magnitude = 1, int duration = -1)
        {
            if (duration < 0)
            {
                duration = GetDefaultDuration(effectType);
            }
            
            return new StatusEffect(effectType, magnitude, duration);
        }
        
        /// <summary>
        /// 캐릭터의 상태 효과 업데이트 (턴 종료 시 호출)
        /// </summary>
        public void UpdateEffects(Character character)
        {
            if (character == null)
                return;
                
            character.UpdateStatusEffects();
        }
        
        /// <summary>
        /// 상태 효과 타입별 기본 수치 가져오기
        /// </summary>
        public int GetDefaultMagnitude(StatusEffectType effectType)
        {
            switch (effectType)
            {
                case StatusEffectType.Amplify:
                    return 1;
                case StatusEffectType.Resonance:
                    return 1;
                case StatusEffectType.Mark:
                    return 2;
                case StatusEffectType.Bleed:
                    return 2;
                case StatusEffectType.Counter:
                    return 3;
                case StatusEffectType.Crush:
                    return 1;
                case StatusEffectType.Curse:
                    return 2;
                case StatusEffectType.Seal:
                    return 1;
                case StatusEffectType.Poison:
                    return 2;
                case StatusEffectType.Burn:
                    return 3;
                case StatusEffectType.MultiAttack:
                    return 2;
                case StatusEffectType.Wound:
                    return 1;
                case StatusEffectType.Fracture:
                    return 1;
                case StatusEffectType.Regeneration:
                    return 3;
                default:
                    return 1;
            }
        }
        
        /// <summary>
        /// 상태 효과 중첩 (기존 효과가 있으면 값을 증가시키고, 없으면 새로 추가)
        /// </summary>
        public void StackEffect(Character target, StatusEffectType effectType, int magnitude = 1, int duration = -1)
        {
            if (target == null)
            {
                Debug.LogError("StatusEffectManager: Cannot stack effect on null target");
                return;
            }
            
            // 기본 지속 시간 설정
            if (duration < 0)
            {
                duration = GetDefaultDuration(effectType);
            }
            
            // 기존 효과 찾기
            StatusEffect existingEffect = target.GetStatusEffect(effectType);
            
            if (existingEffect != null)
            {
                // 기존 효과 업데이트
                existingEffect.IncreaseMagnitude(magnitude);
                
                // 지속 시간은 신규 효과의 지속 시간으로 갱신 (더 긴 시간으로)
                if (duration > existingEffect.RemainingDuration)
                {
                    existingEffect.RemainingDuration = duration;
                }
                
                Debug.Log($"StatusEffectManager: Stacked {effectType} on {target.CharacterName} to magnitude {existingEffect.Magnitude}");
            }
            else
            {
                // 새로운 효과 추가
                StatusEffect newEffect = new StatusEffect(effectType, magnitude, duration);
                target.AddStatusEffect(newEffect);
                
                Debug.Log($"StatusEffectManager: Added new {effectType} effect to {target.CharacterName}");
            }
        }
    }
}