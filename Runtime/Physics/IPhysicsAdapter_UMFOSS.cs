using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// Abstraction layer for physics operations. Enables mechanics to work
    /// with both 2D and 3D physics without direct Rigidbody references.
    /// Implemented by Physics2DAdapter_UMFOSS and Physics3DAdapter_UMFOSS.
    /// </summary>
    public interface IPhysicsAdapter
    {
        /// <summary>Current linear velocity of the physics body.</summary>
        Vector3 Velocity { get; set; }

        /// <summary>
        /// Apply a force to the physics body.
        /// </summary>
        /// <param name="force">Force vector to apply.</param>
        /// <param name="impulse">If true, applies as an instant impulse; otherwise continuous force.</param>
        void AddForce(Vector3 force, bool impulse = false);

        /// <summary>
        /// Check if the object is grounded using dimension-appropriate detection.
        /// </summary>
        /// <param name="origin">World-space origin point for the ground check.</param>
        /// <param name="distance">How far below origin to check.</param>
        /// <param name="groundLayer">Layer mask for ground surfaces.</param>
        /// <returns>True if ground is detected within distance.</returns>
        bool CheckGrounded(Vector3 origin, float distance, LayerMask groundLayer);

        /// <summary>
        /// Gravity scale multiplier applied to the physics body.
        /// For 2D, this maps directly to Rigidbody2D.gravityScale.
        /// For 3D, gravity is applied manually in FixedUpdate using this scale.
        /// </summary>
        float GravityScale { get; set; }
    }
}
