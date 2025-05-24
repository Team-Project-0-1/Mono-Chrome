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
    
        [Header("개선된 던전 생성기")]
        [SerializeField] private ImprovedDungeonGenerator _improvedGenerator;
        [SerializeField] private AdvancedDungeonGenerator _advancedGenerator;
        [SerializeField] private bool _useImprovedGenerator = true;
        [SerializeField] private bool _useAdvancedGenerator = true;
        
        [Header("현재 던전 상태")]
        [SerializeField] private int _currentFloor = 0;
        [SerializeField] private int _currentNodeIndex = -1;
        
        private List<DungeonNode> _dungeonNodes = new List<DungeonNode>();
        private DungeonNode _currentNode;
        
        private GameManager _gameManager;
        private UIManager _uiManager;
        private DungeonUI _dungeonUI;
        
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
            
            // DungeonUI 참조 가져오기
            // 조금 딜레이를 주어 UI가 초기화될 시간 확보
            StartCoroutine(FindDungeonUIWithDelay());
        }
        
        private IEnumerator FindDungeonUIWithDelay()
        {
            // UI 초기화를 위해 1프레임 대기
            yield return null;
            
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                // Canvas에서 DungeonPanel 찾기
                Transform dungeonPanelTransform = canvas.transform.Find("DungeonPanel");
                
                if (dungeonPanelTransform != null)
                {
                    // DungeonPanel에서 DungeonUI 컴포넌트 찾기
                    _dungeonUI = dungeonPanelTransform.GetComponent<DungeonUI>();
                    if (_dungeonUI == null)
                    {
                        _dungeonUI = dungeonPanelTransform.gameObject.AddComponent<DungeonUI>();
                        Debug.Log("DungeonManager: Added DungeonUI component to existing DungeonPanel");
                    }
                    else
                    {
                        Debug.Log("DungeonManager: Found DungeonUI component on DungeonPanel");
                    }
                }
                else
                {
                    // DungeonPanel이 없으면 생성
                    GameObject dungeonPanel = new GameObject("DungeonPanel");
                    dungeonPanel.transform.SetParent(canvas.transform, false);
                    
                    // UI 컴포넌트 추가
                    RectTransform rectTransform = dungeonPanel.AddComponent<RectTransform>();
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    
                    // DungeonUI 컴포넌트 추가
                    _dungeonUI = dungeonPanel.AddComponent<DungeonUI>();
                    Debug.Log("DungeonManager: Created new DungeonPanel with DungeonUI component");
                    
                    // DungeonUI 초기화 시간 확보
                    yield return null;
                }
            }
            else
            {
                Debug.LogError("DungeonManager: Canvas not found in the scene");
            }
            
            // 전체 씬에서 DungeonUI 컴포넌트 찾기 (탭백 방법)
            if (_dungeonUI == null)
            {
                _dungeonUI = FindObjectOfType<DungeonUI>();
                if (_dungeonUI != null)
                {
                    Debug.Log("DungeonManager: Found DungeonUI component in the scene");
                }
                else
                {
                    Debug.LogWarning("DungeonManager: DungeonUI component not found in the scene");
                }
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
        /// 던전 노드 생성 (최적화된 버전 사용)
        /// </summary>
        private void GenerateDungeonNodes()
        {
            if (_useAdvancedGenerator)
            {
                GenerateDungeonNodesAdvanced();
                return;
            }
            
            if (_useImprovedGenerator)
            {
                GenerateDungeonNodesImproved();
                return;
            }
            
            // 기존 방식 (fallback)
            GenerateDungeonNodesLegacy();
        }
        
        /// <summary>
        /// 고급 던전 노드 생성 (최신 버전)
        /// </summary>
        private void GenerateDungeonNodesAdvanced()
        {
            if (_advancedGenerator == null)
            {
                _advancedGenerator = gameObject.AddComponent<AdvancedDungeonGenerator>();
            }
            
            _dungeonNodes = _advancedGenerator.GenerateAdvancedDungeon();
            Debug.Log($"DungeonManager: Generated {_dungeonNodes.Count} nodes using advanced generator");
        }
        
        /// <summary>
        /// 개선된 던전 노드 생성
        /// </summary>
        private void GenerateDungeonNodesImproved()
        {
            if (_improvedGenerator == null)
            {
                _improvedGenerator = gameObject.AddComponent<ImprovedDungeonGenerator>();
            }
            
            _dungeonNodes = _improvedGenerator.GenerateImprovedDungeon();
            Debug.Log($"DungeonManager: Generated {_dungeonNodes.Count} nodes using improved generator");
        }
        
        /// <summary>
        /// 기존 던전 노드 생성 방식 (레거시)
        /// </summary>
        private void GenerateDungeonNodesLegacy()
        {
            // ID 카운터
            int nextId = 0;
            
            // 스테이지별 노드 개수
            int nodesPerStage = _roomsPerFloor;
            
            // 고정 스테이지 위치 (스테이지 진행에 따라 배치)
            float xStep = 200f; // X축 간격
            float yOffset = 150f; // Y축 분기 최대 간격
            
            // 시작 노드 생성 (시작 노드는 Combat 타입으로 설정)
            DungeonNode startNode = new DungeonNode(nextId++, NodeType.Combat, new Vector2(0, 0));
            startNode.IsAccessible = true;
            _dungeonNodes.Add(startNode);
            
            // 분기 트리 구조를 위한 레벨별 노드 생성
            for (int level = 1; level < nodesPerStage - 1; level++)
            {
                // 각 레벨에서 생성할 노드 수 계산 (시작과 끝으로 갈수록 분기 줄임)
                int nodesAtThisLevel = Mathf.Min(_branchCount, 
                    Mathf.RoundToInt(_branchCount * (1f - Mathf.Abs(level - nodesPerStage/2) / (float)nodesPerStage)));
                
                // 최소 1개 이상
                nodesAtThisLevel = Mathf.Max(1, nodesAtThisLevel);
                
                // 위치 스케일링 비율 (레벨에 따라 너비 조정)
                float levelWidthScale = 1.0f;
                if (level < 3 || level > nodesPerStage - 4)
                    levelWidthScale = 0.5f; // 시작과 끝 부분은 더 좁게
                
                // 각 위치별로 분기 생성
                for (int branch = 0; branch < nodesAtThisLevel; branch++)
                {
                    NodeType type = GetRandomNodeType();
                    
                    // 중간 보스방 설정 (5번째 방)
                    if (level == 5)
                    {
                        type = NodeType.MiniBoss;
                    }
                    
                    // 위치 계산 (x: 진행도, y: 분기)
                    // y값 계산 시 균등 분포하되 약간의 랜덤성 추가
                    float yPos = 0;
                    if (nodesAtThisLevel > 1)
                    {
                        // 정규화된 위치 (-1 ~ 1 범위)
                        float normalizedPos = (branch / (float)(nodesAtThisLevel - 1)) * 2f - 1f;
                        // 약간의 랜덤 오프셋 추가 (±10%)
                        float randomOffset = Random.Range(-0.1f, 0.1f);
                        yPos = (normalizedPos + randomOffset) * yOffset * levelWidthScale;
                    }
                    
                    // 최종 위치 (x축은 레벨에 비례, y축은 분기 위치)
                    Vector2 position = new Vector2(level * xStep, yPos);
                    
                    DungeonNode node = new DungeonNode(nextId++, type, position);
                    _dungeonNodes.Add(node);
                }
            }
            
            // 보스 노드 생성
            DungeonNode bossNode = new DungeonNode(nextId++, NodeType.Boss, new Vector2((nodesPerStage - 1) * xStep, 0));
            _dungeonNodes.Add(bossNode);
            
            // 노드 간 연결 설정
            ConnectNodes();
        }
        
        /// <summary>
        /// 노드 간 연결 설정
        /// </summary>
        private void ConnectNodes()
        {
            // 스테이지별 노드 그룹화
            Dictionary<int, List<DungeonNode>> nodesByLevel = new Dictionary<int, List<DungeonNode>>();
            
            // 각 노드를 x 위치에 따라 레벨별로 분류
            foreach (DungeonNode node in _dungeonNodes)
            {
                // x 위치를 기준으로 레벨 계산 (200 단위로 나눔)
                int level = Mathf.RoundToInt(node.Position.x / 200f);
                
                if (!nodesByLevel.ContainsKey(level))
                {
                    nodesByLevel[level] = new List<DungeonNode>();
                }
                
                nodesByLevel[level].Add(node);
            }
            
            // 각 레벨에서 다음 레벨로 연결
            for (int level = 0; level < _roomsPerFloor - 1; level++)
            {
                // 현재 레벨과 다음 레벨의 노드 확인
                if (!nodesByLevel.ContainsKey(level) || !nodesByLevel.ContainsKey(level + 1))
                    continue;
                    
                List<DungeonNode> currentLevelNodes = nodesByLevel[level];
                List<DungeonNode> nextLevelNodes = nodesByLevel[level + 1];
                
                // 모든 현재 레벨 노드에 대해
                foreach (DungeonNode currentNode in currentLevelNodes)
                {
                    // 다음 레벨에 노드가 1개만 있는 경우 (시작 노드 또는 보스 노드로 연결되는 경우)
                    if (nextLevelNodes.Count == 1)
                    {
                        currentNode.ConnectedNodes.Add(nextLevelNodes[0].ID);
                        
                        // 첫 번째 노드에서 접근 가능하거나 방문한 노드의 연결 노드도 접근 가능하게 설정
                        if (currentNode.IsAccessible || currentNode.IsVisited)
                        {
                            nextLevelNodes[0].IsAccessible = true;
                        }
                    }
                    // 다음 레벨에 여러 노드가 있는 경우
                    else
                    {
                        // Y 위치 기반으로 가장 가까운 노드 찾기
                        List<DungeonNode> nearestNodes = FindNearestNodes(currentNode, nextLevelNodes, 2);
                        
                        foreach (DungeonNode nearNode in nearestNodes)
                        {
                            // 이미 연결되어 있지 않은 경우에만 연결
                            if (!currentNode.ConnectedNodes.Contains(nearNode.ID))
                            {
                                currentNode.ConnectedNodes.Add(nearNode.ID);
                                
                                // 접근 가능한 노드에서 연결된 노드도 접근 가능하게 설정
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
            
            // 던전 정보 패널 및 상태 업데이트
            if (_dungeonUI != null)
            {
                // 현재 층 및 방 정보 업데이트
                _dungeonUI.UpdateStageInfo(_currentFloor + 1, _currentNodeIndex + 1);
                
                // 방 선택 패널 활성화 (새로운 메서드 활용)
                _dungeonUI.ShowRoomSelectionPanel();
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
            // DungeonUI 참조 업데이트
            if (_dungeonUI == null)
            {
                _dungeonUI = FindObjectOfType<DungeonUI>();
                if (_dungeonUI == null)
                {
                    Debug.LogError("DungeonManager: DungeonUI component not found during ProcessNodeByType");
                }
            }
            
            switch (node.Type)
            {
                case NodeType.Combat:
                    Debug.Log("DungeonManager: Processing Combat node");
                    
                    // UI 패널 전환 - 전투 패널 활성화
                    if (_gameManager != null)
                    {
                        _gameManager.SwitchPanel("CombatPanel");
                    }
                    
                    // 전투 시작
                    StartCombat("루멘 리퍼", CharacterType.Normal);
                    break;
                    
                case NodeType.MiniBoss:
                    Debug.Log("DungeonManager: Processing MiniBoss node");
                    
                    // UI 패널 전환 - 전투 패널 활성화
                    if (_gameManager != null)
                    {
                        _gameManager.SwitchPanel("CombatPanel");
                    }
                    
                    // 미니보스 전투
                    StartCombat("그림자 수호자", CharacterType.MiniBoss);
                    break;
                    
                case NodeType.Boss:
                    Debug.Log("DungeonManager: Processing Boss node");
                    
                    // UI 패널 전환 - 전투 패널 활성화
                    if (_gameManager != null)
                    {
                        _gameManager.SwitchPanel("CombatPanel");
                    }
                    
                    // 보스 전투
                    StartCombat("검은 심연", CharacterType.Boss);
                    break;
                    
                case NodeType.Event:
                    Debug.Log("DungeonManager: Processing Event node");
                    
                    // UI 패널 전환 - 이벤트 패널 활성화
                    if (_dungeonUI != null)
                    {
                        _dungeonUI.ShowEventPanel();
                    }
                    else if (_gameManager != null)
                    {
                        // 폴백 - GameManager를 통한 패널 전환
                        _gameManager.SwitchPanel("EventPanel");
                    }
                    
                    // 이벤트 발생
                    TriggerEvent();
                    break;
                    
                case NodeType.Shop:
                    Debug.Log("DungeonManager: Processing Shop node");
                    
                    // UI 패널 전환 - 상점 패널 활성화
                    if (_dungeonUI != null)
                    {
                        _dungeonUI.ShowShopPanel();
                    }
                    else if (_gameManager != null)
                    {
                        // 폴백 - GameManager를 통한 패널 전환
                        _gameManager.SwitchPanel("ShopPanel");
                    }
                    
                    // 상점 UI 표시
                    ShowShop();
                    break;
                    
                case NodeType.Rest:
                    Debug.Log("DungeonManager: Processing Rest node");
                    
                    // UI 패널 전환 - 휴식 패널 활성화
                    if (_dungeonUI != null)
                    {
                        _dungeonUI.ShowRestPanel();
                    }
                    else if (_gameManager != null)
                    {
                        // 폴백 - GameManager를 통한 패널 전환
                        _gameManager.SwitchPanel("RestPanel");
                    }
                    
                    // 휴식 효과 적용
                    ApplyRest();
                    break;
                    
                default:
                    Debug.LogWarning($"DungeonManager: Unknown node type: {node.Type}");
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
                Debug.LogWarning("DungeonManager: EventManager not found or currentNode is null");
                
                // 이벤트 매니저가 없는 경우에는 게임 상태만 변경
                if (_gameManager != null)
                {
                    _gameManager.ChangeState(GameManager.GameState.Event);
                }
                
                // 일정 시간 후 방 완료 처리 (임시 코드)
                StartCoroutine(AutoCompleteRoomAfterDelay(3f));
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
                Debug.LogWarning("DungeonManager: ShopManager not found or currentNode is null");
                
                // ShopManager가 없는 경우 일정 시간 후 방 완료 처리
                StartCoroutine(AutoCompleteRoomAfterDelay(3f));
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
                
                // 휴식 UI 설정
                SetupRestUI(healAmount);
                
                // 일정 시간 후 방 완료 처리 (임시 코드)
                StartCoroutine(AutoCompleteRoomAfterDelay(2f));
            }
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
                    // 휴식 타이틀 텍스트 설정
                    TMPro.TextMeshProUGUI restTitle = restPanel.Find("RestTitle")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (restTitle != null)
                    {
                        restTitle.text = "휴식 지점";
                    }
                    
                    // 휴식 설명 텍스트 설정
                    TMPro.TextMeshProUGUI restDesc = restPanel.Find("RestDescription")?.GetComponent<TMPro.TextMeshProUGUI>();
                    if (restDesc != null)
                    {
                        restDesc.text = $"휴식을 취하여 체력을 회복합니다. 회복량: {healAmount}HP";
                    }
                    
                    // 휴식 버튼 설정
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
        /// 일정 시간 후 방 활동 완료 처리 (임시 코드)
        /// </summary>
        private IEnumerator AutoCompleteRoomAfterDelay(float delayInSeconds)
        {
            yield return new WaitForSeconds(delayInSeconds);
            OnRoomCompleted();
        }
        
        /// <summary>
        /// 방 활동 완료 후 호출되는 메서드
        /// </summary>
        public void OnRoomCompleted()
        {
            Debug.Log("DungeonManager: Room activity completed");
            
            // 던전 상태로 변경
            if (_gameManager != null)
            {
                _gameManager.ChangeState(GameManager.GameState.Dungeon);
            }
            
            // DungeonUI가 있다면 방 선택 패널 활성화
            if (_dungeonUI != null)
            {
                _dungeonUI.ShowRoomSelectionPanel();
            }
            
            // UI 업데이트
            UpdateDungeonUI();
        }
    }
}