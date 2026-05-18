// Author: Aditya Jaiswal, Atharv S. Jain
using System.Collections;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Chargeable weapon that ramps damage between a min and max multiplier
    /// based on how long the player holds the trigger. Demonstrates that the
    /// same IWeaponFirable interface can drive completely different mechanics.
    /// </summary>
    public class UnorthodoxWeapon_UMFOSS : WeaponBase_UMFOSS, IWeaponFirable, IWeaponChargeable
    {
        [Header("Charge Settings")]
        [SerializeField] private float maxChargeTime = 3f;
        [SerializeField] private float minDamageMultiplier = 0.5f;
        [SerializeField] private float maxDamageMultiplier = 3f;

        [Header("Plasma Shot")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private float currentChargeTime;
        private bool isCharging;
        private Coroutine chargeCoroutine;

        /// <summary>
        /// Begins charging if not already charging.
        /// </summary>
        public void StartCharge()
        {
            if (isCharging)
            {
                return;
            }

            isCharging = true;
            chargeCoroutine = StartCoroutine(ChargeCoroutine());
        }

        /// <summary>
        /// Releases the held charge, computing the final damage from the current
        /// charge percent, raising the fired event, and resetting state.
        /// </summary>
        public void ReleaseCharge()
        {
            if (!isCharging)
            {
                return;
            }

            StopCoroutine(chargeCoroutine);

            float finalDamage = Mathf.Lerp(
                Damage * minDamageMultiplier,
                Damage * maxDamageMultiplier,
                GetChargePercent());

            Debug.Log($"[{WeaponName}] Released charged shot — final damage: {finalDamage}");

            if (projectilePrefab != null && firePoint != null)
            {
                GameObject spawned = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                if (spawned.TryGetComponent(out Projectile2D_UMFOSS projectile))
                {
                    projectile.Initialize(finalDamage);
                }
            }

            WeaponEventBus.RaiseWeaponFired(GetData());

            currentChargeTime = 0f;
            isCharging = false;
            WeaponEventBus.RaiseChargeChanged(0f);
        }

        /// <summary> Returns the current charge as a normalized 0-1 value. </summary>
        public float GetChargePercent()
        {
            return Mathf.Clamp01(currentChargeTime / maxChargeTime);
        }

        /// <summary>
        /// Routes to StartCharge(). This is polymorphism in action — the
        /// player controller calls Fire() on any IWeaponFirable without
        /// knowing whether it fires a bullet, swings a sword, or begins
        /// charging. The weapon decides what Fire() means.
        /// </summary>
        public void Fire()
        {
            StartCharge();
        }

        /// <summary> Routes to ReleaseCharge(). </summary>
        public void StopFire()
        {
            ReleaseCharge();
        }

        /// <summary> Cannot fire while a charge is already in progress. </summary>
        public bool CanFire()
        {
            return !isCharging;
        }

        /// <summary> Builds the weapon data snapshot from base fields. </summary>
        public override WeaponData GetData()
        {
            return BuildWeaponData();
        }

        /// <summary>
        /// Seeds listeners (charge bar, EventLogger) with an initial 0% charge
        /// so they have a baseline before the player first pulls the trigger.
        /// base.OnEquip() must remain the LAST call.
        /// </summary>
        public override void OnEquip()
        {
            WeaponEventBus.RaiseChargeChanged(0f);
            base.OnEquip();
        }

        /// <summary>
        /// Resets charge state on unequip so a half-held charge does not survive
        /// across weapon swaps. base.OnUnequip() must remain the LAST call.
        /// </summary>
        public override void OnUnequip()
        {
            isCharging = false;
            currentChargeTime = 0f;
            base.OnUnequip();
        }

        private IEnumerator ChargeCoroutine()
        {
            while (true)
            {
                currentChargeTime += Time.deltaTime;
                currentChargeTime = Mathf.Min(currentChargeTime, maxChargeTime);
                WeaponEventBus.RaiseChargeChanged(GetChargePercent());
                yield return null;
            }
        }
    }
}
