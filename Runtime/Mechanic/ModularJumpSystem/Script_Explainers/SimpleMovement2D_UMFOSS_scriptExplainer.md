# SimpleMovement2D_UMFOSS — Line-by-Line Script Explainer

---

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine for MonoBehaviour, Rigidbody2D, Vector2, etc.

```csharp
using UnityEngine.InputSystem;
```
**Explanation:** Imports the new Unity Input System package to read keyboard input via the Keyboard class.

```csharp
using GameplayMechanicsUMFOSS.Physics;
```
**Explanation:** Imports the Physics namespace. Although this script doesn't directly use the adapter, it's included for potential future use.

```csharp
namespace GameplayMechanicsUMFOSS.Movement
```
**Explanation:** Places this class under the Movement namespace alongside the ModularJumpSystem.

```csharp
[RequireComponent(typeof(Rigidbody2D))]
```
**Explanation:** Ensures a Rigidbody2D exists on the same GameObject. Required for physics-based movement.

```csharp
[AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Simple Movement 2D")]
```
**Explanation:** Adds a clean entry in Unity's "Add Component" menu for easy attachment.

```csharp
public class SimpleMovement2D_UMFOSS : MonoBehaviour
```
**Explanation:** A simple horizontal movement controller for 2D characters. Designed to work alongside ModularJumpSystem_UMFOSS and automatically reads its air control multiplier.

---

## Serialized Fields

```csharp
[SerializeField] private float moveSpeed = 7f;
```
**Explanation:** Base horizontal movement speed. Configurable in the Inspector. This gets multiplied by the air control multiplier when airborne.

---

## Private Fields

```csharp
private Rigidbody2D rb;
```
**Explanation:** Cached reference to the Rigidbody2D for setting velocity.

```csharp
private ModularJumpSystem_UMFOSS jumpSystem;
```
**Explanation:** Optional reference to the jump system on the same GameObject. If present, the movement speed is modified by AirControlMultiplier when the player is airborne.

---

## Awake

```csharp
private void Awake()
{
    rb = GetComponent<Rigidbody2D>();
    jumpSystem = GetComponent<ModularJumpSystem_UMFOSS>();
}
```
**Explanation:** Caches both the Rigidbody2D and the optional ModularJumpSystem. GetComponent for jumpSystem may return null if no jump system is attached — this is handled with a null check in Update.

---

## Update

```csharp
private void Update()
{
    float horizontal = 0f;
    Keyboard kb = Keyboard.current;
    if (kb != null)
    {
        if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
        if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;
    }
```
**Explanation:** Reads horizontal input using the new Unity Input System's Keyboard class. Checks for A/Left Arrow (move left = -1) and D/Right Arrow (move right = +1). The `kb != null` check prevents errors when no keyboard is connected (e.g., on mobile).

```csharp
    float speed = moveSpeed;
    if (jumpSystem != null)
    {
        speed *= jumpSystem.AirControlMultiplier;
    }
```
**Explanation:** Starts with the base moveSpeed, then multiplies by the jump system's AirControlMultiplier if one is attached. AirControlMultiplier returns 1.0 when grounded (full speed) and a reduced value (e.g., 0.8) when airborne. This creates the feeling of reduced air control during jumps.

```csharp
    rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
}
```
**Explanation:** Sets the horizontal velocity directly while preserving the current vertical velocity (controlled by the jump system and gravity). `horizontal * speed` gives the final movement speed with direction. Using `linearVelocity` (newer Unity API) instead of `velocity`.
