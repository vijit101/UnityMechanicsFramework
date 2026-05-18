// Author: Aditya Jaiswal, Atharv S. Jain
using System.Collections;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Demo-only target. Subscribes to <see cref="WeaponHitEvent"/> and
    /// plays a brief flash + punch-scale reaction before destroying itself
    /// when it is the reported hit object. Stays decoupled from the bullet —
    /// the projectile self-destructs through its own OnTriggerEnter2D, while
    /// the target reacts through the event bus, so neither side has a hard
    /// reference to the other.
    /// </summary>
    public class DemoTarget2D_UMFOSS : MonoBehaviour
    {
        [Header("Hit Reaction")]
        [Tooltip("How long the flash + scale punch plays before the target is destroyed.")]
        [SerializeField] private float reactionDuration = 0.15f;
        [Tooltip("Color flashed onto the sprite during the reaction.")]
        [SerializeField] private Color flashColor = Color.white;
        [Tooltip("Peak scale multiplier at the start of the punch.")]
        [SerializeField] private float punchScale = 1.3f;

        private SpriteRenderer spriteRenderer;
        private Color baseColor;
        private Vector3 baseScale;
        private bool isReacting;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                baseColor = spriteRenderer.color;
            }
            baseScale = transform.localScale;
        }

        private void OnEnable()
        {
            WeaponEventBus.OnWeaponHit(HandleWeaponHit);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeaponHitEvent>(HandleWeaponHit);
        }

        private void HandleWeaponHit(WeaponHitEvent evt)
        {
            if (isReacting || evt.hitData.hitObject != gameObject)
            {
                return;
            }

            isReacting = true;
            StartCoroutine(ReactAndDie());
        }

        private IEnumerator ReactAndDie()
        {
            float elapsed = 0f;
            while (elapsed < reactionDuration)
            {
                float t = elapsed / reactionDuration;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.Lerp(flashColor, baseColor, t);
                }

                float scaleT = Mathf.Lerp(punchScale, 1f, t);
                transform.localScale = baseScale * scaleT;

                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
