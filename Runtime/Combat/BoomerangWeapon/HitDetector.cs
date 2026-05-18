using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Detects hurtbox overlaps around the weapon and applies damage via IDamageable.
    /// Uses a HashSet to prevent hitting the same target multiple times per flight.
    /// Publishes WeaponHitEvent for each new target so VFX/audio systems can react.
    /// </summary>
    public class HitDetector
    {
        private readonly Collider weaponCollider;
        private readonly LayerMask hurtboxLayer;
        private readonly GameObject weaponObject;
        private readonly BoomerangConfig config;
        private readonly HashSet<Collider> hitTargets = new HashSet<Collider>();
        private bool isReturnFlight;
        private Vector3 previousPosition;
        private float cachedRadius;

        /// <summary>The most recent hurtbox collider hit during this flight. Reset on ClearHitTargets.</summary>
        public Collider LastHitCollider { get; private set; }

        public HitDetector(Collider weaponCollider, LayerMask hurtboxLayer, GameObject weaponObject, BoomerangConfig config)
        {
            this.weaponCollider = weaponCollider;
            this.hurtboxLayer = hurtboxLayer;
            this.weaponObject = weaponObject;
            this.config = config;

            // Cache the radius from the collider bounds while it's valid
            cachedRadius = weaponCollider != null && weaponCollider.enabled
                ? Mathf.Max(weaponCollider.bounds.extents.magnitude, 0.3f)
                : 0.5f;
        }

        /// <summary>Clears the hit history and sets the flight phase for damage calculation.</summary>
        public void ClearHitTargets(bool returnFlight = false)
        {
            hitTargets.Clear();
            isReturnFlight = returnFlight;
            previousPosition = weaponObject.transform.position;
            LastHitCollider = null;
        }

        /// <summary>
        /// Excludes a collider from future hit detection this flight.
        /// Used to prevent the recall flight from re-damaging the target the weapon was embedded in.
        /// </summary>
        public void ExcludeCollider(Collider collider)
        {
            if (collider != null)
                hitTargets.Add(collider);
        }

        /// <summary>
        /// Performs an OverlapSphere at the given position.
        /// For each new target found:
        /// 1. Applies damage via IDamageable (if present)
        /// 2. Publishes WeaponHitEvent via EventBus
        /// </summary>
        public void CheckOverlap(Vector3 position)
        {
            // Use cached radius — the collider may be disabled during flight
            Collider[] hits = UnityEngine.Physics.OverlapSphere(position, cachedRadius, hurtboxLayer);

            float damage = isReturnFlight ? config.ReturnDamage : config.ThrowDamage;
            Vector3 travelDir = (position - previousPosition).normalized;
            Vector3 knockbackDir = travelDir.sqrMagnitude > 0.001f ? travelDir : weaponObject.transform.forward;
            previousPosition = position;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hitTargets.Contains(hits[i])) continue;
                hitTargets.Add(hits[i]);

                var damageable = hits[i].GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(new DamageData
                    {
                        Amount = damage,
                        HitPoint = position,
                        KnockbackDirection = knockbackDir,
                        KnockbackForce = config.KnockbackForce,
                        Source = weaponObject
                    });
                }

                LastHitCollider = hits[i];

                EventBus.Publish(new WeaponHitEvent
                {
                    Target = hits[i].gameObject,
                    Weapon = weaponObject,
                    HitPosition = position,
                    DamageDealt = damage
                });
            }
        }
    }
}
