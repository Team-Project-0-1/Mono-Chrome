using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 턴 기반 던전 생성기
    /// 15턴 구조와 방 분포 비율을 고려한 선택지 시스템
    /// </summary>
    [Serializable]
    public class TurnBasedDungeonGenerator
    {
        [Header("던전 설정")]
        public int MaxTurns = 15;
        public bool EnableDebugLogs = true;

        [Header("방 분포 비율 설정")]
        [SerializeField] private RoomDistributionConfig _distributionConfig;

        private TurnBasedDungeonData _dungeonData;
        private Dictionary<NodeType, MinMaxRange> _typeTargets;
        private SensoryHintSystem _hintSystem;
        private ConditionalEventManager _eventManager;

        public TurnBasedDungeonData DungeonData => _dungeonData;
        public int CurrentTurn => _dungeonData?.CurrentTurn ?? 1;

        public TurnBasedDungeonGenerator()
        {
            InitializeGenerator();
        }

        public TurnBasedDungeonGenerator(int maxTurns)
        {
            MaxTurns = maxTurns;
            InitializeGenerator();
        }

        private void InitializeGenerator()
        {
            _dungeonData = new TurnBasedDungeonData(MaxTurns);
            _hintSystem = new SensoryHintSystem();
            _eventManager = new ConditionalEventManager();
            SetupDefaultDistribution();
            LogDebug("TurnBasedDungeonGenerator 초기화 완료");
        }

        /// <summary>
        /// 기본 방 분포 비율 설정
        /// </summary>
        private void SetupDefaultDistribution()
        {
            _typeTargets = new Dictionary<NodeType, MinMaxRange>
            {
                { NodeType.Combat, new MinMaxRange(4, 6) },     // 26.7~40%
                { NodeType.Event, new MinMaxRange(1, 3) },      // 6.7~20%
                { NodeType.Shop, new MinMaxRange(1, 2) },       // 6.7~13.3%
                { NodeType.Rest, new MinMaxRange(1, 2) },       // 6.7~13.3%
                { NodeType.MiniBoss, new MinMaxRange(0, 1) },   // 5-6턴에만
                { NodeType.Boss, new MinMaxRange(1, 1) }        // 15턴 필수
            };
        }

        /// <summary>
        /// 새로운 던전 생성 시작
        /// </summary>
        public void StartNewDungeon()
        {
            _dungeonData.Reset();
            LogDebug($"새 던전 시작 - {MaxTurns}턴 구조");
        }

        /// <summary>
        /// 현재 턴에 대한 3개 선택지 생성
        /// </summary>
        public List<RoomChoice> GenerateChoicesForTurn(int turn)
        {
            LogDebug($"=== {turn}턴 선택지 생성 시작 ===");
            
            var choices = new List<RoomChoice>();

            // 15턴 - 보스방 3개 고정
            if (turn == MaxTurns)
            {
                for (int i = 0; i < 3; i++)
                {
                    choices.Add(CreateRoomChoice(NodeType.Boss, turn));
                }
                LogDebug($"{turn}턴: 보스방 3개 생성");
                return choices;
            }

            // 5-6턴 - 미니보스 1개 + 일반 2개 (미니보스 미사용 시)
            if (turn >= 5 && turn <= 6 && !_dungeonData.HasUsedType(NodeType.MiniBoss))
            {
                choices.Add(CreateRoomChoice(NodeType.MiniBoss, turn));
                choices.AddRange(Generate2NormalChoices(turn));
                LogDebug($"{turn}턴: 미니보스 1개 + 일반 2개 생성");
            }
            else
            {
                // 일반 3개
                choices = Generate3NormalChoices(turn);
                LogDebug($"{turn}턴: 일반 3개 생성");
            }

            // 유효성 검증 및 조정
            choices = ValidateAndAdjustChoices(choices, turn);

            LogDebug($"=== {turn}턴 선택지 생성 완료: {string.Join(", ", choices.Select(c => c.Type))} ===");
            return choices;
        }

        /// <summary>
        /// 3개 일반 선택지 생성
        /// </summary>
        private List<RoomChoice> Generate3NormalChoices(int turn)
        {
            var choices = new List<RoomChoice>();
            var availableTypes = GetAvailableTypes(turn);
            var weights = CalculateTypeWeights(turn);

            LogDebug($"사용 가능한 타입: {string.Join(", ", availableTypes)}");

            for (int i = 0; i < 3; i++)
            {
                var selectedType = SelectTypeWithWeights(availableTypes, weights);
                choices.Add(CreateRoomChoice(selectedType, turn));
                
                // 동일 타입 연속 방지를 위해 선택된 타입 임시 제거
                availableTypes.Remove(selectedType);
                
                // 사용 가능한 타입이 부족하면 다시 추가
                if (availableTypes.Count == 0)
                {
                    availableTypes = GetAvailableTypes(turn);
                    availableTypes.Remove(selectedType); // 방금 선택한 것만 제외
                }
            }

            return choices;
        }

        /// <summary>
        /// 2개 일반 선택지 생성 (미니보스와 함께)
        /// </summary>
        private List<RoomChoice> Generate2NormalChoices(int turn)
        {
            var choices = new List<RoomChoice>();
            var availableTypes = GetAvailableTypes(turn);
            var weights = CalculateTypeWeights(turn);

            for (int i = 0; i < 2; i++)
            {
                var selectedType = SelectTypeWithWeights(availableTypes, weights);
                choices.Add(CreateRoomChoice(selectedType, turn));
                availableTypes.Remove(selectedType);
                
                if (availableTypes.Count == 0)
                {
                    availableTypes = GetAvailableTypes(turn);
                }
            }

            return choices;
        }

        /// <summary>
        /// 현재 턴에서 사용 가능한 방 타입 목록 반환
        /// </summary>
        private List<NodeType> GetAvailableTypes(int turn)
        {
            var availableTypes = new List<NodeType> { NodeType.Combat, NodeType.Event, NodeType.Shop, NodeType.Rest };
            
            // 최대 사용량에 도달한 타입 제거
            availableTypes.RemoveAll(type => 
            {
                int currentUsage = _dungeonData.GetTypeUsage(type);
                int maxUsage = _typeTargets[type].Max;
                return currentUsage >= maxUsage;
            });

            LogDebug($"현재 사용량: {string.Join(", ", _dungeonData.TypeUsageCount.Select(kvp => $"{kvp.Key}={kvp.Value}"))}");
            
            return availableTypes;
        }

        /// <summary>
        /// 방 타입별 가중치 계산
        /// </summary>
        private Dictionary<NodeType, float> CalculateTypeWeights(int turn)
        {
            var weights = new Dictionary<NodeType, float>
            {
                { NodeType.Combat, 1.0f },
                { NodeType.Event, 0.8f },
                { NodeType.Shop, 0.6f },
                { NodeType.Rest, 0.6f }
            };

            // 턴별 가중치 조정
            if (turn >= 8) // 중후반
            {
                weights[NodeType.Shop] *= 1.5f;
                weights[NodeType.Rest] *= 1.5f;
                LogDebug("중후반 턴 - 상점/휴식 가중치 증가");
            }

            // 사용량 기반 가중치 조정
            foreach (var type in weights.Keys.ToList())
            {
                int currentUsage = _dungeonData.GetTypeUsage(type);
                var target = _typeTargets[type];

                if (currentUsage < target.Min)
                {
                    weights[type] *= 2.0f; // 최소 요구량 미달 시 우선 배치
                    LogDebug($"{type} 가중치 증가 (최소량 미달: {currentUsage}/{target.Min})");
                }
                else if (currentUsage >= target.Max)
                {
                    weights[type] = 0f; // 최대량 달성 시 배치 금지
                    LogDebug($"{type} 가중치 0 (최대량 달성: {currentUsage}/{target.Max})");
                }
            }

            return weights;
        }

        /// <summary>
        /// 가중치를 고려한 타입 선택
        /// </summary>
        private NodeType SelectTypeWithWeights(List<NodeType> availableTypes, Dictionary<NodeType, float> weights)
        {
            if (availableTypes.Count == 0)
                return NodeType.Combat; // 기본값

            if (availableTypes.Count == 1)
                return availableTypes[0];

            // 가중치 합계 계산
            float totalWeight = availableTypes.Sum(type => weights.GetValueOrDefault(type, 0.1f));
            float randomValue = UnityEngine.Random.value * totalWeight;

            float currentWeight = 0f;
            foreach (var type in availableTypes)
            {
                currentWeight += weights.GetValueOrDefault(type, 0.1f);
                if (randomValue <= currentWeight)
                {
                    return type;
                }
            }

            return availableTypes[UnityEngine.Random.Range(0, availableTypes.Count)];
        }

        /// <summary>
        /// 선택지 유효성 검증 및 조정
        /// </summary>
        private List<RoomChoice> ValidateAndAdjustChoices(List<RoomChoice> choices, int turn)
        {
            // 중복 타입 확인 및 조정
            var typeCounts = choices.GroupBy(c => c.Type).ToDictionary(g => g.Key, g => g.Count());
            
            foreach (var kvp in typeCounts.Where(kvp => kvp.Value > 1))
            {
                LogDebug($"중복 타입 발견: {kvp.Key} x{kvp.Value} - 조정 필요");
                // 중복 제거 로직은 필요 시 구현
            }

            return choices;
        }

        /// <summary>
        /// 방 선택지 생성
        /// </summary>
        private RoomChoice CreateRoomChoice(NodeType type, int turn)
        {
            return CreateRoomChoiceWithSense(type, turn, SenseType.None);
        }

        /// <summary>
        /// 감각 기반 방 선택지 생성
        /// </summary>
        public RoomChoice CreateRoomChoiceWithSense(NodeType type, int turn, SenseType playerSense)
        {
            var choice = new RoomChoice(type, turn);
            
            // 감각 기반 힌트 생성
            if (playerSense != SenseType.None)
            {
                choice.SensoryHint = _hintSystem.GenerateHint(type, playerSense);
                choice.HintColor = _hintSystem.GetSenseColor(playerSense);
                
                LogDebug($"감각 기반 선택지 생성: {turn}턴 {type} ({playerSense}) - {choice.SensoryHint}");
            }
            
            // 이벤트 방인 경우 조건부 이벤트 확인 및 성공률 설정
            if (type == NodeType.Event && _eventManager != null)
            {
                var availableEvents = _eventManager.GetAvailableEvents(turn, playerSense, _dungeonData.SelectedPath);
                if (availableEvents.Count > 0)
                {
                    // 가장 높은 가중치의 이벤트 선택
                    var selectedEvent = availableEvents.First();
                    choice.EventId = selectedEvent.Id;
                    choice.AccurateDescription = selectedEvent.Description;
                    choice.DifficultyModifier = selectedEvent.Weight;
                    
                    // 기본 성공률 설정 (40~70% 범위)
                    choice.SuccessRate = UnityEngine.Random.Range(0.4f, 0.7f);
                    
                    // 감각 일치 시 보너스 설정 (+20~30%)
                    if (IsEventMatchingSense(selectedEvent.Id, playerSense))
                    {
                        choice.SenseBonus = UnityEngine.Random.Range(0.2f, 0.3f);
                        choice.ShowSuccessRate = true;
                        
                        LogDebug($"감각 보너스 적용: {playerSense} 감각 일치 (+{choice.SenseBonus * 100:F0}%)");
                    }
                    
                    LogDebug($"조건부 이벤트 적용: {selectedEvent.Id} - {selectedEvent.Description} (성공률: {choice.SuccessRate * 100:F0}%)");
                }
                else
                {
                    // 기본 이벤트 설정
                    choice.SuccessRate = 0.5f; // 50% 기본 성공률
                    choice.ShowSuccessRate = false;
                }
            }
            
            return choice;
        }

        /// <summary>
        /// 플레이어 감각을 고려한 턴별 선택지 생성
        /// </summary>
        public List<RoomChoice> GenerateChoicesForTurnWithSense(int turn, SenseType playerSense)
        {
            LogDebug($"=== {turn}턴 감각별 선택지 생성 시작 (감각: {playerSense}) ===");
            
            var choices = new List<RoomChoice>();

            // 15턴 - 보스방 3개 고정
            if (turn == MaxTurns)
            {
                for (int i = 0; i < 3; i++)
                {
                    choices.Add(CreateRoomChoiceWithSense(NodeType.Boss, turn, playerSense));
                }
                LogDebug($"{turn}턴: 보스방 3개 생성 (감각 적용)");
                return choices;
            }

            // 5-6턴 - 미니보스 1개 + 일반 2개 (미니보스 미사용 시)
            if (turn >= 5 && turn <= 6 && !_dungeonData.HasUsedType(NodeType.MiniBoss))
            {
                choices.Add(CreateRoomChoiceWithSense(NodeType.MiniBoss, turn, playerSense));
                choices.AddRange(Generate2NormalChoicesWithSense(turn, playerSense));
                LogDebug($"{turn}턴: 미니보스 1개 + 일반 2개 생성 (감각 적용)");
            }
            else
            {
                // 일반 3개
                choices = Generate3NormalChoicesWithSense(turn, playerSense);
                LogDebug($"{turn}턴: 일반 3개 생성 (감각 적용)");
            }

            // 유효성 검증 및 조정
            choices = ValidateAndAdjustChoices(choices, turn);

            LogDebug($"=== {turn}턴 감각별 선택지 생성 완료: {string.Join(", ", choices.Select(c => c.Type))} ===");
            return choices;
        }

        /// <summary>
        /// 3개 일반 선택지 생성 (감각 적용)
        /// </summary>
        private List<RoomChoice> Generate3NormalChoicesWithSense(int turn, SenseType playerSense)
        {
            var choices = new List<RoomChoice>();
            var availableTypes = GetAvailableTypes(turn);
            var weights = CalculateTypeWeights(turn);

            for (int i = 0; i < 3; i++)
            {
                var selectedType = SelectTypeWithWeights(availableTypes, weights);
                choices.Add(CreateRoomChoiceWithSense(selectedType, turn, playerSense));
                
                availableTypes.Remove(selectedType);
                
                if (availableTypes.Count == 0)
                {
                    availableTypes = GetAvailableTypes(turn);
                    availableTypes.Remove(selectedType);
                }
            }

            return choices;
        }

        /// <summary>
        /// 2개 일반 선택지 생성 (감각 적용, 미니보스와 함께)
        /// </summary>
        private List<RoomChoice> Generate2NormalChoicesWithSense(int turn, SenseType playerSense)
        {
            var choices = new List<RoomChoice>();
            var availableTypes = GetAvailableTypes(turn);
            var weights = CalculateTypeWeights(turn);

            for (int i = 0; i < 2; i++)
            {
                var selectedType = SelectTypeWithWeights(availableTypes, weights);
                choices.Add(CreateRoomChoiceWithSense(selectedType, turn, playerSense));
                availableTypes.Remove(selectedType);
                
                if (availableTypes.Count == 0)
                {
                    availableTypes = GetAvailableTypes(turn);
                }
            }

            return choices;
        }

        /// <summary>
        /// 플레이어가 선택한 방 기록
        /// </summary>
        public void RecordPlayerChoice(RoomChoice selectedChoice)
        {
            _dungeonData.RecordChoice(selectedChoice.Type);
            
            // 이벤트 완료 기록
            if (selectedChoice.Type == NodeType.Event && !string.IsNullOrEmpty(selectedChoice.EventId))
            {
                _eventManager?.RecordEventCompletion(selectedChoice.EventId);
            }
            
            LogDebug($"선택 기록: {selectedChoice.TurnNumber}턴 - {selectedChoice.Type}");
            LogDebug($"현재 진행률: {_dungeonData.GetProgress():F1}%");
        }

        /// <summary>
        /// 현재 던전 진행 상태 확인
        /// </summary>
        public bool CanContinue()
        {
            return _dungeonData.CurrentTurn <= MaxTurns;
        }

        /// <summary>
        /// 던전 완료 여부 확인
        /// </summary>
        public bool IsCompleted()
        {
            return _dungeonData.CurrentTurn > MaxTurns;
        }

        /// <summary>
        /// 이벤트가 플레이어 감각과 일치하는지 확인
        /// </summary>
        private bool IsEventMatchingSense(string eventId, SenseType playerSense)
        {
            // 설계서에 따른 감각별 이벤트 매칭
            return eventId switch
            {
                "청각_특화_이벤트" => playerSense == SenseType.Auditory,
                "후각_특화_이벤트" => playerSense == SenseType.Olfactory,
                "촉각_특화_이벤트" => playerSense == SenseType.Tactile,
                "영적_보스_예감" => playerSense == SenseType.Spiritual,
                _ => UnityEngine.Random.value < 0.3f // 30% 확률로 일반 이벤트도 감각 보너스
            };
        }

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[TurnBasedDungeonGenerator] {message}");
            }
        }
    }

    /// <summary>
    /// 방 분포 설정을 위한 보조 클래스
    /// </summary>
    [Serializable]
    public class RoomDistributionConfig
    {
        public MinMaxRange CombatRange = new MinMaxRange(4, 6);
        public MinMaxRange EventRange = new MinMaxRange(1, 3);
        public MinMaxRange ShopRange = new MinMaxRange(1, 2);
        public MinMaxRange RestRange = new MinMaxRange(1, 2);
    }

    /// <summary>
    /// 최소-최대 범위를 나타내는 구조체
    /// </summary>
    [Serializable]
    public struct MinMaxRange
    {
        public int Min;
        public int Max;

        public MinMaxRange(int min, int max)
        {
            Min = min;
            Max = max;
        }

        public override string ToString()
        {
            return $"{Min}~{Max}";
        }
    }
}