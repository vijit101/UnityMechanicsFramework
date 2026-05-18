using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Combat;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Subscribes to weapon events via EventBus and spawns visual effects at impact points.
    /// Uses generic GameObject prefabs (not ParticleSystem) for maximum compatibility.
    /// Attach to any GameObject in the scene — no reference to the weapon needed.
    /// </summary>
    public class ImpactEffectSpawner : MonoBehaviour
    {
        [Header("Effect Prefabs (optional)")]
        [Tooltip("Spawned when the weapon hits a target.")]
        [SerializeField] private GameObject hitEffectPrefab;

        [Tooltip("Spawned when the weapon embeds into a surface.")]
        [SerializeField] private GameObject stuckEffectPrefab;

        [Tooltip("Spawned when the weapon is caught back in the hand.")]
        [SerializeField] private GameObject catchEffectPrefab;

        [Header("Settings")]
        [SerializeField] private float effectLifetime = 2f;

        private void OnEnable()
        {
            EventBus.Subscribe<WeaponHitEvent>(OnWeaponHit);
            EventBus.Subscribe<WeaponStuckEvent>(OnWeaponStuck);
            EventBus.Subscribe<WeaponCaughtEvent>(OnWeaponCaught);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeaponHitEvent>(OnWeaponHit);
            EventBus.Unsubscribe<WeaponStuckEvent>(OnWeaponStuck);
            EventBus.Unsubscribe<WeaponCaughtEvent>(OnWeaponCaught);
        }

        private void OnWeaponHit(WeaponHitEvent e)
        {
            SpawnEffect(hitEffectPrefab, e.HitPosition);
        }

        private void OnWeaponStuck(WeaponStuckEvent e)
        {
            SpawnEffect(stuckEffectPrefab, e.ContactPoint);
        }

        private void OnWeaponCaught(WeaponCaughtEvent e)
        {
            if (e.Weapon != null)
                SpawnEffect(catchEffectPrefab, e.Weapon.transform.position);
        }

        private void SpawnEffect(GameObject prefab, Vector3 position)
        {
            if (prefab == null) return;

            GameObject instance = Instantiate(prefab, position, Quaternion.identity);
            Destroy(instance, effectLifetime);
        }
    }
}
