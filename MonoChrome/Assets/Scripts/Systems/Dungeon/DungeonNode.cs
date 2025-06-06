using UnityEngine;
using System.Collections.Generic;

namespace MonoChrome.Systems.Dungeon
{
    
    [System.Serializable]
    public class DungeonNode
    {
        public int ID;
        public NodeType Type;
        public Vector2 Position; // 노드 위치
        public List<int> ConnectedNodes = new List<int>(); // 연결된 노드 ID
        public bool IsVisited = false;
        public bool IsAccessible = false;
        
        // 노드별 특수 데이터
        public int EnemyID = -1; // 전투 노드일 경우 적 ID
        public int EventID = -1; // 이벤트 노드일 경우 이벤트 ID
        
        public DungeonNode(int id, NodeType type, Vector2 position)
        {
            ID = id;
            Type = type;
            Position = position;
        }
        
        // 노드 아이콘 또는 색상 가져오기
        public Color GetNodeColor()
        {
            switch (Type)
            {
                case NodeType.Combat:
                    return new Color(0.8f, 0.2f, 0.2f); // 빨강
                case NodeType.Shop:
                    return new Color(0.2f, 0.8f, 0.2f); // 녹색
                case NodeType.Rest:
                    return new Color(0.2f, 0.6f, 0.8f); // 파랑
                case NodeType.Event:
                    return new Color(0.8f, 0.8f, 0.2f); // 노랑
                case NodeType.MiniBoss:
                    return new Color(0.8f, 0.2f, 0.8f); // 보라
                case NodeType.Boss:
                    return new Color(1.0f, 0.0f, 0.0f); // 진한 빨강
                default:
                    return Color.gray;
            }
        }
        
        public string GetNodeDescription()
        {
            switch (Type)
            {
                case NodeType.Combat:
                    return "일반 전투: 적과 마주칩니다.";
                case NodeType.Shop:
                    return "상점: 아이템을 구매하거나 판매할 수 있습니다.";
                case NodeType.Rest:
                    return "휴식: 체력을 회복하고 새 능력을 배울 수 있습니다.";
                case NodeType.Event:
                    return "이벤트: 특별한 만남이나 선택이 기다리고 있습니다.";
                case NodeType.MiniBoss:
                    return "미니 보스: 강한 적과 마주칩니다.";
                case NodeType.Boss:
                    return "보스: 최종 도전입니다!";
                default:
                    return "알 수 없는 노드";
            }
        }
    }
}