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
    public class RoomNode
    {
        public int index;
        public RoomType type;
        public Vector2 position; // 미니맵에서의 위치
        public bool isVisited;
        public bool isAccessible;
        public List<int> connectedNodes = new List<int>(); // 연결된 노드 인덱스
        
        // 특정 방 타입별 추가 데이터
        public int enemyID = -1; // 전투 방: -1이면 랜덤 생성
        public int eventID = -1; // 이벤트 방: -1이면 랜덤 생성
        
        public RoomNode() { }
        
        public RoomNode(int id, RoomType nodeType, Vector2 nodePosition)
        {
            index = id;
            type = nodeType;
            position = nodePosition;
            isVisited = false;
            isAccessible = false;
        }
        
        /// <summary>
        /// RoomNode를 DungeonNode로 변환
        /// </summary>
        public DungeonNode ToDungeonNode()
        {
            // RoomType을 NodeType으로 변환
            NodeType nodeType = ConvertRoomTypeToNodeType(type);
            
            // 새 DungeonNode 생성
            DungeonNode node = new DungeonNode(index, nodeType, position);
            
            // 상태 정보 복사
            node.IsVisited = isVisited;
            node.IsAccessible = isAccessible;
            
            // 연결된 노드 복사
            node.ConnectedNodes = new List<int>(connectedNodes);
            
            // 추가 데이터 설정
            if (enemyID >= 0)
            {
                node.EnemyID = enemyID;
            }
            
            if (eventID >= 0)
            {
                node.EventID = eventID;
            }
            
            return node;
        }
        
        /// <summary>
        /// DungeonNode를 RoomNode로 변환
        /// </summary>
        public static RoomNode FromDungeonNode(DungeonNode dungeonNode)
        {
            RoomNode roomNode = new RoomNode();
            
            roomNode.index = dungeonNode.ID;
            roomNode.type = ConvertNodeTypeToRoomType(dungeonNode.Type);
            roomNode.position = dungeonNode.Position;
            roomNode.isVisited = dungeonNode.IsVisited;
            roomNode.isAccessible = dungeonNode.IsAccessible;
            roomNode.connectedNodes = new List<int>(dungeonNode.ConnectedNodes);
            roomNode.enemyID = dungeonNode.EnemyID;
            roomNode.eventID = dungeonNode.EventID;
            
            return roomNode;
        }
        
        /// <summary>
        /// RoomType을 NodeType으로 변환
        /// </summary>
        private static NodeType ConvertRoomTypeToNodeType(RoomType type)
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
        
        /// <summary>
        /// NodeType을 RoomType으로 변환
        /// </summary>
        private static RoomType ConvertNodeTypeToRoomType(NodeType type)
        {
            switch (type)
            {
                case NodeType.Combat:
                    return RoomType.Combat;
                case NodeType.Event:
                    return RoomType.Event;
                case NodeType.Shop:
                    return RoomType.Shop;
                case NodeType.Rest:
                    return RoomType.Rest;
                case NodeType.MiniBoss:
                    return RoomType.MiniBoss;
                case NodeType.Boss:
                    return RoomType.Boss;
                default:
                    return RoomType.Combat;
            }
        }
    }
}