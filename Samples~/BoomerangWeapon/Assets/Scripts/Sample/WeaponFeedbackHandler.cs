using System.Collections;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Samples.BoomerangWeapon
{
    /// <summary>
    /// Subscribes to weapon events and triggers camera shake and hit-stop (time freeze)
    /// for game feel. Attach to any GameObject in the scene — fully decoupled from the weapon.
    /// </summary>
    public class WeaponFeedbackHandler : MonoBehaviour
    {
        [Header("Camera Shake")]
        [SerializeField] private CameraShake cameraShake;

        [Header("Shake Intensities")]
        [SerializeField] private float throwShakeIntensity = 0.08f;
        [SerializeField] private float throwShakeDuration = 0.1f;

        [SerializeField] private float hitShakeIntensity = 0.2f;
        [SerializeField] private float hitShakeDuration = 0.15f;

        [SerializeField] private float catchShakeIntensity = 0.12f;
        [SerializeField] private float catchShakeDuration = 0.1f;

        [SerializeField] private float stuckShakeIntensity = 0.15f;
        [SerializeField] private float stuckShakeDuration = 0.12f;

        [Header("Hit Stop")]
        [Tooltip("Brief time-freeze on hit for impact feel. Set to 0 to disable.")]
        [SerializeField] private float hitStopDuration = 0.05f;

        private const float KILL_SHAKE_MULTIPLIER = 1.5f;
        private const float KILL_DURATION_MULTIPLIER = 2f;

        private Coroutine hitStopCoroutine;

        private void OnEnable()
        {
            EventBus.Subscribe<WeaponThrownEvent>(OnThrown);
            EventBus.Subscribe<WeaponHitEvent>(OnHit);
            EventBus.Subscribe<WeaponCaughtEvent>(OnCaught);
            EventBus.Subscribe<WeaponStuckEvent>(OnStuck);
            EventBus.Subscribe<TargetKilledEvent>(OnKill);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<WeaponThrownEvent>(OnThrown);
            EventBus.Unsubscribe<WeaponHitEvent>(OnHit);
            EventBus.Unsubscribe<WeaponCaughtEvent>(OnCaught);
            EventBus.Unsubscribe<WeaponStuckEvent>(OnStuck);
            EventBus.Unsubscribe<TargetKilledEvent>(OnKill);

            Time.timeScale = 1f;
        }

        private void OnThrown(WeaponThrownEvent e)
        {
            if (cameraShake != null)
                cameraShake.Shake(throwShakeIntensity, throwShakeDuration);
        }

        private void OnHit(WeaponHitEvent e)
        {
            if (cameraShake != null)
                cameraShake.Shake(hitShakeIntensity, hitShakeDuration);

            TriggerHitStop();
        }

        private void OnCaught(WeaponCaughtEvent e)
        {
            if (cameraShake != null)
                cameraShake.Shake(catchShakeIntensity, catchShakeDuration);
        }

        private void OnStuck(WeaponStuckEvent e)
        {
            if (cameraShake != null)
                cameraShake.Shake(stuckShakeIntensity, stuckShakeDuration);
        }

        private void OnKill(TargetKilledEvent e)
        {
            if (cameraShake != null)
                cameraShake.Shake(hitShakeIntensity * KILL_SHAKE_MULTIPLIER, hitShakeDuration * KILL_DURATION_MULTIPLIER);

            TriggerHitStop();
        }

        private void TriggerHitStop()
        {
            if (hitStopDuration <= 0f) return;

            if (hitStopCoroutine != null)
                StopCoroutine(hitStopCoroutine);

            hitStopCoroutine = StartCoroutine(HitStopRoutine());
        }

        private IEnumerator HitStopRoutine()
        {
            float previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            // Only restore if we're still the ones controlling timeScale
            if (Time.timeScale == 0f)
                Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f;
            hitStopCoroutine = null;
        }
    }
}
