// Author: Aditya Jaiswal, Atharv S. Jain
using System.Collections;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Stub hitbox component. Activates a Collider2D for a fixed duration and
    /// raises a <see cref="WeaponHitEvent"/> on contact. Damage application is
    /// the listener's job — this script never reaches into other systems.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class HitboxSystem_UMFOSS : MonoBehaviour
    {
        private Collider2D hitboxCollider;
        private float storedDamage;

        private void Awake()
        {
            hitboxCollider = GetComponent<Collider2D>();
            hitboxCollider.enabled = false;
        }

        /// <summary>
        /// Enables the hitbox for <paramref name="duration"/> seconds and stores
        /// the damage to be reported when contact occurs.
        /// </summary>
        public void Activate(float damage, float duration)
        {
            storedDamage = damage;
            hitboxCollider.enabled = true;
            StartCoroutine(DeactivateAfter(duration));
        }

        private IEnumerator DeactivateAfter(float duration)
        {
            yield return new WaitForSeconds(duration);
            hitboxCollider.enabled = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            WeaponEventBus.RaiseWeaponHit(new HitData
            {
                damage = storedDamage,
                hitPoint = hitPoint,
                hitObject = other.gameObject
            });
        }
    }
}
