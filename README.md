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

Open the cloned folder as a Unity project. All packages import automatically via `package.json`.

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

Mechanics never hold direct references to each other. They communicate via events. A jump system never needs to know a sound manager exists.

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
> **Contributors:** when your PR is merged, add your entry here following the format below.  
> See [CONTRIBUTING.md → Section 14](./CONTRIBUTING.md#14-updating-the-mechanics-library-in-readme) for the exact format required.

---

### Quick Navigation

| # | Mechanic | Author | Category | Video |
|---|---|---|---|---|
| 1 | [MonoSingleton Generic](#1-monosingleton-generic) | Shubham B | Core | — |
| 2 | [Generic & Scalable Dialogue System](#2-generic--scalable-dialogue-system) | Mayur | Dialogue | [▶ Watch](https://github.com/vijit101/UnityMechanicsFramework/tree/main/RuntimeMechanics/Dailogue/2.%20GenericAndScalableDialogueSystem/Assets/Video%20tutorial) |
| 3 | [Modular Interaction System](#3-modular-interaction-system) | [Shivam Tiwari](https://github.com/shivam1234100) | Interaction | [▶ Watch](https://github.com/shivam1234100/UnityMechanicsFramework/tree/mechanic/interaction-system/Samples~/InteractionSystem/Video) |

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

A `ScriptableObject`-based dialogue framework for building flexible, branching conversations in Unity. Scale from a single NPC exchange to a full narrative tree without ever modifying the core system. New dialogue is added as data — not code.

**How to use it**
 need to add a fix this doc a bit  / raise an issue
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

### 3. Modular Interaction System

| | |
|---|---|
| **Author** | [Shivam Tiwari](https://github.com/shivam1234100) |
| **Namespace** | `GameplayMechanicsUMFOSS.Interaction` |
| **Location** | `Runtime/Interaction/InteractionController_UMFOSS.cs` |
| **Category** | Interaction |
| **Demo Scene** | `Samples~/InteractionSystem/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Walkthrough](https://github.com/shivam1234100/UnityMechanicsFramework/tree/mechanic/interaction-system/Samples~/InteractionSystem/Video) |

**What it does**

A reusable, modular interaction system that lets any entity — player or AI — interact with any object through one clean interface. Instead of copy-pasting `OnTriggerEnter + tag check + input check` into every interactable, the player gets one controller and every object simply implements `IInteractable_UMFOSS` to decide what happens. Supports instant press and hold-to-interact, three detection modes, and fully decoupled EventBus-driven UI.

**How to use it**

```csharp
using UnityEngine;
using GameplayMechanicsUMFOSS.Interaction;

// Step 1: Implement IInteractable_UMFOSS on any object
public class TreasureChest : MonoBehaviour, IInteractable_UMFOSS
{
    private bool isOpen = false;
    public int Priority => 0;

    public void Interact(GameObject interactor)       { isOpen = true; /* open animation */ }
    public void OnFocused(GameObject interactor)      { /* highlight */ }
    public void OnUnfocused(GameObject interactor)    { /* remove highlight */ }
    public string GetInteractionPrompt()              => "Press E to open";
    public bool CanInteract(GameObject interactor)    => !isOpen;
}

// Step 2: Add InteractionController_UMFOSS to your player
// Step 3: Set the interactable's layer to your Interactable LayerMask
// Done — zero changes to the controller for any new interaction type
```

**Highlights**

- Interface-driven architecture — the controller never knows what it interacts with, making the system infinitely extensible with zero core changes
- Three detection modes (Trigger, OverlapCircle, Raycast) and two selection modes (Nearest, HighestPriority) configurable from the Inspector
- Demonstrates the Observer pattern via EventBus — all UI communication is fully decoupled from gameplay logic

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
| `GameplayMechanicsUMFOSS.Interaction` | InteractionController, IInteractable, InteractionPrompt | ✅ Active |
| `GameplayMechanicsUMFOSS.Inventory` | Item systems, loot, equipment | 🔓 Open for contribution |
| `GameplayMechanicsUMFOSS.Combat` | Hitboxes, damage, status effects | 🔓 Open for contribution |
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
- All mechanics target **2D games** by default. The `IPhysicsAdapter` layer makes extending to 3D straightforward without modifying mechanic code
- Compatible with both **Built-In Render Pipeline** and **URP**
- Compatible with both **Legacy Input** and the **new Unity Input System** via `InputAdapter`
- If your mechanic requires additional packages (Cinemachine, TextMeshPro, etc.), declare them in your PR and in your `ScriptExplainer.txt` header

---

## 9. How to Contribute

This library grows with every Pull Request. Every mechanic you contribute is permanently credited to you in the Mechanics Library above, complete with your name, your GitHub profile, and a link to your walkthrough video.

**The contribution flow:**

```
1.  Open an Issue  →  label: mechanic-proposal  →  describe what you want to build
2.  Fork the repo and create a branch:  mechanic/your-mechanic-name
3.  Build your mechanic inside  Runtime/
4.  Create a self-contained demo scene inside  Samples~/
5.  Write  ScriptExplainer.txt  (line-by-line code explanation)
6.  Record  Demo.mp4  (video walkthrough — mandatory)
7.  Add your entry to the Mechanics Library in this README
8.  Open a PR titled:  [Mechanic] Add Your Mechanic Name
```

**Your README entry must include:**
- Your name linked to your GitHub profile
- A link to your video walkthrough
- A minimal code example showing how to use the mechanic
- A short description and highlights

Read the full [CONTRIBUTING.md](./CONTRIBUTING.md) before you start. It covers everything: folder structure, namespace rules, ScriptExplainer format, video requirements, PR checklist, and code standards.

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
