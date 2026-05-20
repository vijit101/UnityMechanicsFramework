using System.Collections;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Applies screen shake to a camera by offsetting its local position.
    /// Attach to the camera GameObject. Other systems trigger shakes by calling Shake()
    /// directly or by wiring EventBus events to it.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        [Header("Defaults")]
        [SerializeField] private float defaultIntensity = 0.15f;
        [SerializeField] private float defaultDuration = 0.2f;

        private Vector3 originalLocalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            originalLocalPos = transform.localPosition;
        }

        /// <summary>
        /// Triggers a camera shake with the given intensity and duration.
        /// If a shake is already running, it restarts with the new parameters.
        /// </summary>
        public void Shake(float intensity, float duration)
        {
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ShakeRoutine(intensity, duration));
        }

        /// <summary>Triggers a camera shake with default intensity and duration.</summary>
        public void Shake()
        {
            Shake(defaultIntensity, defaultDuration);
        }

        private IEnumerator ShakeRoutine(float intensity, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float decay = 1f - (elapsed / duration);
                float currentIntensity = intensity * decay;

                Vector3 offset = new Vector3(
                    Random.Range(-currentIntensity, currentIntensity),
                    Random.Range(-currentIntensity, currentIntensity),
                    0f
                );

                transform.localPosition = originalLocalPos + offset;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPos;
            shakeCoroutine = null;
        }
    }
}
