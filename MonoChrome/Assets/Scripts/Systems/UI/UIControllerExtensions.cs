using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonoChrome.Systems.Combat;

namespace MonoChrome
{
    /// <summary>
    /// UIController 확장 - 전투 UI 기능 추가
    /// 기존 UIController에 전투 관련 메서드들을 추가하여 확장
    /// </summary>
    public static class UIControllerExtensions
    {
        /// <summary>
        /// 체력바 업데이트
        /// </summary>
        public static void UpdateHealthBar(this UIController controller, string targetType, int current, int max)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            string sliderName = targetType == "Player" ? "PlayerHealthBar" : "EnemyHealthBar";
            Slider healthBar = combatPanel.transform.Find(sliderName)?.GetComponent<Slider>();
            
            if (healthBar != null)
            {
                healthBar.maxValue = max;
                healthBar.value = current;
                Debug.Log($"[UIController] {targetType} 체력바 업데이트: {current}/{max}");
            }
        }

        /// <summary>
        /// 코인 표시 업데이트
        /// </summary>
        public static void UpdateCoinDisplay(this UIController controller, List<bool> coinResults)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            Transform coinArea = combatPanel.transform.Find("CoinArea");
            if (coinArea == null) return;

            // 기존 코인들 제거
            foreach (Transform child in coinArea)
            {
                Object.Destroy(child.gameObject);
            }

            // 새 코인들 생성
            for (int i = 0; i < coinResults.Count; i++)
            {
                CreateCoinUI(coinArea, i, coinResults[i]);
            }

