// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Abstract base for every weapon. Owns identity (name, skin, model) and
    /// shared stats (damage, range, weight) plus the equip/unequip lifecycle.
    /// Capabilities like firing, reloading, or charging are layered on through
    /// interfaces — never on this base class.
    /// </summary>
    public abstract class WeaponBase_UMFOSS : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string weaponName;
        [SerializeField] private Sprite skin;
        [SerializeField] private GameObject model;

        [Header("Stats")]
        [SerializeField] private float damage;
        [SerializeField] private float range;
        [SerializeField] private float weight;

        [Header("State")]
        [SerializeField] private bool isEquipped;
        [SerializeField] private bool isActive;

        /// <summary> Damage stat exposed to derived weapons. </summary>
        protected float Damage => damage;

        /// <summary> Range stat exposed to derived weapons. </summary>
        protected float Range => range;

        /// <summary> Weight stat exposed to derived weapons. </summary>
        protected float Weight => weight;

        /// <summary> Display name exposed to derived weapons. </summary>
        protected string WeaponName => weaponName;

        /// <summary> Model reference exposed to derived weapons. </summary>
        protected GameObject Model => model;

        /// <summary> Equipped flag exposed to derived weapons. </summary>
        protected bool IsEquipped => isEquipped;

        /// <summary> Active flag exposed to derived weapons. </summary>
        protected bool IsActive => isActive;

        /// <summary>
        /// Returns a snapshot of this weapon's data. Concrete weapons must
        /// implement this — typically by returning <see cref="BuildWeaponData"/>.
        /// </summary>
        public abstract WeaponData GetData();

        /// <summary>
        /// Called when the weapon is equipped. Overrides must call base.OnEquip()
        /// as the LAST line so the equipped event fires after subclass setup.
        /// </summary>
        public virtual void OnEquip()
        {
            WeaponEventBus.RaiseWeaponEquipped(GetData());
        }

        /// <summary>
        /// Called when the weapon is unequipped. Overrides must call
        /// base.OnUnequip() as the LAST line so the unequipped event fires after
        /// subclass teardown.
        /// </summary>
        public virtual void OnUnequip()
        {
            WeaponEventBus.RaiseWeaponUnequipped(GetData());
        }

        /// <summary>
        /// Builds a <see cref="WeaponData"/> from this weapon's serialized fields.
        /// Defined here so every concrete weapon shares the exact same packing.
        /// </summary>
        protected WeaponData BuildWeaponData()
        {
            return new WeaponData
            {
                name = weaponName,
                skin = skin,
                damage = damage,
                range = range,
                weight = weight
            };
        }
    }
}
