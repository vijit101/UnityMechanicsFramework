using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Data payload describing a single instance of damage.
    /// Passed to IDamageable.TakeDamage so receivers know how much damage,
    /// where it came from, and how hard to knock back.
    /// </summary>
    public struct DamageData
    {
        /// <summary>Raw damage amount before any modifiers.</summary>
        public float Amount;

        /// <summary>World position where the hit occurred.</summary>
        public Vector3 HitPoint;

        /// <summary>Direction the knockback force should be applied.</summary>
        public Vector3 KnockbackDirection;

        /// <summary>Magnitude of the knockback impulse.</summary>
        public float KnockbackForce;

        /// <summary>The GameObject that dealt the damage (weapon, projectile, etc.).</summary>
        public GameObject Source;
    }
}
