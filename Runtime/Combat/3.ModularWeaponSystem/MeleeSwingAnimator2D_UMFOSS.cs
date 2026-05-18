// Author: Aditya Jaiswal, Atharv S. Jain
using System.Collections;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Combat
{
    /// <summary>
    /// Demo-only swing animator. Listens for <see cref="WeaponFiredEvent"/>
    /// and rotates a target Transform through an arc to visualise a melee
    /// strike. Stays decoupled from <see cref="MeleeWeapon_UMFOSS"/> via the
    /// event bus — drop it on any GameObject, point it at a sword sprite,
    /// and it animates whenever a melee weapon fires.
    /// </summary>
    public class MeleeSwingAnimator2D_UMFOSS : MonoBehaviour
    {
        [Header("Visual")]
        [Tooltip("Transform that will be rotated during the swing — typically a child holding the sword sprite.")]
        [SerializeField] private Transform swordVisual;

        [Header("Swing Arc")]
        [Tooltip("Local Z rotation at the start of the swing (degrees).")]
        [SerializeField] private float startAngle = 60f;
        [Tooltip("Local Z rotation at the end of the swing (degrees).")]
        [SerializeField] private float endAngle = -60f;
        [Tooltip("Total swing duration in seconds. Should match MeleeWeapon swingDuration for a tight feel.")]
        [SerializeField] private float swingDuration = 0.2f;

        [Header("Filter")]
        [Tooltip("Only animate when the fired weapon's name matches this. Leave blank to react to any fired weapon.")]
        [SerializeField] private string weaponNameFilter = "";

        private Coroutine activeSwing;

        private void OnEnable()
        {
            WeaponEventBus.OnWeaponFired(HandleWeaponFired);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeaponFiredEvent>(HandleWeaponFired);
        }

        private void Start()
        {
            if (swordVisual != null)
            {
                swordVisual.localRotation = Quaternion.Euler(0f, 0f, startAngle);
            }
        }

        private void HandleWeaponFired(WeaponFiredEvent evt)
        {
            if (swordVisual == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(weaponNameFilter) && evt.data.name != weaponNameFilter)
            {
                return;
            }

            if (activeSwing != null)
            {
                StopCoroutine(activeSwing);
            }
            activeSwing = StartCoroutine(SwingRoutine());
        }

        private IEnumerator SwingRoutine()
        {
            float elapsed = 0f;
            while (elapsed < swingDuration)
            {
                float t = elapsed / swingDuration;
                // Ease-out so the swing snaps forward then settles.
                float eased = 1f - (1f - t) * (1f - t);
                float angle = Mathf.Lerp(startAngle, endAngle, eased);
                swordVisual.localRotation = Quaternion.Euler(0f, 0f, angle);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Snap back to rest pose so the next swing has somewhere to swing from.
            swordVisual.localRotation = Quaternion.Euler(0f, 0f, startAngle);
            activeSwing = null;
        }
    }
}
