using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonoChrome.Events;
using MonoChrome.Core;
using MonoChrome.Systems.Dungeon;

namespace MonoChrome
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
        
        [Header("디버그 설정")]
        [SerializeField] private bool _enableDebugMode = true;
        [SerializeField] private bool _showDetailedNodeInfo = true;

        private List<DungeonNode> _dungeonNodes = new List<DungeonNode>();
        private DungeonNode _currentNode;
        private int _currentNodeIndex = -1;
        private int _currentStage = 0;
        
        [Header("턴 기반 시스템")]
        [SerializeField] private bool _useTurnBasedSystem = true;
        private TurnBasedDungeonGenerator _turnBasedGenerator;
        private int _currentTurn = 1;

        public List<DungeonNode> CurrentDungeonNodes => new List<DungeonNode>(_dungeonNodes);
        public DungeonNode CurrentNode => _currentNode;
        public int CurrentNodeIndex => _currentNodeIndex;

        private void Awake()
        {
            LogDebug("DungeonController Awake() 호출");
        }

        private void Start()
        {
            LogDebug("DungeonController Start() 호출");
            
            // 이벤트 구독 상태 확인
            int subscriberCount = DungeonEvents.GetSubscriberCount();
            LogDebug($"현재 던전 생성 이벤트 구독자 수: {subscriberCount}");
            
            // 구독이 되어 있지 않다면 다시 구독
            if (subscriberCount == 0)
            {
                LogDebug("구독자가 없음 - 이벤트 재구독 시작");
                DungeonEvents.OnDungeonGenerationRequested += HandleDungeonGenerationRequest;
                DungeonEvents.OnNodeMoveRequested += HandleNodeMoveRequest;
                DungeonEvents.OnRoomActivityCompleted += HandleRoomActivityCompleted;
                LogDebug("이벤트 재구독 완료");
            }
            
            LogDebug("DungeonController Start() 완료");
        }

        private void OnEnable()
        {
            LogDebug("이벤트 구독 시작");
            
            // 중복 구독 방지를 위해 먼저 해제
            DungeonEvents.OnDungeonGenerationRequested -= HandleDungeonGenerationRequest;
            DungeonEvents.OnNodeMoveRequested -= HandleNodeMoveRequest;
            DungeonEvents.OnRoomActivityCompleted -= HandleRoomActivityCompleted;
            
            // 이벤트 구독
            DungeonEvents.OnDungeonGenerationRequested += HandleDungeonGenerationRequest;
            DungeonEvents.OnNodeMoveRequested += HandleNodeMoveRequest;
            DungeonEvents.OnRoomActivityCompleted += HandleRoomActivityCompleted;
            DungeonEvents.OnRoomChoiceSelected += HandleRoomChoiceSelected;
            
            LogDebug($"이벤트 구독 완료 (구독자 수: {DungeonEvents.GetSubscriberCount()})");
        }

        private void OnDisable()
        {
            LogDebug("이벤트 구독 해제 시작");
            DungeonEvents.OnDungeonGenerationRequested -= HandleDungeonGenerationRequest;
            DungeonEvents.OnNodeMoveRequested -= HandleNodeMoveRequest;
            DungeonEvents.OnRoomActivityCompleted -= HandleRoomActivityCompleted;
            DungeonEvents.OnRoomChoiceSelected -= HandleRoomChoiceSelected;
            LogDebug("이벤트 구독 해제 완료");
        }

        private void HandleDungeonGenerationRequest(int stageIndex)
        {
            LogDebug($"=== 던전 생성 이벤트 수신: 스테이지 {stageIndex} ===");
            LogDebug($"현재 구독자 수: {DungeonEvents.GetSubscriberCount()}");
            GenerateDungeon(stageIndex);
            LogDebug($"=== 던전 생성 이벤트 처리 완료: 스테이지 {stageIndex} ===");
        }

        private void HandleNodeMoveRequest(int nodeIndex)
        {
            MoveToNode(nodeIndex);
        }

        private void HandleRoomActivityCompleted()
        {
            LogDebug("방 활동 완료 처리");
            GameStateMachine.Instance.CompleteRoomActivity();
            
            // 턴 기반 시스템인 경우 다음 턴 진행
            if (_useTurnBasedSystem && _turnBasedGenerator != null)
            {
                AdvanceToNextTurn();
            }
        }

        private void HandleRoomChoiceSelected(RoomChoice selectedChoice)
        {
            LogDebug($"=== 방 선택 완료: {selectedChoice.TurnNumber}턴 {selectedChoice.Type} ===");
            
            // 선택 기록
            if (_turnBasedGenerator != null)
            {
                _turnBasedGenerator.RecordPlayerChoice(selectedChoice);
            }
            
            // 해당 방 타입에 맞는 패널 활성화 및 게임 상태 변경
            ProcessRoomChoice(selectedChoice);
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
            
            if (_useTurnBasedSystem)
            {
                StartTurnBasedDungeon(stageIndex);
            }
            else
            {
                _dungeonNodes = CreateSimpleDungeon();
                SetupStartingNode();
                DungeonEvents.UIEvents.RequestDungeonMapUpdate(_dungeonNodes, _currentNodeIndex);
                DungeonEvents.NotifyDungeonGenerated(_dungeonNodes, _currentNodeIndex);
            }
            
            LogDebug($"던전 생성 완료");
            
            // 디버그 모드일 때 자동으로 던전 구조 출력
            if (_enableDebugMode)
            {
                if (_useTurnBasedSystem)
                {
                    LogDebug("턴 기반 던전 시스템 활성화됨");
                }
                else
                {
                    DebugDungeonStructure();
                }
            }
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
            DungeonEvents.UIEvents.RequestDungeonMapUpdate(_dungeonNodes, _currentNodeIndex);
            
            LogDebug($"노드 이동 완료: {nodeIndex + 1}턴 ({targetNode.Type}) - {targetNode.Description}");
            
            // 디버그 모드일 때 현재 상태 출력
            if (_enableDebugMode)
            {
                DebugCurrentState();
            }
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
            DungeonEvents.CombatEvents.RequestCombatStart(enemyType, characterType);
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

        #region 턴 기반 던전 시스템

        /// <summary>
        /// 턴 기반 던전 시작
        /// </summary>
        private void StartTurnBasedDungeon(int stageIndex)
        {
            LogDebug($"턴 기반 던전 시작 - 스테이지 {stageIndex + 1}");
            
            // 턴 기반 생성기 초기화
            if (_turnBasedGenerator == null)
            {
                _turnBasedGenerator = new TurnBasedDungeonGenerator(_roomsPerStage);
            }
            
            _turnBasedGenerator.StartNewDungeon();
            _currentTurn = 1;
            
            // 첫 턴 선택지 생성 및 표시
            GenerateAndShowChoicesForCurrentTurn();
        }

        /// <summary>
        /// 현재 턴의 선택지 생성 및 표시
        /// </summary>
        private void GenerateAndShowChoicesForCurrentTurn()
        {
            LogDebug($"현재 턴 ({_currentTurn}) 선택지 생성");
            
            // 플레이어 감각 타입 가져오기 (추후 CharacterDataManager에서 가져오도록 개선)
            SenseType playerSense = GetPlayerSenseType();
            
            // 선택지 생성
            List<RoomChoice> choices = _turnBasedGenerator.GenerateChoicesForTurnWithSense(_currentTurn, playerSense);
            
            // UI에 선택지 표시
            DungeonEvents.RequestRoomChoicesUpdate(choices);
            
            LogDebug($"턴 {_currentTurn} 선택지 표시 완료: {string.Join(", ", choices.Select(c => c.Type))}");
        }

        /// <summary>
        /// 다음 턴으로 진행
        /// </summary>
        private void AdvanceToNextTurn()
        {
            if (_turnBasedGenerator == null || !_turnBasedGenerator.CanContinue())
            {
                LogDebug("던전 완료 또는 시스템 오류");
                CompleteDungeon();
                return;
            }

            _currentTurn++;
            LogDebug($"다음 턴으로 진행: {_currentTurn}턴");
            
            // 던전 완료 확인
            if (_turnBasedGenerator.IsCompleted())
            {
                LogDebug("던전 완료!");
                CompleteDungeon();
                return;
            }
            
            // 다음 턴 선택지 생성
            GenerateAndShowChoicesForCurrentTurn();
        }

        /// <summary>
        /// 선택한 방 처리 및 패널 전환
        /// </summary>
        private void ProcessRoomChoice(RoomChoice selectedChoice)
        {
            LogDebug($"방 선택 처리: {selectedChoice.Type}");
            
            switch (selectedChoice.Type)
            {
                case NodeType.Combat:
                case NodeType.MiniBoss:
                case NodeType.Boss:
                    StartCombatFromChoice(selectedChoice);
                    break;
                    
                case NodeType.Event:
                    StartEventFromChoice(selectedChoice);
                    break;
                    
                case NodeType.Shop:
                    StartShopFromChoice(selectedChoice);
                    break;
                    
                case NodeType.Rest:
                    StartRestFromChoice(selectedChoice);
                    break;
                    
                default:
                    LogDebug($"알 수 없는 방 타입: {selectedChoice.Type}");
                    break;
            }
        }

        /// <summary>
        /// 전투 시작 (RoomChoice 기반)
        /// </summary>
        private void StartCombatFromChoice(RoomChoice choice)
        {
            LogDebug($"전투 시작: {choice.Type}");
            
            GameStateMachine.Instance.StartCombat();
            
            string enemyType = GetEnemyType(choice.Type);
            CharacterType characterType = GetCharacterType(choice.Type);
            
            DungeonEvents.CombatEvents.RequestCombatStart(enemyType, characterType);
        }

        /// <summary>
        /// 이벤트 시작 (RoomChoice 기반)
        /// </summary>
        private void StartEventFromChoice(RoomChoice choice)
        {
            LogDebug($"이벤트 시작: {choice.EventId}");
            
            GameStateMachine.Instance.EnterEvent();
            DungeonEvents.UIEvents.RequestDungeonSubPanelShow("EventPanel");
            
            // 추후 특정 이벤트 ID가 있다면 해당 이벤트 로드
            if (!string.IsNullOrEmpty(choice.EventId))
            {
                // LoadSpecificEvent(choice.EventId);
            }
        }

        /// <summary>
        /// 상점 시작 (RoomChoice 기반)
        /// </summary>
        private void StartShopFromChoice(RoomChoice choice)
        {
            LogDebug("상점 시작");
            
            GameStateMachine.Instance.EnterShop();
            DungeonEvents.UIEvents.RequestDungeonSubPanelShow("ShopPanel");
        }

        /// <summary>
        /// 휴식 시작 (RoomChoice 기반)
        /// </summary>
        private void StartRestFromChoice(RoomChoice choice)
        {
            LogDebug("휴식 시작");
            
            GameStateMachine.Instance.EnterRest();
            DungeonEvents.UIEvents.RequestDungeonSubPanelShow("RestPanel");
        }

        /// <summary>
        /// 플레이어 감각 타입 가져오기 (임시 구현)
        /// </summary>
        private SenseType GetPlayerSenseType()
        {
            // 추후 CharacterDataManager 또는 PlayerManager에서 가져오도록 개선
            // 현재는 테스트용으로 랜덤 반환
            var senseTypes = new[] { SenseType.Auditory, SenseType.Olfactory, SenseType.Tactile, SenseType.Spiritual };
            return senseTypes[UnityEngine.Random.Range(0, senseTypes.Length)];
        }

        /// <summary>
        /// 던전 완료 처리
        /// </summary>
        private void CompleteDungeon()
        {
            LogDebug("던전 완료 처리");
            
            if (_turnBasedGenerator != null)
            {
                var summary = _turnBasedGenerator.DungeonData.GetSummary();
                LogDebug($"던전 완료 요약: {summary}");
                
                // 던전 완료 통계 로그
                LogDebug($"완료된 던전 통계:");
                LogDebug($"  - 스테이지: {_currentStage + 1}");
                LogDebug($"  - 총 턴: {summary.CurrentTurn - 1}");
                LogDebug($"  - 진행률: {summary.Progress:F1}%");
                LogDebug($"  - 소요 시간: {summary.ElapsedTime.TotalMinutes:F1}분");
                LogDebug($"  - 마지막 선택: {summary.LastChoiceType}");
            }
            
            // 던전 완료 상태로 전환
            GameStateMachine.Instance.CompleteDungeon();
            
            // 던전 완료 이벤트 발행 (다른 시스템에서 처리할 수 있도록)
            DungeonEvents.NotifyDungeonCompleted(_currentStage);
            LogDebug("던전 완료 이벤트 발행 완료");
        }

        #endregion

        #region 디버그 시스템
        
        [ContextMenu("던전 구조 디버그")]
        public void DebugDungeonStructure()
        {
            if (!_enableDebugMode) return;

            Debug.Log("=================================================");
            Debug.Log($"          던전 구조 디버그 (스테이지 {_currentStage + 1})         ");
            Debug.Log("=================================================");
            Debug.Log($"총 노드 수: {_dungeonNodes.Count}");
            Debug.Log($"현재 노드: {_currentNodeIndex + 1}턴 ({(_currentNode?.Type.ToString() ?? "없음")})");
            Debug.Log($"진행률: {GetDungeonProgress():F1}%");
            Debug.Log("=================================================");
            
            for (int i = 0; i < _dungeonNodes.Count; i++)
            {
                var node = _dungeonNodes[i];
                string status = GetNodeStatusString(i, node);
                string connections = GetNodeConnectionsString(node);
                
                Debug.Log($"{i + 1,2}턴: {node.Type,-10} {status,-12} {connections}");
                
                if (_showDetailedNodeInfo)
                {
                    Debug.Log($"     └─ 설명: {node.Description}");
                    Debug.Log($"     └─ 위치: {node.Position}");
                }
            }
            
            Debug.Log("=================================================");
            DebugTypeDistribution();
            Debug.Log("=================================================");
        }

        [ContextMenu("방 타입 분포 디버그")]
        public void DebugTypeDistribution()
        {
            if (!_enableDebugMode || _dungeonNodes.Count == 0) return;

            var typeCount = new Dictionary<NodeType, int>();
            
            foreach (var node in _dungeonNodes)
            {
                if (typeCount.ContainsKey(node.Type))
                    typeCount[node.Type]++;
                else
                    typeCount[node.Type] = 1;
            }

            Debug.Log("📊 방 타입 분포:");
            foreach (var kvp in typeCount)
            {
                float percentage = (float)kvp.Value / _dungeonNodes.Count * 100f;
                Debug.Log($"   {kvp.Key,-10}: {kvp.Value,2}개 ({percentage,4:F1}%)");
            }
        }

        [ContextMenu("현재 상태 디버그")]
        public void DebugCurrentState()
        {
            if (!_enableDebugMode) return;

            Debug.Log("🎯 현재 던전 상태:");
            Debug.Log($"   현재 턴: {_currentNodeIndex + 1}/{_dungeonNodes.Count}");
            Debug.Log($"   현재 방: {_currentNode?.Type ?? NodeType.None}");
            Debug.Log($"   방문한 방 수: {GetVisitedNodeCount()}");
            Debug.Log($"   접근 가능한 방 수: {GetAccessibleNodeCount()}");
            
            var accessibleNodes = GetAccessibleNodes();
            if (accessibleNodes.Count > 0)
            {
                Debug.Log("   다음 선택 가능한 방들:");
                foreach (var node in accessibleNodes)
                {
                    int index = _dungeonNodes.IndexOf(node);
                    Debug.Log($"     └─ {index + 1}턴: {node.Type} - {node.Description}");
                }
            }
        }

        [ContextMenu("던전 진행 경로 디버그")]
        public void DebugDungeonPath()
        {
            if (!_enableDebugMode) return;

            Debug.Log("🗺️ 던전 진행 경로:");
            
            var visitedNodes = _dungeonNodes.Where(n => n.IsVisited).ToList();
            if (visitedNodes.Count == 0)
            {
                Debug.Log("   아직 방문한 방이 없습니다.");
                return;
            }

            for (int i = 0; i < visitedNodes.Count; i++)
            {
                var node = visitedNodes[i];
                int nodeIndex = _dungeonNodes.IndexOf(node);
                string marker = (nodeIndex == _currentNodeIndex) ? "👤" : "✅";
                Debug.Log($"   {i + 1}. {marker} {nodeIndex + 1}턴: {node.Type} - {node.Description}");
            }
        }

        private string GetNodeStatusString(int index, DungeonNode node)
        {
            if (index == _currentNodeIndex) return "[현재위치]";
            if (node.IsVisited) return "[방문완료]";
            if (node.IsAccessible) return "[접근가능]";
            return "[잠김상태]";
        }

        private string GetNodeConnectionsString(DungeonNode node)
        {
            if (node.ConnectedNodes.Count == 0) return "";
            return $"→ 연결: [{string.Join(", ", node.ConnectedNodes.Select(id => (id + 1).ToString()))}]";
        }

        private float GetDungeonProgress()
        {
            if (_dungeonNodes.Count == 0) return 0f;
            return ((float)(_currentNodeIndex + 1) / _dungeonNodes.Count) * 100f;
        }

        private int GetVisitedNodeCount()
        {
            return _dungeonNodes.Count(n => n.IsVisited);
        }

        private int GetAccessibleNodeCount()
        {
            return _dungeonNodes.Count(n => n.IsAccessible && !n.IsVisited);
        }

        private List<DungeonNode> GetAccessibleNodes()
        {
            return _dungeonNodes.Where(n => n.IsAccessible && !n.IsVisited).ToList();
        }

        #endregion
    }
}
