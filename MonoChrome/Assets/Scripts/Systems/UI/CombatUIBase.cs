using System.Collections.Generic;
using MonoChrome.Systems.Combat;
using MonoChrome.StatusEffects;
using UnityEngine;
using UnityEngine.UI;

namespace MonoChrome.Systems.UI
{
    /// <summary>
    /// Shared functionality for combat UI implementations.
    /// </summary>
    public class CombatUIBase : MonoBehaviour
    {
        [Header("Health Bars")]
        [SerializeField] protected Slider playerHealthBar;
        [SerializeField] protected Slider enemyHealthBar;
        [SerializeField] protected Text playerHealthText;
        [SerializeField] protected Text enemyHealthText;

        [Header("Coin UI")]
        [SerializeField] protected Transform coinContainer;
        [SerializeField] protected GameObject coinPrefab;

        [Header("Pattern UI")]
        [SerializeField] protected Transform patternContainer;
        [SerializeField] protected GameObject patternButtonPrefab;

        [Header("Combat Controls")]
        [SerializeField] protected Button activeSkillButton;
        [SerializeField] protected Button endTurnButton;
        [SerializeField] protected Text activeSkillText;
        [SerializeField] protected Text turnInfoText;
        [SerializeField] protected Text enemyIntentionText;

        [Header("Status Effects")]
        [SerializeField] protected Transform playerStatusEffectContainer;
        [SerializeField] protected Transform enemyStatusEffectContainer;
        [SerializeField] protected GameObject statusEffectPrefab;

        protected readonly List<GameObject> coinObjects = new();
        protected readonly List<GameObject> patternObjects = new();
        protected readonly List<GameObject> playerStatusEffectObjects = new();
        protected readonly List<GameObject> enemyStatusEffectObjects = new();

        protected CombatSystem combatManager;

        /// <summary>
        /// Update health bar values and text.
        /// </summary>
        public virtual void UpdateHealthBars(float playerHealth, float playerMaxHealth, float enemyHealth, float enemyMaxHealth)
        {
            if (playerHealthBar != null)
            {
                playerHealthBar.maxValue = playerMaxHealth;
                playerHealthBar.value = playerHealth;
            }
            if (playerHealthText != null)
                playerHealthText.text = $"{Mathf.RoundToInt(playerHealth)}/{Mathf.RoundToInt(playerMaxHealth)}";

            if (enemyHealthBar != null)
            {
                enemyHealthBar.maxValue = enemyMaxHealth;
                enemyHealthBar.value = enemyHealth;
            }
            if (enemyHealthText != null)
                enemyHealthText.text = $"{Mathf.RoundToInt(enemyHealth)}/{Mathf.RoundToInt(enemyMaxHealth)}";
        }

        /// <summary>
        /// Update coin UI elements.
        /// </summary>
        public virtual void UpdateCoinUI(List<bool> coinResults)
        {
            if (coinContainer == null || coinPrefab == null) return;

            foreach (GameObject obj in coinObjects)
                Destroy(obj);
            coinObjects.Clear();

            for (int i = 0; i < coinResults.Count; i++)
            {
                GameObject coinObj = Instantiate(coinPrefab, coinContainer);
                Image img = coinObj.GetComponent<Image>();
                Text txt = coinObj.GetComponentInChildren<Text>();
                if (img != null)
                    img.color = coinResults[i] ? Color.red : Color.blue;
                if (txt != null)
                    txt.text = coinResults[i] ? "공격" : "방어";
                coinObjects.Add(coinObj);
            }
        }

        /// <summary>
        /// Update pattern buttons.
        /// </summary>
        public virtual void UpdatePatternUI(List<Pattern> patterns)
        {
            if (patternContainer == null || patternButtonPrefab == null) return;

            foreach (GameObject obj in patternObjects)
                Destroy(obj);
            patternObjects.Clear();

            for (int i = 0; i < patterns.Count; i++)
            {
                Pattern pattern = patterns[i];
                GameObject patternObj = Instantiate(patternButtonPrefab, patternContainer);
                Pattern localPattern = pattern;
                Button button = patternObj.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => OnPatternSelected(localPattern));
                }
                Text text = patternObj.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = pattern.Name;
                patternObjects.Add(patternObj);
            }
        }

        protected virtual void OnPatternSelected(Pattern pattern)
        {
            combatManager?.ExecutePlayerPattern(pattern);
        }

        /// <summary>
        /// Update displayed turn count.
        /// </summary>
        public virtual void UpdateTurnCounter(int turnCount)
        {
            if (turnInfoText != null)
                turnInfoText.text = $"Turn: {turnCount}";
        }

        /// <summary>
        /// Enable or disable the active skill button.
        /// </summary>
        public virtual void UpdateActiveSkillButton(bool isAvailable, string skillName = "액티브 스킬")
        {
            if (activeSkillButton != null)
                activeSkillButton.interactable = isAvailable;

            if (activeSkillText != null)
            {
                activeSkillText.text = skillName;
                activeSkillText.color = isAvailable ? Color.white : Color.gray;
            }
        }

        /// <summary>
        /// Show enemy intention text.
        /// </summary>
        public virtual void ShowEnemyIntention(string intention)
        {
            if (enemyIntentionText != null)
                enemyIntentionText.text = intention;
        }

        /// <summary>
        /// Update status effect icons.
        /// </summary>
        public virtual void UpdateStatusEffectsUI(List<StatusEffect> playerEffects, List<StatusEffect> enemyEffects)
        {
            ClearStatusEffects(playerStatusEffectObjects);
            ClearStatusEffects(enemyStatusEffectObjects);

            if (playerStatusEffectContainer != null)
                CreateStatusEffectIcons(playerEffects, playerStatusEffectContainer, playerStatusEffectObjects);
            if (enemyStatusEffectContainer != null)
                CreateStatusEffectIcons(enemyEffects, enemyStatusEffectContainer, enemyStatusEffectObjects);
        }

        private void CreateStatusEffectIcons(List<StatusEffect> effects, Transform container, List<GameObject> list)
        {
            if (statusEffectPrefab == null) return;

            for (int i = 0; i < effects.Count; i++)
            {
                GameObject obj = Instantiate(statusEffectPrefab, container);
                StatusEffect effect = effects[i];
                Text text = obj.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = $"{effect.EffectType} {effect.Magnitude}";
                list.Add(obj);
            }
        }

        private void ClearStatusEffects(List<GameObject> list)
        {
            foreach (var obj in list)
                if (obj != null) Destroy(obj);
            list.Clear();
        }
    }
}
