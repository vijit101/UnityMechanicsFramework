using UnityEngine;
using UnityEngine.InputSystem;
using GameplayMechanicsUMFOSS.Physics;

namespace GameplayMechanicsUMFOSS.Movement
{
    /// <summary>
    /// Simple 2D horizontal movement controller.
    /// Uses arrow keys / A-D for movement via Unity Input System.
    /// Reads AirControlMultiplier from ModularJumpSystem if present.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Movement/Simple Movement 2D")]
    public class SimpleMovement2D_UMFOSS : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = 7f;

        private Rigidbody2D rb;
        private ModularJumpSystem_UMFOSS jumpSystem;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            jumpSystem = GetComponent<ModularJumpSystem_UMFOSS>();
        }

        private void Update()
        {
            float horizontal = 0f;
            Keyboard kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) horizontal = -1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal = 1f;
            }

            float speed = moveSpeed;
            if (jumpSystem != null)
            {
                speed *= jumpSystem.AirControlMultiplier;
            }

            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);
        }
    }
}
