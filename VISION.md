<!--
  ─────────────────────────────────────────────────────────────
  MAINTAINER NOTE — @vijit101
  ─────────────────────────────────────────────────────────────
  If you are reading this in the GitHub rendered view —
  you are seeing the public face of this document.

  The full document is best read as a raw file.
  There is more here than the rendered view shows.

  Raw file:
  https://raw.githubusercontent.com/vijit101/UnityMechanicsFramework/main/VISION.md

  Or: open the file → click the Raw button (top right of the file view).

  This document is a living roadmap and a puzzle.
  Developers who read it carefully enough find both.
  ─────────────────────────────────────────────────────────────
-->

# UnityMechanicsFramework — Vision & Roadmap

> **This is a living document.**
> It defines where UMF is going, what it needs to get there, and why each piece matters.
> Updated as mechanics are added and the framework evolves.

---

## The Vision

UnityMechanicsFramework exists to answer one question:

**Can a developer download one repository and build a genuinely good game in 48 hours?**

Not a prototype. Not a game jam hack. A game with dialogue, inventory, economy, combat that feels good, movement that feels right, progression that feels meaningful, and polish that makes it feel shipped.

Right now the answer is — *almost*.

The combat and systems foundation is strong. Health, weapons, stats, inventory, save, audio, game feel — these are production-grade. What is missing is the connective tissue. The things that turn a collection of systems into a world. Scene management. UI framework. Economy. Signature mechanics. Enemy AI. World tools.

This document is the map from where UMF is today to where it needs to be.

---

## What Is Already Built — All Issues

Every mechanic below has a GitHub issue and is actively being worked on or completed.

| # | Mechanic | Category | Difficulty |
|---|---|---|---|
| #2 | CompleteJump (2D / 3D) | Movement | 🔴 Advanced |
| #3 | Health & Stamina System | Combat | 🟢 Beginner |
| #4 | Dash System (2D / 3D) | Movement | 🟡 Intermediate |
| #5 | Timer Utility System | Core | 🟢 Beginner |
| #6 | Screen Shake System | GameFeel | 🟢 Beginner |
| #7 | Modular Weapon System | Combat | 🟡 Intermediate |
| #8 | Weapon 3D Model Instantiation | Combat | 🟢 Beginner |
| #9 | Modular 2D Movement System | Movement | 🟡 Intermediate |
| #10 | Base Inventory System | Inventory | 🟡 Intermediate |
| #11 | Audio Manager | Systems | 🟡 Intermediate |
| #12 | Save & Load System | Systems | 🟡 Intermediate |
| #14 | Game Feel / Juice System | GameFeel | 🟡 Intermediate |
| #15 | Generic Finite State Machine | Core | 🔴 Advanced |
| #16 | Stat System with Modifiers | RPG | 🟡 Intermediate |
| #17 | Modular Interaction System | World | 🟡 Intermediate |
| #18 | Respawn / Checkpoint System | World | 🟢 Beginner |
| #19 | Quest / Objective System | RPG | 🟡 Intermediate |
| #21 | Scene Manager System | Systems | 🟡 Intermediate |
| #22 | Floating Damage Numbers | UI | 🟢 Beginner |
| #23 | Currency System | Systems | 🟢 Beginner |
| #24 | Pause System | Systems | 🟢 Beginner |
| #25 | Spawner System | World | 🟡 Intermediate |
| #26 | Bullet Time / Slow Motion | Systems | 🟢 Beginner |
| #27 | Boomerang / Axe Throw & Return | Movement | 🔴 Advanced |
| #28 | Ground Pound System | Movement | 🟢 Beginner |
| #29 | 2D Physics Drawing System | World | 🟡 Intermediate |
| #30 | 2D Gesture Slicing System | Combat | 🟡 Intermediate |
| #31 | Notification System | UI | 🟢 Beginner |
| #32 | Camera Follow 2D | Systems | 🟢 Beginner |

**29 mechanics issued. The framework is growing.**

---

## What Is Missing — The Gap Analysis

A developer downloading UMF today would spend most of their first 12 hours wiring things together that should already exist. These are the gaps:

