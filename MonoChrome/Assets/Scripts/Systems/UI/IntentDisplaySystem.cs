using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MonoChrome.AI;
using MonoChrome.Data;

namespace MonoChrome.Systems.UI
{
    /// <summary>
    /// 몬스터의 다음 행동 의도를 UI에 표시하는 시스템
    /// AIManager와 연동하여 몬스터가 다음 턴에 할 행동을 미리 보여준다.
    /// </summary>
    public class IntentDisplaySystem : MonoBehaviour
    {
        [Header("UI 컴포넌트")]
        [SerializeField] private Transform intentContainer;
        [SerializeField] private GameObject intentPrefab;
        
        [Header("의도 아이콘")]
        [SerializeField] private Sprite attackIcon;
        [SerializeField] private Sprite defenseIcon;
        [SerializeField] private Sprite statusIcon;
        [SerializeField] private Sprite specialIcon;
        [SerializeField] private Sprite unknownIcon;
        
        [Header("색상 설정")]
        [SerializeField] private Color attackColor = Color.red;
        [SerializeField] private Color defenseColor = Color.blue;
        [SerializeField] private Color statusColor = Color.magenta;
        [SerializeField] private Color specialColor = Color.yellow;
        [SerializeField] private Color unknownColor = Color.gray;
        
        // 의도 UI 캐시
        private Dictionary<Character, IntentUI> intentUICache = new Dictionary<Character, IntentUI>();
        
        #region Unity Lifecycle
        private void Start()
        {
            InitializeIntentSystem();
        }
        
        private void OnDestroy()
        {
            CleanupIntentSystem();
        }
        #endregion
        
        #region Initialization
        /// <summary>
        /// 의도 표시 시스템 초기화
        /// </summary>
        private void InitializeIntentSystem()
        {
            if (intentContainer == null)
            {
                Debug.LogError("IntentDisplaySystem: intentContainer가 설정되지 않았습니다!");
                return;
            }
            
            if (intentPrefab == null)
            {
                Debug.LogError("IntentDisplaySystem: intentPrefab이 설정되지 않았습니다!");
                return;
            }
            
            Debug.Log("IntentDisplaySystem: 의도 표시 시스템 초기화 완료");
        }
        
        /// <summary>
        /// 의도 표시 시스템 정리
        /// </summary>
        private void CleanupIntentSystem()
        {
            foreach (var intentUI in intentUICache.Values)
            {
                if (intentUI?.gameObject != null)
                {
                    Destroy(intentUI.gameObject);
                }
            }
            
            intentUICache.Clear();
        }
        #endregion
        
        #region Intent Display
        /// <summary>
        /// 몬스터의 의도를 표시한다
        /// </summary>
        /// <param name="monster">대상 몬스터</param>
        /// <param name="player">플레이어 (AI 계산용)</param>
        public void DisplayIntent(Character monster, Character player)
        {
            if (monster == null || player == null)
            {
                Debug.LogError("IntentDisplaySystem: monster 또는 player가 null입니다");
                return;
            }
            
            // AI Manager를 통해 몬스터의 의도 결정
            MonsterPatternSO intent = AIManager.Instance.DetermineIntent(monster, player);
            
            if (intent == null)
            {
                Debug.LogWarning($"IntentDisplaySystem: {monster.CharacterName}의 의도를 결정할 수 없습니다");
                ShowUnknownIntent(monster);
                return;
            }
            
            // 의도 UI 생성 또는 업데이트
            IntentUI intentUI = GetOrCreateIntentUI(monster);
            if (intentUI != null)
            {
                UpdateIntentUI(intentUI, intent);
            }
        }
        
