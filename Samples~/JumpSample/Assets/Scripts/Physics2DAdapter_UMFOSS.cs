using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 2D physics adapter. Wraps Rigidbody2D and uses CircleCast for ground detection.
    /// Attach to the same GameObject as a Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [DefaultExecutionOrder(-10)]
    public class Physics2DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        // ── Serialized Fields ──

        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private Vector2 groundCheckOffset = Vector2.zero;
        [SerializeField] private float groundCheckRadius = 0.3f;

        // ── Private Fields ──

        private Rigidbody2D rb;
        private bool isGrounded;

        // ── IPhysicsAdapter Properties ──

        public Vector3 Velocity
        {
            get => new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 0f);
            set => rb.linearVelocity = new Vector2(value.x, value.y);
        }

        public bool IsGrounded => isGrounded;

        public float GravityScale
        {
            get => rb.gravityScale;
            set => rb.gravityScale = value;
        }

        public float Mass => rb.mass;

        // ── Unity Lifecycle ──

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // ── IPhysicsAdapter Methods ──

        public void SetVerticalVelocity(float velocity)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, velocity);
        }

        public void UpdateGroundCheck()
        {
            Vector2 origin = (Vector2)transform.position + groundCheckOffset;
            RaycastHit2D hit = Physics2D.CircleCast(
                origin,
                groundCheckRadius,
                Vector2.down,
                groundCheckDistance,
                groundLayer
            );
            isGrounded = hit.collider != null;
        }

        public void ClampVerticalVelocity(float terminalVelocity)
        {
            if (rb.linearVelocity.y < -terminalVelocity)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -terminalVelocity);
            }
        }
    }
}