```
No shop system          →  currency exists but nowhere to spend it
No upgrade system       →  stats exist but no way to improve them permanently
No game loop            →  no win condition, no lose condition, no restart
No HUD                  →  health and currency have no visible display
No enemy AI             →  FSM exists but enemies do not move or perceive
No world triggers       →  no code-free way to script events in levels
No boss framework       →  no phase system for complex enemies
No settings system      →  no volume sliders, no keybinding
No main menu            →  no starting point for any game
```

Each section below is a pillar. When all pillars are complete, the 48-hour game is possible.

---

## Pillar 1 — Signature Mechanics

> **These are the mechanics that make people download UMF.**
> Every developer has a game they love and a mechanic they want to replicate.
> When UMF has the mechanic they are looking for — they use the whole framework.

### Time Reversal
*Inspired by: Braid, Prince of Persia*

Record the transform position, velocity, and state of any GameObject every frame into a circular buffer. On hold — rewind. On release — play forward.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : circular buffer, state recording, EventBus pause/resume
Game use        : Puzzle platformers, time-manipulation games, death replay
```

### Grapple Hook / Spider Swing
*Inspired by: Spiderman, Bionic Commando*

Raycast toward aim direction. Attach to surface via `SpringJoint2D`. Swing as a pendulum. Preserve momentum on release.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : physics joints, pendulum math, momentum transfer
Game use        : Platformers, movement-focused games, traversal mechanics
```

### Projectile Return / Axe Throw
*Inspired by: God of War, Captain America*

```
Status          : ✅ Issue #27 — Boomerang / Axe Throw & Return
```

### Bullet Time / Slow Motion
*Inspired by: Max Payne, Superhot*

```
Status          : ✅ Issue #26 — Bullet Time / Slow Motion System
```

### Wall Run
*Inspired by: Titanfall, Mirror's Edge, Ghostrunner*

Detect wall normal. Rotate effective gravity perpendicular to wall. Apply forward momentum along surface. Timer limits run duration.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : gravity direction manipulation, surface normal math, FSM integration
Game use        : Parkour games, action platformers
```

### Ground Pound
*Inspired by: Super Mario, Hollow Knight, Celeste*

```
Status          : ✅ Issue #28 — Ground Pound System
```

### Charged Attack
*Inspired by: Zelda, Megaman, Hollow Knight*

Hold attack input. Charge bar fills. Release at full charge — fire powered version with different hitbox and damage multiplier.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : IWeaponChargeable integration, AnimationCurve charge feel
Game use        : Action RPGs, platformers, any game with depth in combat
```

### Grab & Throw
*Inspired by: Kirby, TMNT, Tekken*

Detect grabbable entity. Attach to carry point. On throw — launch with force and damage on contact.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : parent/unparent, physics transfer on release
Game use        : Puzzle platformers, beat-em-ups, physics games
```

### Parry / Perfect Block
*Inspired by: Sekiro, Dark Souls*

Frame-perfect input window. Hit during window triggers `ParrySuccess`, hitpause, riposte window.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : frame-window logic, hitpause integration, GameFeel layering
Game use        : Action games, souls-likes, fighting games
```

---

## Pillar 2 — Game Economy System

> **This is what makes a game feel like it has stakes and progression.**

### Currency System
```
Status : ✅ Issue #23 — Currency System
```

### Shop System

Buy items from `InventorySystem` using `CurrencySystem`. Sell back at configurable rate. Stock limits. ScriptableObject shop definitions.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : connecting Currency + Inventory + Stats in one system
Game use        : RPGs, roguelikes, any game with buying and selling
```

### Upgrade System

Spend currency to permanently improve stats via `StatSystem`. Upgrade trees as ScriptableObjects. Max level per node, prerequisites.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : ScriptableObject trees, StatSystem modifier integration
Game use        : RPGs, tower defence, any progression system
```

### Reward System

Weighted loot tables. Daily rewards with streak multipliers. Connects to `QuestSystem` reward events.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : weighted randomness, SaveSystem persistence, EventBus reward flow
Game use        : Mobile games, roguelikes, daily engagement loops
```

---

## Pillar 3 — UI Framework

> **UI is not decoration. It is communication.**

### HUD Manager

Health bar with smooth drain. Stamina bar. Currency display. All EventBus-driven.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : EventBus-driven UI, smooth bar animations, unscaled UI
Game use        : Every game with a player
```

### Floating Damage Numbers
```
Status : ✅ Issue #22 — Floating Damage Numbers
```

