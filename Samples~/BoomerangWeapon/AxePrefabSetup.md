# Manual Axe Prefab Setup

This guide shows how to build a Boomerang weapon prefab **by hand**, without
the `UMF > Build Boomerang Demo Scene` editor tool. Use this when you want to:

- Add a Boomerang weapon to your own game scene
- Understand exactly what the editor tool does under the hood
- Create a custom weapon variant (Hammer, Shield, etc.) using the same pattern

The end result is identical to what the build tool produces.

---

## What you will build

```
Player (existing rig in your game)
└── Camera
    └── HandSocket            (empty Transform — anchor for the weapon)
        └── Axe               (the weapon GameObject)
            ├── VisualPivot   (child Transform — holds the spinning mesh)
            │   └── Blade     (mesh renderer + collider)
            └── (Rigidbody, BoomerangWeapon_UMFOSS, Physics3DAdapter)
```

---

## Step 1 — Create the layers

Open `Edit ▸ Project Settings ▸ Tags and Layers` and add these two
custom layers (any free index works, the names are what matters):

| Layer name | Used for |
|------------|----------|
| `Hurtbox`  | Anything the weapon can damage (target dummies, enemies) |
| `Stuck`    | Surfaces the weapon should embed into (walls, platforms) |

Then in `Edit ▸ Project Settings ▸ Physics` make sure the weapon's
own layer (e.g. `Default`) is set to **collide with both `Hurtbox`
and `Stuck`** in the collision matrix.

---

## Step 2 — Create the HandSocket

1. Inside your player rig, navigate to `Camera` (or whatever bone
   you want the weapon attached to).
2. `GameObject ▸ Create Empty` and rename it `HandSocket`.
3. Set its local position to a sensible offset such as
   `(0.4, -0.3, 0.8)` — slightly right, down, and in front of the
   camera. Tweak until the weapon looks correctly held in Play.

The weapon will be reparented to `HandSocket` whenever it is in
the `Equipped` state.

---

## Step 3 — Create the Axe GameObject

1. `GameObject ▸ Create Empty` at the scene root, name it `Axe`.
2. Set its layer to `Default` (or any layer that collides with
   `Hurtbox` and `Stuck`).
3. Add components in this exact order:
   - `Rigidbody` — set **Use Gravity = false**, leave Kinematic
     unchecked. The weapon's state machine flips Kinematic
     itself.
   - `Box Collider` (or `Capsule Collider`) — sized to your mesh.
     This is what triggers the embed collision. Leave **Is Trigger
     = false**.
4. Reset the transform so its world pose is at the origin while
   you build it. You will reparent to `HandSocket` at the end.

---

## Step 4 — Create the VisualPivot

The visual pivot is a child Transform that holds **only the
mesh**. The spin animation is applied to this pivot, not to the
Axe root, so the root's `forward` vector stays clean.

1. With `Axe` selected, `GameObject ▸ Create Empty Child`, name
   it `VisualPivot`. Reset its local transform.
2. Add your blade mesh as a child of `VisualPivot`:
   - For a placeholder, use `GameObject ▸ 3D Object ▸ Cube`,
     scale to something like `(0.3, 0.05, 0.6)`, and parent
     under `VisualPivot`.
   - For a real model, drag your imported mesh prefab under
     `VisualPivot` and orient it so its **length runs along
     local +Z** — that is the throw direction.
3. **Remove any colliders from the visual mesh.** The Axe root
   already has the collider; nested colliders here will produce
   double hits.

---

## Step 5 — Create the BoomerangConfig asset

The runtime weapon reads all tunable values from a
`BoomerangConfig` ScriptableObject so you can swap weapons by
swapping the asset.

1. In your Project window, right-click the folder where you keep
   ScriptableObject assets (e.g. `Assets/Configs/`).
2. `Create ▸ UMF ▸ Combat ▸ Boomerang Config`. Name it
   `AxeConfig`.
3. Fill in the values. Here are the defaults the build tool uses
   for the demo Axe — they are a good starting point:

