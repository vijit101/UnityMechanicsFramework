# Quest System sample

1. Open this folder as a Unity project (`Samples~/QuestSystem`).
2. The package resolves via `Packages/manifest.json` → `file:../../..` (repository root containing `package.json`).
3. Open `Assets/Scenes/DemoScene.unity` and press **Play**.

The scene contains a single `QuestDemo` object with `QuestSampleSceneBootstrap`, which builds:

- EventSystem and Canvas (quest tracker, `GameEventPayload` log, reward feed, controls, **death overlay**).
- Quest runtime: `QuestManager_UMFOSS`, `QuestSystem_UMFOSS`, three demo quests (`QuestSampleRuntimeSetup`), HUD, save coordinator, world registry.
- A **bounded** **3D playground**: perimeter walls around the ground plane, WASD movement (camera-relative), **F** to attack the nearest goblin, **E** at the **Quest Board** (blue pillar near spawn) to **start Clear The Camp**, **E** at the **Merchant** to **start and progress** the merchant quest (staged: talk → collect ore → return), walk into loot spheres and exploration volumes.
- **No quest auto-starts** on load. The HUD **Available quests** list shows how to begin: **Clear The Camp** (Quest Board) and **The Merchant's Request** (Merchant), with bracket-style hints.
- **Death and respawn** (`QuestSamplePlayerLifecycle`): **P** triggers death, publishes the quest `PlayerDeathEvent` payload (for `failOnDeath` quests such as **Clear The Camp**), shows **You died**, locks movement/combat/interact/pickups/zones until you **Respawn** or **5 seconds** auto-respawn at the spawn point. When **Clear The Camp** fails, the **goblin camp encounter resets** (goblins + camp loot/trinket pickups) so retries are fair; a **retry hint** appears after respawn. Use the Quest Board (**E**) again to restart that quest.
- **S** / **L** save and load **quest state + player position + consumed world entities** (save is disabled while dead).

Save data is stored in `PlayerPrefs` under key `UMFOSS_QuestSampleFullSave` (replaces the older quest-only key if you had one).

See `ScriptExplainer.txt` in this folder for architecture notes.

## Demo quests (running together)

| Quest | Objectives (summary) |
|-------|----------------------|
| **Clear The Camp** (Main) | Kill 3 goblins, collect GoblinLoot, optional BonusTrinket |
| **The Merchant's Request** (Side) | Talk to Merchant (`InteractEvent` + `merchantPhase=Talk`), collect 2 Iron Ore, return (`merchantPhase=Return` after ore is done) |
| **Explorer** (Hidden) | Reach East Gate, reach North Tower — registered but **not** auto-started in this sample |

The **Explorer** quest has `autoStart` off so nothing is active on load except what you start at the Quest Board or Merchant. To try Explorer, call `StartQuest` from a test hook or set `autoStart` in an extended demo.

Rewards are announced via `QuestRewardGrantedEvent_UMFOSS` and shown in the bottom-right HUD panel.

## Video

Record a walkthrough to `Video/` and link it from the root `README.md` Mechanics Library entry per `CONTRIBUTING.md`.
