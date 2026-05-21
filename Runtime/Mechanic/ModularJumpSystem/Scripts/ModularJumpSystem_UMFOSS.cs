using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// Jump state tracked by the system.
    /// </summary>
    public enum JumpState
    {
        Grounded,
        Rising,
        Falling
    }

    /// <summary>
    /// Physics dimension mode. Determines which physics adapter is used.
    /// </summary>
    public enum DimensionMode
    {
        Mode2D,
        Mode3D
    }

    /// <summary>
    /// Modular, configurable jump system supporting both 2D and 3D physics.
    /// Uses IPhysicsAdapter for dimension-agnostic physics operations.
    ///
    /// Features:
    /// - Multi-jump support (configurable N jumps)
    /// - Variable or consistent jump height
    /// - Separate gravity multipliers for rising and falling
    /// - Terminal velocity cap
    /// - Coyote time and jump buffering
    /// - Air control modifier (exposed for external movement systems)
    /// - Event hooks: OnJumpStart, OnJumpEnd
    /// - Unity Input System integration (with fallback public API)
    ///
    /// Setup:
    /// 1. Attach this component to your player GameObject
    /// 2. Select DimensionMode (2D or 3D) - adapter is auto-added if missing
    /// 3. Assign a Jump InputActionReference, or call OnJumpPressed/OnJumpReleased from your own input code
    /// 4. Configure ground detection layer and settings
    /// </summary>
    [DefaultExecutionOrder(-10)]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Modular Jump System")]
    public class ModularJumpSystem_UMFOSS : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Serialized / Inspector fields
        // ─────────────────────────────────────────────

        [Header("Dimension Mode")]
        [Tooltip("Select 2D or 3D physics. The appropriate adapter will be auto-added if not present.")]
        [SerializeField] private DimensionMode dimensionMode = DimensionMode.Mode2D;

        [Header("Jump Settings")]
        [Tooltip("Initial upward velocity applied on jump")]
        [SerializeField] private float jumpForce = 12f;

        [Tooltip("Maximum number of jumps before needing to land (1 = single, 2 = double, etc.)")]
        [SerializeField, Min(1)] private int maxJumps = 2;

        [Tooltip("When enabled, jump always reaches full height. When disabled, releasing early cuts the jump short (variable jump height).")]
        [SerializeField] private bool consistentJumpHeight = false;

        [Tooltip("Velocity multiplier applied when jump is released early (only when consistentJumpHeight is false). Lower = shorter minimum jump.")]
        [SerializeField, Range(0f, 1f)] private float jumpCutMultiplier = 0.5f;

        [Header("Gravity Settings")]
        [Tooltip("Apply gravity while the character is rising")]
        [SerializeField] private bool applyGravityWhileRising = true;

        [Tooltip("Apply gravity while the character is falling")]
        [SerializeField] private bool applyGravityWhileFalling = true;

        [Tooltip("Gravity multiplier while rising (higher = shorter peak)")]
        [SerializeField] private float riseGravityMultiplier = 1f;

        [Tooltip("Gravity multiplier while falling (higher = snappier fall)")]
        [SerializeField] private float fallGravityMultiplier = 2.5f;

        [Tooltip("Maximum downward speed. Set to 0 to disable cap.")]
        [SerializeField] private float terminalVelocity = 20f;

        [Header("Platformer Enhancements")]
        [Tooltip("Grace period after leaving ground where jump is still allowed (seconds)")]
        [SerializeField] private float coyoteTimeDuration = 0.15f;

        [Tooltip("Time before landing where a jump press is buffered and auto-executed on ground contact (seconds)")]
        [SerializeField] private float jumpBufferDuration = 0.1f;

        [Tooltip("Horizontal movement speed multiplier while airborne. Read by external movement systems via AirControlMultiplier property.")]
        [SerializeField, Range(0f, 1f)] private float airControlMultiplier = 0.8f;

        [Header("Ground Detection")]
        [Tooltip("Offset from transform.position for the ground check origin")]
        [SerializeField] private Vector3 groundCheckOffset = Vector3.zero;

        [Tooltip("Distance below origin to check for ground")]
        [SerializeField] private float groundCheckDistance = 0.2f;

        [Tooltip("Which layers count as ground")]
        [SerializeField] private LayerMask groundLayer = ~0;

#if ENABLE_INPUT_SYSTEM
        [Header("Input (Unity Input System)")]
        [Tooltip("Reference to the Jump action from your Input Actions asset. Leave empty if calling OnJumpPressed/OnJumpReleased manually.")]
        [SerializeField] private InputActionReference jumpInputAction;
