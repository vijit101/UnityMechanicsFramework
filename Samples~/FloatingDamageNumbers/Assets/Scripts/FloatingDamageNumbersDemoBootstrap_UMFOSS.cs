using System.Collections;
using GameplayMechanicsUMFOSS.Combat;
using GameplayMechanicsUMFOSS.UI;
using GameplayMechanicsUMFOSS.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace GameplayMechanicsUMFOSS.Samples.FloatingDamageNumbers
{
    /// <summary>
    /// Builds a fully playable floating damage number demo scene at runtime from one bootstrap object.
    /// </summary>
    public class FloatingDamageNumbersDemoBootstrap_UMFOSS : MonoBehaviour
    {
        private const float PanelWidth = 380f;
        private const float ButtonHeight = 40f;
        private const float ButtonSpacing = 8f;
        private const float EntityY = 1f;

        private FloatingDamageNumbers_UMFOSS manager;
        private HealthSystem_UMFOSS playerHealth;
        private HealthSystem_UMFOSS enemyHealth;
        private HealthSystem_UMFOSS chestHealth;

        private TextMeshProUGUI poolUsageLabel;
        private TextMeshProUGUI combineStateLabel;
        private TextMeshProUGUI curveStateLabel;
        private TextMeshProUGUI playerHealthLabel;
        private TextMeshProUGUI enemyHealthLabel;
        private TextMeshProUGUI chestHealthLabel;

        private string currentCurveName = "Smooth";
        private bool isBuilt;

        private void Start()
        {
            if (isBuilt)
            {
                return;
            }

            BuildDemo();
        }

        private void Update()
        {
            if (manager == null || poolUsageLabel == null)
            {
                return;
            }

            ObjectPoolManager_UMFOSS.PoolStats stats = manager.GetPoolStats();
            poolUsageLabel.text = "Pool Usage: " + stats.ActiveCount + " / " + stats.TotalCount;
            combineStateLabel.text = "Combine Rapid: " + (manager.CombineRapid ? "ON" : "OFF") + "  Window: " + manager.RapidWindow.ToString("F2") + "s";
            curveStateLabel.text = "Curve Preset: " + currentCurveName;
            RefreshHealthLabels();
        }

        private void BuildDemo()
        {
            isBuilt = true;

            EnsureMainCamera();
            EnsureDirectionalLight();
            EnsureEventSystem();
            CreateEnvironment();
            CreateManager();
            CreateOverlayUi();
            ApplySmoothCurve();
        }

        private void EnsureMainCamera()
        {
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0f, 3.5f, -12f);
                Camera.main.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
                return;
            }

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 3.5f, -12f);
            cameraObject.transform.rotation = Quaternion.Euler(12f, 0f, 0f);

            Camera cameraComponent = cameraObject.GetComponent<Camera>();
            cameraComponent.backgroundColor = new Color(0.08f, 0.1f, 0.14f);
            cameraComponent.clearFlags = CameraClearFlags.SolidColor;
        }

        private void EnsureDirectionalLight()
        {
            if (FindObjectOfType<Light>() != null)
            {
                return;
            }

            GameObject lightObject = new GameObject("Directional Light", typeof(Light));
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            Light lightComponent = lightObject.GetComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.intensity = 1.2f;
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            DontDestroyOnLoad(eventSystemObject);
        }

        private void CreateEnvironment()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "DemoFloor";
            floor.transform.position = Vector3.zero;
            floor.transform.localScale = new Vector3(2.4f, 1f, 1.4f);
            floor.GetComponent<Renderer>().material.color = new Color(0.18f, 0.21f, 0.24f);

            playerHealth = CreateEntity("Player", PrimitiveType.Capsule, new Vector3(-4f, EntityY, 4f), new Color(0.42f, 0.77f, 1f));
            enemyHealth = CreateEntity("Enemy", PrimitiveType.Cube, new Vector3(0f, EntityY, 6f), new Color(1f, 0.52f, 0.35f));
            chestHealth = CreateEntity("Chest", PrimitiveType.Cylinder, new Vector3(4f, 0.8f, 4.5f), new Color(0.84f, 0.66f, 0.32f));
        }

        private void CreateManager()
        {
            GameObject managerObject = new GameObject("FloatingDamageNumbersManager");
            manager = managerObject.AddComponent<FloatingDamageNumbers_UMFOSS>();
            manager.CombineRapid = false;
            manager.RapidWindow = 0.1f;
            manager.DecimalPlaces = 0;
        }

        private void CreateOverlayUi()
        {
            GameObject canvasObject = new GameObject("DemoUI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            RectTransform panel = CreatePanel(canvas.transform);
            CreateLabel(panel, "Floating Damage Numbers Demo", 30, FontStyles.Bold);
            CreateLabel(panel, "One manager listens to EventBus events. Player, Enemy, and Chest only have HealthSystem_UMFOSS.", 18, FontStyles.Normal);

            poolUsageLabel = CreateLabel(panel, "Pool Usage: 0 / 0", 18, FontStyles.Bold);
            combineStateLabel = CreateLabel(panel, "Combine Rapid: OFF", 18, FontStyles.Bold);
            curveStateLabel = CreateLabel(panel, "Curve Preset: Smooth", 18, FontStyles.Bold);
            playerHealthLabel = CreateLabel(panel, "Player HP: 100 / 100", 18, FontStyles.Bold);
            enemyHealthLabel = CreateLabel(panel, "Enemy HP: 100 / 100", 18, FontStyles.Bold);
            chestHealthLabel = CreateLabel(panel, "Chest HP: 100 / 100", 18, FontStyles.Bold);
            RefreshHealthLabels();

            CreateButton(panel, "Deal 25 Damage To Player", () => playerHealth.ApplyDamage(25f, DamagePresentation.DamageTaken));
            CreateButton(panel, "Deal 100 Damage To Player", () => playerHealth.ApplyDamage(100f, DamagePresentation.DamageTaken));
            CreateButton(panel, "Heal Player 30", () => playerHealth.Heal(30f));
            CreateButton(panel, "Deal 15 Damage To Enemy", () => enemyHealth.ApplyDamage(15f, DamagePresentation.Damage));
            CreateButton(panel, "Critical Hit Enemy 80", () => enemyHealth.ApplyCriticalDamage(80f));
            CreateButton(panel, "Miss Enemy", () => enemyHealth.RegisterMiss());
            CreateButton(panel, "Shield Block Player 20", () => playerHealth.BlockDamage(20f));
            CreateButton(panel, "Poison Tick Chest 12", () => chestHealth.ApplyDamage(12f, DamagePresentation.PoisonDamage));
            CreateButton(panel, "Gain 40 XP", () => playerHealth.GainExperience(40f));
            CreateButton(panel, "Rapid Fire (5 x 10)", () =>
            {
                manager.CombineRapid = false;
                StartCoroutine(RapidFireRoutine(false));
            });
            CreateButton(panel, "Enable Combine + Rapid Fire", () =>
            {
                manager.CombineRapid = true;
                StartCoroutine(RapidFireRoutine(true));
            });
            CreateButton(panel, "Curve Preset: Smooth", ApplySmoothCurve);
            CreateButton(panel, "Curve Preset: Bounce", ApplyBounceCurve);
            CreateButton(panel, "Curve Preset: Elastic", ApplyElasticCurve);
            CreateButton(panel, "Reset Health", ResetAllHealth);
        }

        private HealthSystem_UMFOSS CreateEntity(string entityName, PrimitiveType primitiveType, Vector3 position, Color colour)
        {
            GameObject entityObject = GameObject.CreatePrimitive(primitiveType);
            entityObject.name = entityName;
            entityObject.transform.position = position;
            entityObject.GetComponent<Renderer>().material.color = colour;

            if (primitiveType == PrimitiveType.Cube)
            {
                entityObject.transform.localScale = new Vector3(1.5f, 2f, 1.2f);
            }
            else if (primitiveType == PrimitiveType.Cylinder)
            {
                entityObject.transform.localScale = new Vector3(1.2f, 0.8f, 1.2f);
            }

            GameObject labelObject = new GameObject(entityName + "_Label", typeof(TextMeshPro));
            labelObject.transform.SetParent(entityObject.transform, false);
            labelObject.transform.localPosition = Vector3.up * 1.6f;

            TextMeshPro labelText = labelObject.GetComponent<TextMeshPro>();
            labelText.text = entityName;
            labelText.fontSize = 4f;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;
            if (TMP_Settings.defaultFontAsset != null)
            {
                labelText.font = TMP_Settings.defaultFontAsset;
            }

            HealthSystem_UMFOSS healthSystem = entityObject.AddComponent<HealthSystem_UMFOSS>();
            return healthSystem;
        }

        private RectTransform CreatePanel(Transform parent)
        {
            GameObject panelObject = new GameObject("ControlPanel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            panelObject.transform.SetParent(parent, false);

            Image panelImage = panelObject.GetComponent<Image>();
            panelImage.color = new Color(0.08f, 0.1f, 0.12f, 0.84f);

            RectTransform rectTransform = panelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0f, 1f);
            rectTransform.anchorMax = new Vector2(0f, 1f);
            rectTransform.pivot = new Vector2(0f, 1f);
            rectTransform.anchoredPosition = new Vector2(20f, -20f);
            rectTransform.sizeDelta = new Vector2(PanelWidth, 0f);

            VerticalLayoutGroup layoutGroup = panelObject.GetComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(16, 16, 16, 16);
            layoutGroup.spacing = ButtonSpacing;
            layoutGroup.childAlignment = TextAnchor.UpperLeft;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandHeight = false;

            ContentSizeFitter fitter = panelObject.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            return rectTransform;
        }

        private TextMeshProUGUI CreateLabel(RectTransform parent, string text, float fontSize, FontStyles fontStyle)
        {
            GameObject labelObject = new GameObject(text.Replace(" ", string.Empty) + "_Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            labelObject.transform.SetParent(parent, false);

            LayoutElement layoutElement = labelObject.GetComponent<LayoutElement>();

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.overflowMode = TextOverflowModes.Overflow;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Left;
            if (TMP_Settings.defaultFontAsset != null)
            {
                label.font = TMP_Settings.defaultFontAsset;
            }

            float preferredHeight = label.GetPreferredValues(text, PanelWidth - 32f, 0f).y;
            layoutElement.preferredHeight = Mathf.Ceil(preferredHeight) + 8f;

            return label;
        }

        private void CreateButton(RectTransform parent, string label, UnityEngine.Events.UnityAction action)
        {
            GameObject buttonObject = new GameObject(label.Replace(" ", string.Empty) + "_Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
            buttonObject.transform.SetParent(parent, false);

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.32f, 0.42f, 0.95f);

            LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
            layoutElement.preferredHeight = ButtonHeight;

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.sizeDelta = new Vector2(PanelWidth - 32f, ButtonHeight);

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);

            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            TextMeshProUGUI buttonLabel = labelObject.GetComponent<TextMeshProUGUI>();
            buttonLabel.text = label;
            buttonLabel.fontSize = 18f;
            buttonLabel.color = Color.white;
            buttonLabel.alignment = TextAlignmentOptions.Center;
            if (TMP_Settings.defaultFontAsset != null)
            {
                buttonLabel.font = TMP_Settings.defaultFontAsset;
            }
        }

        private IEnumerator RapidFireRoutine(bool keepCombineEnabled)
        {
            for (int hitIndex = 0; hitIndex < 5; hitIndex++)
            {
                enemyHealth.ApplyDamage(10f, DamagePresentation.Damage);
                yield return new WaitForSecondsRealtime(0.02f);
            }

            if (!keepCombineEnabled)
            {
                manager.CombineRapid = false;
            }
        }

        private void ApplySmoothCurve()
        {
            ApplyCurvePreset("Smooth", AnimationCurve.EaseInOut(0f, 0f, 1f, 1f));
        }

        private void ApplyBounceCurve()
        {
            AnimationCurve bounceCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 2.4f),
                new Keyframe(0.72f, 1.14f, 0f, 0f),
                new Keyframe(1f, 1f, -1.2f, 0f));
            ApplyCurvePreset("Bounce", bounceCurve);
        }

        private void ApplyElasticCurve()
        {
            AnimationCurve elasticCurve = new AnimationCurve(
                new Keyframe(0f, 0f, 0f, 3f),
                new Keyframe(0.55f, 1.2f, 0f, 0f),
                new Keyframe(0.82f, 0.94f, 0f, 0f),
                new Keyframe(1f, 1f, 0.4f, 0f));
            ApplyCurvePreset("Elastic", elasticCurve);
        }

        private void ApplyCurvePreset(string presetName, AnimationCurve movementCurve)
        {
            currentCurveName = presetName;
            if (manager == null || manager.Config == null || manager.Config.animations == null)
            {
                return;
            }

            for (int index = 0; index < manager.Config.animations.Length; index++)
            {
                FloatingNumberConfig_UMFOSS.AnimationStyle animationStyle = manager.Config.animations[index];
                animationStyle.movementCurve = new AnimationCurve(movementCurve.keys);
                animationStyle.fadeCurve = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            }
        }

        private void ResetAllHealth()
        {
            playerHealth.ResetHealth();
            enemyHealth.ResetHealth();
            chestHealth.ResetHealth();
            RefreshHealthLabels();
        }

        private void RefreshHealthLabels()
        {
            if (playerHealthLabel != null && playerHealth != null)
            {
                playerHealthLabel.text = "Player HP: " + FormatHealth(playerHealth);
            }

            if (enemyHealthLabel != null && enemyHealth != null)
            {
                enemyHealthLabel.text = "Enemy HP: " + FormatHealth(enemyHealth);
            }

            if (chestHealthLabel != null && chestHealth != null)
            {
                chestHealthLabel.text = "Chest HP: " + FormatHealth(chestHealth);
            }
        }

        private static string FormatHealth(HealthSystem_UMFOSS healthSystem)
        {
            return Mathf.RoundToInt(healthSystem.CurrentHealth) + " / " + Mathf.RoundToInt(healthSystem.MaxHealth);
        }
    }
}
