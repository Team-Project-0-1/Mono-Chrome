using System.Collections;
using System.Collections.Generic;
using MonoChrome;
using MonoChrome.Events;
using UnityEngine;

namespace MonoChrome
{
    public class ShopManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerManager playerManager;
        [SerializeField] private DungeonController dungeonController;
        
        private void Awake()
        {
            if (playerManager == null)
                playerManager = FindObjectOfType<PlayerManager>();
            
            if (dungeonController == null)
                dungeonController = FindObjectOfType<DungeonController>();
        }
        
        public void OpenShop(RoomData roomData)
        {
            Debug.Log($"Opening shop. Room difficulty: {roomData.difficulty}");
            
            // RoomData를 DungeonNode로 변환
            DungeonNode node = roomData.ToDungeonNode();
            
            // 상점 노드 처리
            OpenShop(node);
        }
        
        // DungeonNode를 직접 처리하는 오버로드
        public void OpenShop(DungeonNode node)
        {
            Debug.Log($"Opening shop. Node type: {node.Type}");
            // 실제 구현은 추후에 진행
            
            // 테스트를 위해 바로 상점 종료 처리
            DungeonEvents.NotifyRoomActivityCompleted();
        }
    }
}