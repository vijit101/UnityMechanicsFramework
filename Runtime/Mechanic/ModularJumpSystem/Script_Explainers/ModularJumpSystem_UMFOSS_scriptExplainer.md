# ModularJumpSystem_UMFOSS — Line-by-Line Script Explainer

---

## Imports & Namespace

```csharp
using System;
```
**Explanation:** Imports the System namespace for the `Action` delegate type used in events (OnJumpStart, OnJumpEnd).

```csharp
using UnityEngine;
```
**Explanation:** Imports Unity's core engine classes — MonoBehaviour, Vector3, SerializeField, LayerMask, etc.

```csharp
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
```
**Explanation:** Conditionally imports the new Unity Input System package. Only compiles if the package is installed — prevents build errors when the package is absent.

```csharp
using GameplayMechanicsUMFOSS.Physics;
```
**Explanation:** Imports the Physics namespace to access IPhysicsAdapter, Physics2DAdapter_UMFOSS, and Physics3DAdapter_UMFOSS.

```csharp
namespace GameplayMechanicsUMFOSS.Movement
```
**Explanation:** Places this class under the Movement namespace, keeping the framework organized by feature category.

---

## Enums

```csharp
public enum JumpState
{
    Grounded,
    Rising,
    Falling
}
```
**Explanation:** Defines the three possible jump states. `Grounded` = on the ground, `Rising` = moving upward after a jump, `Falling` = moving downward. External scripts can read this to trigger animations or sound effects.

```csharp
public enum DimensionMode
{
    Mode2D,
    Mode3D
}
```
**Explanation:** Dropdown selector for the Inspector. Determines whether the system uses Rigidbody2D (2D) or Rigidbody (3D) physics via the adapter pattern.

---

## Class Declaration & Attributes

```csharp
[DefaultExecutionOrder(-10)]
```
**Explanation:** Forces this script to run BEFORE most other scripts in Update/FixedUpdate. This ensures gravity scale is set before the 3D adapter applies gravity in its own FixedUpdate.

```csharp
[AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Modular Jump System")]
```
**Explanation:** Adds a clean, organized entry in Unity's "Add Component" menu so users can find it easily.

```csharp
public class ModularJumpSystem_UMFOSS : MonoBehaviour
```
**Explanation:** Main class declaration. Inherits from MonoBehaviour so it can be attached to GameObjects and use Unity lifecycle methods (Awake, Update, FixedUpdate).

---

## Serialized Fields — Dimension Mode

```csharp
[SerializeField] private DimensionMode dimensionMode = DimensionMode.Mode2D;
```
**Explanation:** Stores the selected physics dimension mode. Defaults to 2D. The `[SerializeField]` attribute exposes it in the Inspector while keeping it private in code.

---

## Serialized Fields — Jump Settings

```csharp
[SerializeField] private float jumpForce = 12f;
```
**Explanation:** The initial upward velocity applied when the player jumps. Set directly as velocity (not physics force) for predictable, mass-independent jump heights.

```csharp
[SerializeField, Min(1)] private int maxJumps = 2;
```
**Explanation:** Maximum number of jumps allowed before landing. 1 = single jump, 2 = double jump, etc. `Min(1)` prevents setting it to 0 in the Inspector.

```csharp
[SerializeField] private bool consistentJumpHeight = false;
```
**Explanation:** When false, releasing the jump button early cuts the jump short (variable height). When true, jump always reaches full height regardless of how long the button is held.

```csharp
[SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;
```
**Explanation:** When the jump button is released early, upward velocity is multiplied by this value. 0.0 = instant stop, 1.0 = no effect. Only used when consistentJumpHeight is false.

---

## Serialized Fields — Gravity Settings

```csharp
[SerializeField] private bool applyGravityWhileRising = true;
```
**Explanation:** Toggle to enable/disable gravity while the character is moving upward. Setting to false creates a "floaty" feeling at the jump peak.

