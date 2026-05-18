using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// Physics-agnostic interface for mechanics that need Rigidbody operations.
    /// Swap Physics2DAdapter for Physics3DAdapter on your GameObject and the
    /// mechanic works in both dimensions without any code changes.
    /// </summary>
    public interface IPhysicsAdapter
    {
        /// <summary>Current linear velocity.</summary>
        Vector3 Velocity { get; set; }

        /// <summary>Whether the body is kinematic (ignores physics forces).</summary>
        bool IsKinematic { get; set; }

        /// <summary>Applies a force to the body using the specified mode.</summary>
        void AddForce(Vector3 force, ForceMode mode = ForceMode.Force);

        /// <summary>
        /// Safely transitions kinematic state by zeroing velocity first,
        /// preventing stored forces from causing artifacts on mode change.
        /// </summary>
        void SetKinematic(bool kinematic);
    }
}
