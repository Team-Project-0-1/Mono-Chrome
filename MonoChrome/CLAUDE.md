# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**MONOCHROME: the Eclipse** is a Unity 6 C# turn-based roguelike game with a coin-based combat system and Korean localization support. This is a portfolio project demonstrating enterprise-level game development practices, clean architecture patterns, and sophisticated dependency injection systems.

## Build & Development Commands

### Unity Development
- **Open Project**: Open the MonoChrome folder in Unity Hub (Unity 6.0+ required)
- **Build Game**: File → Build Settings → Build (or Ctrl+Shift+B)
- **Run Tests**: Window → General → Test Runner
- **Assembly Definitions**: Project uses MonoChrome.asmdef for compilation optimization

### Project Validation
```csharp
// Auto-setup new development environment
Unity Menu: MonoChrome > 프로젝트 검증 및 설정 > 자동 설정 실행

// Manual validation
Add ProjectSetupAndValidator component to scene and run validation
```

### Testing
```csharp
// System integration tests
Add SystemIntegrationChecker component and run RunCompleteSystemCheck()

// Architecture tests  
Add ImprovedArchitectureTest component with "Enable Auto Test" checked
```

## Core Architecture

### Event-Driven System
The project uses a **decoupled event-driven architecture** with three main event categories:
- `DungeonEvents`: Dungeon generation, node movement, room activities
- `CombatEvents`: Turn-based combat, pattern execution, coin flips
- `UIEvents`: Panel transitions, user input handling

**Key Pattern**:
```csharp
// Request pattern - any system can request actions
DungeonEvents.RequestDungeonGeneration(stageIndex);
DungeonEvents.RequestNodeMove(nodeId);

// Notification pattern - systems broadcast completion
DungeonEvents.NotifyDungeonGenerated(nodes, currentIndex);
DungeonEvents.NotifyNodeMoveCompleted(node);
```

### State Management
`GameStateMachine` handles pure state transitions:
- MainMenu → Dungeon → Combat → GameOver
- Event-driven state notifications
- Thread-safe singleton pattern

### System Controllers
Each major system has a dedicated controller following single responsibility:

- **MasterGameManager**: Central coordinator (711 lines) with enterprise-level architecture
- **DungeonController**: Located in `Assets/Scripts/Systems/Dungeon/` with multiple generation algorithms (Advanced, Improved, Procedural, Configurable)
- **UIController**: Located in `Assets/Scripts/Systems/UI/` with comprehensive UI components (Combat, Dungeon, Tooltips, Debug tools)
- **ServiceLocator**: Dependency injection system for loose coupling between systems
- **PlayerManager & ShopManager**: Located in `Assets/Scripts/Managers/` for player and shop functionality

### Data Architecture
ScriptableObject-based data system with Korean localization:
- **CharacterDataManager**: Player and enemy character definitions with Korean names
- **PatternDataManager**: Combat patterns organized by sensory types (Auditory, Olfactory, Tactile, Spiritual)
- **StageThemeDataAsset**: Stage-specific theming and configuration
- **DataConnector**: Centralized ScriptableObject management and loading system
- **EventBus**: Centralized event dispatching and subscription management

## Key Namespaces

```csharp
MonoChrome                    // Base namespace for characters and core types
MonoChrome.Core              // Central managers, data systems, and game state
MonoChrome.Core.Data         // Data management and ScriptableObject systems
MonoChrome.Core.Architecture // Architecture patterns and dependency injection
MonoChrome.Events            // Event system and messaging (DungeonEvents, UIEvents, CombatEvents)
MonoChrome.Systems.Dungeon   // Multiple dungeon generation algorithms and navigation systems
MonoChrome.Systems.Combat    // Turn-based combat, pattern systems, and UI bridges
MonoChrome.Systems.UI        // Comprehensive user interface controllers and management
MonoChrome.StatusEffects     // Buff/debuff system and effect management
MonoChrome.Testing           // System integration and architecture testing
MonoChrome.Setup             // Project setup and initialization systems
```

## Critical Implementation Notes

### Memory Management
- All event subscriptions must be properly unsubscribed in OnDestroy()
- Singleton pattern with thread-safety and application quit handling
- DontDestroyOnLoad objects for persistent managers

### Legacy System Integration
- `LegacySystemBridge` provides compatibility with older code during major refactoring
- `UnifiedUIBridge` handles transition between old and new UI systems
- `SystemMigrator` assists with gradual migration approach for incremental refactoring
- Extensive cleanup completed: 50+ legacy files removed from Combat/, UI/, Controllers/ folders

