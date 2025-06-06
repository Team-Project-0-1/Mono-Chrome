# 🎮 MONOCHROME: the Eclipse - 포트폴리오 프로젝트

## 📊 프로젝트 개요

**MONOCHROME: the Eclipse**는 Unity6 C#으로 개발된 동전 기반 턴제 로그라이크 게임입니다.  
**대기업 게임회사 취업을 목표**로, 실무 수준의 **클린 아키텍처**와 **SOLID 원칙**을 적용한 고품질 포트폴리오 프로젝트입니다.

### 🎯 핵심 기술 스택
- **Unity 6.0** + **C# 최신 버전**
- **이벤트 드리븐 아키텍처** (Event-Driven Architecture)
- **SOLID 원칙** 완전 적용
- **클린 아키텍처** 패턴
- **상태 패턴** (State Machine)
- **싱글톤 패턴** (메모리 누수 방지)
- **의존성 주입** (Dependency Injection)

> **Note**: 기존 `GameManager`와 `CoreGameManager` 클래스는 `MasterGameManager`로 완전히 통합되었습니다.
> 레거시 호환을 위해 존재하지만 새 코드에서는 사용을 지양합니다.

---

## 🏆 포트폴리오 어필 포인트

### ✅ 아키텍처 설계 능력 증명
| 개선 전 | 개선 후 | 효과 |
|---------|---------|------|
| 높은 결합도 (직접 참조) | 낮은 결합도 (이벤트 통신) | **확장성 ↑** |
| 낮은 응집도 (혼재된 책임) | 높은 응집도 (단일 책임) | **유지보수성 ↑** |
| 테스트 불가능 | 단위 테스트 가능 | **품질 보장** |
| GameManager 500+ 줄 (현재는 MasterGameManager로 통합) | 200줄로 단순화 | **60% 코드 감소** |

### ✅ 실무 역량 증명
- **리팩토링 경험**: 기존 코드의 문제점 파악 → 체계적 개선
- **시스템 설계**: 확장 가능하고 유지보수 용이한 구조 설계
- **성능 최적화**: 메모리 누수 방지, GC 부담 감소
- **팀워크 고려**: 다른 개발자가 쉽게 이해할 수 있는 코드 작성

---

## 🔧 핵심 시스템 구조

### 📡 이벤트 시스템 (느슨한 결합)
```csharp
// 기존 방식 (높은 결합도)
dungeonManager.MoveToNode(nodeId);
gameManager.SwitchPanel("DungeonPanel");

// 개선된 방식 (낮은 결합도)
DungeonEvents.RequestNodeMove(nodeId);
GameStateMachine.Instance.EnterDungeon();
```

### 🎛️ 상태 머신 (순수한 상태 관리)
```csharp
public class GameStateMachine : MonoBehaviour
{
    public enum GameState { MainMenu, Dungeon, Combat, ... }
    
    public bool TryChangeState(GameState newState)
    {
        if (!IsValidStateTransition(_currentState, newState))
            return false;
            
        // 이벤트 발행으로 시스템 알림
        OnStateChanged?.Invoke(previousState, newState);
        return true;
    }
}
```

### 🏗️ 컨트롤러 분리 (단일 책임 원칙)
```csharp
// DungeonController - 오직 던전 로직만
public class DungeonController : MonoBehaviour
{
    private void HandleDungeonGenerationRequest(int stageIndex)
    {
        GenerateDungeon(stageIndex);
        DungeonEvents.NotifyDungeonGenerated(nodes, currentIndex);
    }
}

// UIController - 오직 UI 표시만
public class UIController : MonoBehaviour
{
    private void OnGameStateChanged(GameState newState)
    {
        ShowPanelForState(newState);
    }
}
```

---

## 🚀 빠른 시작 가이드

### 1️⃣ 자동 설정 (권장)
```csharp
// Unity 메뉴: MonoChrome > 프로젝트 검증 및 설정 > 자동 설정 실행
// 또는 씬에 ProjectSetupAndValidator 추가 후 "자동 설정" 실행
```

### 2️⃣ 수동 설정
1. **핵심 시스템 생성**:
   ```
   Hierarchy:
   ├── [MasterGameManager] (DontDestroyOnLoad)
   ├── [GameStateMachine] (DontDestroyOnLoad)
   ├── [EventBus] (DontDestroyOnLoad)
   ├── [DungeonController]
   ├── [UIController]
   └── [LegacySystemBridge] (기존 코드 호환용)
   ```

2. **스크립트 연결**: 각 GameObject에 해당 스크립트 컴포넌트 추가

