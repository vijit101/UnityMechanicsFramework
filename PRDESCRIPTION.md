[Mechanic] Modular Dash System
Closes #4

### Contributors
- Nayanshi Singh (@nayanshisingh)
- Mahak Juriani (@mahak-juriani)


### Mechanic Name
Modular Dash System — `GameplayMechanicsUMFOSS.Movement`

### What does it do?
A highly modular, dimension-agnostic dash system that solves common implementation problems and offers robust design configuration:
- **Dimension Agnostic** — Uses an `IPhysicsAdapter` pattern so the exact same logic works perfectly in both 2D (`Physics2DAdapter`) and 3D (`Physics3DAdapter`).
- **Charge & Cooldown Management** — Handles max charges, unlimited dashes, and cooldown periods automatically. Charges can reset instantly upon touching the ground.
- **Iframe Integration** — Optional invincibility frames during the dash. Instead of coupling to a hurtbox script, it uses the `EventBus` to notify combat systems when iframes start and end.
- **Decoupled Architecture** — Completely decoupled from input managers, VFX, and audio. Uses the `EventBus` to fire events like `DashStartEvent`, `DashEndEvent`, `DashIframeStartEvent`, and `DashCountChangedEvent`.
- **Directional Modes** — Supports dashing in the `LastMoveDirection` or the character's `FacingDirection`.

### How to test it
1. Extract or clone the project and open it in Unity 2021.3 LTS or later.
2. Navigate to `Samples~/DashSystemSample/Assets/Scenes/`.
3. Open **DemoScene2D.unity** or **DemoScene3D.unity**.
4. Press **Play**.
5. Controls: **WASD / Arrow Keys** to move, **Space** to Dash.
6. What to verify on the live demo:
   - Dashing consumes a charge (represented in the UI).
   - Once all charges are spent, the cooldown timer begins.
   - Touching the ground immediately restores dash charges.
   - The dash velocity and trajectory are perfectly horizontal when "Ignore Gravity" is checked in the Inspector.
   - The character visually flips or rotates to face the dash direction.

### Demo Video
▶ Walkthrough videos are bundled inside the repo at `Samples~/DashSystemSample/Video/`:
- **DashSystemTutorial.mp4** — Demonstrates 2D & 3D gameplay, Inspector configuration, and a full code walkthrough.

### Namespace used
`GameplayMechanicsUMFOSS.Movement`
`GameplayMechanicsUMFOSS.Physics`

### Folder structure
```text
Runtime/Mechanic/DashSystem/
├── Scripts/
│   ├── DashSystem_UMFOSS.cs             ← main logic controller
│   ├── DashEvents_UMFOSS.cs             ← EventBus structs (DashStart, DashEnd, etc.)
│   ├── DashCanvasSetup_UMFOSS.cs        ← Runtime UI setup for the demo
│   ├── DashDemoMovement2D_UMFOSS.cs     ← Minimal 2D movement for demo
│   └── DashDemoMovement3D_UMFOSS.cs     ← Minimal 3D movement for demo
└── Script_Explainers/                   
    └── DashSystem_UMFOSS_ScriptExplainer.txt

Runtime/Physics/
├── IPhysicsAdapter_UMFOSS.cs            ← Abstraction layer
├── Physics2DAdapter_UMFOSS.cs           
├── Physics3DAdapter_UMFOSS.cs           
└── DimensionMode_UMFOSS.cs              

Samples~/DashSystemSample/
├── Assets/Scenes/
│   ├── DemoScene2D.unity
│   └── DemoScene3D.unity
└── Video/
    └── DashSystemTutorial.mp4
```

### README entry
A new mechanic card has been added at `README.md` Section 6 (Mechanics Library) as entry #1 — Modular Dash System, credited to Nayanshi Singh and Mahak Juriani, plus a new row in the Quick Navigation table linking to the video file in the repo.

### Acceptance criteria coverage
| Criterion | Met? | Where |
|---|---|---|
| Works in both 2D and 3D without code duplication | ✅ | `IPhysicsAdapter_UMFOSS.cs` |
| Supports N-charges and cooldowns | ✅ | `DashSystem_UMFOSS.TryStartDash()` |
| Ground resets restore charges | ✅ | `DashSystem_UMFOSS.CheckGround()` |
| EventBus is used to broadcast dash states (VFX/Audio decoupling) | ✅ | `DashEvents_UMFOSS.cs` & `Publish()` |
| Iframe windows are broadcasted without touching hurtbox logic | ✅ | `DashIframeStartEvent` |
| Inspector is fully documented with tooltips | ✅ | `DashSystem_UMFOSS` SerializedFields |
| Script explainer included | ✅ | `Script_Explainers/DashSystem_UMFOSS_ScriptExplainer.txt` |
| Demo scenes work immediately on Play | ✅ | `DemoScene2D.unity` and `DemoScene3D.unity` |
| Demo video included | ✅ | `Samples~/DashSystemSample/Video/` |
| README Quick Navigation row added | ✅ | `README.md` |
| README full mechanic card added | ✅ | `README.md` |

### Checklist
- [x] Compiles with zero errors and zero warnings
- [x] Folder structure followed (`Runtime/Mechanic/DashSystem/` for code, `Samples~/DashSystemSample/` for demo)
- [x] Namespace `GameplayMechanicsUMFOSS.Movement` and `GameplayMechanicsUMFOSS.Physics`
- [x] No magic numbers, no direct cross-mechanic dependencies
- [x] Public APIs have XML `<summary>` documentation
- [x] One Script Explainer per script in `Runtime/Mechanic/DashSystem/Script_Explainers/`
- [x] Demo scene runs immediately on Play with no missing references
- [x] Walkthrough video bundled inside repo
- [x] README Quick Navigation row added with working anchor and video link
- [x] README full mechanic card added (metadata table, what it does, code example, highlights) credited to Nayanshi Singh and Mahak Juriani
