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

---

## 4. Setting Up Your Environment

### 4.1 Requirements

| Tool | Version |
|---|---|
| Unity | 2021.3 LTS or later (2022.3 LTS recommended) |
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
Step 1  ->  Open an Issue with your mechanic proposal
Step 2  ->  Fork the repository and set up locally
Step 3  ->  Create a branch: mechanic/your-mechanic-name
Step 4  ->  Build your mechanic scripts inside Runtime/
Step 5  ->  Create your demo scene inside Samples~/
Step 6  ->  Write ScriptExplainer.txt (line-by-line explanation)
Step 7  ->  Record your demo video (Demo.mp4)
Step 8  ->  Write or update tests inside Tests/
Step 9  ->  Update the Mechanics Library in README.md
Step 10 ->  Commit and push your branch
Step 11 ->  Open a Pull Request against main using the correct title format
Step 12 ->  Respond to reviewer feedback promptly
```

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

---

## 7. Mechanic Folder Structure

Your demo and supporting files live under `Samples~/`. Follow this structure exactly.

```
Samples~/
+-- YourMechanicName/
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
    |   +-- Demo.mp4                         <- Your walkthrough video
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
| `GameplayMechanicsUMFOSS.Core` | Singletons, EventBus, StateMachine, base classes |
| `GameplayMechanicsUMFOSS.Movement` | Jump, dash, wall slide, swim, fly mechanics |
| `GameplayMechanicsUMFOSS.Dialogue` | Dialogue nodes, databases, conversation systems |
| `GameplayMechanicsUMFOSS.Inventory` | Items, loot, equipment, currency systems |
| `GameplayMechanicsUMFOSS.Systems` | Save/load, audio, scene management systems |
| `GameplayMechanicsUMFOSS.UI` | HUD, menus, health bars, tooltip systems |
| `GameplayMechanicsUMFOSS.Combat` | Attacks, hitboxes, damage, status effects |
| `GameplayMechanicsUMFOSS.AI` | Pathfinding, patrol, decision-making systems |
| `GameplayMechanicsUMFOSS.Utils` | Timers, object pooling, extension methods |

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
- **Always** use PascalCase for class names
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

Every mechanic must include a `ScriptExplainer.txt`. This is **mandatory and non-negotiable**.

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

### 11.3 Where to put the video

- **Under 100MB** — Place at `Samples~/YourMechanicName/Video/Demo.mp4`
- **Over 100MB** — Upload to YouTube (unlisted is fine) or Google Drive and paste the link in your PR description and in your `ScriptExplainer.txt` header

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

If any manual setup is genuinely required that cannot be avoided, include a `README.txt` inside your mechanic folder with numbered setup steps. Keep it as short as possible.

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

After your mechanic is merged, update the **Mechanics Library** section in `README.md`.

### Format to follow

```markdown
### N. Your Mechanic Name
**Author:** [Your Name](https://github.com/your-github-handle)

One or two sentences describing what the mechanic does and why it is useful.
What type of game would use this? What problem does it solve?

**Highlights:**
- Key architectural feature (e.g. "Interface-driven - supports both 2D and 3D physics")
- Key gameplay feature (e.g. "Coyote time and jump buffering built in")
- Key learning value (e.g. "Demonstrates the Strategy pattern applied to game mechanics")
```

### Example entry

```markdown
### 3. Modular Inventory System
**Author:** [Vijit](https://github.com/vijit101)

A ScriptableObject-driven inventory system supporting item stacking, equipment slots,
and runtime save/load. Designed to be integrated into any 2D RPG or action game
with minimal coupling to existing player code.

**Highlights:**
- Fully data-driven via ScriptableObject item definitions
- Supports item stacking, unique items, and equipment slots
- Uses EventBus for UI sync - no direct references between inventory and HUD
- Demonstrates Command pattern for undo-able inventory actions
```

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
  [ ] All required prefabs are included in Samples~/YourMechanicName/Assets/Prefabs/
  [ ] ScriptExplainer.txt is present and complete
  [ ] Demo.mp4 is present (or YouTube/Drive link is in PR description)

Documentation
  [ ] README.md Mechanics Library section is updated with your entry
  [ ] ScriptExplainer.txt covers every meaningful code block
  [ ] DemoScene includes UI labels explaining the controls

Git hygiene
  [ ] Branch is named mechanic/your-mechanic-name
  [ ] Commits are clean and descriptive (not "fix", "wip", "asdfgh")
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
<!-- One paragraph describing the mechanic and the problem it solves. -->

## How to test it
<!-- Step by step - what to do after opening the DemoScene to see the mechanic work. -->

## Demo Video
<!-- Link here if not included as Demo.mp4 -->

## Namespace used
<!-- e.g. GameplayMechanicsUMFOSS.Movement -->

## Checklist
- [ ] Compiles with zero errors/warnings
- [ ] Folder structure followed
- [ ] ScriptExplainer.txt included
- [ ] DemoScene runs immediately on Play
- [ ] Demo video included or linked
- [ ] README.md updated
```

### 15.4 What happens after you submit

- A maintainer will review your PR within 7 days
- You may receive review comments requesting changes — respond and push updates
- Once approved, your mechanic will be merged and added to the Mechanics Library
- Your name will appear permanently in the README contributors list

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
| **Scene** | Does it run immediately? Are all references wired? |
| **ScriptExplainer** | Is it complete? Does it teach, not just describe? |
| **Video** | Does it cover all required sections? Is it clear? |
| **README** | Is the entry formatted correctly and informative? |

### How to respond to review feedback

- Do **not** argue in comments — if you disagree with feedback, explain your reasoning calmly and the reviewer will discuss it
- Push all requested changes as new commits on the same branch (do not close and reopen the PR)
- After addressing feedback, leave a comment: *"All feedback addressed — ready for re-review"*

---

## 17. Common Mistakes to Avoid

Learn from what gets PRs rejected:

```
Missing references in DemoScene (NullReferenceException on Play)
Mechanic depends on another mechanic not included in the PR
ScriptExplainer.txt only describes what each line does, not why
Video does not show the mechanic actually running
Wrong or missing namespace
Magic numbers scattered throughout the code
.meta files committed without the corresponding asset
.DS_Store or OS files committed
Branch named "main", "fix", "update" or anything non-descriptive
PR title not following [Mechanic] Add X format
README not updated
Prefab references point to assets outside the mechanic's folder
Demo scene requires the contributor's machine-specific paths
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

*Thank you for contributing to UnityMechanicsFramework.*  
*Every mechanic you add is one less system that someone else has to write from scratch.*  
*Build something great.* 🎮

</div>
