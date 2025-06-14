using System;
using System.Collections.Generic;
using UnityEngine;
using MonoChrome.Core;

namespace MonoChrome.Systems.Dungeon
{
    /// <summary>
    /// 감각 기반 힌트 생성 시스템
    /// 플레이어의 감각 타입에 따라 방 정보를 다르게 제공
    /// </summary>
    public class SensoryHintSystem : MonoBehaviour
    {
        [Header("힌트 시스템 설정")]
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private float hintAccuracy = 0.8f;

        // 감각별 방 타입 힌트 데이터베이스
        private Dictionary<NodeType, Dictionary<SenseType, string[]>> _senseHints;
        private Dictionary<NodeType, string[]> _genericHints;

        private void Awake()
        {
            InitializeHintDatabases();
            LogDebug("SensoryHintSystem 초기화 완료");
        }

        /// <summary>
        /// 힌트 데이터베이스들 초기화
        /// </summary>
        private void InitializeHintDatabases()
        {
            _senseHints = InitializeSenseHints();
            _genericHints = InitializeGenericHints();
        }

        /// <summary>
        /// 감각별 힌트 데이터베이스 초기화
        /// </summary>
        private Dictionary<NodeType, Dictionary<SenseType, string[]>> InitializeSenseHints()
        {
            var hints = new Dictionary<NodeType, Dictionary<SenseType, string[]>>();

            // Combat 노드 힌트
            hints[NodeType.Combat] = new Dictionary<SenseType, string[]>();
            hints[NodeType.Combat][SenseType.Auditory] = new string[]
            {
                "멀리서 금속이 부딪치는 소리가 들린다",
                "무언가 날카로운 것이 공기를 가르는 소리가 난다",
                "거친 숨소리와 발걸음 소리가 들린다",
                "날개 소리와 함께 위협적인 울음소리가 들린다"
            };
            hints[NodeType.Combat][SenseType.Olfactory] = new string[]
            {
                "피비린내가 코를 찌른다",
                "쇠 냄새와 함께 불쾌한 냄새가 난다",
                "땀과 두려움의 냄새가 섞여 있다",
                "썩은 냄새와 함께 위험한 기운이 느껴진다"
            };
            hints[NodeType.Combat][SenseType.Tactile] = new string[]
            {
                "차가운 바람이 불어와 소름이 돈다",
                "공기 중에 긴장감이 흐르고 있다",
                "피부에 닿는 공기가 날카롭게 느껴진다",
                "온도가 급격히 떨어지며 위험함이 느껴진다"
            };
            hints[NodeType.Combat][SenseType.Spiritual] = new string[]
            {
                "적대적인 기운이 강하게 감지된다",
                "살의에 찬 어두운 영혼들이 느껴진다",
                "분노와 증오의 영적 에너지가 맴돈다",
                "전투의 흔적이 남긴 어둠의 기운이 느껴진다"
            };

            // Event 노드 힌트
            hints[NodeType.Event] = new Dictionary<SenseType, string[]>();
            hints[NodeType.Event][SenseType.Auditory] = new string[]
            {
                "이상한 속삭임 소리가 들린다",
                "알 수 없는 언어의 중얼거림이 들린다",
                "기묘한 음향이 공간을 채우고 있다",
                "마법적인 윙윙거리는 소리가 들린다"
            };
            hints[NodeType.Event][SenseType.Olfactory] = new string[]
            {
                "달콤하면서도 이상한 냄새가 난다",
                "오래된 책과 향의 냄새가 섞여 있다",
                "신비로운 향기가 코끝을 스친다",
                "알 수 없는 화학적 냄새가 감돈다"
            };
            hints[NodeType.Event][SenseType.Tactile] = new string[]
            {
                "공기가 이상하게 무겁게 느껴진다",
                "정전기가 피부를 간질인다",
                "온도가 불규칙하게 변화한다",
                "마법적인 에너지가 피부에 닿는다"
            };
            hints[NodeType.Event][SenseType.Spiritual] = new string[]
            {
                "수수께끼 같은 영적 에너지가 감지된다",
                "운명이 갈라지는 지점의 기운이 느껴진다",
                "고대의 지혜가 깃든 공간의 느낌이다",
                "변화와 선택의 영적 파동이 느껴진다"
            };

            // Shop 노드 힌트
            hints[NodeType.Shop] = new Dictionary<SenseType, string[]>();
            hints[NodeType.Shop][SenseType.Auditory] = new string[]
            {
                "동전이 딸랑거리는 소리가 들린다",
                "물건들이 정리되는 소리가 난다",
                "상인의 웅성거림이 들린다",
                "거래하는 듯한 대화 소리가 들린다"
            };
            hints[NodeType.Shop][SenseType.Olfactory] = new string[]
            {
                "다양한 물건들의 냄새가 섞여 있다",
                "금속과 가죽의 냄새가 난다",
                "향신료와 기름의 냄새가 감돈다",
                "오래된 보물의 특유한 냄새가 난다"
            };
            hints[NodeType.Shop][SenseType.Tactile] = new string[]
            {
                "따뜻하고 안전한 느낌이 든다",
                "사람의 체온이 느껴진다",
                "상거래의 활기찬 기운이 전해진다",
                "풍요로움의 에너지가 느껴진다"
            };
            hints[NodeType.Shop][SenseType.Spiritual] = new string[]
            {
                "탐욕과 번영의 기운이 감지된다",
                "거래와 교환의 영적 에너지가 맴돈다",
                "물질적 풍요로움의 기운이 느껴진다",
                "상인의 영혼에서 나오는 상술의 기운이 느껴진다"
            };

            // Rest 노드 힌트
            hints[NodeType.Rest] = new Dictionary<SenseType, string[]>();
            hints[NodeType.Rest][SenseType.Auditory] = new string[]
            {
                "고요하고 평화로운 침묵이 흐른다",
                "물이 떨어지는 잔잔한 소리가 들린다",
                "바람이 부드럽게 스치는 소리가 난다",
                "새들의 지저귐이 멀리서 들린다"
            };
            hints[NodeType.Rest][SenseType.Olfactory] = new string[]
            {
                "신선하고 깨끗한 공기의 냄새가 난다",
                "꽃향기와 풀냄새가 은은하게 퍼진다",
                "치유의 허브 냄새가 감돈다",
                "숲의 상쾌한 냄새가 코를 채운다"
            };
            hints[NodeType.Rest][SenseType.Tactile] = new string[]
            {
                "따뜻하고 포근한 기운이 감싼다",
                "부드러운 에너지가 몸을 감싸준다",
                "편안함과 안정감이 느껴진다",
                "치유의 따뜻함이 온몸에 퍼진다"
            };
            hints[NodeType.Rest][SenseType.Spiritual] = new string[]
            {
                "평화롭고 조화로운 기운이 가득하다",
                "치유와 회복의 영적 에너지가 느껴진다",
                "순수하고 맑은 영혼의 기운이 감돈다",
                "안식과 평온의 신성한 기운이 느껴진다"
            };

            // MiniBoss 노드 힌트
            hints[NodeType.MiniBoss] = new Dictionary<SenseType, string[]>();
            hints[NodeType.MiniBoss][SenseType.Auditory] = new string[]
            {
                "위압적인 포효 소리가 울려퍼진다",
                "무거운 갑옷 소리와 함께 위협적인 기운이 들린다",
                "강력한 마법이 맴도는 소리가 들린다",
                "전장의 지배자가 내는 위엄 있는 소리가 난다"
            };
            hints[NodeType.MiniBoss][SenseType.Olfactory] = new string[]
            {
                "강력한 마력과 함께 위험한 냄새가 진동한다",
                "고농축된 어둠의 에너지 냄새가 코를 찌른다",
                "피와 쇠의 냄새가 압도적으로 강하다",
                "죽음과 파괴의 냄새가 공기를 가득 채운다"
            };
            hints[NodeType.MiniBoss][SenseType.Tactile] = new string[]
            {
                "압도적인 압박감이 온몸을 누른다",
                "공기 자체가 무겁고 위험하게 느껴진다",
                "강력한 기운이 피부를 압박한다",
                "전투 본능이 경고하는 긴장감이 느껴진다"
            };
            hints[NodeType.MiniBoss][SenseType.Spiritual] = new string[]
            {
                "강력하고 악의에 찬 영혼이 감지된다",
                "압도적인 어둠의 영적 에너지가 맴돈다",
                "절망과 공포를 불러일으키는 기운이 느껴진다",
                "미니보스 급 존재의 어둠의 기운이 감지된다"
            };

            // Boss 노드 힌트
            hints[NodeType.Boss] = new Dictionary<SenseType, string[]>();
            hints[NodeType.Boss][SenseType.Auditory] = new string[]
            {
                "현실을 뒤흔드는 절대적인 권능의 소리가 들린다",
                "모든 소리를 집어삼키는 절대적 침묵이 흐른다",
                "세상 끝의 어둠이 울려퍼지는 소리가 들린다",
                "절망 그 자체가 내는 마지막 울음소리가 들린다"
            };
            hints[NodeType.Boss][SenseType.Olfactory] = new string[]
            {
                "절대적 어둠의 냄새가 존재 자체를 압도한다",
                "종말과 절망의 냄새가 모든 것을 집어삼킨다",
                "현실이 무너지는 부패의 냄새가 진동한다",
                "생명 자체를 거부하는 죽음의 냄새가 가득하다"
            };
            hints[NodeType.Boss][SenseType.Tactile] = new string[]
            {
                "존재 자체가 무너질 것 같은 절대적 압박감이 느껴진다",
                "현실이 왜곡되는 듯한 이상한 감각이 온몸을 덮는다",
                "절대적 추위가 영혼까지 얼려버릴 것 같다",
                "모든 감각이 마비될 정도의 압도적 기운이 느껴진다"
            };
            hints[NodeType.Boss][SenseType.Spiritual] = new string[]
            {
                "절대적 어둠의 존재가 모든 것을 압도하고 있다",
                "절망과 종말의 영적 에너지가 현실을 뒤덮는다",
                "모든 빛을 집어삼키는 검은 심연의 기운이 느껴진다",
                "최종 보스의 압도적 어둠이 영혼을 짓누른다"
            };

            return hints;
        }

