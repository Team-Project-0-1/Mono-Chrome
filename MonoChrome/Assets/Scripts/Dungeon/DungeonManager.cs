using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Dungeon
{
    /// <summary>
    /// 던전 생성 및 관리를 담당하는 클래스
    /// </summary>
    public class DungeonManager : MonoBehaviour
    {
        #region Singleton
        private static DungeonManager _instance;
        public static DungeonManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<DungeonManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("DungeonManager");
                        _instance = obj.AddComponent<DungeonManager>();
                    }
                }
                return _instance;
            }
        }
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }
        }
        #endregion
        
        [Header("던전 설정")]
        [SerializeField] private int _roomsPerFloor = 15; // 층별 방 개수
        [SerializeField] private int _floorCount = 3; // 총 층 수
        [SerializeField] private int _branchCount = 3; // 각 지점별 분기 수
        
        [Header("방 타입 확률")]
        [Range(0, 1)] [SerializeField] private float _combatRoomChance = 0.5f;
        [Range(0, 1)] [SerializeField] private float _eventRoomChance = 0.2f;
        [Range(0, 1)] [SerializeField] private float _shopRoomChance = 0.15f;
        [Range(0, 1)] [SerializeField] private float _restRoomChance = 0.15f;
        
        [Header("현재 던전 상태")]
        [SerializeField] private int _currentFloor = 0;
        [SerializeField] private int _currentNodeIndex = -1;
        
        private List<DungeonNode> _dungeonNodes = new List<DungeonNode>();
        private DungeonNode _currentNode;
        
        private GameManager _gameManager;
        private UIManager _uiManager;
        
        private void Start()
        {
            // 게임 매니저 참조 가져오기
            _gameManager = GameManager.Instance;
            
            if (_gameManager != null)
            {
                _uiManager = _gameManager.UIManager;
            }
            else
            {
                Debug.LogError("DungeonManager: GameManager instance not found");
            }
        }
        
        /// <summary>
        /// 새 던전 생성
        /// </summary>
        public void GenerateNewDungeon()
        {
            Debug.Log("DungeonManager: Generating new dungeon");
            
            // 기존 던전 노드 정리
            _dungeonNodes.Clear();
            
            // 던전 상태 초기화
            _currentFloor = 0;
            _currentNodeIndex = -1;
            _currentNode = null;
            
            // 던전 노드 생성
            GenerateDungeonNodes();
            
            // 생성된 노드 디버그 출력
            Debug.Log("===== DUNGEON NODES =====");
            foreach (var node in _dungeonNodes)
            {
                Debug.Log($"Node ID: {node.ID}, Type: {node.Type}, Position: {node.Position}, " +
                          $"Accessible: {node.IsAccessible}, Connected to: {string.Join(", ", node.ConnectedNodes)}");
            }
            Debug.Log("=========================");
            
            // UI 업데이트
            UpdateDungeonUI();
            
            Debug.Log($"DungeonManager: Generated dungeon with {_dungeonNodes.Count} nodes");
        }
        
        /// <summary>
        /// 던전 노드 생성
        /// </summary>
        private void GenerateDungeonNodes()
        {
            // ID 카운터
            int nextId = 0;
            
            // 시작 노드 생성 (시작 노드는 Combat 타입으로 설정)
            DungeonNode startNode = new DungeonNode(nextId++, NodeType.Combat, new Vector2(0, 0));
            startNode.IsAccessible = true;
            _dungeonNodes.Add(startNode);
            
            // 중간 노드 생성
            for (int i = 1; i < _roomsPerFloor - 1; i++)
            {
                // 각 위치별로 분기 생성
                for (int branch = 0; branch < _branchCount; branch++)
                {
                    NodeType type = GetRandomNodeType();
                    
                    // 중간 보스방 설정 (5번째 방)
                    if (i == 5)
                    {
                        type = NodeType.MiniBoss;
                    }
                    
                    // 위치 계산 (x: 진행도, y: 분기)
                    Vector2 position = new Vector2(i, branch - (_branchCount - 1) / 2.0f);
                    
                    DungeonNode node = new DungeonNode(nextId++, type, position);
                    _dungeonNodes.Add(node);
                }
            }
            
            // 보스 노드 생성
            DungeonNode bossNode = new DungeonNode(nextId++, NodeType.Boss, new Vector2(_roomsPerFloor - 1, 0));
            _dungeonNodes.Add(bossNode);
            
            // 노드 간 연결 설정
            ConnectNodes();
        }
        
        /// <summary>
        /// 노드 간 연결 설정
        /// </summary>
        private void ConnectNodes()
        {
            // 각 위치별로 다음 위치의 노드와 연결
            for (int i = 0; i < _roomsPerFloor - 1; i++)
            {
                // 현재 위치의 노드 찾기
                List<DungeonNode> currentPositionNodes = _dungeonNodes.FindAll(n => Mathf.RoundToInt(n.Position.x) == i);
                
                // 다음 위치의 노드 찾기
                List<DungeonNode> nextPositionNodes = _dungeonNodes.FindAll(n => Mathf.RoundToInt(n.Position.x) == i + 1);
                
                // 연결 설정
                foreach (DungeonNode currentNode in currentPositionNodes)
                {
                    // 다음 위치에 노드가 1개만 있는 경우 (시작 노드 또는 보스 노드)
                    if (nextPositionNodes.Count == 1)
                    {
                        currentNode.ConnectedNodes.Add(nextPositionNodes[0].ID);
                        // 첫 번째 노드에서 접근 가능한 모든 노드는 접근 가능 표시
                        if (currentNode.IsAccessible || currentNode.IsVisited)
                        {
                            nextPositionNodes[0].IsAccessible = true;
                        }
                    }
                    // 다음 위치에 여러 노드가 있는 경우
                    else
                    {
                        // 가장 가까운 노드들과 연결
                        List<DungeonNode> nearestNodes = FindNearestNodes(currentNode, nextPositionNodes, 2);
                        foreach (DungeonNode nearNode in nearestNodes)
                        {
                            currentNode.ConnectedNodes.Add(nearNode.ID);
                            // 첫 번째 노드에서 접근 가능한 모든 노드는 접근 가능 표시
                            if (currentNode.IsAccessible || currentNode.IsVisited)
                            {
                                nearNode.IsAccessible = true;
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 가장 가까운 노드 찾기
        /// </summary>
        private List<DungeonNode> FindNearestNodes(DungeonNode sourceNode, List<DungeonNode> targetNodes, int count)
        {
            // 거리에 따라 노드 정렬
            List<DungeonNode> sortedNodes = new List<DungeonNode>(targetNodes);
            sortedNodes.Sort((a, b) => 
                Vector2.Distance(sourceNode.Position, a.Position).CompareTo(
                Vector2.Distance(sourceNode.Position, b.Position)));
            
            // 가장 가까운 노드 반환 (최대 count개)
            return sortedNodes.GetRange(0, Mathf.Min(count, sortedNodes.Count));
        }
        
        /// <summary>
        /// 방 타입 랜덤 결정
        /// </summary>
        private NodeType GetRandomNodeType()
        {
            float random = Random.value;
            
            if (random < _combatRoomChance)
            {
                return NodeType.Combat;
            }
            else if (random < _combatRoomChance + _eventRoomChance)
            {
                return NodeType.Event;
            }
            else if (random < _combatRoomChance + _eventRoomChance + _shopRoomChance)
            {
                return NodeType.Shop;
            }
            else
            {
                return NodeType.Rest;
            }
        }
        
        /// <summary>
        /// 던전 UI 업데이트
        /// </summary>
        private void UpdateDungeonUI()
        {
            if (_uiManager != null)
            {
                _uiManager.UpdateDungeonMap(_dungeonNodes, _currentNodeIndex);
            }
            else
            {
                Debug.LogError("DungeonManager: Cannot update dungeon UI - UIManager not found");
            }
        }
        
        /// <summary>
        /// 특정 던전 노드로 이동
        /// </summary>
        public void MoveToNode(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= _dungeonNodes.Count)
            {
                Debug.LogError($"DungeonManager: Invalid node index: {nodeIndex}");
                return;
            }
            
            DungeonNode targetNode = _dungeonNodes[nodeIndex];
            
            // 현재 노드에서 이동 가능한지 확인
            if (_currentNode != null && !targetNode.IsAccessible)
            {
                Debug.LogWarning($"DungeonManager: Cannot move to node {nodeIndex} - not accessible");
                return;
            }
            
            // 노드 이동
            _currentNodeIndex = nodeIndex;
            _currentNode = targetNode;
            _currentNode.IsVisited = true;
            _currentNode.IsAccessible = false; // 방문했으므로 접근 가능이 아닌 방문 완료로 변경
            
            // 연결된 노드들을 접근 가능으로 변경
            foreach (int connectedNodeId in _currentNode.ConnectedNodes)
            {
                DungeonNode connectedNode = _dungeonNodes.Find(n => n.ID == connectedNodeId);
                if (connectedNode != null && !connectedNode.IsVisited)
                {
                    connectedNode.IsAccessible = true;
                }
            }
            
            Debug.Log($"DungeonManager: Moved to node {nodeIndex}, type: {_currentNode.Type}");
            
            // 노드 유형에 따른 처리
            ProcessNodeByType(_currentNode);
            
            // UI 업데이트
            UpdateDungeonUI();
        }
        
        /// <summary>
        /// 노드 유형에 따른 처리
        /// </summary>
        private void ProcessNodeByType(DungeonNode node)
        {
            switch (node.Type)
            {
                case NodeType.Combat:
                    // 전투 시작
                    StartCombat("루멘 리퍼", CharacterType.Normal);
                    break;
                    
                case NodeType.MiniBoss:
                    // 미니보스 전투
                    StartCombat("그림자 수호자", CharacterType.MiniBoss);
                    break;
                    
                case NodeType.Boss:
                    // 보스 전투
                    StartCombat("검은 심연", CharacterType.Boss);
                    break;
                    
                case NodeType.Event:
                    // 이벤트 발생
                    TriggerEvent();
                    break;
                    
                case NodeType.Shop:
                    // 상점 UI 표시
                    ShowShop();
                    break;
                    
                case NodeType.Rest:
                    // 휴식 효과 적용
                    ApplyRest();
                    break;
            }
        }
        
        /// <summary>
        /// 전투 시작
        /// </summary>
        private void StartCombat(string enemyType, CharacterType type)
        {
            if (_gameManager != null)
            {
                // 적 생성
                CharacterManager characterManager = CharacterManager.Instance;
                if (characterManager != null)
                {
                    EnemyCharacter enemy = characterManager.CreateEnemyCharacter(enemyType, type);
                    
                    // 전투 매니저에 적 설정
                    if (_gameManager.CombatManager != null)
                    {
                        _gameManager.CombatManager.SetEnemyCharacter(enemy);
                    }
                }
                
                // 전투 시작
                _gameManager.StartCombat();
            }
            else
            {
                Debug.LogError("DungeonManager: Cannot start combat - GameManager not found");
            }
        }
        
        /// <summary>
        /// 이벤트 발생
        /// </summary>
        private void TriggerEvent()
        {
            // 이벤트 처리 - 이벤트 매니저를 통해 처리
            Debug.Log("DungeonManager: Event triggered");
            
            // 이벤트 매니저를 찾아서 호출
            EventManager eventManager = FindObjectOfType<EventManager>();
            if (eventManager != null && _currentNode != null)
            {
                eventManager.StartEvent(_currentNode);
            }
            else
            {
                Debug.LogError("DungeonManager: EventManager not found or currentNode is null");
                
                // 이벤트 매니저가 없는 경우에는 게임 상태만 변경
                if (_gameManager != null)
                {
                    _gameManager.ChangeState(GameManager.GameState.Event);
                }
            }
        }
        
        /// <summary>
        /// 상점 표시
        /// </summary>
        private void ShowShop()
        {
            // 상점 UI 표시 - ShopManager를 통해 처리
            Debug.Log("DungeonManager: Shop shown");
            
            // 상점 매니저를 찾아서 호출
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null && _currentNode != null)
            {
                shopManager.OpenShop(_currentNode);
            }
            else
            {
                Debug.LogError("DungeonManager: ShopManager not found or currentNode is null");
                // 상점 허거타임 에러 처리
                OnRoomCompleted(); // 방 종료
            }
        }
        
        /// <summary>
        /// 휴식 효과 적용
        /// </summary>
        private void ApplyRest()
        {
            // 휴식 효과 적용 - 추후 구현
            Debug.Log("DungeonManager: Rest applied");
            
            // 플레이어 체력 회복 (30%)
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.CurrentPlayer != null)
            {
                PlayerCharacter player = characterManager.CurrentPlayer;
                int healAmount = Mathf.RoundToInt(player.MaxHealth * 0.3f);
                player.Heal(healAmount);
                
                Debug.Log($"DungeonManager: Player healed for {healAmount}");
            }
        }
        
        /// <summary>
        /// 방 활동 완료 후 호출되는 메서드
        /// </summary>
        public void OnRoomCompleted()
        {
            Debug.Log("DungeonManager: Room activity completed");
            
            // 다음 방 선택을 위한 준비 - 추후 구현
            // 현재는 단순히 UI로 보여주지 않고 자동으로 던전 화면으로 복귀
            if (_gameManager != null)
            {
                _gameManager.ChangeState(GameManager.GameState.Dungeon);
            }
        }
    }
}