```csharp
[SerializeField] private bool applyGravityWhileFalling = true;
```
**Explanation:** Toggle to enable/disable gravity while falling. Almost always true — disabling it would make the character float in the air.

```csharp
[SerializeField] private float riseGravityMultiplier = 1f;
```
**Explanation:** Gravity strength multiplier while rising. Higher values = faster deceleration = shorter time to reach the peak of the jump.

```csharp
[SerializeField] private float fallGravityMultiplier = 2.5f;
```
**Explanation:** Gravity strength multiplier while falling. 2.5x is a classic platformer value — it makes the fall feel snappy and responsive rather than floaty. This is a key "game feel" parameter.

```csharp
[SerializeField] private float terminalVelocity = 20f;
```
**Explanation:** Maximum downward speed cap. Prevents the character from accelerating infinitely during long falls. Set to 0 to disable the cap entirely.

---

## Serialized Fields — Platformer Enhancements

```csharp
[SerializeField] private float coyoteTimeDuration = 0.15f;
```
**Explanation:** Grace period (in seconds) after walking off a ledge where the player can still jump. Makes platforming feel forgiving — the player doesn't need pixel-perfect timing at ledge edges.

```csharp
[SerializeField] private float jumpBufferDuration = 0.1f;
```
**Explanation:** Time window (in seconds) before landing where a jump press is stored and auto-executes on ground contact. Prevents the frustrating "I pressed jump but it didn't register" situation.

```csharp
[SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.8f;
```
**Explanation:** Movement speed multiplier while airborne. 0.8 = 80% of ground speed. Exposed as a public property for external movement scripts to read. This script does NOT handle horizontal movement — it just provides the multiplier.

---

## Serialized Fields — Ground Detection

```csharp
[SerializeField] private Vector3 groundCheckOffset = Vector3.zero;
```
**Explanation:** Offset from the GameObject's position where the ground check raycast originates. Adjust this if the character's pivot point isn't at their feet.

```csharp
[SerializeField] private float groundCheckDistance = 0.2f;
```
**Explanation:** How far below the origin point to check for ground. Should be slightly larger than the distance between the pivot and the bottom of the collider.

```csharp
[SerializeField] private LayerMask groundLayer = ~0;
```
**Explanation:** Which layers count as ground. Default `~0` means "everything." In practice, set this to only your "Ground" layer for accurate detection.

---

## Serialized Fields — Input

```csharp
#if ENABLE_INPUT_SYSTEM
[SerializeField] private InputActionReference jumpInputAction;
#endif
```
**Explanation:** Reference to a Jump action from a Unity Input Actions asset. Wrapped in a preprocessor directive so the code compiles even without the Input System package. When left empty, use OnJumpPressed()/OnJumpReleased() manually.

---

## Private Fields

```csharp
private IPhysicsAdapter physicsAdapter;
```
**Explanation:** Reference to the physics adapter (either 2D or 3D). All physics operations go through this interface — the jump system never touches Rigidbody directly.

```csharp
private int jumpsRemaining;
```
**Explanation:** Tracks how many jumps the player has left. Decremented on each jump, reset to maxJumps on landing.

```csharp
private float coyoteTimeCounter;
```
**Explanation:** Countdown timer for coyote time. Starts at coyoteTimeDuration when walking off a ledge, ticks down each frame.

```csharp
private float jumpBufferCounter;
```
**Explanation:** Countdown timer for jump buffering. Starts at jumpBufferDuration when jump is pressed while airborne, ticks down each frame.

```csharp
private bool isGrounded;
```
**Explanation:** Whether the character is currently touching the ground this frame.

```csharp
private bool wasGrounded;
```
**Explanation:** Whether the character was touching the ground last frame. Used to detect landing/leaving transitions.

```csharp
private bool jumpRequested;
```
**Explanation:** Flag set to true when the player presses the jump button. Consumed when a jump is executed.

