// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Demo-only input forwarder. Holds a reference to a single weapon and
    /// forwards Fire / StopFire / Reload to it based on key input. Talks to
    /// the weapon exclusively through its capability interfaces, so the same
    /// script drops into a scene with any weapon type without modification.
    /// One scene = one weapon = one DemoWeaponInput.
    /// </summary>
    public class DemoWeaponInput : MonoBehaviour
    {
        [Header("Weapon")]
        [Tooltip("Drag the weapon GameObject (or any component on it) here.")]
        [SerializeField] private WeaponBase_UMFOSS weapon;

        [Header("Input")]
        [SerializeField] private KeyCode fireKey = KeyCode.Mouse0;
        [SerializeField] private KeyCode reloadKey = KeyCode.R;
        [SerializeField] private bool equipOnStart = true;

        private IWeaponFirable firable;
        private IWeaponReloadable reloadable;

        private void Start()
        {
            if (weapon == null)
            {
                Debug.LogWarning($"[DemoWeaponInput] No weapon assigned on {name}.");
                return;
            }

            firable = weapon as IWeaponFirable;
            reloadable = weapon as IWeaponReloadable;

            if (equipOnStart)
            {
                weapon.OnEquip();
            }
        }

        private void Update()
        {
            if (firable != null)
            {
                if (Input.GetKeyDown(fireKey))
                {
                    firable.Fire();
                }
                else if (Input.GetKeyUp(fireKey))
                {
                    firable.StopFire();
                }
            }

            if (reloadable != null && Input.GetKeyDown(reloadKey))
            {
                reloadable.Reload();
            }
        }
    }
}
