# IPhysicsAdapter_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine — needed for Vector3 and LayerMask types used in the interface.

```csharp
namespace GameplayMechanicsUMFOSS.Physics
```
**Explanation:** Places this interface under the Physics namespace. All physics adapters (2D and 3D) live here.

```csharp
public interface IPhysicsAdapter
```
**Explanation:** Declares a public interface — a contract that both Physics2DAdapter and Physics3DAdapter must implement. The jump system talks ONLY to this interface, never to Rigidbody or Rigidbody2D directly. This is the Adapter Pattern — it decouples mechanics from specific physics implementations.

```csharp
Vector3 Velocity { get; set; }
```
**Explanation:** Property to get and set the physics body's velocity. Uses Vector3 as the common type for both 2D and 3D. The 2D adapter internally converts between Vector2 and Vector3 (z component is ignored). This allows the jump system to set `velocity.y = jumpForce` without knowing if it's a Rigidbody2D or Rigidbody.

```csharp
void AddForce(Vector3 force, bool impulse = false);
```
**Explanation:** Method to apply a force to the physics body. When `impulse = true`, the force is applied as an instant velocity change (like a sudden hit). When `impulse = false`, it's applied as a continuous force (like wind pushing). The 2D adapter maps this to ForceMode2D.Impulse/Force, the 3D adapter maps to ForceMode.Impulse/Force.

```csharp
bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer);
```
**Explanation:** Method to check if the object is touching the ground. Takes a world-space origin point, a distance to check below it, and a layer mask for what counts as ground. The 2D adapter uses Physics2D.Raycast (a 2D line check), the 3D adapter uses Physics.SphereCast (a 3D sphere sweep, more reliable for 3D characters). Returns true if ground is detected.

```csharp
float GravityScale { get; set; }
```
**Explanation:** Property to control the gravity strength multiplier. For 2D, this maps directly to Rigidbody2D.gravityScale (Unity handles the gravity automatically). For 3D, since Rigidbody has no built-in gravityScale, the value is stored internally and applied manually each FixedUpdate via `rb.AddForce(Physics.gravity * gravityScale)`. This abstraction lets the jump system control gravity identically in both modes.
