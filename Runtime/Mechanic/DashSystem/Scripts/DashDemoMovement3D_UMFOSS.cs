using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// Simple 3D movement controller for the Dash System demo scene.
    /// Reads WASD/Arrow keys via the Unity Input System and moves the player
    /// on the XZ plane. Uses smooth LookAt-style rotation so the player
    /// faces the movement direction without spinning erratically.
    ///
    /// Automatically feeds movement direction into the DashSystem
    /// so that LastMoveDirection mode works correctly.
    ///
    /// This script is a demo helper — it is NOT the dash system itself.
    /// In your own project, replace this with your own movement controller
    /// and call DashSystem_UMFOSS.SetMoveDirection() from it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Dash Demo Movement 3D")]
    public class DashDemoMovement3D_UMFOSS : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Serialized / Inspector fields
        // ─────────────────────────────────────────────

        [Header("Movement Settings")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 7f;

        [Header("Rotation Settings")]
        [Tooltip("How fast the player rotates to face the movement direction (degrees per second).\n" +
                 "Higher values = snappier turns. Set very high (e.g., 1440) for instant rotation.")]
        [SerializeField] private float rotationSpeed = 720f;

#if ENABLE_INPUT_SYSTEM
        [Header("Input (Unity Input System)")]
        [Tooltip("Reference to the Move action from your Input Actions asset.\n" +
                 "Should be a Value type action with Vector2 control type.")]
        [SerializeField] private InputActionReference moveInputAction;
#endif

        // ─────────────────────────────────────────────
        // Constants
        // ─────────────────────────────────────────────

        /// <summary>Minimum input magnitude to trigger rotation (prevents jitter at rest).</summary>
        private const float ROTATION_DEADZONE = 0.1f;

        // ─────────────────────────────────────────────
        // Private fields
        // ─────────────────────────────────────────────

        private Rigidbody rb;
        private DashSystem_UMFOSS dashSystem;

        // ─────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            dashSystem = GetComponent<DashSystem_UMFOSS>();
        }

        private void OnEnable()
        {
#if ENABLE_INPUT_SYSTEM
            if (moveInputAction != null && moveInputAction.action != null)
            {
                moveInputAction.action.Enable();
            }
#endif
        }

        private void OnDisable()
        {
#if ENABLE_INPUT_SYSTEM
            if (moveInputAction != null && moveInputAction.action != null)
            {
                moveInputAction.action.Disable();
            }
#endif
        }

        private void Update()
        {
            // Don't override movement while dashing — the dash system controls velocity
            if (dashSystem != null && dashSystem.IsDashing) return;

            Vector2 input = ReadMoveInput();

            // Convert 2D input to 3D movement on the XZ plane
            // Input X = world X, Input Y = world Z (forward/backward)
            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);

            // Apply movement velocity while preserving vertical velocity (gravity/jumping)
            Vector3 velocity = moveDirection * moveSpeed;
            velocity.y = rb.linearVelocity.y;
            rb.linearVelocity = velocity;

            // Feed movement direction to the dash system
            if (dashSystem != null && moveDirection.sqrMagnitude > ROTATION_DEADZONE * ROTATION_DEADZONE)
            {
                dashSystem.SetMoveDirection(moveDirection);
            }

            // Rotate player to face movement direction using smooth LookAt
            // This prevents the player from spinning wildly or snapping unnaturally
            RotateTowardMovement(moveDirection);
        }

        // ─────────────────────────────────────────────
        // Private methods
        // ─────────────────────────────────────────────

        private void RotateTowardMovement(Vector3 moveDirection)
        {
            // Only rotate when there is meaningful input
            if (moveDirection.sqrMagnitude < ROTATION_DEADZONE * ROTATION_DEADZONE) return;

            // Calculate the target rotation from the movement direction
            // LookRotation creates a rotation that faces the given forward direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);

            // Smoothly interpolate toward the target rotation
            // RotateTowards gives us frame-rate-independent, speed-capped rotation
            // This is what prevents erratic spinning — the player turns at a controlled rate
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        private Vector2 ReadMoveInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (moveInputAction != null && moveInputAction.action != null)
            {
                return moveInputAction.action.ReadValue<Vector2>();
            }

            // Fallback: read keyboard directly via the Input System API
            Keyboard kb = Keyboard.current;
            if (kb == null) return Vector2.zero;

            float horizontal = 0f;
            float vertical = 0f;

            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) vertical = 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) vertical = -1f;

            Vector2 raw = new Vector2(horizontal, vertical);
            // Normalize diagonal input so the player doesn't move faster diagonally
            return raw.sqrMagnitude > 1f ? raw.normalized : raw;
#else
            return Vector2.zero;
#endif
        }
    }
}