```csharp
private bool jumpHeld;
```
**Explanation:** Whether the jump button is currently being held down. Used for variable jump height — releasing early cuts the jump.

```csharp
private bool jumpCutApplied;
```
**Explanation:** Prevents the variable jump cut from being applied more than once per jump. Set to true after the first velocity cut.

```csharp
private JumpState currentState = JumpState.Grounded;
```
**Explanation:** Tracks the current jump phase. Updated every frame based on vertical velocity and ground state.

```csharp
private const float DEFAULT_GRAVITY_SCALE = 1f;
```
**Explanation:** The default gravity scale used when grounded. Constant value of 1 = normal gravity.

---

## Public Properties

```csharp
public bool IsGrounded => isGrounded;
```
**Explanation:** Read-only property exposing the grounded state. External scripts can check this for animations, sounds, etc.

```csharp
public JumpState CurrentJumpState => currentState;
```
**Explanation:** Read-only property exposing the current jump state (Grounded/Rising/Falling).

```csharp
public int JumpsRemaining => jumpsRemaining;
```
**Explanation:** Read-only property showing how many jumps are left. Useful for UI display or conditional logic.

```csharp
public float AirControlMultiplier => isGrounded ? 1f : airControlMultiplier;
```
**Explanation:** Returns 1.0 when grounded (full speed) or airControlMultiplier when airborne (reduced speed). External movement scripts multiply their speed by this value.

```csharp
public IPhysicsAdapter PhysicsAdapter => physicsAdapter;
```
**Explanation:** Read-only access to the active physics adapter. Allows other scripts to use the same adapter for consistency.

```csharp
public DimensionMode CurrentDimensionMode => dimensionMode;
```
**Explanation:** Read-only property exposing which dimension mode is active (2D or 3D).

---

## Events

```csharp
public event Action OnJumpStart;
```
**Explanation:** Event fired every time a jump begins. Subscribe to this for jump animations, particles, or sound effects. Fires separately for each jump in a multi-jump sequence.

```csharp
public event Action OnJumpEnd;
```
**Explanation:** Event fired when the character lands on the ground after being airborne. Subscribe for landing effects, camera shake, etc.

---

## Unity Lifecycle — Awake

```csharp
private void Awake()
{
    InitializePhysicsAdapter();
    jumpsRemaining = maxJumps;
}
```
**Explanation:** Called once when the script initializes. Sets up the physics adapter (auto-adds one if missing) and initializes the jump counter to the maximum value.

---

## Unity Lifecycle — OnEnable / OnDisable

```csharp
private void OnEnable()
{
    if (jumpInputAction != null && jumpInputAction.action != null)
    {
        jumpInputAction.action.Enable();
    }
}
```
**Explanation:** When the component is enabled, enables the input action so it can receive jump button events. Null checks prevent errors when no input action is assigned.

```csharp
private void OnDisable()
{
    if (jumpInputAction != null && jumpInputAction.action != null)
    {
        jumpInputAction.action.Disable();
    }
}
```
**Explanation:** When the component is disabled, disables the input action to stop receiving events. Proper cleanup prevents input from firing on inactive objects.

---

## Unity Lifecycle — Update (runs every frame)

```csharp
private void Update()
{
    ReadInput();
    CheckGround();
    HandleCoyoteTime();
    HandleJumpBuffer();
    TryExecuteJump();
    HandleVariableJumpHeight();
    UpdateState();
}
```
**Explanation:** The main frame loop. Executes seven steps in order every frame: (1) read input, (2) check if grounded, (3) manage coyote time, (4) manage jump buffer, (5) attempt to jump, (6) handle variable jump height, (7) update the state enum. Order matters — ground check must happen before jump logic.

---

## Unity Lifecycle — FixedUpdate (runs every physics step)