### Pattern-Based Combat
- Combat uses sensory-based patterns (Auditory, Olfactory, Tactile, Spiritual)
- Korean-localized pattern names: 각성, 앞면/뒷면 연계 시스템
- Coin flip mechanics determine success/failure with obverse/verso mechanics
- Status effects system with 14+ different effect types and DungeonStatusEffect extensions

## File Structure Guidance

### Core Systems Location
- `Assets/Scripts/Core/`: Central managers, coordinators, and architecture patterns
- `Assets/Scripts/Core/Architecture/`: Dependency injection and design patterns (MasterGameManager, GameStateMachine, ServiceLocator)
- `Assets/Scripts/Core/Data/`: Data management systems (DataConnector)
- `Assets/Scripts/Core/Events/`: Event definitions and bus system (GameEvents, EventBus)
- `Assets/Scripts/Core/Interfaces/`: Interface definitions for all major systems
- `Assets/Scripts/Systems/Combat/`: Combat mechanics, pattern system, and UI bridges
- `Assets/Scripts/Systems/Dungeon/`: Multiple dungeon generation algorithms and node management
- `Assets/Scripts/Systems/UI/`: Comprehensive user interface controllers and components
- `Assets/Scripts/Managers/`: Player and shop management systems

### Data Assets Location
- `Assets/Data/`: ScriptableObject data files including StageThemes
- `Assets/Resources/`: Runtime-loadable assets including CharacterDataManager and PatternDataManager
- `Assets/ScriptableObjects/`: SO class definitions and instance files
- `Assets/ScriptableObjects/Characters/`: Korean-named character definitions (검은 심연, 곽장환, etc.)
- `Assets/ScriptableObjects/Patterns/`: Korean-named pattern definitions (각성, 앞면/뒷면 연계)

### Documentation Location
- `Assets/Scripts/ARCHITECTURE_GUIDE.md`: Complete usage guide
- `Assets/Scripts/README_PORTFOLIO.md`: Project overview and achievements
- `Assets/Scripts/SYSTEM_SETUP_GUIDE.md`: Setup instructions
- `Assets/Scripts/Systems/Dungeon/README.md`: Dungeon system documentation
- `Assets/Scripts/Systems/UI/README.md`: UI system documentation

## Development Patterns

### Adding New Systems
1. Create controller class in appropriate namespace under `Assets/Scripts/Systems/`
2. Define events in the relevant event category (DungeonEvents, UIEvents, CombatEvents)
3. Subscribe to relevant events in Awake() using EventBus
4. Unsubscribe in OnDestroy() to prevent memory leaks
5. Register with ServiceLocator for dependency injection
6. Add system reference to MasterGameManager if needed for lifecycle management

### Event-Driven Communication
Always prefer events over direct references:
```csharp
// Good - decoupled using event system
UIEvents.RequestPanelTransition(PanelType.Combat);
DungeonEvents.RequestDungeonGeneration(stageIndex);

// Good - dependency injection through ServiceLocator
var uiController = ServiceLocator.Get<UIController>();

// Avoid - creates tight coupling  
uiManager.ShowCombatPanel();
FindObjectOfType<SomeManager>().DoSomething();
```

### Testing Integration
- **SystemIntegrationChecker**: Located in `Assets/Scripts/Testing/` - Run `RunCompleteSystemCheck()` for full system validation
- **ImprovedArchitectureTest**: Located in `Assets/Scripts/Testing/` - Enable "Auto Test" checkbox for continuous architecture validation
- **ProjectSetupAndValidator**: Located in `Assets/Scripts/Setup/` - Automatic project setup and environment verification

## Quality Standards

This project follows enterprise-level standards:
- **SOLID principles** applied throughout with dependency injection
- **Significant code consolidation** achieved through major refactoring (50+ files removed)
- **Thread-safe singleton** implementations with application quit handling
- **Memory leak prevention** with proper event cleanup and lifecycle management
- **Modular architecture** supporting team collaboration through ServiceLocator pattern
- **Korean localization support** for international game development
- **Legacy compatibility** maintained during incremental migration process

## Advanced Architecture Features

### Dependency Injection
- `ServiceLocator` pattern provides loose coupling between systems
- Thread-safe service registration and retrieval
- Lifecycle management integration with Unity's MonoBehaviour system

### Korean Asset Management
- Unicode filename support for Korean character and pattern names
- Proper encoding handling for asset pipeline
- Localization-ready ScriptableObject architecture

### Migration Strategy
- `SystemMigrator` handles incremental refactoring
- Legacy bridges maintain backward compatibility
- Gradual transition approach minimizes development disruption