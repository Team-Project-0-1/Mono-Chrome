using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Dungeon
{
    /// <summary>
    /// 노드 위치 관리 및 겹침 방지를 담당하는 포트폴리오 품질 클래스
    /// 고급 배치 알고리즘과 충돌 검출을 통해 노드 겹침을 방지합니다.
    /// </summary>
    public class NodePositionManager : MonoBehaviour
    {
        [Header("배치 설정")]
        [SerializeField] private float minNodeDistance = 100f;
        [SerializeField] private float maxAttempts = 50f;
        [SerializeField] private float containerWidth = 800f;
        [SerializeField] private float containerHeight = 600f;
        [SerializeField] private bool useGridBasedLayout = true;
        
        [Header("시각적 설정")]
        [SerializeField] private float nodeSize = 80f;
        [SerializeField] private bool showDebugGizmos = false;
        
        private List<Vector2> occupiedPositions = new List<Vector2>();
        private List<Rect> occupiedAreas = new List<Rect>();
        
        /// <summary>
        /// 노드 위치 초기화
        /// </summary>
        public void Initialize(RectTransform container)
        {
            if (container != null)
            {
                containerWidth = container.rect.width;
                containerHeight = container.rect.height;
            }
            
            ClearOccupiedPositions();
            Debug.Log($"NodePositionManager: Initialized with container size {containerWidth}x{containerHeight}");
        }
        
        /// <summary>
        /// 모든 노드에 대해 겹치지 않는 위치 계산
        /// </summary>
        public List<Vector2> CalculateNodePositions(int nodeCount, bool isLinearProgression = true)
        {
            List<Vector2> positions = new List<Vector2>();
            ClearOccupiedPositions();
            
            if (useGridBasedLayout && !isLinearProgression)
            {
                positions = CalculateGridPositions(nodeCount);
            }
            else
            {
                positions = CalculateLinearProgression(nodeCount);
            }
            
            // 충돌 검출 및 재배치
            positions = ResolveCollisions(positions);
            
            Debug.Log($"NodePositionManager: Calculated {positions.Count} non-overlapping positions");
            return positions;
        }
        
        /// <summary>
        /// 선형 진행 방식 위치 계산 (던전 진행용)
        /// </summary>
        private List<Vector2> CalculateLinearProgression(int nodeCount)
        {
            List<Vector2> positions = new List<Vector2>();
            
            // 3개씩 그룹으로 나누어 층별 배치
            int groupSize = 3;
            int layers = Mathf.CeilToInt((float)nodeCount / groupSize);
            
            float layerHeight = containerHeight / (layers + 1);
            float nodeSpacing = containerWidth / (groupSize + 1);
            
            for (int i = 0; i < nodeCount; i++)
            {
                int layer = i / groupSize;
                int positionInLayer = i % groupSize;
                
                float x = nodeSpacing * (positionInLayer + 1) - containerWidth / 2;
                float y = containerHeight / 2 - layerHeight * (layer + 1);
                
                // 약간의 랜덤 오프셋 추가 (자연스러운 배치)
                x += Random.Range(-20f, 20f);
                y += Random.Range(-10f, 10f);
                
                positions.Add(new Vector2(x, y));
            }
            
            return positions;
        }
        
        /// <summary>
        /// 그리드 기반 위치 계산
        /// </summary>
        private List<Vector2> CalculateGridPositions(int nodeCount)
        {
            List<Vector2> positions = new List<Vector2>();
            
            // 최적의 그리드 크기 계산
            int gridWidth = Mathf.CeilToInt(Mathf.Sqrt(nodeCount));
            int gridHeight = Mathf.CeilToInt((float)nodeCount / gridWidth);
            
            float cellWidth = containerWidth / gridWidth;
            float cellHeight = containerHeight / gridHeight;
            
            for (int i = 0; i < nodeCount; i++)
            {
                int row = i / gridWidth;
                int col = i % gridWidth;
                
                float x = col * cellWidth + cellWidth / 2 - containerWidth / 2;
                float y = containerHeight / 2 - row * cellHeight - cellHeight / 2;
                
                // 셀 내에서 랜덤 오프셋
                x += Random.Range(-cellWidth * 0.2f, cellWidth * 0.2f);
                y += Random.Range(-cellHeight * 0.2f, cellHeight * 0.2f);
                
                positions.Add(new Vector2(x, y));
            }
            
            return positions;
        }
        
        /// <summary>
        /// 충돌 검출 및 해결
        /// </summary>
        private List<Vector2> ResolveCollisions(List<Vector2> positions)
        {
            List<Vector2> resolvedPositions = new List<Vector2>(positions);
            
            for (int i = 0; i < resolvedPositions.Count; i++)
            {
                Vector2 currentPos = resolvedPositions[i];
                bool hasCollision = true;
                int attempts = 0;
                
                while (hasCollision && attempts < maxAttempts)
                {
                    hasCollision = false;
                    
                    // 다른 노드들과의 거리 검사
                    for (int j = 0; j < resolvedPositions.Count; j++)
                    {
                        if (i == j) continue;
                        
                        float distance = Vector2.Distance(currentPos, resolvedPositions[j]);
                        if (distance < minNodeDistance)
                        {
                            // 충돌 발생 - 새 위치 계산
                            currentPos = FindAlternativePosition(currentPos, resolvedPositions, i);
                            hasCollision = true;
                            break;
                        }
                    }
                    
                    attempts++;
                }
                
                resolvedPositions[i] = currentPos;
                
                // 확정된 위치를 점유 목록에 추가
                AddOccupiedPosition(currentPos);
            }
            
            return resolvedPositions;
        }
        
        /// <summary>
        /// 대체 위치 찾기
        /// </summary>
        private Vector2 FindAlternativePosition(Vector2 originalPos, List<Vector2> existingPositions, int excludeIndex)
        {
            // 나선형 탐색으로 가장 가까운 빈 공간 찾기
            float radius = minNodeDistance;
            float angleStep = 30f; // 30도씩 회전
            
            for (int radiusStep = 0; radiusStep < 10; radiusStep++)
            {
                for (float angle = 0; angle < 360; angle += angleStep)
                {
                    float radians = angle * Mathf.Deg2Rad;
                    Vector2 testPos = originalPos + new Vector2(
                        Mathf.Cos(radians) * radius,
                        Mathf.Sin(radians) * radius
                    );
                    
                    // 컨테이너 범위 내 확인
                    if (IsWithinContainer(testPos) && !HasCollisionAt(testPos, existingPositions, excludeIndex))
                    {
                        return testPos;
                    }
                }
                
                radius += minNodeDistance * 0.5f; // 반지름 증가
            }
            
            // 대체 위치를 찾지 못한 경우 랜덤 위치 반환
            return GetRandomSafePosition(existingPositions, excludeIndex);
        }
        
        /// <summary>
        /// 특정 위치에서 충돌 검사
        /// </summary>
        private bool HasCollisionAt(Vector2 position, List<Vector2> existingPositions, int excludeIndex)
        {
            for (int i = 0; i < existingPositions.Count; i++)
            {
                if (i == excludeIndex) continue;
                
                if (Vector2.Distance(position, existingPositions[i]) < minNodeDistance)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 컨테이너 범위 내 확인
        /// </summary>
        private bool IsWithinContainer(Vector2 position)
        {
            float halfWidth = containerWidth / 2;
            float halfHeight = containerHeight / 2;
            
            return position.x >= -halfWidth + nodeSize / 2 && 
                   position.x <= halfWidth - nodeSize / 2 && 
                   position.y >= -halfHeight + nodeSize / 2 && 
                   position.y <= halfHeight - nodeSize / 2;
        }
        
        /// <summary>
        /// 안전한 랜덤 위치 생성
        /// </summary>
        private Vector2 GetRandomSafePosition(List<Vector2> existingPositions, int excludeIndex)
        {
            float halfWidth = containerWidth / 2 - nodeSize / 2;
            float halfHeight = containerHeight / 2 - nodeSize / 2;
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2 randomPos = new Vector2(
                    Random.Range(-halfWidth, halfWidth),
                    Random.Range(-halfHeight, halfHeight)
                );
                
                if (!HasCollisionAt(randomPos, existingPositions, excludeIndex))
                {
                    return randomPos;
                }
            }
            
            // 최후의 수단: 강제 위치
            return new Vector2(
                Random.Range(-halfWidth, halfWidth),
                Random.Range(-halfHeight, halfHeight)
            );
        }
        
        /// <summary>
        /// 점유된 위치 추가
        /// </summary>
        private void AddOccupiedPosition(Vector2 position)
        {
            occupiedPositions.Add(position);
            
            // 점유 영역도 추가 (노드 크기 고려)
            Rect occupiedRect = new Rect(
                position.x - nodeSize / 2,
                position.y - nodeSize / 2,
                nodeSize,
                nodeSize
            );
            occupiedAreas.Add(occupiedRect);
        }
        
        /// <summary>
        /// 점유된 위치 목록 초기화
        /// </summary>
        private void ClearOccupiedPositions()
        {
            occupiedPositions.Clear();
            occupiedAreas.Clear();
        }
        
        /// <summary>
        /// 특정 노드에 최적 위치 할당
        /// </summary>
        public Vector2 GetOptimalPositionForNode(int nodeIndex, int totalNodes, List<Vector2> existingPositions = null)
        {
            if (existingPositions == null)
            {
                existingPositions = new List<Vector2>();
            }
            
            // 노드 타입에 따른 선호 위치 계산
            Vector2 preferredPosition = CalculatePreferredPosition(nodeIndex, totalNodes);
            
            // 충돌 없는 최적 위치 찾기
            if (!HasCollisionAt(preferredPosition, existingPositions, -1))
            {
                return preferredPosition;
            }
            
            return FindAlternativePosition(preferredPosition, existingPositions, -1);
        }
        
        /// <summary>
        /// 노드 인덱스에 따른 선호 위치 계산
        /// </summary>
        private Vector2 CalculatePreferredPosition(int nodeIndex, int totalNodes)
        {
            // 진행 기반 배치
            float progress = (float)nodeIndex / (totalNodes - 1);
            
            // 레이어 계산
            int nodesPerLayer = 3;
            int layer = nodeIndex / nodesPerLayer;
            int positionInLayer = nodeIndex % nodesPerLayer;
            
            float layerProgress = containerHeight * 0.8f / Mathf.Max(1, (totalNodes - 1) / nodesPerLayer);
            float x = (positionInLayer - 1) * (containerWidth / 4);
            float y = containerHeight / 2 - layer * layerProgress - 50f;
            
            return new Vector2(x, y);
        }
        
        #if UNITY_EDITOR
        /// <summary>
        /// 에디터용 시각적 디버깅
        /// </summary>
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;
            
            // 컨테이너 영역 표시
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(containerWidth, containerHeight, 0));
            
            // 점유된 위치들 표시
            Gizmos.color = Color.red;
            foreach (Vector2 pos in occupiedPositions)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)pos, nodeSize / 2);
            }
            
            // 최소 거리 표시
            Gizmos.color = Color.yellow;
            foreach (Vector2 pos in occupiedPositions)
            {
                Gizmos.DrawWireSphere(transform.position + (Vector3)pos, minNodeDistance / 2);
            }
        }
        #endif
    }
}
