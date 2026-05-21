using System;
using System.Collections;
using UnityEngine;
using GameplayMechanicsUMFOSS.Combat;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// A destructible target for the boomerang demo. Has a Damageable component for health,
    /// shows a health bar, ragdolls on death, and respawns after a delay.
    /// Safely detaches any parented objects (e.g., embedded weapons) before respawn teleport.
    /// </summary>
    [RequireComponent(typeof(Damageable))]
    public class TargetDummy : MonoBehaviour
    {
        [Header("Respawn")]
        [SerializeField] private float respawnDelay = 3f;

        [Header("Death Effect")]
        [Tooltip("If assigned, this Rigidbody is set non-kinematic on death for ragdoll.")]
        [SerializeField] private Rigidbody ragdollRb;

        [Header("Health Bar")]
        [Tooltip("A child Transform scaled on X to represent health (e.g., a quad).")]
        [SerializeField] private Transform healthBarFill;

        public event Action OnRespawned;

        private const float RAGDOLL_IMPULSE = 3f;

        private Damageable damageable;
        private Vector3 spawnPosition;
        private Quaternion spawnRotation;
        private Vector3 healthBarOriginalScale;
        private int originalChildCount;

        private void Awake()
        {
            damageable = GetComponent<Damageable>();
            spawnPosition = transform.position;
            spawnRotation = transform.rotation;
            originalChildCount = transform.childCount;

            if (healthBarFill != null)
                healthBarOriginalScale = healthBarFill.localScale;
        }

        private void OnEnable()
        {
            damageable.OnHealthChanged += UpdateHealthBar;
            damageable.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            damageable.OnHealthChanged -= UpdateHealthBar;
            damageable.OnDeath -= HandleDeath;
        }

        private void UpdateHealthBar(float healthRatio)
        {
            if (healthBarFill == null) return;

            Vector3 scale = healthBarOriginalScale;
            scale.x *= healthRatio;
            healthBarFill.localScale = scale;
        }

        private void HandleDeath(Damageable dead)
        {
            // Detach any foreign children (e.g., embedded weapons) before ragdolling
            DetachForeignChildren();

            if (ragdollRb != null)
            {
                ragdollRb.isKinematic = false;
                ragdollRb.AddForce(Vector3.up * RAGDOLL_IMPULSE, ForceMode.Impulse);
            }

            StartCoroutine(RespawnRoutine());
        }

        private IEnumerator RespawnRoutine()
        {
            yield return new WaitForSeconds(respawnDelay);

            // Detach again in case something parented during the respawn delay
            DetachForeignChildren();

            transform.position = spawnPosition;
            transform.rotation = spawnRotation;

            if (ragdollRb != null)
            {
                ragdollRb.linearVelocity = Vector3.zero;
                ragdollRb.angularVelocity = Vector3.zero;
                ragdollRb.isKinematic = true;
            }

            damageable.ResetHealth();
            UpdateHealthBar(1f);

            OnRespawned?.Invoke();
        }

        /// <summary>
        /// Unparents any children that weren't part of the original hierarchy
        /// (e.g., embedded weapons). Preserves their world position.
        /// </summary>
        private void DetachForeignChildren()
        {
            for (int i = transform.childCount - 1; i >= originalChildCount; i--)
            {
                Transform child = transform.GetChild(i);
                child.SetParent(null);
            }
        }
    }
}