| Field | Value |
|---|---|
| Throw Force | `30` |
| Max Range | `25` |
| Return Speed | `15` |
| Return Acceleration | `2` |
| Catch Distance | `0.5` |
| Arc Height Ratio | `0.3` |
| Arc Side Ratio | `0.15` |
| Throw Damage | `25` |
| Return Damage | `15` |
| Knockback Force | `8` |
| Hurtbox Layer | `Hurtbox` (the layer you created) |
| Stuck Layer | `Stuck` |
| Embed In Targets | ☑ |
| Rotation Per Second | `(0, 0, 1080)` — full Z-axis spin |

For a Spear variant, copy the asset and change to:
`Throw Force = 40`, `Max Range = 35`, `Throw Damage = 35`,
`Rotation Per Second = (0, 0, 0)` (no spin).

---

## Step 6 — Add the BoomerangWeapon_UMFOSS component

1. Select the `Axe` GameObject.
2. `Add Component ▸ BoomerangWeapon_UMFOSS`.
3. Drag the asset references into the Inspector:
   - **Config** → your `AxeConfig` asset
   - **Hand Socket** → the `HandSocket` Transform from Step 2
   - **Visual Pivot** → the `VisualPivot` Transform from Step 4
   - **Weapon Collider** → the Axe's own `Box Collider` (the one
     on the root, not the visual pivot)
4. Save.

The script will auto-add a `Physics3DAdapter` on Awake if you
do not add one yourself, but you can add it manually:
`Add Component ▸ Physics3DAdapter`.

---

## Step 7 — Wire input

For input you have two choices:

**A) Use the demo controller as-is.** Add the
`BoomerangDemoController` component to your Player GameObject and
drag the `Axe` into its `Weapons` array. The Inspector exposes
`Throw Mouse Button`, `Recall Key`, and `Switch Weapon Key` —
change them to whatever you want.

**B) Call the API yourself.** From any of your scripts:

```csharp
[SerializeField] private BoomerangWeapon_UMFOSS weapon;
[SerializeField] private Transform aimOrigin; // usually the camera

void Update()
{
    if (Input.GetKeyDown(KeyCode.Mouse0)) weapon.Throw(aimOrigin.forward);
    if (Input.GetKeyDown(KeyCode.R))      weapon.Recall();
}
```

That is the entire public surface area of the weapon. The state
machine handles everything else.

---

## Step 8 — Reparent to HandSocket

With everything wired:

1. Drag the `Axe` GameObject under `HandSocket` in the Hierarchy.
2. Reset its local Position and Rotation to `(0, 0, 0)`.
3. Tweak local Position / Rotation until the weapon sits in the
   hand the way you want it. The state machine restores this
   exact pose every time the weapon is caught.

---

## Step 9 — Add at least one target

For the embedding behaviour to be testable, you need at least one
collider on the `Stuck` layer.

1. `GameObject ▸ 3D Object ▸ Cube`, name it `Wall`.
2. Set its layer to `Stuck`.
3. Move it 10 units in front of the player.

For a damageable target with a health bar:

1. Use the `TargetDummy` script from the sample (`Add Component ▸
   Target Dummy`) and configure its `Damageable` health.
2. Set the layer to `Hurtbox`.

---

## Step 10 — Press Play

- Click in the Game view to lock the cursor.
- Throw with your configured key.
- Aim at a `Stuck` surface to see embedding.
- Press your configured Recall key to bring it back.

If something does not work, the most common issues are:

| Symptom | Most likely cause |
|---|---|
| Weapon falls through the world | Use Gravity left enabled on Rigidbody |
| Weapon never embeds | Wall not on `Stuck` layer, or Hurtbox layer misassigned in BoomerangConfig |
| Weapon spins the entire root | Spin applied to root because `VisualPivot` field is empty |
| Catch snaps to wrong pose | Local pose under HandSocket was not zero when entering Play — re-place |
| Throw does nothing | Cursor not locked (controller gates throw on `Cursor.lockState == Locked`) |

---

## What this matches in the demo

This procedure produces an `Axe` that is functionally identical
to what `UMF ▸ Build Boomerang Demo Scene` constructs in code.
The build tool exists so reviewers can see the mechanic in 30
seconds; this manual procedure exists so users can integrate
the weapon into their own projects.
