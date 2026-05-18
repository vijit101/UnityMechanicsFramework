using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Utility methods for common physics operations.
    /// Centralizes Rigidbody state management to prevent physics artifacts.
    /// </summary>
    public static class PhysicsUtils
    {
        /// <summary>
        /// Safely sets a Rigidbody to kinematic mode.
        /// Always zeros velocity and angular velocity BEFORE toggling isKinematic
        /// to prevent stored forces from causing impulses on the next mode change.
        /// </summary>
        public static void SetKinematic(Rigidbody rb, bool kinematic)
        {
            if (rb == null) return;
            if (kinematic && !rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = kinematic;
        }
    }
}
