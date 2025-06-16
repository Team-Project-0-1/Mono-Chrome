namespace MonoChrome
{
    /// <summary>
    /// 모든 캐릭터(플레이어 및 적)의 유형을 정의합니다.
    /// </summary>
    public enum CharacterType
    {
        None = 0,
        Player = 1,    // 플레이어
        Normal = 2,    // 일반 적
        Elite = 3,     // 엘리트 적
        MiniBoss = 4,  // 미니보스
        Boss = 5,      // 보스
        Windows = 6,   // Windows 환경
        Neutral = 7,   // 중립 NPC
        Vendor = 8     // 상인
    }
    
    /// <summary>
    /// 감각 유형을 정의합니다.
    /// </summary>
    public enum SenseType
    {
        None = -1,     // 없음
        Auditory = 0,  // 청각
        Olfactory = 1, // 후각
        Tactile = 2,   // 촉각
        Spiritual = 3  // 영적
    }
    
    /// <summary>
    /// 패턴(족보) 유형을 정의합니다.
    /// </summary>
    public enum PatternType
    {
        None = -1,         // 없음
        Consecutive2 = 0,  // 2연속
        Consecutive3 = 1,  // 3연속
        Consecutive4 = 2,  // 4연속
        Consecutive5 = 3,  // 5연속
        AllOfOne = 4,      // 모두 같은 면
        Alternating = 5    // 교대 패턴
    }
    
    /// <summary>
    /// 상태 효과 유형을 정의합니다.
    /// </summary>
    public enum StatusEffectType
    {
        None = 0,          // 효과 없음
        Amplify = 1,       // 증폭: 공격력/방어력 증가
        Resonance = 2,     // 공명: 일정 턴 후 누적 피해
        Mark = 3,          // 표식: 추가 공격 유발
        Bleed = 4,         // 출혈: 방어력 무시 지속 피해
        Counter = 5,       // 반격: 피격시 반격 피해
        Crush = 6,         // 분쇄: 방어력 감소
        Curse = 7,         // 저주: 디버프 효과 증가
        Seal = 8,          // 봉인: 동전 봉인
        Poison = 9,        // 중독: 지속 피해
        Burn = 10,         // 화상: 즉시 피해
        MultiAttack = 11,  // 연격: 연속 공격
        Wound = 12,        // 상처: 받는 피해량 증가
        Fracture = 13,     // 골절: 공격력 감소
        Regeneration = 14  // 재생: 체력 회복
    }
    
    /// <summary>
    /// 코인 면 유형을 정의합니다.
    /// </summary>
    public enum CoinFace
    {
        None = -1,
        Head = 0,  // 앞면 (공격)
        Tail = 1   // 뒷면 (방어)
    }
    
    /// <summary>
    /// 액티브 스킬 유형을 정의합니다.
    /// </summary>
    public enum ActiveSkillType
    {
        None = 0,
        RethrowAll = 1,   // 모든 동전 재던지기
        FlipOne = 2,     // 동전 1개 뒤집기
        LockOne = 3,     // 동전 1개 고정
        SwapTwo = 4      // 동전 2개 교환
    }
    
    /// <summary>
    /// 플레이어 캐릭터 유형을 정의합니다.
    /// </summary>
    public enum PlayerCharacterType
    {
        Warrior = 0,  // 전사: 증폭 + 공명
        Rogue = 1,    // 도적: 표식 + 출혈
        Mage = 2,     // 마법사: 저주 + 봉인
        Tank = 3      // 탱커: 반격 + 분쇄
    }
    
    /// <summary>
    /// 룸 유형을 정의합니다.
    /// </summary>
    public enum RoomType
    {
        None = 0,
        Combat = 1,    // 전투
        Event = 2,     // 이벤트
        Shop = 3,      // 상점
        Rest = 4,      // 휴식
        MiniBoss = 5,  // 미니보스
        Boss = 6       // 보스
    }
    
    /// <summary>
    /// 던전 노드 유형을 정의합니다.
    /// </summary>
    public enum NodeType
    {
        None = 0,
        Combat = 1,    // 전투
        Event = 2,     // 이벤트
        Shop = 3,      // 상점
        Rest = 4,      // 휴식
        MiniBoss = 5,  // 미니보스
        Boss = 6       // 보스
    }
    
    /// <summary>
    /// AI 성격 유형을 정의합니다.
    /// </summary>
    public enum AIPersonality
    {
        Balanced = 0,     // 균형: 기본 AI
        Aggressive = 1,   // 공격적: 공격 패턴 선호
        Defensive = 2,    // 방어적: 방어 패턴 선호
        Strategic = 3,    // 전략적: 상황 판단 우선
        Chaotic = 4       // 혼돈: 예측 불가능한 행동
    }
    
    /// <summary>
    /// 몬스터 의도 유형을 정의합니다.
    /// </summary>
    public enum IntentType
    {
        None = 0,
        Attack = 1,       // 공격
        Defend = 2,       // 방어
        Buff = 3,         // 버프
        Debuff = 4,       // 디버프
        Special = 5,      // 특수 행동
        Ultimate = 6      // 궁극기
    }
    
    /// <summary>
    /// 전투 페이즈를 정의합니다.
    /// </summary>
    public enum BattlePhase
    {
        Phase1 = 1,       // 1페이즈 (100-75% 체력)
        Phase2 = 2,       // 2페이즈 (75-50% 체력)
        Phase3 = 3,       // 3페이즈 (50-25% 체력)
        Final = 4         // 최종 페이즈 (25-0% 체력)
    }
}