3. **검증 실행**: `ProjectSetupAndValidator`로 시스템 상태 확인

---

## 🧪 테스트 및 검증

### 자동 검증 시스템
```csharp
public class SystemIntegrationChecker : MonoBehaviour
{
    // 전체 시스템 통합 테스트
    public IEnumerator RunCompleteSystemCheck()
    {
        // 1. 이벤트 시스템 테스트
        // 2. 상태 머신 테스트  
        // 3. 컨트롤러 통합 테스트
        // 4. 포트폴리오 준비도 평가
    }
}
```

### 포트폴리오 품질 검증
- **🟢 우수**: 90% 이상 → 대기업 면접 제출 가능
- **🟡 양호**: 70% 이상 → 추가 개선 권장  
- **🔴 개선 필요**: 70% 미만 → 기본 구조 보완 필요

---

## 📚 학습 자료 및 문서

### 📖 상세 문서
- [`ARCHITECTURE_GUIDE.md`](Assets/Scripts/ARCHITECTURE_GUIDE.md) - 아키텍처 사용법
- [`README_REFACTORING_GUIDE.md`](Assets/Scripts/README_REFACTORING_GUIDE.md) - 리팩토링 과정
- 게임 기획서 - Google Docs 문서 참조

### 🔍 코드 품질 확인 포인트
1. **단일 책임 원칙**: 각 클래스가 하나의 명확한 역할만 담당
2. **개방-폐쇄 원칙**: 확장에는 열려있고 수정에는 닫혀있음
3. **의존성 역전**: 구체적인 구현이 아닌 인터페이스에 의존
4. **이벤트 드리븐**: 시스템 간 느슨한 결합으로 확장성 확보
5. **메모리 관리**: 이벤트 구독/해제 적절한 처리로 누수 방지

---

## 🎯 면접 어필 전략

### 💼 기술면접에서 강조할 포인트

#### 1. 문제 인식 능력
> "기존 코드에서 GameManager가 500줄이 넘고 7가지 책임을 가지고 있어서 유지보수가 어려웠습니다."

#### 2. 해결 방안 설계
> "SOLID 원칙을 적용해 단일 책임 원칙에 따라 시스템을 분리하고, 이벤트 시스템으로 느슨한 결합을 구현했습니다."

#### 3. 구체적인 개선 결과
> "결과적으로 코드량 60% 감소, 테스트 가능성 100% 향상, 확장성 대폭 개선을 달성했습니다."

#### 4. 실무 적용 가능성
> "이 아키텍처는 팀 개발 환경에서 각자 독립적으로 개발할 수 있도록 설계되어 협업 효율성을 높입니다."

### 🎮 게임 개발 역량
- **완성도 높은 게임**: 플레이 가능한 프로토타입 완성
- **시스템 설계**: 동전 기반 확률 시스템의 독창적 설계
- **밸런싱**: 게임플레이 루프와 난이도 곡선 설계
- **UI/UX**: 직관적이고 반응성 좋은 사용자 인터페이스

---

## 📈 개발 로드맵

### ✅ Phase 1: 아키텍처 구축 (완료)
- 이벤트 시스템 구현
- 상태 머신 설계
- 컨트롤러 분리
- 테스트 시스템 구축

### 🔄 Phase 2: 게임 시스템 완성 (진행 중)
- 전투 시스템 완성
- 던전 생성 알고리즘 최적화
- 캐릭터 시스템 구현
- 밸런싱 및 튜닝

### 🚀 Phase 3: 포트폴리오 완성 (예정)
- 최종 품질 검증
- 성능 최적화
- 포트폴리오 문서 완성
- 플레이 가능한 빌드 제작

---

## 🤝 기여 및 피드백

이 프로젝트는 **대기업 게임회사 취업용 포트폴리오**로 개발되었습니다.  
코드 품질, 아키텍처, 게임 디자인에 대한 피드백을 환영합니다!

### 📞 연락처
- **개발자**: [이름]
- **이메일**: [이메일]
- **GitHub**: [GitHub 주소]
- **포트폴리오**: [포트폴리오 사이트]

---

## 📄 라이선스

이 프로젝트는 포트폴리오 목적으로 개발되었으며, 상업적 사용을 금지합니다.  
코드는 학습 목적으로 참고 가능하나, 직접적인 복사는 지양해 주세요.

---

**🎯 "단순히 게임을 만드는 것이 아니라, 확장 가능하고 유지보수가 용이한 시스템을 설계할 수 있는 개발자입니다."**

