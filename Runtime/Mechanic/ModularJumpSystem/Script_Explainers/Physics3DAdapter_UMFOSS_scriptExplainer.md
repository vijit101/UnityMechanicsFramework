# Physics3DAdapter_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine — needed for MonoBehaviour, Rigidbody, Vector3, Physics, ForceMode, LayerMask, etc.

```csharp
namespace GameplayMechanicsUMFOSS.Physics
```
**Explanation:** Places this class under the Physics namespace alongside the interface and the 2D adapter.

```csharp
[RequireComponent(typeof(Rigidbody))]
```
**Explanation:** Ensures a Rigidbody (3D) component exists on the same GameObject. Unity auto-adds it if missing when this component is attached.

```csharp
[AddComponentMenu("Gameplay Mechanics UMFOSS/Physics/Physics 3D Adapter")]
```
**Explanation:** Adds a clean entry in Unity's "Add Component" menu for easy manual attachment.

```csharp
public class Physics3DAdapter_UMFOSS : MonoBehaviour, IPhysicsAdapter
```
**Explanation:** Class declaration. Inherits from MonoBehaviour and implements IPhysicsAdapter. This is the 3D implementation of the adapter pattern — the counterpart to Physics2DAdapter_UMFOSS.

---

## Serialized Fields

```csharp
[SerializeField] private float sphereCastRadius = 0.3f;
```
**Explanation:** The radius of the sphere used for ground detection. Unlike the 2D adapter which uses a thin raycast, the 3D adapter uses a SphereCast — a sphere swept downward. The radius should roughly match the character's base width. Configurable in the Inspector because different character sizes need different radii.

---

## Private Fields

```csharp
private Rigidbody rb;
```
**Explanation:** Cached reference to the Rigidbody component. Set in Awake() to avoid repeated GetComponent calls.

```csharp
private float gravityScale = 1f;
```
**Explanation:** Stores the gravity scale value internally. Unlike Rigidbody2D, Unity's Rigidbody has NO built-in gravityScale property. This field is used in FixedUpdate to apply gravity manually at the desired strength.

---

## Velocity Property

```csharp
public Vector3 Velocity
{
    get => rb.linearVelocity;
    set => rb.linearVelocity = value;
}
```
**Explanation:** Direct passthrough to Rigidbody's linearVelocity. No type conversion needed because Rigidbody already uses Vector3 (unlike the 2D adapter which needs Vector2/Vector3 casting). Uses `linearVelocity` (the newer Unity API name).

---

## GravityScale Property

```csharp
public float GravityScale
{
    get => gravityScale;
    set => gravityScale = value;
}
```
**Explanation:** Gets/sets the internal gravity scale field. The actual gravity application happens in FixedUpdate below. When the jump system sets GravityScale = 2.5, the next FixedUpdate applies 2.5x gravity. When set to 0, no gravity is applied (e.g., disabled while rising for a floaty feel).

---

## Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody>();
    rb.useGravity = false;
}
```
**Explanation:** Caches the Rigidbody reference and DISABLES Unity's built-in gravity. This is critical — since Rigidbody has no gravityScale, we disable automatic gravity and apply it manually in FixedUpdate. This gives us full control over gravity strength at any time.

---

## FixedUpdate

```csharp
private void FixedUpdate()
{
    rb.AddForce(UnityEngine.Physics.gravity * gravityScale, ForceMode.Acceleration);
}
```
**Explanation:** Applies gravity manually every physics step. `Physics.gravity` is Unity's global gravity vector (default: 0, -9.81, 0). Multiplying by `gravityScale` controls the strength. `ForceMode.Acceleration` ignores the object's mass — just like real gravity, which accelerates all objects equally regardless of mass. When gravityScale = 0, the force is zero (no gravity). When gravityScale = 2.5, gravity is 2.5x stronger than normal.

---

## AddForce Method

```csharp
public void AddForce(Vector3 force, bool impulse = false)
{
    ForceMode mode = impulse ? ForceMode.Impulse : ForceMode.Force;
    rb.AddForce(force, mode);
}
```
**Explanation:** Applies a physics force to the Rigidbody. Maps the boolean `impulse` parameter to ForceMode.Impulse (instant velocity change) or ForceMode.Force (continuous push). No type conversion needed — Rigidbody natively uses Vector3.

---

## CheckGrounded Method

```csharp
public bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer)
{
    return UnityEngine.Physics.SphereCast(
        origin, sphereCastRadius, Vector3.down, out _, distance, groundLayer
    );
}
```
**Explanation:** Uses a SphereCast instead of a Raycast for 3D ground detection. A SphereCast sweeps a sphere downward from the origin point — this is more reliable for 3D characters because it accounts for the character's width (a thin raycast might miss sloped or uneven ground at the edges). `out _` discards the hit information since we only need a boolean: did we hit ground or not? Returns true if ground is within range.