```csharp
private void FixedUpdate()
{
    ApplyGravityMultiplier();
    ClampTerminalVelocity();
}
```
**Explanation:** Physics-rate loop (default 50 times/sec). Adjusts gravity scale based on whether the character is rising or falling, then clamps downward speed to terminal velocity. Done in FixedUpdate because it modifies physics values.

---

## Public Methods — OnJumpPressed

```csharp
public void OnJumpPressed()
{
    jumpRequested = true;
    jumpBufferCounter = jumpBufferDuration;
    jumpHeld = true;
}
```
**Explanation:** Call this from your input handler when the jump button is pressed down. Sets the jump request flag, starts the jump buffer timer, and marks the button as held. Use this when NOT using InputActionReference.

---

## Public Methods — OnJumpReleased

```csharp
public void OnJumpReleased()
{
    jumpHeld = false;
}
```
**Explanation:** Call this from your input handler when the jump button is released. Clears the held flag, which triggers variable jump height cut if the character is still rising.

---

## Public Methods — ForceJump (no parameter)

```csharp
public void ForceJump()
{
    if (jumpsRemaining > 0 || isGrounded)
    {
        ExecuteJump();
    }
}
```
**Explanation:** Programmatic jump that respects the jump counter. Use for bounce pads, springs, or power-ups. Only executes if the player has jumps remaining or is on the ground.

---

## Public Methods — ForceJump (with custom force)

```csharp
public void ForceJump(float customForce)
{
    Vector3 velocity = physicsAdapter.Velocity;
    velocity.y = customForce;
    physicsAdapter.Velocity = velocity;
    jumpCutApplied = true;
    currentState = JumpState.Rising;
    OnJumpStart?.Invoke();
}
```
**Explanation:** Programmatic jump with a custom upward velocity that BYPASSES the jump counter. Use for launch pads, explosions, or scripted events. Sets jumpCutApplied to true so variable height doesn't interfere. Fires the OnJumpStart event.

---

## Private Methods — InitializePhysicsAdapter

```csharp
private void InitializePhysicsAdapter()
{
    physicsAdapter = GetComponent<IPhysicsAdapter>();
    if (physicsAdapter != null) return;

    switch (dimensionMode)
    {
        case DimensionMode.Mode2D:
            physicsAdapter = gameObject.AddComponent<Physics2DAdapter_UMFOSS>();
            break;
        case DimensionMode.Mode3D:
            physicsAdapter = gameObject.AddComponent<Physics3DAdapter_UMFOSS>();
            break;
    }

    Debug.Log($"[ModularJumpSystem] Auto-added {dimensionMode} physics adapter to '{gameObject.name}'.");
}
```
**Explanation:** First checks if a physics adapter already exists on the GameObject (user may have added one manually). If not found, automatically adds the correct adapter based on the selected DimensionMode. Logs a message so the user knows it was auto-added. This is the core of the adapter pattern — the jump system never needs to know which concrete adapter it's using.

---

## Private Methods — ReadInput

```csharp
private void ReadInput()
{
    if (jumpInputAction == null || jumpInputAction.action == null) return;

    if (jumpInputAction.action.WasPressedThisFrame())
    {
        jumpRequested = true;
        jumpBufferCounter = jumpBufferDuration;
    }

    jumpHeld = jumpInputAction.action.IsPressed();
}
```
**Explanation:** Reads the Unity Input System action (if assigned). WasPressedThisFrame() detects the press moment, IsPressed() checks if the button is still held. If no InputActionReference is assigned, this method returns early and does nothing — the user must call OnJumpPressed/OnJumpReleased manually instead.

---

## Private Methods — CheckGround

