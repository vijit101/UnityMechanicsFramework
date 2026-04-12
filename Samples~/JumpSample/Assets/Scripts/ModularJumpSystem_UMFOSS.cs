using System;
using UnityEngine;
using UnityEngine.InputSystem;
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// A modular, configurable jump system supporting both 2D and 3D physics.
    /// Communicates with physics through <see cref="IPhysicsAdapter"/>, never directly with Rigidbody.
    /// Features: multi-jump, coyote time, jump buffering, gravity modifiers, terminal velocity.
    /// Attach alongside a Physics2DAdapter or Physics3DAdapter.
    /// </summary>
    public class ModularJumpSystem : MonoBehaviour
    {
        // ── Serialized Fields ──

        [Header("Dimension")]
        [SerializeField] private DimensionMode dimensionMode = DimensionMode.Mode2D;

        [Header("Jump Settings")]
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float jumpForce = 12f;
        [SerializeField]
        [Tooltip("If true, sets velocity directly for consistent jump height. If false, adds to current velocity for variable height (release early to jump lower).")]
        private bool consistentJumpHeight = true;

        [Header("Gravity Modifiers")]
        [SerializeField] private float fallingGravityMultiplier = 2.5f;
        [SerializeField] private float risingGravityMultiplier = 1f;
        [SerializeField] private bool applyGravityWhileFalling = true;
        [SerializeField] private bool applyGravityWhileRising = true;
        [SerializeField]
        [Tooltip("Maximum downward speed. Set to 0 to disable.")]
        private float terminalVelocity = 20f;

        [Header("Platformer Enhancements")]
        [SerializeField] private float coyoteTimeDuration = 0.12f;
        [SerializeField] private float jumpBufferDuration = 0.1f;
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Multiplier applied to horizontal input while airborne. A movement controller can read this via AirControlModifier.")]
        private float airControlModifier = 0.8f;

        [Header("Input")]
        [SerializeField] private InputActionReference jumpInputAction;

        // ── Private Fields ──

        private IPhysicsAdapter physicsAdapter;
        private int jumpsRemaining;
        private JumpState currentState = JumpState.Grounded;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private bool wasGroundedLastFrame;
        private float defaultGravityScale;

        // ── Events ──

        /// <summary>Fired the frame a jump is executed.</summary>
        public event Action OnJumpStart;

        /// <summary>Fired the frame the character lands after being airborne.</summary>
        public event Action OnJumpEnd;

        // ── Properties ──

        /// <summary>Current jump state (Grounded, Rising, or Falling).</summary>
        public JumpState CurrentState => currentState;

        /// <summary>How many jumps the character can still perform before landing.</summary>
        public int JumpsRemaining => jumpsRemaining;

        /// <summary>Whether the character is on the ground (delegates to the physics adapter).</summary>
        public bool IsGrounded => physicsAdapter != null && physicsAdapter.IsGrounded;

        /// <summary>
        /// Horizontal input multiplier while airborne. Movement controllers should
        /// read this value and scale their horizontal input accordingly.
        /// </summary>
        public float AirControlModifier => airControlModifier;

        // ── Unity Lifecycle ──

        private void Start()
        {
            ResolveAdapter();
            if (physicsAdapter != null)
            {
                defaultGravityScale = physicsAdapter.GravityScale;
                jumpsRemaining = maxJumps;
            }
        }

        private void OnEnable()
        {
            if (jumpInputAction != null && jumpInputAction.action != null)
            {
                jumpInputAction.action.Enable();
                jumpInputAction.action.performed += OnJumpInputPerformed;
                jumpInputAction.action.canceled += OnJumpInputCanceled;
            }
        }

        private void OnDisable()
        {
            if (jumpInputAction != null && jumpInputAction.action != null)
            {
                jumpInputAction.action.performed -= OnJumpInputPerformed;
                jumpInputAction.action.canceled -= OnJumpInputCanceled;
                jumpInputAction.action.Disable();
            }
        }

        private void Update()
        {
            if (physicsAdapter == null) return;

            physicsAdapter.UpdateGroundCheck();
            UpdateTimers();
            ProcessBufferedJump();
            UpdateState();
        }

        private void FixedUpdate()
        {
            if (physicsAdapter == null) return;

            ApplyGravityModifiers();
            ApplyTerminalVelocity();
        }

        // ── Public Methods ──

        /// <summary>
        /// Triggers a jump attempt programmatically, bypassing input.
        /// Useful for AI controllers or scripted sequences.
        /// </summary>
        public void TriggerJump()
        {
            jumpBufferCounter = jumpBufferDuration;
            TryJump();
        }

        // ── Input Handlers ──

        private void OnJumpInputPerformed(InputAction.CallbackContext ctx)
        {
            jumpBufferCounter = jumpBufferDuration;
            TryJump();
        }

        private void OnJumpInputCanceled(InputAction.CallbackContext ctx)
        {
            // Variable jump height: cut upward velocity short when button released while rising
            if (!consistentJumpHeight && currentState == JumpState.Rising)
            {
                float currentVel = physicsAdapter.Velocity.y;
                if (currentVel > 0f)
                {
                    physicsAdapter.SetVerticalVelocity(currentVel * 0.5f);
                }
            }
        }

        // ── Core Logic ──

        private void TryJump()
        {
            // Coyote jump: allowed only if player hasn't jumped yet (walked off edge)
            bool canCoyoteJump = coyoteTimeCounter > 0f && jumpsRemaining == maxJumps;

            if (jumpsRemaining > 0 || canCoyoteJump)
            {
                ExecuteJump();
            }
        }

        private void ExecuteJump()
        {
            if (consistentJumpHeight)
            {
                physicsAdapter.SetVerticalVelocity(jumpForce);
            }
            else
            {
                float currentVel = physicsAdapter.Velocity.y;
                physicsAdapter.SetVerticalVelocity(currentVel + jumpForce);
            }

            jumpsRemaining--;
            coyoteTimeCounter = 0f;
            jumpBufferCounter = 0f;
            currentState = JumpState.Rising;

            OnJumpStart?.Invoke();
        }

        private void UpdateTimers()
        {
            // Coyote time: reset while grounded, count down while airborne
            if (physicsAdapter.IsGrounded)
            {
                coyoteTimeCounter = coyoteTimeDuration;
            }
            else
            {
                coyoteTimeCounter -= Time.deltaTime;
            }

            // Jump buffer: count down after press
            if (jumpBufferCounter > 0f)
            {
                jumpBufferCounter -= Time.deltaTime;
            }
        }

        private void ProcessBufferedJump()
        {
            // If we have a buffered jump and just landed, execute it
            if (jumpBufferCounter > 0f && physicsAdapter.IsGrounded && jumpsRemaining > 0)
            {
                ExecuteJump();
            }
        }

        private void UpdateState()
        {
            bool grounded = physicsAdapter.IsGrounded;
            float verticalVelocity = physicsAdapter.Velocity.y;

            // Don't count as grounded if moving upward (just jumped but raycast still hits)
            if (grounded && verticalVelocity > 0.5f)
            {
                grounded = false;
            }

            if (grounded)
            {
                if (currentState == JumpState.Falling || currentState == JumpState.Rising)
                {
                    OnJumpEnd?.Invoke();
                }

                currentState = JumpState.Grounded;
                jumpsRemaining = maxJumps;
            }
            else if (verticalVelocity > 0.01f)
            {
                currentState = JumpState.Rising;
            }
            else
            {
                currentState = JumpState.Falling;
            }

            wasGroundedLastFrame = grounded;
        }

        private void ApplyGravityModifiers()
        {
            float targetScale = defaultGravityScale;

            if (currentState == JumpState.Falling)
            {
                targetScale = applyGravityWhileFalling
                    ? defaultGravityScale * fallingGravityMultiplier
                    : 0f;
            }
            else if (currentState == JumpState.Rising)
            {
                targetScale = applyGravityWhileRising
                    ? defaultGravityScale * risingGravityMultiplier
                    : 0f;
            }

            physicsAdapter.GravityScale = targetScale;
        }

        private void ApplyTerminalVelocity()
        {
            if (terminalVelocity > 0f)
            {
                physicsAdapter.ClampVerticalVelocity(terminalVelocity);
            }
        }

        private void ResolveAdapter()
        {
            switch (dimensionMode)
            {
                case DimensionMode.Mode2D:
                    physicsAdapter = GetComponent<Physics2DAdapter>();
                    if (physicsAdapter == null)
                        Debug.LogError($"[ModularJumpSystem] DimensionMode is 2D but no Physics2DAdapter found on {gameObject.name}. Add a Physics2DAdapter component.", this);
                    break;

                case DimensionMode.Mode3D:
                    physicsAdapter = GetComponent<Physics3DAdapter>();
                    if (physicsAdapter == null)
                        Debug.LogError($"[ModularJumpSystem] DimensionMode is 3D but no Physics3DAdapter found on {gameObject.name}. Add a Physics3DAdapter component.", this);
                    break;
            }
        }
    }
}
