using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MonoChrome
{
    public class PlayerManager : MonoBehaviour
    {
        [Header("Player Stats")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private int currentHealth;
        [SerializeField] private int baseAttackPower = 5;
        [SerializeField] private int baseDefensePower = 3;
        
        [Header("Character")]
        [SerializeField] private MonoChrome.PlayerCharacterType characterType;
        [SerializeField] private List<PatternData> unlockedPatterns = new List<PatternData>();
        
        [Header("Status Effects")]
        [SerializeField] private List<DungeonStatusEffect> activeStatusEffects = new List<DungeonStatusEffect>();
        
        [Header("References")]
        [SerializeField] private DungeonUI dungeonUI;

        // Events
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);
        public event HealthChangedEventHandler OnHealthChanged;
        
        public delegate void StatusEffectsChangedEventHandler(List<DungeonStatusEffect> activeEffects);
        public event StatusEffectsChangedEventHandler OnStatusEffectsChanged;
        
        private void Awake()
        {
            // Initialize health at start
            currentHealth = maxHealth;
            
            if (dungeonUI == null)
            {
                dungeonUI = FindObjectOfType<DungeonUI>();
            }
        }
        
        private void Start()
        {
            // Set initial status effects based on character type
            InitializeCharacterStatusEffects();
            
            // Update UI with initial values
            UpdateUI();
        }
        
        private void InitializeCharacterStatusEffects()
        {
            // Initialize based on character type
            switch (characterType)
            {
                case PlayerCharacterType.Warrior:
                    // Warrior starts with amplify status
                    AddStatusEffect(new DungeonStatusEffect("Amplify", 1, -1));
                    break;
                case PlayerCharacterType.Rogue:
                    // Rogue starts with mark status
                    AddStatusEffect(new DungeonStatusEffect("Mark", 1, -1));
                    break;
                case PlayerCharacterType.Mage:
                    // Mage starts with curse status
                    AddStatusEffect(new DungeonStatusEffect("Curse", 1, -1));
                    break;
                case PlayerCharacterType.Tank:
                    // Tank starts with counter status
                    AddStatusEffect(new DungeonStatusEffect("Counter", 1, -1));
                    break;
            }
        }        
        private void UpdateUI()
        {
            if (dungeonUI != null)
            {
                // Update health display
                dungeonUI.UpdatePlayerStatus(currentHealth, maxHealth);
                
                // Update status effects
                dungeonUI.UpdateStatusEffects(activeStatusEffects);
            }
        }
        
        public int GetMaxHealth()
        {
            return maxHealth;
        }
        
        public int GetCurrentHealth()
        {
            return currentHealth;
        }
        
        public int GetAttackPower()
        {
            int totalAttack = baseAttackPower;
            
            // Apply status effect modifiers
            foreach (var effect in activeStatusEffects)
            {
                if (effect.name == "Amplify")
                {
                    totalAttack += effect.stacks;
                }
                else if (effect.name == "Fracture")
                {
                    // Reduce attack power due to fracture effect
                    totalAttack = Mathf.CeilToInt(totalAttack * 0.75f); // 25% reduction
                }
            }
            
            return Mathf.Max(1, totalAttack); // Ensure minimum 1 attack
        }
        
        public int GetDefensePower()
        {
            int totalDefense = baseDefensePower;
            
            // Apply status effect modifiers
            foreach (var effect in activeStatusEffects)
            {
                if (effect.name == "Amplify")
                {
                    totalDefense += effect.stacks;
                }
                else if (effect.name == "Crush")
                {
                    // Reduce defense power due to crush effect
                    totalDefense = Mathf.CeilToInt(totalDefense * 0.75f); // 25% reduction
                }
            }
            
            return Mathf.Max(0, totalDefense); // Allow 0 defense
        }
        
        public void TakeDamage(int damage, bool ignoreDefense = false)
        {
            // Apply defense to reduce damage
            int damageTaken = ignoreDefense ? damage : Mathf.Max(1, damage - GetDefensePower());
            
            // Check for wound effect (increases damage taken)
            foreach (var effect in activeStatusEffects)
            {
                if (effect.name == "Wound")
                {
                    damageTaken = Mathf.CeilToInt(damageTaken * 1.5f); // 50% more damage
                    break;
                }
            }
            
            // Apply damage
            currentHealth = Mathf.Max(0, currentHealth - damageTaken);
            
            // Trigger event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Update UI
            UpdateUI();
            
            // Check for death
            if (currentHealth <= 0)
            {
                Die();
            }
        }
        
        public void Heal(int amount)
        {
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            
            // Trigger event
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            // Update UI
            UpdateUI();
        }
        
        public void AddStatusEffect(DungeonStatusEffect effect)
        {
            // Check if effect already exists
            DungeonStatusEffect existingEffect = activeStatusEffects.Find(e => e.name == effect.name);
            
            if (existingEffect != null)
            {
                // Stack existing effect
                existingEffect.stacks += effect.stacks;
                
                // Update duration if needed
                if (effect.duration > existingEffect.duration)
                {
                    existingEffect.duration = effect.duration;
                }
            }
            else
            {
                // Add new effect
                activeStatusEffects.Add(effect);
            }
            
            // Trigger event
            OnStatusEffectsChanged?.Invoke(activeStatusEffects);
            
            // Update UI
            UpdateUI();
        }
        
        public void RemoveStatusEffect(string effectName)
        {
            DungeonStatusEffect effectToRemove = activeStatusEffects.Find(e => e.name == effectName);
            
            if (effectToRemove != null)
            {
                activeStatusEffects.Remove(effectToRemove);
                
                // Trigger event
                OnStatusEffectsChanged?.Invoke(activeStatusEffects);
                
                // Update UI
                UpdateUI();
            }
        }
        
        public void UpdateStatusEffects()
        {
            // Update durations and remove expired effects
            for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
            {
                DungeonStatusEffect effect = activeStatusEffects[i];
                
                // Skip permanent effects (duration -1)
                if (effect.duration == -1)
                {
                    continue;
                }
                
                effect.duration--;
                
                if (effect.duration <= 0)
                {
                    activeStatusEffects.RemoveAt(i);
                }
            }
            
            // Trigger event
            OnStatusEffectsChanged?.Invoke(activeStatusEffects);
            
            // Update UI
            UpdateUI();
        }
        
        public List<PatternData> GetUnlockedPatterns()
        {
            return unlockedPatterns;
        }
        
        public void UnlockPattern(PatternData pattern)
        {
            if (!unlockedPatterns.Exists(p => p.patternName == pattern.patternName))
            {
                unlockedPatterns.Add(pattern);
                Debug.Log($"Unlocked new pattern: {pattern.patternName}");
            }
        }
        
        private void Die()
        {
            Debug.Log("Player has died!");
            
            // Implement game over logic
            // For now, reload the scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }
    
    
    [System.Serializable]
    public class PatternData
    {
        public string patternName;
        public MonoChrome.PlayerPatternType patternType;
        public MonoChrome.CoinFace requiredFace;
        public int effectValue;
        public string description;
        
        // Method to check if a coin pattern matches this pattern data
        public bool MatchesPattern(MonoChrome.CoinFace[] coins)
        {
            switch (patternType)
            {
                case PlayerPatternType.TwoInARow:
                    return HasConsecutiveFaces(coins, requiredFace, 2);
                case PlayerPatternType.ThreeInARow:
                    return HasConsecutiveFaces(coins, requiredFace, 3);
                case PlayerPatternType.FourInARow:
                    return HasConsecutiveFaces(coins, requiredFace, 4);
                case PlayerPatternType.FiveInARow:
                    return HasConsecutiveFaces(coins, requiredFace, 5);
                case PlayerPatternType.OnlyOneFace:
                    return HasOnlyOneFace(coins, requiredFace);
                case PlayerPatternType.Alternating:
                    return HasAlternatingFaces(coins);
                default:
                    return false;
            }
        }
        
        private bool HasConsecutiveFaces(MonoChrome.CoinFace[] coins, MonoChrome.CoinFace face, int count)
        {
            int consecutiveCount = 0;
            
            for (int i = 0; i < coins.Length; i++)
            {
                if (coins[i] == face)
                {
                    consecutiveCount++;
                    if (consecutiveCount >= count)
                    {
                        return true;
                    }
                }
                else
                {
                    consecutiveCount = 0;
                }
            }
            
            return false;
        }
        
        private bool HasOnlyOneFace(MonoChrome.CoinFace[] coins, MonoChrome.CoinFace face)
        {
            bool foundOneFace = false;
            
            for (int i = 0; i < coins.Length; i++)
            {
                if (coins[i] == face)
                {
                    // If we've already found this face, it's not unique
                    if (foundOneFace)
                    {
                        return false;
                    }
                    
                    foundOneFace = true;
                }
            }
            
            return foundOneFace;
        }
        
        private bool HasAlternatingFaces(MonoChrome.CoinFace[] coins)
        {
            if (coins.Length < 2)
            {
                return false;
            }
            
            for (int i = 1; i < coins.Length; i++)
            {
                if (coins[i] == coins[i - 1])
                {
                    return false;
                }
            }
            
            return true;
        }
    }
    
    // 이 열거형은 Core.PatternType과 구분하기 위한 플레이어 전용 패턴 유형입니다.
    public enum PlayerPatternType
    {
        TwoInARow,
        ThreeInARow,
        FourInARow,
        FiveInARow,
        OnlyOneFace,
        Alternating
    }
}