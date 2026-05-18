# UnityMechanicsFramework

<div align="center">

**A modular, open-source collection of plug-and-play gameplay mechanics built for Unity.**

Stop rewriting the same systems across every project.  
This repository centralizes production-ready, reusable mechanics — built by the community, documented for everyone.

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vijit101/UnityMechanicsFramework/pulls)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2021.3%2B-black.svg)](https://unity.com/)
[![Contributors](https://img.shields.io/github/contributors/vijit101/UnityMechanicsFramework)](https://github.com/vijit101/UnityMechanicsFramework/graphs/contributors)

</div>

---

## Table of Contents

1. [What Is This?](#1-what-is-this)
2. [Who Is This For?](#2-who-is-this-for)
3. [Getting Started](#3-getting-started)
4. [Folder Structure](#4-folder-structure)
5. [Core Architecture](#5-core-architecture)
6. [Mechanics Library](#6-mechanics-library) ← **Start here to find a mechanic**
7. [Namespace Reference](#7-namespace-reference)
8. [Unity Version & Compatibility](#8-unity-version--compatibility)
9. [How to Contribute](#9-how-to-contribute)
10. [License](#10-license)

---

## 1. What Is This?

Every Unity developer has rewritten the same 10 mechanics dozens of times — a singleton manager, a jump controller, a dialogue system, a drag-and-drop handler. Each time from scratch. Each time slightly differently.

**UnityMechanicsFramework** puts an end to that.

This is a centralized, open-source library of gameplay mechanics that are:

- **Plug-and-play** : drop any mechanic into your project and have it running within minutes
- **Modular** : each system is fully self-contained with no hidden dependencies on other mechanics
- **Video-documented** : every mechanic ships with a contributor-recorded walkthrough video
- **Explained line-by-line** : every mechanic includes a `ScriptExplainer.txt` that teaches the code, not just shares it
- **Production-ready** : built with clean architecture, interface-based physics, and decoupled event systems

The goal is simple: build the mechanic once, document it properly, and let every Unity developer benefit from it forever.

---

## 2. Who Is This For?

| Developer Type | How This Helps You |
|---|---|
| **Learner / Student** | Study real Unity patterns with video walkthroughs and line-by-line code explanations. Raise and contribute issues |
| **Indie Developer** | Grab production-ready mechanics and integrate them in minutes, not hours |
| **Game Jam Participant** | Ship faster by pulling from a library of ready-to-use, pre-tested systems |
| **Educator / Mentor** | Point students at specific mechanics — every one has a video on how to use it , and a full code explainer  |
| **Open Source Contributor** | Add your mechanic, get it reviewed, and leave a permanent credited entry in this library |

---

## 3. Getting Started

### Option A — Clone the full repository

```bash
git clone https://github.com/vijit101/UnityMechanicsFramework.git
```

Import this Unity as a github pacakge using Unity Package manager . All packages import automatically via `package.json`.

### Option B — Grab a single mechanic

Each mechanic lives in its own self-contained folder under `Samples~/`. Copy any mechanic folder directly into your existing project without pulling in the entire repository.

### Running a demo

```
1. Open the repo in Unity (2021.3 LTS or later)
2. Go to Samples~/ and open any mechanic folder
3. Open Assets/Scenes/DemoScene.unity
4. Press Play
```

There are only scripts that you would need to load up as a package and follow the video to understand how to use it . For some the runnable demos are also present but its not a mandate due to sheer size of the repo and unity projects.

---

## 4. Folder Structure

```
UnityMechanicsFramework/
|
+-- package.json                    # UPM Manifest
+-- README.md                       # You are here — the mechanics index
+-- CONTRIBUTING.md                 # Read this before contributing
+-- CHANGELOG.md
+-- LICENSE
|
+-- Runtime/                        # All mechanic scripts live here
|   +-- Core/                       # Foundational systems (Singleton, EventBus, StateMachine)
|   +-- Physics/                    # IPhysicsAdapter, Physics2DAdapter, Physics3DAdapter
|   +-- Movement/                   # Jump, Dash, WallSlide
|   +-- Dialogue/                   # DialogueSystem, DialogueNode, DialogueDatabase
|   +-- Input/                      # InputAdapter
|   +-- Utils/                      # TimerUtility and shared helpers
|
+-- Editor/                         # Editor-only tools, inspectors, property drawers
|
+-- Samples~/                       # One folder per mechanic — runnable demos
|   +-- JumpExample/
|   +-- DialogueExample/
|   +-- [YourMechanicName]/         # Added by contributors
|
+-- Tests/
    +-- Runtime/                    # Play mode tests
    +-- Editor/                     # Edit mode tests
```

---

## 5. Core Architecture

Three foundational patterns run across the entire framework. Understanding them takes 5 minutes and will make every mechanic immediately readable.

### MonoSingleton — Generic Singleton Base

Convert any `MonoBehaviour` into a persistent singleton by inheriting `MonoSingletonGeneric<T>`. No boilerplate. No repeated code.

```csharp
using GameplayMechanicsUMFOSS.Core;

public class AudioManager : MonoSingletonGeneric<AudioManager>
{
    public void PlaySFX(AudioClip clip) { /* ... */ }
}

// Access from anywhere, any scene:
AudioManager.Instance.PlaySFX(jumpClip);
```

### IPhysicsAdapter — Physics-Agnostic Mechanics

All physics-dependent mechanics reference `IPhysicsAdapter` instead of `Rigidbody2D` directly. Swap `Physics2DAdapter` for `Physics3DAdapter` on your GameObject and the mechanic works in both dimensions without any code changes.

```csharp
[SerializeField] private IPhysicsAdapter physics;

// Works with both 2D and 3D — no changes needed
physics.AddForce(Vector2.up * jumpForce);
physics.SetVelocity(Vector2.zero);
```

### EventBus — Decoupled Communication

Mechanics never hold direct references to each other. They communicate via events. A jump system never needs to know a sound manager exists. Not all mechanics migght follow this depending on the issues raised .

```csharp
// Any mechanic can publish:
EventBus.Publish(new PlayerJumpedEvent { height = 12f });

// Any other system can react — from anywhere:
EventBus.Subscribe<PlayerJumpedEvent>(e => audioManager.PlayJumpSound());
```

---

## 6. Mechanics Library

> **This is the living index of every mechanic in this framework.**
>
> Every entry is contributed by a community member. Each one includes:
> the author's name and profile, a video walkthrough, a link to the mechanic, usage instructions, and highlights.
>
> **Contributors — before you open a PR:**
> 1. Add a row to the **Quick Navigation** table below (use your GitHub **issue number** as `#`, e.g. Issue **#25** → `#25-spawner-system`).
> 2. Add a full **`### N. Your Mechanic Name`** section using the template at the bottom of this section.
> 3. See [CONTRIBUTING.md → Section 14](./CONTRIBUTING.md#14-updating-the-mechanics-library-in-readme) for the exact format, video rules, and PR checklist.
>

---

### Quick Navigation

| # | Mechanic | Author | Category | Video |
|---|---|---|---|---|
| 1 | [MonoSingleton Generic](#1-monosingleton-generic) | Shubham B | Core | — |
| 2 | [Generic & Scalable Dialogue System](#2-generic--scalable-dialogue-system) | Mayur | Dialogue | [▶ Watch](https://github.com/vijit101/UnityMechanicsFramework/tree/main/RuntimeMechanics/Dailogue/2.%20GenericAndScalableDialogueSystem/Assets/Video%20tutorial) |
| 3 | [Scene Manager System](#3-scene-manager-system) | [Nymish](https://github.com/nymishkash) | Systems | [▶ Watch](Samples~/SceneManagerSample/SceneManagerVideos.zip) |
| 5 | [Modular Jump System](#5-modular-jump-system) | [Ankur Kalita](https://github.com/ankur-kalita) | Movement | [▶ Watch](./Samples~/JumpSystemSample/Video/ModularJumpImpl.mp4.zip) |
| 6 | [Screen Shake System](#6-screen-shake-system) | [Paramjeet Kaur](https://github.com/kauxp) | Systems | [▶ Watch](Samples~/ScreenShakeExample/Video/ScreenShakeTutorial.mp4) |
| 24 | [Pause System](#24-pause-system) | [Souvik Kumar](https://github.com/Souvik-Cyclic) | Systems | [▶ Watch](Samples~/PauseSystemSample/Video/PauseSystemVideo.zip) |
| 25 | [Spawner System](#25-spawner-system) | [Naresh Kumar Thodupunoori](https://github.com/Naresh-Kumar-Thodupunoori) | World / Spawning | [▶ Watch](./Sample/SpawnerSystem/SpawnerSystemTutorial.mp4.zip) |
| 27 | [Boomerang Weapon](#27-boomerang-weapon-system) | [Shrinibas Mahanta](https://github.com/2k4sm), [Shreyas Garg](https://github.com/shreyas-garg), [Sudharsan](https://github.com/Bug-Finderr) | Combat | [▶ Watch](https://github.com/vijit101/UnityMechanicsFramework/tree/main/Samples~/BoomerangWeapon/BoomerangWeaponDemoVideo.zip) |
| 64 | [Utils](#4-utils) | [Shubham](https://github.com/vijit101) | Core | — |


*More mechanics are added with every merged PR. [Contribute yours →](#9-how-to-contribute)*

---

### 1. MonoSingleton Generic

| | |
|---|---|
| **Author** | Shubham B |
| **Namespace** | `GameplayMechanicsUMFOSS.Core`  need to add a namespace / raise an issue |
| **Location** | `Runtime/Core/MonoSingleton.cs` |
| **Category** | Core / Architecture |
| **Demo Scene** | `Samples~/CoreExamples/Assets/Scenes/DemoScene.unity` |
| **Video** | — |

**What it does**

A reusable generic singleton base class for `MonoBehaviour`. Eliminates repetitive singleton boilerplate across your entire project. Any manager class inherits this and becomes a globally accessible, persistent single instance in two lines.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.Core;

// Step 1: Inherit from MonoSingletonGeneric<T>
public class GameManager : MonoSingletonGeneric<GameManager>
{
    public int score;
    public void AddScore(int points) => score += points;
}

// Step 2: Access it from anywhere in your project
GameManager.Instance.AddScore(10);
```

**Highlights**

- Generic — one class works for every manager in your project
- Automatically destroys any duplicate instances at runtime
- Persistent across scene loads — no need to re-find the instance
- Zero external dependencies — drop-in ready

---

### 2. Generic & Scalable Dialogue System

| | |
|---|---|
| **Author** | [Mayur](https://github.com/M-dev-acc) |
| **Namespace** | `GameplayMechanicsUMFOSS.Dialogue` need to add a namespace / raise an issue | 
| **Location** | [`RuntimeMechanics/Dialogue/2. GenericAndScalableDialogueSystem/`](https://github.com/vijit101/UnityMechanicsFramework/tree/main/RuntimeMechanics/Dailogue/2.%20GenericAndScalableDialogueSystem) |
| **Category** | Dialogue / Narrative |
| **Demo Scene** | `Samples~/DialogueExample/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Tutorial](https://github.com/vijit101/UnityMechanicsFramework/tree/main/Samples~/dailogueSample/Video) |

**What it does**

A `ScriptableObject`-based dialogue framework for building flexible, branching conversations in Unity. Scale from a single NPC exchange to a full narrative tree without ever modifying the core system. New dialogue is added as data not code.

**How to use it**
 Note to maintainer: need to fix the part for how to use the dialogue system later / for those using it, find the video and watch it  
```csharp
using GameplayMechanicsUMFOSS.Dialogue;

// Step 1: Create DialogueNode ScriptableObjects in the Inspector
// Step 2: Link them into a DialogueDatabase asset
// Step 3: Reference the database from your DialogueSystem component

[SerializeField] private DialogueSystem dialogueSystem;
[SerializeField] private DialogueDatabase npcDatabase;

// Step 4: Start a conversation
dialogueSystem.StartDialogue(npcDatabase, onComplete: () =>
{
    Debug.Log("Conversation finished.");
});
```

**Highlights**

- Fully data-driven — all dialogue lives in ScriptableObject assets, not in code
- Supports branching and multi-path dialogue trees
- Clean separation between data (`DialogueDatabase`) and logic (`DialogueSystem`)
- Add new conversations without touching any existing scripts
- Scales to large narrative systems without architectural changes

---

### 27. Boomerang Weapon System

| | |
|---|---|
| **Author** | [Shrinibas Mahanta](https://github.com/2k4sm), [Shreyas Garg](https://github.com/shreyas-garg), [Sudharsan](https://github.com/Bug-Finderr) |
| **Namespace** | `GameplayMechanicsUMFOSS.Combat` |
| **Location** | `Runtime/Combat/BoomerangWeapon/BoomerangWeapon_UMFOSS.cs` |
| **Category** | Combat |
| **Demo Scene** | `Samples~/BoomerangWeapon/Assets/Scenes/RecallDemo.unity` |
| **Video** | [▶ Watch Walkthrough](https://github.com/vijit101/UnityMechanicsFramework/tree/main/Samples~/BoomerangWeapon/BoomerangWeaponDemoVideo.zip) |

**What it does**

A throw-and-recall weapon system inspired by God of War's Leviathan Axe. Throw any 3D object, embed it in walls or moving platforms, and recall it along a curved Bezier path back to the player's hand.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.Combat;

[SerializeField] private BoomerangWeapon_UMFOSS weapon;

// Throw toward camera forward
weapon.Throw(Camera.main.transform.forward);

// Recall back to hand
weapon.Recall();

// React to events from anywhere
EventBus.Subscribe<WeaponCaughtEvent>(e => Debug.Log("Caught!"));
```

**Highlights**

- 4-state machine (Equipped, Thrown, Embedded, Recalling) with clean physics handoffs via IPhysicsAdapter
- Bezier curve return path with accelerating speed for a satisfying catch
- Parents to hit surfaces on impact, works with moving platforms out of the box

---

### 6. Screen Shake System

| | |
|---|---|
| **Author** | [Paramjeet Kaur](https://github.com/kauxp) |
| **Namespace** | `GameplayMechanicsUMFOSS.Systems` |
| **Location** | `Runtime/Systems/ScreenShake/ScreenShakeSystem_UMFOSS.cs` |
| **Category** | Systems |
| **Demo Scene** | `Samples~/ScreenShakeExample/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Walkthrough](Samples~/ScreenShakeExample/Video/ScreenShakeTutorial.mp4) |

**What it does**

A trauma-based camera shake system for Unity. Adds smooth positional and rotational shake for impacts, explosions, or heavy actions. Can be triggered via buttons or programmatically. Works in both 2D and 3D games. Handles multiple triggers, ensures smooth decay, and returns the camera to its original position with zero drift.

**How to use it**

1. Attach `ScreenShakeSystem_UMFOSS` to any GameObject (e.g., a background object).  
2. Set shake parameters in the Inspector:  
   - **ShakeDecay** — how fast shake fades  
   - **TraumaMultiplier** — intensity scaling  
   - **PositionMagnitude** — positional shake strength  
   - **RotationMagnitude** — rotational shake strength  
3. Add `ShakeDemoButtons` script to a Canvas UI Button and set `magnitude` and `duration`.  

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.ScreenShake
{
    public class ShakeDemoButtons : MonoBehaviour
    {
        [SerializeField] public float magnitude;
        [SerializeField] public float duration;

        public void Trigger()
        {
            ScreenShakeSystem_UMFOSS.Instance.TriggerShake(magnitude, duration);
        }
    }
}
```

4. In the Button’s `OnClick()`, assign the `Trigger()` method of `ShakeDemoButtons`.  



#### Highlights

- Trauma-based design — smooth shake intensity that decays naturally; multiple hits stack
- Uses Perlin noise instead of random to generate smooth, jitter-free camera motion
- Singleton architecture — any script can trigger shake in one line (Instance.TriggerShake)

------

### 25. Spawner System

| | |
|---|---|
| **Issue** | [#25 — Spawner System](https://github.com/vijit101/UnityMechanicsFramework/issues/25) |
| **Author** | [Naresh Kumar Thodupunoori](https://github.com/Naresh-Kumar-Thodupunoori) |
| **Namespace** | `GameplayMechanicsUMFOSS.World` |
| **Location** | [`Runtime/Mechanic/SpawnerSystem/Scripts/`](Runtime/Mechanic/SpawnerSystem/Scripts/) — `WaveSpawner_UMFOSS`, `TimedSpawner_UMFOSS`, `ProximitySpawner_UMFOSS`, `SpawnProfile_UMFOSS`, `SpawnPoint_UMFOSS`, `SpawnerSpawnExecution`, `SpawnerTrackedEntity_UMFOSS`, `SpawnerEvents_UMFOSS`, `SpawnerEnums` |
| **Script Explainers** | [`Runtime/Mechanic/SpawnerSystem/Script_Explainers/`](Runtime/Mechanic/SpawnerSystem/Script_Explainers/) (one per script) |
| **Category** | World / Spawning |
| **UPM sample** | Package Manager → **Samples** → import **Spawner System** (`Sample/SpawnerSystem/`) |
| **Demo project** | Unzip [Spwaner_System.zip](./Sample/SpawnerSystem/Spwaner_System.zip) and open in Unity — demo scene: `Assets/Scenes/DemoScene.unity` |
| **Extra samples** | [`Samples~/SpawnerSystem/SpawnerSystem.zip`](Samples~/SpawnerSystem/SpawnerSystem.zip), [`SpawnerSystemVideos.zip`](Samples~/SpawnerSystem/SpawnerSystemVideos.zip) |
| **Video** | [▶ Watch tutorial](./Sample/SpawnerSystem/SpawnerSystemTutorial.mp4.zip) |

**What it does**

ScriptableObject-driven spawning for three patterns: **wave-based** rounds, **timed** interval respawns (with optional refill on defeat), and **proximity-triggered** bursts using `Physics2D.OverlapCircle` (no trigger collider required on the spawner). All runtime spawning goes through `ObjectPoolManager_UMFOSS`; active counts use `SpawnerTrackedEntity_UMFOSS` and lifecycle events instead of polling for destroyed objects.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.World;

// 1. Create SpawnProfile_UMFOSS assets (Create → UMFOSS/World/SpawnProfile) with entries, weights, and caps.
// 2. Place SpawnPoint_UMFOSS objects in the scene.
// 3. Add WaveSpawner_UMFOSS, TimedSpawner_UMFOSS, or ProximitySpawner_UMFOSS; assign profiles + spawn points.
// 4. Ensure ObjectPoolManager_UMFOSS exists and pools are warmed for each prefab in the profile.

// Wave example — start from the Inspector or code:
GetComponent<WaveSpawner_UMFOSS>().StartWaves();

// Listen for wave lifecycle (Event Bus — types may change when the unified bus lands)
EventBus.Subscribe<OnWaveStartedEvent>(e => Debug.Log($"Wave {e.WaveNumber}/{e.TotalWaves}"));
EventBus.Subscribe<OnSpawnCountChangedEvent>(e => Debug.Log($"Active: {e.ActiveCount}/{e.MaxCount}"));
```

**Highlights**

- Three spawner components (`WaveSpawner_UMFOSS`, `TimedSpawner_UMFOSS`, `ProximitySpawner_UMFOSS`) sharing one `SpawnProfile_UMFOSS` data layer
- Weighted random selection for mixed enemy types (timed/proximity single picks; waves iterate per entry row)
- Pause-aware via `GamePausedEvent` and `TimerUtility_UMFOSS` for timed spawns
- `AnimationCurve` count/delay scaling optional — flat curves at `1` preserve default behaviour
- Event Bus payloads in `SpawnerEvents_UMFOSS.cs` (e.g. `OnWaveStartedEvent`, `OnWaveClearedEvent`, `OnTimedSpawnTriggeredEvent`, `OnProximitySpawnTriggeredEvent`, `OnSpawnCountChangedEvent`)

> **Event Bus note:** This mechanic uses the current `EventBus` API. When the maintainer ships the **common Event Bus**, event struct names or namespaces may be updated — subscribe via `EventBus.Subscribe<T>` and watch repo release notes for a short migration.

---

### 4. Utils

| | |
|---|---|
| **Author** | [Shubham](https://github.com/vijit101) |
| **Namespace** | `GameplayMechanicsUMFOSS.Utils`, `GameplayMechanicsUMFOSS.Core` |
| **Location** | `Runtime/Utils/` (`TimerUtility_UMFOSS`, `ObjectPoolManager_UMFOSS`, …), `Runtime/Utils/Utils.cs` |
| **Category** | Core / Helpers |
| **Demo Scene** | — |
| **Video** | — |

**What it does**

Shared helpers used across mechanics: pause-aware timers (`TimerUtility_UMFOSS`), object pooling (`ObjectPoolManager_UMFOSS`), and small static utilities such as scene reload helpers (`Utils`).

**How to use it**

```csharp
// Example: schedule a one-shot callback
TimerUtility_UMFOSS.Instance.ScheduleOnce(2f, () => Debug.Log("Fired"));

// Example: reload active scene
GameplayMechanicsUMFOSS.Core.Utils.ReloadLvl();
```

**Highlights**

- Centralized timers that respect `GamePausedEvent`
- Pooling integration for spawn-heavy mechanics
- Lightweight static helpers for common scene flow

---

### 3. Scene Manager System

| | |
|---|---|
| **Author** | [Nymish](https://github.com/nymishkash) |
| **Namespace** | `GameplayMechanicsUMFOSS.Systems` |
| **Location** | `Runtime/Systems/1. SceneManagerSystem/Scripts/` |
| **Script Explainers** | `Runtime/Systems/1. SceneManagerSystem/Script_Explainers/` (one per script) |
| **Category** | Systems |
| **Sample Project** | `Samples~/SceneManagerSample/SceneManagerSystemCompleteProject.zip` (extract & open in Unity) |
| **Video** | [▶ Watch Walkthrough](Samples~/SceneManagerSample/SceneManagerVideos.zip) |

**What it does**

A centralized async scene management system that solves four real-world problems with Unity's built-in `SceneManager`: main-thread blocking on load, singleton destruction across scene changes, missing fade transitions, and zero support for additive overlay scenes (pause menus, inventory, settings). Ships with a persistent-scene pattern that keeps your singletons alive across every load, fade transitions as ScriptableObject assets, an auto-created fade canvas (zero manual UI setup), a stack-based push/pop API for overlays, and a full EventBus integration so any other system can react to scene transitions without holding a direct reference.
### 3. Modular Jump System

| | |
|---|---|
| **Author** | [Ankur Kalita](https://github.com/ankur-kalita) |
| **Namespace** | `GameplayMechanicsUMFOSS.Movement` / `GameplayMechanicsUMFOSS.Physics` |
| **Location** | `Runtime/Mechanic/ModularJumpSystem/Scripts/` |
| **Script Explainers** | `Runtime/Mechanic/ModularJumpSystem/Script_Explainers/` |
| **Category** | Movement |
| **Demo Scene** | Included in `Samples~/JumpSystemSample/JumpSystemProjectZip.zip` |
| **Video** | [▶ Watch Walkthrough](./Samples~/JumpSystemSample/Video/ModularJumpImpl.mp4.zip) |

**What it does**

A fully modular, configurable jump system supporting both 2D and 3D physics via the adapter pattern. Drop it onto any GameObject, pick a dimension mode, and get multi-jump, coyote time, jump buffering, variable jump height, and tunable gravity — all from the Inspector.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.Systems;
using GameplayMechanicsUMFOSS.Core;

// Step 1: Drop SceneManager_UMFOSS + PersistentScene_UMFOSS onto a bootstrap
//         GameObject in your persistent scene. Set persistentSceneName + a default
//         SceneTransition asset in the inspector. The fade canvas is created
//         automatically on Awake — no manual UI wiring needed.

// Step 2: Load a scene with a fade transition
SceneManager_UMFOSS.Instance.LoadScene("Level_01", fadeBlack);

// Step 3: Push an overlay (pause menu, inventory, settings)
SceneManager_UMFOSS.Instance.Push("PauseMenu");
SceneManager_UMFOSS.Instance.Pop(); // close it

// Step 4: React to scene events from anywhere via the EventBus
EventBus.Subscribe<SceneLoadCompleteEvent>(e => Debug.Log($"Loaded {e.sceneName}"));
EventBus.Subscribe<SceneLoadProgressEvent>(e => loadingBar.fillAmount = e.progress);
using GameplayMechanicsUMFOSS.Movement;

// Step 1: Add ModularJumpSystem_UMFOSS component to your player
// Step 2: Select DimensionMode (Mode2D or Mode3D) in Inspector
// Step 3: Assign a Jump InputActionReference, or call methods directly:

ModularJumpSystem_UMFOSS jumpSystem = GetComponent<ModularJumpSystem_UMFOSS>();

// Manual input (when not using InputActionReference)
jumpSystem.OnJumpPressed();
jumpSystem.OnJumpReleased();

// Read state for other systems
bool grounded = jumpSystem.IsGrounded;
float airControl = jumpSystem.AirControlMultiplier;

// Listen to events
jumpSystem.OnJumpStart += () => Debug.Log("Jumped!");
jumpSystem.OnJumpEnd += () => Debug.Log("Landed!");
```

**Highlights**

- **Async-first** — `LoadSceneMode.Additive` + `allowSceneActivation = false` until 90% means no main-thread freeze and no half-loaded flashes
- **Persistent scene pattern** — your `AudioManager`, `SaveSystem`, and HUD singletons survive every transition without scattered `DontDestroyOnLoad` calls
- **Auto-created fade canvas** — drop the prefab in any scene, call `LoadScene`, fades just work; zero inspector wiring required
- **Push / Pop scene stacking** — pause menus, inventory, settings overlays additively load on top of gameplay without unloading the world beneath
- **Seven EventBus events fire across the load lifecycle** — `SceneLoadStart`, `SceneLoadProgress`, `SceneLoadComplete`, `ScenePushed`, `ScenePopped`, `SceneReloaded`, `InputLock` — every other mechanic can hook in without coupling
- **Ships with a full SLITHER snake game demo** — three levels, pause/stats overlays, game-over and victory screens — proving every API surface in a real game flow
- **Adapter pattern** — `IPhysicsAdapter` with `Physics2DAdapter` and `Physics3DAdapter`. Zero duplicated logic between 2D and 3D modes.
- **Platformer-ready** — coyote time, jump buffering, variable jump height, N-jumps, gravity multipliers, and terminal velocity — all configurable from the Inspector
- **Demonstrates the Strategy pattern** — swappable physics backends via interface abstraction, teaching clean dependency inversion in Unity

---

<!--
================================================================
CONTRIBUTOR ENTRY TEMPLATE
================================================================

Copy the block below and fill it in when your PR is merged.
Delete this comment block before committing.

### N. Your Mechanic Name

| | |
|---|---|
| **Author** | [Your Name](https://github.com/your-handle) |
| **Namespace** | `GameplayMechanicsUMFOSS.YourFeatureGroup` |
| **Location** | `Runtime/YourFeatureGroup/YourMechanicScript.cs` |
| **Category** | Movement / Combat / UI / Core / etc. |
| **Demo Scene** | `Samples~/YourMechanicName/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Walkthrough](YOUR_VIDEO_LINK_HERE) |

**What it does**

One or two sentences. What problem does this mechanic solve?
What type of game would use this?

**How to use it**

```csharp
// A minimal working code example showing how to drop this into a project.
// Show the most common use case — keep it short and clear.
```

**Highlights**

- Key architectural point
- Key gameplay feature
- Key learning value (what pattern or concept does this teach?)

Also add a row to the Quick Navigation table above:
| N | [Your Mechanic Name](#n-your-mechanic-name) | Your Name | Category | [▶ Watch](YOUR_VIDEO_LINK) |

================================================================
-->

---

## 7. Namespace Reference

All scripts use `GameplayMechanicsUMFOSS` as the base namespace, extended by feature group.

| Namespace | Purpose | Status |
|---|---|---|
| `GameplayMechanicsUMFOSS.Core` | MonoSingleton, EventBus, StateMachine | ✅ Active |
| `GameplayMechanicsUMFOSS.Physics` | IPhysicsAdapter, 2D/3D adapters | ✅ Active |
| `GameplayMechanicsUMFOSS.Movement` | Jump, Dash, WallSlide | ✅ Active |
| `GameplayMechanicsUMFOSS.Dialogue` | DialogueSystem, nodes, database | ✅ Active |
| `GameplayMechanicsUMFOSS.Input` | InputAdapter | ✅ Active |
| `GameplayMechanicsUMFOSS.Utils` | TimerUtility, helpers | ✅ Active |
| `GameplayMechanicsUMFOSS.World` | Spawner system, spawn points, world tools | ✅ Active |
| `GameplayMechanicsUMFOSS.Inventory` | Item systems, loot, equipment | 🔓 Open for contribution |
| `GameplayMechanicsUMFOSS.Combat` | Boomerang weapon, damage, combat events | ✅ Active |
| `GameplayMechanicsUMFOSS.UI` | HUD, menus, tooltips | 🔓 Open for contribution |
| `GameplayMechanicsUMFOSS.AI` | Patrol, pathfinding, decisions | 🔓 Open for contribution |
| `GameplayMechanicsUMFOSS.Systems` | Save/load, audio, scene management | 🔓 Open for contribution |

---

## 8. Unity Version & Compatibility

| Unity Version | Status |
|---|---|
| Unity 2020.x and below | ❌ Not supported |
| Unity 2021.3 LTS | ✅ Minimum supported |
| Unity 2022.3 LTS | ✅ Recommended |
| Unity 6 | ✅ Supported |

**Additional notes:**
- All mechanics target **2D games** by default. But some Issues and PR's  are beyond 2d or 3d that can be used by all. The `IPhysicsAdapter` layer makes extending to 3D straightforward without modifying the mechanics code
- Compatible with both **Built-In Render Pipeline** and **URP**
- Compatible with both **Legacy Input** and the **new Unity Input System** via `InputAdapter`
- If your mechanic requires additional packages (Cinemachine, TextMeshPro, etc.), declare them in your PR and in your `ScriptExplainer.txt` header

---

## 9. How to Contribute

This library grows with every Pull Request. Every mechanic you contribute is permanently credited to you in the [Mechanics Library](#6-mechanics-library) above, complete with your name, your GitHub profile, and a link to your walkthrough video.

> **Upcoming change (Event Bus):** Maintainers will introduce a **common Event Bus** after reviewing all submissions. You may need to update event types or subscriptions in a follow-up PR once that lands. Until then, use `EventBus.Publish` / `EventBus.Subscribe` with small `struct` event payloads — avoid duplicating bus classes in your mechanic folder.

**The contribution flow at a glance** (details in [CONTRIBUTING.md](./CONTRIBUTING.md)):

```
1.  Open an Issue  →  label: mechanic-proposal  →  describe what you want to build
2.  Fork the repo and create a branch:  mechanic/your-mechanic-name
3.  Build your mechanic inside  Runtime/
4.  Add samples under  Sample/  and/or  Samples~/  (see CONTRIBUTING.md)
5.  Write  ScriptExplainer.txt  (line-by-line code explanation — mandatory)
6.  Record a video walkthrough (zip or linked — mandatory)
7.  Update README.md: Quick Navigation row + full  ### N.  section (N = issue number)
8.  Open a PR titled:  [Mechanic] Add Your Mechanic Name
```

**Your README entry must include:**
- Your GitHub **issue number** in the section heading (e.g. `### 25. Spawner System`)
- Your name linked to your GitHub profile
- **Location** and **Script Explainers** paths under `Runtime/`
- Links to demo/sample assets and your video walkthrough
- A minimal code example showing how to use the mechanic
- A short **What it does** summary and **Highlights** bullets

Read the full [CONTRIBUTING.md](./CONTRIBUTING.md) before you start. It covers folder structure, namespace rules, ScriptExplainer format, video requirements, PR checklist, and code standards.

> **Not sure if your mechanic fits?** Open an Issue with the label `mechanic-proposal` before writing any code. You'll get feedback on scope and design before you invest time building.

---

## 10. License

This repository is distributed under the [MIT License](./LICENSE).

You are free to use any mechanic in this library in personal, commercial, or open-source Unity projects. Attribution is appreciated but not required.

Contributors retain permanent credit in the Mechanics Library for every mechanic they add.

---

<div align="center">

*Built by the Unity community, for the Unity community.*  
*Find a mechanic that saves you time. Contribute one that saves someone else's.* ⭐

</div>
