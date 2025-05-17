# MONOCHROME: the Eclipse - 프로젝트 리팩토링 가이드

이 문서는 MonoChrome 프로젝트의 코드 구조를 개선하고 중복 클래스를 제거하여 프로젝트 가독성과 유지보수성을 높이기 위한 가이드입니다.

## 리팩토링 주요 변경 사항

### 1. 통일된 네임스페이스 구조

모든 클래스는 다음과 같은 네임스페이스 구조를 따릅니다:

- `MonoChrome`: 최상위 네임스페이스, 공통 열거형 정의 (AllEnums.cs)
- `MonoChrome.Core`: 핵심 시스템 클래스 (GameManager, 인터페이스 등)
- `MonoChrome.Characters`: 캐릭터 관련 클래스
- `MonoChrome.Combat`: 전투 시스템 관련 클래스
- `MonoChrome.Dungeon`: 던전 생성 및 탐험 관련 클래스
- `MonoChrome.UI`: UI 관련 클래스
- `MonoChrome.StatusEffects`: 상태 효과 관련 클래스
- `MonoChrome.Utils`: 유틸리티 클래스 및 확장 메서드

### 2. 중복 클래스 통합

- 모든 Manager 클래스가 중복되었던 이슈를 해결했습니다.
- 주요 클래스 최종 위치:
  - `CombatManager` → `Combat/CombatManager.cs`
  - `DungeonManager` → `Dungeon/DungeonManager.cs`
  - `CharacterManager` → `Characters/CharacterManager.cs`
  - `StatusEffectManager` → `StatusEffects/StatusEffectManager.cs`
- `Managers/` 폴더는 이전 버전 호환성을 위한 브릿지 클래스만 포함합니다.

### 3. 열거형 통합

- 모든 열거형은 `MonoChrome` 네임스페이스의 `AllEnums.cs` 파일에 정의되어 있습니다.
- 중복 정의되었던 열거형 (NodeType, RoomType, PlayerCharacterType 등)이 통합되었습니다.
- 사용하지 않는 열거형과 중복 파일은 `.old` 확장자로 변경되었습니다.

### 4. 확장 메서드 통합

- 모든 확장 메서드는 `Utils/` 폴더에 위치하도록 정리했습니다.
- 확장 메서드 클래스 이름 규칙: `[타입명]Extensions.cs`
- 예: `CharacterTypeExtensions.cs`, `StatusEffectExtensions.cs`

### 5. 전역 using 지시문 도입

- `GlobalUsings.cs` 파일이 추가되어 모든 스크립트에서 공통으로 사용하는 네임스페이스를 간편하게 가져올 수 있습니다.
- 이를 통해 코드 중복을 줄이고 일관된 네임스페이스 구조를 유지할 수 있습니다.

## 새로운 스크립트 작성 가이드

새로운 스크립트를 작성할 때는 다음 원칙을 따라주세요:

1. **적절한 네임스페이스 사용**:
   - 클래스/스크립트 기능에 맞는 정확한 네임스페이스 선택
   - 예: 전투 관련 → `MonoChrome.Combat`, 던전 관련 → `MonoChrome.Dungeon`

2. **명확한 참조**:
   - 모호한 참조를 피하기 위해 필요시 전체 네임스페이스 경로 사용
   - 예: `using MonoChrome.Combat;` 대신 `MonoChrome.Combat.CombatManager`

3. **중복 방지**:
   - 새 열거형은 `AllEnums.cs`에 추가
   - 새 확장 메서드는 해당 타입의 확장 클래스에 추가

4. **싱글톤 패턴 사용**:
   - 주요 Manager 클래스는 싱글톤 패턴을 사용하여 접근 일관성 유지
   - 예: `CombatManager.Instance`, `DungeonManager.Instance`

## 기존 코드 점진적 마이그레이션

기존 코드를 새 구조로 마이그레이션할 때:

1. 네임스페이스와 참조를 명확히 확인하고 업데이트
2. 중복된 코드는 새 구조로 통합
3. 확장 메서드 사용 시 적절한 네임스페이스에서 가져오기
4. 열거형 참조 시 `MonoChrome.AllEnums`에서 가져오기

## 레거시 코드 참고 사항

- `.old` 확장자로 변경된 파일들은 참조용으로만 유지됩니다.
- 일부 브릿지 클래스들은 이전 코드와의 호환성을 위해 유지되었습니다.
- 향후 이러한 브릿지 클래스들도 점진적으로 제거될 예정입니다.

이 가이드를 통해 더 깔끔하고 유지보수 가능한 코드 구조를 만들 수 있습니다. 질문이나 제안이 있으시면 팀에 공유해 주세요.
