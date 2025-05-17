using System.Collections;
using System.Collections.Generic;
using MonoChrome.Combat;
using UnityEngine;

namespace MonoChrome.Managers
{
    /// <summary>
    /// 브릿지 패턴을 사용하여 Combat/CombatManager로 리디렉션하는 클래스
    /// 레거시 코드와의 호환성을 위해 유지됩니다.
    /// </summary>
    [RequireComponent(typeof(Combat.CombatManager))]
    public class CombatManager : MonoBehaviour
    {
        private Combat.CombatManager _combatManager;

        private void Awake()
        {
            // Combat.CombatManager 참조 가져오기
            _combatManager = GetComponent<Combat.CombatManager>();
            
            if (_combatManager == null)
            {
                _combatManager = gameObject.AddComponent<Combat.CombatManager>();
                Debug.Log("Managers.CombatManager: Added Combat.CombatManager component");
            }
        }

        /// <summary>
        /// 전투 시작 - RoomData 사용 (이전 버전과 호환성 유지)
        /// </summary>
        public void StartCombat(RoomData roomData, bool isMiniBoss = false, bool isBoss = false)
        {
            if (_combatManager != null)
            {
                _combatManager.StartCombat(roomData, isMiniBoss, isBoss);
            }
        }

        /// <summary>
        /// 전투 시작
        /// </summary>
        public void StartCombat(DungeonNode node, bool isMiniBoss = false, bool isBoss = false)
        {
            if (_combatManager != null)
            {
                _combatManager.StartCombat(node, isMiniBoss, isBoss);
            }
        }

        /// <summary>
        /// 패턴 선택
        /// </summary>
        public void SelectPattern(Pattern pattern)
        {
            if (_combatManager != null)
            {
                _combatManager.SelectPattern(pattern);
            }
        }

        /// <summary>
        /// 액티브 스킬 사용
        /// </summary>
        public void UseActiveSkill()
        {
            if (_combatManager != null)
            {
                _combatManager.UseActiveSkill();
            }
        }

        /// <summary>
        /// 플레이어 턴 종료
        /// </summary>
        public void FinishPlayerTurn()
        {
            if (_combatManager != null)
            {
                _combatManager.FinishPlayerTurn();
            }
        }
        
        /// <summary>
        /// 전투 초기화
        /// </summary>
        public void InitializeCombat()
        {
            if (_combatManager != null)
            {
                _combatManager.InitializeCombat();
            }
        }
        
        /// <summary>
        /// 플레이어 캐릭터 설정
        /// </summary>
        public void SetPlayerCharacter(PlayerCharacter character)
        {
            if (_combatManager != null)
            {
                _combatManager.SetPlayerCharacter(character);
            }
        }
        
        /// <summary>
        /// 적 캐릭터 설정
        /// </summary>
        public void SetEnemyCharacter(EnemyCharacter character)
        {
            if (_combatManager != null)
            {
                _combatManager.SetEnemyCharacter(character);
            }
        }
    }
}