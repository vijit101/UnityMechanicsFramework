using UnityEngine;

namespace GameplayMechanicsUMFOSS.Samples.Interaction
{
    /// <summary>
    /// Simple top-down movement script for the interaction system demo.
    /// Moves the player using WASD or arrow keys via Rigidbody2D velocity.
    /// Gravity Scale on the Rigidbody2D should be set to 0 for top-down movement.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class DemoPlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("Movement speed in units per second.")]
        [SerializeField] private float moveSpeed = 5f;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical   = Input.GetAxisRaw("Vertical");

            Vector2 direction = new Vector2(horizontal, vertical).normalized;
            rb.velocity = direction * moveSpeed;
        }
    }
}