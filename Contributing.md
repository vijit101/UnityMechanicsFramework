# Contributing to UnityMechanicsFramework

<div align="center">

**A community-driven, plug-and-play gameplay mechanics library for Unity.**

Every mechanic you contribute becomes part of a shared framework used by developers and learners across the Unity ecosystem.  
Build it clean. Build it reusable. Build it for someone else.

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](https://github.com/vijit101/UnityMechanicsFramework/pulls)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

</div>

---

## Table of Contents

1. [Philosophy & Goals](#1-philosophy--goals)
2. [Who Can Contribute](#2-who-can-contribute)
3. [Before You Start](#3-before-you-start)
4. [Setting Up Your Environment](#4-setting-up-your-environment)
5. [Contribution Workflow — Step by Step](#5-contribution-workflow--step-by-step)
6. [Repository Structure Explained](#6-repository-structure-explained)
7. [Mechanic Folder Structure](#7-mechanic-folder-structure)
8. [Namespace Rules](#8-namespace-rules)
9. [Script Writing Standards](#9-script-writing-standards)
10. [Script Explainer Requirement](#10-script-explainer-requirement)
11. [Video Demonstration](#11-video-demonstration)
12. [Prefabs & Scene Setup](#12-prefabs--scene-setup)
13. [Writing Tests](#13-writing-tests)
14. [Updating the Mechanics Library in README](#14-updating-the-mechanics-library-in-readme)
15. [Pull Request Rules](#15-pull-request-rules)
16. [Code Review Process](#16-code-review-process)
17. [Common Mistakes to Avoid](#17-common-mistakes-to-avoid)
18. [Code Quality Expectations](#18-code-quality-expectations)
19. [Unity Version & Compatibility](#19-unity-version--compatibility)
20. [License Agreement](#20-license-agreement)
21. [Contact & Questions](#21-contact--questions)
22. [TLDR](#22-TLDR)

---

## 1. Philosophy & Goals

UnityMechanicsFramework exists because great gameplay systems should not be rewritten from scratch every single project.

The core goals of this repository are:

- **Centralize** reusable, production-ready mechanics in one place
- **Educate** developers by making every mechanic fully documented and explainable
- **Standardize** how Unity mechanics are structured, named, and tested
- **Empower** the community — whether you are a seasoned engineer or learning Unity for the first time

Every contribution must serve all four of these goals simultaneously. A mechanic that works but cannot be understood by a learner does not fit this repository. A mechanic that is educational but poorly architected does not fit either.

**If you are unsure whether your mechanic belongs here, ask yourself:**  
*"Could a developer drop this into any Unity project today and have it working in under 10 minutes?"*  
If yes — it belongs here.

---

## 2. Who Can Contribute

Everyone is welcome to contribute. There are no experience requirements.

| Contributor Type | What You Can Do |
|---|---|
| Learner / Student | Add a well-documented beginner mechanic with a clear explainer |
| Intermediate Developer | Contribute modular systems with proper interfaces and patterns |
| Senior Developer | Add production-grade systems, improve architecture, review PRs |
| Designer | Improve documentation, demo scenes, and ScriptExplainer files |

> If you are a first-time open source contributor, this is a great place to start. Read through this guide fully before opening your first PR.

---

## 3. Before You Start

**Do these three things before writing a single line of code.**

### 3.1 Check for duplicates

Browse the **Mechanics Library** in [README.md](./README.md) and search open [Issues](https://github.com/vijit101/UnityMechanicsFramework/issues).  
If your mechanic already exists, consider improving the existing one instead of adding a new one.

### 3.2 Open an Issue first (for new mechanics)

Before building, open an Issue with the label `mechanic-proposal` describing:

- What the mechanic does
- Why it is useful / what problem it solves
- What Unity systems it relies on (Physics2D, Input System, Animator, etc.)
- Rough folder and namespace placement

This prevents wasted effort if the mechanic is out of scope or already in progress.

### 3.3 Read the existing mechanics

Before writing code, study at least one existing mechanic in the repository — ideally one closest to what you plan to build. Understanding the patterns already in use (MonoSingleton, IPhysicsAdapter, ScriptableObject data) will help you write code that fits naturally into the framework.
If you are into vidoes then this is a video guide for Contributing (but would make more sense once you read this doc)- https://drive.google.com/file/d/113GKIsXtv2R4fxNOHIhpJ-wPA6Ss6MLs/view?usp=sharing 

---

## 4. Setting Up Your Environment

### 4.1 Requirements

| Tool | Version |
|---|---|
| Unity | 2021.3 LTS or later (2022.3 LTS recommended) , any 6.0+ will also work  |
| .NET | Compatible with Unity's built-in C# version |
| Git | Latest stable |
| IDE | Visual Studio / Rider / VS Code with Unity extension |

### 4.2 Fork and Clone

```bash
# 1. Fork the repo via GitHub UI, then:

# 2. Clone your fork
git clone https://github.com/YOUR_USERNAME/UnityMechanicsFramework.git
cd UnityMechanicsFramework

# 3. Add the upstream remote so you can pull future changes
git remote add upstream https://github.com/vijit101/UnityMechanicsFramework.git

# 4. Verify your remotes
git remote -v
# origin    https://github.com/YOUR_USERNAME/UnityMechanicsFramework.git (fetch)
# upstream  https://github.com/vijit101/UnityMechanicsFramework.git (fetch)
```

### 4.3 Open in Unity

Open the cloned folder as a Unity project. Unity will import all packages from `package.json` automatically.

Do **not** commit any Unity auto-generated files that are already covered in `.gitignore`.

### 4.4 Create your working branch

```bash
# Always branch off main
git checkout main
git pull upstream main

# Name your branch after your mechanic
git checkout -b mechanic/your-mechanic-name

# Examples:
# mechanic/inventory-system
# mechanic/wall-slide-controller
# mechanic/camera-follow-2d
```

---

## 5. Contribution Workflow — Step by Step

Follow these steps **in order**. Skipping any step is grounds for a PR rejection.

```
Step 1  ->  Open an Issue  (label: mechanic-proposal)  Describe what you want to build or pick an issue and ask to work on it 
Step 2  ->  Fork the repository and set up locally
Step 3  ->  Create a branch:  mechanic/your-mechanic-name
Step 4  ->  Build your mechanic scripts inside  Runtime/
Step 5  ->  Create your self-contained demo scene inside  Samples~/
Step 6  ->  Write  ScriptExplainer.txt  (line-by-line code explanation)
Step 7  ->  Record  Demo.mp4  (mandatory video walkthrough — see Section 11)
Step 8  ->  Write or update tests inside  Tests/
Step 9  ->  Update README.md  in TWO places:
              9a. Add a row to the Quick Navigation table
              9b. Add your full mechanic documentation card  (see Section 14)
Step 10 ->  Commit and push your branch
Step 11 ->  Open a Pull Request:  [Mechanic] Add Your Mechanic Name
Step 12 ->  Respond to reviewer feedback promptly
```

> **Step 9 is not optional.** The README is the living index of this entire framework. Every user who visits the repo navigates it to find mechanics. If your entry is missing, incomplete, or has no video link, your PR will not be merged.

---

## 6. Repository Structure Explained

Understanding where everything lives is essential before contributing.

```
UnityMechanicsFramework/
|
+-- Runtime/                        <- All mechanic scripts live here
|   +-- Core/                       <- Foundational systems used by other mechanics
|   |   +-- MonoSingleton.cs        <- Generic singleton base class
|   |   +-- EventBus.cs             <- Decoupled event communication
|   |   +-- StateMachine/           <- Reusable finite state machine
|   |
|   +-- Physics/                    <- Platform-agnostic physics layer
|   |   +-- IPhysicsAdapter.cs      <- Interface - never reference Rigidbody2D directly
|   |   +-- Physics2DAdapter.cs     <- 2D implementation
|   |   +-- Physics3DAdapter.cs     <- 3D implementation
|   |
|   +-- Movement/                   <- All movement-related mechanics
|   |   +-- ModularJumpSystem.cs
|   |   +-- DashSystem.cs
|   |   +-- WallSlideSystem.cs
|   |
|   +-- Dialogue/                   <- Dialogue and narrative systems
|   |   +-- DialogueSystem.cs
|   |   +-- DialogueNode.cs
|   |   +-- DialogueDatabase.cs
|   |
|   +-- Input/                      <- Input abstraction layer
|   |   +-- InputAdapter.cs
|   |
|   +-- Utils/                      <- Shared helpers and utilities
|       +-- TimerUtility.cs
|
+-- Editor/                         <- Editor-only tools (NOT included in builds)
|   +-- CustomInspectors/           <- Custom Inspector drawers
|   +-- PropertyDrawers/            <- Custom property drawers
|
+-- Samples~/                       <- Demo scenes and runnable examples
|   +-- JumpExample/
|   +-- DialogueExample/
|   +-- YourMechanicExample/        <- Your demo goes here
|
+-- Tests/
|   +-- Runtime/                    <- Play mode tests
|   +-- Editor/                     <- Edit mode tests
|
+-- README.md
+-- CONTRIBUTING.md
+-- LICENSE
+-- package.json
```

### Where does your mechanic go?

| Mechanic Type | Script Location | Sample Location |
|---|---|---|
| Movement (jump, dash, slide) | `Runtime/Movement/` | `Samples~/YourMechanicExample/` |
| Physics utilities | `Runtime/Physics/` | `Samples~/YourMechanicExample/` |
| UI / Inventory / Items | Create `Runtime/UI/` or `Runtime/Inventory/` | `Samples~/YourMechanicExample/` |
| Core base classes | `Runtime/Core/` | N/A (used by other mechanics) |
| Helpers / Extensions | `Runtime/Utils/` | N/A |

|Note : Only the script and demo video, code explainer be added here 
---

## 7. Mechanic Folder Structure

Your demo and supporting files live under `Samples~/`. Follow this structure exactly.

```
Samples~/
+-- YourMechanicNameSample/
    |
    +-- Assets/
    |   |
    |   +-- Scenes/
    |   |   +-- DemoScene.unity              <- Must run immediately on Play
    |   |
    |   +-- Scripts/
    |   |   +-- YourMechanicScript.cs        <- Mirror of what's in Runtime/
    |   |
    |   +-- Prefabs/
    |       +-- RequiredPrefab.prefab        <- Every required prefab included
    |
    +-- Video/
    |   +-- [YourMechanicNameTutorial].mp4   <- Named after the mechanic
    |
    +-- ScriptExplainer.txt                  <- Line-by-line code explanation
```

### Golden rule

**Clone -> Open DemoScene -> Press Play -> Mechanic works.**  
No missing prefabs. No broken references. No manual configuration required.  
If it does not work immediately on a clean clone, the PR will be rejected.

---

## 8. Namespace Rules

All scripts must use the base namespace `GameplayMechanicsUMFOSS` extended with the appropriate feature group.

### 8.1 Namespace map

| Namespace | Use for |
|---|---|
| `GameplayMechanicsUMFOSS.Core` | Singletons, EventBus, StateMachine, base classes ,anything core to any game can be added here  |
| `GameplayMechanicsUMFOSS.Movement` | Jump, dash, wall slide, swim, fly mechanics |
| `GameplayMechanicsUMFOSS.Dialogue` | Dialogue nodes, databases, conversation systems |
| `GameplayMechanicsUMFOSS.Inventory` | Items, loot, equipment, currency systems |
| `GameplayMechanicsUMFOSS.Systems` | Save/load, audio, scene management systems |
| `GameplayMechanicsUMFOSS.UI` | HUD, menus, health bars, tooltip systems |
| `GameplayMechanicsUMFOSS.Combat` | Attacks, hitboxes, damage, status effects |
| `GameplayMechanicsUMFOSS.AI` | Pathfinding, patrol, decision-making systems |
| `GameplayMechanicsUMFOSS.Utils` | Timers, object pooling, extension methods |
| `GameplayMechanicsUMFOSS.Samples.Jump` | Sample scripts for the Jump mechanic |

### 8.2 Usage example

```csharp
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// A modular, configurable jump controller with coyote time and jump buffering.
    /// Attach to the player root GameObject alongside a Rigidbody2D and Physics2DAdapter.
    /// </summary>
    public class ModularJumpSystem : MonoBehaviour
    {
        [Header("Jump Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float coyoteTimeDuration = 0.15f;
        [SerializeField] private float jumpBufferDuration = 0.1f;

        // ...
    }
}
```

### 8.3 Rules

- **Never** use the global namespace (no namespace at all)
- **Never** nest more than one level beyond the feature group
- **Always** use PascalCase for class names , for the rest use camelCase
- **Always** match the namespace to the `Runtime/` subfolder the script lives in

---

## 9. Script Writing Standards

### 9.1 Class structure order

Maintain this ordering inside every class for consistency:

```csharp
// 1. Serialized / Inspector fields (with [Header] grouping)
// 2. Private fields
// 3. Public properties
// 4. Unity lifecycle methods (Awake, Start, OnEnable, Update, FixedUpdate, OnDisable, OnDestroy)
// 5. Public methods
// 6. Private methods
// 7. Editor-only code inside #if UNITY_EDITOR blocks
```

### 9.2 Header grouping

Group related inspector fields with `[Header]` attributes:

```csharp
[Header("Movement Settings")]
[SerializeField] private float moveSpeed = 5f;
[SerializeField] private float acceleration = 10f;

[Header("Jump Settings")]
[SerializeField] private float jumpForce = 12f;
[SerializeField] private int maxJumps = 2;

[Header("References")]
[SerializeField] private Rigidbody2D rb;
```

### 9.3 No magic numbers

```csharp
// Bad
if (health < 20) TriggerLowHealthWarning();

// Good
private const float LOW_HEALTH_THRESHOLD = 20f;
if (health < LOW_HEALTH_THRESHOLD) TriggerLowHealthWarning();
```

### 9.4 Use interfaces for dependencies

```csharp
// Bad - hardcoded to 2D physics
[SerializeField] private Rigidbody2D rb;
rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

// Good - works with both 2D and 3D via adapter
[SerializeField] private IPhysicsAdapter physics;
physics.AddForce(Vector2.up * jumpForce);
```

### 9.5 Events over direct references

Use the built-in `EventBus` for cross-system communication. Do not create direct references between mechanics.

```csharp
// Publishing an event
EventBus.Publish(new PlayerJumpedEvent { height = jumpForce });

// Subscribing to an event
EventBus.Subscribe<PlayerJumpedEvent>(OnPlayerJumped);
```

### 9.6 XML documentation on public APIs

```csharp
/// <summary>
/// Triggers the jump mechanic. Safe to call from any input handler.
/// Will respect coyote time and jump buffering windows.
/// </summary>
/// <param name="forceMultiplier">Optional multiplier applied to base jump force. Default is 1.0.</param>
public void TriggerJump(float forceMultiplier = 1f)
{
    // ...
}
```

---

## 10. Script Explainer Requirement

Every mechanic must include a `yourScriptNameExplainer.txt`. This is **mandatory and non-negotiable**.

This file is what separates UnityMechanicsFramework from every other Unity GitHub repo. It transforms a script into a lesson.

### 10.1 Format

```
====================================================
SCRIPT: ModularJumpSystem.cs
AUTHOR: Your Name
NAMESPACE: GameplayMechanicsUMFOSS.Movement
====================================================

PURPOSE:
A configurable jump controller implementing coyote time (grace period after
walking off a ledge) and jump buffering (queuing a jump input slightly before landing).

----------------------------------------------------

Code    : using UnityEngine;
Explain : Imports the Unity engine namespace. Required for MonoBehaviour, Vector2,
          Rigidbody2D and all core Unity types.

Code    : namespace GameplayMechanicsUMFOSS.Movement
Explain : Places this class inside the Movement feature group of the framework's
          namespace hierarchy. Prevents naming conflicts with other assemblies.

Code    : public class ModularJumpSystem : MonoBehaviour
Explain : Declares the class and inherits from MonoBehaviour so it can be
          attached to a Unity GameObject as a component.

Code    : [SerializeField] private float jumpForce = 12f;
Explain : Exposes jumpForce in the Unity Inspector while keeping it private in code.
          The default value of 12 is a good starting point for most 2D platformers.

Code    : private float coyoteTimeCounter;
Explain : Tracks how much coyote time is remaining. Decremented in Update().
          When above zero, the player can still jump even after leaving a platform.

Code    : public void TriggerJump(float forceMultiplier = 1f)
Explain : The main entry point for triggering a jump. Takes an optional multiplier
          so external systems (like a powerup) can modify jump height without
          changing the base configuration.
```

### 10.2 What makes a good explainer

- Explain **why**, not just **what** — *"Decremented in Update() so it counts down in real time"* not just *"a counter"*
- Cover Unity-specific behaviour that a learner might not know (`ForceMode2D.Impulse`, `Physics2D.OverlapCircle`, etc.)
- Explain design decisions — why you chose this pattern over a simpler one
- Keep language simple and direct — write for someone one step below your level

---

## 11. Video Demonstration

A demo video is **mandatory** for every mechanic contribution. No video = PR rejected.

### 11.1 What the video must cover

Your video must include all of the following sections (in any order):

1. **Introduction** — Your name, the mechanic name, and one sentence on what it does
2. **Problem statement** — What problem does this mechanic solve? Why would a developer need it?
3. **Scene walkthrough** — Show the DemoScene hierarchy, which GameObjects are used, what components are attached
4. **Prefab setup** — Walk through any required prefabs and their configuration
5. **Inspector settings** — Show and explain every serialized field in the Inspector
6. **Live demo** — Run the game in Play Mode and demonstrate the mechanic in action
7. **Script walkthrough** — Open the script and explain the key sections of code

### 11.2 Video specifications

| Property | Requirement |
|---|---|
| Format | `.mp4` (H.264 encoding preferred) |
| Resolution | 720p minimum (1080p preferred) |
| Frame rate | 30fps minimum |
| Length | 3–8 minutes (be thorough but focused) |
| File size | Compress to under 100MB if possible |
| Audio | A voiceover is strongly recommended |
| Captions | Optional but appreciated for accessibility |

### 11.3 Where to put the video and how to link it

- **Under 100MB** — Create a `Video` folder inside your sample directory (e.g., `Samples~/YourMechanicNameSample/Video/`) and name your video `[YourMechanicNameTutorial].mp4`. Link to it in your README entry using a relative GitHub path.
- **Over 100MB** — Upload to YouTube (unlisted is fine) or Google Drive and paste the full URL in:
  - Your README mechanic entry (metadata table + Quick Navigation row)
  - Your PR description
  - The header of your `ScriptExplainer.txt`

> **Your video link must be live and accessible at the time your PR is submitted.** A broken or missing link will block the PR from being merged, since the README is the public documentation and every user will click it.

### 11.4 Recording tools

Any screen recorder works. Recommended options:
- **OBS Studio** (free, cross-platform)
- **Loom** (easy sharing)
- **ShareX** (Windows)
- **QuickTime** (macOS)

---

## 12. Prefabs & Scene Setup

### 12.1 Prefab requirements

- Every required prefab must be included in `Prefabs/`
- Prefabs must have all component references pre-wired — no missing references
- Name prefabs clearly: `Player_JumpController.prefab` not `Prefab1.prefab`
- If your mechanic modifies a shared prefab (like a generic Player), create a variant instead

### 12.2 Demo scene requirements

- The scene must be self-contained — it cannot depend on assets outside your mechanic's folder
- Name it `DemoScene.unity`
- The scene should load and run correctly the moment someone presses Play on a fresh clone
- Include a simple environment (ground, walls, platforms) so the mechanic is immediately visible and testable
- Add UI labels (`TextMeshPro` or legacy UI) to tell the viewer what controls to use

### 12.3 Configuration instructions

If any manual setup is genuinely required that cannot be avoided, include a `README.txt` inside your mechanic folder with numbered setup steps or show it in the video on how to setup the prefab. Keep it as short as possible.

```
SETUP INSTRUCTIONS - InventorySystem

1. Open Samples~/InventorySystem/Assets/Scenes/DemoScene.unity
2. Press Play
3. Press I to open/close the inventory
4. Left-click items to pick them up

No additional setup required.
```

---

## 13. Writing Tests

Tests are not mandatory for your first contribution but are **strongly encouraged**.  
Mechanics with tests will be prioritised in code review.

### 13.1 Where tests live

```
Tests/
+-- Runtime/     <- Play mode tests (test mechanics that need physics, time, etc.)
+-- Editor/      <- Edit mode tests (test pure logic, data, ScriptableObjects)
```

### 13.2 Naming convention

```
Tests/Runtime/Movement/ModularJumpSystemTests.cs
Tests/Editor/Inventory/InventoryDatabaseTests.cs
```

### 13.3 Basic test example

```csharp
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using GameplayMechanicsUMFOSS.Movement;

public class ModularJumpSystemTests
{
    [UnityTest]
    public IEnumerator JumpSystem_AppliesForce_WhenGrounded()
    {
        // Arrange
        var go = new GameObject();
        go.AddComponent<Rigidbody2D>();
        var jumpSystem = go.AddComponent<ModularJumpSystem>();

        yield return new WaitForFixedUpdate();

        // Act
        jumpSystem.TriggerJump();

        yield return new WaitForFixedUpdate();

        // Assert
        Assert.Greater(go.GetComponent<Rigidbody2D>().velocity.y, 0f,
            "Rigidbody2D should have positive Y velocity after jump");

        Object.Destroy(go);
    }
}
```

---

## 14. Updating the Mechanics Library in README

The `README.md` is the **living index** of this entire framework — it is the first thing every user sees, and the primary way people discover, understand, and use every mechanic in this repo.

When your PR is merged, you are required to update the README in **two places**. Both are mandatory. A PR missing either update will not be approved.

---

### 14.1 Part 1 — Add a row to the Quick Navigation table

The Quick Navigation table lives at the top of the Mechanics Library section. It gives users an instant overview and lets them jump directly to any mechanic entry.

Find this table in `README.md`:

```markdown
| # | Mechanic | Author | Category | Video |
|---|---|---|---|---|
| 1 | [MonoSingleton Generic](#1-monosingleton-generic) | Shubham B | Core | — |
| 2 | [Generic & Scalable Dialogue System](#2-generic--scalable-dialogue-system) | Mayur | Dialogue | [▶ Watch](...) |
```

Add your row at the bottom:

```markdown
| N | [Your Mechanic Name](#n-your-mechanic-name) | [Your Name](https://github.com/your-handle) | Category | [▶ Watch](YOUR_VIDEO_LINK) |
```

**Rules for your nav row:**
- `N` must be the next sequential number
- The anchor link (e.g. `#3-your-mechanic-name`) must exactly match the heading you add in Part 2
- The Category must match one of: `Core`, `Movement`, `Physics`, `Dialogue`, `Inventory`, `Combat`, `UI`, `AI`, `Systems`, `Utils`
- The video link is **mandatory** — if your video is a file in the repo, link to it; if it is hosted externally, paste the full URL
- If no video exists, the PR will not be merged — `—` is not an acceptable value

---

### 14.2 Part 2 — Add your full mechanic documentation card

Below the Quick Navigation table, add your mechanic's full entry. This is the one-stop documentation for every developer who uses your mechanic. It must contain all five parts below.

#### The full entry template

```markdown
### N. Your Mechanic Name

| | |
|---|---|
| **Author** | [Your Name](https://github.com/your-handle) |
| **Namespace** | `GameplayMechanicsUMFOSS.YourFeatureGroup` |
| **Location** | `Runtime/YourFeatureGroup/YourMechanicScript.cs` |
| **Category** | Movement / Combat / UI / Core / etc. |
| **Demo Scene** | `Samples~/YourMechanicName/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Walkthrough](YOUR_VIDEO_LINK) |

**What it does**

One to three sentences. Describe the problem this mechanic solves and what type of game
would use it. Do not describe implementation details here — save that for the code example.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.YourFeatureGroup;

// Step 1: Brief setup comment
YourClass instance = GetComponent<YourClass>();

// Step 2: Show the most common usage
instance.DoTheThing();

// Step 3: Show a typical result or callback if relevant
instance.OnComplete += () => Debug.Log("Done.");
```

**Highlights**
- One key architectural point (e.g. "Interface-driven — supports both 2D and 3D")
- One key gameplay feature (e.g. "Supports coyote time and jump buffering")
- One key learning value (e.g. "Demonstrates the Observer pattern via EventBus")
```

---

### 14.3 What each part must contain

| Part | What is required | Common mistake |
|---|---|---|
| **Metadata table** | All 6 rows filled — author linked to GitHub, video linked | Missing video link, or linking to a folder instead of the video file |
| **What it does** | Problem + use case — no implementation details | Too vague ("it handles jumping") or too technical (explaining internals) |
| **How to use it** | A minimal, working code example a developer can copy into their project | Showing internal implementation instead of external usage |
| **Highlights** | Exactly 3 bullet points — architecture, gameplay, learning value | Repeating the description, or listing Unity features instead of what makes this mechanic special |
| **Quick Nav row** | A correctly formatted row in the nav table with a working anchor and video link | Anchor doesn't match the heading, or video link is missing |

---

### 14.4 Filled example — what a complete entry looks like

```markdown
### 3. Modular Inventory System

| | |
|---|---|
| **Author** | [Vijit](https://github.com/vijit101) |
| **Namespace** | `GameplayMechanicsUMFOSS.Inventory` |
| **Location** | `Runtime/Inventory/InventorySystem.cs` |
| **Category** | Inventory |
| **Demo Scene** | `Samples~/InventorySystem/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch Walkthrough](https://youtube.com/your-link-here) |

**What it does**

A ScriptableObject-driven inventory system for 2D RPG and action games.
Handles item stacking, equipment slots, and runtime save/load without coupling
to any specific player implementation.

**How to use it**

```csharp
using GameplayMechanicsUMFOSS.Inventory;

// Step 1: Add InventorySystem component to your player
InventorySystem inventory = GetComponent<InventorySystem>();

// Step 2: Add an item at runtime
inventory.AddItem(swordItemSO);

// Step 3: React to changes via EventBus
EventBus.Subscribe<ItemAddedEvent>(e => uiManager.RefreshInventoryUI());
```

**Highlights**
- Fully data-driven — all item definitions live in ScriptableObject assets, not code
- Uses EventBus for UI sync, keeping inventory and HUD completely decoupled
- Demonstrates the Command pattern for undo-able add/remove actions

And the Quick Navigation row:

| 3 | [Modular Inventory System](#3-modular-inventory-system) | [Vijit](https://github.com/vijit101) | Inventory | [▶ Watch](https://youtube.com/your-link-here) |
```

---

### 14.5 Where the contributor entry template lives in README.md

There is an HTML comment block inside the Mechanics Library section of `README.md` that contains the copy-paste template. It looks like this when you open the raw file:

```
<!--
================================================================
CONTRIBUTOR ENTRY TEMPLATE
Copy the block below and fill it in when your PR is merged.
Delete this comment block before committing.
...
================================================================
-->
```

Copy the template, fill it in, delete the comment markers, and place your entry below the last existing mechanic. Never edit or delete entries above yours.

---

## 15. Pull Request Rules

### 15.1 Pre-submission checklist

Do not open a PR until every item below is complete.

```
Code & Architecture
  [ ] Code compiles with zero errors
  [ ] Code compiles with zero warnings
  [ ] Correct namespace is used (GameplayMechanicsUMFOSS.FeatureGroup)
  [ ] No hardcoded magic numbers or strings
  [ ] No direct Rigidbody2D references - IPhysicsAdapter used where applicable
  [ ] No direct cross-mechanic dependencies
  [ ] Public APIs have XML <summary> documentation

Files & Structure
  [ ] Mechanic scripts are inside the correct Runtime/ subfolder
  [ ] Demo scene is inside Samples~/YourMechanicName/Assets/Scenes/
  [ ] All required prefabs are inside Samples~/YourMechanicName/Assets/Prefabs/
  [ ] ScriptExplainer.txt is present, complete, and explains the why not just the what
  [ ] Demo.mp4 is present at Samples~/YourMechanicName/Video/ OR an external link is ready

README — Living Index (both updates required)
  [ ] Quick Navigation table has a new row with the correct anchor, category, and working video link
  [ ] Full mechanic documentation card added below the last existing entry
  [ ] Metadata table is complete (all 6 rows filled, author linked to GitHub, video linked)
  [ ] "What it does" section written (problem + use case, no implementation details)
  [ ] "How to use it" section has a working, minimal code example
  [ ] "Highlights" has exactly 3 bullet points (architecture, gameplay feature, learning value)

Demo Scene
  [ ] DemoScene.unity runs immediately on Play with no missing references
  [ ] DemoScene includes UI text labels showing the player controls

Git hygiene
  [ ] Branch is named mechanic/your-mechanic-name
  [ ] Commits are clean and descriptive (not "fix", "wip", "stuff")
  [ ] No Unity auto-generated files outside of .gitignore scope
  [ ] No .DS_Store, Thumbs.db, or other OS artefacts committed
```

### 15.2 PR title format

```
[Mechanic] Add <Mechanic Name>
```

**Valid examples:**
```
[Mechanic] Add Inventory System
[Mechanic] Add Drag & Drop System
[Mechanic] Add Wall Slide Controller
[Mechanic] Add Camera Follow 2D
[Mechanic] Add Object Pooling Manager
```

### 15.3 PR description template

Use this template when opening your PR:

```markdown
## Mechanic Name
<!-- What is the mechanic called? -->

## What does it do?
<!-- One paragraph. What problem does this solve? What type of game would use it? -->

## How to test it
<!-- Numbered steps: what to do after opening DemoScene to see the mechanic work -->

## Demo Video
<!-- Paste your video link here — YouTube, Google Drive, or relative path to Demo.mp4 -->

## Namespace used
<!-- e.g. GameplayMechanicsUMFOSS.Movement -->

## README entry
<!-- Paste a preview of your Quick Navigation row and your full mechanic card here so reviewers can check it before looking at the file -->

## Checklist
- [ ] Compiles with zero errors and zero warnings
- [ ] Folder structure followed exactly
- [ ] ScriptExplainer.txt included and complete
- [ ] DemoScene runs immediately on Play — no missing references
- [ ] Demo video included at Samples~/YourMechanicName/Video/ or linked above
- [ ] README Quick Navigation row added with working anchor and video link
- [ ] README full mechanic card added (metadata table, what it does, code example, highlights)
```

### 15.4 What happens after you submit

- A maintainer will review your PR within a week of submission
- You may receive review comments requesting changes — respond and push updates to the same branch
- Once approved, your mechanic will be merged and your README entry goes live immediately
- Your name, GitHub profile link, and video will be permanently visible in the Mechanics Library to every developer who visits the repo

> PRs that have no activity from the contributor for 14 days after review comments will be closed. You can always reopen.

---

## 16. Code Review Process

Understanding how code review works will help you get your PR merged faster.

### What reviewers check for

| Area | What they look for |
|---|---|
| **Architecture** | Is the mechanic modular? Does it avoid tight coupling? |
| **Namespace** | Correct namespace placement and format |
| **Scripts** | Readability, naming, class structure ordering |
| **Scene** | Does it run immediately? Are all references wired? Controls labelled? |
| **ScriptExplainer** | Is it complete? Does it explain the *why*, not just the *what*? |
| **Video** | Does it exist? Is the link live? Does it cover all 7 required sections? |
| **README — Nav row** | Correct anchor, correct category, working video link |
| **README — Card** | All 6 metadata rows filled, code example works, highlights are specific not generic |

### How to respond to review feedback

- Do **not** argue in comments — if you disagree with feedback, explain your reasoning calmly and the reviewer will discuss it
- Push all requested changes as new commits on the same branch (do not close and reopen the PR)
- After addressing feedback, leave a comment: *"All feedback addressed — ready for re-review"*

---

## 17. Common Mistakes to Avoid

Learn from what gets PRs rejected:

```
Scene & Files
  Missing references in DemoScene (NullReferenceException on Play)
  Mechanic depends on another mechanic not included in the PR
  Prefab references point to assets outside the mechanic's folder
  Demo scene requires machine-specific or absolute paths

Code
  Wrong or missing namespace
  Magic numbers scattered throughout the code
  Direct Rigidbody2D reference instead of IPhysicsAdapter

ScriptExplainer
  Only describes what each line does, not why
  Missing sections — not every meaningful block is explained

Video
  No video at all — PR will be rejected outright
  Video link in README is broken or points to a private file
  Video does not show the mechanic actually running in Play Mode
  Video is too short — doesn't cover the 7 required sections

README — the most commonly missed
  Quick Navigation row is missing entirely
  Quick Navigation anchor doesn't match the heading (broken link)
  Video link in the nav table is missing or shows "—"
  Metadata table is incomplete (missing rows, author not linked to GitHub)
  "How to use it" shows internal implementation instead of how to call the mechanic
  "Highlights" bullet points are generic ("clean code", "easy to use") not specific
  README entry placed in the wrong location (not below the last existing entry)

Git
  .meta files committed without the corresponding asset
  .DS_Store or Thumbs.db committed
  Branch named "main", "fix", "update" or anything non-descriptive
  PR title not following [Mechanic] Add X format
```

---

## 18. Code Quality Expectations

This repository aims to be production-ready. Write code as if it is shipping in a real game tomorrow.

### Design principles

**Modular** — Each mechanic is fully self-contained. It does not reach into other mechanics.

**Decoupled** — Use `IPhysicsAdapter`, `EventBus`, and ScriptableObjects to communicate. Never grab a direct reference to another mechanic's component.

**Readable** — Code is read far more than it is written. Name things clearly. A new developer reading your script should understand it without needing comments on every line.

**Reusable** — Your mechanic should drop into any Unity project. No hardcoded GameObjects, no assumed scene structure.

**Tested** — Where possible, include a test that proves the mechanic's core behaviour.

### Naming guide

| Item | Convention | Example |
|---|---|---|
| Classes | PascalCase | `ModularJumpSystem` |
| Methods | PascalCase | `TriggerJump()` |
| Private fields | camelCase | `jumpForce` |
| Serialized fields | camelCase | `[SerializeField] private float jumpForce` |
| Constants | UPPER_SNAKE_CASE | `MAX_JUMP_COUNT` |
| Interfaces | IPascalCase | `IPhysicsAdapter` |
| Events | PascalCase + Event suffix | `PlayerJumpedEvent` |

---

## 19. Unity Version & Compatibility

| Version | Status |
|---|---|
| Unity 2021.3 LTS | Minimum supported |
| Unity 2022.3 LTS | Recommended |
| Unity 6 | Supported (test before submitting) |
| Unity 2020.x and below | Not supported |

If your mechanic uses any Unity packages beyond the defaults (e.g. Input System, Cinemachine, TextMeshPro), list them explicitly in your PR description and in your `ScriptExplainer.txt` header.

---

## 20. License Agreement

By opening a Pull Request to this repository, you confirm and agree to all of the following:

1. Your contribution will be distributed under the repository's [MIT License](./LICENSE)
2. The code you are contributing is your **own original work**
3. Your contribution is **not owned by or derived from** another organisation's codebase
4. Your contribution does **not violate** any third-party license or intellectual property rights
5. If a copyright or IP infringement issue arises from your contribution, you accept **full personal responsibility**

The repository owner reserves the right to remove any contribution found to be in violation of these terms without prior notice.

---

## 21. Contact & Questions

Open an **Issue** before starting work on any new mechanic — this is the best way to get feedback before you spend time building.

| Reason | Action |
|---|---|
| Proposing a new mechanic | Open Issue with label: `mechanic-proposal` |
| Reporting a bug in an existing mechanic | Open Issue with label: `bug` |
| Asking a question about contribution process | Open Issue with label: `question` |
| Suggesting improvements to the framework architecture | Open Issue with label: `enhancement` |

---

<div align="center">

## 22. TLDR 

# How To Submit Your Mechanic

---

## The Rule Before Everything

> **Build and test in your own Unity project first.**
> Only move to the UMF repo once your mechanic works perfectly in isolation.
> This saves you hours of pain.

---

## Step 1 — Build In Your Own Project

Create a brand new Unity project. Build your mechanic there. Test it until it works with zero console errors. Press Play — it should just work.

While you are here:
- Write your `ScriptExplainer.txt`
- Build your demo scene
- Record your video (upload to YouTube unlisted or Google Drive)

---

## Step 2 — Fork and Clone the UMF Repo

```bash
# Fork on GitHub first, then:
git clone https://github.com/YOUR_USERNAME/UnityMechanicsFramework.git
cd UnityMechanicsFramework
git remote add upstream https://github.com/vijit101/UnityMechanicsFramework.git
git pull upstream main
git checkout -b mechanic/your-mechanic-name
```

---

## Step 3 — Place Your Files

**Script goes here:**
```
Runtime/YourCategory/YourScript_UMFOSS.cs
```

**Everything else goes here:**
```
Samples~/YourMechanicNameSample/
  Assets/
    Scenes/    → DemoScene.unity
    Scripts/   → copy of your script
    Prefabs/   → any prefabs the scene needs
  Video/       → YourMechanicNameTutorial.mp4  (if under 100MB)
  ScriptExplainer.txt
```

---

## Step 4 — The Meta Files Rule

> ⚠️ This is the most Unity-specific thing and the most commonly missed.

Every Unity file has a `.meta` file alongside it. **Always copy both together.**

```
DemoScene.unity       ✅ copy
DemoScene.unity.meta  ✅ copy this too

MyPrefab.prefab       ✅ copy
MyPrefab.prefab.meta  ✅ copy this too
```

Missing `.meta` files = broken references when someone else opens the project = PR rejected.

**Safest way:** Copy files through Explorer/Finder while Unity is closed. Open Unity after.

---

## Step 5 — Open UMF in Unity and Verify

Open the cloned UMF repo in Unity. Open your `DemoScene.unity`.

```
✅ Zero errors in console
✅ Press Play — mechanic works immediately
✅ No missing scripts, no pink objects
```

Fix anything broken before moving on.

---

## Step 6 — Update the README

In `README.md` update two things:

**1. Quick Navigation table — add one row:**
```markdown
| N | [Your Mechanic](#n-your-mechanic) | [Your Name](github link) | Category | [▶ Watch](video link) |
```

**2. Full mechanic card — add below last entry:**
```markdown
### N. Your Mechanic Name
| | |
|---|---|
| **Author** | [Your Name](github link) |
| **Namespace** | `GameplayMechanicsUMFOSS.YourCategory` |
| **Location** | `Runtime/YourCategory/YourScript_UMFOSS.cs` |
| **Category** | Combat / Movement / Systems / etc. |
| **Demo Scene** | `Samples~/YourMechanicNameSample/Assets/Scenes/DemoScene.unity` |
| **Video** | [▶ Watch](your video link) |

**What it does**
One or two sentences. What problem does it solve?

**How to use it**
```csharp
// Minimal working example
```

**Highlights**
- Key architectural point
- Key gameplay feature
- Key learning value
```

---

## Step 7 — Commit and Push

```bash
git add Runtime/YourCategory/YourScript_UMFOSS.cs
git add Runtime/YourCategory/YourScript_UMFOSS.cs.meta
git add "Samples~/YourMechanicNameSample/"
git add README.md
git commit -m "Add YourMechanicName — script, demo, ScriptExplainer, README"
git push origin mechanic/your-mechanic-name
```

---

## Step 8 — Raise the PR

On GitHub — open a PR from your branch to `main`.

**Title:** `[Mechanic] Add Your Mechanic Name`

**Body:**
```
## What it does
One paragraph.

## How to test
1. Open Samples~/YourMechanicNameSample/Assets/Scenes/DemoScene.unity
2. Press Play
3. ...

## Video link
[paste here]

## Namespace
GameplayMechanicsUMFOSS.YourCategory
```

---

## ✅ Pre-PR Checklist

Go through this before you click Submit. Every unchecked box = PR not ready.

**Code**
- [ ] Script compiles — zero errors, zero warnings
- [ ] Namespace is `GameplayMechanicsUMFOSS.FeatureGroup`
- [ ] No magic numbers — all values named as constants
- [ ] No direct `Rigidbody2D` references — `IPhysicsAdapter` used
- [ ] `OnEnable` / `OnDisable` used for EventBus subscriptions
- [ ] `OnDisable` restores any state the script modified (gravity, velocity, input lock)

**Files**
- [ ] Script is in correct `Runtime/` subfolder
- [ ] Demo scene is in `Samples~/YourMechanicNameSample/Assets/Scenes/`
- [ ] Every `.unity` file has its `.meta` file
- [ ] Every `.prefab` file has its `.meta` file
- [ ] Every `.cs` file has its `.meta` file
- [ ] No `Library/`, `Temp/`, or `Logs/` folders staged

**Scene**
- [ ] `DemoScene.unity` plays immediately — zero setup, zero errors
- [ ] No missing script errors in console
- [ ] No pink / magenta objects in scene
- [ ] Controls are labelled on screen with UI text

**Documentation**
- [ ] `ScriptExplainer.txt` complete — explains the WHY not just the what
- [ ] Video uploaded and link is publicly accessible (not private)

**README**
- [ ] Quick Navigation row added — anchor matches heading exactly
- [ ] Video link in nav row is live
- [ ] Full mechanic card added below last existing entry
- [ ] All 6 metadata rows filled
- [ ] Code example in "How to use it" actually works
- [ ] Exactly 3 Highlights bullet points

**Git**
- [ ] Branch named `mechanic/your-mechanic-name`
- [ ] No `.DS_Store`, `Thumbs.db`, or OS files committed
- [ ] Commit message is descriptive

---

**Stuck?** Open an Issue with label `question` and tag @vijit101.
Don't stay blocked for more than 30 minutes without asking.

*Thank you for contributing to UnityMechanicsFramework.*  
*Every mechanic you add is one less system that someone else has to write from scratch.*  
*Build something great.* 🎮

</div>
