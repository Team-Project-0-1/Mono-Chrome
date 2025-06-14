using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 턴 기반 던전 진행 데이터
    /// 현재 상태, 선택 기록, 타입 사용량 추적
    /// </summary>
    [Serializable]
    public class TurnBasedDungeonData
    {
        [Header("던전 진행 상태")]
        public int CurrentTurn = 1;
        public int MaxTurns = 15;
        
        [Header("선택 기록")]
        public List<NodeType> SelectedPath = new List<NodeType>();
        public List<int> SelectedTurns = new List<int>();
        
        [Header("타입 사용량")]
        public Dictionary<NodeType, int> TypeUsageCount = new Dictionary<NodeType, int>();
        
        [Header("게임 상태")]
        public bool IsCompleted = false;
        public DateTime StartTime;
        public DateTime LastChoiceTime;

        public TurnBasedDungeonData()
        {
            Initialize(15);
        }

        public TurnBasedDungeonData(int maxTurns)
        {
            Initialize(maxTurns);
        }

        private void Initialize(int maxTurns)
        {
            MaxTurns = maxTurns;
            Reset();
        }

        /// <summary>
        /// 던전 데이터 초기화
        /// </summary>
        public void Reset()
        {
            CurrentTurn = 1;
            IsCompleted = false;
            StartTime = DateTime.Now;
            LastChoiceTime = DateTime.Now;
            
            SelectedPath.Clear();
            SelectedTurns.Clear();
            
            // 타입 사용량 초기화
            TypeUsageCount.Clear();
            foreach (NodeType type in Enum.GetValues(typeof(NodeType)))
            {
                if (type != NodeType.None)
                {
                    TypeUsageCount[type] = 0;
                }
            }
        }

        /// <summary>
        /// 플레이어 선택 기록
        /// </summary>
        public void RecordChoice(NodeType selectedType)
        {
            if (CurrentTurn > MaxTurns)
            {
                Debug.LogWarning($"최대 턴 수를 초과했습니다: {CurrentTurn}/{MaxTurns}");
                return;
            }

            SelectedPath.Add(selectedType);
            SelectedTurns.Add(CurrentTurn);
            
            if (TypeUsageCount.ContainsKey(selectedType))
            {
                TypeUsageCount[selectedType]++;
            }
            else
            {
                TypeUsageCount[selectedType] = 1;
            }

            LastChoiceTime = DateTime.Now;
            CurrentTurn++;

            if (CurrentTurn > MaxTurns)
            {
                IsCompleted = true;
            }
        }

        /// <summary>
        /// 특정 타입의 사용량 반환
        /// </summary>
        public int GetTypeUsage(NodeType type)
        {
            return TypeUsageCount.GetValueOrDefault(type, 0);
        }

        /// <summary>
        /// 특정 타입이 사용되었는지 확인
        /// </summary>
        public bool HasUsedType(NodeType type)
        {
            return GetTypeUsage(type) > 0;
        }

        /// <summary>
        /// 현재 진행률 반환 (0~100%)
        /// </summary>
        public float GetProgress()
        {
            if (MaxTurns == 0) return 100f;
            return ((float)(CurrentTurn - 1) / MaxTurns) * 100f;
        }

        /// <summary>
        /// 남은 턴 수 반환
        /// </summary>
        public int GetRemainingTurns()
        {
            return Math.Max(0, MaxTurns - CurrentTurn + 1);
        }

        /// <summary>
        /// 특정 타입을 더 선택할 수 있는지 확인
        /// </summary>
        public bool CanSelectMoreOfType(NodeType type, int maxAllowed)
        {
            return GetTypeUsage(type) < maxAllowed;
        }

        /// <summary>
        /// 던전 진행 시간 반환
        /// </summary>
        public TimeSpan GetElapsedTime()
        {
            return DateTime.Now - StartTime;
        }

        /// <summary>
        /// 마지막 선택 이후 경과 시간 반환
        /// </summary>
        public TimeSpan GetTimeSinceLastChoice()
        {
            return DateTime.Now - LastChoiceTime;
        }

        /// <summary>
        /// 선택 경로를 문자열로 반환
        /// </summary>
        public string GetPathString()
        {
            if (SelectedPath.Count == 0)
                return "선택한 경로 없음";

            return string.Join(" → ", SelectedPath.Select((type, index) => 
                $"{SelectedTurns[index]}턴:{type}"));
        }

        /// <summary>
        /// 타입별 분포를 문자열로 반환
        /// </summary>
        public string GetDistributionString()
        {
            if (TypeUsageCount.Count == 0)
                return "사용량 데이터 없음";

            var totalSelections = SelectedPath.Count;
            if (totalSelections == 0)
                return "선택 없음";

            return string.Join(", ", TypeUsageCount
                .Where(kvp => kvp.Value > 0)
                .Select(kvp => 
                {
                    float percentage = (float)kvp.Value / totalSelections * 100f;
                    return $"{kvp.Key}:{kvp.Value}({percentage:F1}%)";
                }));
        }

        /// <summary>
        /// 현재 상태의 요약 정보 반환
        /// </summary>
        public DungeonProgressSummary GetSummary()
        {
            return new DungeonProgressSummary
            {
                CurrentTurn = CurrentTurn,
                MaxTurns = MaxTurns,
                Progress = GetProgress(),
                RemainingTurns = GetRemainingTurns(),
                IsCompleted = IsCompleted,
                PathLength = SelectedPath.Count,
                ElapsedTime = GetElapsedTime(),
                LastChoiceType = SelectedPath.LastOrDefault(),
                TypeDistribution = new Dictionary<NodeType, int>(TypeUsageCount)
            };
        }

        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool IsValid()
        {
            // 기본 유효성 검사
            if (CurrentTurn < 1 || MaxTurns < 1)
                return false;

            // 선택 경로와 턴 기록의 일관성 확인
            if (SelectedPath.Count != SelectedTurns.Count)
                return false;

            // 타입 사용량과 선택 경로의 일관성 확인
            var pathTypeCounts = SelectedPath.GroupBy(type => type)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var kvp in pathTypeCounts)
            {
                if (GetTypeUsage(kvp.Key) != kvp.Value)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 디버그용 문자열 반환
        /// </summary>
        public override string ToString()
        {
            return $"TurnBasedDungeonData[{CurrentTurn}/{MaxTurns}]: " +
                   $"진행률 {GetProgress():F1}%, " +
                   $"선택 {SelectedPath.Count}개, " +
                   $"경과시간 {GetElapsedTime().TotalMinutes:F1}분";
        }
    }

    /// <summary>
    /// 던전 진행 상태 요약 정보
    /// </summary>
    [Serializable]
    public struct DungeonProgressSummary
    {
        public int CurrentTurn;
        public int MaxTurns;
        public float Progress;
        public int RemainingTurns;
        public bool IsCompleted;
        public int PathLength;
        public TimeSpan ElapsedTime;
        public NodeType LastChoiceType;
        public Dictionary<NodeType, int> TypeDistribution;

        public override string ToString()
        {
            return $"던전 진행 요약: {CurrentTurn}/{MaxTurns}턴 ({Progress:F1}%), " +
                   $"마지막 선택: {LastChoiceType}";
        }
    }
}