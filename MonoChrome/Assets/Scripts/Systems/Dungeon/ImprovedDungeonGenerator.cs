using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 개선된 던전 생성기 - 노드 겹침 방지 및 최적화된 배치
    /// 포트폴리오 품질을 위한 체계적인 던전 구조 생성
    /// </summary>
    /// <remarks>
    /// Provides a lightweight layout algorithm focused on simple overlap
    /// prevention. It remains for experimentation and can be chosen via
    /// <see cref="ConfigurableDungeonGenerator"/>.
    /// </remarks>
    public class ImprovedDungeonGenerator : MonoBehaviour
    {
        [Header("던전 설정")]
        [SerializeField] private int roomsPerFloor = 15;
        [SerializeField] private int branchCount = 3;
        [SerializeField] private float minNodeDistance = 100f; // 노드 간 최소 거리
        [SerializeField] private int maxPlacementAttempts = 50; // 배치 시도 횟수
        
        [Header("레이아웃 설정")]
        [SerializeField] private float horizontalSpacing = 200f;
        [SerializeField] private float verticalSpread = 300f;
        [SerializeField] private float randomPositionVariance = 50f;
        
        [Header("방 타입 확률")]
        [Range(0, 1)] [SerializeField] private float combatRoomChance = 0.5f;
        [Range(0, 1)] [SerializeField] private float eventRoomChance = 0.2f;
        [Range(0, 1)] [SerializeField] private float shopRoomChance = 0.15f;
        [Range(0, 1)] [SerializeField] private float restRoomChance = 0.15f;
        
        /// <summary>
        /// 겹침 방지 알고리즘을 사용한 던전 노드 생성
        /// </summary>
        public List<DungeonNode> GenerateImprovedDungeon()
        {
            List<DungeonNode> nodes = new List<DungeonNode>();
            int nextId = 0;
            
            // 1단계: 시작 노드 생성
            DungeonNode startNode = new DungeonNode(nextId++, NodeType.Combat, Vector2.zero);
            startNode.IsAccessible = true;
            nodes.Add(startNode);
            
            Debug.Log("ImprovedDungeonGenerator: Created start node");
            
            // 2단계: 레벨별 노드 생성 (겹침 방지)
            for (int level = 1; level < roomsPerFloor - 1; level++)
            {
                List<Vector2> levelPositions = GenerateLevelPositions(level, nodes);
                
                foreach (Vector2 position in levelPositions)
                {
                    NodeType nodeType = GetNodeTypeForLevel(level);
                    DungeonNode node = new DungeonNode(nextId++, nodeType, position);
                    nodes.Add(node);
                }
                
                Debug.Log($"ImprovedDungeonGenerator: Created {levelPositions.Count} nodes for level {level}");
            }
            
            // 3단계: 보스 노드 생성
            Vector2 bossPosition = new Vector2((roomsPerFloor - 1) * horizontalSpacing, 0);
            DungeonNode bossNode = new DungeonNode(nextId++, NodeType.Boss, bossPosition);
            nodes.Add(bossNode);
            
            // 4단계: 노드 연결
            ConnectNodesImproved(nodes);
            
            Debug.Log($"ImprovedDungeonGenerator: Generated {nodes.Count} nodes total");
            return nodes;
        }
        
        /// <summary>
        /// 특정 레벨의 노드 위치 생성 (겹침 방지)
        /// </summary>
        private List<Vector2> GenerateLevelPositions(int level, List<DungeonNode> existingNodes)
        {
            List<Vector2> positions = new List<Vector2>();
            
            // 해당 레벨의 기본 노드 수 계산
            int nodesForLevel = CalculateNodesForLevel(level);
            
            // 기본 X 위치
            float baseX = level * horizontalSpacing;
            
            // Y 위치 후보들 생성
            List<float> candidateYPositions = GenerateYCandidates(nodesForLevel);
            
            // 각 Y 위치에 대해 겹침 방지 배치
            foreach (float candidateY in candidateYPositions)
            {
                Vector2 finalPosition = FindNonOverlappingPosition(
                    new Vector2(baseX, candidateY), 
                    existingNodes, 
                    positions
                );
                
                if (finalPosition != Vector2.zero) // 유효한 위치를 찾은 경우
                {
                    positions.Add(finalPosition);
                }
                
                // 목표 노드 수에 도달하면 중단
                if (positions.Count >= nodesForLevel)
                    break;
            }
            
            return positions;
        }
        
        /// <summary>
        /// 레벨별 노드 수 계산 (가변적인 분기 구조)
        /// </summary>
        private int CalculateNodesForLevel(int level)
        {
            int totalLevels = roomsPerFloor - 2; // 시작과 보스 제외
            float normalizedLevel = (float)level / totalLevels;
            
            // 중간 부분에서 분기가 많고, 시작과 끝에서는 적음
            float branchFactor = 1.0f - Mathf.Abs(normalizedLevel * 2.0f - 1.0f);
            int nodeCount = Mathf.RoundToInt(branchCount * (0.5f + branchFactor * 0.5f));
            
            // 최소 1개, 최대 branchCount개
            return Mathf.Clamp(nodeCount, 1, branchCount);
        }
        
        /// <summary>
        /// Y 위치 후보들 생성
        /// </summary>
        private List<float> GenerateYCandidates(int nodeCount)
        {
            List<float> candidates = new List<float>();
            
            if (nodeCount == 1)
            {
                // 노드가 1개인 경우 중앙에 배치
                candidates.Add(0f);
            }
            else
            {
                // 노드가 여러 개인 경우 균등 분포 + 약간의 랜덤성
                for (int i = 0; i < nodeCount; i++)
                {
                    // 기본 균등 분포 위치
                    float normalizedPosition = (float)i / (nodeCount - 1);  // 0 ~ 1
                    float baseY = (normalizedPosition - 0.5f) * verticalSpread; // 중앙 기준 분포
                    
                    // 랜덤 오프셋 추가
                    float randomOffset = Random.Range(-randomPositionVariance, randomPositionVariance);
                    float finalY = baseY + randomOffset;
                    
                    candidates.Add(finalY);
                }
            }
            
            return candidates;
        }
        
        /// <summary>
        /// 겹치지 않는 위치 찾기
        /// </summary>
        private Vector2 FindNonOverlappingPosition(Vector2 preferredPosition, List<DungeonNode> existingNodes, List<Vector2> currentLevelPositions)
        {
            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                Vector2 testPosition = preferredPosition;
                
                // 첫 번째 시도가 아닌 경우 위치 조정
                if (attempt > 0)
                {
                    float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float distance = Random.Range(minNodeDistance * 0.5f, minNodeDistance * 1.5f);
                    Vector2 offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                    testPosition = preferredPosition + offset;
                }
                
                // 겹침 검사
                if (IsPositionValid(testPosition, existingNodes, currentLevelPositions))
                {
                    return testPosition;
                }
            }
            
            Debug.LogWarning($"ImprovedDungeonGenerator: Could not find non-overlapping position for {preferredPosition}");
            return Vector2.zero; // 유효한 위치를 찾지 못함
        }
        
        /// <summary>
        /// 위치 유효성 검사 (겹침 방지)
        /// </summary>
        private bool IsPositionValid(Vector2 testPosition, List<DungeonNode> existingNodes, List<Vector2> currentLevelPositions)
        {
            // 기존 노드들과의 거리 검사
            foreach (DungeonNode existingNode in existingNodes)
            {
                float distance = Vector2.Distance(testPosition, existingNode.Position);
                if (distance < minNodeDistance)
                {
                    return false;
                }
            }
            
            // 현재 레벨의 다른 위치들과의 거리 검사
            foreach (Vector2 existingPosition in currentLevelPositions)
            {
                float distance = Vector2.Distance(testPosition, existingPosition);
                if (distance < minNodeDistance)
                {
                    return false;
                }
            }
            
            return true;
        }
        
        /// <summary>
        /// 레벨에 따른 노드 타입 결정
        /// </summary>
        private NodeType GetNodeTypeForLevel(int level)
        {
            // 미니보스는 5-6레벨에 배치
            if (level == 5 || level == 6)
            {
                return NodeType.MiniBoss;
            }
            
            // 나머지는 확률적으로 결정
            return GetRandomNodeType();
        }
        
        /// <summary>
        /// 랜덤 노드 타입 결정
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
        
        /// <summary>
        /// 개선된 노드 연결 알고리즘
        /// </summary>
        private void ConnectNodesImproved(List<DungeonNode> nodes)
        {
            // 레벨별로 노드 분류
            Dictionary<int, List<DungeonNode>> nodesByLevel = new Dictionary<int, List<DungeonNode>>();
            
            foreach (DungeonNode node in nodes)
            {
                int level = Mathf.RoundToInt(node.Position.x / horizontalSpacing);
                
                if (!nodesByLevel.ContainsKey(level))
                    nodesByLevel[level] = new List<DungeonNode>();
                    
                nodesByLevel[level].Add(node);
            }
            
            // 연결 생성
            for (int level = 0; level < roomsPerFloor - 1; level++)
            {
                if (!nodesByLevel.ContainsKey(level) || !nodesByLevel.ContainsKey(level + 1))
                    continue;
                    
                ConnectLevels(nodesByLevel[level], nodesByLevel[level + 1]);
            }
        }
        
        /// <summary>
        /// 두 레벨 간 연결 생성
        /// </summary>
        private void ConnectLevels(List<DungeonNode> currentLevel, List<DungeonNode> nextLevel)
        {
            foreach (DungeonNode currentNode in currentLevel)
            {
                // 거리 기반으로 연결할 다음 레벨 노드들 찾기
                List<DungeonNode> nearestNodes = FindNearestNodes(currentNode, nextLevel, 2);
                
                foreach (DungeonNode nearNode in nearestNodes)
                {
                    if (!currentNode.ConnectedNodes.Contains(nearNode.ID))
                    {
                        currentNode.ConnectedNodes.Add(nearNode.ID);
                        
                        // 접근 가능성 설정
                        if (currentNode.IsAccessible || currentNode.IsVisited)
                        {
                            nearNode.IsAccessible = true;
                        }
                    }
                }
                
                // 연결이 없는 경우 강제로 가장 가까운 노드에 연결
                if (currentNode.ConnectedNodes.Count == 0 && nextLevel.Count > 0)
                {
                    DungeonNode closestNode = FindClosestNode(currentNode, nextLevel);
                    currentNode.ConnectedNodes.Add(closestNode.ID);
                    
                    if (currentNode.IsAccessible || currentNode.IsVisited)
                    {
                        closestNode.IsAccessible = true;
                    }
                }
            }
        }
        
        /// <summary>
        /// 가장 가까운 노드들 찾기
        /// </summary>
        private List<DungeonNode> FindNearestNodes(DungeonNode sourceNode, List<DungeonNode> targetNodes, int count)
        {
            List<DungeonNode> sortedNodes = new List<DungeonNode>(targetNodes);
            sortedNodes.Sort((a, b) => 
                Vector2.Distance(sourceNode.Position, a.Position).CompareTo(
                Vector2.Distance(sourceNode.Position, b.Position)));
            
            return sortedNodes.GetRange(0, Mathf.Min(count, sortedNodes.Count));
        }
        
        /// <summary>
        /// 가장 가까운 노드 찾기
        /// </summary>
        private DungeonNode FindClosestNode(DungeonNode sourceNode, List<DungeonNode> targetNodes)
        {
            DungeonNode closest = targetNodes[0];
            float closestDistance = Vector2.Distance(sourceNode.Position, closest.Position);
            
            for (int i = 1; i < targetNodes.Count; i++)
            {
                float distance = Vector2.Distance(sourceNode.Position, targetNodes[i].Position);
                if (distance < closestDistance)
                {
                    closest = targetNodes[i];
                    closestDistance = distance;
                }
            }
            
            return closest;
        }
    }
}
