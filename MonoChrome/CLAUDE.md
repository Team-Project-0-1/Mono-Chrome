# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**MONOCHROME: the Eclipse** is a Unity 6 C# turn-based roguelike game with a coin-based combat system. This is a portfolio project demonstrating enterprise-level game development practices and clean architecture patterns.

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

- **MasterGameManager**: Central coordinator (200 lines, reduced from 500+)
- **DungeonController**: Dungeon generation and node management only
- **UIController**: UI panel display and transitions only  
- **CombatSystem**: Turn-based combat logic only

### Data Architecture
ScriptableObject-based data system:
- **CharacterDataManager**: Player and enemy character definitions
- **PatternDataManager**: Combat patterns organized by sensory types (Auditory, Olfactory, Tactile, Spiritual)
- **StageThemeDataAsset**: Stage-specific theming and configuration

## Key Namespaces

```csharp
MonoChrome.Core          // Central managers and game state
MonoChrome.Events        // Event system and messaging
MonoChrome.Dungeon       // Dungeon generation and navigation
MonoChrome.Combat        // Turn-based combat and patterns
MonoChrome.Characters    // Player and enemy character systems
MonoChrome.UI            // User interface controllers
MonoChrome.StatusEffects // Buff/debuff system
```

## Critical Implementation Notes

### Memory Management
- All event subscriptions must be properly unsubscribed in OnDestroy()
- Singleton pattern with thread-safety and application quit handling
- DontDestroyOnLoad objects for persistent managers

### Legacy System Integration
- `LegacySystemBridge` provides compatibility with older code
- `UnifiedUIBridge` handles transition between old and new UI systems
- Gradual migration approach allows incremental refactoring

### Pattern-Based Combat
- Combat uses sensory-based patterns (Auditory, Olfactory, Tactile, Spiritual)
- Coin flip mechanics determine success/failure
- Status effects system with 14+ different effect types

## File Structure Guidance

### Core Systems Location
- `Assets/Scripts/Core/`: Central managers and coordinators
- `Assets/Scripts/Events/`: Event definitions and bus system
- `Assets/Scripts/Combat/`: Combat mechanics and pattern system
- `Assets/Scripts/Dungeon/`: Procedural dungeon generation
- `Assets/Scripts/UI/`: User interface controllers

### Data Assets Location
- `Assets/Data/`: ScriptableObject data files
- `Assets/Resources/`: Runtime-loadable assets
- `Assets/ScriptableObjects/`: SO class definitions

### Documentation Location
- `Assets/Scripts/ARCHITECTURE_GUIDE.md`: Complete usage guide
- `Assets/Scripts/README_PORTFOLIO.md`: Project overview and achievements
- `Assets/Scripts/SYSTEM_SETUP_GUIDE.md`: Setup instructions

## Development Patterns

### Adding New Systems
1. Create controller class in appropriate namespace
2. Define events in GameEvents.cs
3. Subscribe to relevant events in Awake()
4. Unsubscribe in OnDestroy()
5. Add system reference to MasterGameManager if needed

### Event-Driven Communication
Always prefer events over direct references:
```csharp
// Good - decoupled
UIEvents.RequestPanelTransition(PanelType.Combat);

// Avoid - creates tight coupling  
uiManager.ShowCombatPanel();
```

### Testing Integration
- Use SystemIntegrationChecker for full system tests
- ImprovedArchitectureTest for architecture validation
- ProjectSetupAndValidator for environment verification

## Quality Standards

This project follows enterprise-level standards:
- **SOLID principles** applied throughout
- **60% code reduction** achieved through refactoring
- **Thread-safe singleton** implementations
- **Memory leak prevention** with proper event cleanup
- **Modular architecture** supporting team collaboration