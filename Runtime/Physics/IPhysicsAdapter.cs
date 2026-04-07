using UnityEngine;

namespace GameplayMechanicsUMFOSS.Physics
{
    /// <summary>
    /// Abstraction over Unity physics. Swap the adapter component to switch
    /// between 2D and 3D without changing mechanic code.
    /// </summary>
    public interface IPhysicsAdapter
    {
        Vector3 Velocity { get; set; }
        bool IsKinematic { get; set; }

        void AddForce(Vector3 force, bool isImpulse = false);
        void SetPosition(Vector3 position);

        /// <summary>Preferred over SetPosition for kinematic bodies (smooth interpolation).</summary>
        void MovePosition(Vector3 position);

        /// <summary>Clears forces/torques and resets velocity to zero.</summary>
        void ClearForces();
    }
}
