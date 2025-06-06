using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 이 클래스는 이전 버전과의 호환성을 위한 클래스입니다.
    /// 실제로는 DungeonNode 클래스를 사용하도록 변환합니다.
    /// </summary>
    [System.Serializable]
    public class RoomData
    {
        public int id;
        public RoomType roomType;
        public int difficulty;
        public int enemyId = -1;
        public int eventId = -1;
        
        /// <summary>
        /// RoomData를 DungeonNode로 변환
        /// </summary>
        public DungeonNode ToDungeonNode()
        {
            // RoomType을 NodeType으로 변환
            NodeType nodeType = ConvertRoomTypeToNodeType(roomType);
            
            // 새 DungeonNode 생성
            DungeonNode node = new DungeonNode(id, nodeType, Vector2.zero);
            
            // 필요한 추가 데이터 설정
            if (enemyId >= 0)
            {
                node.EnemyID = enemyId;
            }
            
            if (eventId >= 0)
            {
                node.EventID = eventId;
            }
            
            return node;
        }
        
        /// <summary>
        /// RoomType을 NodeType으로 변환
        /// </summary>
        private NodeType ConvertRoomTypeToNodeType(RoomType type)
        {
            switch (type)
            {
                case RoomType.Combat:
                    return NodeType.Combat;
                case RoomType.Event:
                    return NodeType.Event;
                case RoomType.Shop:
                    return NodeType.Shop;
                case RoomType.Rest:
                    return NodeType.Rest;
                case RoomType.MiniBoss:
                    return NodeType.MiniBoss;
                case RoomType.Boss:
                    return NodeType.Boss;
                default:
                    return NodeType.Combat;
            }
        }
    }
    
    /// <summary>
    /// 이전 버전과의 호환성을 위한 열거형
    /// </summary>
    // 이 열거형은 AllEnums.cs로 이동되었습니다.
    // Enum definition moved to AllEnums.cs
    /*
    public enum RoomType
    {
        Combat,
        Shop,
        Rest,
        Event,
        MiniBoss,
        Boss
    }
    */
}