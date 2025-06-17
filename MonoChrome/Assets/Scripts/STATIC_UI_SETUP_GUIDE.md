# MONOCHROME 정적 UI 설정 가이드

## 개요
CombatUI 시스템은 정적 UI 구성을 우선적으로 사용하며, 정적 오브젝트가 없을 때만 동적으로 생성합니다. 이 가이드는 전투 UI를 위한 정적 오브젝트 설정 방법을 설명합니다.

## 1. CombatUI GameObject 구조 설정

### 기본 계층 구조
```
Scene
└── Canvas
    └── CombatUI (Canvas 하위에 생성)
        ├── PlayerHealthBar
        ├── EnemyHealthBar
        ├── CoinArea
        ├── PatternArea
        ├── PlayerStatusEffectContainer
        ├── EnemyStatusEffectContainer
        ├── ActiveSkillButton
        ├── EndTurnButton
        ├── TurnInfoText
        └── EnemyIntentionText
```

## 2. 각 UI 요소 상세 설정

### 2.1 체력바 설정 (PlayerHealthBar, EnemyHealthBar)
```
PlayerHealthBar (GameObject)
├── Component: Slider
├── Component: RectTransform
│   ├── Anchor: Top-Left
│   ├── Position: (50, -30)
│   └── Size: (200, 20)
└── Child: Handle Slide Area
    └── Child: Handle
        └── Child: Text (체력 표시용)
            └── Component: Text
                ├── Text: "100/100"
                ├── Color: White
                └── Alignment: Center
```

### 2.2 동전 영역 설정 (CoinArea)
```
CoinArea (GameObject)
├── Component: RectTransform
│   ├── Anchor: Bottom-Center
│   ├── Position: (0, 100)
│   └── Size: (600, 100)
└── Component: HorizontalLayoutGroup (선택사항)
    ├── Spacing: 10
    ├── Child Alignment: Middle Center
    └── Child Force Expand: Width
```

### 2.3 패턴 영역 설정 (PatternArea)
```
PatternArea (GameObject)
├── Component: RectTransform
│   ├── Anchor: Middle-Left
│   ├── Position: (150, 0)
│   └── Size: (300, 400)
├── Component: ScrollRect
│   ├── Content: Content GameObject
│   ├── Vertical: true
│   └── Horizontal: false
└── Child: Content
    ├── Component: RectTransform
    ├── Component: VerticalLayoutGroup
    │   ├── Spacing: 5
    │   └── Child Force Expand: Width
    └── Component: ContentSizeFitter
        └── Vertical Fit: Preferred Size
```

### 2.4 상태 효과 컨테이너 설정
```
PlayerStatusEffectContainer (GameObject)
├── Component: RectTransform
│   ├── Anchor: Top-Left
│   ├── Position: (50, -70)
│   └── Size: (300, 40)
└── Component: HorizontalLayoutGroup
    ├── Spacing: 5
    └── Child Force Expand: Height

EnemyStatusEffectContainer (GameObject)
├── Component: RectTransform
│   ├── Anchor: Top-Right
│   ├── Position: (-50, -70)
│   └── Size: (300, 40)
└── Component: HorizontalLayoutGroup
    ├── Spacing: 5
    └── Child Force Expand: Height
```

### 2.5 버튼 설정
```
ActiveSkillButton (GameObject)
├── Component: Button
├── Component: Image (배경)
│   └── Color: (0.8, 0.6, 0.2, 0.8)
├── Component: RectTransform
│   ├── Position: (-200, -200)
│   └── Size: (100, 40)
└── Child: Text
    ├── Text: "액티브 스킬"
    └── Color: White

EndTurnButton (GameObject)
├── Component: Button
├── Component: Image (배경)
│   └── Color: (0.6, 0.6, 0.6, 0.8)
├── Component: RectTransform
│   ├── Position: (-80, -200)
│   └── Size: (100, 40)
└── Child: Text
    ├── Text: "턴 종료"
    └── Color: White
```

### 2.6 텍스트 요소 설정
```
TurnInfoText (GameObject)
├── Component: Text
│   ├── Text: "Turn: 1"
│   ├── Color: White
│   ├── Font Size: 18
│   └── Alignment: Middle Center
└── Component: RectTransform
    ├── Anchor: Top-Center
    ├── Position: (0, -50)
    └── Size: (200, 30)

EnemyIntentionText (GameObject)
├── Component: Text
│   ├── Text: "적이 준비 중..."
│   ├── Color: Yellow
│   ├── Font Size: 16
│   └── Alignment: Middle Center
└── Component: RectTransform
    ├── Anchor: Top-Center
    ├── Position: (0, -100)
    └── Size: (300, 30)
```

## 3. 프리팹 설정

### 3.1 Resources 폴더 구조
```
Assets/
└── Resources/
    ├── UI/
    │   ├── CoinPrefab.prefab
    │   ├── PatternButtonPrefab.prefab
    │   └── StatusEffectPrefab.prefab
    ├── CoinPrefab.prefab (대체 경로)
    └── Prefabs/
        ├── CoinPrefab.prefab (대체 경로)
        ├── PatternButtonPrefab.prefab (대체 경로)
        └── StatusEffectPrefab.prefab (대체 경로)
```

