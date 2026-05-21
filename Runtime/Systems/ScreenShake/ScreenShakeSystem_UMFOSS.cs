using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class ScreenShakeSystem_UMFOSS : MonoSingletongeneric<ScreenShakeSystem_UMFOSS>
    {
        [Header("Shake Settings")]
        [SerializeField] private float ShakeDecay = 1.3f;
        [SerializeField] private float TraumaMultiplier = 16f;
        [SerializeField] private float PositionMagnitude = 0.8f;
        [SerializeField] private float RotationMagnitude = 10f;

        private const float RETURN_SPEED = 5f;
        private const float TRAUMA_THRESHOLD = 0.3f;
        private const float TRAUMA_POW = 0.5f;

        private Camera cam;
        private Vector3 originalPosition;
        private Quaternion originalRotation;

        private float trauma = 0f;
        private float traumaTimeRemaining = 0f;

        private float timeCounter;

        protected override void Awake()
        {
            base.Awake();

            cam = Camera.main;

            if (cam == null)
            {
                Debug.LogError("Camera.main NOT found!");
                return;
            }

            originalPosition = cam.transform.localPosition;
            originalRotation = cam.transform.localRotation;
        }

        /// <summary>
        /// Adds trauma to the camera shake system and sets the shake duration.
        /// </summary>
        /// <param name="magnitude">Amount of trauma to add (0 to 1 limit applies).</param>
        /// <param name="duration">Duration to shake in seconds.</param>
        public void TriggerShake(float magnitude, float duration)
        {
            trauma = Mathf.Clamp01(trauma + magnitude);
            traumaTimeRemaining = duration;
        }

        float GetFloat(float seed)
        {
            return (Mathf.PerlinNoise(seed, timeCounter) - 0.5f) * 2f;
        }

        Vector3 GetVec3()
        {
            return new Vector3(
                GetFloat(1),
                GetFloat(10),
                0f
            );
        }

        void Update()
        {
            if (traumaTimeRemaining > 0f && trauma > 0f)
            {
                traumaTimeRemaining -= Time.deltaTime;

                // smoother time progression
                timeCounter += Time.deltaTime * Mathf.Pow(trauma, TRAUMA_POW) * TraumaMultiplier * TRAUMA_THRESHOLD;

                float smoothTrauma = trauma * trauma;
                Vector3 offset = GetVec3() * PositionMagnitude * smoothTrauma;

                cam.transform.localPosition = originalPosition + offset;
                cam.transform.localRotation = Quaternion.Euler(offset * RotationMagnitude);

                // smoother decay
                trauma -= Time.deltaTime * ShakeDecay * (trauma + TRAUMA_THRESHOLD);
                trauma = Mathf.Clamp01(trauma);
            }
            else
            {
                // smooth return
                cam.transform.localPosition =
                    Vector3.Lerp(cam.transform.localPosition, originalPosition, Time.deltaTime * RETURN_SPEED);

                cam.transform.localRotation =
                    Quaternion.Lerp(cam.transform.localRotation, originalRotation, Time.deltaTime * RETURN_SPEED);

                trauma = 0f;
            }
        }
    }
}