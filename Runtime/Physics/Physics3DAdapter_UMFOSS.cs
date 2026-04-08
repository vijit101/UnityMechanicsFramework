using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 3D physics adapter. Wraps Rigidbody and uses SphereCast for ground detection.
    /// Disables built-in gravity and applies it manually so GravityScale works like Rigidbody2D.
    /// Attach to the same GameObject as a Rigidbody.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [DefaultExecutionOrder(-10)]
    public class Physics3DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        // ── Serialized Fields ──

        [Header("Ground Detection")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private Vector3 groundCheckOffset = Vector3.zero;
        [SerializeField] private float groundCheckRadius = 0.3f;

        // ── Private Fields ──

        private Rigidbody rb;
        private bool isGrounded;
        private float gravityScale = 1f;

        // ── IPhysicsAdapter Properties ──

        public Vector3 Velocity
        {
            get => rb.linearVelocity;
            set => rb.linearVelocity = value;
        }

        public bool IsGrounded => isGrounded;

        public float GravityScale
        {
            get => gravityScale;
            set => gravityScale = value;
        }

        public float Mass => rb.mass;

        // ── Unity Lifecycle ──

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;
        }

        private void FixedUpdate()
        {
            // Apply gravity manually so GravityScale works identically to Rigidbody2D.gravityScale
            rb.AddForce(UnityEngine.Physics.gravity * (gravityScale * rb.mass));
        }

        // ── IPhysicsAdapter Methods ──

        public void SetVerticalVelocity(float velocity)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, velocity, rb.linearVelocity.z);
        }

        public void UpdateGroundCheck()
        {
            // Use a short raycast downward from just inside the collider bottom
            // This is precise and doesn't overlap platforms after jumping
            Vector3 origin = transform.position + groundCheckOffset;
            isGrounded = UnityEngine.Physics.Raycast(
                origin,
                Vector3.down,
                groundCheckDistance,
                groundLayer
            );
        }

        public void ClampVerticalVelocity(float terminalVelocity)
        {
            if (rb.linearVelocity.y < -terminalVelocity)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, -terminalVelocity, rb.linearVelocity.z);
            }
        }
    }
}
