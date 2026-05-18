================================================================
  StateMachine Sample — Folder Layout
================================================================

Per the PR review feedback, the sample is delivered as zips inside
the repo. No loose Assets folder is checked in.

Files in this folder
--------------------
StateMachineSample.zip
    Contains the demo Unity project files:
        Assets/Scenes/DemoScene.unity
        Assets/Scripts/PlayerFSM_UMFOSS.cs
        Assets/Scripts/EnemyFSM_UMFOSS.cs
        Assets/Scripts/GroundDetector.cs
        Assets/Sprites/WhiteSquare.png
    Drop the Assets folder into a Unity 2021.3 LTS or newer project,
    or extract directly under your existing project's root.

Video/StateMachineTutorial.mp4
    The original demo walkthrough — shows the player and enemy FSMs
    running and the live debugger in the Inspector.

Video/StateMachineTutorial.zip
    Zipped copy of the demo video, per the PR review requirement that
    videos be delivered zipped inside the repo.

Per-script explainers
---------------------
Every runtime script now ships with its own ScriptExplainer in
    Runtime/Core/StateMachine/Script_Explainers/

Open them alongside the matching .cs file in
    Runtime/Core/StateMachine/Scripts/


How to set up the demo from scratch
-----------------------------------
1. Open a Unity 2021.3 LTS or newer project.
2. Add this package via Package Manager (Add from disk → package.json
   in the repo root) OR copy the Runtime/ folder into Assets/.
3. Extract StateMachineSample.zip and copy its Assets/ contents into
   your project's Assets/ folder.
4. Open Assets/Scenes/DemoScene.unity.
5. The Player and Enemy GameObjects already have their components
   wired:
       Player: Rigidbody2D, BoxCollider2D, GroundDetector,
               PlayerFSM_UMFOSS, StateMachineDebugger_UMFOSS
       Enemy:  Rigidbody2D, BoxCollider2D, EnemyFSM_UMFOSS,
               StateMachineDebugger_UMFOSS (with two waypoint
               Transforms assigned)
6. Press Play.
   - A / D to move, Space to jump, Left Shift to dash.
   - Both StateMachineDebugger_UMFOSS components show live state,
     duration, and recent transitions in the Inspector.
================================================================
