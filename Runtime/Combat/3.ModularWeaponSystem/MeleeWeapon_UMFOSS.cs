// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Instantaneous melee weapon. "Firing" activates a hitbox for a swing
    /// duration, gated by an attack cooldown.
    /// </summary>
    public class MeleeWeapon_UMFOSS : WeaponBase_UMFOSS, IWeaponFirable
    {
        [Header("Melee Settings")]
        [SerializeField] private float attackCooldown = DEFAULT_ATTACK_COOLDOWN;
        [SerializeField] private float swingDuration = 0.2f;
        [SerializeField] private HitboxSystem_UMFOSS hitbox;

        private const float DEFAULT_ATTACK_COOLDOWN = 0.5f;

        private float lastAttackTime;

        /// <summary> Returns true when the attack cooldown has elapsed. </summary>
        public bool CanFire()
        {
            return Time.time >= lastAttackTime + attackCooldown;
        }

        /// <summary>
        /// Activates the hitbox with this weapon's damage for the swing duration
        /// and raises a fired event.
        /// </summary>
        public void Fire()
        {
            if (!CanFire())
            {
                return;
            }

            hitbox.Activate(Damage, swingDuration);
            lastAttackTime = Time.time;
            WeaponEventBus.RaiseWeaponFired(GetData());
        }

        /// <summary>
        /// Melee attacks are instantaneous — there is no held state to release.
        /// StopFire exists to satisfy IWeaponFirable. Any weapon that implements
        /// the interface must provide this method, even when it has no meaning
        /// for that weapon type. This is the interface contract: the caller does
        /// not need to know whether StopFire does anything.
        /// </summary>
        public void StopFire()
        {
        }

        /// <summary> Builds the weapon data snapshot from base fields. </summary>
        public override WeaponData GetData()
        {
            return BuildWeaponData();
        }

        /// <summary> Equip hook. base.OnEquip() must remain the LAST call. </summary>
        public override void OnEquip()
        {
            base.OnEquip();
        }

        /// <summary> Unequip hook. base.OnUnequip() must remain the LAST call. </summary>
        public override void OnUnequip()
        {
            base.OnUnequip();
        }
    }
}
