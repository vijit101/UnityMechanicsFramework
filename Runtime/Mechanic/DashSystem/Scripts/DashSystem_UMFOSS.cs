using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GameplayMechanicsUMFOSS.Physics;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// Dash state tracked by the system.
    /// Ready = can dash, Dashing = mid-dash, Cooldown = waiting for reset.
    /// </summary>
    public enum DashState
    {
        Ready,
        Dashing,
        Cooldown
    }

    /// <summary>
    /// Determines how the dash direction is calculated.
    /// </summary>
    public enum DashDirectionMode
    {
        /// <summary>Dash in the direction the player was last moving.</summary>
        LastMoveDirection,

        /// <summary>Dash in the direction the character is currently facing.</summary>
        FacingDirection
    }

    /// <summary>
    /// A fully modular, configurable dash system supporting both 2D and 3D physics
    /// via the IPhysicsAdapter pattern. Drop it onto any GameObject, select a dimension
    /// mode, configure Inspector fields, and get dash charges, cooldowns, iframes,
    /// and event-driven communication — all without coupling to any player controller.
    ///
    /// Features:
    /// - 2D/3D support via IPhysicsAdapter (zero duplicated physics logic)
    /// - Configurable dash distance, duration, and speed
    /// - Charge system with configurable max count (-1 = unlimited)
    /// - Cooldown timer between dash refills
    /// - Two direction modes: LastMoveDirection and FacingDirection
    /// - Optional gravity ignore during dash
    /// - Optional velocity maintenance vs ease-out
    /// - Optional invincibility frames (iframes) during dash
    /// - Dash count resets on ground contact
    /// - Optional cooldown reset on enemy kill (via EventBus)
    /// - Four event hooks: OnDashStart, OnDashEnd, OnDashReady, OnDashCountChanged
    /// - Full EventBus integration for decoupled cross-system communication
    /// - Unity Input System integration with fallback public API
    /// - No hardcoded KeyCode references
    /// - No direct Rigidbody/Rigidbody2D references
    /// - No GetComponent calls in Update/FixedUpdate
    ///
    /// Setup:
    /// 1. Attach this component to your player GameObject
    /// 2. Select DimensionMode (Mode2D or Mode3D) — adapter is auto-added if missing
    /// 3. Assign a Dash InputActionReference, or call OnDashPressed() manually
    /// 4. Feed movement direction via SetMoveDirection() from your movement script
    /// 5. Configure all settings in Inspector
    /// </summary>
    [DefaultExecutionOrder(-5)]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Dash System")]
    public class DashSystem_UMFOSS : MonoBehaviour
    {
        // ═══════════════════════════════════════════════
        // Constants — no magic numbers
        // ═══════════════════════════════════════════════

        /// <summary>Minimum allowed dash duration to prevent division-by-zero issues.</summary>
        private const float MIN_DASH_DURATION = 0.01f;

        /// <summary>Default gravity scale restored after a dash that suppressed gravity.</summary>
        private const float DEFAULT_GRAVITY_SCALE = 1f;

        /// <summary>Input magnitude below this is treated as zero (stick drift protection).</summary>
        private const float DIRECTION_DEADZONE = 0.01f;

        /// <summary>Value representing unlimited dash charges.</summary>
        private const int UNLIMITED_DASHES = -1;

        // ═══════════════════════════════════════════════
        // Serialized / Inspector fields
        // ═══════════════════════════════════════════════

        #region Inspector Fields

        [Header("Dimension Mode")]
        [Tooltip("Select 2D or 3D physics. The appropriate adapter will be auto-added if not present.")]
        [SerializeField] private DimensionMode dimensionMode = DimensionMode.Mode2D;

        [Header("Dash Settings")]
        [Tooltip("Total distance the player covers during a dash (in world units).")]
        [SerializeField] private float dashDistance = 5f;

        [Tooltip("Duration of the dash in seconds. Shorter = snappier.")]
        [SerializeField, Min(0.01f)] private float dashDuration = 0.2f;

        [Header("Dash Direction")]
        [Tooltip("How the dash direction is determined.\n" +
                 "LastMoveDirection: dash toward where the player was last moving.\n" +
                 "FacingDirection: dash toward where the character is currently facing.")]
        [SerializeField] private DashDirectionMode dashDirectionMode = DashDirectionMode.LastMoveDirection;

        [Header("Cooldown & Charges")]
        [Tooltip("Time in seconds before dash charges begin refilling after the last dash.")]
        [SerializeField] private float cooldownDuration = 1f;

        [Tooltip("Maximum number of dashes before cooldown triggers.\n" +
                 "Set to -1 for unlimited dashes (no charge limit).")]
        [SerializeField] private int maxDashCount = 2;

        [Header("Gravity & Velocity")]
        [Tooltip("When enabled, gravity is set to zero for the duration of the dash.\n" +
                 "Prevents the player from arcing downward during a horizontal dash.")]
        [SerializeField] private bool ignoreGravityDuringDash = true;

        [Tooltip("When enabled, dash velocity stays constant for the entire duration.\n" +
                 "When disabled, velocity eases out (lerps to zero) over the dash duration.")]
        [SerializeField] private bool maintainDashVelocity = true;

        [Header("Iframe Settings")]
        [Tooltip("Enable invincibility frames during the dash. When active, a DashIframeStartEvent " +
                 "is published on the EventBus so hurtbox systems can disable the player's hurtbox.")]
        [SerializeField] private bool enableIframes = false;

        [Tooltip("Duration of the iframe window in seconds. Capped at dashDuration.")]
        [SerializeField] private float iframeDuration = 0.2f;

        [Header("Platformer Options")]
        [Tooltip("Reset dash charges when the player touches the ground.")]
        [SerializeField] private bool resetCountOnGround = true;

        [Tooltip("Reset cooldown timer when the player kills an enemy.\n" +
                 "Requires external systems to publish PlayerKillEvent on the EventBus.")]
        [SerializeField] private bool resetCooldownOnKill = false;

        [Tooltip("When enabled, publishes a DashKillEvent when the player collides with " +
                 "objects on the enemy layer during a dash.")]
        [SerializeField] private bool dashCanKillEnemies = false;

        [Tooltip("Layer mask for objects that can be killed by dashing into them.\n" +
                 "Only used when dashCanKillEnemies is enabled.")]
        [SerializeField] private LayerMask enemyLayer;

        [Header("Ground Detection")]
        [Tooltip("Offset from transform.position for the ground check origin.")]
        [SerializeField] private Vector3 groundCheckOffset = Vector3.zero;

        [Tooltip("Distance below origin to check for ground.")]
        [SerializeField] private float groundCheckDistance = 0.2f;

        [Tooltip("Which layers count as ground.")]
        [SerializeField] private LayerMask groundLayer = ~0;

#if ENABLE_INPUT_SYSTEM
        [Header("Input (Unity Input System)")]
        [Tooltip("Reference to the Dash action from your Input Actions asset.\n" +
                 "Leave empty if calling OnDashPressed() manually from your own input code.")]
        [SerializeField] private InputActionReference dashInputAction;
#endif

        #endregion

        // ═══════════════════════════════════════════════
        // Private fields
        // ═══════════════════════════════════════════════

        #region Private Fields

        // Physics adapter — all forces/velocities go through this interface
        private IPhysicsAdapter physicsAdapter;

        // Dash state tracking
        private DashState currentDashState = DashState.Ready;
        private float dashTimer;
        private float cooldownTimer;
        private int dashesRemaining;
        private float dashSpeed;

        // Direction tracking
        private Vector3 lastMoveDirection = Vector3.right;
        private Vector3 currentDashDirection;

        // Iframe tracking
        private float iframeTimer;
        private bool iframesActive;

        // Gravity restoration
        private float savedGravityScale;

        // Ground detection
        private bool isGrounded;
        private bool wasGrounded;

        // Velocity ease-out tracking
        private float initialDashSpeed;

        #endregion

        // ═══════════════════════════════════════════════
        // Public properties
        // ═══════════════════════════════════════════════

        #region Public Properties

        /// <summary>Current dash state (Ready, Dashing, Cooldown).</summary>
        public DashState CurrentDashState => currentDashState;

        /// <summary>Whether the player is currently mid-dash.</summary>
        public bool IsDashing => currentDashState == DashState.Dashing;

        /// <summary>Number of dash charges remaining.</summary>
        public int DashesRemaining => dashesRemaining;

        /// <summary>Whether the character is currently on the ground.</summary>
        public bool IsGrounded => isGrounded;

        /// <summary>The active physics adapter instance.</summary>
        public IPhysicsAdapter PhysicsAdapter => physicsAdapter;

        /// <summary>The currently selected dimension mode.</summary>
        public DimensionMode CurrentDimensionMode => dimensionMode;

        /// <summary>The direction of the current or most recent dash.</summary>
        public Vector3 CurrentDashDirection => currentDashDirection;

        /// <summary>Whether iframes are currently active.</summary>
        public bool IframesActive => iframesActive;

        #endregion

        // ═══════════════════════════════════════════════
        // Events — C# events for direct subscribers
        // ═══════════════════════════════════════════════

        #region Events

        /// <summary>Fired the frame a dash begins.</summary>
        public event Action OnDashStart;

        /// <summary>Fired when dash duration completes.</summary>
        public event Action OnDashEnd;

        /// <summary>Fired when cooldown fully resets and player can dash again.</summary>
        public event Action OnDashReady;

        /// <summary>Fired every time a dash charge is consumed or restored.</summary>
        public event Action<int> OnDashCountChanged;

        #endregion

        // ═══════════════════════════════════════════════
        // Unity lifecycle
        // ═══════════════════════════════════════════════

        #region Unity Lifecycle

        private void Awake()
        {
            InitializePhysicsAdapter();
            // Compute dash speed from distance and duration
            // Speed = Distance / Time, ensuring duration is never zero
            dashSpeed = dashDistance / Mathf.Max(dashDuration, MIN_DASH_DURATION);
            initialDashSpeed = dashSpeed;
            ResetDashCharges();
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (dashInputAction != null && dashInputAction.action != null)
            {
                dashInputAction.action.Enable();
            }
#endif

            // Subscribe to external events via EventBus
            if (resetCooldownOnKill)
            {
                EventBus.Subscribe<PlayerKillEvent>(OnPlayerKill);
            }
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (dashInputAction != null && dashInputAction.action != null)
            {
                dashInputAction.action.Disable();
            }
#endif

            // Unsubscribe from EventBus to prevent memory leaks
            if (resetCooldownOnKill)
            {
                EventBus.Unsubscribe<PlayerKillEvent>(OnPlayerKill);
            }

            // Restore any modified state so disabling the component
            // doesn't leave the physics body in a broken state
            if (currentDashState == DashState.Dashing)
            {
                RestoreGravity();
                if (iframesActive)
                {
                    EndIframes();
                }
            }
        }

        private void Update()
        {
            ReadInput();
            CheckGround();
            UpdateDashState();
            UpdateIframes();
            UpdateCooldown();
        }

        private void FixedUpdate()
        {
            // Apply dash velocity in FixedUpdate for physics consistency
            if (currentDashState == DashState.Dashing)
            {
                ApplyDashVelocity();
            }

            // Handle dash-kill collision detection
            if (dashCanKillEnemies && currentDashState == DashState.Dashing)
            {
                CheckDashKillCollision();
            }
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Public methods
        // ═══════════════════════════════════════════════

        #region Public Methods

        /// <summary>
        /// Call from your input handler when the dash button is pressed.
        /// Use this when not using InputActionReference.
        /// </summary>
        public void OnDashPressed()
        {
            TryStartDash();
        }

        /// <summary>
        /// Feed the current movement direction from your movement controller.
        /// The dash system stores this as the last move direction for
        /// LastMoveDirection mode. Call this every frame from your movement script.
        /// </summary>
        /// <param name="direction">Normalized movement direction vector.</param>
        public void SetMoveDirection(Vector3 direction)
        {
            // Only update if the input is beyond the deadzone
            // This preserves the last valid direction when the player stops moving
            if (direction.sqrMagnitude > DIRECTION_DEADZONE * DIRECTION_DEADZONE)
            {
                lastMoveDirection = direction.normalized;
            }
        }

        /// <summary>
        /// Force a dash programmatically, bypassing input checks.
        /// Useful for scripted sequences, bounce pads, or AI-controlled characters.
        /// Still respects charge count and cooldown.
        /// </summary>
        public void ForceDash()
        {
            TryStartDash();
        }

        /// <summary>
        /// Force a dash in a specific direction, bypassing direction mode.
        /// </summary>
        /// <param name="direction">World-space direction to dash toward.</param>
        public void ForceDash(Vector3 direction)
        {
            if (!CanDash()) return;

            currentDashDirection = direction.normalized;
            StartDash();
        }

        /// <summary>
        /// Immediately reset all dash charges to maximum.
        /// Useful for powerups, checkpoints, or respawning.
        /// </summary>
        public void ResetDashCharges()
        {
            int previous = dashesRemaining;
            dashesRemaining = maxDashCount == UNLIMITED_DASHES ? 1 : maxDashCount;

            if (dashesRemaining != previous)
            {
                OnDashCountChanged?.Invoke(dashesRemaining);
                EventBus.Publish(new DashCountChangedEvent { remaining = dashesRemaining });
            }
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Private methods — Input Handling
        // ═══════════════════════════════════════════════

        #region Input Handling

        private void ReadInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (dashInputAction == null || dashInputAction.action == null) return;

            // WasPressedThisFrame ensures one dash per button press
            // Holding the button does NOT repeat dashes
            if (dashInputAction.action.WasPressedThisFrame())
            {
                TryStartDash();
            }
#endif
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Private methods — State Logic
        // ═══════════════════════════════════════════════

        #region State Logic

        private bool CanDash()
        {
            // Cannot dash while already dashing
            if (currentDashState == DashState.Dashing) return false;

            // Unlimited dashes mode — always allow (ignore charge count)
            if (maxDashCount == UNLIMITED_DASHES) return true;

            // Must have charges remaining
            return dashesRemaining > 0;
        }

        private void TryStartDash()
        {
            if (!CanDash()) return;

            // Resolve dash direction based on the selected mode
            currentDashDirection = ResolveDashDirection();

            // Safety check: if direction is zero (player never moved), default to right/forward
            if (currentDashDirection.sqrMagnitude < DIRECTION_DEADZONE * DIRECTION_DEADZONE)
            {
                currentDashDirection = dimensionMode == DimensionMode.Mode2D
                    ? Vector3.right
                    : Vector3.forward;
            }

            StartDash();
        }

        private void StartDash()
        {
            // Transition to dashing state
            currentDashState = DashState.Dashing;
            dashTimer = dashDuration;

            // Consume a charge (unless unlimited)
            if (maxDashCount != UNLIMITED_DASHES)
            {
                dashesRemaining--;
                OnDashCountChanged?.Invoke(dashesRemaining);
                EventBus.Publish(new DashCountChangedEvent { remaining = dashesRemaining });
            }

            // Handle gravity suppression during dash
            if (ignoreGravityDuringDash)
            {
                savedGravityScale = physicsAdapter.GravityScale;
                physicsAdapter.GravityScale = 0f;

                // Also zero out vertical velocity so the player doesn't
                // continue falling/rising from momentum before the dash
                Vector3 vel = physicsAdapter.Velocity;
                vel.y = 0f;
                physicsAdapter.Velocity = vel;
            }

            // Orient the player toward the dash direction to prevent erratic rotation
            OrientPlayer(currentDashDirection);

            // Recalculate speed in case Inspector values changed at runtime
            dashSpeed = dashDistance / Mathf.Max(dashDuration, MIN_DASH_DURATION);
            initialDashSpeed = dashSpeed;

            // Set initial dash velocity immediately for responsive feel
            physicsAdapter.Velocity = currentDashDirection * dashSpeed;

            // Start iframes if enabled
            if (enableIframes)
            {
                StartIframes();
            }

            // Fire events — C# events for direct subscribers
            OnDashStart?.Invoke();

            // Fire EventBus event — decoupled subscribers (VFX, audio, etc.)
            EventBus.Publish(new DashStartEvent
            {
                direction = currentDashDirection,
                duration = dashDuration
            });
        }

        private void UpdateDashState()
        {
            if (currentDashState != DashState.Dashing) return;

            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }

        private void EndDash()
        {
            currentDashState = DashState.Cooldown;
            cooldownTimer = cooldownDuration;

            // Restore gravity if it was suppressed
            if (ignoreGravityDuringDash)
            {
                RestoreGravity();
            }

            // Stop dash velocity so the player doesn't slide indefinitely
            Vector3 vel = physicsAdapter.Velocity;
            if (dimensionMode == DimensionMode.Mode2D)
            {
                // In 2D, zero out horizontal velocity but keep vertical for gravity
                vel.x = 0f;
            }
            else
            {
                // In 3D, zero out horizontal plane velocity but keep vertical
                vel.x = 0f;
                vel.z = 0f;
            }
            physicsAdapter.Velocity = vel;

            // End iframes if still active
            if (iframesActive)
            {
                EndIframes();
            }

            // Fire events
            OnDashEnd?.Invoke();
            EventBus.Publish(new DashEndEvent());

            // If cooldown is zero, immediately transition to ready
            if (cooldownDuration <= 0f)
            {
                TransitionToReady();
            }
        }

        private void UpdateCooldown()
        {
            if (currentDashState != DashState.Cooldown) return;

            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                TransitionToReady();
            }
        }

        private void TransitionToReady()
        {
            currentDashState = DashState.Ready;

            // Refill charges when cooldown completes
            if (maxDashCount != UNLIMITED_DASHES)
            {
                int previous = dashesRemaining;
                dashesRemaining = maxDashCount;

                if (dashesRemaining != previous)
                {
                    OnDashCountChanged?.Invoke(dashesRemaining);
                    EventBus.Publish(new DashCountChangedEvent { remaining = dashesRemaining });
                }
            }

            // Fire ready event
            OnDashReady?.Invoke();
            EventBus.Publish(new DashReadyEvent());
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Private methods — Physics Logic
        // ═══════════════════════════════════════════════

        #region Physics Logic

        private void InitializePhysicsAdapter()
        {
            // First try to find an existing adapter on this GameObject
            // Using GetComponent<IPhysicsAdapter>() only here in Awake — never in Update
            physicsAdapter = GetComponent<IPhysicsAdapter>();
            if (physicsAdapter != null) return;

            // Auto-add the correct adapter based on dimension mode
            switch (dimensionMode)
            {
                case DimensionMode.Mode2D:
                    physicsAdapter = gameObject.AddComponent<Physics2DAdapter_UMFOSS>();
                    break;
                case DimensionMode.Mode3D:
                    physicsAdapter = gameObject.AddComponent<Physics3DAdapter_UMFOSS>();
                    break;
            }

            Debug.Log($"[DashSystem] Auto-added {dimensionMode} physics adapter to '{gameObject.name}'.");
        }

        private void ApplyDashVelocity()
        {
            if (maintainDashVelocity)
            {
                // Constant velocity: override every physics frame to ensure
                // the player moves at exactly dashSpeed regardless of collisions or drag
                physicsAdapter.Velocity = currentDashDirection * dashSpeed;
            }
            else
            {
                // Ease-out: lerp the speed toward zero over the dash duration.
                // Uses the ratio of remaining time to total duration as the lerp factor.
                float progress = 1f - (dashTimer / Mathf.Max(dashDuration, MIN_DASH_DURATION));
                float easedSpeed = Mathf.Lerp(initialDashSpeed, 0f, progress);
                physicsAdapter.Velocity = currentDashDirection * easedSpeed;
            }
        }

        private Vector3 ResolveDashDirection()
        {
            switch (dashDirectionMode)
            {
                case DashDirectionMode.LastMoveDirection:
                    return lastMoveDirection.normalized;

                case DashDirectionMode.FacingDirection:
                    return GetFacingDirection();

                default:
                    return lastMoveDirection.normalized;
            }
        }

        private Vector3 GetFacingDirection()
        {
            // In 2D: facing is determined by the local X scale sign (sprite flip convention)
            // In 3D: facing is transform.forward
            if (dimensionMode == DimensionMode.Mode2D)
            {
                float sign = Mathf.Sign(transform.localScale.x);
                return new Vector3(sign, 0f, 0f);
            }
            else
            {
                return transform.forward;
            }
        }

        private void OrientPlayer(Vector3 direction)
        {
            // Stabilize the player's rotation so they face the dash direction
            // without spinning wildly. This is critical for a polished feel.
            if (dimensionMode == DimensionMode.Mode2D)
            {
                // In 2D: flip the sprite using local scale X
                // This is the standard 2D convention — no actual rotation applied
                if (direction.x != 0f)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
                    transform.localScale = scale;
                }
            }
            else
            {
                // In 3D: smoothly rotate to face the dash direction
                // Using LookRotation with immediate snap during dash for responsiveness
                if (direction.sqrMagnitude > DIRECTION_DEADZONE * DIRECTION_DEADZONE)
                {
                    // Only consider horizontal direction for rotation (ignore Y)
                    Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);
                    if (flatDirection.sqrMagnitude > DIRECTION_DEADZONE * DIRECTION_DEADZONE)
                    {
                        Quaternion targetRotation = Quaternion.LookRotation(flatDirection, Vector3.up);
                        transform.rotation = targetRotation;
                    }
                }
            }
        }

        private void RestoreGravity()
        {
            physicsAdapter.GravityScale = savedGravityScale > 0f
                ? savedGravityScale
                : DEFAULT_GRAVITY_SCALE;
        }

        private void CheckGround()
        {
            wasGrounded = isGrounded;
            Vector3 checkOrigin = transform.position + groundCheckOffset;
            isGrounded = physicsAdapter.CheckGrounded(checkOrigin, groundCheckDistance, groundLayer);

            // Reset dash charges on landing
            if (isGrounded && !wasGrounded && resetCountOnGround)
            {
                if (currentDashState != DashState.Dashing)
                {
                    ResetDashCharges();

                    // Also reset cooldown on landing if we were in cooldown
                    if (currentDashState == DashState.Cooldown)
                    {
                        TransitionToReady();
                    }
                }
            }
        }

        private void CheckDashKillCollision()
        {
            // Use the adapter's physics dimension to check for enemy collisions
            // along the dash path. A short raycast in the dash direction detects enemies.
            float checkDistance = dashSpeed * Time.fixedDeltaTime;
            Vector3 origin = transform.position;

            if (dimensionMode == DimensionMode.Mode2D)
            {
                RaycastHit2D hit = Physics2D.Raycast(origin, currentDashDirection, checkDistance, enemyLayer);
                if (hit.collider != null)
                {
                    EventBus.Publish(new DashKillEvent { target = hit.collider.gameObject });
                }
            }
            else
            {
                if (UnityEngine.Physics.Raycast(origin, currentDashDirection, out RaycastHit hit, checkDistance, enemyLayer))
                {
                    EventBus.Publish(new DashKillEvent { target = hit.collider.gameObject });
                }
            }
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Private methods — Iframe Logic
        // ═══════════════════════════════════════════════

        #region Iframe Logic

        private void StartIframes()
        {
            // Clamp iframe duration to dash duration — iframes can't outlast the dash
            iframeTimer = Mathf.Min(iframeDuration, dashDuration);
            iframesActive = true;

            EventBus.Publish(new DashIframeStartEvent());
        }

        private void UpdateIframes()
        {
            if (!iframesActive) return;

            iframeTimer -= Time.deltaTime;

            if (iframeTimer <= 0f)
            {
                EndIframes();
            }
        }

        private void EndIframes()
        {
            iframesActive = false;
            EventBus.Publish(new DashIframeEndEvent());
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Private methods — EventBus Handlers
        // ═══════════════════════════════════════════════

        #region EventBus Handlers

        private void OnPlayerKill(PlayerKillEvent evt)
        {
            // Reset cooldown immediately when the player kills an enemy
            // This rewards aggressive play with more dashes
            if (currentDashState == DashState.Cooldown)
            {
                TransitionToReady();
            }
        }

        #endregion

        // ═══════════════════════════════════════════════
        // Editor
        // ═══════════════════════════════════════════════

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Visualize ground check ray in the Scene view
            Vector3 origin = transform.position + groundCheckOffset;
            bool grounded = Application.isPlaying && isGrounded;

            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
            Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.05f);

            // Visualize last move direction
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawRay(transform.position, lastMoveDirection * 2f);
            }
        }

        private void OnValidate()
        {
            // Clamp Inspector values to prevent invalid configurations
            if (dashDistance < 0f) dashDistance = 0f;
            if (dashDuration < MIN_DASH_DURATION) dashDuration = MIN_DASH_DURATION;
            if (cooldownDuration < 0f) cooldownDuration = 0f;
            if (iframeDuration < 0f) iframeDuration = 0f;
            if (groundCheckDistance < 0f) groundCheckDistance = 0f;
            if (maxDashCount < UNLIMITED_DASHES) maxDashCount = UNLIMITED_DASHES;
        }
#endif
    }
}
