# Unity Mechanics Framework - Gemini Context

This project is a modular, open-source collection of plug-and-play gameplay mechanics for Unity (2021.3+). It emphasizes reusability, modularity, and educational value through line-by-line code explanations and video documentation.

## Project Overview

*   **Goal:** Provide a centralized library of production-ready Unity mechanics to avoid redundant implementation.
*   **Technologies:** Unity 2021.3 LTS (minimum), 2022.3 LTS (recommended), Unity 6 (supported). C# with a focus on clean architecture.
*   **Architecture:**
    *   **MonoSingletonGeneric<T>:** A base class for persistent, globally accessible singletons.
    *   **IPhysicsAdapter:** A physics-agnostic layer allowing mechanics to work in both 2D and 3D without code changes.
    *   **EventBus:** A decoupled communication system using the Observer pattern to prevent tight coupling between mechanics.

## Directory Structure

*   `Runtime/`: Core and feature-specific mechanics (Core, Dialogue, Movement, Physics, etc.).
*   `Editor/`: Editor-only tools, inspectors, and property drawers.
*   `Samples~/`: Self-contained demo scenes, prefabs, and videos for each mechanic. Uses the `~` suffix to be hidden from the `Assets` folder by default but importable via UPM.
*   `Tests/`: Runtime (Play mode) and Editor (Edit mode) tests.

## Development Conventions

### Namespaces
All scripts must follow the `GameplayMechanicsUMFOSS.<FeatureGroup>` convention:
*   `GameplayMechanicsUMFOSS.Core`: Singletons, EventBus, StateMachine.
*   `GameplayMechanicsUMFOSS.Movement`: Jump, dash, etc.
*   `GameplayMechanicsUMFOSS.Dialogue`: Narrative systems.
*   `GameplayMechanicsUMFOSS.Physics`: Adapters and utilities.
*   `GameplayMechanicsUMFOSS.Samples.Jump`: Sample scripts for the Jump mechanic.
*   `GameplayMechanicsUMFOSS.UI`, `GameplayMechanicsUMFOSS.Utils`, etc.

*Note: Some existing scripts currently lack namespaces and should be updated.*

### Coding Standards
*   **Naming:** `PascalCase` for classes and methods; `camelCase` for private/serialized fields; `UPPER_SNAKE_CASE` for constants.
*   **Structure:**
    1.  Serialized fields (with `[Header]` grouping).
    2.  Private fields.
    3.  Public properties.
    4.  Unity lifecycle methods (`Awake`, `Start`, etc.).
    5.  Public methods.
    6.  Private methods.
*   **Best Practices:**
    *   No magic numbers (use constants).
    *   Use `IPhysicsAdapter` instead of direct `Rigidbody`/`Rigidbody2D` references.
    *   Prefer `EventBus` over direct references for cross-mechanic communication.
    *   XML documentation (`/// <summary>`) for public APIs.

### Mandatory Contribution Assets
Every new mechanic must include:
1.  **ScriptExplainer.txt:** A line-by-line explanation of the code's purpose and logic.
2.  **Demo.mp4:** A walkthrough video covering setup, inspector settings, and a live demo (located in `Samples~/YourMechanic/Video/`).
3.  **DemoScene:** A self-contained scene in `Samples~/YourMechanic/Assets/Scenes/` that works immediately on Play.

## Building and Running

1.  **Open Project:** Open the root directory in Unity 2021.3+ or Unity 6.
2.  **Demo Scenes:** Navigate to the specific mechanic's sample folder (e.g., `Samples~/DialogueExample/Assets/Scenes/`) and open `DemoScene.unity`.
3.  **Run:** Press the Play button in the Unity Editor.
4.  **Testing:** Use the Unity Test Runner (`Window > General > Test Runner`) to run tests found in the `Tests/` directory.

## Common Tasks
*   **Adding a Mechanic:** Create a subfolder in `Runtime/`, implement the logic using the core patterns, create a demo scene/prefab in `Samples~/`, and write the `ScriptExplainer.txt`.
*   **Fixing Namespaces:** Ensure all files in `Runtime/` are wrapped in the `GameplayMechanicsUMFOSS` namespace hierarchy.
*   **Updating README:** When a mechanic is finalized, add it to the "Mechanics Library" section and the "Quick Navigation" table in `README.md`.
