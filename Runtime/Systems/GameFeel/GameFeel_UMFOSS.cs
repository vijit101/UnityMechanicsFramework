using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using GameplayMechanicsUMFOSS.Core;
using GameplayMechanicsUMFOSS.Utils;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// A single MonoBehaviour that layers five independent visual feedback effects
    /// onto any GameObject via EventBus triggers. Each effect is controlled by a
    /// nested handler class with its own collapsible Inspector section.
    ///
    /// Effects: Hitpause, Squash and Stretch, Afterimage, Screen Flash, Ghost Trail.
    ///
    /// Attach this component to any GameObject. Subscribe to EventBus events from
    /// other mechanics — no direct references required.
    /// </summary>
    public class GameFeel_UMFOSS : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  1. HITPAUSE
        // ─────────────────────────────────────────────

        [Header("Hitpause")]
        [Tooltip("Enable or disable hitpause effect at runtime.")]
        [SerializeField] private bool hitpauseEnabled = true;

        [Tooltip("Duration of the pause in seconds. Sweet spot: 0.02 - 0.08s (1-4 frames at 60fps).")]
        [SerializeField, Range(0.01f, 0.2f)] private float hitpauseDuration = 0.05f;

        [Tooltip("Time scale during pause. 0 = full freeze, 0.1 = near-freeze.")]
        [SerializeField, Range(0f, 0.1f)] private float hitpauseTimeScale = 0f;

        // ─────────────────────────────────────────────
        //  2. SQUASH & STRETCH
        // ─────────────────────────────────────────────

        [Header("Squash & Stretch")]
        [Tooltip("Enable or disable squash and stretch effect at runtime.")]
        [SerializeField] private bool squashStretchEnabled = true;

        [Tooltip("The transform to apply squash and stretch to. Defaults to this GameObject's transform if null.")]
        [SerializeField] private Transform squashTarget;

        [Tooltip("Scale multiplier applied on jump start (stretch upward).")]
        [SerializeField] private Vector3 jumpStretchScale = new Vector3(0.8f, 1.3f, 1f);

        [Tooltip("Scale multiplier applied on landing (squash flat).")]
        [SerializeField] private Vector3 landSquashScale = new Vector3(1.3f, 0.7f, 1f);

        [Tooltip("Scale multiplier applied on taking damage.")]
        [SerializeField] private Vector3 hitSquashScale = new Vector3(1.2f, 0.8f, 1f);

        [Tooltip("Scale multiplier applied on item pickup (pop effect).")]
        [SerializeField] private Vector3 pickupPopScale = new Vector3(1.15f, 1.15f, 1f);

        [Tooltip("How fast the transform recovers to its original scale. Higher = snappier.")]
        [SerializeField, Range(1f, 30f)] private float squashRecoverySpeed = 12f;

        [Tooltip("Curve controlling recovery shape. Values above 1.0 produce springy overshoot.")]
        [SerializeField] private AnimationCurve squashRecoveryCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.4f, 1.15f),
            new Keyframe(0.7f, 0.95f),
            new Keyframe(1f, 1f)
        );

        // ─────────────────────────────────────────────
        //  3. AFTERIMAGE
        // ─────────────────────────────────────────────

        [Header("Afterimage")]
        [Tooltip("Enable or disable afterimage effect at runtime.")]
        [SerializeField] private bool afterimageEnabled = true;

        [Tooltip("SpriteRenderer on this GameObject used to copy the sprite for afterimages.")]
        [SerializeField] private SpriteRenderer sourceSprite;

        [Tooltip("Time in seconds between spawning each afterimage copy.")]
        [SerializeField, Range(0.01f, 0.2f)] private float afterimageSpawnInterval = 0.05f;

        [Tooltip("How long each afterimage lives before returning to the pool.")]
        [SerializeField, Range(0.1f, 2f)] private float afterimageLifetime = 0.4f;

        [Tooltip("Starting color of the afterimage (usually semi-transparent).")]
        [SerializeField] private Color afterimageStartColor = new Color(0.5f, 0.8f, 1f, 0.6f);

        [Tooltip("Ending color before the afterimage returns to pool.")]
        [SerializeField] private Color afterimageEndColor = new Color(0.5f, 0.8f, 1f, 0f);

        [Tooltip("Maximum afterimages alive simultaneously.")]
        [SerializeField, Range(1, 30)] private int afterimagePoolSize = 10;

        // ─────────────────────────────────────────────
        //  4. SCREEN FLASH
        // ─────────────────────────────────────────────

        [Header("Screen Flash")]
        [Tooltip("Enable or disable screen flash effect at runtime.")]
        [SerializeField] private bool screenFlashEnabled = true;

        [Tooltip("Default flash color for hit events.")]
        [SerializeField] private Color flashColorHit = new Color(1f, 1f, 1f, 0.4f);

        [Tooltip("Flash color for damage taken events.")]
        [SerializeField] private Color flashColorDamage = new Color(1f, 1f, 1f, 0.3f);

        [Tooltip("Flash color for death events.")]
        [SerializeField] private Color flashColorDeath = new Color(1f, 0f, 0f, 0.5f);

        [Tooltip("Flash color for item pickup events (subtle yellow).")]
        [SerializeField] private Color flashColorPickup = new Color(1f, 0.9f, 0.3f, 0.15f);

        [Tooltip("Duration of the flash in seconds.")]
        [SerializeField, Range(0.05f, 1f)] private float flashDuration = 0.15f;

        [Tooltip("Curve controlling flash alpha over its lifetime. Sharp spike + decay = impact feel.")]
        [SerializeField] private AnimationCurve flashCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.2f, 0.6f),
            new Keyframe(1f, 0f)
        );

        // ─────────────────────────────────────────────
        //  5. GHOST TRAIL
        // ─────────────────────────────────────────────

        [Header("Ghost Trail")]
        [Tooltip("Enable or disable ghost trail effect at runtime.")]
        [SerializeField] private bool ghostTrailEnabled = true;

        [Tooltip("Minimum velocity magnitude required to activate the trail.")]
        [SerializeField, Range(0.1f, 20f)] private float ghostTrailSpeedThreshold = 5f;

        [Tooltip("TrailRenderer component. Auto-assigned from this GameObject if null.")]
        [SerializeField] private TrailRenderer ghostTrailRenderer;

        [Tooltip("Trail lifetime in seconds when active.")]
        [SerializeField, Range(0.05f, 2f)] private float ghostTrailTime = 0.3f;

        [Tooltip("Start color of the ghost trail.")]
        [SerializeField] private Color ghostTrailStartColor = new Color(0.3f, 0.6f, 1f, 0.8f);

        [Tooltip("End color of the ghost trail (usually faded out).")]
        [SerializeField] private Color ghostTrailEndColor = new Color(0.3f, 0.6f, 1f, 0f);

        // ─────────────────────────────────────────────
        //  PRIVATE STATE
        // ─────────────────────────────────────────────

        private Vector3 originalScale;
        private Coroutine squashCoroutine;
        private Coroutine flashCoroutine;
        private Coroutine hitpauseCoroutine;
        private bool isAfterimageActive;
        private float afterimageTimer;
        private bool isDashGhostTrailActive;
        private float preHitpauseTimeScale = 1f;

        // Auto-created screen flash UI
        private Canvas flashCanvas;
        private Image flashImage;

        // Afterimage object pool
        private ObjectPoolManager_UMFOSS afterimagePool;
        private GameObject afterimagePrefab;

        // Velocity tracking for ghost trail threshold
        private Vector3 previousPosition;

        // ─────────────────────────────────────────────
        //  UNITY LIFECYCLE
        // ─────────────────────────────────────────────

        private void Awake()
        {
            CacheOriginalScale();
            CreateScreenFlashCanvas();
            CreateAfterimagePool();
            SetupGhostTrail();
        }

        private void OnEnable()
        {
            EventBus.Subscribe<OnHitRegistered>(HandleHitRegistered);
            EventBus.Subscribe<OnDamageTaken>(HandleDamageTaken);
            EventBus.Subscribe<OnDeath>(HandleDeath);
            EventBus.Subscribe<OnJumpStart>(HandleJumpStart);
            EventBus.Subscribe<OnLanding>(HandleLanding);
            EventBus.Subscribe<OnDashStart>(HandleDashStart);
            EventBus.Subscribe<OnDashEnd>(HandleDashEnd);
            EventBus.Subscribe<OnItemPickedUp>(HandleItemPickedUp);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<OnHitRegistered>(HandleHitRegistered);
            EventBus.Unsubscribe<OnDamageTaken>(HandleDamageTaken);
            EventBus.Unsubscribe<OnDeath>(HandleDeath);
            EventBus.Unsubscribe<OnJumpStart>(HandleJumpStart);
            EventBus.Unsubscribe<OnLanding>(HandleLanding);
            EventBus.Unsubscribe<OnDashStart>(HandleDashStart);
            EventBus.Unsubscribe<OnDashEnd>(HandleDashEnd);
            EventBus.Unsubscribe<OnItemPickedUp>(HandleItemPickedUp);

            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            // Clean up scene-root canvas that is not parented to this transform
            if (flashCanvas != null)
            {
                Destroy(flashCanvas.gameObject);
            }
        }

        private void Update()
        {
            UpdateAfterimage();
            UpdateGhostTrail();
            previousPosition = transform.position;
        }

        // ─────────────────────────────────────────────
        //  EVENT HANDLERS
        // ─────────────────────────────────────────────

        private void HandleHitRegistered(OnHitRegistered evt)
        {
            TriggerHitpause();
            TriggerScreenFlash(flashColorHit);
        }

        private void HandleDamageTaken(OnDamageTaken evt)
        {
            TriggerHitpause();
            TriggerSquashStretch(hitSquashScale);
            TriggerScreenFlash(flashColorDamage);
        }

        private void HandleDeath(OnDeath evt)
        {
            TriggerScreenFlash(flashColorDeath);
            TriggerSquashStretch(hitSquashScale);
        }

        private void HandleJumpStart(OnJumpStart evt)
        {
            TriggerSquashStretch(jumpStretchScale);
        }

        private void HandleLanding(OnLanding evt)
        {
            TriggerSquashStretch(landSquashScale);
        }

        private void HandleDashStart(OnDashStart evt)
        {
            isAfterimageActive = true;
            isDashGhostTrailActive = true;
        }

        private void HandleDashEnd(OnDashEnd evt)
        {
            isAfterimageActive = false;
            isDashGhostTrailActive = false;
        }

        private void HandleItemPickedUp(OnItemPickedUp evt)
        {
            TriggerSquashStretch(pickupPopScale);
            TriggerScreenFlash(flashColorPickup);
        }

        // ─────────────────────────────────────────────
        //  HITPAUSE HANDLER
        // ─────────────────────────────────────────────

        /// <summary>
        /// Freezes Time.timeScale for the configured duration, then restores it.
        /// Uses WaitForSecondsRealtime so the coroutine itself is not frozen
        /// when timeScale reaches zero.
        /// </summary>
        private void TriggerHitpause()
        {
            if (!hitpauseEnabled) return;

            // If a hitpause is already running, stop it and reuse the saved timeScale.
            // This prevents the race condition where a second coroutine captures
            // timeScale = 0 as its "previous" value and restores to 0 on exit.
            if (hitpauseCoroutine != null)
            {
                StopCoroutine(hitpauseCoroutine);
            }
            else
            {
                preHitpauseTimeScale = Time.timeScale;
            }

            hitpauseCoroutine = StartCoroutine(HitpauseCoroutine());
        }

        private IEnumerator HitpauseCoroutine()
        {
            Time.timeScale = hitpauseTimeScale;

            // WaitForSecondsRealtime measures wall-clock time, unaffected by timeScale.
            // WaitForSeconds would wait indefinitely when timeScale is 0.
            yield return new WaitForSecondsRealtime(hitpauseDuration);

            Time.timeScale = preHitpauseTimeScale;
            hitpauseCoroutine = null;
        }

        private void RestoreTimeScale()
        {
            // Safety: if a hitpause coroutine is running when this component is disabled,
            // restore to the pre-hitpause value instead of leaving timeScale frozen.
            if (hitpauseCoroutine != null)
            {
                StopCoroutine(hitpauseCoroutine);
                hitpauseCoroutine = null;
                Time.timeScale = preHitpauseTimeScale;
            }
        }

        // ─────────────────────────────────────────────
        //  SQUASH & STRETCH HANDLER
        // ─────────────────────────────────────────────

        /// <summary>
        /// Applies a target scale to the squash target, then recovers to the
        /// original scale along the recovery curve. Uses LerpUnclamped so that
        /// curve values above 1.0 produce springy overshoot.
        /// </summary>
        /// <param name="targetScaleMultiplier">Multiplied against originalScale to get the deformed scale.</param>
        private void TriggerSquashStretch(Vector3 targetScaleMultiplier)
        {
            if (!squashStretchEnabled) return;

            if (squashCoroutine != null)
            {
                StopCoroutine(squashCoroutine);
            }

            Transform target = squashTarget != null ? squashTarget : transform;
            Vector3 deformedScale = Vector3.Scale(originalScale, targetScaleMultiplier);
            squashCoroutine = StartCoroutine(SquashStretchCoroutine(target, deformedScale));
        }

        private IEnumerator SquashStretchCoroutine(Transform target, Vector3 deformedScale)
        {
            target.localScale = deformedScale;
            float elapsed = 0f;
            float duration = 1f / squashRecoverySpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / duration);
                float curveValue = squashRecoveryCurve.Evaluate(normalizedTime);

                // LerpUnclamped allows curveValue above 1.0 to overshoot past originalScale,
                // creating the springy, alive-feeling recovery that Lerp clamps away.
                target.localScale = Vector3.LerpUnclamped(deformedScale, originalScale, curveValue);
                yield return null;
            }

            target.localScale = originalScale;
            squashCoroutine = null;
        }

        // ─────────────────────────────────────────────
        //  AFTERIMAGE HANDLER
        // ─────────────────────────────────────────────

        /// <summary>
        /// When active, spawns pooled sprite copies at a fixed interval.
        /// Each copy fades from startColor to endColor over its lifetime,
        /// then returns itself to the pool. Zero runtime allocation.
        /// </summary>
        private void UpdateAfterimage()
        {
            if (!afterimageEnabled || !isAfterimageActive || sourceSprite == null) return;

            afterimageTimer -= Time.deltaTime;

            if (afterimageTimer <= 0f)
            {
                afterimageTimer = afterimageSpawnInterval;
                SpawnAfterimage();
            }
        }

        private void SpawnAfterimage()
        {
            if (afterimagePool == null) return;

            GameObject ghost = afterimagePool.Get(transform.position, transform.rotation);
            SpriteRenderer ghostRenderer = ghost.GetComponent<SpriteRenderer>();

            if (ghostRenderer != null)
            {
                ghostRenderer.sprite = sourceSprite.sprite;
                ghostRenderer.flipX = sourceSprite.flipX;
                ghostRenderer.flipY = sourceSprite.flipY;
                ghost.transform.localScale = transform.localScale;
            }

            StartCoroutine(FadeAndReturnAfterimage(ghost, ghostRenderer));
        }

        private IEnumerator FadeAndReturnAfterimage(GameObject ghost, SpriteRenderer renderer)
        {
            float elapsed = 0f;

            while (elapsed < afterimageLifetime)
            {
                elapsed += Time.deltaTime;
                float normalizedTime = elapsed / afterimageLifetime;

                if (renderer != null)
                {
                    renderer.color = Color.Lerp(afterimageStartColor, afterimageEndColor, normalizedTime);
                }

                yield return null;
            }

            if (afterimagePool != null)
            {
                afterimagePool.Return(ghost);
            }
        }

        // ─────────────────────────────────────────────
        //  SCREEN FLASH HANDLER
        // ─────────────────────────────────────────────

        /// <summary>
        /// Triggers a full-screen color overlay that fades according to the flash curve.
        /// The canvas and Image are auto-created on Awake — no manual scene setup required.
        /// </summary>
        /// <param name="color">The flash color including base alpha.</param>
        private void TriggerScreenFlash(Color color)
        {
            if (!screenFlashEnabled) return;

            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }

            flashCoroutine = StartCoroutine(ScreenFlashCoroutine(color));
        }

        private IEnumerator ScreenFlashCoroutine(Color color)
        {
            flashImage.gameObject.SetActive(true);
            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalizedTime = Mathf.Clamp01(elapsed / flashDuration);
                float curveAlpha = flashCurve.Evaluate(normalizedTime);

                Color currentColor = color;
                currentColor.a = color.a * curveAlpha;
                flashImage.color = currentColor;

                yield return null;
            }

            flashImage.color = Color.clear;
            flashImage.gameObject.SetActive(false);
            flashCoroutine = null;
        }

        // ─────────────────────────────────────────────
        //  GHOST TRAIL HANDLER
        // ─────────────────────────────────────────────

        /// <summary>
        /// Enables or disables the TrailRenderer based on velocity magnitude
        /// exceeding the configured threshold, or when a dash is active.
        /// </summary>
        private void UpdateGhostTrail()
        {
            if (!ghostTrailEnabled || ghostTrailRenderer == null) return;

            // Guard against division by zero on the first frame or when deltaTime is zero
            if (Time.deltaTime <= 0f) return;

            float velocity = (transform.position - previousPosition).magnitude / Time.deltaTime;
            bool shouldBeActive = isDashGhostTrailActive || velocity > ghostTrailSpeedThreshold;

            if (shouldBeActive && !ghostTrailRenderer.emitting)
            {
                ghostTrailRenderer.emitting = true;
            }
            else if (!shouldBeActive && ghostTrailRenderer.emitting)
            {
                ghostTrailRenderer.emitting = false;
            }
        }

        // ─────────────────────────────────────────────
        //  SETUP & INITIALIZATION
        // ─────────────────────────────────────────────

        private void CacheOriginalScale()
        {
            Transform target = squashTarget != null ? squashTarget : transform;
            originalScale = target.localScale;
        }

        /// <summary>
        /// Auto-creates a screen-space overlay canvas with a full-screen Image
        /// component for the flash effect. No manual scene setup required.
        /// Configured with the highest sort order and raycast disabled so it
        /// never blocks gameplay input.
        /// </summary>
        private void CreateScreenFlashCanvas()
        {
            // Placed at scene root (not parented to this transform) to prevent
            // squash/stretch deformations from distorting the flash overlay.
            // DontDestroyOnLoad is NOT used — the canvas lives and dies with the scene.
            GameObject canvasObj = new GameObject("GameFeel_ScreenFlashCanvas");

            flashCanvas = canvasObj.AddComponent<Canvas>();
            flashCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            flashCanvas.sortingOrder = 999;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            GameObject imageObj = new GameObject("FlashImage");
            imageObj.transform.SetParent(canvasObj.transform, false);

            flashImage = imageObj.AddComponent<Image>();
            flashImage.color = Color.clear;
            flashImage.raycastTarget = false;

            // Stretch the image to fill the entire screen
            RectTransform rect = flashImage.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            imageObj.SetActive(false);
        }

        /// <summary>
        /// Creates the afterimage object pool by generating a simple prefab
        /// with a SpriteRenderer at runtime. The pool is pre-warmed to avoid
        /// any runtime instantiation during gameplay.
        /// </summary>
        private void CreateAfterimagePool()
        {
            // Build a minimal afterimage prefab at runtime and parent it to this
            // transform so it is cleaned up when this GameObject is destroyed.
            afterimagePrefab = new GameObject("AfterimageTemplate");
            afterimagePrefab.transform.SetParent(transform);
            SpriteRenderer renderer = afterimagePrefab.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = -1;
            afterimagePrefab.SetActive(false);

            GameObject poolObj = new GameObject("GameFeel_AfterimagePool");
            poolObj.transform.SetParent(transform);
            afterimagePool = poolObj.AddComponent<ObjectPoolManager_UMFOSS>();
            afterimagePool.Initialize(afterimagePrefab, afterimagePoolSize);
        }

        private void SetupGhostTrail()
        {
            if (ghostTrailRenderer == null)
            {
                ghostTrailRenderer = GetComponent<TrailRenderer>();
            }

            if (ghostTrailRenderer != null)
            {
                ghostTrailRenderer.time = ghostTrailTime;
                ghostTrailRenderer.startColor = ghostTrailStartColor;
                ghostTrailRenderer.endColor = ghostTrailEndColor;
                ghostTrailRenderer.emitting = false;
            }

            previousPosition = transform.position;
        }

        // ─────────────────────────────────────────────
        //  PUBLIC API — MANUAL TRIGGERS
        // ─────────────────────────────────────────────

        /// <summary>
        /// Manually trigger hitpause without publishing an EventBus event.
        /// Useful for testing or direct integration.
        /// </summary>
        public void ManualHitpause() => TriggerHitpause();

        /// <summary>
        /// Manually trigger squash and stretch with a custom scale multiplier.
        /// </summary>
        /// <param name="scaleMultiplier">Multiplied against the original scale.</param>
        public void ManualSquashStretch(Vector3 scaleMultiplier) => TriggerSquashStretch(scaleMultiplier);

        /// <summary>
        /// Manually trigger a screen flash with a custom color.
        /// </summary>
        /// <param name="color">Flash color including alpha intensity.</param>
        public void ManualScreenFlash(Color color) => TriggerScreenFlash(color);

        /// <summary>
        /// Manually enable or disable afterimage spawning.
        /// </summary>
        /// <param name="active">True to start spawning, false to stop.</param>
        public void ManualSetAfterimageActive(bool active) => isAfterimageActive = active;

        /// <summary>
        /// Manually enable or disable the ghost trail.
        /// </summary>
        /// <param name="active">True to force trail on, false to return to threshold-based control.</param>
        public void ManualSetGhostTrailActive(bool active) => isDashGhostTrailActive = active;

        // ─────────────────────────────────────────────
        //  PUBLIC API — TOGGLE EFFECTS
        // ─────────────────────────────────────────────

        /// <summary>Set hitpause enabled state. When disabled, hitpause calls are no-ops with zero cost.</summary>
        public void SetHitpauseEnabled(bool enabled) => hitpauseEnabled = enabled;

        /// <summary>Set squash and stretch enabled state.</summary>
        public void SetSquashStretchEnabled(bool enabled) => squashStretchEnabled = enabled;

        /// <summary>Set afterimage enabled state.</summary>
        public void SetAfterimageEnabled(bool enabled) => afterimageEnabled = enabled;

        /// <summary>Set screen flash enabled state.</summary>
        public void SetScreenFlashEnabled(bool enabled) => screenFlashEnabled = enabled;

        /// <summary>Set ghost trail enabled state.</summary>
        public void SetGhostTrailEnabled(bool enabled) => ghostTrailEnabled = enabled;

        /// <summary>Enable or disable all five effects at once.</summary>
        public void SetAllEffectsEnabled(bool enabled)
        {
            hitpauseEnabled = enabled;
            squashStretchEnabled = enabled;
            afterimageEnabled = enabled;
            screenFlashEnabled = enabled;
            ghostTrailEnabled = enabled;
        }
    }
}