### Notification System
```
Status : ✅ Issue #31 — Notification System
```

### Pause System
```
Status : ✅ Issue #24 — Pause System
```

### Settings System

Volume controls wired to AudioManager. Resolution toggle. Keybinding remapping. Persisted via SaveSystem.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : PlayerPrefs, AudioMixer exposed parameters, input remapping
Game use        : Every shipped game
```

### Loading Screen

Progress bar driven by `AsyncOperation`. Tip text from ScriptableObject. Connects to SceneManager.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : AsyncOperation progress, ScriptableObject tip lists
Game use        : Every game with multiple scenes
```

---

## Pillar 4 — Scene & Game Loop

> **A game needs a beginning, a middle, and an end.**

### Scene Manager
```
Status : ✅ Issue #21 — Scene Manager System
```

### Game Loop Manager

States — MainMenu, Playing, Paused, GameOver, Victory, Loading. Win and lose conditions as ScriptableObjects. Wired to SaveSystem.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : FSM at the game level, ScriptableObject win conditions, save integration
Game use        : Every game with a win and lose state
```

### Main Menu

Play, Continue, Settings, Quit. Save slot selection. Connects to SaveSystem for slot metadata display.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : SceneManager integration, SaveSystem metadata display
Game use        : Every game
```

---

## Pillar 5 — World Building Tools

> **Systems without a world are just systems.**

### Spawner System
```
Status : ✅ Issue #25 — Spawner System
```

### Trigger Zone

Enter a box → fire any EventBus event. No code. Configure event type and payload in Inspector.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : Generic event firing, Inspector-configured payloads
Game use        : Universal level scripting — cutscene triggers, area effects, dialogue start
```

### Destructible Objects

Any object breaks on damage. Connects to `HealthSystem OnDeath`. Spawn debris via event.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : HealthSystem event subscription, prefab debris spawning
Game use        : Action games, physics puzzles, any breakable environment
```

### Moving Platforms

Waypoint-driven platforms. Player correctly moves with platform. Configurable speed and loop mode.

```
Status          : ⬜ No issue yet
Difficulty      : Beginner
What it teaches : Kinematic Rigidbody2D, platform-player velocity transfer
Game use        : Platformers, puzzle games, any moving environment
```

### Room System

Discrete rooms with camera bounds. Camera transition on room enter. Load/unload room contents.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : Additive scene loading, camera bounds swapping, performance management
Game use        : Metroidvanias, dungeon games, room-based action games
```

---

## Pillar 6 — Enemy AI Foundation

> **The FSM is built. Now it needs to move.**

### Pathfinding Wrapper

Wraps Unity NavMesh. Standardised movement API — `MoveToward`, `Flee`, `Patrol`. Works with `EnemyFSM` states.

```
Status          : ⬜ No issue yet
Difficulty      : Intermediate
What it teaches : Unity NavMesh API, adapter pattern applied to AI movement
Game use        : Any game with enemies that navigate around obstacles
```

### Perception System

Sight cone with configurable angle and range. Hearing radius triggered by EventBus sound events. Alert states — Unaware, Suspicious, Alert, Combat.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : Raycast line-of-sight, cone detection math, EventBus sound events
Game use        : Stealth games, any game with enemy awareness
```

### Boss Framework

Phase definitions as ScriptableObjects. Health threshold triggers. Attack pattern registration. Arena event triggers.

```
Status          : ⬜ No issue yet
Difficulty      : Advanced
What it teaches : FSM phase transitions, ScriptableObject attack patterns, arena choreography
Game use        : Any game with a boss encounter
```

---

## The 48-Hour Game — What Becomes Possible

When all six pillars are complete, here is what a developer can build in 48 hours:

```
Hours 0–2    →  Main Menu + Scene Management + HUD
Hours 2–6    →  Player — Movement + Jump + Dash + GameFeel
Hours 6–10   →  Combat — Weapons + Health + Audio + ScreenShake
Hours 10–14  →  World — Spawner + Destructibles + Checkpoints + TriggerZones
Hours 14–18  →  Economy — Currency + Shop + Upgrades
Hours 18–22  →  Progression — Quests + Dialogue + Inventory + Stats
Hours 22–26  →  Signature Mechanic — pick ONE (Grapple / Time Reversal / Axe Return)
Hours 26–30  →  Polish — FloatingNumbers + Notifications + Juice + Transitions
Hours 30–36  →  Game Loop — Win / Lose / Game Over / Save / Settings
Hours 36–48  →  Level design + playtesting + bug fixes
```