        /// <summary>
        /// 몬스터의 현재 의도를 새로고침한다
        /// </summary>
        /// <param name="monster">대상 몬스터</param>
        public void RefreshIntent(Character monster)
        {
            if (monster == null) return;
            
            MonsterPatternSO currentIntent = AIManager.Instance.GetCurrentIntent(monster);
            
            if (currentIntent == null)
            {
                ShowUnknownIntent(monster);
                return;
            }
            
            IntentUI intentUI = GetOrCreateIntentUI(monster);
            if (intentUI != null)
            {
                UpdateIntentUI(intentUI, currentIntent);
            }
        }
        
        /// <summary>
        /// 모든 몬스터의 의도를 표시한다
        /// </summary>
        /// <param name="monsters">몬스터 목록</param>
        /// <param name="player">플레이어</param>
        public void DisplayAllIntents(List<Character> monsters, Character player)
        {
            if (monsters == null || player == null) return;
            
            foreach (Character monster in monsters)
            {
                if (monster != null && monster.IsAlive)
                {
                    DisplayIntent(monster, player);
                }
            }
        }
        
        /// <summary>
        /// 알 수 없는 의도 표시
        /// </summary>
        private void ShowUnknownIntent(Character monster)
        {
            IntentUI intentUI = GetOrCreateIntentUI(monster);
            if (intentUI != null)
            {
                intentUI.SetIcon(unknownIcon);
                intentUI.SetColor(unknownColor);
                intentUI.SetText("???");
                intentUI.SetDescription("알 수 없는 행동");
            }
        }
        
        /// <summary>
        /// 몬스터의 의도 UI 숨기기
        /// </summary>
        /// <param name="monster">대상 몬스터</param>
        public void HideIntent(Character monster)
        {
            if (monster == null) return;
            
            if (intentUICache.TryGetValue(monster, out IntentUI intentUI))
            {
                intentUI.Hide();
            }
        }
        
        /// <summary>
        /// 몬스터의 의도 UI 제거
        /// </summary>
        /// <param name="monster">대상 몬스터</param>
        public void RemoveIntent(Character monster)
        {
            if (monster == null) return;
            
            if (intentUICache.TryGetValue(monster, out IntentUI intentUI))
            {
                if (intentUI?.gameObject != null)
                {
                    Destroy(intentUI.gameObject);
                }
                
                intentUICache.Remove(monster);
            }
        }
        #endregion
        
        #region UI Management
        /// <summary>
        /// 몬스터의 의도 UI를 가져오거나 생성한다
        /// </summary>
        private IntentUI GetOrCreateIntentUI(Character monster)
        {
            if (monster == null) return null;
            
            // 기존 UI가 있으면 반환
            if (intentUICache.TryGetValue(monster, out IntentUI existingUI))
            {
                if (existingUI?.gameObject != null)
                {
                    return existingUI;
                }
                else
                {
                    // UI가 파괴되었으면 캐시에서 제거
                    intentUICache.Remove(monster);
                }
            }
            
            // 새 UI 생성
            return CreateIntentUI(monster);
        }
        
        /// <summary>
        /// 새로운 의도 UI 생성
        /// </summary>
        private IntentUI CreateIntentUI(Character monster)
        {
            if (intentPrefab == null || intentContainer == null)
            {
                Debug.LogError("IntentDisplaySystem: Prefab 또는 Container가 설정되지 않았습니다");
                return null;
            }
            
            GameObject uiObject = Instantiate(intentPrefab, intentContainer);
            IntentUI intentUI = uiObject.GetComponent<IntentUI>();
            
            if (intentUI == null)
            {
                intentUI = uiObject.AddComponent<IntentUI>();
            }
            
            // 캐시에 저장
            intentUICache[monster] = intentUI;
            
            Debug.Log($"IntentDisplaySystem: {monster.CharacterName}의 의도 UI 생성 완료");
            return intentUI;
        }
        
