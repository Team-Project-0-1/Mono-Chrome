using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonoChrome.Events;
using MonoChrome.Combat;

namespace MonoChrome.UI
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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            if (combatPanel == null || combatPanel.name != "CombatPanel") return;

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
            return GameObject.Find("CombatPanel");
        }

        /// <summary>
        /// 코인 UI 생성
        /// </summary>
        private static void CreateCoinUI(Transform parent, int index, bool isHeads)
        {
            GameObject coinObj = new GameObject($"Coin_{index}");
            coinObj.transform.SetParent(parent);

            Image coinImage = coinObj.AddComponent<Image>();
            // TODO: 앞면/뒷면 스프라이트 설정
            coinImage.color = isHeads ? Color.yellow : Color.blue;

            RectTransform rectTransform = coinObj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(index * 60 - 120, 0);
            rectTransform.sizeDelta = new Vector2(50, 50);
        }

        /// <summary>
        /// 패턴 버튼 생성
        /// </summary>
        private static void CreatePatternButton(Transform parent, int index, Pattern pattern)
        {
            GameObject buttonObj = new GameObject($"PatternButton_{index}");
            buttonObj.transform.SetParent(parent);

            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = Color.white;

            // 버튼 텍스트 추가
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform);
            Text text = textObj.AddComponent<Text>();
            text.text = pattern.Name;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;

            // TODO: 폰트 설정
            // text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            // 버튼 이벤트 등록
            button.onClick.AddListener(() => {
                // 패턴 선택 이벤트 발행 (CombatEvents를 통해)
                Debug.Log($"[UIController] 패턴 선택: {pattern.Name}");
                // TODO: 패턴 선택 로직 구현
            });

            // 레이아웃 설정
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchoredPosition = new Vector2(index * 120 - 180, 0);
            buttonRect.sizeDelta = new Vector2(100, 50);

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        #endregion
    }


}
