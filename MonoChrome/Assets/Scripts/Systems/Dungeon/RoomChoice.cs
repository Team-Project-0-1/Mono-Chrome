using System;
using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 턴별 방 선택지 데이터 구조
    /// 감각 기반 힌트 시스템과 방 정보를 포함
    /// </summary>
    [Serializable]
    public class RoomChoice
    {
        [Header("방 기본 정보")]
        public NodeType Type;
        public int TurnNumber;
        
        [Header("감각 기반 힌트")]
        public string SensoryHint;              // 플레이어 감각에 따른 힌트
        public string AccurateDescription;      // 정확한 설명 (디버그용)
        
        [Header("추가 데이터")]
        public float DifficultyModifier = 1.0f; // 난이도 조정값
        public string EventId = "";             // 이벤트인 경우 이벤트 ID
        public float SuccessRate = 0.5f;        // 기본 성공률 (이벤트용)
        public float SenseBonus = 0.0f;         // 감각 일치 시 보너스
        
        [Header("UI 표시 정보")]
        public Color HintColor = Color.white;   // 힌트 텍스트 색상
        public Sprite RoomIcon;                 // 방 아이콘 (선택사항)
        public bool ShowSuccessRate = false;    // 성공률 표시 여부

        public RoomChoice()
        {
            Type = NodeType.Combat;
            SensoryHint = "알 수 없는 기운이 느껴진다";
            AccurateDescription = "미지의 방";
        }

        public RoomChoice(NodeType type, int turnNumber)
        {
            Type = type;
            TurnNumber = turnNumber;
            SensoryHint = GetDefaultHint(type);
            AccurateDescription = GetDefaultDescription(type);
            HintColor = GetDefaultColor(type);
        }

        public RoomChoice(NodeType type, int turnNumber, string sensoryHint, string accurateDescription)
        {
            Type = type;
            TurnNumber = turnNumber;
            SensoryHint = sensoryHint;
            AccurateDescription = accurateDescription;
            HintColor = GetDefaultColor(type);
        }

        /// <summary>
        /// 방 타입에 따른 기본 힌트 반환
        /// </summary>
        private string GetDefaultHint(NodeType type)
        {
            return type switch
            {
                NodeType.Combat => "무언가 위험한 기운이 느껴진다",
                NodeType.Event => "이상한 기운이 감지된다",
                NodeType.Shop => "누군가의 존재가 느껴진다",
                NodeType.Rest => "평화로운 기운이 느껴진다",
                NodeType.MiniBoss => "강력한 적대감이 느껴진다",
                NodeType.Boss => "압도적인 어둠의 기운이 느껴진다",
                _ => "알 수 없는 기운이 느껴진다"
            };
        }

        /// <summary>
        /// 방 타입에 따른 기본 설명 반환
        /// </summary>
        private string GetDefaultDescription(NodeType type)
        {
            return type switch
            {
                NodeType.Combat => "전투방",
                NodeType.Event => "이벤트방",
                NodeType.Shop => "상점",
                NodeType.Rest => "휴식처",
                NodeType.MiniBoss => "미니보스방",
                NodeType.Boss => "보스방",
                _ => "미지의 방"
            };
        }

        /// <summary>
        /// 방 타입에 따른 기본 색상 반환
        /// </summary>
        private Color GetDefaultColor(NodeType type)
        {
            return type switch
            {
                NodeType.Combat => new Color(1f, 0.3f, 0.3f),      // 붉은색
                NodeType.Event => new Color(1f, 1f, 0.3f),         // 노란색
                NodeType.Shop => new Color(0.3f, 1f, 0.3f),        // 초록색
                NodeType.Rest => new Color(0.3f, 0.3f, 1f),        // 파란색
                NodeType.MiniBoss => new Color(1f, 0.5f, 0f),      // 주황색
                NodeType.Boss => new Color(0.5f, 0f, 0.5f),        // 보라색
                _ => Color.white
            };
        }

        /// <summary>
        /// 이 선택지가 유효한지 확인
        /// </summary>
        public bool IsValid()
        {
            return Type != NodeType.None && 
                   !string.IsNullOrEmpty(SensoryHint) && 
                   !string.IsNullOrEmpty(AccurateDescription);
        }

        /// <summary>
        /// 플레이어 감각에 따른 최종 성공률 계산
        /// </summary>
        public float GetFinalSuccessRate(SenseType playerSense)
        {
            if (Type != NodeType.Event) return 1.0f; // 이벤트가 아니면 항상 성공
            
            float finalRate = SuccessRate;
            
            // 감각 일치 시 보너스 적용
            if (SenseBonus > 0.0f && IsMatchingSense(playerSense))
            {
                finalRate += SenseBonus;
            }
            
            // 0~1 범위로 제한
            return Mathf.Clamp01(finalRate);
        }

        /// <summary>
        /// 플레이어 감각과 이벤트가 일치하는지 확인
        /// </summary>
        private bool IsMatchingSense(SenseType playerSense)
        {
            // 이벤트 ID나 힌트 내용을 기반으로 감각 일치 여부 판단
            // 추후 더 정교한 매칭 로직으로 확장 가능
            return playerSense != SenseType.None;
        }

        /// <summary>
        /// 성공률을 백분율 문자열로 반환
        /// </summary>
        public string GetSuccessRateText(SenseType playerSense)
        {
            float rate = GetFinalSuccessRate(playerSense);
            return $"{rate * 100:F0}%";
        }

        /// <summary>
        /// 감각 보너스 적용 여부 확인
        /// </summary>
        public bool HasSenseBonus(SenseType playerSense)
        {
            return SenseBonus > 0.0f && IsMatchingSense(playerSense);
        }

        /// <summary>
        /// 디버그용 문자열 반환
        /// </summary>
        public override string ToString()
        {
            string rateInfo = Type == NodeType.Event ? $" (성공률: {SuccessRate * 100:F0}%)" : "";
            return $"RoomChoice[{TurnNumber}턴]: {Type}{rateInfo} - {SensoryHint}";
        }
    }
}