        /// <summary>
        /// 의도 UI 업데이트
        /// </summary>
        private void UpdateIntentUI(IntentUI intentUI, MonsterPatternSO pattern)
        {
            if (intentUI == null || pattern == null) return;
            
            // 의도 타입에 따른 아이콘과 색상 설정
            IntentType intentType = DetermineIntentType(pattern);
            
            switch (intentType)
            {
                case IntentType.Attack:
                    intentUI.SetIcon(attackIcon);
                    intentUI.SetColor(attackColor);
                    break;
                    
                case IntentType.Defense:
                    intentUI.SetIcon(defenseIcon);
                    intentUI.SetColor(defenseColor);
                    break;
                    
                case IntentType.Status:
                    intentUI.SetIcon(statusIcon);
                    intentUI.SetColor(statusColor);
                    break;
                    
                case IntentType.Special:
                    intentUI.SetIcon(specialIcon);
                    intentUI.SetColor(specialColor);
                    break;
                    
                default:
                    intentUI.SetIcon(unknownIcon);
                    intentUI.SetColor(unknownColor);
                    break;
            }
            
            // 텍스트 정보 설정
            string displayText = GetDisplayText(pattern, intentType);
            intentUI.SetText(displayText);
            intentUI.SetDescription(pattern.Description);
            
            // UI 표시
            intentUI.Show();
        }
        
        /// <summary>
        /// 패턴의 의도 타입 결정
        /// </summary>
        private IntentType DetermineIntentType(MonsterPatternSO pattern)
        {
            string intentType = pattern.IntentType.ToLower();
            
            if (intentType.Contains("공격") || intentType.Contains("타격") || intentType.Contains("피해"))
            {
                return IntentType.Attack;
            }
            else if (intentType.Contains("방어") || intentType.Contains("보호") || intentType.Contains("회복"))
            {
                return IntentType.Defense;
            }
            else if (intentType.Contains("상태") || intentType.Contains("저주") || intentType.Contains("독") || 
                     intentType.Contains("출혈") || intentType.Contains("봉인"))
            {
                return IntentType.Status;
            }
            else if (intentType.Contains("특수") || intentType.Contains("강화") || intentType.Contains("분노"))
            {
                return IntentType.Special;
            }
            
            return IntentType.Unknown;
        }
        
        /// <summary>
        /// 표시할 텍스트 생성
        /// </summary>
        private string GetDisplayText(MonsterPatternSO pattern, IntentType intentType)
        {
            switch (intentType)
            {
                case IntentType.Attack:
                    int damage = pattern.AttackBonus;
                    return damage > 0 ? damage.ToString() : "공격";
                    
                case IntentType.Defense:
                    int defense = pattern.DefenseBonus;
                    return defense > 0 ? $"+{defense}" : "방어";
                    
                case IntentType.Status:
                    if (pattern.StatusEffects != null && pattern.StatusEffects.Length > 0)
                    {
                        return pattern.StatusEffects.Length.ToString();
                    }
                    return "상태";
                    
                case IntentType.Special:
                    return "특수";
                    
                default:
                    return "???";
            }
        }
        #endregion
        
        #region Public Methods
        /// <summary>
        /// 전투 시작 시 호출
        /// </summary>
        public void OnBattleStart()
        {
            CleanupIntentSystem();
            Debug.Log("IntentDisplaySystem: 전투 시작 - 의도 시스템 준비 완료");
        }
        
        /// <summary>
        /// 전투 종료 시 호출
        /// </summary>
        public void OnBattleEnd()
        {
            CleanupIntentSystem();
            Debug.Log("IntentDisplaySystem: 전투 종료 - 의도 시스템 정리 완료");
        }
        
        /// <summary>
        /// 턴 시작 시 모든 의도 새로고침
        /// </summary>
        public void OnTurnStart(List<Character> monsters, Character player)
        {
            DisplayAllIntents(monsters, player);
        }
        #endregion
    }
    
    #region Enums
    /// <summary>
    /// 의도 타입 열거형
    /// </summary>
    public enum IntentType
    {
        Attack,     // 공격
        Defense,    // 방어
        Status,     // 상태이상
        Special,    // 특수
        Unknown     // 알 수 없음
    }
    #endregion
}