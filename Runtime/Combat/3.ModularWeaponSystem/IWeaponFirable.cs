// Author: Aditya Jaiswal, Atharv S. Jain

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Capability contract for any weapon that can be "fired". The interpretation
    /// of fire is left to the implementer — bullet, swing, charge, or anything else.
    /// </summary>
    public interface IWeaponFirable
    {
        /// <summary>
        /// Begins or executes the weapon's primary use action.
        /// </summary>
        void Fire();

        /// <summary>
        /// Stops or releases the weapon's primary use action. May be a no-op for
        /// instantaneous weapons.
        /// </summary>
        void StopFire();

        /// <summary>
        /// Returns true when the weapon is currently allowed to fire.
        /// </summary>
        bool CanFire();
    }
}
