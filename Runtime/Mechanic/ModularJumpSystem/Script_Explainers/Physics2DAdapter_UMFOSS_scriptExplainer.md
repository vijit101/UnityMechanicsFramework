# Physics2DAdapter_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine — needed for MonoBehaviour, Rigidbody2D, Vector2, Vector3, Physics2D, RaycastHit2D, ForceMode2D, LayerMask, etc.

```csharp
namespace GameplayMechanicsUMFOSS.Physics
```
**Explanation:** Places this class under the Physics namespace alongside the interface and the 3D adapter.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
```
**Explanation:** Unity attribute that ensures a Rigidbody2D component exists on the same GameObject. If one doesn't exist, Unity auto-adds it when this component is attached. Prevents runtime NullReferenceException errors.

```csharp
[AddComponentMenu("Gameplay Mechanics UMFOSS/Physics/Physics 2D Adapter")]
```
**Explanation:** Adds a clean entry in Unity's "Add Component" menu under a structured path, making it easy to find manually.

```csharp
public class Physics2DAdapter_UMFOSS : MonoBehaviour, IPhysicsAdapter
```
**Explanation:** Class declaration. Inherits from MonoBehaviour (can be attached to GameObjects) and implements IPhysicsAdapter (fulfills the physics adapter contract). This is the 2D implementation of the adapter pattern.

```csharp
private Rigidbody2D rb;
```
**Explanation:** Private reference to the Rigidbody2D component on this GameObject. Cached in Awake() for performance — calling GetComponent every frame is expensive.

---

## Velocity Property

```csharp
public Vector3 Velocity
{
    get => (Vector3)rb.linearVelocity;
    set => rb.linearVelocity = (Vector2)value;
}
```
**Explanation:** Implements the IPhysicsAdapter Velocity property. The getter casts Rigidbody2D's Vector2 velocity to Vector3 (z becomes 0). The setter casts Vector3 back to Vector2 (z component is dropped). This bridge lets the jump system work with Vector3 while the physics engine uses Vector2 internally. Uses `linearVelocity` (the newer Unity API name for `velocity`).

---

## GravityScale Property

```csharp
public float GravityScale
{
    get => rb.gravityScale;
    set => rb.gravityScale = value;
}
```
**Explanation:** Direct passthrough to Rigidbody2D's built-in gravityScale property. Unity's 2D physics engine automatically applies gravity each physics step, scaled by this value. When the jump system sets GravityScale = 2.5, Rigidbody2D makes gravity 2.5x stronger — no manual FixedUpdate gravity code needed (unlike the 3D adapter).

---

## Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
}
```
**Explanation:** Called once when the script initializes. Caches the Rigidbody2D reference for use in all other methods. GetComponent is guaranteed to succeed because of the [RequireComponent] attribute.

---

## AddForce Method

```csharp
public void AddForce(Vector3 force, bool impulse = false)
{
    ForceMode2D mode = impulse ? ForceMode2D.Impulse : ForceMode2D.Force;
    rb.AddForce((Vector2)force, mode);
}
```
**Explanation:** Applies a physics force to the Rigidbody2D. Converts the `impulse` boolean to the correct ForceMode2D enum: `Impulse` for instant velocity change, `Force` for continuous push. Casts Vector3 to Vector2 (drops z) before applying. This is used by the jump system for force-based interactions.

---

## CheckGrounded Method

```csharp
public bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer)
{
    RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayer);
    return hit.collider != null;
}
```
**Explanation:** Shoots a 2D raycast straight down from the given origin point. If the ray hits any collider on the specified ground layer within the given distance, returns true (grounded). Returns false if nothing is hit (airborne). This is how the jump system knows whether the character is on the ground.
