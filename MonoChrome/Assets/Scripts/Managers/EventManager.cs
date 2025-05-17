using System.Collections;
using System.Collections.Generic;
using MonoChrome.Dungeon;
using MonoChrome.Managers;
using UnityEngine;
using DungeonManager = MonoChrome.Dungeon.DungeonManager;

namespace MonoChrome
{
    public class EventManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private DungeonManager dungeonManager;
        
        private void Awake()
        {
            if (playerManager == null)
                playerManager = FindObjectOfType<PlayerManager>();
            
            if (dungeonManager == null)
                dungeonManager = FindObjectOfType<DungeonManager>();
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
            Debug.Log($"Starting event. Node type: {node.Type}");
            // 실제 구현은 추후에 진행
            
            // 테스트를 위해 바로 이벤트 종료
            if (dungeonManager != null)
            {
                dungeonManager.OnRoomCompleted();
            }
        }
    }
}