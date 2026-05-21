using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Handles the weapon's visual spin during flight.
    /// Rotates a child pivot transform so the parent's forward vector stays clean
    /// for movement direction calculations.
    /// </summary>
    public class WeaponVisualHandler
    {
        private readonly Transform visualPivot;
        private readonly Vector3 degreesPerSecond;

        public WeaponVisualHandler(Transform visualPivot, Vector3 degreesPerSecond)
        {
            this.visualPivot = visualPivot;
            this.degreesPerSecond = degreesPerSecond;
        }

        /// <summary>Rotates the visual pivot around each local axis per frame.</summary>
        public void Spin()
        {
            if (visualPivot == null) return;
            visualPivot.Rotate(degreesPerSecond * Time.deltaTime, Space.Self);
        }

        /// <summary>Resets the visual pivot rotation to identity (stops spin).</summary>
        public void ResetRotation()
        {
            if (visualPivot == null) return;
            visualPivot.localRotation = Quaternion.identity;
        }
    }
}
