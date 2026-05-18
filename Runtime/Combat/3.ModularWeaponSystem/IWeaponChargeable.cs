// Author: Aditya Jaiswal, Atharv S. Jain

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Capability contract for any weapon that builds up a charge over time
    /// before releasing its effect.
    /// </summary>
    public interface IWeaponChargeable
    {
        /// <summary>
        /// Begins building charge.
        /// </summary>
        void StartCharge();

        /// <summary>
        /// Releases the accumulated charge and resets state.
        /// </summary>
        void ReleaseCharge();

        /// <summary>
        /// Returns the current charge as a normalized 0-1 value.
        /// </summary>
        float GetChargePercent();
    }
}
