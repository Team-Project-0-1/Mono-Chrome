# MONOCHROME 프로젝트 네임스페이스 통합 가이드

## 네임스페이스 단일화 결정

MONOCHROME 프로젝트의 복잡한 네임스페이스 구조로 인한 충돌 문제를 해결하기 위해, **모든 코드를 단일 `MonoChrome` 네임스페이스로 통합**하기로 결정했습니다.

## 변경 사항

1. **모든 열거형을 단일 파일에 정의**
   - 모든 열거형은 `Assets/Scripts/Enums.cs` 파일에 정의됩니다.
   - `namespace MonoChrome`으로 감싸져 있습니다.

2. **이전 중복 열거형 정의 파일 제거**
   - `MonoChrome.Core`
   - `MonoChrome.Characters`
   - `MonoChrome.Combat`
   - `MonoChrome.StatusEffects`
   - 이들 네임스페이스의 열거형 정의 파일은 모두 비워졌습니다.

3. **유틸리티 클래스**
   - 열거형 변환 및 조작을 위한 `GlobalTypeHelper` 클래스가 추가되었습니다.
   - 이 클래스는 `Assets/Scripts/Utils/GlobalTypeHelper.cs`에 위치합니다.

## 개발자 가이드라인

1. **새로운 클래스 작성 시**
   ```csharp
   using UnityEngine;
   
   namespace MonoChrome
   {
       public class MyNewClass : MonoBehaviour
       {
           // 코드 작성
       }
   }
   ```

2. **기존 클래스 수정 시**
   - 클래스의 네임스페이스를 `MonoChrome`으로 변경하세요.
   - 예: `namespace MonoChrome.Core` → `namespace MonoChrome`

3. **열거형 사용 시**
   ```csharp
   // 기존 방식 (더 이상 사용하지 마세요):
   // using MonoChrome.Core;
   // CharacterType type = CharacterType.Player;
   
   // 새로운 방식:
   using MonoChrome;
   CharacterType type = CharacterType.Player;
   ```

4. **새로운 열거형 추가 필요 시**
   - `Enums.cs` 파일에만 추가하세요.
   - 절대로 다른 파일에 열거형을 정의하지 마세요.

5. **타입 변환 작업 시**
   ```csharp
   // 문자열 → 열거형 변환
   string effectName = "Amplify";
   StatusEffectType type = GlobalTypeHelper.GetEffectTypeFromName(effectName);
   
   // 열거형 관련 정보 가져오기
   string description = GlobalTypeHelper.GetEffectDescription(type);
   Color color = GlobalTypeHelper.GetEffectColor(type);
   ```

## 문제 해결

통합 작업 후에도 컴파일 오류가 발생하면 다음을 확인하세요:

1. **중복 네임스페이스**: 같은 클래스가 여러 네임스페이스에 정의되어 있는지 확인
2. **잘못된 참조**: 코드에서 더 이상 존재하지 않는 네임스페이스 참조 확인
3. **유지된 using 문**: 불필요한 using 문이 있는지 확인  
   (예: `using MonoChrome.Core;` → `using MonoChrome;`)

## 이점

이 접근법은 다음과 같은 이점을 제공합니다:

1. **단순성**: 모든 코드가 동일한 네임스페이스에 있어 참조 문제가 사라집니다.
2. **명확성**: 개발자가 어느 네임스페이스를 사용해야 할지 고민할 필요가 없습니다.
3. **충돌 감소**: 동일한 이름의 클래스를 서로 다른 네임스페이스에 정의하는 문제가 없어집니다.
4. **유지보수**: 새로운 코드를 추가할 때 네임스페이스 고민 없이 작업할 수 있습니다.