        /// <summary>
        /// 일반적인 힌트 데이터베이스 초기화 (감각이 맞지 않을 때)
        /// </summary>
        private Dictionary<NodeType, string[]> InitializeGenericHints()
        {
            var genericHints = new Dictionary<NodeType, string[]>();

            genericHints[NodeType.Combat] = new string[]
            {
                "뭔가 위험한 기운이 느껴진다",
                "전투의 흔적이 감지된다",
                "적대적인 존재가 있는 것 같다"
            };

            genericHints[NodeType.Event] = new string[]
            {
                "뭔가 이상한 기운이 느껴진다",
                "특별한 일이 일어날 것 같다",
                "수상한 에너지가 감지된다"
            };

            genericHints[NodeType.Shop] = new string[]
            {
                "누군가의 존재가 느껴진다",
                "거래할 수 있을 것 같다",
                "물건들이 있는 것 같다"
            };

            genericHints[NodeType.Rest] = new string[]
            {
                "평화로운 기운이 느껴진다",
                "휴식을 취할 수 있을 것 같다",
                "안전한 장소인 것 같다"
            };

            genericHints[NodeType.MiniBoss] = new string[]
            {
                "강력한 적의 기운이 느껴진다",
                "위험한 존재가 있는 것 같다",
                "압도적인 기운이 감지된다"
            };

            genericHints[NodeType.Boss] = new string[]
            {
                "절대적 어둠의 기운이 느껴진다",
                "최종 결전의 무대가 기다리고 있다",
                "운명의 순간이 다가오고 있다"
            };

            return genericHints;
        }

