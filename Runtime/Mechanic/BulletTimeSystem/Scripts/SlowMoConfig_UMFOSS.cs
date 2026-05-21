using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Configures one slow motion profile.
    /// Designers can create multiple assets with different timing, pitch, and resource behavior.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSlowMoConfig", menuName = "UMFOSS/Systems/SlowMoConfig")]
    public class SlowMoConfig_UMFOSS : ScriptableObject
    {
        [Header("Time")]
        [SerializeField] private float timeScale = 0.2f;
        [SerializeField] private float enterDuration = 0.1f;
        [SerializeField] private float exitDuration = 0.2f;
        [SerializeField] private float maxDuration = 3f;
        [SerializeField] private AnimationCurve enterCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        [SerializeField] private AnimationCurve exitCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Audio")]
        [SerializeField] private float audioPitchScale = 0.8f;
        [SerializeField] private float audioMixerTransitionTime = 0.1f;

        [Header("Resource")]
        [SerializeField] private float resourceCost = 0f;
        [SerializeField] private float resourceDrainRate = 1f;
        [SerializeField] private float resourceRechargeRate = 0.5f;
        [SerializeField] private float maxResource = 100f;

        public float TimeScale => Mathf.Clamp(timeScale, 0.01f, 1f);
        public float EnterDuration => Mathf.Max(0f, enterDuration);
        public float ExitDuration => Mathf.Max(0f, exitDuration);
        public float MaxDuration => Mathf.Max(0f, maxDuration);
        public AnimationCurve EnterCurve => enterCurve;
        public AnimationCurve ExitCurve => exitCurve;
        public float AudioPitchScale => Mathf.Clamp(audioPitchScale, 0.05f, 3f);
        public float AudioMixerTransitionTime => Mathf.Max(0f, audioMixerTransitionTime);
        public float ResourceCost => Mathf.Max(0f, resourceCost);
        public float ResourceDrainRate => Mathf.Max(0f, resourceDrainRate);
        public float ResourceRechargeRate => Mathf.Max(0f, resourceRechargeRate);
        public float MaxResource => Mathf.Max(1f, maxResource);

        public void ApplyRuntimePreset(
            float presetTimeScale,
            float presetEnterDuration,
            float presetExitDuration,
            float presetMaxDuration,
            AnimationCurve presetEnterCurve,
            AnimationCurve presetExitCurve,
            float presetAudioPitchScale,
            float presetAudioMixerTransitionTime,
            float presetResourceCost,
            float presetResourceDrainRate,
            float presetResourceRechargeRate,
            float presetMaxResource)
        {
            timeScale = presetTimeScale;
            enterDuration = presetEnterDuration;
            exitDuration = presetExitDuration;
            maxDuration = presetMaxDuration;
            enterCurve = presetEnterCurve;
            exitCurve = presetExitCurve;
            audioPitchScale = presetAudioPitchScale;
            audioMixerTransitionTime = presetAudioMixerTransitionTime;
            resourceCost = presetResourceCost;
            resourceDrainRate = presetResourceDrainRate;
            resourceRechargeRate = presetResourceRechargeRate;
            maxResource = presetMaxResource;
        }

        private void OnValidate()
        {
            if (enterCurve == null || enterCurve.length == 0)
                enterCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

            if (exitCurve == null || exitCurve.length == 0)
                exitCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }
}