### 3.2 CoinPrefab 설정
```
CoinPrefab (GameObject)
├── Component: Image (동전 배경)
│   ├── Color: (0.8, 0.2, 0.2, 0.9) - 앞면 기본
│   └── Size: (60, 60)
├── Component: RectTransform
│   └── Size: (60, 60)
└── Child: Text
    ├── Component: Text
    │   ├── Text: "앞면"
    │   ├── Color: White
    │   ├── Font Size: 12
    │   └── Alignment: Middle Center
    └── Component: RectTransform (전체 크기)
```

### 3.3 PatternButtonPrefab 설정
```
PatternButtonPrefab (GameObject)
├── Component: Button
├── Component: Image (버튼 배경)
│   ├── Color: (0.8, 0.2, 0.2, 0.8) - 앞면 기본
│   └── Size: (250, 55)
├── Component: RectTransform
│   └── Size: (250, 55)
├── Child: PatternName (Text)
│   ├── Component: Text
│   │   ├── Text: "패턴명"
│   │   ├── Color: White
│   │   ├── Font Size: 16
│   │   ├── Font Style: Bold
│   │   └── Alignment: Middle Left
│   └── Anchor/Position: Left side
├── Child: PatternEffect (Text)
│   ├── Component: Text
│   │   ├── Text: "효과 설명"
│   │   ├── Color: Light Gray
│   │   ├── Font Size: 12
│   │   └── Alignment: Middle Left
│   └── Anchor/Position: Left side, below name
└── Child: PowerText (Text)
    ├── Component: Text
    │   ├── Text: "공격\n+0"
    │   ├── Color: Red/Blue
    │   ├── Font Size: 14
    │   └── Alignment: Middle Center
    └── Anchor/Position: Right side
```

### 3.4 StatusEffectPrefab 설정
```
StatusEffectPrefab (GameObject)
├── Component: Image (효과 배경)
│   ├── Color: (0.2, 0.2, 0.2, 0.8)
│   └── Size: (40, 40)
├── Component: RectTransform
│   └── Size: (40, 40)
└── Child: EffectText
    ├── Component: Text
    │   ├── Text: "효과"
    │   ├── Color: White
    │   ├── Font Size: 8
    │   └── Alignment: Middle Center
    └── Component: RectTransform (전체 크기)
```

## 4. CombatUI MonoBehaviour 설정

### 4.1 CombatUI 컴포넌트 Inspector 설정
```
CombatUI (MonoBehaviour)
├── Player Health Bar: PlayerHealthBar 할당
├── Enemy Health Bar: EnemyHealthBar 할당
├── Coin Container: CoinArea 할당
├── Pattern Container: PatternArea/Content 할당
├── Player Status Effect Container: PlayerStatusEffectContainer 할당
├── Enemy Status Effect Container: EnemyStatusEffectContainer 할당
├── Active Skill Button: ActiveSkillButton 할당
├── End Turn Button: EndTurnButton 할당
├── Turn Info Text: TurnInfoText 할당
├── Enemy Intention Text: EnemyIntentionText 할당
├── Coin Prefab: CoinPrefab 할당 (선택사항)
├── Pattern Button Prefab: PatternButtonPrefab 할당 (선택사항)
└── Status Effect Prefab: StatusEffectPrefab 할당 (선택사항)
```

## 5. 설정 검증 방법

### 5.1 Unity Editor에서 확인
1. CombatUI GameObject가 활성화된 상태에서 Play 모드 진입
2. Console에서 다음 메시지들 확인:
   - ✅ "CombatUI: 정적 CoinArea 찾음"
   - ✅ "CombatUI: 정적 PatternArea 찾음"
   - ✅ "CombatUI: 정적 CoinPrefab 로드 완료"
   - ✅ "CombatUI: 정적 PatternButtonPrefab 로드 완료"

### 5.2 경고 메시지가 나타나는 경우
- ⚠️ "정적 CoinArea를 찾을 수 없어 동적 생성합니다" → CoinArea GameObject 생성 필요
- ⚠️ "정적 PatternArea를 찾을 수 없어 동적 생성합니다" → PatternArea GameObject 생성 필요
- ⚠️ "정적 CoinPrefab을 찾을 수 없어 동적 생성합니다" → Resources 폴더에 프리팹 배치 필요

## 6. 추가 팁

### 6.1 대체 이름 지원
시스템은 다음 이름들을 자동으로 검색합니다:
- **CoinArea**: CoinContainer, Coins
- **PatternArea**: PatternContainer, Patterns, PatternArea/Content
- **프리팹**: UI/PrefabName, PrefabName, Prefabs/PrefabName

### 6.2 레이아웃 그룹 활용
- HorizontalLayoutGroup: 동전, 상태 효과 자동 정렬
- VerticalLayoutGroup: 패턴 버튼 자동 정렬
- ContentSizeFitter: 스크롤 영역 자동 크기 조정

### 6.3 성능 최적화
- 정적 UI 사용 시 런타임 GameObject 생성 최소화
- 프리팹 재사용으로 메모리 효율성 증대
- Layout Group 사용으로 위치 계산 자동화

이 가이드를 따라 설정하면 동적 생성 없이 완전히 정적인 UI로 전투 시스템을 구성할 수 있습니다.