        /// <summary>
        /// 감각 기반 힌트 생성
        /// </summary>
        public string GenerateHint(NodeType roomType, SenseType playerSense)
        {
            if (_senseHints == null || _genericHints == null)
            {
                LogDebug("힌트 데이터베이스가 초기화되지 않음");
                InitializeHintDatabases();
            }

            LogDebug($"힌트 생성: {roomType} 방, {playerSense} 감각");

            // 정확한 감각 힌트 시도
            if (_senseHints.ContainsKey(roomType) && 
                _senseHints[roomType].ContainsKey(playerSense))
            {
                var senseSpecificHints = _senseHints[roomType][playerSense];
                if (senseSpecificHints.Length > 0)
                {
                    var hint = senseSpecificHints[UnityEngine.Random.Range(0, senseSpecificHints.Length)];
                    LogDebug($"감각별 힌트 생성: {hint}");
                    return hint;
                }
            }

            // 일반 힌트로 대체
            if (_genericHints.ContainsKey(roomType))
            {
                var genericHints = _genericHints[roomType];
                if (genericHints.Length > 0)
                {
                    var hint = genericHints[UnityEngine.Random.Range(0, genericHints.Length)];
                    LogDebug($"일반 힌트 생성: {hint}");
                    return hint;
                }
            }

            // 최종 기본값
            string fallbackHint = "알 수 없는 기운이 느껴진다";
            LogDebug($"기본 힌트 사용: {fallbackHint}");
            return fallbackHint;
        }

