// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// 2D physics adapter implementation using Rigidbody2D.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Physics2DAdapter : MonoBehaviour, IPhysicsAdapter
    {
        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Applies force using the default 2D force mode.
        /// </summary>
        /// <param name="force">Force vector to apply.</param>
        public void AddForce(Vector2 force)
        {
            rb.AddForce(force);
        }

        /// <summary>
        /// Applies force using the provided 2D force mode.
        /// </summary>
        /// <param name="force">Force vector to apply.</param>
        /// <param name="mode">Force application mode.</param>
        public void AddForce(Vector2 force, ForceMode2D mode)
        {
            rb.AddForce(force, mode);
        }

        /// <summary>
        /// Sets the current 2D linear velocity.
        /// </summary>
        /// <param name="velocity">Velocity to assign.</param>
        public void SetVelocity(Vector2 velocity)
        {
            rb.linearVelocity = velocity;
        }

        /// <summary>
        /// Gets the current 2D linear velocity.
        /// </summary>
        /// <returns>The current velocity vector.</returns>
        public Vector2 GetVelocity()
        {
            return rb.linearVelocity;
        }

        /// <summary>
        /// Sets Rigidbody2D gravity scale.
        /// </summary>
        /// <param name="scale">Desired gravity scale.</param>
        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }

        /// <summary>
        /// Gets Rigidbody2D gravity scale.
        /// </summary>
        /// <returns>The current gravity scale.</returns>
        public float GetGravityScale()
        {
            return rb.gravityScale;
        }

        /// <summary>
        /// Checks grounded state using an overlap circle query.
        /// </summary>
        /// <param name="checkOrigin">World-space origin for the query.</param>
        /// <param name="checkRadius">Radius of the grounded query.</param>
        /// <param name="groundLayer">Layers considered as ground.</param>
        /// <returns>True when a ground collider is found.</returns>
        public bool IsGrounded(Vector2 checkOrigin, float checkRadius, LayerMask groundLayer)
        {
            return Physics2D.OverlapCircle(checkOrigin, checkRadius, groundLayer) != null;
        }
    }
}