#endif

        // ─────────────────────────────────────────────
        // Private fields
        // ─────────────────────────────────────────────

        private IPhysicsAdapter physicsAdapter;
        private int jumpsRemaining;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool isGrounded;
        private bool wasGrounded;
        private bool jumpRequested;
        private bool jumpHeld;
        private bool jumpCutApplied;
        private JumpState currentState = JumpState.Grounded;

        private const float DEFAULT_GRAVITY_SCALE = 1f;

        // ─────────────────────────────────────────────
        // Public properties
        // ─────────────────────────────────────────────

        /// <summary>Whether the character is currently on the ground.</summary>
        public bool IsGrounded => isGrounded;

        /// <summary>Current jump state (Grounded, Rising, Falling).</summary>
        public JumpState CurrentJumpState => currentState;

        /// <summary>Number of jumps remaining before needing to land.</summary>
        public int JumpsRemaining => jumpsRemaining;

        /// <summary>
        /// Movement speed multiplier based on airborne state.
        /// Returns 1.0 when grounded, airControlMultiplier when airborne.
        /// External movement systems should multiply their speed by this value.
        /// </summary>
        public float AirControlMultiplier => isGrounded ? 1f : airControlMultiplier;

        /// <summary>The active physics adapter instance.</summary>
        public IPhysicsAdapter PhysicsAdapter => physicsAdapter;

        /// <summary>The currently selected dimension mode.</summary>
        public DimensionMode CurrentDimensionMode => dimensionMode;

        // ─────────────────────────────────────────────
        // Events
        // ─────────────────────────────────────────────

        /// <summary>Fired when a jump begins (each jump in a multi-jump sequence fires separately).</summary>
        public event Action OnJumpStart;

        /// <summary>Fired when the character lands on the ground after being airborne.</summary>
        public event Action OnJumpEnd;

        // ─────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────

        private void Awake()
        {
            InitializePhysicsAdapter();
            jumpsRemaining = maxJumps;
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (jumpInputAction != null && jumpInputAction.action != null)
            {
                jumpInputAction.action.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (jumpInputAction != null && jumpInputAction.action != null)
            {
                jumpInputAction.action.Disable();
            }
#endif
        }

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

        private void FixedUpdate()
        {
            ApplyGravityMultiplier();
            ClampTerminalVelocity();
        }

        // ─────────────────────────────────────────────
        // Public methods
        // ─────────────────────────────────────────────

        /// <summary>
        /// Call from your input handler when the jump button is pressed.
        /// Use this when not using InputActionReference.
        /// </summary>
        public void OnJumpPressed()
        {
            jumpRequested = true;
            jumpBufferCounter = jumpBufferDuration;
            jumpHeld = true;
        }

        /// <summary>
        /// Call from your input handler when the jump button is released.
        /// Use this when not using InputActionReference.
        /// </summary>
        public void OnJumpReleased()
        {
            jumpHeld = false;
        }

        /// <summary>
        /// Trigger a jump programmatically, respecting jump count.
        /// Useful for bounce pads, springs, etc.
        /// </summary>
        public void ForceJump()
        {
            if (jumpsRemaining > 0 || isGrounded)
            {
                ExecuteJump();
            }
        }

        /// <summary>
        /// Trigger a jump with a custom force, bypassing jump count.
        /// Useful for launch pads, explosions, etc.
        /// </summary>
        /// <param name="customForce">Upward velocity to apply.</param>
        public void ForceJump(float customForce)
        {
            Vector3 velocity = physicsAdapter.Velocity;
            velocity.y = customForce;
            physicsAdapter.Velocity = velocity;

            jumpCutApplied = true;
            currentState = JumpState.Rising;
            OnJumpStart?.Invoke();
        }

        // ─────────────────────────────────────────────
        // Private methods
        // ─────────────────────────────────────────────

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

        private void ReadInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (jumpInputAction == null || jumpInputAction.action == null) return;

            if (jumpInputAction.action.WasPressedThisFrame())
            {
                jumpRequested = true;
                jumpBufferCounter = jumpBufferDuration;
            }

            jumpHeld = jumpInputAction.action.IsPressed();
#endif
        }

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

        private void HandleCoyoteTime()
        {
            // Start coyote time when walking off a ledge (not from a jump)
            if (wasGrounded && !isGrounded && currentState == JumpState.Grounded)
            {
                coyoteTimeCounter = coyoteTimeDuration;
            }

            if (coyoteTimeCounter > 0f)
            {
                coyoteTimeCounter -= Time.deltaTime;

                // Coyote time expired without jumping - consume the ground jump
                if (coyoteTimeCounter <= 0f && jumpsRemaining == maxJumps)
                {
                    jumpsRemaining--;
                }
            }
        }

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

        private void ExecuteJump()
        {
            // Set vertical velocity directly for predictable, mass-independent jump height
            Vector3 velocity = physicsAdapter.Velocity;
            velocity.y = jumpForce;
            physicsAdapter.Velocity = velocity;

            jumpsRemaining--;
            jumpCutApplied = false;
            currentState = JumpState.Rising;
            OnJumpStart?.Invoke();
        }

        private void HandleVariableJumpHeight()
        {
            if (consistentJumpHeight) return;
            if (currentState != JumpState.Rising) return;
            if (jumpCutApplied) return;

            // When jump button is released while rising, cut upward velocity once
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

        // ─────────────────────────────────────────────
        // Editor
        // ─────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 origin = transform.position + groundCheckOffset;
            bool grounded = Application.isPlaying && isGrounded;

            Gizmos.color = grounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
            Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, 0.05f);
        }

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
#endif
    }
}
