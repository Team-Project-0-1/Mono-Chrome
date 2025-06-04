# 🎮 MONOCHROME: the Eclipse - 시스템 아키텍처 설정 가이드

## 📋 개요
이 가이드는 GameManager 충돌 및 의존성 문제를 해결하고, 통합된 아키텍처를 설정하는 방법을 설명합니다.

## 🔧 해결된 문제들
1. **여러 GameManager 클래스 충돌** - 4개의 서로 다른 GameManager가 존재
2. **네임스페이스 충돌** - MonoChrome vs MonoChrome.Core
3. **UI 시스템 중복** - UIManager와 UIController 동시 존재
4. **복잡한 의존성** - 순환 참조 및 불명확한 초기화 순서

## ✅ 새로운 통합 아키텍처

### 핵심 시스템 구조
```
MasterGameManager (Core)
├── GameStateMachine (상태 관리)
├── EventBus (이벤트 중계)
├── DataConnector (데이터 관리)
└── UnifiedUIBridge (UI 통합)
```

### 주요 컴포넌트

#### 1. MasterGameManager
- **파일**: `Scripts/Core/MasterGameManager.cs`
- **역할**: 모든 시스템의 중앙 조정자
- **특징**: 
  - 싱글톤 패턴 (Thread-Safe)
  - 이벤트 기반 통신
  - 씬 전환 시 안전한 초기화
  - 레거시 시스템 호환성 유지

#### 2. UnifiedUIBridge
- **파일**: `Scripts/Core/UnifiedUIBridge.cs`
- **역할**: 기존 UI 시스템들을 통합
- **특징**:
  - 새로운 UIController와 레거시 UIManager 브릿지
  - 이벤트 기반 UI 업데이트
  - 런타임 시스템 전환 가능

#### 3. SystemStartupInitializer
- **파일**: `Scripts/Setup/SystemStartupInitializer.cs`
- **역할**: 시스템 자동 초기화
- **특징**:
  - MasterGameManager 자동 생성
  - 씬별 초기화 보장

## 🚀 설정 방법

### 1단계: 씬 설정
1. **GameScene** 또는 **MainMenu** 씬을 엽니다
2. 빈 GameObject를 생성하고 이름을 `"SystemInitializer"`로 설정
3. `SystemStartupInitializer` 컴포넌트를 추가합니다
4. 설정 확인:
   - ✅ Create Master Game Manager
   - ✅ Enable Debug Logs
   - ❌ Destroy After Initialization (디버깅용으로 유지)

### 2단계: UI 브릿지 설정
1. Canvas 하위에 빈 GameObject 생성
2. 이름을 `"UIBridge"`로 설정
3. `UnifiedUIBridge` 컴포넌트 추가
4. 설정 확인:
   - ✅ Use New UI Controller
   - ✅ Maintain Legacy Support
   - ✅ Enable Debug Logs

### 3단계: 기존 GameManager 비활성화
**중요**: 기존 GameManager들을 찾아서 비활성화하거나 제거하세요:
- `GameManager` (이미 백업됨)
- `CoreGameManager` (이미 백업됨)
- `ImprovedGameManager` (이미 백업됨)
- `UnifiedGameManager` (이미 백업됨)

### 4단계: 테스트 및 검증
1. Unity에서 Play 버튼을 누릅니다
2. Console에서 다음 로그들을 확인:
   ```
   [SystemStartupInitializer] MasterGameManager 생성 완료
   [MasterGameManager] 마스터 게임 매니저 초기화 완료
   [UnifiedUIBridge] UI 브릿지 초기화 완료
   ```

## 🎯 사용 방법

### 게임 상태 변경
```csharp
// MasterGameManager를 통한 게임 플로우 제어
MasterGameManager.Instance.StartNewGame();
MasterGameManager.Instance.SelectCharacter("김훈희");
MasterGameManager.Instance.EnterDungeon();
MasterGameManager.Instance.StartCombat();
```

### UI 업데이트
```csharp
// 이벤트 기반 UI 업데이트
UIEvents.RequestPanelShow("DungeonPanel");
UIEvents.RequestDungeonMapUpdate(nodes, currentIndex);
UIEvents.RequestPlayerStatusUpdate(hp, maxHp);
```

### 던전 및 전투 이벤트
```csharp
// 던전 생성
DungeonEvents.RequestDungeonGeneration(stageIndex);

// 전투 시작
CombatEvents.RequestCombatStart(enemyType, characterType);

// 전투 종료
CombatEvents.RequestCombatEnd(isVictory);
```

## 🛠️ 디버깅 가이드

### 시스템 상태 확인
1. **MasterGameManager**에서 우클릭 → "Generate System Status Report"
2. **UnifiedUIBridge**에서 우클릭 → "Generate UI Bridge Status Report"

### 일반적인 문제 해결

#### 문제: UI가 표시되지 않음
```
해결책:
1. UnifiedUIBridge가 활성화되어 있는지 확인
2. Canvas가 존재하는지 확인
3. 패널 오브젝트들이 올바른 이름을 가지는지 확인
```

#### 문제: 전투가 시작되지 않음
```
해결책:
1. CombatManager가 활성화되어 있는지 확인
2. DataConnector가 초기화되었는지 확인
3. CharacterDataManager와 PatternDataManager가 Resources 폴더에 있는지 확인
```

#### 문제: 던전이 생성되지 않음
```
해결책:
1. DungeonController가 존재하는지 확인
2. StageTheme 데이터가 올바르게 설정되어 있는지 확인
3. 던전 생성 이벤트가 올바르게 구독되었는지 확인
```

## 📁 파일 구조

### 새로 생성된 파일들
```
Scripts/
├── Core/
│   ├── MasterGameManager.cs          # 🆕 주 게임 매니저
│   └── UnifiedUIBridge.cs            # 🆕 UI 통합 브릿지
├── Setup/
│   └── SystemStartupInitializer.cs   # 🆕 시스템 초기화
└── Events/
    └── GameEvents.cs                  # 🔄 업데이트됨
```

### 백업된 파일들
```
Scripts/Core/
├── GameManager_OLD.cs                # 백업
├── CoreGameManager_BACKUP.cs         # 백업
├── ImprovedGameManager_BACKUP.cs     # 백업
├── UnifiedGameManager_BACKUP.cs      # 백업
└── UIManager_LEGACY.cs               # 백업
```

## 🎖️ 포트폴리오 품질 특징

### 설계 원칙 준수
- **단일 책임 원칙**: 각 클래스가 명확한 역할
- **개방-폐쇄 원칙**: 새 기능 추가 시 기존 코드 수정 불필요
- **의존성 역전 원칙**: 추상화에 의존, 구체적 구현에 의존하지 않음

### 유지보수성
- **명확한 네이밍**: 클래스, 메서드, 변수명이 역할을 명확히 표현
- **문서화**: 모든 public 메서드에 XML 문서 주석
- **로깅**: 디버깅을 위한 체계적인 로그 시스템

### 확장성
- **이벤트 기반**: 새로운 시스템 추가 시 기존 코드 영향 최소화
- **모듈형 구조**: 각 시스템이 독립적으로 작동
- **설정 가능**: Inspector에서 시스템 동작 조정 가능

## 📞 추가 지원

문제가 발생하거나 추가 기능이 필요한 경우:
1. Console 로그를 확인하여 에러 메시지 파악
2. System Status Report를 생성하여 시스템 상태 확인
3. 이 가이드의 디버깅 섹션 참조

---
**마지막 업데이트**: 2025년 6월 4일
**버전**: 1.0.0
**작성자**: AI Assistant for MONOCHROME Project
