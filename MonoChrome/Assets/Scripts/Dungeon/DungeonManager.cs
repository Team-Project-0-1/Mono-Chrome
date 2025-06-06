using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MonoChrome.Dungeon
{
    /// <summary>
    /// 던전 생성 및 관리를 담당하는 클래스
    /// 포트폴리오급 던전 시스템으로 3가지 생성기를 지원
    /// - 기본 생성기 (Legacy)
    /// - 개선된 생성기 (Improved) 
    /// - 고급 생성기 (Advanced)
    /// - 절차적 생성기 (Procedural) - 최신 문서 요구사항 완전 구현
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
        
        #region Configuration
        [Header("== 던전 기본 설정 ==")]
        [SerializeField] private int _roomsPerFloor = 15; // 층별 방 개수
        [SerializeField] private int _floorCount = 3; // 총 층 수
        [SerializeField] private int _branchCount = 3; // 각 지점별 분기 수
        
        [Header("== 생성기 선택 ==")]
        [SerializeField] private DungeonGeneratorType _generatorType = DungeonGeneratorType.Procedural;
        [SerializeField] private bool _enableDebugOutput = true;
        
        [Header("== 방 타입 확률 (Legacy용) ==")]
        [Range(0, 1)] [SerializeField] private float _combatRoomChance = 0.5f;
        [Range(0, 1)] [SerializeField] private float _eventRoomChance = 0.2f;
        [Range(0, 1)] [SerializeField] private float _shopRoomChance = 0.15f;
        [Range(0, 1)] [SerializeField] private float _restRoomChance = 0.15f;
    
        [Header("== 생성기 컴포넌트 ==")]
        [SerializeField] private ImprovedDungeonGenerator _improvedGenerator;
        [SerializeField] private AdvancedDungeonGenerator _advancedGenerator;
        [SerializeField] private ProceduralDungeonGenerator _proceduralGenerator;
        
        [Header("== 현재 던전 상태 ==")]
        [SerializeField] private int _currentStage = 0;
        [SerializeField] private int _currentNodeIndex = -1;
        
        // 런타임 데이터
        private List<DungeonNode> _dungeonNodes = new List<DungeonNode>();
        private DungeonNode _currentNode;
        
        // 시스템 참조
        private GameManager _gameManager;
        private UIManager _uiManager;
        private DungeonUI _dungeonUI;
        #endregion
        
        #region Enums
        /// <summary>
        /// 던전 생성기 타입
        /// </summary>
        public enum DungeonGeneratorType
        {
            Legacy,      // 기존 방식
            Improved,    // 개선된 방식 (겹침 방지)
            Advanced,    // 고급 방식 (물리 기반)
            Procedural   // 절차적 방식 (문서 요구사항 완전 구현)
        }
        #endregion
        
        #region Initialization
        private void Start()
        {
            InitializeManagers();
            InitializeGenerators();
            StartCoroutine(FindDungeonUIWithDelay());
        }
        
        /// <summary>
        /// 매니저 참조 초기화
        /// </summary>
        private void InitializeManagers()
        {
            _gameManager = GameManager.Instance;
            
            if (_gameManager != null)
            {
                _uiManager = _gameManager.UIManager;
                LogDebug("DungeonManager: 게임 매니저 참조 완료");
            }
            else
            {
                Debug.LogError("DungeonManager: GameManager instance not found");
            }
        }
        
        /// <summary>
        /// 던전 생성기들 초기화
        /// </summary>
        private void InitializeGenerators()
        {
            // 생성기 컴포넌트들 자동 생성
            if (_improvedGenerator == null)
            {
                _improvedGenerator = GetComponent<ImprovedDungeonGenerator>();
                if (_improvedGenerator == null)
                {
                    _improvedGenerator = gameObject.AddComponent<ImprovedDungeonGenerator>();
                }
            }
            
            if (_advancedGenerator == null)
            {
                _advancedGenerator = GetComponent<AdvancedDungeonGenerator>();
                if (_advancedGenerator == null)
                {
                    _advancedGenerator = gameObject.AddComponent<AdvancedDungeonGenerator>();
                }
            }
            
            if (_proceduralGenerator == null)
            {
                _proceduralGenerator = GetComponent<ProceduralDungeonGenerator>();
                if (_proceduralGenerator == null)
                {
                    _proceduralGenerator = gameObject.AddComponent<ProceduralDungeonGenerator>();
                }
            }
            
            LogDebug("DungeonManager: 모든 생성기 초기화 완료");
        }
        
        /// <summary>
        /// DungeonUI 찾기 (딜레이 적용)
        /// </summary>
        private IEnumerator FindDungeonUIWithDelay()
        {
            yield return null; // UI 초기화 대기
            
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform dungeonPanelTransform = canvas.transform.Find("DungeonPanel");
                
                if (dungeonPanelTransform != null)
                {
                    _dungeonUI = dungeonPanelTransform.GetComponent<DungeonUI>();
                    if (_dungeonUI == null)
                    {
                        _dungeonUI = dungeonPanelTransform.gameObject.AddComponent<DungeonUI>();
                        LogDebug("DungeonManager: DungeonUI 컴포넌트 추가됨");
                    }
                    else
                    {
                        LogDebug("DungeonManager: DungeonUI 컴포넌트 발견됨");
                    }
                }
                else
                {
                    // DungeonPanel 생성
                    GameObject dungeonPanel = CreateDungeonPanel(canvas);
                    _dungeonUI = dungeonPanel.AddComponent<DungeonUI>();
                    LogDebug("DungeonManager: 새 DungeonPanel 생성됨");
                    
                    yield return null; // 초기화 대기
                }
            }
            else
            {
                Debug.LogError("DungeonManager: Canvas를 찾을 수 없음");
            }
            
            // 폴백: 전체 씬에서 DungeonUI 찾기
            if (_dungeonUI == null)
            {
                _dungeonUI = FindObjectOfType<DungeonUI>();
                if (_dungeonUI != null)
                {
                    LogDebug("DungeonManager: 씬에서 DungeonUI 발견됨");
                }
                else
                {
                    Debug.LogWarning("DungeonManager: DungeonUI를 찾을 수 없음");
                }
            }
        }
        
        /// <summary>
        /// DungeonPanel 생성
        /// </summary>
        private GameObject CreateDungeonPanel(Canvas canvas)
        {
            GameObject dungeonPanel = new GameObject("DungeonPanel");
            dungeonPanel.transform.SetParent(canvas.transform, false);
            
            RectTransform rectTransform = dungeonPanel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            return dungeonPanel;
        }
        #endregion
        
        #region Public Interface
        /// <summary>
        /// 새 던전 생성 (메인 인터페이스)
        /// </summary>
        public void GenerateNewDungeon(int stageIndex = 0)
        {
            LogDebug($"DungeonManager: 새 던전 생성 시작 (스테이지 {stageIndex + 1}, 생성기: {_generatorType})");
            
            // 초기화
            ClearCurrentDungeon();
            _currentStage = stageIndex;
            
            // 선택된 생성기로 던전 생성
            _dungeonNodes = GenerateDungeonWithSelectedGenerator(stageIndex);
            
            // 생성 결과 검증
            ValidateGeneratedDungeon();
            
            // UI 업데이트
            UpdateDungeonUI();
            
            LogDebug($"DungeonManager: 던전 생성 완료 - {_dungeonNodes.Count}개 노드");
        }
        
        /// <summary>
        /// 특정 노드로 이동
        /// </summary>
        public void MoveToNode(int nodeIndex)
        {
            if (!IsValidNodeIndex(nodeIndex))
            {
                Debug.LogError($"DungeonManager: 잘못된 노드 인덱스: {nodeIndex}");
                return;
            }
            
            DungeonNode targetNode = _dungeonNodes[nodeIndex];
            
            if (!CanMoveToNode(targetNode))
            {
                Debug.LogWarning($"DungeonManager: 노드 {nodeIndex}로 이동 불가 - 접근 불가능");
                return;
            }
            
            // 노드 이동 실행
            ExecuteNodeMovement(nodeIndex, targetNode);
        }
        
        /// <summary>
        /// 방 활동 완료 처리
        /// </summary>
        public void OnRoomCompleted()
        {
            LogDebug("DungeonManager: 방 활동 완료");
            
            // 게임 상태를 던전으로 변경
            if (_gameManager != null)
            {
                _gameManager.ChangeState(GameManager.GameState.Dungeon);
            }
            
            // 방 선택 패널 활성화
            if (_dungeonUI != null)
            {
                _dungeonUI.ShowRoomSelectionPanel();
            }
            
            UpdateDungeonUI();
        }
        
        /// <summary>
        /// 생성기 타입 변경
        /// </summary>
        public void SetGeneratorType(DungeonGeneratorType newType)
        {
            _generatorType = newType;
            LogDebug($"DungeonManager: 생성기 타입 변경됨 - {newType}");
        }
        #endregion
        
        #region Dungeon Generation
        /// <summary>
        /// 현재 던전 정리
        /// </summary>
        private void ClearCurrentDungeon()
        {
            _dungeonNodes.Clear();
            _currentNodeIndex = -1;
            _currentNode = null;
        }
        
        /// <summary>
        /// 선택된 생성기로 던전 생성
        /// </summary>
        private List<DungeonNode> GenerateDungeonWithSelectedGenerator(int stageIndex)
        {
            switch (_generatorType)
            {
                case DungeonGeneratorType.Procedural:
                    return GenerateProceduralDungeon(stageIndex);
                    
                case DungeonGeneratorType.Advanced:
                    return GenerateAdvancedDungeon();
                    
                case DungeonGeneratorType.Improved:
                    return GenerateImprovedDungeon();
                    
                case DungeonGeneratorType.Legacy:
                default:
                    return GenerateLegacyDungeon();
            }
        }
        
        /// <summary>
        /// 절차적 던전 생성 (최신 문서 요구사항)
        /// </summary>
        private List<DungeonNode> GenerateProceduralDungeon(int stageIndex)
        {
            if (_proceduralGenerator == null)
            {
                Debug.LogError("DungeonManager: ProceduralDungeonGenerator가 없음");
                return GenerateLegacyDungeon(); // 폴백
            }
            
            LogDebug("DungeonManager: 절차적 생성기 사용");
            return _proceduralGenerator.GenerateProceduralDungeon(stageIndex);
        }
        
        /// <summary>
        /// 고급 던전 생성
        /// </summary>
        private List<DungeonNode> GenerateAdvancedDungeon()
        {
            if (_advancedGenerator == null)
            {
                Debug.LogError("DungeonManager: AdvancedDungeonGenerator가 없음");
                return GenerateLegacyDungeon(); // 폴백
            }
            
            LogDebug("DungeonManager: 고급 생성기 사용");
            return _advancedGenerator.GenerateAdvancedDungeon();
        }
        
        /// <summary>
        /// 개선된 던전 생성
        /// </summary>
        private List<DungeonNode> GenerateImprovedDungeon()
        {
            if (_improvedGenerator == null)
            {
                Debug.LogError("DungeonManager: ImprovedDungeonGenerator가 없음");
                return GenerateLegacyDungeon(); // 폴백
            }
            
            LogDebug("DungeonManager: 개선된 생성기 사용");
            return _improvedGenerator.GenerateImprovedDungeon();
        }
        
        /// <summary>
        /// 레거시 던전 생성 (기존 방식)
        /// </summary>
        private List<DungeonNode> GenerateLegacyDungeon()
        {
            LogDebug("DungeonManager: 레거시 생성기 사용");
            
            List<DungeonNode> nodes = new List<DungeonNode>();
            int nextId = 0;
            float xStep = 200f;
            float yOffset = 150f;
            
            // 시작 노드
            DungeonNode startNode = new DungeonNode(nextId++, NodeType.Combat, Vector2.zero);
            startNode.IsAccessible = true;
            nodes.Add(startNode);
            
            // 중간 노드들
            for (int level = 1; level < _roomsPerFloor - 1; level++)
            {
                int nodesAtLevel = Mathf.Min(_branchCount, 
                    Mathf.RoundToInt(_branchCount * (1f - Mathf.Abs(level - _roomsPerFloor/2) / (float)_roomsPerFloor)));
                nodesAtLevel = Mathf.Max(1, nodesAtLevel);
                
                float levelWidthScale = (level < 3 || level > _roomsPerFloor - 4) ? 0.5f : 1.0f;
                
                for (int branch = 0; branch < nodesAtLevel; branch++)
                {
                    NodeType type = (level == 5) ? NodeType.MiniBoss : GetRandomNodeType();
                    
                    float yPos = 0;
                    if (nodesAtLevel > 1)
                    {
                        float normalizedPos = (branch / (float)(nodesAtLevel - 1)) * 2f - 1f;
                        float randomOffset = Random.Range(-0.1f, 0.1f);
                        yPos = (normalizedPos + randomOffset) * yOffset * levelWidthScale;
                    }
                    
                    Vector2 position = new Vector2(level * xStep, yPos);
                    DungeonNode node = new DungeonNode(nextId++, type, position);
                    nodes.Add(node);
                }
            }
            
            // 보스 노드
            DungeonNode bossNode = new DungeonNode(nextId++, NodeType.Boss, 
                new Vector2((_roomsPerFloor - 1) * xStep, 0));
            nodes.Add(bossNode);
            
            // 연결 생성
            ConnectNodesLegacy(nodes);
            
            return nodes;
        }
        #endregion
        
        #region Node Management
        /// <summary>
        /// 노드 이동 가능 여부 확인
        /// </summary>
        private bool CanMoveToNode(DungeonNode targetNode)
        {
            if (_currentNode == null) return targetNode.IsAccessible;
            return targetNode.IsAccessible && !targetNode.IsVisited;
        }
        
        /// <summary>
        /// 노드 이동 실행
        /// </summary>
        private void ExecuteNodeMovement(int nodeIndex, DungeonNode targetNode)
        {
            // 노드 상태 업데이트
            _currentNodeIndex = nodeIndex;
            _currentNode = targetNode;
            _currentNode.IsVisited = true;
            _currentNode.IsAccessible = false;
            
            // 연결된 노드들 활성화
            ActivateConnectedNodes(_currentNode);
            
            LogDebug($"DungeonManager: 노드 {nodeIndex}로 이동 완료 (타입: {_currentNode.Type})");
            
            // 노드 타입별 처리
            ProcessNodeByType(_currentNode);
            
            // UI 업데이트
            UpdateDungeonUI();
        }
        
        /// <summary>
        /// 연결된 노드들 활성화
        /// </summary>
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
        
        /// <summary>
        /// 노드 인덱스 유효성 검사
        /// </summary>
        private bool IsValidNodeIndex(int nodeIndex)
        {
            return nodeIndex >= 0 && nodeIndex < _dungeonNodes.Count;
        }
        #endregion
        
        #region Node Processing
        /// <summary>
        /// 노드 타입별 처리
        /// </summary>
        private void ProcessNodeByType(DungeonNode node)
        {
            switch (node.Type)
            {
                case NodeType.Combat:
                    ProcessCombatNode(node);
                    break;
                    
                case NodeType.MiniBoss:
                    ProcessMiniBossNode(node);
                    break;
                    
                case NodeType.Boss:
                    ProcessBossNode(node);
                    break;
                    
                case NodeType.Event:
                    ProcessEventNode(node);
                    break;
                    
                case NodeType.Shop:
                    ProcessShopNode(node);
                    break;
                    
                case NodeType.Rest:
                    ProcessRestNode(node);
                    break;
                    
                default:
                    Debug.LogWarning($"DungeonManager: 알 수 없는 노드 타입: {node.Type}");
                    break;
            }
        }
        
        /// <summary>
        /// 전투 노드 처리
        /// </summary>
        private void ProcessCombatNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 전투 노드 처리");
            
            if (_gameManager != null)
            {
                _gameManager.SwitchPanel("CombatPanel");
            }
            
            StartCombat("루멘 리퍼", CharacterType.Normal);
        }
        
        /// <summary>
        /// 미니보스 노드 처리
        /// </summary>
        private void ProcessMiniBossNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 미니보스 노드 처리");
            
            if (_gameManager != null)
            {
                _gameManager.SwitchPanel("CombatPanel");
            }
            
            StartCombat("그림자 수호자", CharacterType.MiniBoss);
        }
        
        /// <summary>
        /// 보스 노드 처리
        /// </summary>
        private void ProcessBossNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 보스 노드 처리");
            
            if (_gameManager != null)
            {
                _gameManager.SwitchPanel("CombatPanel");
            }
            
            StartCombat("검은 심연", CharacterType.Boss);
        }
        
        /// <summary>
        /// 이벤트 노드 처리
        /// </summary>
        private void ProcessEventNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 이벤트 노드 처리");
            
            // UI 패널 전환
            if (_dungeonUI != null)
            {
                _dungeonUI.ShowEventPanel();
            }
            else if (_gameManager != null)
            {
                _gameManager.SwitchPanel("EventPanel");
            }
            
            TriggerEvent();
        }
        
        /// <summary>
        /// 상점 노드 처리
        /// </summary>
        private void ProcessShopNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 상점 노드 처리");
            
            // UI 패널 전환
            if (_dungeonUI != null)
            {
                _dungeonUI.ShowShopPanel();
            }
            else if (_gameManager != null)
            {
                _gameManager.SwitchPanel("ShopPanel");
            }
            
            ShowShop();
        }
        
        /// <summary>
        /// 휴식 노드 처리
        /// </summary>
        private void ProcessRestNode(DungeonNode node)
        {
            LogDebug("DungeonManager: 휴식 노드 처리");
            
            // UI 패널 전환
            if (_dungeonUI != null)
            {
                _dungeonUI.ShowRestPanel();
            }
            else if (_gameManager != null)
            {
                _gameManager.SwitchPanel("RestPanel");
            }
            
            ApplyRest();
        }
        #endregion
        
        #region Combat & Events
        /// <summary>
        /// 전투 시작
        /// </summary>
        private void StartCombat(string enemyType, CharacterType type)
        {
            if (_gameManager != null)
            {
                CharacterManager characterManager = CharacterManager.Instance;
                if (characterManager != null)
                {
                    EnemyCharacter enemy = characterManager.CreateEnemyCharacter(enemyType, type);
                    
                    if (_gameManager.CombatManager != null)
                    {
                        _gameManager.CombatManager.SetEnemyCharacter(enemy);
                    }
                }
                
                _gameManager.StartCombat();
            }
            else
            {
                Debug.LogError("DungeonManager: 전투 시작 실패 - GameManager 없음");
            }
        }
        
        /// <summary>
        /// 이벤트 발생
        /// </summary>
        private void TriggerEvent()
        {
            LogDebug("DungeonManager: 이벤트 발생");
            
            EventManager eventManager = FindObjectOfType<EventManager>();
            if (eventManager != null && _currentNode != null)
            {
                eventManager.StartEvent(_currentNode);
            }
            else
            {
                Debug.LogWarning("DungeonManager: EventManager 없음 또는 currentNode null");
                
                if (_gameManager != null)
                {
                    _gameManager.ChangeState(GameManager.GameState.Event);
                }
                
                StartCoroutine(AutoCompleteRoomAfterDelay(3f));
            }
        }
        
        /// <summary>
        /// 상점 표시
        /// </summary>
        private void ShowShop()
        {
            LogDebug("DungeonManager: 상점 표시");
            
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null && _currentNode != null)
            {
                shopManager.OpenShop(_currentNode);
            }
            else
            {
                Debug.LogWarning("DungeonManager: ShopManager 없음");
                StartCoroutine(AutoCompleteRoomAfterDelay(3f));
            }
        }
        
        /// <summary>
        /// 휴식 효과 적용
        /// </summary>
        private void ApplyRest()
        {
            LogDebug("DungeonManager: 휴식 적용");
            
            CharacterManager characterManager = CharacterManager.Instance;
            if (characterManager != null && characterManager.CurrentPlayer != null)
            {
                PlayerCharacter player = characterManager.CurrentPlayer;
                int healAmount = Mathf.RoundToInt(player.MaxHealth * 0.3f);
                player.Heal(healAmount);
                
                LogDebug($"DungeonManager: 플레이어 체력 회복 - {healAmount}HP");
                
                SetupRestUI(healAmount);
                StartCoroutine(AutoCompleteRoomAfterDelay(2f));
            }
        }
        #endregion
        
        #region Utility Methods
        /// <summary>
        /// 생성된 던전 검증
        /// </summary>
        private void ValidateGeneratedDungeon()
        {
            if (_dungeonNodes == null || _dungeonNodes.Count == 0)
            {
                Debug.LogError("DungeonManager: 던전 노드가 생성되지 않음");
                return;
            }
            
            int combatCount = 0, eventCount = 0, shopCount = 0, restCount = 0, miniBossCount = 0, bossCount = 0;
            
            foreach (DungeonNode node in _dungeonNodes)
            {
                switch (node.Type)
                {
                    case NodeType.Combat: combatCount++; break;
                    case NodeType.Event: eventCount++; break;
                    case NodeType.Shop: shopCount++; break;
                    case NodeType.Rest: restCount++; break;
                    case NodeType.MiniBoss: miniBossCount++; break;
                    case NodeType.Boss: bossCount++; break;
                }
            }
            
            LogDebug("=== 던전 구조 검증 ===");
            LogDebug($"전투: {combatCount}, 이벤트: {eventCount}, 상점: {shopCount}");
            LogDebug($"휴식: {restCount}, 미니보스: {miniBossCount}, 보스: {bossCount}");
            LogDebug($"총 노드 수: {_dungeonNodes.Count}");
            LogDebug("===================");
            
            // 필수 요소 검증
            if (miniBossCount < 1)
            {
                Debug.LogWarning("DungeonManager: 미니보스가 없습니다!");
            }
            
            if (bossCount < 1)
            {
                Debug.LogWarning("DungeonManager: 보스가 없습니다!");
            }
        }
        
        /// <summary>
        /// 레거시 노드 연결
        /// </summary>
        private void ConnectNodesLegacy(List<DungeonNode> nodes)
        {
            Dictionary<int, List<DungeonNode>> nodesByLevel = new Dictionary<int, List<DungeonNode>>();
            
            // 레벨별 분류
            foreach (DungeonNode node in nodes)
            {
                int level = Mathf.RoundToInt(node.Position.x / 200f);
                
                if (!nodesByLevel.ContainsKey(level))
                    nodesByLevel[level] = new List<DungeonNode>();
                    
                nodesByLevel[level].Add(node);
            }
            
            // 레벨 간 연결
            for (int level = 0; level < _roomsPerFloor - 1; level++)
            {
                if (!nodesByLevel.ContainsKey(level) || !nodesByLevel.ContainsKey(level + 1))
                    continue;
                    
                List<DungeonNode> currentLevel = nodesByLevel[level];
                List<DungeonNode> nextLevel = nodesByLevel[level + 1];
                
                foreach (DungeonNode currentNode in currentLevel)
                {
                    if (nextLevel.Count == 1)
                    {
                        currentNode.ConnectedNodes.Add(nextLevel[0].ID);
                        if (currentNode.IsAccessible || currentNode.IsVisited)
                        {
                            nextLevel[0].IsAccessible = true;
                        }
                    }
                    else
                    {
                        List<DungeonNode> nearestNodes = FindNearestNodes(currentNode, nextLevel, 2);
                        foreach (DungeonNode nearNode in nearestNodes)
                        {
                            if (!currentNode.ConnectedNodes.Contains(nearNode.ID))
                            {
                                currentNode.ConnectedNodes.Add(nearNode.ID);
                                if (currentNode.IsAccessible || currentNode.IsVisited)
                                {
                                    nearNode.IsAccessible = true;
                                }
                            }
                        }
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
        /// 랜덤 노드 타입 결정
        /// </summary>
        private NodeType GetRandomNodeType()
        {
            float random = Random.value;
            
            if (random < _combatRoomChance)
                return NodeType.Combat;
            else if (random < _combatRoomChance + _eventRoomChance)
                return NodeType.Event;
            else if (random < _combatRoomChance + _eventRoomChance + _shopRoomChance)
                return NodeType.Shop;
            else
                return NodeType.Rest;
        }
        
        /// <summary>
        /// 휴식 UI 설정
        /// </summary>
        private void SetupRestUI(int healAmount)
        {
            if (_dungeonUI != null)
            {
                Transform restPanel = _dungeonUI.transform.Find("DungeonPanel/RestPanel");
                if (restPanel != null)
                {
                    TMPro.TextMeshProUGUI restTitle = restPanel.Find("RestTitle")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (restTitle != null)
                    {
                        restTitle.text = "휴식 지점";
                    }
                    
                    TMPro.TextMeshProUGUI restDesc = restPanel.Find("RestDescription")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (restDesc != null)
                    {
                        restDesc.text = $"휴식을 취하여 체력을 회복합니다. 회복량: {healAmount}HP";
                    }
                    
                    UnityEngine.UI.Button restButton = restPanel.Find("RestButton")?.GetComponent<UnityEngine.UI.Button>();
                    if (restButton != null)
                    {
                        restButton.onClick.RemoveAllListeners();
                        restButton.onClick.AddListener(() => OnRoomCompleted());
                    }
                }
            }
        }
        
        /// <summary>
        /// 자동 방 완료 처리
        /// </summary>
        private IEnumerator AutoCompleteRoomAfterDelay(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            OnRoomCompleted();
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
                Debug.LogError("DungeonManager: UIManager 없음 - 던전 UI 업데이트 실패");
            }
            
            if (_dungeonUI != null)
            {
                _dungeonUI.UpdateStageInfo(_currentStage + 1, _currentNodeIndex + 1);
                _dungeonUI.ShowRoomSelectionPanel();
            }
        }
        
        /// <summary>
        /// 디버그 로그
        /// </summary>
        private void LogDebug(string message)
        {
            if (_enableDebugOutput)
            {
                Debug.Log($"[DungeonManager] {message}");
            }
        }
        #endregion
    }
}