using System.Collections.Generic;
using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;

namespace MonoChrome.Dungeon
{
    /// <summary>
    /// 던전 컨트롤러 - 오직 던전 로직만 담당 (단일 책임 원칙)
    /// UI, 전투, 상점 등과는 이벤트를 통해서만 소통 (낮은 결합도)
    /// </summary>
    public class DungeonController : MonoBehaviour
    {
        [Header("던전 설정")]
        [SerializeField] private int _roomsPerStage = 15;
        [SerializeField] private bool _enableDebugLogs = true;

        private List<DungeonNode> _dungeonNodes = new List<DungeonNode>();
        private DungeonNode _currentNode;
        private int _currentNodeIndex = -1;
        private int _currentStage = 0;

        public List<DungeonNode> CurrentDungeonNodes => new List<DungeonNode>(_dungeonNodes);
        public DungeonNode CurrentNode => _currentNode;
        public int CurrentNodeIndex => _currentNodeIndex;

        private void OnEnable()
        {
            DungeonEvents.OnDungeonGenerationRequested += HandleDungeonGenerationRequest;
            DungeonEvents.OnNodeMoveRequested += HandleNodeMoveRequest;
            DungeonEvents.OnRoomActivityCompleted += HandleRoomActivityCompleted;
        }

        private void OnDisable()
        {
            DungeonEvents.OnDungeonGenerationRequested -= HandleDungeonGenerationRequest;
            DungeonEvents.OnNodeMoveRequested -= HandleNodeMoveRequest;
            DungeonEvents.OnRoomActivityCompleted -= HandleRoomActivityCompleted;
        }

        private void HandleDungeonGenerationRequest(int stageIndex)
        {
            GenerateDungeon(stageIndex);
        }

        private void HandleNodeMoveRequest(int nodeIndex)
        {
            MoveToNode(nodeIndex);
        }

        private void HandleRoomActivityCompleted()
        {
            LogDebug("방 활동 완료 처리");
            GameStateMachine.Instance.CompleteRoomActivity();
        }

        /// <summary>
        /// 새로운 던전 생성 (Public API)
        /// </summary>
        public void GenerateNewDungeon(int stageIndex)
        {
            GenerateDungeon(stageIndex);
        }
        
        /// <summary>
        /// 던전 생성 내부 로직
        /// </summary>
        private void GenerateDungeon(int stageIndex)
        {
            LogDebug($"던전 생성 시작 - 스테이지 {stageIndex + 1}");

            ClearCurrentDungeon();
            _currentStage = stageIndex;
            _dungeonNodes = CreateSimpleDungeon();
            SetupStartingNode();
            
            UIEvents.RequestDungeonMapUpdate(_dungeonNodes, _currentNodeIndex);
            DungeonEvents.NotifyDungeonGenerated(_dungeonNodes, _currentNodeIndex);
            
            LogDebug($"던전 생성 완료 - {_dungeonNodes.Count}개 노드");
        }

        /// <summary>
        /// 특정 노드로 이동 (Public API)
        /// </summary>
        public void MoveToNode(int nodeIndex)
        {
            if (!IsValidMove(nodeIndex)) return;

            DungeonNode targetNode = _dungeonNodes[nodeIndex];
            UpdateNodeStates(nodeIndex, targetNode);
            DungeonEvents.NotifyNodeMoveCompleted(targetNode);
            ProcessNodeType(targetNode);
            UIEvents.RequestDungeonMapUpdate(_dungeonNodes, _currentNodeIndex);
            
            LogDebug($"노드 이동 완료: {nodeIndex} ({targetNode.Type})");
        }

        private void ClearCurrentDungeon()
        {
            _dungeonNodes.Clear();
            _currentNode = null;
            _currentNodeIndex = -1;
        }

        private void SetupStartingNode()
        {
            if (_dungeonNodes.Count > 0)
            {
                _currentNodeIndex = 0;
                _currentNode = _dungeonNodes[0];
                _currentNode.IsAccessible = true;
                ActivateConnectedNodes(_currentNode);
            }
        }

        private bool IsValidMove(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= _dungeonNodes.Count) return false;
            DungeonNode targetNode = _dungeonNodes[nodeIndex];
            return targetNode.IsAccessible && !targetNode.IsVisited;
        }

        private void UpdateNodeStates(int nodeIndex, DungeonNode targetNode)
        {
            _currentNodeIndex = nodeIndex;
            _currentNode = targetNode;
            _currentNode.IsVisited = true;
            ActivateConnectedNodes(_currentNode);
        }

        private void ActivateConnectedNodes(DungeonNode currentNode)
        {
            foreach (int connectedNodeId in currentNode.ConnectedNodes)
            {
                DungeonNode connectedNode = _dungeonNodes.Find(n => n.ID == connectedNodeId);
                if (connectedNode != null && !connectedNode.IsVisited)
                {
                    connectedNode.IsAccessible = true;
                }
            }
        }

        private void ProcessNodeType(DungeonNode node)
        {
            switch (node.Type)
            {
                case NodeType.Combat:
                case NodeType.MiniBoss:
                case NodeType.Boss:
                    RequestCombat(node);
                    break;
                case NodeType.Event:
                    GameStateMachine.Instance.EnterEvent();
                    break;
                case NodeType.Shop:
                    GameStateMachine.Instance.EnterShop();
                    break;
                case NodeType.Rest:
                    GameStateMachine.Instance.EnterRest();
                    break;
            }
        }

        private void RequestCombat(DungeonNode node)
        {
            GameStateMachine.Instance.StartCombat();
            string enemyType = GetEnemyType(node.Type);
            CharacterType characterType = GetCharacterType(node.Type);
            CombatEvents.RequestCombatStart(enemyType, characterType);
        }

        private string GetEnemyType(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Combat => "루멘 리퍼",
                NodeType.MiniBoss => "그림자 수호자", 
                NodeType.Boss => "검은 심연",
                _ => "기본 적"
            };
        }

        private CharacterType GetCharacterType(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Combat => CharacterType.Normal,
                NodeType.MiniBoss => CharacterType.MiniBoss,
                NodeType.Boss => CharacterType.Boss,
                _ => CharacterType.Normal
            };
        }

        private List<DungeonNode> CreateSimpleDungeon()
        {
            var nodes = new List<DungeonNode>();
            
            // 시작 노드
            var startNode = new DungeonNode(0, NodeType.Combat, Vector2.zero);
            nodes.Add(startNode);
            
            // 중간 노드들
            for (int i = 1; i < _roomsPerStage - 1; i++)
            {
                NodeType type = (i == 5) ? NodeType.MiniBoss : GetRandomNodeType();
                var node = new DungeonNode(i, type, new Vector2(i * 100, 0));
                
                if (i > 0)
                {
                    nodes[i - 1].ConnectedNodes.Add(i);
                }
                
                nodes.Add(node);
            }
            
            // 보스 노드
            var bossNode = new DungeonNode(_roomsPerStage - 1, NodeType.Boss, 
                new Vector2((_roomsPerStage - 1) * 100, 0));
            nodes[_roomsPerStage - 2].ConnectedNodes.Add(_roomsPerStage - 1);
            nodes.Add(bossNode);
            
            return nodes;
        }

        private NodeType GetRandomNodeType()
        {
            float random = Random.value;
            
            if (random < 0.5f) return NodeType.Combat;
            else if (random < 0.7f) return NodeType.Event;
            else if (random < 0.85f) return NodeType.Shop;
            else return NodeType.Rest;
        }

        private void LogDebug(string message)
        {
            if (_enableDebugLogs)
            {
                Debug.Log($"[DungeonController] {message}");
            }
        }
    }
}
