using System;
using System.Collections;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Concrete implementation of IDamageable. Attach to any GameObject that should
    /// receive damage. Handles health tracking, knockback physics, damage flash,
    /// and death notification via EventBus.
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        // ───────────────────────────────────────────────
        // Serialized Fields
        // ───────────────────────────────────────────────

        [Header("Health")]
        [SerializeField] private float maxHealth = 100f;

        [Header("Knockback")]
        [Tooltip("If assigned, knockback impulse is applied to this Rigidbody.")]
        [SerializeField] private Rigidbody knockbackRb;

        [Header("Visual Feedback")]
        [Tooltip("If assigned, flashes this renderer red on hit.")]
        [SerializeField] private Renderer hitFlashRenderer;

        [SerializeField] private Color hitFlashColor = Color.red;
        [SerializeField] private float hitFlashDuration = 0.15f;

        // ───────────────────────────────────────────────
        // Private Fields
        // ───────────────────────────────────────────────

        private float currentHealth;
        private Color originalColor;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private Coroutine flashCoroutine;

        // ───────────────────────────────────────────────
        // Events
        // ───────────────────────────────────────────────

        /// <summary>Fired when this object takes damage. Passes current health ratio (0-1).</summary>
        public event Action<float> OnHealthChanged;

        /// <summary>Fired when health reaches zero.</summary>
        public event Action<Damageable> OnDeath;

        // ───────────────────────────────────────────────
        // Public Properties (IDamageable)
        // ───────────────────────────────────────────────

        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public float HealthRatio => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public bool IsAlive => currentHealth > 0f;

        // ───────────────────────────────────────────────
        // Unity Lifecycle
        // ───────────────────────────────────────────────

        private void Awake()
        {
            currentHealth = maxHealth;
            propertyBlock = new MaterialPropertyBlock();

            if (hitFlashRenderer != null)
            {
                hitFlashRenderer.GetPropertyBlock(propertyBlock);
                originalColor = hitFlashRenderer.material.color;
            }
        }

        // ───────────────────────────────────────────────
        // Public Methods
        // ───────────────────────────────────────────────

        /// <summary>
        /// Applies damage, knockback, and visual feedback.
        /// Returns the actual damage dealt (clamped to remaining health).
        /// </summary>
        public float TakeDamage(DamageData damageData)
        {
            if (!IsAlive) return 0f;

            float actualDamage = Mathf.Min(damageData.Amount, currentHealth);
            currentHealth -= actualDamage;

            ApplyKnockback(damageData);
            TriggerHitFlash();

            OnHealthChanged?.Invoke(HealthRatio);

            EventBus.Publish(new DamageDealtEvent
            {
                Target = gameObject,
                Source = damageData.Source,
                DamageAmount = actualDamage,
                RemainingHealth = currentHealth,
                HitPoint = damageData.HitPoint
            });

            if (!IsAlive)
            {
                OnDeath?.Invoke(this);
                EventBus.Publish(new TargetKilledEvent
                {
                    Target = gameObject,
                    Source = damageData.Source
                });
            }

            return actualDamage;
        }

        /// <summary>Restores health to maximum and re-enables the object.</summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(HealthRatio);
        }

        // ───────────────────────────────────────────────
        // Private Methods
        // ───────────────────────────────────────────────

        private void ApplyKnockback(DamageData damageData)
        {
            if (knockbackRb == null || damageData.KnockbackForce <= 0f) return;

            knockbackRb.AddForce(
                damageData.KnockbackDirection.normalized * damageData.KnockbackForce,
                ForceMode.Impulse
            );
        }

        private void TriggerHitFlash()
        {
            if (hitFlashRenderer == null) return;

            if (flashCoroutine != null)
                StopCoroutine(flashCoroutine);

            flashCoroutine = StartCoroutine(HitFlashRoutine());
        }

        private IEnumerator HitFlashRoutine()
        {
            SetRendererColor(hitFlashColor);
            yield return new WaitForSeconds(hitFlashDuration);
            SetRendererColor(originalColor);
            flashCoroutine = null;
        }

        private void SetRendererColor(Color color)
        {
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            hitFlashRenderer.SetPropertyBlock(propertyBlock);
        }
    }

    // ───────────────────────────────────────────────
    // Damage Events (published via EventBus)
    // ───────────────────────────────────────────────

    /// <summary>Fired whenever any Damageable takes damage.</summary>
    public struct DamageDealtEvent
    {
        public GameObject Target;
        public GameObject Source;
        public float DamageAmount;
        public float RemainingHealth;
        public Vector3 HitPoint;
    }

    /// <summary>Fired when a Damageable's health reaches zero.</summary>
    public struct TargetKilledEvent
    {
        public GameObject Target;
        public GameObject Source;
    }
}
