using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 2D physics adapter using Rigidbody2D.
    /// Ground detection uses Physics2D.Raycast.
    /// GravityScale maps directly to Rigidbody2D.gravityScale.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [AddComponentMenu("Gameplay Mechanics UMFOSS/Physics/Physics 2D Adapter")]
    public class Physics2DAdapter_UMFOSS : MonoBehaviour, IPhysicsAdapter
    {
        // Private fields
        private Rigidbody2D rb;

        // IPhysicsAdapter implementation
        public Vector3 Velocity
        {
            get => (Vector3)rb.linearVelocity;
            set => rb.linearVelocity = (Vector2)value;
        }

        public float GravityScale
        {
            get => rb.gravityScale;
            set => rb.gravityScale = value;
        }

        // Unity lifecycle
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        // Public methods
        public void AddForce(Vector3 force, bool impulse = false)
        {
            ForceMode2D mode = impulse ? ForceMode2D.Impulse : ForceMode2D.Force;
            rb.AddForce((Vector2)force, mode);
        }

        public bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer)
        {
            RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, distance, groundLayer);
            return hit.collider != null;
        }
    }
}
