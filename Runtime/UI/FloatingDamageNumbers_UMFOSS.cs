using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.UI
{
    /// <summary>
    /// Listens to health-related events and displays pooled floating numbers for the entire scene.
    /// </summary>
    public class FloatingDamageNumbers_UMFOSS : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private FloatingNumberConfig_UMFOSS config;
        [SerializeField] private GameObject numberPrefab;

        [Header("Display Settings")]
        [SerializeField] private bool showDamage = true;
        [SerializeField] private bool showHealing = true;
        [SerializeField] private bool showCrits = true;
        [SerializeField] private int decimalPlaces = 0;
        [SerializeField] private bool combineRapid = false;
        [SerializeField] private float rapidWindow = 0.1f;

        [Header("Canvas Settings")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RenderMode renderMode = RenderMode.WorldSpace;

        private readonly FloatingNumberCombineBuffer_UMFOSS combineBuffer = new FloatingNumberCombineBuffer_UMFOSS();
        private readonly List<FloatingNumberCombineBuffer_UMFOSS.CombinedNumber> expiredCombinedNumbers = new List<FloatingNumberCombineBuffer_UMFOSS.CombinedNumber>();

        private GameObject runtimeNumberPrefab;
        private bool initialized;
        private bool ownsRuntimePrefab;
        private bool previousCombineRapid;

        public FloatingNumberConfig_UMFOSS Config => config;
        public bool CombineRapid
        {
            get => combineRapid;
            set => combineRapid = value;
        }

        public float RapidWindow
        {
            get => rapidWindow;
            set => rapidWindow = Mathf.Max(0.01f, value);
        }

        public int DecimalPlaces
        {
            get => decimalPlaces;
            set => decimalPlaces = Mathf.Max(0, value);
        }

        public RenderMode ActiveRenderMode => renderMode;
        public Canvas TargetCanvas => targetCanvas;

        private void Awake()
        {
            InitializeIfNeeded();
        }

        private void OnEnable()
        {
            InitializeIfNeeded();
            global::EventBus.Subscribe<DamageTakenEvent>(OnDamageTaken);
            global::EventBus.Subscribe<HealedEvent>(OnHealed);
            global::EventBus.Subscribe<CriticalHitEvent>(OnCriticalHit);
            global::EventBus.Subscribe<ShieldBlockEvent>(OnShieldBlock);
            global::EventBus.Subscribe<MissEvent>(OnMiss);
            global::EventBus.Subscribe<ExperienceGainedEvent>(OnExperienceGained);
        }

        private void Update()
        {
            if (!combineRapid && previousCombineRapid)
            {
                combineBuffer.Clear();
            }

            if (combineRapid)
            {
                FlushExpiredCombinedNumbers(Time.unscaledTime);
            }

            previousCombineRapid = combineRapid;
        }

        private void OnDisable()
        {
            global::EventBus.Unsubscribe<DamageTakenEvent>(OnDamageTaken);
            global::EventBus.Unsubscribe<HealedEvent>(OnHealed);
            global::EventBus.Unsubscribe<CriticalHitEvent>(OnCriticalHit);
            global::EventBus.Unsubscribe<ShieldBlockEvent>(OnShieldBlock);
            global::EventBus.Unsubscribe<MissEvent>(OnMiss);
            global::EventBus.Unsubscribe<ExperienceGainedEvent>(OnExperienceGained);
            combineBuffer.Clear();
        }

        private void OnDestroy()
        {
            if (ownsRuntimePrefab && runtimeNumberPrefab != null)
            {
                Destroy(runtimeNumberPrefab);
            }
        }

        /// <summary>
        /// Replaces the current configuration object and reapplies pool prewarm settings.
        /// </summary>
        public void SetConfiguration(FloatingNumberConfig_UMFOSS configuration)
        {
            config = configuration;
            initialized = false;
            InitializeIfNeeded();
        }

        /// <summary>
        /// Returns current pool usage for the popup prefab.
        /// </summary>
        public ObjectPoolManager_UMFOSS.PoolStats GetPoolStats()
        {
            InitializeIfNeeded();
            return runtimeNumberPrefab == null
                ? default
                : ObjectPoolManager_UMFOSS.Instance.GetStats(runtimeNumberPrefab);
        }

        /// <summary>
        /// Applies either world-space or screen-space positioning for an active popup.
        /// </summary>
        public void ApplyPopupPosition(FloatingNumber_UMFOSS floatingNumber, Vector3 worldPosition)
        {
            if (floatingNumber == null)
            {
                return;
            }

            if (renderMode == RenderMode.WorldSpace)
            {
                floatingNumber.RectTransform.position = worldPosition;
                FaceCamera(floatingNumber.RectTransform);
                return;
            }

            Camera renderCamera = ResolveCamera();
            Vector3 screenPoint = renderCamera != null ? renderCamera.WorldToScreenPoint(worldPosition) : worldPosition;
            floatingNumber.RectTransform.position = screenPoint;
        }

        /// <summary>
        /// Publishes the return event for one popup instance.
        /// </summary>
        public void NotifyNumberReturned(NumberType type)
        {
            global::EventBus.Publish(new FloatingNumberReturnedEvent(type));
        }

        private void InitializeIfNeeded()
        {
            if (initialized)
            {
                return;
            }

            if (config == null)
            {
                config = FloatingNumberConfig_UMFOSS.CreateDefault();
            }

            if (targetCanvas == null)
            {
                targetCanvas = CreateCanvas();
            }

            ownsRuntimePrefab = numberPrefab == null;
            runtimeNumberPrefab = numberPrefab != null ? numberPrefab : CreateRuntimeNumberPrefab();
            ObjectPoolManager_UMFOSS.Instance.Prewarm(runtimeNumberPrefab, Mathf.Max(1, config.poolSize), targetCanvas.transform);

            initialized = true;
            previousCombineRapid = combineRapid;
        }

        private void OnDamageTaken(DamageTakenEvent eventData)
        {
            if (!showDamage)
            {
                return;
            }

            NumberType numberType = MapDamageType(eventData.Presentation);
            QueueOrSpawn(eventData.Target, eventData.Amount, numberType);
        }

        private void OnHealed(HealedEvent eventData)
        {
            if (!showHealing)
            {
                return;
            }

            QueueOrSpawn(eventData.Target, eventData.Amount, NumberType.Heal);
        }

        private void OnCriticalHit(CriticalHitEvent eventData)
        {
            if (!showDamage || !showCrits)
            {
                return;
            }

            QueueOrSpawn(eventData.Target, eventData.Amount, NumberType.CriticalHit);
        }

        private void OnShieldBlock(ShieldBlockEvent eventData)
        {
            QueueOrSpawn(eventData.Target, eventData.Amount, NumberType.ShieldBlock);
        }

        private void OnMiss(MissEvent eventData)
        {
            SpawnImmediate(0f, NumberType.Miss, ResolveWorldPosition(eventData.Target));
        }

        private void OnExperienceGained(ExperienceGainedEvent eventData)
        {
            QueueOrSpawn(eventData.Target, eventData.Amount, NumberType.Experience);
        }

        private void QueueOrSpawn(Transform target, float amount, NumberType type)
        {
            Vector3 worldPosition = ResolveWorldPosition(target);
            if (!combineRapid || !IsCombinable(type))
            {
                SpawnImmediate(amount, type, worldPosition);
                return;
            }

            int targetInstanceId = target != null ? target.GetInstanceID() : 0;
            combineBuffer.Add(targetInstanceId, type, amount, worldPosition, Time.unscaledTime, rapidWindow);
        }

        private void FlushExpiredCombinedNumbers(float currentTime)
        {
            expiredCombinedNumbers.Clear();
            combineBuffer.CollectExpired(currentTime, expiredCombinedNumbers);

            for (int index = 0; index < expiredCombinedNumbers.Count; index++)
            {
                FloatingNumberCombineBuffer_UMFOSS.CombinedNumber combinedNumber = expiredCombinedNumbers[index];
                SpawnImmediate(combinedNumber.Amount, combinedNumber.Type, combinedNumber.Position);
            }
        }

        private void SpawnImmediate(float amount, NumberType type, Vector3 worldPosition)
        {
            InitializeIfNeeded();

            FloatingNumberConfig_UMFOSS.NumberStyle style = config.GetStyle(type);
            FloatingNumberConfig_UMFOSS.AnimationStyle animationStyle = config.GetAnimation(type);
            float fontSize = style.critThreshold > 0f && amount >= style.critThreshold
                ? style.fontSizeCrit
                : style.fontSize;

            string text = FloatingNumberFormatter_UMFOSS.Format(amount, type, decimalPlaces);
            GameObject pooledObject = ObjectPoolManager_UMFOSS.Instance.Get(runtimeNumberPrefab, targetCanvas.transform);
            if (pooledObject == null)
            {
                return;
            }

            Vector3 horizontalSpread = new Vector3(Random.Range(-animationStyle.randomOffset.x, animationStyle.randomOffset.x), 0f, 0f);
            Vector3 spawnPosition = worldPosition + config.spawnOffset + horizontalSpread;

            FloatingNumber_UMFOSS floatingNumber = pooledObject.GetComponent<FloatingNumber_UMFOSS>();
            floatingNumber.Setup(this, text, style.colour, fontSize, type, spawnPosition, animationStyle);
            floatingNumber.Animate();

            global::EventBus.Publish(new FloatingNumberSpawnedEvent(type, amount, worldPosition));
        }

        private Canvas CreateCanvas()
        {
            string canvasName = renderMode == RenderMode.WorldSpace
                ? "FloatingDamageNumbers_WorldCanvas"
                : "FloatingDamageNumbers_ScreenCanvas";

            GameObject canvasObject = new GameObject(canvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = renderMode == RenderMode.WorldSpace ? RenderMode.WorldSpace : RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            canvas.worldCamera = ResolveCamera();

            RectTransform rectTransform = canvas.GetComponent<RectTransform>();
            if (canvas.renderMode == RenderMode.WorldSpace)
            {
                rectTransform.sizeDelta = new Vector2(32f, 18f);
                rectTransform.localScale = Vector3.one * 0.01f;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return canvas;
        }

        private GameObject CreateRuntimeNumberPrefab()
        {
            GameObject popupObject = new GameObject("FloatingNumber_RuntimePrefab", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI), typeof(FloatingNumber_UMFOSS));
            popupObject.SetActive(false);
            popupObject.hideFlags = HideFlags.HideAndDontSave;

            RectTransform rectTransform = popupObject.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(280f, 90f);

            TextMeshProUGUI textMesh = popupObject.GetComponent<TextMeshProUGUI>();
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.raycastTarget = false;
            textMesh.textWrappingMode = TextWrappingModes.NoWrap;
            textMesh.overflowMode = TextOverflowModes.Overflow;
            textMesh.text = string.Empty;
            textMesh.fontSize = 36f;
            if (TMP_Settings.defaultFontAsset != null)
            {
                textMesh.font = TMP_Settings.defaultFontAsset;
            }

            return popupObject;
        }

        private void FaceCamera(Transform popupTransform)
        {
            Camera renderCamera = ResolveCamera();
            if (renderCamera == null)
            {
                return;
            }

            popupTransform.LookAt(
                popupTransform.position + renderCamera.transform.rotation * Vector3.forward,
                renderCamera.transform.rotation * Vector3.up);
        }

        private Camera ResolveCamera()
        {
            if (targetCanvas != null && targetCanvas.worldCamera != null)
            {
                return targetCanvas.worldCamera;
            }

            return Camera.main;
        }

        private static bool IsCombinable(NumberType type)
        {
            return type != NumberType.Miss;
        }

        private static NumberType MapDamageType(DamagePresentation presentation)
        {
            switch (presentation)
            {
                case DamagePresentation.DamageTaken:
                    return NumberType.DamageTaken;
                case DamagePresentation.PoisonDamage:
                    return NumberType.PoisonDamage;
                default:
                    return NumberType.Damage;
            }
        }

        private static Vector3 ResolveWorldPosition(Transform target)
        {
            return target != null ? target.position : Vector3.zero;
        }
    }
}
