using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// Simple 2D movement controller for the Dash System demo scene.
    /// Reads WASD/Arrow keys via the Unity Input System and moves the player
    /// horizontally. Automatically feeds movement direction into the DashSystem
    /// so that LastMoveDirection mode works correctly.
    ///
    /// This script is a demo helper — it is NOT the dash system itself.
    /// In your own project, replace this with your own movement controller
    /// and call DashSystem_UMFOSS.SetMoveDirection() from it.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Dash Demo Movement 2D")]
    public class DashDemoMovement2D_UMFOSS : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        // Serialized / Inspector fields
        // ─────────────────────────────────────────────

        [Header("Movement Settings")]
        [Tooltip("Horizontal movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 7f;

#if ENABLE_INPUT_SYSTEM
        [Header("Input (Unity Input System)")]
        [Tooltip("Reference to the Move action from your Input Actions asset.\n" +
                 "Should be a Value type action with Vector2 control type.")]
        [SerializeField] private InputActionReference moveInputAction;
#endif

        // ─────────────────────────────────────────────
        // Private fields
        // ─────────────────────────────────────────────

        private Rigidbody2D rb;
        private DashSystem_UMFOSS dashSystem;

        // ─────────────────────────────────────────────
        // Unity lifecycle
        // ─────────────────────────────────────────────

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
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

            // Apply horizontal movement while preserving vertical velocity (gravity)
            rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);

            // Feed movement direction to the dash system so it knows
            // which way the player was last moving (for LastMoveDirection mode)
            if (dashSystem != null && input.sqrMagnitude > 0.01f)
            {
                // In 2D, we only care about horizontal direction
                dashSystem.SetMoveDirection(new Vector3(input.x, 0f, 0f));
            }

            // Flip sprite based on movement direction
            // This is the standard 2D convention for facing direction
            if (input.x != 0f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * Mathf.Sign(input.x);
                transform.localScale = scale;
            }
        }

        // ─────────────────────────────────────────────
        // Private methods
        // ─────────────────────────────────────────────

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
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;

            return new Vector2(horizontal, 0f);
#else
            return Vector2.zero;
#endif
        }
    }
}
