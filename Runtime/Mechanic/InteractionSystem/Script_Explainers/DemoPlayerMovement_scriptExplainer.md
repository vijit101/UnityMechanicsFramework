# DemoPlayerMovement — Line-by-Line Script Explainer

```
SCRIPT     : DemoPlayerMovement.cs
AUTHOR     : Pranav Aggarwal, Shivam Tiwari
NAMESPACE  : GameplayMechanicsUMFOSS.Samples.Interaction
LOCATION   : Samples~/InteractionSystemSample/Assets/Scripts/
```

---

## Purpose

A bare-bones top-down WASD movement script for the demo scene only. Lets the player walk around so they can approach interactables. Not part of the runtime package — lives in `Samples~/`.

---

## Imports & Namespace

```csharp
using UnityEngine;
```
**Explanation:** Required for `MonoBehaviour`, `Rigidbody2D`, `Vector2`, `Input`.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Interaction
```
**Explanation:** Sample-only namespace — convention is `Samples.<MechanicName>` so demo scripts are clearly distinct from the runtime API.

---

## Class Declaration

```csharp
[RequireComponent(typeof(Rigidbody2D))]
public class DemoPlayerMovement : MonoBehaviour
```
**Explanation:** `[RequireComponent]` makes Unity auto-add a `Rigidbody2D` if one isn't already present, and prevents the user from removing it later — the script depends on it for velocity-based movement.

---

## Serialized Fields

```csharp
[Header("Movement")]
[SerializeField] private float moveSpeed = 5f;
```
**Explanation:** Walking speed in world units per second. 5 is a comfortable top-down feel — fast enough to feel responsive, slow enough to give the player time to read prompts.

---

## Private Fields

```csharp
private Rigidbody2D rb;
```
**Explanation:** Cached reference to the `Rigidbody2D`. Caching in `Awake` is cheaper than `GetComponent` every `FixedUpdate`.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
}
```
**Explanation:** One-time component lookup. Guaranteed to find the Rigidbody2D because of `[RequireComponent]`.

---

## Unity Lifecycle — FixedUpdate

```csharp
private void FixedUpdate()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical   = Input.GetAxisRaw("Vertical");

    Vector2 direction = new Vector2(horizontal, vertical).normalized;
    rb.velocity = direction * moveSpeed;
}
```
**Explanation:** Physics-rate movement. `FixedUpdate` (default 50 Hz) is the right place to write to `Rigidbody2D.velocity` so it stays in sync with the physics step. `GetAxisRaw` returns -1, 0, or 1 with no smoothing — feels snappy. `.normalized` ensures diagonal movement isn't faster than cardinal directions (a bug if you just used `(h, v) * speed`). The `Rigidbody2D.gravityScale` should be 0 in the inspector for true top-down behaviour.
