# JumpDemo2DController_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine for MonoBehaviour, Rigidbody2D, Vector2, Input, KeyCode, Debug, etc.

```csharp
using GameplayMechanicsUMFOSS.Movement;
```
**Explanation:** Imports the Movement namespace to access ModularJumpSystem_UMFOSS.

```csharp
namespace GameplayMechanicsUMFOSS.Samples.Jump
```
**Explanation:** Places this class under the Samples.Jump namespace — separate from the main framework code. This is a demo/example script, not part of the core library.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
```
**Explanation:** Ensures a Rigidbody2D exists on this GameObject. Required for 2D physics-based movement.

```csharp
[RequireComponent(typeof(ModularJumpSystem_UMFOSS))]
```
**Explanation:** Ensures the ModularJumpSystem is also attached. This demo controller depends on it for jump functionality and air control.

```csharp
public class JumpDemo2DController_UMFOSS : MonoBehaviour
```
**Explanation:** A minimal 2D player controller that demonstrates how to use the ModularJumpSystem. This is meant as a reference/example — developers would write their own controller based on this pattern.

---

## Serialized Fields

```csharp
[SerializeField] private float moveSpeed = 7f;
```
**Explanation:** Base horizontal movement speed. Configurable in the Inspector.

---

## Private Fields

```csharp
private Rigidbody2D rb;
```
**Explanation:** Cached reference to the Rigidbody2D for setting velocity.

```csharp
private ModularJumpSystem_UMFOSS jumpSystem;
```
**Explanation:** Reference to the ModularJumpSystem on the same GameObject. Used for jump input and reading the air control multiplier.

---

## Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    jumpSystem = GetComponent<ModularJumpSystem_UMFOSS>();
```
**Explanation:** Caches references to both required components for use in Update.

```csharp
    jumpSystem.OnJumpStart += () => Debug.Log("Jump started!");
    jumpSystem.OnJumpEnd += () => Debug.Log("Landed!");
}
```
**Explanation:** Subscribes to the jump system's events using lambda expressions. OnJumpStart fires when any jump begins (including double jumps), OnJumpEnd fires when the character lands. In a real game, you'd trigger animations, particles, or sounds here instead of Debug.Log.

---

## Update

```csharp
private void Update()
{
    float horizontal = Input.GetAxisRaw("Horizontal");
```
**Explanation:** Reads horizontal input using Unity's legacy Input system. GetAxisRaw returns -1 (left), 0 (none), or 1 (right) with no smoothing. Works with arrow keys and A/D by default.

```csharp
    float speed = moveSpeed * jumpSystem.AirControlMultiplier;
```
**Explanation:** Calculates the effective movement speed by multiplying base speed with the jump system's air control modifier. When grounded = full speed, when airborne = reduced speed (e.g., 80%).

```csharp
    rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
```
**Explanation:** Sets horizontal velocity directly while preserving vertical velocity (controlled by the jump system). This is a simple approach — production code might use acceleration/deceleration for smoother movement.

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
**Explanation:** Passes jump input to the ModularJumpSystem using the legacy Input API as a fallback. GetKeyDown detects the frame Space is pressed (triggers jump request + buffer). GetKeyUp detects the frame Space is released (enables variable jump height cut). This is the alternative to using an InputActionReference directly on the jump system.
