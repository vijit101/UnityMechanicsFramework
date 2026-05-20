using UnityEngine;
#if UMF_NEW_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// First-person player controller for the boomerang demo.
    /// WASD movement, mouse look, sprint, jump, cursor lock, and moving-platform carry.
    /// Supports both Legacy Input Manager and new Input System package.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float sprintSpeed = 10f;
        [SerializeField] private float gravity = -15f;
        [SerializeField] private float jumpForce = 6f;

        [Header("Mouse Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float maxLookAngle = 80f;
        [SerializeField] private Transform cameraHolder;

        [Header("Platform Detection")]
        [SerializeField] private float groundCheckDistance = 1.5f;

        private const float GROUNDED_PULL = -2f;
        private const float NEW_INPUT_MOUSE_SCALE = 0.1f;

        private CharacterController characterController;
        private float verticalVelocity;
        private float cameraPitch;

        // Moving platform tracking
        private Transform currentPlatform;
        private Vector3 lastPlatformPosition;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (characterController == null) return;

            HandleMouseLook();
            HandleMovement();
            HandleCursorToggle();
        }

        private void HandleMouseLook()
        {
            Vector2 mouseDelta = GetMouseDelta();
            float mouseX = mouseDelta.x * mouseSensitivity;
            float mouseY = mouseDelta.y * mouseSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -maxLookAngle, maxLookAngle);

            if (cameraHolder != null)
                cameraHolder.localEulerAngles = new Vector3(cameraPitch, 0f, 0f);

            transform.Rotate(Vector3.up, mouseX);
        }

        private void HandleMovement()
        {
            Vector2 moveInput = GetMoveInput();
            bool isSprinting = GetSprintHeld();

            float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;
            Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
            move *= currentSpeed;

            if (characterController.isGrounded)
            {
                if (verticalVelocity < 0f)
                    verticalVelocity = GROUNDED_PULL;

                if (GetJumpPressed())
                    verticalVelocity = jumpForce;
            }

            verticalVelocity += gravity * Time.deltaTime;
            move.y = verticalVelocity;

            // Apply moving platform delta before player movement
            Vector3 platformDelta = GetPlatformDelta();
            if (platformDelta.sqrMagnitude > 0f)
            {
                characterController.Move(platformDelta);
            }

            characterController.Move(move * Time.deltaTime);

            // Detect what we're standing on for next frame
            DetectPlatform();
        }

        /// <summary>
        /// Raycasts down from the player's center to find the surface they're standing on.
        /// Tracks the platform transform so we can compute its movement delta next frame.
        /// </summary>
        private void DetectPlatform()
        {
            if (!characterController.isGrounded)
            {
                currentPlatform = null;
                return;
            }

            // Cast from just above the bottom of the CharacterController
            Vector3 origin = transform.position + Vector3.up * 0.1f;
            if (UnityEngine.Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance))
            {
                if (hit.transform != currentPlatform)
                {
                    // Landed on a new surface
                    currentPlatform = hit.transform;
                    lastPlatformPosition = currentPlatform.position;
                }
            }
            else
            {
                currentPlatform = null;
            }
        }

        /// <summary>
        /// Returns the world-space movement of the platform since last frame.
        /// </summary>
        private Vector3 GetPlatformDelta()
        {
            if (currentPlatform == null) return Vector3.zero;

            Vector3 delta = currentPlatform.position - lastPlatformPosition;
            lastPlatformPosition = currentPlatform.position;
            return delta;
        }

        private void HandleCursorToggle()
        {
            if (GetEscapePressed())
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (GetLeftMousePressed() && Cursor.lockState != CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        // ───────────────────────────────────────────────
        // Input Abstraction (Legacy + New Input System)
        // ───────────────────────────────────────────────

        private Vector2 GetMouseDelta()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.delta.ReadValue() * NEW_INPUT_MOUSE_SCALE;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#endif
            return Vector2.zero;
        }

        private Vector2 GetMoveInput()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
            {
                float x = 0f;
                float y = 0f;
                if (Keyboard.current.dKey.isPressed) x += 1f;
                if (Keyboard.current.aKey.isPressed) x -= 1f;
                if (Keyboard.current.wKey.isPressed) y += 1f;
                if (Keyboard.current.sKey.isPressed) y -= 1f;
                return new Vector2(x, y);
            }
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif
            return Vector2.zero;
        }

        private bool GetSprintHeld()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.leftShiftKey.isPressed;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(KeyCode.LeftShift);
#endif
            return false;
        }

        private bool GetJumpPressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.spaceKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Space);
#endif
            return false;
        }

        private bool GetEscapePressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Keyboard.current != null)
                return Keyboard.current.escapeKey.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#endif
            return false;
        }

        private bool GetLeftMousePressed()
        {
#if UMF_NEW_INPUT_SYSTEM
            if (Mouse.current != null)
                return Mouse.current.leftButton.wasPressedThisFrame;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#endif
            return false;
        }
    }
}
