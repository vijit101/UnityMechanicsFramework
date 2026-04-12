using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// Abstracts physics operations across 2D and 3D.
    /// Implement this interface on a MonoBehaviour that wraps either Rigidbody2D or Rigidbody.
    /// The jump system (and future movement systems) talk only to this interface.
    /// </summary>
    public interface IPhysicsAdapter
    {
        /// <summary>
        /// Current velocity as Vector3. 2D adapters ignore the z component.
        /// </summary>
        Vector3 Velocity { get; set; }

        /// <summary>
        /// Sets the vertical (Y) velocity directly, preserving horizontal velocity.
        /// Used for consistent jump impulses.
        /// </summary>
        void SetVerticalVelocity(float velocity);

        /// <summary>
        /// Whether the object is currently touching the ground.
        /// Updated each frame via <see cref="UpdateGroundCheck"/>.
        /// </summary>
        bool IsGrounded { get; }

        /// <summary>
        /// Refreshes the ground check. Call once per Update frame.
        /// Uses raycast (2D) or spherecast (3D) internally.
        /// </summary>
        void UpdateGroundCheck();

        /// <summary>
        /// Gravity multiplier. Maps to Rigidbody2D.gravityScale in 2D
        /// or a manual gravity application in 3D.
        /// </summary>
        float GravityScale { get; set; }

        /// <summary>
        /// Clamps downward velocity to the specified terminal velocity.
        /// Pass 0 to skip clamping.
        /// </summary>
        void ClampVerticalVelocity(float terminalVelocity);

        /// <summary>
        /// Mass of the rigidbody.
        /// </summary>
        float Mass { get; }
    }
}