```csharp
private void CheckGround()
{
    wasGrounded = isGrounded;
    Vector3 checkOrigin = transform.position + groundCheckOffset;
    isGrounded = physicsAdapter.CheckGrounded(checkOrigin, groundCheckDistance, groundLayer);

    if (isGrounded && !wasGrounded)
    {
        HandleLanding();
    }
}
```
**Explanation:** Saves the previous grounded state, then uses the physics adapter to raycast/spherecast downward. If the character just landed (wasn't grounded last frame but is now), calls HandleLanding() to reset jumps and fire the landing event.

---

## Private Methods — HandleLanding

```csharp
private void HandleLanding()
{
    jumpsRemaining = maxJumps;
    coyoteTimeCounter = 0f;

    if (currentState != JumpState.Grounded)
    {
        currentState = JumpState.Grounded;
        OnJumpEnd?.Invoke();
    }
}
```
**Explanation:** Called when the character touches the ground after being airborne. Resets jumps to maximum, clears coyote time, sets state to Grounded, and fires the OnJumpEnd event. The state check prevents the event from firing if somehow called while already grounded.

---

## Private Methods — HandleCoyoteTime

```csharp
private void HandleCoyoteTime()
{
    if (wasGrounded && !isGrounded && currentState == JumpState.Grounded)
    {
        coyoteTimeCounter = coyoteTimeDuration;
    }

    if (coyoteTimeCounter > 0f)
    {
        coyoteTimeCounter -= Time.deltaTime;

        if (coyoteTimeCounter <= 0f && jumpsRemaining == maxJumps)
        {
            jumpsRemaining--;
        }
    }
}
```
**Explanation:** Detects when the player walks off a ledge (was grounded, now isn't, and didn't jump). Starts a grace period timer. During this window, the player can still jump as if grounded. If the timer expires without jumping, one jump is consumed — the "free ground jump" was forfeited.

---

## Private Methods — HandleJumpBuffer

```csharp
private void HandleJumpBuffer()
{
    if (jumpBufferCounter > 0f)
    {
        jumpBufferCounter -= Time.deltaTime;
    }
    else
    {
        jumpRequested = false;
    }
}
```
**Explanation:** Counts down the jump buffer timer. While active, the jump request stays alive. Once it expires, the jump request is cleared. This means pressing jump slightly before landing will still register.

---

## Private Methods — TryExecuteJump

```csharp
private void TryExecuteJump()
{
    if (!jumpRequested && jumpBufferCounter <= 0f) return;

    bool canCoyoteJump = coyoteTimeCounter > 0f && jumpsRemaining == maxJumps;
    bool canJump = isGrounded || canCoyoteJump || jumpsRemaining > 0;

    if (!canJump) return;

    ExecuteJump();
    jumpRequested = false;
    jumpBufferCounter = 0f;
    coyoteTimeCounter = 0f;
}
```
**Explanation:** The decision gate for jumping. Checks three conditions: (1) is on the ground, (2) is within coyote time, (3) has jumps remaining (for multi-jump). If any condition is true, executes the jump and clears all timers/flags to prevent double-execution.

---

## Private Methods — ExecuteJump

```csharp
private void ExecuteJump()
{
    Vector3 velocity = physicsAdapter.Velocity;
    velocity.y = jumpForce;
    physicsAdapter.Velocity = velocity;

    jumpsRemaining--;
    jumpCutApplied = false;
    currentState = JumpState.Rising;
    OnJumpStart?.Invoke();
}
```
**Explanation:** The actual jump execution. Sets vertical velocity directly to jumpForce (not AddForce) for predictable, mass-independent height. Decrements the jump counter, resets the jump cut flag for variable height, sets state to Rising, and fires the OnJumpStart event.

---

## Private Methods — HandleVariableJumpHeight

```csharp
private void HandleVariableJumpHeight()
{
    if (consistentJumpHeight) return;
    if (currentState != JumpState.Rising) return;
    if (jumpCutApplied) return;

    if (!jumpHeld)
    {
        Vector3 velocity = physicsAdapter.Velocity;
        if (velocity.y > 0f)
        {
            velocity.y *= jumpCutMultiplier;
            physicsAdapter.Velocity = velocity;
            jumpCutApplied = true;
        }
    }
}
```
**Explanation:** Implements variable jump height. Three guard clauses: skip if consistent height is on, skip if not rising, skip if already cut. When the player releases the jump button while rising, the upward velocity is multiplied by jumpCutMultiplier (e.g., 0.5 = halved). The flag ensures this only happens once per jump. Result: short press = small hop, long press = full jump.

---

## Private Methods — UpdateState

```csharp
private void UpdateState()
{
    if (isGrounded)
    {
        currentState = JumpState.Grounded;
        return;
    }

    float verticalVelocity = physicsAdapter.Velocity.y;
    currentState = verticalVelocity > 0f ? JumpState.Rising : JumpState.Falling;
}
```
**Explanation:** Updates the JumpState enum based on current conditions. If grounded, state is Grounded. If airborne, checks vertical velocity: positive = Rising, negative or zero = Falling. External scripts read this for animation/state logic.

---

## Private Methods — ApplyGravityMultiplier (FixedUpdate)

```csharp
private void ApplyGravityMultiplier()
{
    if (isGrounded)
    {
        physicsAdapter.GravityScale = DEFAULT_GRAVITY_SCALE;
        return;
    }

    float verticalVelocity = physicsAdapter.Velocity.y;

    if (verticalVelocity > 0f)
    {
        physicsAdapter.GravityScale = applyGravityWhileRising ? riseGravityMultiplier : 0f;
    }
    else
    {
        physicsAdapter.GravityScale = applyGravityWhileFalling ? fallGravityMultiplier : 0f;
    }
}
```
**Explanation:** Dynamically adjusts gravity based on the jump phase. When grounded, uses normal gravity (1x). While rising, applies riseGravityMultiplier (or 0 if gravity is disabled for rising). While falling, applies fallGravityMultiplier (typically 2.5x for snappy falls). This creates the classic platformer "heavy fall" feel.

---

## Private Methods — ClampTerminalVelocity (FixedUpdate)

```csharp
private void ClampTerminalVelocity()
{
    if (terminalVelocity <= 0f) return;

    Vector3 velocity = physicsAdapter.Velocity;
    if (velocity.y < -terminalVelocity)
    {
        velocity.y = -terminalVelocity;
        physicsAdapter.Velocity = velocity;
    }
}
```
**Explanation:** Prevents the character from falling faster than the terminal velocity cap. If terminalVelocity is 0, the cap is disabled. Checks if downward velocity exceeds the limit and clamps it. Prevents unrealistic acceleration during long falls.

---

## Editor Methods — OnDrawGizmosSelected

```csharp
private void OnDrawGizmosSelected()
{
    Vector3 origin = transform.position + groundCheckOffset;
    bool grounded = Application.isPlaying && isGrounded;

    Gizmos.color = grounded ? Color.green : Color.red;
    Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
    Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.05f);
}
```
**Explanation:** Draws a visual debug line in the Scene view when the GameObject is selected. Shows the ground check ray — green when grounded, red when airborne. Helps developers visually tune groundCheckOffset and groundCheckDistance. Only runs in the editor, not in builds.

---

## Editor Methods — OnValidate

```csharp
private void OnValidate()
{
    if (jumpForce < 0f) jumpForce = 0f;
    if (riseGravityMultiplier < 0f) riseGravityMultiplier = 0f;
    if (fallGravityMultiplier < 0f) fallGravityMultiplier = 0f;
    if (terminalVelocity < 0f) terminalVelocity = 0f;
    if (coyoteTimeDuration < 0f) coyoteTimeDuration = 0f;
    if (jumpBufferDuration < 0f) jumpBufferDuration = 0f;
    if (groundCheckDistance < 0f) groundCheckDistance = 0f;
}
```
**Explanation:** Called automatically when a value changes in the Inspector. Clamps all numeric fields to 0 minimum — negative values don't make sense for any of these parameters. Provides immediate feedback in the editor.