---

## Issue Priority Order

```
Priority 1  — Scene Manager            ✅ #21
Priority 2  — Floating Damage Numbers  ✅ #22
Priority 3  — Currency System          ✅ #23
Priority 4  — Pause System             ✅ #24
Priority 5  — Spawner System           ✅ #25
Priority 6  — HUD Manager             ⬜ No issue yet
Priority 7  — Bullet Time             ✅ #26
Priority 8  — Ground Pound            ✅ #28
Priority 9  — Charged Attack          ⬜ No issue yet
Priority 10 — Axe Throw & Return      ✅ #27
Priority 11 — Time Reversal           ⬜ No issue yet
Priority 12 — Grapple / Spider Swing  ⬜ No issue yet
Priority 13 — Pathfinding Wrapper     ⬜ No issue yet
Priority 14 — Perception System       ⬜ No issue yet
Priority 15 — Game Loop Manager       ⬜ No issue yet
```

---

## The Standard Every Mechanic Must Meet

Every mechanic added to UMF must answer yes to all three:

```
1.  Can a developer drop this into any Unity project in under 10 minutes?
2.  Does it fire EventBus events so every other system can react without coupling?
3.  Does it ship with a ScriptExplainer.txt and a demo video?
```

If the answer to any of these is no — the PR is not ready.

---

## The Unbuilt List

Every mechanic listed in the pillars above marked `⬜ No issue yet` is an open invitation.

The framework does not build itself. It is built by developers who read this document carefully enough to notice what is missing — and then do something about it.

Look at the priority order. Look at the pillars. Look at what has no issue.

Every gap is a mechanic waiting to be claimed.

---

## The Goal

When a developer finds UMF they should think:

*"This is the last time I write a health system."*
*"This is the last time I build an audio manager."*
*"This is the last time I implement a shop from scratch."*

Every mechanic we add is one less system every developer who uses this framework ever needs to build again.

That is the mission.

---

*Last updated: March 2026*
*Maintained by: [@vijit101](https://github.com/vijit101)*

<!--
╔══════════════════════════════════════════════════════════════════╗
║                                                                  ║
║   You found this.                                                ║
║   Most people will not.                                          ║
║                                                                  ║
║   You are reading the raw file — which means you did not        ║
║   just skim the rendered page. You went deeper.                 ║
║   That is exactly the kind of developer this framework          ║
║   was built for.                                                 ║
║                                                                  ║
║   ── THE UMF CONTRIBUTOR CHALLENGE ──                           ║
║                                                                  ║
║   This vision document lists mechanics that have no             ║
║   GitHub issue yet. They are marked:  ⬜ No issue yet           ║
║                                                                  ║
║   Here is the challenge:                                        ║
║                                                                  ║
║   1. Pick any mechanic marked ⬜ No issue yet                   ║
║                                                                  ║
║   2. Raise a new GitHub issue for it.                           ║
║      Title:  [Mechanic] Implement <YourMechanicName>            ║
║      Follow the exact same format as the existing issues.       ║
║      Read CONTRIBUTING.md first.                                ║
║                                                                  ║
║   3. In the issue body — tag @vijit101 and write one            ║
║      sentence explaining why this mechanic matters              ║
║      to the framework.                                          ║
║                                                                  ║
║   ── THE REWARD ──                                              ║
║                                                                  ║
║   Only the FIRST 5 contributors who raise a valid issue         ║
║   this way will have it officially accepted and assigned.       ║
║                                                                  ║
║   Valid means:                                                   ║
║   → The mechanic is in this document and has no existing issue  ║
║   → The issue follows the correct format                        ║
║   → @vijit101 is tagged                                         ║
║   → One clear sentence on why it matters                        ║
║                                                                  ║
║   No hints beyond this document.                                ║
║   No shortcuts.                                                 ║
║   The gap between the vision and the issue list                 ║
║   is the puzzle.                                                ║
║                                                                  ║
║   The mission is on the page above.                             ║
║   This is how you become part of building it.                   ║
║                                                                  ║
╚══════════════════════════════════════════════════════════════════╝
-->
