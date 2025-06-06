using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 고급 던전 생성기 - 완전한 노드 겹침 방지 및 최적화
    /// 포트폴리오 품질을 위한 전문적인 던전 레이아웃 알고리즘
    /// </summary>
    public class AdvancedDungeonGenerator : MonoBehaviour
    {
        #region Configuration
        [Header("던전 구조 설정")]
        [SerializeField] private int totalRooms = 15;
        [SerializeField] private int maxBranches = 3;
        [SerializeField] private float nodeSpacing = 120f;
        [SerializeField] private float levelWidth = 800f;
        [SerializeField] private float verticalSpread = 600f;
        
        [Header("노드 배치 알고리즘")]
        [SerializeField] private float minDistanceBetweenNodes = 100f;
        [SerializeField] private int maxPlacementAttempts = 100;
        [SerializeField] private bool useForceBasedLayout = true;
        [SerializeField] private float forceStrength = 50f;
        [SerializeField] private int forceIterations = 50;
        
        [Header("방 타입 확률")]
        [Range(0, 1)] [SerializeField] private float combatRoomChance = 0.5f;
        [Range(0, 1)] [SerializeField] private float eventRoomChance = 0.2f;
        [Range(0, 1)] [SerializeField] private float shopRoomChance = 0.15f;
        [Range(0, 1)] [SerializeField] private float restRoomChance = 0.15f;
        
        // 내부 데이터
        private List<Vector2> _occupiedPositions = new List<Vector2>();
        private Dictionary<int, List<DungeonNode>> _nodesByLevel = new Dictionary<int, List<DungeonNode>>();
        #endregion
        
        #region Public Methods
        /// <summary>
        /// 개선된 던전 생성 (메인 엔트리 포인트)
        /// </summary>
        public List<DungeonNode> GenerateAdvancedDungeon()
        {
            Debug.Log("AdvancedDungeonGenerator: 고급 던전 생성 시작");
            
            // 초기화
            InitializeGeneration();
            
            // 1단계: 레벨별 기본 구조 생성
            List<DungeonNode> nodes = GenerateBasicStructure();
            
            // 2단계: 노드 겹침 해결
            ResolveNodeOverlaps(nodes);
            
            // 3단계: 물리 기반 레이아웃 최적화 (선택사항)
            if (useForceBasedLayout)
            {
                ApplyForceBasedLayout(nodes);
            }
            
            // 4단계: 노드 연결 생성
            ConnectNodes(nodes);
            
            // 5단계: 최종 검증
            ValidateLayout(nodes);
            
            Debug.Log($"AdvancedDungeonGenerator: 던전 생성 완료 - {nodes.Count}개 노드");
            return nodes;
        }
        #endregion
        
        #region Generation Steps
        /// <summary>
        /// 생성 초기화
        /// </summary>
        private void InitializeGeneration()
        {
            _occupiedPositions.Clear();
            _nodesByLevel.Clear();
            Random.InitState(System.DateTime.Now.Millisecond);
        }
        
        /// <summary>
        /// 기본 구조 생성
        /// </summary>
        private List<DungeonNode> GenerateBasicStructure()
        {
            List<DungeonNode> nodes = new List<DungeonNode>();
            int nodeId = 0;
            
            // 레벨별 노드 수 계산
            List<int> nodesPerLevel = CalculateNodesPerLevel();
            
            for (int level = 0; level < nodesPerLevel.Count; level++)
            {
                List<DungeonNode> levelNodes = GenerateLevelNodes(level, nodesPerLevel[level], ref nodeId);
                nodes.AddRange(levelNodes);
                _nodesByLevel[level] = levelNodes;
            }
            
            return nodes;
        }
        
        /// <summary>
        /// 레벨별 노드 수 계산
        /// </summary>
        private List<int> CalculateNodesPerLevel()
        {
            List<int> nodesPerLevel = new List<int>();
            int remainingNodes = totalRooms;
            int levels = Mathf.CeilToInt((float)totalRooms / maxBranches);
            
            for (int level = 0; level < levels; level++)
            {
                int nodesForThisLevel;
                
                if (level == 0)
                {
                    // 시작 레벨: 1개
                    nodesForThisLevel = 1;
                }
                else if (level == levels - 1)
                {
                    // 마지막 레벨: 1개 (보스)
                    nodesForThisLevel = 1;
                }
                else if (level == levels - 2)
                {
                    // 보스 직전 레벨: 미니보스 포함
                    nodesForThisLevel = Mathf.Min(maxBranches, remainingNodes - 1);
                }
                else
                {
                    // 중간 레벨: 점진적 증가 후 감소
                    float progressRatio = (float)level / (levels - 1);
                    float branchFactor = 1.0f - Mathf.Abs(progressRatio * 2.0f - 1.0f); // 중앙에서 최대
                    nodesForThisLevel = Mathf.RoundToInt(maxBranches * (0.3f + branchFactor * 0.7f));
                    nodesForThisLevel = Mathf.Clamp(nodesForThisLevel, 1, maxBranches);
                    nodesForThisLevel = Mathf.Min(nodesForThisLevel, remainingNodes - (levels - level - 1));
                }
                
                nodesPerLevel.Add(nodesForThisLevel);
                remainingNodes -= nodesForThisLevel;
                
                if (remainingNodes <= 0) break;
            }
            
            return nodesPerLevel;
        }
        
        /// <summary>
        /// 레벨 노드들 생성
        /// </summary>
        private List<DungeonNode> GenerateLevelNodes(int level, int nodeCount, ref int nodeId)
        {
            List<DungeonNode> levelNodes = new List<DungeonNode>();
            float baseX = level * nodeSpacing;
            
            // Y 위치 후보들 생성
            List<float> yPositions = GenerateYPositions(nodeCount);
            
            for (int i = 0; i < nodeCount; i++)
            {
                Vector2 targetPosition = new Vector2(baseX, yPositions[i]);
                Vector2 finalPosition = FindValidPosition(targetPosition, levelNodes);
                
                NodeType nodeType = DetermineNodeType(level, nodeCount, i);
                DungeonNode node = new DungeonNode(nodeId++, nodeType, finalPosition);
                
                // 시작 노드는 접근 가능
                if (level == 0)
                {
                    node.IsAccessible = true;
                }
                
                levelNodes.Add(node);
                _occupiedPositions.Add(finalPosition);
            }
            
            return levelNodes;
        }
        
        /// <summary>
        /// Y 위치들 생성
        /// </summary>
        private List<float> GenerateYPositions(int nodeCount)
        {
            List<float> positions = new List<float>();
            
            if (nodeCount == 1)
            {
                positions.Add(0f);
            }
            else
            {
                // 균등 분포 기반
                for (int i = 0; i < nodeCount; i++)
                {
                    float normalizedPos = (float)i / (nodeCount - 1); // 0 ~ 1
                    float centeredPos = (normalizedPos - 0.5f) * verticalSpread; // 중앙 기준
                    
                    // 약간의 랜덤 오프셋 추가
                    float randomOffset = Random.Range(-nodeSpacing * 0.2f, nodeSpacing * 0.2f);
                    positions.Add(centeredPos + randomOffset);
                }
            }
            
            return positions;
        }
        
        /// <summary>
        /// 유효한 위치 찾기 (겹침 방지)
        /// </summary>
        private Vector2 FindValidPosition(Vector2 targetPosition, List<DungeonNode> existingLevelNodes)
        {
            Vector2 bestPosition = targetPosition;
            
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                if (IsPositionValid(bestPosition, existingLevelNodes))
                {
                    return bestPosition;
                }
                
                // 위치 조정
                if (attempt < maxPlacementAttempts / 2)
                {
                    // 초기 시도: 작은 원형 오프셋
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float distance = (attempt + 1) * 20f;
                    bestPosition = targetPosition + new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                }
                else
                {
                    // 후반 시도: 더 넓은 범위의 랜덤 위치
                    bestPosition = targetPosition + new Vector2(
                        Random.Range(-nodeSpacing, nodeSpacing),
                        Random.Range(-nodeSpacing, nodeSpacing)
                    );
                }
            }
            
            Debug.LogWarning($"AdvancedDungeonGenerator: 유효한 위치를 찾지 못함 - {targetPosition}");
            return bestPosition; // 실패해도 반환
        }
        
        /// <summary>
        /// 위치 유효성 검증
        /// </summary>
        private bool IsPositionValid(Vector2 position, List<DungeonNode> existingLevelNodes)
        {
            // 기존 모든 노드와의 거리 검사
            foreach (Vector2 occupiedPos in _occupiedPositions)
            {
                if (Vector2.Distance(position, occupiedPos) < minDistanceBetweenNodes)
                {
                    return false;
                }
            }
            
            // 현재 레벨 내 노드들과의 거리 검사
            foreach (DungeonNode node in existingLevelNodes)
            {
                if (Vector2.Distance(position, node.Position) < minDistanceBetweenNodes)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 노드 타입 결정
        /// </summary>
        private NodeType DetermineNodeType(int level, int levelNodeCount, int nodeIndex)
        {
            // 특수 케이스들
            if (level == 0)
                return NodeType.Combat; // 시작점
                
            if (level == _nodesByLevel.Count && levelNodeCount == 1)
                return NodeType.Boss; // 마지막 단일 노드는 보스
                
            // 미니보스 (보스 직전 레벨에 배치)
            if (level == _nodesByLevel.Count - 1)
                return NodeType.MiniBoss;
            
            // 일반 방들은 확률적으로 결정
            return GetRandomNodeType();
        }
        
        /// <summary>
        /// 랜덤 노드 타입
        /// </summary>
        private NodeType GetRandomNodeType()
        {
            float random = Random.value;
            
            if (random < combatRoomChance)
                return NodeType.Combat;
            else if (random < combatRoomChance + eventRoomChance)
                return NodeType.Event;
            else if (random < combatRoomChance + eventRoomChance + shopRoomChance)
                return NodeType.Shop;
            else
                return NodeType.Rest;
        }
        #endregion
        
        #region Overlap Resolution
        /// <summary>
        /// 노드 겹침 해결
        /// </summary>
        private void ResolveNodeOverlaps(List<DungeonNode> nodes)
        {
            Debug.Log("AdvancedDungeonGenerator: 노드 겹침 해결 시작");
            
            bool hasOverlaps = true;
            int iterations = 0;
            int maxIterations = 50;
            
            while (hasOverlaps && iterations < maxIterations)
            {
                hasOverlaps = false;
                
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        DungeonNode nodeA = nodes[i];
                        DungeonNode nodeB = nodes[j];
                        
                        float distance = Vector2.Distance(nodeA.Position, nodeB.Position);
                        if (distance < minDistanceBetweenNodes)
                        {
                            // 겹침 발견, 분리
                            SeparateNodes(nodeA, nodeB, distance);
                            hasOverlaps = true;
                        }
                    }
                }
                
                iterations++;
            }
            
            Debug.Log($"AdvancedDungeonGenerator: 겹침 해결 완료 - {iterations}회 반복");
        }
        
        /// <summary>
        /// 두 노드 분리
        /// </summary>
        private void SeparateNodes(DungeonNode nodeA, DungeonNode nodeB, float currentDistance)
        {
            Vector2 direction = (nodeB.Position - nodeA.Position).normalized;
            float separationDistance = (minDistanceBetweenNodes - currentDistance) * 0.6f;
            
            // 둘 다 이동 (질량이 같다고 가정)
            Vector2 offsetA = -direction * separationDistance * 0.5f;
            Vector2 offsetB = direction * separationDistance * 0.5f;
            
            nodeA.Position += offsetA;
            nodeB.Position += offsetB;
        }
        #endregion
        
        #region Force-Based Layout
        /// <summary>
        /// 물리 기반 레이아웃 적용
        /// </summary>
        private void ApplyForceBasedLayout(List<DungeonNode> nodes)
        {
            Debug.Log("AdvancedDungeonGenerator: 물리 기반 레이아웃 적용");
            
            for (int iteration = 0; iteration < forceIterations; iteration++)
            {
                // 각 노드에 대해 힘 계산 및 적용
                Vector2[] forces = new Vector2[nodes.Count];
                
                for (int i = 0; i < nodes.Count; i++)
                {
                    forces[i] = CalculateForces(nodes[i], nodes);
                }
                
                // 힘 적용
                for (int i = 0; i < nodes.Count; i++)
                {
                    Vector2 newPosition = nodes[i].Position + forces[i] * Time.fixedDeltaTime;
                    
                    // 레벨 제약 적용 (X축 고정)
                    int nodeLevel = Mathf.RoundToInt(nodes[i].Position.x / nodeSpacing);
                    newPosition.x = nodeLevel * nodeSpacing;
                    
                    // Y축 범위 제한
                    newPosition.y = Mathf.Clamp(newPosition.y, -verticalSpread * 0.6f, verticalSpread * 0.6f);
                    
                    nodes[i].Position = newPosition;
                }
            }
        }
        
        /// <summary>
        /// 노드에 작용하는 힘 계산
        /// </summary>
        private Vector2 CalculateForces(DungeonNode targetNode, List<DungeonNode> allNodes)
        {
            Vector2 totalForce = Vector2.zero;
            
            foreach (DungeonNode otherNode in allNodes)
            {
                if (otherNode == targetNode) continue;
                
                Vector2 direction = targetNode.Position - otherNode.Position;
                float distance = direction.magnitude;
                
                if (distance > 0 && distance < minDistanceBetweenNodes * 2f)
                {
                    // 척력 (너무 가까우면 밀어내기)
                    float repulsionStrength = forceStrength / (distance * distance);
                    totalForce += direction.normalized * repulsionStrength;
                }
            }
            
            return totalForce;
        }
        #endregion
        
        #region Node Connection
        /// <summary>
        /// 노드 연결 생성
        /// </summary>
        private void ConnectNodes(List<DungeonNode> nodes)
        {
            Debug.Log("AdvancedDungeonGenerator: 노드 연결 생성");
            
            for (int level = 0; level < _nodesByLevel.Count - 1; level++)
            {
                if (!_nodesByLevel.ContainsKey(level) || !_nodesByLevel.ContainsKey(level + 1))
                    continue;
                
                ConnectLevels(_nodesByLevel[level], _nodesByLevel[level + 1]);
            }
        }
        
        /// <summary>
        /// 레벨 간 연결
        /// </summary>
        private void ConnectLevels(List<DungeonNode> currentLevel, List<DungeonNode> nextLevel)
        {
            foreach (DungeonNode currentNode in currentLevel)
            {
                // 가장 가까운 1-2개 노드와 연결
                List<DungeonNode> nearestNodes = nextLevel
                    .OrderBy(n => Vector2.Distance(currentNode.Position, n.Position))
                    .Take(Mathf.Min(2, nextLevel.Count))
                    .ToList();
                
                foreach (DungeonNode nearNode in nearestNodes)
                {
                    if (!currentNode.ConnectedNodes.Contains(nearNode.ID))
                    {
                        currentNode.ConnectedNodes.Add(nearNode.ID);
                        
                        // 접근성 전파
                        if (currentNode.IsAccessible)
                        {
                            nearNode.IsAccessible = true;
                        }
                    }
                }
                
                // 최소 1개 연결 보장
                if (currentNode.ConnectedNodes.Count == 0 && nextLevel.Count > 0)
                {
                    DungeonNode closestNode = nextLevel.OrderBy(n => Vector2.Distance(currentNode.Position, n.Position)).First();
                    currentNode.ConnectedNodes.Add(closestNode.ID);
                    
                    if (currentNode.IsAccessible)
                    {
                        closestNode.IsAccessible = true;
                    }
                }
            }
        }
        #endregion
        
        #region Validation
        /// <summary>
        /// 레이아웃 검증
        /// </summary>
        private void ValidateLayout(List<DungeonNode> nodes)
        {
            int overlapCount = 0;
            int disconnectedCount = 0;
            
            // 겹침 검사
            for (int i = 0; i < nodes.Count; i++)
            {
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    if (Vector2.Distance(nodes[i].Position, nodes[j].Position) < minDistanceBetweenNodes)
                    {
                        overlapCount++;
                    }
                }
            }
            
            // 연결성 검사
            foreach (DungeonNode node in nodes)
            {
                if (node.ConnectedNodes.Count == 0 && node.Type != NodeType.Boss)
                {
                    disconnectedCount++;
                }
            }
            
            Debug.Log($"AdvancedDungeonGenerator: 레이아웃 검증 완료");
            Debug.Log($"  - 겹침: {overlapCount}개");
            Debug.Log($"  - 연결되지 않은 노드: {disconnectedCount}개");
            
            if (overlapCount > 0)
            {
                Debug.LogWarning("AdvancedDungeonGenerator: 일부 노드가 여전히 겹쳐있습니다.");
            }
            
            if (disconnectedCount > 0)
            {
                Debug.LogWarning("AdvancedDungeonGenerator: 일부 노드가 연결되지 않았습니다.");
            }
        }
        #endregion
    }
}
