using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.CurrencySystem
{
    /// <summary>
    /// Bootstraps the entire Currency System demo at runtime.
    /// Attach to an empty GameObject in a scene that already has a Camera and EventSystem.
    /// </summary>
    public class DemoSceneSetup : MonoBehaviour
    {
        private CurrencySystem_UMFOSS currencySystem;

        private TextMeshProUGUI goldText;
        private TextMeshProUGUI gemsText;
        private TextMeshProUGUI xpText;
        private TextMeshProUGUI eventLogText;

        private const int MAX_LOG_ENTRIES = 5;
        private const string SAVE_KEY = "CurrencySystem_DemoSave";
        private readonly List<string> eventLog = new List<string>();

        private void Start()
        {
            SetupCurrencySystem();
            BuildUI();
            SubscribeToEvents();
            UpdateAllDisplays();
        }

        private void OnDestroy()
        {
            if (currencySystem == null) return;
            currencySystem.OnCurrencyEarned    -= HandleEarned;
            currencySystem.OnCurrencySpent     -= HandleSpent;
            currencySystem.OnInsufficientFunds -= HandleInsufficient;
            currencySystem.OnCapReached        -= HandleCap;
            currencySystem.OnAllBalancesReset  -= HandleReset;
        }

        // ---- Setup ----

        private void SetupCurrencySystem()
        {
            var config = ScriptableObject.CreateInstance<CurrencyConfig_UMFOSS>();
            config.currencies = new[]
            {
                new CurrencyConfig_UMFOSS.CurrencyEntry
                {
                    type = CurrencyType_UMFOSS.Gold,
                    displayName = "Gold",
                    startingBalance = 100,
                    maxBalance = 9999,
                    canGoNegative = false
                },
                new CurrencyConfig_UMFOSS.CurrencyEntry
                {
                    type = CurrencyType_UMFOSS.Gems,
                    displayName = "Gems",
                    startingBalance = 10,
                    maxBalance = 999,
                    canGoNegative = false
                },
                new CurrencyConfig_UMFOSS.CurrencyEntry
                {
                    type = CurrencyType_UMFOSS.XP,
                    displayName = "XP",
                    startingBalance = 0,
                    maxBalance = 0,
                    canGoNegative = false
                }
            };

            // Disable the GO so AddComponent doesn't trigger Awake() before config is set
            var csGO = new GameObject("CurrencySystem");
            csGO.SetActive(false);

            currencySystem = csGO.AddComponent<CurrencySystem_UMFOSS>();

            // Inject config via reflection before Awake runs
            var configField = typeof(CurrencySystem_UMFOSS)
                .GetField("config", BindingFlags.NonPublic | BindingFlags.Instance);
            configField.SetValue(currencySystem, config);

            // Now activate — this triggers Awake() with config already in place
            csGO.SetActive(true);
        }

        // ---- UI Construction ----

        private void BuildUI()
        {
            var canvas = CreateCanvas();
            var root = CreateVerticalPanel(canvas.transform, "Root",
                TextAnchor.UpperCenter, new Vector2(0, 0), new Vector2(1, 1),
                padding: 20, spacing: 12);

            // Title
            CreateText(root, "Title", "Currency System Demo", 28, FontStyles.Bold, Color.white);

            // Separator
            CreateSeparator(root);

            // --- Balance display row ---
            var balanceRow = CreateHorizontalPanel(root, "BalanceRow", spacing: 40);
            goldText = CreateText(balanceRow, "GoldText", "Gold: 100 / 9999", 22, FontStyles.Normal, new Color(1f, 0.84f, 0f));
            gemsText = CreateText(balanceRow, "GemsText", "Gems: 10 / 999", 22, FontStyles.Normal, new Color(0.3f, 0.85f, 1f));
            xpText   = CreateText(balanceRow, "XPText",   "XP: 0", 22, FontStyles.Normal, new Color(0.6f, 1f, 0.6f));

            CreateSeparator(root);

            // --- Earn buttons row ---
            var earnRow = CreateHorizontalPanel(root, "EarnRow", spacing: 10);
            CreateButton(earnRow, "Earn 50 Gold",  new Color(0.85f, 0.65f, 0.13f), () => currencySystem.Earn(CurrencyType_UMFOSS.Gold, 50));
            CreateButton(earnRow, "Earn 5 Gems",   new Color(0.2f,  0.6f,  0.9f),  () => currencySystem.Earn(CurrencyType_UMFOSS.Gems, 5));
            CreateButton(earnRow, "Earn 100 XP",   new Color(0.3f,  0.75f, 0.3f),  () => currencySystem.Earn(CurrencyType_UMFOSS.XP, 100));

            // --- Spend buttons row ---
            var spendRow = CreateHorizontalPanel(root, "SpendRow", spacing: 10);
            CreateButton(spendRow, "Spend 30 Gold", new Color(0.7f, 0.5f, 0.1f),  () => currencySystem.Spend(CurrencyType_UMFOSS.Gold, 30));
            CreateButton(spendRow, "Spend 20 Gems", new Color(0.15f, 0.45f, 0.7f), () => currencySystem.Spend(CurrencyType_UMFOSS.Gems, 20));

            // --- Trade + Reset row ---
            var tradeRow = CreateHorizontalPanel(root, "TradeRow", spacing: 10);
            CreateButton(tradeRow, "Trade 10 Gems \u2192 200 Gold", new Color(0.6f, 0.4f, 0.8f),
                () => currencySystem.Transaction(CurrencyType_UMFOSS.Gold, 200, CurrencyType_UMFOSS.Gems, 10));
            CreateButton(tradeRow, "Reset All", new Color(0.8f, 0.25f, 0.25f), () => currencySystem.ResetAll());

            // --- Save / Load row ---
            var saveRow = CreateHorizontalPanel(root, "SaveRow", spacing: 10);
            CreateButton(saveRow, "Save", new Color(0.3f, 0.6f, 0.3f), Save);
            CreateButton(saveRow, "Load", new Color(0.3f, 0.5f, 0.65f), Load);

            CreateSeparator(root);

            // --- Event log ---
            CreateText(root, "LogHeader", "Event Log", 20, FontStyles.Bold, Color.white);
            eventLogText = CreateText(root, "EventLog", "(no events yet)", 16, FontStyles.Italic, new Color(0.85f, 0.85f, 0.85f));
            eventLogText.alignment = TextAlignmentOptions.TopLeft;

            // Give the event log a minimum height
            var logLayout = eventLogText.gameObject.AddComponent<LayoutElement>();
            logLayout.minHeight = 120;
            logLayout.flexibleWidth = 1;
        }

        // ---- Events ----

        private void SubscribeToEvents()
        {
            currencySystem.OnCurrencyEarned    += HandleEarned;
            currencySystem.OnCurrencySpent     += HandleSpent;
            currencySystem.OnInsufficientFunds += HandleInsufficient;
            currencySystem.OnCapReached        += HandleCap;
            currencySystem.OnAllBalancesReset  += HandleReset;
        }

        private void HandleEarned(CurrencyType_UMFOSS type, int amount, int newBalance)
        {
            Log($"OnCurrencyEarned \u2014 {type} +{amount} \u2014 Balance: {newBalance}");
            UpdateAllDisplays();
        }

        private void HandleSpent(CurrencyType_UMFOSS type, int amount, int newBalance)
        {
            Log($"OnCurrencySpent \u2014 {type} -{amount} \u2014 Balance: {newBalance}");
            UpdateAllDisplays();
        }

        private void HandleInsufficient(CurrencyType_UMFOSS type, int required, int available)
        {
            Log($"OnInsufficientFunds \u2014 {type} required: {required}, available: {available}");
        }

        private void HandleCap(CurrencyType_UMFOSS type, int cap)
        {
            Log($"OnCapReached \u2014 {type} capped at {cap}");
        }

        private void HandleReset()
        {
            Log("OnAllBalancesReset \u2014 all currencies back to starting values");
            UpdateAllDisplays();
        }

        // ---- Display ----

        private void UpdateAllDisplays()
        {
            int goldMax = currencySystem.GetMaxBalance(CurrencyType_UMFOSS.Gold);
            goldText.text = goldMax > 0
                ? $"Gold: {currencySystem.GetBalance(CurrencyType_UMFOSS.Gold)} / {goldMax}"
                : $"Gold: {currencySystem.GetBalance(CurrencyType_UMFOSS.Gold)}";

            int gemsMax = currencySystem.GetMaxBalance(CurrencyType_UMFOSS.Gems);
            gemsText.text = gemsMax > 0
                ? $"Gems: {currencySystem.GetBalance(CurrencyType_UMFOSS.Gems)} / {gemsMax}"
                : $"Gems: {currencySystem.GetBalance(CurrencyType_UMFOSS.Gems)}";

            int xpMax = currencySystem.GetMaxBalance(CurrencyType_UMFOSS.XP);
            xpText.text = xpMax > 0
                ? $"XP: {currencySystem.GetBalance(CurrencyType_UMFOSS.XP)} / {xpMax}"
                : $"XP: {currencySystem.GetBalance(CurrencyType_UMFOSS.XP)}";
        }

        private void Log(string entry)
        {
            eventLog.Add(entry);
            while (eventLog.Count > MAX_LOG_ENTRIES)
                eventLog.RemoveAt(0);
            eventLogText.text = string.Join("\n", eventLog);
        }

        // ---- Save / Load ----

        private void Save()
        {
            var state = (CurrencySystem_UMFOSS.CurrencySaveData)currencySystem.CaptureState();
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
            Log("SAVED \u2014 balances written to PlayerPrefs");
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                Log("LOAD FAILED \u2014 no save data found");
                return;
            }

            var data = JsonUtility.FromJson<CurrencySystem_UMFOSS.CurrencySaveData>(
                PlayerPrefs.GetString(SAVE_KEY));
            if (data == null || data.entries == null) return;

            currencySystem.RestoreState(data);
            Log("LOADED \u2014 balances restored from PlayerPrefs");
            UpdateAllDisplays();
        }

        // ---- UI Factory Helpers ----

        private Canvas CreateCanvas()
        {
            var go = new GameObject("DemoCanvas");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private RectTransform CreateVerticalPanel(Transform parent, string name,
            TextAnchor childAlignment, Vector2 anchorMin, Vector2 anchorMax,
            int padding = 0, int spacing = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;

            var vlg = go.AddComponent<VerticalLayoutGroup>();
            vlg.childAlignment = childAlignment;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.spacing = spacing;
            vlg.padding = new RectOffset(padding, padding, padding, padding);

            var fitter = go.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Dark background
            var bg = go.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.15f, 0.95f);

            return rt;
        }

        private RectTransform CreateHorizontalPanel(Transform parent, string name, int spacing = 0)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();

            var hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.spacing = spacing;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 45;
            layout.flexibleWidth = 1;

            return rt;
        }

        private TextMeshProUGUI CreateText(Transform parent, string name, string text,
            int fontSize, FontStyles style, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.enableAutoSizing = false;

            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = fontSize + 10;
            layout.flexibleWidth = 1;

            return tmp;
        }

        private Button CreateButton(Transform parent, string label, Color bgColor,
            UnityEngine.Events.UnityAction onClick)
        {
            var go = new GameObject(label, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 40);

            var img = go.AddComponent<Image>();
            img.color = bgColor;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            var colors = btn.colors;
            colors.highlightedColor = bgColor * 1.15f;
            colors.pressedColor = bgColor * 0.8f;
            btn.colors = colors;

            btn.onClick.AddListener(onClick);

            // Button label
            var textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            var textRT = textGO.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(5, 0);
            textRT.offsetMax = new Vector2(-5, 0);

            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 16;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        private void CreateSeparator(Transform parent)
        {
            var go = new GameObject("Separator", typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(1, 1, 1, 0.15f);
            var layout = go.AddComponent<LayoutElement>();
            layout.preferredHeight = 2;
            layout.flexibleWidth = 1;
        }
    }
}