        /// <summary>
        /// 감각 타입에 따른 색상 반환
        /// </summary>
        public Color GetSenseColor(SenseType senseType)
        {
            return senseType switch
            {
                SenseType.Auditory => new Color(0.8f, 0.8f, 1f),    // 연한 파란색
                SenseType.Olfactory => new Color(1f, 0.8f, 0.8f),   // 연한 빨간색
                SenseType.Tactile => new Color(0.8f, 1f, 0.8f),     // 연한 초록색
                SenseType.Spiritual => new Color(1f, 0.8f, 1f),     // 연한 보라색
                _ => Color.white
            };
        }

        /// <summary>
        /// 감각별 힌트 아이콘 반환
        /// </summary>
        public string GetSenseIcon(SenseType senseType)
        {
            return senseType switch
            {
                SenseType.Auditory => "👂",   // 귀
                SenseType.Olfactory => "👃",  // 코
                SenseType.Tactile => "✋",     // 손
                SenseType.Spiritual => "👁️",   // 눈
                _ => "❓"
            };
        }

        /// <summary>
        /// 감각 타입의 한국어 이름 반환
        /// </summary>
        public string GetSenseName(SenseType senseType)
        {
            return senseType switch
            {
                SenseType.Auditory => "청각",
                SenseType.Olfactory => "후각",
                SenseType.Tactile => "촉각",
                SenseType.Spiritual => "영적",
                _ => "알 수 없음"
            };
        }

        /// <summary>
        /// 힌트 품질 평가 (정확도 기반)
        /// </summary>
        public HintQuality EvaluateHintQuality(NodeType roomType, SenseType playerSense)
        {
            if (_senseHints == null || _genericHints == null)
            {
                return HintQuality.Poor;
            }

            if (_senseHints.ContainsKey(roomType) && 
                _senseHints[roomType].ContainsKey(playerSense))
            {
                return HintQuality.Accurate; // 정확한 감각 힌트
            }
            else if (_genericHints.ContainsKey(roomType))
            {
                return HintQuality.Vague; // 모호한 힌트
            }
            else
            {
                return HintQuality.Poor; // 부정확한 힌트
            }
        }

        private void LogDebug(string message)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"[SensoryHintSystem] {message}");
            }
        }
    }

    /// <summary>
    /// 힌트 품질 등급
    /// </summary>
    public enum HintQuality
    {
        Poor = 0,      // 부정확
        Vague = 1,     // 모호함
        Accurate = 2   // 정확함
    }
}