            Debug.Log($"[UIController] 코인 표시 업데이트: {coinResults.Count}개");
        }

        /// <summary>
        /// 패턴 버튼 업데이트
        /// </summary>
        public static void UpdatePatternButtons(this UIController controller, List<Pattern> patterns)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            Transform patternArea = combatPanel.transform.Find("PatternArea");
            if (patternArea == null) return;

            // 기존 버튼들 제거
            foreach (Transform child in patternArea)
            {
                Object.Destroy(child.gameObject);
            }

            // 새 패턴 버튼들 생성
            for (int i = 0; i < patterns.Count; i++)
            {
                CreatePatternButton(patternArea, i, patterns[i]);
            }

            Debug.Log($"[UIController] 패턴 버튼 업데이트: {patterns.Count}개");
        }

        /// <summary>
        /// 액티브 스킬 버튼 업데이트
        /// </summary>
        public static void UpdateActiveSkillButton(this UIController controller, bool available)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            Button skillButton = combatPanel.transform.Find("ActiveSkillButton")?.GetComponent<Button>();
            if (skillButton != null)
            {
                skillButton.interactable = available;
                
                // 버튼 색상도 변경
                var colors = skillButton.colors;
                colors.normalColor = available ? Color.white : Color.gray;
                skillButton.colors = colors;
            }
        }

        /// <summary>
        /// 턴 정보 업데이트
        /// </summary>
        public static void UpdateTurnInfo(this UIController controller, int turnCount)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            Text turnText = combatPanel.transform.Find("TurnInfoText")?.GetComponent<Text>();
            if (turnText != null)
            {
                turnText.text = $"턴 {turnCount}";
            }
        }

        /// <summary>
        /// 캐릭터 정보 업데이트
        /// </summary>
        public static void UpdateCharacterInfo(this UIController controller, string playerName, string enemyName)
        {
            GameObject combatPanel = controller.GetCurrentPanel();
            if (combatPanel == null || combatPanel.name != "CombatUI") return;

            Text playerNameText = combatPanel.transform.Find("PlayerNameText")?.GetComponent<Text>();
            Text enemyNameText = combatPanel.transform.Find("EnemyNameText")?.GetComponent<Text>();

            if (playerNameText != null) playerNameText.text = playerName;
            if (enemyNameText != null) enemyNameText.text = enemyName;
        }

        /// <summary>
        /// 효과 메시지 표시
        /// </summary>
        public static void ShowPatternEffect(this UIController controller, string patternName)
        {
            Debug.Log($"[UIController] 패턴 효과: {patternName}");
            // TODO: 실제 효과 애니메이션 구현
        }

        public static void ShowDamageEffect(this UIController controller, string targetName, int damage)
        {
            Debug.Log($"[UIController] 피해 효과: {targetName}에게 {damage} 피해");
            // TODO: 피해 애니메이션 구현
        }

        public static void ShowDefenseEffect(this UIController controller, string targetName, int defense)
        {
            Debug.Log($"[UIController] 방어 효과: {targetName}이 {defense} 방어력 획득");
            // TODO: 방어 애니메이션 구현
        }

        /// <summary>
        /// 승리/패배 메시지 표시
        /// </summary>
        public static void ShowVictoryMessage(this UIController controller, string message)
        {
            Debug.Log($"[UIController] 승리 메시지: {message}");
            // TODO: 승리 팝업 구현
        }

        public static void ShowDefeatMessage(this UIController controller, string message)
        {
            Debug.Log($"[UIController] 패배 메시지: {message}");
            // TODO: 패배 팝업 구현
        }

        #region Helper Methods
        /// <summary>
        /// 현재 활성 패널 가져오기 (private 필드 접근을 위한 리플렉션 대안)
        /// </summary>
        public static GameObject GetCurrentPanel(this UIController controller)
        {
            // CombatPanel 찾기
            return GameObject.Find("CombatUI");
        }

        /// <summary>
        /// 코인 UI 생성
        /// </summary>
        private static void CreateCoinUI(Transform parent, int index, bool isHeads)
        {
            // 프리팹을 Resources에서 로드하여 사용
            GameObject coinPrefab = Resources.Load<GameObject>("UI/CoinPrefab");
            if (coinPrefab == null)
            {
                Debug.LogWarning("CoinPrefab not found in Resources/UI/, creating fallback UI");
                // 기존 코드로 폴백
                GameObject fallbackCoin = new GameObject($"Coin_{index}");
                fallbackCoin.transform.SetParent(parent);
                Image fallbackImage = fallbackCoin.AddComponent<Image>();
                fallbackImage.color = isHeads ? Color.yellow : Color.blue;
                RectTransform fallbackRect = fallbackCoin.GetComponent<RectTransform>();
                fallbackRect.anchoredPosition = new Vector2(index * 60 - 120, 0);
                fallbackRect.sizeDelta = new Vector2(50, 50);
                return;
            }
            
            GameObject coinInstance = Object.Instantiate(coinPrefab, parent);
            coinInstance.name = $"Coin_{index}";
            
            // 프리팹 기반 동전 설정
            Image coinImage = coinInstance.GetComponent<Image>();
            if (coinImage != null)
            {
                coinImage.color = isHeads ? Color.yellow : Color.blue;
            }
            
            RectTransform rectTransform = coinInstance.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(index * 60 - 120, 0);
        }

        /// <summary>
        /// 패턴 버튼 생성
        /// </summary>
        private static void CreatePatternButton(Transform parent, int index, Pattern pattern)
        {
            // 프리팹을 Resources에서 로드하여 사용
            GameObject patternPrefab = Resources.Load<GameObject>("UI/PatternButtonPrefab");
            if (patternPrefab == null)
            {
                Debug.LogWarning("PatternButtonPrefab not found in Resources/UI/, creating fallback UI");
                // 기존 코드로 폴백
                GameObject fallbackButton = new GameObject($"PatternButton_{index}");
                fallbackButton.transform.SetParent(parent);
                Button fallbackButtonComp = fallbackButton.AddComponent<Button>();
                Image fallbackButtonImage = fallbackButton.AddComponent<Image>();
                fallbackButtonImage.color = Color.white;
                GameObject fallbackTextObj = new GameObject("Text");
                fallbackTextObj.transform.SetParent(fallbackButton.transform);
                Text fallbackText = fallbackTextObj.AddComponent<Text>();
                fallbackText.text = pattern.Name;
                fallbackText.alignment = TextAnchor.MiddleCenter;
                fallbackText.color = Color.black;
                fallbackButtonComp.onClick.AddListener(() => {
                    Debug.Log($"[UIController] 패턴 선택: {pattern.Name}");
                });
                RectTransform fallbackButtonRect = fallbackButton.GetComponent<RectTransform>();
                fallbackButtonRect.anchoredPosition = new Vector2(index * 120 - 180, 0);
                fallbackButtonRect.sizeDelta = new Vector2(100, 50);
                RectTransform fallbackTextRect = fallbackTextObj.GetComponent<RectTransform>();
                fallbackTextRect.anchorMin = Vector2.zero;
                fallbackTextRect.anchorMax = Vector2.one;
                fallbackTextRect.sizeDelta = Vector2.zero;
                fallbackTextRect.anchoredPosition = Vector2.zero;
                return;
            }
            
            GameObject buttonInstance = Object.Instantiate(patternPrefab, parent);
            buttonInstance.name = $"PatternButton_{index}";
            
            // 프리팹 기반 패턴 버튼 설정
            Button buttonComponent = buttonInstance.GetComponent<Button>();
            Text nameText = buttonInstance.transform.Find("PatternName")?.GetComponent<Text>();
            if (nameText != null)
            {
                nameText.text = pattern.Name;
            }
            
            // 버튼 이벤트 등록
            if (buttonComponent != null)
            {
                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(() => {
                    Debug.Log($"[UIController] 패턴 선택: {pattern.Name}");
                    // TODO: 패턴 선택 로직 구현
                });
            }
            
            // 레이아웃 설정
            RectTransform buttonRect = buttonInstance.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(index * 120 - 180, 0);
        }
        #endregion
    }


}
