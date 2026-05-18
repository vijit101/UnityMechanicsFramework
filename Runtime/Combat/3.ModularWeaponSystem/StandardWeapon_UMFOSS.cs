// Author: Aditya Jaiswal, Atharv S. Jain
using System.Collections;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Standard projectile weapon. Fires a prefab from a fire point on a
    /// rate-limited cadence and supports magazine reloads from a reserve pool.
    /// </summary>
    public class StandardWeapon_UMFOSS : WeaponBase_UMFOSS, IWeaponFirable, IWeaponReloadable
    {
        [Header("Fire Settings")]
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private int magazineSize = 12;
        [SerializeField] private float reloadDuration = 1.5f;
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        private const int RESERVE_MULTIPLIER = 3;

        private int currentAmmo;
        private int reserveAmmo;
        private bool isReloading;
        private float lastFireTime;

        private void Awake()
        {
            currentAmmo = magazineSize;
            reserveAmmo = magazineSize * RESERVE_MULTIPLIER;
        }

        /// <summary>
        /// Returns true when ammo is loaded, no reload is in progress, and the
        /// fire-rate cooldown has elapsed.
        /// </summary>
        public bool CanFire()
        {
            return currentAmmo > 0
                && !isReloading
                && Time.time >= lastFireTime + (1f / fireRate);
        }

        /// <summary>
        /// Spawns the projectile prefab at the fire point, decrements ammo, and
        /// raises fired and ammo-changed events.
        /// </summary>
        public void Fire()
        {
            if (!CanFire())
            {
                return;
            }

            GameObject spawned = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (spawned.TryGetComponent(out Projectile2D_UMFOSS projectile))
            {
                projectile.Initialize(Damage);
            }
            currentAmmo--;
            lastFireTime = Time.time;
            WeaponEventBus.RaiseWeaponFired(GetData());
            WeaponEventBus.RaiseAmmoChanged(currentAmmo, reserveAmmo);
        }

        /// <summary>
        /// Auto-fire is not implemented in this version. StopFire exists to
        /// satisfy IWeaponFirable. Add a repeating coroutine here when
        /// implementing automatic fire mode.
        /// </summary>
        public void StopFire()
        {
        }

        /// <summary>
        /// Returns true when not already reloading, reserve ammo exists, and the
        /// magazine is not already full.
        /// </summary>
        public bool CanReload()
        {
            return !isReloading
                && reserveAmmo > 0
                && currentAmmo < magazineSize;
        }

        /// <summary>
        /// Begins a reload coroutine if <see cref="CanReload"/> is true.
        /// </summary>
        public void Reload()
        {
            if (!CanReload())
            {
                return;
            }

            StartCoroutine(ReloadCoroutine());
        }

        /// <summary> Returns rounds currently in the magazine. </summary>
        public int GetCurrentAmmo()
        {
            return currentAmmo;
        }

        /// <summary> Returns rounds currently in reserve. </summary>
        public int GetReserveAmmo()
        {
            return reserveAmmo;
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

        private IEnumerator ReloadCoroutine()
        {
            isReloading = true;
            WeaponEventBus.RaiseWeaponReloadStart(GetData());

            yield return new WaitForSeconds(reloadDuration);

            int needed = magazineSize - currentAmmo;
            int taken = Mathf.Min(needed, reserveAmmo);
            currentAmmo += taken;
            reserveAmmo -= taken;

            isReloading = false;
            WeaponEventBus.RaiseWeaponReloadComplete(GetData());
            WeaponEventBus.RaiseAmmoChanged(currentAmmo, reserveAmmo);
        }
    }
}
