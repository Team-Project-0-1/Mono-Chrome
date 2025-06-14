using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 조건부 이벤트 관리 시스템
    /// 턴 수, 캐릭터 조건, 선택 이력에 따른 이벤트 필터링
    /// </summary>
    [Serializable]
    public class ConditionalEventManager
    {
        [Header("이벤트 시스템 설정")]
        public bool EnableDebugLogs = true;
        
        private Dictionary<string, EventCondition> _eventConditions;
        private List<string> _completedEvents;
        
        public ConditionalEventManager()
        {
            InitializeEventConditions();
            _completedEvents = new List<string>();
        }

        /// <summary>
        /// 이벤트 조건들 초기화
        /// </summary>
        private void InitializeEventConditions()
        {
            _eventConditions = new Dictionary<string, EventCondition>
            {
                ["첫_상점_방문"] = new EventCondition
                {
                    EventId = "첫_상점_방문",
                    Description = "첫 번째 상점 방문 시 특별 할인",
                    MinTurn = 1,
                    MaxTurn = 15,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType> { NodeType.Shop },
                    MaxOccurrences = 1,
                    Weight = 1.0f
                },
                
                ["전투_연속_3회"] = new EventCondition
                {
                    EventId = "전투_연속_3회",
                    Description = "전투를 3번 연속으로 선택했을 때 피로도 증가",
                    MinTurn = 4,
                    MaxTurn = 15,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredConsecutiveChoices = NodeType.Combat,
                    ConsecutiveCount = 3,
                    MaxOccurrences = 2,
                    Weight = 0.8f
                },
                
                ["미니보스_후_휴식"] = new EventCondition
                {
                    EventId = "미니보스_후_휴식",
                    Description = "미니보스 처치 후 휴식 시 특별 회복",
                    MinTurn = 6,
                    MaxTurn = 15,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType> { NodeType.MiniBoss, NodeType.Rest },
                    MaxOccurrences = 1,
                    Weight = 1.2f
                },
                
                ["청각_특화_이벤트"] = new EventCondition
                {
                    EventId = "청각_특화_이벤트",
                    Description = "청각 캐릭터 전용 숨겨진 소리 발견",
                    MinTurn = 3,
                    MaxTurn = 12,
                    RequiredSenseTypes = new List<SenseType> { SenseType.Auditory },
                    RequiredPreviousChoices = new List<NodeType> { NodeType.Event },
                    MaxOccurrences = 2,
                    Weight = 1.5f
                },
                
                ["후각_특화_이벤트"] = new EventCondition
                {
                    EventId = "후각_특화_이벤트",
                    Description = "후각 캐릭터 전용 숨겨진 향기 추적",
                    MinTurn = 3,
                    MaxTurn = 12,
                    RequiredSenseTypes = new List<SenseType> { SenseType.Olfactory },
                    RequiredPreviousChoices = new List<NodeType> { NodeType.Event },
                    MaxOccurrences = 2,
                    Weight = 1.5f
                },
                
                ["영적_보스_예감"] = new EventCondition
                {
                    EventId = "영적_보스_예감",
                    Description = "영적 감각으로 보스의 기운을 미리 감지",
                    MinTurn = 13,
                    MaxTurn = 14,
                    RequiredSenseTypes = new List<SenseType> { SenseType.Spiritual },
                    RequiredPreviousChoices = new List<NodeType>(),
                    MaxOccurrences = 1,
                    Weight = 2.0f
                },
                
                ["균형잡힌_탐험"] = new EventCondition
                {
                    EventId = "균형잡힌_탐험",
                    Description = "모든 타입의 방을 골고루 방문했을 때 보너스",
                    MinTurn = 8,
                    MaxTurn = 15,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredDiverseChoices = true,
                    MaxOccurrences = 1,
                    Weight = 1.3f
                },
                
                // 설계서 기반 스테이지별 테마 이벤트들
                ["벙커_외곽_돌연변이"] = new EventCondition
                {
                    EventId = "벙커_외곽_돌연변이",
                    Description = "벙커 외곽에서 돌연변이 흔적을 발견했다",
                    MinTurn = 1,
                    MaxTurn = 5,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType>(),
                    MaxOccurrences = 1,
                    Weight = 1.0f
                },
                
                ["중립지대_포식자"] = new EventCondition
                {
                    EventId = "중립지대_포식자",
                    Description = "빛을 먹은 포식자의 기운이 감지된다",
                    MinTurn = 6,
                    MaxTurn = 10,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType>(),
                    MaxOccurrences = 1,
                    Weight = 1.2f
                },
                
                ["심층_숙주_징조"] = new EventCondition
                {
                    EventId = "심층_숙주_징조",
                    Description = "숙주의 영향력이 강해지고 있다",
                    MinTurn = 11,
                    MaxTurn = 15,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType>(),
                    MaxOccurrences = 1,
                    Weight = 1.3f
                },
                
                ["불길한_예감_이벤트"] = new EventCondition
                {
                    EventId = "불길한_예감_이벤트",
                    Description = "왠지 불길한 예감이 든다. 이 곳을 잠시 탐색할까?",
                    MinTurn = 2,
                    MaxTurn = 14,
                    RequiredSenseTypes = new List<SenseType>(),
                    RequiredPreviousChoices = new List<NodeType>(),
                    MaxOccurrences = 3,
                    Weight = 1.0f
                }
            };
        }

        /// <summary>
        /// 현재 조건에 맞는 이벤트들 필터링
        /// </summary>
        public List<EventData> GetAvailableEvents(
            int currentTurn, 
            SenseType playerSense, 
            List<NodeType> playerChoices)
        {
            var availableEvents = new List<EventData>();
            
            LogDebug($"이벤트 필터링: {currentTurn}턴, {playerSense} 감각, {playerChoices.Count}개 선택 기록");

            foreach (var condition in _eventConditions.Values)
            {
                if (IsEventAvailable(condition, currentTurn, playerSense, playerChoices))
                {
                    var eventData = CreateEventData(condition);
                    availableEvents.Add(eventData);
                    LogDebug($"사용 가능한 이벤트: {condition.EventId}");
                }
            }

            // 가중치에 따라 정렬
            availableEvents = availableEvents.OrderByDescending(e => e.Weight).ToList();
            
            LogDebug($"총 {availableEvents.Count}개 이벤트 사용 가능");
            return availableEvents;
        }

        /// <summary>
        /// 특정 이벤트가 현재 조건에서 사용 가능한지 확인
        /// </summary>
        private bool IsEventAvailable(
            EventCondition condition, 
            int currentTurn, 
            SenseType playerSense, 
            List<NodeType> playerChoices)
        {
            // 1. 턴 범위 확인
            if (currentTurn < condition.MinTurn || currentTurn > condition.MaxTurn)
            {
                return false;
            }

            // 2. 최대 발생 횟수 확인
            int occurrenceCount = _completedEvents.Count(id => id == condition.EventId);
            if (occurrenceCount >= condition.MaxOccurrences)
            {
                return false;
            }

            // 3. 감각 타입 확인
            if (condition.RequiredSenseTypes.Count > 0 && 
                !condition.RequiredSenseTypes.Contains(playerSense))
            {
                return false;
            }

            // 4. 필수 선택 이력 확인
            if (condition.RequiredPreviousChoices.Count > 0)
            {
                if (!HasRequiredChoices(playerChoices, condition.RequiredPreviousChoices))
                {
                    return false;
                }
            }

            // 5. 연속 선택 확인
            if (condition.RequiredConsecutiveChoices != NodeType.None)
            {
                if (!HasConsecutiveChoices(playerChoices, condition.RequiredConsecutiveChoices, condition.ConsecutiveCount))
                {
                    return false;
                }
            }

            // 6. 다양성 선택 확인
            if (condition.RequiredDiverseChoices)
            {
                if (!HasDiverseChoices(playerChoices))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 필수 선택 이력이 있는지 확인
        /// </summary>
        private bool HasRequiredChoices(List<NodeType> playerChoices, List<NodeType> requiredChoices)
        {
            foreach (var required in requiredChoices)
            {
                if (!playerChoices.Contains(required))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 연속 선택이 있는지 확인
        /// </summary>
        private bool HasConsecutiveChoices(List<NodeType> playerChoices, NodeType requiredType, int consecutiveCount)
        {
            if (playerChoices.Count < consecutiveCount)
                return false;

            int currentConsecutive = 0;
            int maxConsecutive = 0;

            foreach (var choice in playerChoices)
            {
                if (choice == requiredType)
                {
                    currentConsecutive++;
                    maxConsecutive = Math.Max(maxConsecutive, currentConsecutive);
                }
                else
                {
                    currentConsecutive = 0;
                }
            }

            return maxConsecutive >= consecutiveCount;
        }

        /// <summary>
        /// 다양한 선택을 했는지 확인
        /// </summary>
        private bool HasDiverseChoices(List<NodeType> playerChoices)
        {
            var uniqueTypes = playerChoices.Distinct().ToList();
            
            // 전투, 이벤트, 상점, 휴식 중 최소 3가지 이상
            var basicTypes = new[] { NodeType.Combat, NodeType.Event, NodeType.Shop, NodeType.Rest };
            int diversityCount = uniqueTypes.Count(type => basicTypes.Contains(type));
            
            return diversityCount >= 3;
        }

        /// <summary>
        /// 조건에서 이벤트 데이터 생성
        /// </summary>
        private EventData CreateEventData(EventCondition condition)
        {
            return new EventData
            {
                Id = condition.EventId,
                Description = condition.Description,
                Weight = condition.Weight
            };
        }

        /// <summary>
        /// 이벤트 완료 기록
        /// </summary>
        public void RecordEventCompletion(string eventId)
        {
            _completedEvents.Add(eventId);
            LogDebug($"이벤트 완료 기록: {eventId}");
        }

        /// <summary>
        /// 완료된 이벤트 목록 초기화
        /// </summary>
        public void ResetCompletedEvents()
        {
            _completedEvents.Clear();
            LogDebug("완료된 이벤트 목록 초기화");
        }

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[ConditionalEventManager] {message}");
            }
        }
    }

    /// <summary>
    /// 이벤트 발생 조건
    /// </summary>
    [Serializable]
    public class EventCondition
    {
        public string EventId;
        public string Description;
        public int MinTurn = 1;
        public int MaxTurn = 15;
        public List<SenseType> RequiredSenseTypes = new List<SenseType>();
        public List<NodeType> RequiredPreviousChoices = new List<NodeType>();
        public NodeType RequiredConsecutiveChoices = NodeType.None;
        public int ConsecutiveCount = 1;
        public bool RequiredDiverseChoices = false;
        public int MaxOccurrences = 1;
        public float Weight = 1.0f;
    }

    /// <summary>
    /// 이벤트 데이터
    /// </summary>
    [Serializable]
    public class EventData
    {
        public string Id;
        public string Description;
        public float Weight;
        
        public override string ToString()
        {
            return $"EventData[{Id}]: {Description} (가중치: {Weight})";
        }
    }
}