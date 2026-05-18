// Author: Aditya Jaiswal, Atharv S. Jain
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Self-contained 2D projectile. Launches itself forward on spawn, reports
    /// hits through <see cref="WeaponEventBus"/>, and despawns on contact or
    /// after a fixed lifetime. Designed to be dropped onto a prefab and
    /// assigned to <see cref="StandardWeapon_UMFOSS"/>'s projectile slot.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Projectile2D_UMFOSS : MonoBehaviour
    {
        [Header("Flight")]
        [SerializeField] private float speed = 12f;
        [SerializeField] private float lifetime = 3f;

        [Header("Damage")]
        [Tooltip("Fallback damage used when the spawner does not call Initialize.")]
        [SerializeField] private float damage = 10f;

        private Rigidbody2D rb;

        /// <summary>
        /// Overrides the projectile's damage at spawn time. Spawners (e.g.
        /// <see cref="StandardWeapon_UMFOSS"/>) call this immediately after
        /// Instantiate so <see cref="WeaponData.damage"/> remains the source
        /// of truth instead of the prefab's serialized fallback.
        /// </summary>
        public void Initialize(float damageOverride)
        {
            damage = damageOverride;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // Trigger collider so OnTriggerEnter2D fires and the projectile
            // does not physically push the target.
            Collider2D col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void Start()
        {
            // transform.right is the local +X axis, which matches the firePoint
            // rotation passed to Instantiate in StandardWeapon_UMFOSS.Fire().
            rb.linearVelocity = transform.right * speed;
            Destroy(gameObject, lifetime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            WeaponEventBus.RaiseWeaponHit(new HitData
            {
                damage = damage,
                hitPoint = hitPoint,
                hitObject = other.gameObject
            });

            Destroy(gameObject);
        }
    }
}
