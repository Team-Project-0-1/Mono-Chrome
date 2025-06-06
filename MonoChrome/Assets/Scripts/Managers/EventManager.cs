using System.Collections;
using System.Collections.Generic;
using MonoChrome.Dungeon;
using UnityEngine;

namespace MonoChrome
{
    /// <summary>
    /// 이벤트 관리를 담당하는 매니저 클래스
    /// 단일 구현체 사용으로 깔끔하게 정리됨
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private DungeonManager dungeonManager;  // 직접 구현체 사용
        
        private void Awake()
        {
            if (playerManager == null)
                playerManager = FindObjectOfType<PlayerManager>();
            
            if (dungeonManager == null)
                dungeonManager = FindObjectOfType<DungeonManager>();  // 직접 구현체 사용
        }
        
        public void StartEvent(RoomData roomData)
        {
            Debug.Log($"Starting event. Room difficulty: {roomData.difficulty}");
            
            // RoomData를 DungeonNode로 변환
            DungeonNode node = roomData.ToDungeonNode();
            
            // 이벤트 노드 처리
            StartEvent(node);
        }
        
        // DungeonNode를 직접 처리하는 오버로드
        public void StartEvent(DungeonNode node)
        {
            Debug.Log($"Starting event. Node type: {node.Type} (direct implementation)");
            // 실제 구현은 추후에 진행
            
            // 테스트를 위해 바로 이벤트 종료
            if (dungeonManager != null)
            {
                dungeonManager.OnRoomCompleted();
            }
        }
    }
}