// Author: Aditya Jaiswal, Atharv S. Jain

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Capability contract for any weapon that maintains an ammo magazine and
    /// reserve, and can be reloaded.
    /// </summary>
    public interface IWeaponReloadable
    {
        /// <summary>
        /// Begins a reload sequence if <see cref="CanReload"/> is true.
        /// </summary>
        void Reload();

        /// <summary>
        /// Returns true when the weapon is currently allowed to begin a reload.
        /// </summary>
        bool CanReload();

        /// <summary>
        /// Returns the rounds currently loaded in the magazine.
        /// </summary>
        int GetCurrentAmmo();

        /// <summary>
        /// Returns the rounds currently held in reserve.
        /// </summary>
        int GetReserveAmmo();
    }
}
