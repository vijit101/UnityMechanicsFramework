using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Global bullet time / slow motion system.
    /// Smoothly transitions Time.timeScale, keeps fixedDeltaTime proportional,
    /// coordinates with PauseSystem, and broadcasts lifecycle events via EventBus.
    /// </summary>
    public class BulletTimeSystem_UMFOSS : MonoSingletongeneric<BulletTimeSystem_UMFOSS>
    {
        [Header("Default Configuration")]
        [SerializeField] private SlowMoConfig_UMFOSS defaultConfig;

        [Header("Audio")]
        [SerializeField] private AudioMixer gameMixer;
        [SerializeField] private string pitchParameter = "MasterPitch";

        [Tooltip("Optional demo fallback when no AudioMixer is assigned.")]
        [SerializeField] private List<AudioSource> fallbackAudioSources = new List<AudioSource>();

        [Header("Debug")]
        [SerializeField] private bool logWarnings = true;

        private SlowMoConfig_UMFOSS activeConfig;
        private float currentResource;
        private float storedNormalTimeScale = 1f;
        private float baseFixedDeltaTime = 0.02f;
        private float remainingDuration = -1f;
        private float currentAudioPitch = 1f;
        private bool isActive;
        private bool isPausedByPauseSystem;
        private bool resourceInitialized;
        private Coroutine activeTransition;
        private PendingTransition pendingTransition;

        private struct PendingTransition
        {
            public bool HasValue;
            public float TargetTimeScale;
            public float TargetPitch;
            public float Duration;
            public AnimationCurve Curve;
            public TransitionMode Mode;
        }

        private enum TransitionMode
        {
            None,
            Enter,
            Exit
        }

        public bool IsActive() => isActive;
        public float GetCurrentResource() => currentResource;
        public float GetCurrentTimeScale() => Time.timeScale;
        public float GetCurrentAudioPitch() => currentAudioPitch;

        public float GetResourcePercent()
        {
            float max = GetResourceMax();
            return max > 0f ? currentResource / max : 0f;
        }

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
                return;

            baseFixedDeltaTime = Time.fixedDeltaTime > 0f ? Time.fixedDeltaTime : 0.02f;
            activeConfig = defaultConfig;
            EnsureResourceInitialized(defaultConfig);
            ApplyAudioPitchImmediate(1f);
        }

        private void OnEnable()
        {
            EventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Subscribe<GameResumedEvent>(OnGameResumed);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            EventBus.Unsubscribe<GameResumedEvent>(OnGameResumed);
        }

        private void Update()
        {
            SlowMoConfig_UMFOSS config = GetRelevantConfig();
            if (config == null)
                return;

            if (isActive)
            {
                TickDuration(config);
                TickActiveResource(config);
            }
            else
            {
                TickRecharge(config);
            }
        }

        public void Enter(SlowMoConfig_UMFOSS config = null)
        {
            if (IsPaused())
            {
                if (logWarnings)
                    Debug.LogWarning("[BulletTime] Enter ignored because the game is paused.");
                return;
            }

            SlowMoConfig_UMFOSS chosenConfig = ResolveConfig(config);
            if (chosenConfig == null)
            {
                if (logWarnings)
                    Debug.LogWarning("[BulletTime] No SlowMoConfig assigned.");
                return;
            }

            if (isActive && Time.timeScale <= chosenConfig.TimeScale + 0.0001f)
                return;

            EnsureResourceInitialized(chosenConfig);
            currentResource = Mathf.Clamp(currentResource, 0f, chosenConfig.MaxResource);

            if (!isActive)
            {
                storedNormalTimeScale = Mathf.Max(0.01f, Time.timeScale);

                if (chosenConfig.ResourceCost > 0f)
                {
                    if (currentResource < chosenConfig.ResourceCost)
                    {
                        if (logWarnings)
                            Debug.LogWarning("[BulletTime] Not enough resource to enter slow motion.");
                        return;
                    }

                    currentResource -= chosenConfig.ResourceCost;
                    PublishResourceChanged(chosenConfig);
                }
            }

            activeConfig = chosenConfig;
            isActive = true;
            remainingDuration = chosenConfig.MaxDuration > 0f ? chosenConfig.MaxDuration : -1f;

            EventBus.Publish(new BulletTimeEnterEvent
            {
                targetTimeScale = chosenConfig.TimeScale,
                enterDuration = chosenConfig.EnterDuration
            });

            StartTransition(
                chosenConfig.TimeScale,
                chosenConfig.AudioPitchScale,
                Mathf.Max(chosenConfig.EnterDuration, chosenConfig.AudioMixerTransitionTime),
                chosenConfig.EnterCurve,
                TransitionMode.Enter
            );
        }

        public void Exit()
        {
            if (!isActive && !pendingTransition.HasValue)
                return;

            SlowMoConfig_UMFOSS config = GetRelevantConfig();
            float exitDuration = config != null ? config.ExitDuration : 0f;
            AnimationCurve exitCurve = config != null ? config.ExitCurve : AnimationCurve.Linear(0f, 0f, 1f, 1f);

            isActive = false;
            remainingDuration = -1f;

            EventBus.Publish(new BulletTimeExitEvent
            {
                restoredTimeScale = storedNormalTimeScale,
                exitDuration = exitDuration
            });

            if (isPausedByPauseSystem)
            {
                StopActiveTransition();
                PauseSystem_UMFOSS.Instance?.OverrideStoredTimeScale(storedNormalTimeScale);
                pendingTransition = new PendingTransition
                {
                    HasValue = false,
                    Mode = TransitionMode.None
                };
                ApplyAudioPitchImmediate(1f);
                return;
            }

            StartTransition(
                storedNormalTimeScale,
                1f,
                Mathf.Max(exitDuration, config != null ? config.AudioMixerTransitionTime : 0f),
                exitCurve,
                TransitionMode.Exit
            );
        }

        public void Toggle(SlowMoConfig_UMFOSS config = null)
        {
            if (isActive) Exit();
            else          Enter(config);
        }

        /// <summary>
        /// Allows runtime demo bootstrappers to assign a default config without relying on inspector wiring.
        /// </summary>
        public void SetDefaultConfig(SlowMoConfig_UMFOSS config)
        {
            defaultConfig = config;
            if (activeConfig == null)
                activeConfig = config;

            EnsureResourceInitialized(config);
        }

        public void RegisterFallbackAudioSource(AudioSource source)
        {
            if (source == null || fallbackAudioSources.Contains(source))
                return;

            fallbackAudioSources.Add(source);
            source.pitch = currentAudioPitch;
        }

        public void UnregisterFallbackAudioSource(AudioSource source)
        {
            if (source == null)
                return;

            fallbackAudioSources.Remove(source);
            source.pitch = 1f;
        }

        private void TickDuration(SlowMoConfig_UMFOSS config)
        {
            if (config.MaxDuration <= 0f)
                return;

            remainingDuration -= Time.unscaledDeltaTime;
            if (remainingDuration > 0f)
                return;

            remainingDuration = 0f;
            EventBus.Publish(new BulletTimeExpiredEvent());
            Exit();
        }

        private void TickActiveResource(SlowMoConfig_UMFOSS config)
        {
            if (config.ResourceCost <= 0f)
                return;

            float previous = currentResource;
            currentResource -= config.ResourceDrainRate * Time.unscaledDeltaTime;
            currentResource = Mathf.Clamp(currentResource, 0f, config.MaxResource);

            if (!Mathf.Approximately(previous, currentResource))
                PublishResourceChanged(config);

            if (currentResource > 0f)
                return;

            EventBus.Publish(new BulletTimeExpiredEvent());
            Exit();
        }

        private void TickRecharge(SlowMoConfig_UMFOSS config)
        {
            float maxResource = config.MaxResource;
            if (currentResource >= maxResource)
                return;

            float previous = currentResource;
            currentResource += config.ResourceRechargeRate * Time.unscaledDeltaTime;
            currentResource = Mathf.Min(currentResource, maxResource);

            if (!Mathf.Approximately(previous, currentResource))
                PublishResourceChanged(config);
        }

        private void PublishResourceChanged(SlowMoConfig_UMFOSS config)
        {
            EventBus.Publish(new BulletTimeResourceChangedEvent
            {
                current = currentResource,
                max = config.MaxResource,
                percent = config.MaxResource > 0f ? currentResource / config.MaxResource : 0f
            });
        }

        private void StartTransition(float targetTimeScale, float targetPitch, float duration, AnimationCurve curve, TransitionMode mode)
        {
            StopActiveTransition();

            pendingTransition = new PendingTransition
            {
                HasValue = true,
                TargetTimeScale = Mathf.Clamp(targetTimeScale, 0f, 1f),
                TargetPitch = Mathf.Max(0.05f, targetPitch),
                Duration = Mathf.Max(0f, duration),
                Curve = curve != null ? curve : AnimationCurve.Linear(0f, 0f, 1f, 1f),
                Mode = mode
            };

            activeTransition = StartCoroutine(TransitionRoutine(pendingTransition));
        }

        private IEnumerator TransitionRoutine(PendingTransition transition)
        {
            float startScale = Time.timeScale;
            float startPitch = currentAudioPitch;

            if (transition.Duration <= 0f)
            {
                ApplyTimeScaleImmediate(transition.TargetTimeScale);
                ApplyAudioPitchImmediate(transition.TargetPitch);
                CompleteTransition(transition.Mode);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < transition.Duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / transition.Duration);
                float eased = transition.Curve.Evaluate(t);

                ApplyTimeScaleImmediate(Mathf.LerpUnclamped(startScale, transition.TargetTimeScale, eased));
                ApplyAudioPitchImmediate(Mathf.Lerp(startPitch, transition.TargetPitch, t));

                yield return null;
            }

            ApplyTimeScaleImmediate(transition.TargetTimeScale);
            ApplyAudioPitchImmediate(transition.TargetPitch);
            CompleteTransition(transition.Mode);
        }

        private void CompleteTransition(TransitionMode mode)
        {
            activeTransition = null;
            pendingTransition = new PendingTransition
            {
                HasValue = false,
                Mode = TransitionMode.None
            };

            if (mode == TransitionMode.Exit)
            {
                ApplyTimeScaleImmediate(storedNormalTimeScale);
                ApplyAudioPitchImmediate(1f);
            }
        }

        private void StopActiveTransition()
        {
            if (activeTransition == null)
                return;

            StopCoroutine(activeTransition);
            activeTransition = null;
        }

        private void ApplyTimeScaleImmediate(float scale)
        {
            scale = Mathf.Clamp(scale, 0f, 1f);
            Time.timeScale = scale;
            Time.fixedDeltaTime = baseFixedDeltaTime * scale;

            if (!isActive && Mathf.Approximately(scale, storedNormalTimeScale))
                Time.fixedDeltaTime = baseFixedDeltaTime * storedNormalTimeScale;
        }

        private void ApplyAudioPitchImmediate(float targetPitch)
        {
            currentAudioPitch = Mathf.Max(0.05f, targetPitch);

            if (gameMixer != null && !string.IsNullOrWhiteSpace(pitchParameter))
                gameMixer.SetFloat(pitchParameter, currentAudioPitch);

            for (int i = fallbackAudioSources.Count - 1; i >= 0; i--)
            {
                if (fallbackAudioSources[i] == null)
                {
                    fallbackAudioSources.RemoveAt(i);
                    continue;
                }

                fallbackAudioSources[i].pitch = currentAudioPitch;
            }
        }

        private void OnGamePaused(GamePausedEvent _)
        {
            isPausedByPauseSystem = true;
            StopActiveTransition();
        }

        private void OnGameResumed(GameResumedEvent _)
        {
            isPausedByPauseSystem = false;

            if (pendingTransition.HasValue)
            {
                activeTransition = StartCoroutine(TransitionRoutine(pendingTransition));
                return;
            }

            if (isActive && activeConfig != null)
            {
                ApplyTimeScaleImmediate(activeConfig.TimeScale);
                ApplyAudioPitchImmediate(activeConfig.AudioPitchScale);
            }
            else
            {
                ApplyTimeScaleImmediate(storedNormalTimeScale);
                ApplyAudioPitchImmediate(1f);
            }
        }

        private bool IsPaused()
        {
            return isPausedByPauseSystem || (PauseSystem_UMFOSS.Instance != null && PauseSystem_UMFOSS.Instance.IsPaused);
        }

        private SlowMoConfig_UMFOSS ResolveConfig(SlowMoConfig_UMFOSS config)
        {
            return config != null ? config : defaultConfig;
        }

        private SlowMoConfig_UMFOSS GetRelevantConfig()
        {
            if (activeConfig != null)
                return activeConfig;

            if (defaultConfig != null)
                return defaultConfig;

            return null;
        }

        private void EnsureResourceInitialized(SlowMoConfig_UMFOSS config)
        {
            if (config == null)
                return;

            if (!resourceInitialized)
            {
                currentResource = config.MaxResource;
                resourceInitialized = true;
            }

            currentResource = Mathf.Clamp(currentResource, 0f, config.MaxResource);
        }

        private float GetResourceMax()
        {
            SlowMoConfig_UMFOSS config = GetRelevantConfig();
            return config != null ? config.MaxResource : 1f;
        }
    }
}
