namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Interface for any object that can receive damage.
    /// Implement on enemies, destructibles, or any hittable target.
    /// The boomerang weapon (and any future combat mechanic) uses this
    /// instead of SendMessage for type-safe, compile-time-checked damage delivery.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>Current health remaining.</summary>
        float CurrentHealth { get; }

        /// <summary>Whether this target is still alive.</summary>
        bool IsAlive { get; }

        /// <summary>
        /// Applies damage to this target. Returns the actual damage dealt
        /// (may differ from DamageData.Amount due to armor, shields, etc.).
        /// </summary>
        float TakeDamage(DamageData damageData);
    }
}
