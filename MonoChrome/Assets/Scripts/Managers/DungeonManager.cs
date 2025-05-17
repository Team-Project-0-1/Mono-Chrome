using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Managers
{
    /// <summary>
    /// 브릿지 패턴을 사용하여 Dungeon/DungeonManager로 리디렉션하는 클래스
    /// 레거시 코드와의 호환성을 위해 유지됩니다.
    /// </summary>
    [RequireComponent(typeof(Dungeon.DungeonManager))]
    public class DungeonManager : MonoBehaviour
    {
        private Dungeon.DungeonManager _dungeonManager;

        private void Awake()
        {
            // Dungeon.DungeonManager 참조 가져오기
            _dungeonManager = GetComponent<Dungeon.DungeonManager>();
            
            if (_dungeonManager == null)
            {
                _dungeonManager = gameObject.AddComponent<Dungeon.DungeonManager>();
                Debug.Log("Managers.DungeonManager: Added Dungeon.DungeonManager component");
            }
        }

        /// <summary>
        /// 새 던전 생성
        /// </summary>
        public void GenerateNewDungeon()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.GenerateNewDungeon();
            }
        }

        /// <summary>
        /// 특정 던전 노드로 이동
        /// </summary>
        public void MoveToNode(int nodeIndex)
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.MoveToNode(nodeIndex);
            }
        }

        /// <summary>
        /// 방 활동 완료 후 호출되는 메서드
        /// </summary>
        public void OnRoomCompleted()
        {
            if (_dungeonManager != null)
            {
                _dungeonManager.OnRoomCompleted();
            }
        }
    }
}