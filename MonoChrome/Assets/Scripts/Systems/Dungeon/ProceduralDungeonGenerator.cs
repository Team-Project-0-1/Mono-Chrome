using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 개선된 절차적 던전 생성기 v2.0
    /// StageThemeDataAsset과 연동하여 완전한 문서 요구사항 구현
    /// - 스테이지별 테마 시스템
    /// - 감각 기반 조건부 필터링  
    /// - 매 턴 3개 선택지 생성
    /// - 이벤트 연동 시스템
    /// - 디자이너 친화적 데이터 관리
    /// </summary>
    public class ProceduralDungeonGenerator : MonoBehaviour
    {
        #region Configuration
        [Header("== 기본 던전 설정 ==")]
        [SerializeField] private int defaultTotalTurns = 15;
        [SerializeField] private int defaultBranchesPerTurn = 3;
        
        [Header("== 스테이지 테마 데이터 ==")]
        [SerializeField] private StageThemeDataAsset[] stageThemes;
        [SerializeField] private StageThemeDataAsset fallbackTheme;
        
        [Header("== 레이아웃 설정 ==")]
        [SerializeField] private float turnSpacing = 180f;
        [SerializeField] private float branchSpacing = 120f;
        [SerializeField] private float verticalSpread = 400f;
        [SerializeField] private float minNodeDistance = 90f;
        
        [Header("== 감각 시스템 ==")]
        [SerializeField] private bool enableSenseBasedHints = true;
        [SerializeField] private SenseType currentPlayerSense = SenseType.None;
        
        [Header("== 디버그 ==")]
        [SerializeField] private bool enableDebugLogging = true;
        [SerializeField] private bool showGenerationStats = true;
        
        // 내부 데이터
        private int _currentStage = 0;
        private StageThemeDataAsset _currentTheme;
        private Dictionary<int, List<DungeonNode>> _nodesByTurn = new Dictionary<int, List<DungeonNode>>();
        private List<Vector2> _occupiedPositions = new List<Vector2>();
        private GenerationStats _stats = new GenerationStats();
        #endregion
        
        #region Data Structures
        [System.Serializable]
        public class GenerationStats
        {
            public int totalNodesGenerated;
            public int combatNodes;
            public int eventNodes;
            public int shopNodes;
            public int restNodes;
            public int miniBossNodes;
            public int bossNodes;
            public float generationTime;
            public string lastError;
            
            public void Reset()
            {
                totalNodesGenerated = combatNodes = eventNodes = shopNodes = restNodes = miniBossNodes = bossNodes = 0;
                generationTime = 0f;
                lastError = "";
            }
            
            public void CountNode(NodeType nodeType)
            {
                totalNodesGenerated++;
                switch (nodeType)
                {
                    case NodeType.Combat: combatNodes++; break;
                    case NodeType.Event: eventNodes++; break;
                    case NodeType.Shop: shopNodes++; break;
                    case NodeType.Rest: restNodes++; break;
                    case NodeType.MiniBoss: miniBossNodes++; break;
                    case NodeType.Boss: bossNodes++; break;
                }
            }
        }
        
        [System.Serializable]
        public class TurnChoiceData
        {
            public int turnIndex;
            public List<DungeonNode> choices = new List<DungeonNode>();
            public bool hasMiniBoss;
            public bool isFinalTurn;
            public List<string> senseHints = new List<string>(); // 감각 기반 힌트
        }
        
        [System.Serializable]
        public class MandatoryPlacement
        {
            public int turnIndex;
            public NodeType nodeType;
            public int priority; // 높을수록 우선
            public string reason;
        }
        #endregion
        
        #region Public Interface
        /// <summary>
        /// 절차적 던전 생성 (메인 엔트리 포인트)
        /// </summary>
        public List<DungeonNode> GenerateProceduralDungeon(int stageIndex = 0)
        {
            float startTime = Time.realtimeSinceStartup;
            _stats.Reset();
            
            LogDebug($"=== 절차적 던전 생성 시작 (스테이지 {stageIndex + 1}) ===");
            
            // 초기화
            if (!InitializeGeneration(stageIndex))
            {
                return GenerateFallbackDungeon();
            }
            
            // 턴별 선택지 생성
            List<TurnChoiceData> turnChoices = GenerateTurnChoices();
            if (turnChoices == null || turnChoices.Count == 0)
            {
                _stats.lastError = "턴 선택지 생성 실패";
                return GenerateFallbackDungeon();
            }
            
            // 노드 변환 및 연결
            List<DungeonNode> finalNodes = ConvertToFinalNodeStructure(turnChoices);
            
            // 감각 기반 힌트 적용
            if (enableSenseBasedHints)
            {
                ApplySenseBasedHints(finalNodes, turnChoices);
            }
            
            // 최종 검증 및 통계
            ValidateDungeonStructure(finalNodes);
            
            _stats.generationTime = Time.realtimeSinceStartup - startTime;
            LogGenerationStats();
            
            LogDebug($"=== 던전 생성 완료 - {finalNodes.Count}개 노드, {_stats.generationTime:F3}초 소요 ===");
            return finalNodes;
        }
        
        /// <summary>
        /// 플레이어 감각 설정
        /// </summary>
        public void SetPlayerSense(SenseType senseType)
        {
            currentPlayerSense = senseType;
            LogDebug($"플레이어 감각 설정: {senseType}");
        }
        
        /// <summary>
        /// 특정 턴의 선택지 가져오기
        /// </summary>
        public List<DungeonNode> GetTurnChoices(int turnIndex)
        {
            return _nodesByTurn.ContainsKey(turnIndex) ? _nodesByTurn[turnIndex] : new List<DungeonNode>();
        }
        
        /// <summary>
        /// 현재 스테이지 테마 정보 가져오기
        /// </summary>
        public StageThemeDataAsset GetCurrentTheme()
        {
            return _currentTheme;
        }
        
        /// <summary>
        /// 생성 통계 가져오기
        /// </summary>
        public GenerationStats GetGenerationStats()
        {
            return _stats;
        }
        #endregion
        
        #region Generation Process
        /// <summary>
        /// 생성 초기화
        /// </summary>
        private bool InitializeGeneration(int stageIndex)
        {
            _currentStage = stageIndex;
            _nodesByTurn.Clear();
            _occupiedPositions.Clear();
            
            // 스테이지 테마 설정
            _currentTheme = GetStageTheme(stageIndex);
            if (_currentTheme == null)
            {
                Debug.LogError($"ProceduralDungeonGenerator: 스테이지 {stageIndex}에 대한 테마를 찾을 수 없음");
                return false;
            }
            
            // 테마 데이터 검증
            if (!_currentTheme.ValidateData())
            {
                Debug.LogWarning($"ProceduralDungeonGenerator: 스테이지 테마 '{_currentTheme.name}' 데이터에 문제가 있음");
            }
            
            LogDebug($"스테이지 테마 로드됨: {_currentTheme.StageDisplayName}");
            LogDebug($"  - 설명: {_currentTheme.StageDescription}");
            LogDebug($"  - 턴 수: {_currentTheme.TotalTurns}, 분기: {_currentTheme.BranchesPerTurn}");
            
            return true;
        }
        
        /// <summary>
        /// 스테이지 테마 가져오기
        /// </summary>
        private StageThemeDataAsset GetStageTheme(int stageIndex)
        {
            if (stageThemes != null && stageIndex < stageThemes.Length && stageThemes[stageIndex] != null)
            {
                return stageThemes[stageIndex];
            }
            
            if (fallbackTheme != null)
            {
                LogDebug($"폴백 테마 사용: {fallbackTheme.name}");
                return fallbackTheme;
            }
            
            return null;
        }
        
        /// <summary>
        /// 턴별 선택지 생성
        /// </summary>
        private List<TurnChoiceData> GenerateTurnChoices()
        {
            List<TurnChoiceData> turnChoices = new List<TurnChoiceData>();
            
            // 필수 배치 계획
            List<MandatoryPlacement> mandatoryPlacements = PlanMandatoryPlacements();
            LogDebug($"필수 배치 계획: {mandatoryPlacements.Count}개 항목");
            
            for (int turn = 0; turn < _currentTheme.TotalTurns; turn++)
            {
                TurnChoiceData turnData = GenerateTurnChoice(turn, mandatoryPlacements);
                if (turnData == null)
                {
                    _stats.lastError = $"턴 {turn} 생성 실패";
                    return null;
                }
                
                turnChoices.Add(turnData);
                _nodesByTurn[turn] = turnData.choices;
                
                LogDebug($"턴 {turn + 1}: {turnData.choices.Count}개 선택지 생성");
            }
            
            return turnChoices;
        }
        
        /// <summary>
        /// 필수 배치 계획
        /// </summary>
        private List<MandatoryPlacement> PlanMandatoryPlacements()
        {
            List<MandatoryPlacement> placements = new List<MandatoryPlacement>();
            
            // 시작점 (항상 전투)
            placements.Add(new MandatoryPlacement 
            { 
                turnIndex = 0, 
                nodeType = NodeType.Combat, 
                priority = 100, 
                reason = "시작점"
            });
            
            // 미니보스
            int miniBossTurn = _currentTheme.MiniBossPosition - 1; // 0-based
            if (miniBossTurn > 0 && miniBossTurn < _currentTheme.TotalTurns - 1)
            {
                placements.Add(new MandatoryPlacement 
                { 
                    turnIndex = miniBossTurn, 
                    nodeType = NodeType.MiniBoss, 
                    priority = 90, 
                    reason = "미니보스"
                });
                
                // 미니보스 전 상점 (옵션)
                if (_currentTheme.ForceShopBeforeMiniBoss && miniBossTurn > 1)
                {
                    placements.Add(new MandatoryPlacement 
                    { 
                        turnIndex = miniBossTurn - 1, 
                        nodeType = NodeType.Shop, 
                        priority = 70, 
                        reason = "미니보스 전 상점"
                    });
                }
                
                // 미니보스 후 휴식 (옵션)
                if (_currentTheme.ForceRestAfterMiniBoss && miniBossTurn < _currentTheme.TotalTurns - 2)
                {
                    placements.Add(new MandatoryPlacement 
                    { 
                        turnIndex = miniBossTurn + 1, 
                        nodeType = NodeType.Rest, 
                        priority = 70, 
                        reason = "미니보스 후 휴식"
                    });
                }
            }
            
            // 보스 (마지막 턴)
            int finalTurn = _currentTheme.TotalTurns - 1;
            placements.Add(new MandatoryPlacement 
            { 
                turnIndex = finalTurn, 
                nodeType = NodeType.Boss, 
                priority = 100, 
                reason = "최종 보스"
            });
            
            // 보장된 상점과 휴식소 배치
            AddGuaranteedPlacements(placements, NodeType.Shop, _currentTheme.GuaranteedShops, "보장된 상점");
            AddGuaranteedPlacements(placements, NodeType.Rest, _currentTheme.GuaranteedRests, "보장된 휴식");
            
            // 우선순위 정렬
            placements.Sort((a, b) => b.priority.CompareTo(a.priority));
            
            return placements;
        }
        
        /// <summary>
        /// 보장된 배치 추가
        /// </summary>
        private void AddGuaranteedPlacements(List<MandatoryPlacement> placements, NodeType nodeType, int count, string reason)
        {
            List<int> availableTurns = GetAvailableTurns(placements);
            
            for (int i = 0; i < count && availableTurns.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableTurns.Count);
                int selectedTurn = availableTurns[randomIndex];
                availableTurns.RemoveAt(randomIndex);
                
                placements.Add(new MandatoryPlacement 
                { 
                    turnIndex = selectedTurn, 
                    nodeType = nodeType, 
                    priority = 60, 
                    reason = reason
                });
            }
        }
        
        /// <summary>
        /// 사용 가능한 턴 목록 가져오기
        /// </summary>
        private List<int> GetAvailableTurns(List<MandatoryPlacement> existingPlacements)
        {
            List<int> available = new List<int>();
            
            for (int turn = 1; turn < _currentTheme.TotalTurns - 1; turn++) // 시작과 끝 제외
            {
                bool isOccupied = existingPlacements.Any(p => p.turnIndex == turn);
                if (!isOccupied)
                {
                    available.Add(turn);
                }
            }
            
            return available;
        }
        
        /// <summary>
        /// 개별 턴 선택지 생성
        /// </summary>
        private TurnChoiceData GenerateTurnChoice(int turnIndex, List<MandatoryPlacement> mandatoryPlacements)
        {
            TurnChoiceData turnData = new TurnChoiceData
            {
                turnIndex = turnIndex,
                hasMiniBoss = turnIndex == _currentTheme.MiniBossPosition - 1,
                isFinalTurn = turnIndex == _currentTheme.TotalTurns - 1
            };
            
            // 이 턴의 필수 배치들 가져오기
            List<MandatoryPlacement> turnMandatory = mandatoryPlacements
                .Where(p => p.turnIndex == turnIndex)
                .OrderByDescending(p => p.priority)
                .ToList();
            
            // 선택지 생성
            for (int branch = 0; branch < _currentTheme.BranchesPerTurn; branch++)
            {
                Vector2 position = CalculateNodePosition(turnIndex, branch);
                NodeType nodeType;
                
                // 필수 타입이 있다면 우선 배치
                if (turnMandatory.Count > 0)
                {
                    nodeType = turnMandatory[0].nodeType;
                    LogDebug($"  턴 {turnIndex}, 분기 {branch}: {nodeType} (필수 - {turnMandatory[0].reason})");
                    turnMandatory.RemoveAt(0);
                }
                else
                {
                    nodeType = GetRandomNodeTypeForTheme();
                    LogDebug($"  턴 {turnIndex}, 분기 {branch}: {nodeType} (랜덤)");
                }
                
                DungeonNode node = CreateNode(turnIndex * _currentTheme.BranchesPerTurn + branch, nodeType, position);
                
                // 시작 노드는 접근 가능
                if (turnIndex == 0 && branch == 0)
                {
                    node.IsAccessible = true;
                }
                
                turnData.choices.Add(node);
                _occupiedPositions.Add(position);
                _stats.CountNode(nodeType);
            }
            
            return turnData;
        }
        
        /// <summary>
        /// 노드 위치 계산
        /// </summary>
        private Vector2 CalculateNodePosition(int turnIndex, int branchIndex)
        {
            float baseX = turnIndex * turnSpacing;
            float baseY = 0f;
            
            // 분기 배치
            if (_currentTheme.BranchesPerTurn > 1)
            {
                float normalizedBranch = (float)branchIndex / (_currentTheme.BranchesPerTurn - 1);
                baseY = (normalizedBranch - 0.5f) * verticalSpread;
            }
            
            // 랜덤 오프셋
            Vector2 randomOffset = new Vector2(
                Random.Range(-30f, 30f),
                Random.Range(-40f, 40f)
            );
            
            Vector2 finalPosition = new Vector2(baseX, baseY) + randomOffset;
            return EnsureNonOverlappingPosition(finalPosition);
        }
        
        /// <summary>
        /// 겹침 방지 위치 조정
        /// </summary>
        private Vector2 EnsureNonOverlappingPosition(Vector2 preferredPosition)
        {
            Vector2 testPosition = preferredPosition;
            int attempts = 0;
            int maxAttempts = 50;
            
            while (attempts < maxAttempts)
            {
                bool hasOverlap = false;
                
                foreach (Vector2 occupied in _occupiedPositions)
                {
                    if (Vector2.Distance(testPosition, occupied) < minNodeDistance)
                    {
                        hasOverlap = true;
                        break;
                    }
                }
                
                if (!hasOverlap) return testPosition;
                
                // 위치 조정
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float distance = minNodeDistance + (attempts * 10f);
                Vector2 offset = new Vector2(Mathf.Cos(angle) * distance, Mathf.Sin(angle) * distance);
                testPosition = preferredPosition + offset;
                
                attempts++;
            }
            
            LogDebug($"경고: 겹침 방지 실패 - {preferredPosition}");
            return testPosition;
        }
        
        /// <summary>
        /// 노드 생성
        /// </summary>
        private DungeonNode CreateNode(int nodeId, NodeType nodeType, Vector2 position)
        {
            DungeonNode node = new DungeonNode(nodeId, nodeType, position);
            
            // 스테이지 테마 기반 특수 데이터 설정
            switch (nodeType)
            {
                case NodeType.Combat:
                    var enemyData = _currentTheme.GetRandomEnemy(_currentStage + 1);
                    if (enemyData != null)
                    {
                        node.EnemyID = System.Array.IndexOf(_currentTheme.EnemyPool, enemyData);
                    }
                    break;
                    
                case NodeType.Event:
                    var eventData = _currentTheme.GetRandomEvent(currentPlayerSense);
                    if (eventData != null)
                    {
                        node.EventID = eventData.eventID;
                    }
                    break;
                    
                case NodeType.MiniBoss:
                    node.EnemyID = 100; // 미니보스 고정 ID
                    break;
                    
                case NodeType.Boss:
                    node.EnemyID = 200; // 보스 고정 ID
                    break;
            }
            
            return node;
        }
        
        /// <summary>
        /// 테마 기반 랜덤 노드 타입
        /// </summary>
        private NodeType GetRandomNodeTypeForTheme()
        {
            float random = Random.value;
            float cumulative = 0f;
            
            cumulative += _currentTheme.CombatChance;
            if (random < cumulative) return NodeType.Combat;
            
            cumulative += _currentTheme.EventChance;
            if (random < cumulative) return NodeType.Event;
            
            cumulative += _currentTheme.ShopChance;
            if (random < cumulative) return NodeType.Shop;
            
            return NodeType.Rest;
        }
        #endregion
        
        #region Advanced Features
        /// <summary>
        /// 감각 기반 힌트 적용
        /// </summary>
        private void ApplySenseBasedHints(List<DungeonNode> nodes, List<TurnChoiceData> turnChoices)
        {
            if (_currentTheme.SenseFilters == null || currentPlayerSense == SenseType.None) return;
            
            LogDebug("감각 기반 힌트 적용 중...");
            
            foreach (var filter in _currentTheme.SenseFilters)
            {
                if (filter.targetSense == currentPlayerSense)
                {
                    ApplySenseFilter(nodes, filter, turnChoices);
                }
            }
        }
        
        /// <summary>
        /// 개별 감각 필터 적용
        /// </summary>
        private void ApplySenseFilter(List<DungeonNode> nodes, StageThemeDataAsset.SenseFilterConfig filter, List<TurnChoiceData> turnChoices)
        {
            foreach (DungeonNode node in nodes)
            {
                if (System.Array.Exists(filter.applicableNodeTypes, t => t == node.Type))
                {
                    // 해당 턴의 힌트 추가
                    int turnIndex = node.ID / _currentTheme.BranchesPerTurn;
                    if (turnIndex < turnChoices.Count)
                    {
                        turnChoices[turnIndex].senseHints.Add(filter.hintText);
                    }
                    
                    LogDebug($"노드 {node.ID}에 {filter.targetSense} 힌트 적용: {filter.hintText}");
                }
            }
        }
        
        /// <summary>
        /// 최종 노드 구조 변환
        /// </summary>
        private List<DungeonNode> ConvertToFinalNodeStructure(List<TurnChoiceData> turnChoices)
        {
            List<DungeonNode> finalNodes = new List<DungeonNode>();
            
            // 모든 선택지를 단일 리스트로 변환
            foreach (TurnChoiceData turnData in turnChoices)
            {
                finalNodes.AddRange(turnData.choices);
            }
            
            // 연결 생성 (턴 기반)
            ConnectTurnBasedNodes(turnChoices);
            
            return finalNodes;
        }
        
        /// <summary>
        /// 턴 기반 노드 연결
        /// </summary>
        private void ConnectTurnBasedNodes(List<TurnChoiceData> turnChoices)
        {
            for (int turn = 0; turn < turnChoices.Count - 1; turn++)
            {
                List<DungeonNode> currentTurn = turnChoices[turn].choices;
                List<DungeonNode> nextTurn = turnChoices[turn + 1].choices;
                
                // 각 현재 턴 노드를 다음 턴의 모든 노드와 연결
                foreach (DungeonNode currentNode in currentTurn)
                {
                    foreach (DungeonNode nextNode in nextTurn)
                    {
                        if (!currentNode.ConnectedNodes.Contains(nextNode.ID))
                        {
                            currentNode.ConnectedNodes.Add(nextNode.ID);
                        }
                    }
                }
            }
        }
        #endregion
        
        #region Validation & Fallback
        /// <summary>
        /// 던전 구조 검증
        /// </summary>
        private void ValidateDungeonStructure(List<DungeonNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
            {
                _stats.lastError = "생성된 노드가 없음";
                return;
            }
            
            LogDebug("=== 던전 구조 검증 ===");
            LogDebug($"전투: {_stats.combatNodes}, 이벤트: {_stats.eventNodes}, 상점: {_stats.shopNodes}");
            LogDebug($"휴식: {_stats.restNodes}, 미니보스: {_stats.miniBossNodes}, 보스: {_stats.bossNodes}");
            LogDebug($"총 노드: {_stats.totalNodesGenerated}");
            
            // 필수 요소 검증
            if (_stats.miniBossNodes < 1)
            {
                Debug.LogWarning("ProceduralDungeonGenerator: 미니보스가 없습니다!");
            }
            
            if (_stats.bossNodes < 1)
            {
                Debug.LogWarning("ProceduralDungeonGenerator: 보스가 없습니다!");
            }
            
            if (_stats.shopNodes < _currentTheme.GuaranteedShops)
            {
                Debug.LogWarning($"ProceduralDungeonGenerator: 보장된 상점 수가 부족합니다! (현재: {_stats.shopNodes}, 필요: {_currentTheme.GuaranteedShops})");
            }
            
            LogDebug("===================");
        }
        
        /// <summary>
        /// 폴백 던전 생성
        /// </summary>
        private List<DungeonNode> GenerateFallbackDungeon()
        {
            LogDebug("폴백 던전 생성 중...");
            
            List<DungeonNode> nodes = new List<DungeonNode>();
            int nodeId = 0;
            
            // 시작 노드
            DungeonNode startNode = new DungeonNode(nodeId++, NodeType.Combat, Vector2.zero);
            startNode.IsAccessible = true;
            nodes.Add(startNode);
            
            // 중간 노드들 (3개씩)
            for (int turn = 1; turn < defaultTotalTurns - 1; turn++)
            {
                for (int branch = 0; branch < defaultBranchesPerTurn; branch++)
                {
                    NodeType nodeType = (turn == 5) ? NodeType.MiniBoss : NodeType.Combat;
                    Vector2 position = new Vector2(turn * turnSpacing, (branch - 1) * branchSpacing);
                    
                    DungeonNode node = new DungeonNode(nodeId++, nodeType, position);
                    nodes.Add(node);
                }
            }
            
            // 보스 노드
            for (int branch = 0; branch < defaultBranchesPerTurn; branch++)
            {
                Vector2 position = new Vector2((defaultTotalTurns - 1) * turnSpacing, (branch - 1) * branchSpacing);
                DungeonNode bossNode = new DungeonNode(nodeId++, NodeType.Boss, position);
                nodes.Add(bossNode);
            }
            
            // 간단한 연결
            for (int i = 0; i < nodes.Count - defaultBranchesPerTurn; i++)
            {
                for (int j = 0; j < defaultBranchesPerTurn; j++)
                {
                    int nextIndex = i + defaultBranchesPerTurn + j;
                    if (nextIndex < nodes.Count)
                    {
                        nodes[i].ConnectedNodes.Add(nodes[nextIndex].ID);
                    }
                }
            }
            
            _stats.lastError = "폴백 던전 생성됨";
            return nodes;
        }
        
        /// <summary>
        /// 생성 통계 로그
        /// </summary>
        private void LogGenerationStats()
        {
            if (!showGenerationStats) return;
            
            LogDebug("=== 생성 통계 ===");
            LogDebug($"생성 시간: {_stats.generationTime:F3}초");
            LogDebug($"총 노드: {_stats.totalNodesGenerated}개");
            LogDebug($"분포 - 전투:{_stats.combatNodes} 이벤트:{_stats.eventNodes} 상점:{_stats.shopNodes} 휴식:{_stats.restNodes}");
            LogDebug($"특수 - 미니보스:{_stats.miniBossNodes} 보스:{_stats.bossNodes}");
            if (!string.IsNullOrEmpty(_stats.lastError))
            {
                LogDebug($"오류: {_stats.lastError}");
            }
            LogDebug("==============");
        }
        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (enableDebugLogging)
            {
                Debug.Log($"[ProcDungeonGen] {message}");
            }
        }
        #endregion
    }
}