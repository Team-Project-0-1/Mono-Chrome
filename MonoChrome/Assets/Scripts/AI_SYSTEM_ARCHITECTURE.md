# MONOCHROME: the Eclipse - AI 시스템 아키텍처 가이드

## 📋 개요

본 문서는 MONOCHROME: the Eclipse의 AI 시스템 전체 아키텍처와 사용법을 설명합니다.
동전 기반 확률 전투 시스템에 특화된 고급 AI 시스템으로, 몬스터별 개성화된 행동 패턴과 의도 표시 시스템을 제공합니다.

## 🏗️ 시스템 아키텍처

### 핵심 컴포넌트

```
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   AIManager     │◄──►│PatternDataManager│◄──►│ MonsterPatternSO│
│  (싱글톤)       │    │    (데이터)       │    │   (데이터)      │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         ▲                        ▲
         │                        │
         ▼                        ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   MonsterAI     │    │   TurnManager    │◄──►│IntentDisplaySys │
│ (개별 몬스터)   │    │   (턴 관리)      │    │  (UI 표시)     │
└─────────────────┘    └──────────────────┘    └─────────────────┘
         │                        │                       │
         ▼                        ▼                       ▼
┌─────────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Character     │    │   CombatManager  │    │    IntentUI     │
│   (몬스터)      │    │   (전투 실행)    │    │  (개별 UI)     │
└─────────────────┘    └──────────────────┘    └─────────────────┘
```

## 🎯 주요 구성 요소

### 1. AIManager (Core AI System)
- **위치**: `Scripts/AI/AIManager.cs`
- **역할**: 전체 AI 시스템 관리, 패턴 선택 로직
- **특징**: 
  - 싱글톤 패턴
  - MonsterPatternSO 기반 의사결정
  - 몬스터 타입별 차별화된 AI 로직
  - 의도 캐싱 시스템

```csharp
// 사용 예시
MonsterPatternSO intent = AIManager.Instance.DetermineIntent(monster, player);
MonsterPatternSO currentIntent = AIManager.Instance.GetCurrentIntent(monster);
```

### 2. MonsterAI (Individual Monster AI)
- **위치**: `Scripts/AI/MonsterAI.cs`
- **역할**: 개별 몬스터의 고유 특성 관리
- **특징**:
  - Character 컴포넌트 필수
  - AI 성격 시스템 (Aggressive, Defensive, Strategic, 등)
  - 페이즈 전환 관리
  - 분노 모드 시스템

```csharp
// MonsterAI 설정
MonsterAI monsterAI = enemy.GetComponent<MonsterAI>();
MonsterPatternSO decision = monsterAI.DecideAction(player);
bool isEnraged = monsterAI.IsEnraged();
int currentPhase = monsterAI.GetCurrentPhase();
```

### 3. IntentDisplaySystem (UI Intent System)
- **위치**: `Scripts/Systems/UI/IntentDisplaySystem.cs`
- **역할**: 몬스터 의도 UI 표시 관리
- **특징**:
  - AIManager와 연동
  - 실시간 의도 업데이트
  - 툴팁 시스템
  - 애니메이션 지원

```csharp
// 의도 표시
intentDisplay.DisplayIntent(monster, player);
intentDisplay.DisplayAllIntents(monsters, player);
intentDisplay.RefreshIntent(monster);
```

### 4. TurnManager (Turn Management)
- **위치**: `Scripts/Systems/Combat/TurnManager.cs`
- **역할**: 전투 턴 및 페이즈 관리
- **특징**:
  - 3단계 턴 프로세스 (Planning → Execution → Resolution)
  - 페이즈 전환 관리
  - 전투 흐름 제어
  - 이벤트 기반 알림

```csharp
// 전투 관리
turnManager.StartBattle(player, enemies);
int currentTurn = turnManager.GetCurrentTurn();
TurnPhase phase = turnManager.GetCurrentPhase();
turnManager.EndBattle(playerWon);
```

### 5. PatternDataManager (Data Management)
- **위치**: `Scripts/ScriptableObjects/PatternDataManager.cs`
- **역할**: 패턴 데이터 중앙 관리
- **특징**:
  - MonsterPatternSO와 PatternSO 모두 지원
  - 타입별 패턴 캐싱
  - 동적 패턴 로딩
  - Resources 폴더 자동 스캔

```csharp
// 패턴 데이터 접근
var allPatterns = PatternDataManager.Instance.GetAllMonsterPatterns();
var typePatterns = PatternDataManager.Instance.GetMonsterPatternsForType("루멘 리퍼");
var intentPatterns = PatternDataManager.Instance.GetMonsterPatternsByIntent("공격");
```

## 🔧 설정 및 사용법

### 1. 기본 설정

#### 1.1 PatternDataManager 설정
1. Resources 폴더에 PatternDataManager 에셋 생성
2. MonsterPatternSO 배열에 생성된 패턴들 할당
3. Initialize() 호출로 캐시 구축

#### 1.2 몬스터 설정
```csharp
// 몬스터 GameObject에 필요한 컴포넌트들
Character character = monster.GetComponent<Character>();
MonsterAI ai = monster.AddComponent<MonsterAI>(); // 자동 설정됨

// AI 특성 조정 (선택적)
ai.ModifyCharacteristics(aggressionDelta: 2, cautionDelta: -1, intelligenceDelta: 1);
```

