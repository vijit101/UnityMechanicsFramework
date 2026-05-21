# JumpDemo3DController_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine for MonoBehaviour, Rigidbody, Vector3, Input, KeyCode, RigidbodyConstraints, Debug, etc.

```csharp
using GameplayMechanicsUMFOSS.Movement;
```
**Explanation:** Imports the Movement namespace to access ModularJumpSystem_UMFOSS.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Jump
```
**Explanation:** Places this class under the Samples.Jump namespace — separate from the core library. This is a demo/example script.

```csharp
[RequireComponent(typeof(Rigidbody))]
```
**Explanation:** Ensures a Rigidbody (3D) exists on this GameObject. Required for 3D physics-based movement.

```csharp
[RequireComponent(typeof(ModularJumpSystem_UMFOSS))]
```
**Explanation:** Ensures the ModularJumpSystem is also attached. This demo controller depends on it.

```csharp
public class JumpDemo3DController_UMFOSS : MonoBehaviour
```
**Explanation:** A minimal 3D player controller that demonstrates how to use the ModularJumpSystem in 3D mode. Movement is on the XZ horizontal plane (WASD), jumping is on the Y axis.

---

## Serialized Fields

```csharp
[SerializeField] private float moveSpeed = 7f;
```
**Explanation:** Base movement speed on the XZ plane. Configurable in the Inspector.

---

## Private Fields

```csharp
private Rigidbody rb;
```
**Explanation:** Cached reference to the Rigidbody for setting velocity.

```csharp
private ModularJumpSystem_UMFOSS jumpSystem;
```
**Explanation:** Reference to the ModularJumpSystem on the same GameObject.

---

## Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody>();
    jumpSystem = GetComponent<ModularJumpSystem_UMFOSS>();
```
**Explanation:** Caches references to both required components.

```csharp
    rb.constraints = RigidbodyConstraints.FreezeRotation;
```
**Explanation:** Freezes all rotation axes (X, Y, Z) on the Rigidbody. Without this, the capsule character would topple over on collisions because physics forces can cause rotation. This is a common setup for 3D character controllers.

```csharp
    jumpSystem.OnJumpStart += () => Debug.Log("Jump started!");
    jumpSystem.OnJumpEnd += () => Debug.Log("Landed!");
}
```
**Explanation:** Subscribes to jump events for debug logging. Same pattern as the 2D demo — in a real game, replace with animation triggers, particle effects, or sound playback.

---

## Update

```csharp
private void Update()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
    float vertical = Input.GetAxisRaw("Vertical");
```
**Explanation:** Reads both horizontal (A/D or Left/Right arrows) and vertical (W/S or Up/Down arrows) input. Unlike the 2D demo which only uses horizontal, 3D movement needs both axes for the XZ plane.

```csharp
    float speed = moveSpeed * jumpSystem.AirControlMultiplier;
```
**Explanation:** Applies the air control multiplier to movement speed — same pattern as the 2D demo.

```csharp
    Vector3 move = new Vector3(horizontal, 0f, vertical).normalized * speed;
```
**Explanation:** Creates a movement vector on the XZ plane (Y = 0 because vertical movement is handled by the jump system). `.normalized` ensures diagonal movement isn't faster than cardinal movement (without normalization, moving diagonally would be ~1.41x speed).

```csharp
    rb.velocity = new Vector3(move.x, rb.velocity.y, move.z);
```
**Explanation:** Sets XZ velocity from input while preserving Y velocity (controlled by the jump system and gravity). This pattern — controlling horizontal and vertical velocity separately — is the standard approach for 3D character controllers.

```csharp
    if (Input.GetKeyDown(KeyCode.Space))
    {
        jumpSystem.OnJumpPressed();
    }
    if (Input.GetKeyUp(KeyCode.Space))
    {
        jumpSystem.OnJumpReleased();
    }
}
```
**Explanation:** Same jump input fallback as the 2D demo. Passes Space key press/release to the ModularJumpSystem. This is used when no InputActionReference is assigned directly on the jump system component.
