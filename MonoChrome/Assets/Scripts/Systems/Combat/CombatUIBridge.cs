using System.Collections.Generic;
using MonoChrome.Core;
using MonoChrome;
using UnityEngine;

namespace MonoChrome.Systems.Combat
{
    /// <summary>
    /// 전투 시스템과 UI 간의 연결고리
    /// 
    /// 책임:
    /// - CombatSystem 이벤트를 UI 이벤트로 변환
    /// - UI에서 발생한 사용자 입력을 CombatSystem으로 전달
    /// - 전투 관련 UI 업데이트 처리
    /// 
    /// 설계 원칙:
    /// - CombatSystem과 UI 시스템 간의 중개자 역할
    /// - 이벤트 기반 통신으로 낮은 결합도 유지
    /// - UI 관련 로직만 담당하여 단일 책임 원칙 준수
    /// </summary>
    public class CombatUIBridge : MonoBehaviour
    {
        #region UI Controller Reference
        private UI.UIController _uiController;
        #endregion

        #region Initialization
        private void Awake()
        {
            // UI 컨트롤러 찾기
            _uiController = FindFirstObjectByType<UI.UIController>();
            if (_uiController == null)
            {
                Debug.LogError("[CombatUIBridge] UIController를 찾을 수 없습니다");
            }
        }

        private void OnEnable()
        {
            // CombatSystem 이벤트 구독
            SubscribeToCombatEvents();
        }

        private void OnDisable()
        {
            // 이벤트 구독 해제
            UnsubscribeFromCombatEvents();
        }
        #endregion

        #region Event Subscription
        private void SubscribeToCombatEvents()
        {
            // CombatSystem 이벤트들 구독
            CombatSystem.OnCombatStarted += OnCombatStarted;
            CombatSystem.OnCombatEnded += OnCombatEnded;
            CombatSystem.OnTurnChanged += OnTurnChanged;
            CombatSystem.OnPlayerTurnStarted += OnPlayerTurnStarted;
            CombatSystem.OnPatternExecuted += OnPatternExecuted;
            CombatSystem.OnDamageDealt += OnDamageDealt;
            CombatSystem.OnDefenseAdded += OnDefenseAdded;
        }

        private void UnsubscribeFromCombatEvents()
        {
            // CombatSystem 이벤트들 구독 해제
            CombatSystem.OnCombatStarted -= OnCombatStarted;
            CombatSystem.OnCombatEnded -= OnCombatEnded;
            CombatSystem.OnTurnChanged -= OnTurnChanged;
            CombatSystem.OnPlayerTurnStarted -= OnPlayerTurnStarted;
            CombatSystem.OnPatternExecuted -= OnPatternExecuted;
            CombatSystem.OnDamageDealt -= OnDamageDealt;
            CombatSystem.OnDefenseAdded -= OnDefenseAdded;
        }
        #endregion

        #region Combat Event Handlers
        private void OnCombatStarted(PlayerCharacter player, EnemyCharacter enemy)
        {
            Debug.Log($"[CombatUIBridge] 전투 시작: {player.CharacterName} vs {enemy.CharacterName}");

            if (_uiController == null) return;

            // 전투 패널 표시
            _uiController.ShowPanel("CombatPanel");

            // 캐릭터 정보 업데이트 (확장 메서드 사용)
            _uiController.UpdateCharacterInfo(player.CharacterName, enemy.CharacterName);

            // 체력바 초기화
            _uiController.UpdateHealthBar("Player", player.CurrentHealth, player.MaxHealth);
            _uiController.UpdateHealthBar("Enemy", enemy.CurrentHealth, enemy.MaxHealth);

            // 캐릭터 체력 변경 이벤트 구독
            player.OnHealthChanged += (current, max) => 
                _uiController.UpdateHealthBar("Player", current, max);
            enemy.OnHealthChanged += (current, max) => 
                _uiController.UpdateHealthBar("Enemy", current, max);
        }