#### 1.3 의도 표시 UI 설정
```csharp
// IntentDisplaySystem 설정
IntentDisplaySystem intentSystem = GameObject.FindObjectOfType<IntentDisplaySystem>();

// Intent UI 프리팹 준비 (IntentUI 컴포넌트 포함)
// 필요한 UI 요소: Image (아이콘), TextMeshProUGUI (수치), CanvasGroup (애니메이션)
```

### 2. 전투 시작 시퀀스

```csharp
// 1. 시스템 초기화
PatternDataManager.Instance.Initialize();
AIManager.Instance; // 싱글톤 초기화

// 2. 전투 참여자 설정
List<Character> enemies = GetEnemies();
Character player = GetPlayer();

// 3. 전투 시작
TurnManager turnManager = FindObjectOfType<TurnManager>();
turnManager.StartBattle(player, enemies);

// 4. 의도 표시 시스템 준비
IntentDisplaySystem intentDisplay = FindObjectOfType<IntentDisplaySystem>();
intentDisplay.OnBattleStart();
```

### 3. 턴 진행 흐름

```
[턴 시작]
    ↓
[계획 단계] - 모든 몬스터의 의도 결정
    ↓
[실행 단계] - 플레이어 및 몬스터 행동 실행
    ↓
[해결 단계] - 상태 효과 처리, 페이즈 확인
    ↓
[턴 종료] - 다음 턴으로 전환 또는 전투 종료
```

## 🎮 AI 행동 로직

### 몬스터 타입별 AI 전략

#### Normal (일반 몬스터)
- 단순한 체력 기반 판단
- 70% 공격, 30% 방어 선호
- 체력 30% 이하 시 방어 우선

#### Elite (엘리트 몬스터)  
- 플레이어 상태 고려
- 4턴마다 특수 패턴
- 상황 적응형 전략

#### MiniBoss (미니보스)
- 페이즈 기반 전략
- 첫 턴 오프닝 패턴
- 3턴마다 강력한 공격

#### Boss (보스)
- 복잡한 3페이즈 시스템
- 체력 비율별 차별화된 전략
- 분노 모드 시스템

### AI 성격 시스템

```csharp
public enum AIPersonality
{
    Simple,      // 단순 로직
    Aggressive,  // 공격 선호  
    Defensive,   // 방어 선호
    Strategic,   // 상황 판단 중시
    Chaotic,     // 예측 불가
    Balanced     // 균형적
}
```

## 🔍 디버깅 및 테스트

### 1. AISystemIntegrationTest 사용
```csharp
// 테스트 컴포넌트 설정
AISystemIntegrationTest tester = FindObjectOfType<AISystemIntegrationTest>();
tester.RunAllTestsMenu(); // 컨텍스트 메뉴에서 실행

// 개별 테스트
tester.TestAIManagerOnly();
tester.TestPatternDataManagerOnly();
tester.QuickTest();
```

### 2. 디버그 로그 활용
- AIManager: `enableAdvancedAI` 플래그로 상세 로그
- MonsterAI: `showDebugLogs` 플래그로 개별 AI 로그  
- TurnManager: `showTurnDebugLogs` 플래그로 턴 진행 로그

### 3. Inspector 실시간 모니터링
- MonsterAI 컴포넌트에서 현재 AI 상태 확인
- TurnManager에서 전투 진행 상황 확인
- IntentDisplaySystem에서 의도 표시 상태 확인

## 📈 성능 최적화

### 1. 패턴 캐싱
- PatternDataManager에서 자동 캐싱
- 타입별 사전 분류로 검색 최적화
- 중복 로딩 방지

### 2. AI 연산 최적화
- 턴당 1회만 의도 결정
- 불필요한 AI 업데이트 방지
- 사망한 몬스터 자동 정리

### 3. UI 최적화
- Object Pooling 패턴 적용 가능
- 오프스크린 UI 비활성화
- 애니메이션 최적화

## 🚀 확장 가능성

### 1. 새로운 AI 성격 추가
```csharp
// AIPersonality enum에 새 타입 추가
// MonsterAI.ApplyPersonalityModification()에 로직 구현
```

### 2. 커스텀 몬스터 패턴
```csharp
// MonsterPatternSO 에셋 생성
// PatternDataManager에 자동 등록
// AIManager에서 자동 인식
```

### 3. 고급 AI 기능
- 플레이어 학습 시스템
- 적응형 난이도 조절
- 협력 AI (다중 몬스터)

## 📋 체크리스트

### 설정 완료 확인
- [ ] PatternDataManager 에셋 생성 및 설정
- [ ] MonsterPatternSO 에셋들 생성
- [ ] Character에 MonsterAI 컴포넌트 추가
- [ ] IntentDisplaySystem UI 설정
- [ ] TurnManager 씬에 배치

### 테스트 확인
- [ ] AISystemIntegrationTest 모든 테스트 통과
- [ ] 의도 표시 UI 정상 동작
- [ ] 턴 진행 정상 동작
- [ ] 페이즈 전환 정상 동작

### 성능 확인
- [ ] 60FPS 유지
- [ ] 메모리 누수 없음
- [ ] AI 응답 시간 1초 이내

---

## 📞 문의 및 지원

시스템 사용 중 문제가 발생하면 다음을 확인해주세요:

1. **AISystemIntegrationTest** 실행으로 전체 시스템 상태 확인
2. **Console 로그**에서 오류 메시지 확인  
3. **PatternDataManager** 초기화 상태 확인
4. **MonsterAI 컴포넌트** 정상 부착 확인

이 시스템은 모듈화되어 있어 각 컴포넌트를 독립적으로 테스트하고 확장할 수 있습니다.