        private void OnCombatEnded(bool playerVictory)
        {
            Debug.Log($"[CombatUIBridge] 전투 종료: 승리 {playerVictory}");

            if (_uiController == null) return;

            if (playerVictory)
            {
                _uiController.ShowVictoryMessage("승리!");
                // 2초 후 던전으로 복귀
                StartCoroutine(ReturnToDungeonAfterDelay(2f));
            }
            else
            {
                _uiController.ShowDefeatMessage("패배...");
                // 게임 오버 처리
                // TODO: 게임 오버 로직 구현
            }
        }

        private void OnTurnChanged(int turnCount)
        {
            Debug.Log($"[CombatUIBridge] 턴 변경: {turnCount}");

            if (_uiController == null) return;

            // 턴 정보 업데이트
            _uiController.UpdateTurnInfo(turnCount);
        }

        private void OnPlayerTurnStarted(List<Pattern> availablePatterns, List<bool> coinResults)
        {
            Debug.Log($"[CombatUIBridge] 플레이어 턴 시작: {availablePatterns.Count}개 패턴, {coinResults.Count}개 코인");

            if (_uiController == null) return;

            // 코인 표시 업데이트
            _uiController.UpdateCoinDisplay(coinResults);

            // 패턴 버튼 업데이트
            _uiController.UpdatePatternButtons(availablePatterns);

            // 액티브 스킬 버튼 상태 업데이트
            var combatSystem = CombatSystem.Instance;
            var player = combatSystem?.CurrentPlayer;
            bool skillAvailable = player?.IsActiveSkillAvailable() ?? false;
            _uiController.UpdateActiveSkillButton(skillAvailable);
        }

        private void OnPatternExecuted(Pattern pattern)
        {
            Debug.Log($"[CombatUIBridge] 패턴 실행: {pattern.Name}");

            if (_uiController == null) return;

            // 패턴 효과 표시
            _uiController.ShowPatternEffect(pattern.Name);
        }

        private void OnDamageDealt(Character target, int damage)
        {
            Debug.Log($"[CombatUIBridge] 피해: {target.CharacterName}에게 {damage} 피해");

            if (_uiController == null) return;

            // 피해 효과 표시
            _uiController.ShowDamageEffect(target.CharacterName, damage);
        }

        private void OnDefenseAdded(Character target, int defense)
        {
            Debug.Log($"[CombatUIBridge] 방어: {target.CharacterName}이 {defense} 방어력 획득");

            if (_uiController == null) return;

            // 방어 효과 표시
            _uiController.ShowDefenseEffect(target.CharacterName, defense);
        }
        #endregion

        #region UI to Combat Communication
        /// <summary>
        /// UI에서 패턴이 선택되었을 때 호출
        /// </summary>
        public void OnPatternSelectedFromUI(Pattern pattern)
        {
            Debug.Log($"[CombatUIBridge] UI에서 패턴 선택됨: {pattern.Name}");

            var combatSystem = CombatSystem.Instance;
            if (combatSystem != null && combatSystem.IsCombatActive && combatSystem.IsPlayerTurn)
            {
                combatSystem.ExecutePlayerPattern(pattern);
            }
        }

        /// <summary>
        /// UI에서 액티브 스킬 버튼이 클릭되었을 때 호출
        /// </summary>
        public void OnActiveSkillRequestedFromUI()
        {
            Debug.Log("[CombatUIBridge] UI에서 액티브 스킬 요청됨");

            var combatSystem = CombatSystem.Instance;
            if (combatSystem != null && combatSystem.IsCombatActive && combatSystem.IsPlayerTurn)
            {
                combatSystem.UseActiveSkill();
            }
        }

        /// <summary>
        /// UI에서 턴 종료 버튼이 클릭되었을 때 호출
        /// </summary>
        public void OnTurnEndRequestedFromUI()
        {
            Debug.Log("[CombatUIBridge] UI에서 턴 종료 요청됨");

            // TODO: 턴 강제 종료 로직 구현 (필요시)
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// 지연 후 던전으로 복귀
        /// </summary>
        private System.Collections.IEnumerator ReturnToDungeonAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            Debug.Log("[CombatUIBridge] 던전으로 복귀");

            // 던전 패널로 전환
            _uiController?.ShowPanel("DungeonPanel");

            // 게임 상태 변경
            var stateMachine = GameStateMachine.Instance;
            stateMachine?.TryChangeState(GameStateMachine.GameState.Dungeon);
        }
        #endregion
    }
}
