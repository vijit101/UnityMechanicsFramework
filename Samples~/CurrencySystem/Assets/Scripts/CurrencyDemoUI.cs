using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GameplayMechanicsUMFOSS.Systems;

namespace GameplayMechanicsUMFOSS.Samples.CurrencySystem
{
    public class CurrencyDemoUI : MonoBehaviour
    {
        [Header("Currency Displays")]
        [SerializeField] private TextMeshProUGUI goldBalanceText;
        [SerializeField] private TextMeshProUGUI gemsBalanceText;
        [SerializeField] private TextMeshProUGUI xpBalanceText;

        [Header("Action Buttons")]
        [SerializeField] private Button earnGoldButton;
        [SerializeField] private Button earnGemsButton;
        [SerializeField] private Button earnXPButton;
        [SerializeField] private Button spendGoldButton;
        [SerializeField] private Button spendGemsButton;
        [SerializeField] private Button tradeGemsForGoldButton;
        [SerializeField] private Button resetAllButton;

        [Header("Save / Load")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button loadButton;

        [Header("Event Log")]
        [SerializeField] private TextMeshProUGUI eventLogText;

        private const int MAX_LOG_ENTRIES = 5;
        private const string SAVE_KEY = "CurrencySystem_DemoSave";
        private readonly List<string> eventLog = new List<string>();

        private void Start()
        {
            WireButtons();
            SubscribeToEvents();
            UpdateAllDisplays();
        }

        private void OnDestroy()
        {
            if (CurrencySystem_UMFOSS.Instance == null) return;
            var cs = CurrencySystem_UMFOSS.Instance;
            cs.OnCurrencyEarned    -= HandleEarned;
            cs.OnCurrencySpent     -= HandleSpent;
            cs.OnInsufficientFunds -= HandleInsufficient;
            cs.OnCapReached        -= HandleCap;
            cs.OnAllBalancesReset  -= HandleReset;
        }

        private void WireButtons()
        {
            earnGoldButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Earn(CurrencyType_UMFOSS.Gold, 50));
            earnGemsButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Earn(CurrencyType_UMFOSS.Gems, 5));
            earnXPButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Earn(CurrencyType_UMFOSS.XP, 100));
            spendGoldButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Spend(CurrencyType_UMFOSS.Gold, 30));
            spendGemsButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Spend(CurrencyType_UMFOSS.Gems, 20));
            tradeGemsForGoldButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.Transaction(
                    CurrencyType_UMFOSS.Gold, 200,
                    CurrencyType_UMFOSS.Gems, 10));
            resetAllButton.onClick.AddListener(() =>
                CurrencySystem_UMFOSS.Instance.ResetAll());

            saveButton.onClick.AddListener(Save);
            loadButton.onClick.AddListener(Load);
        }

        private void SubscribeToEvents()
        {
            var cs = CurrencySystem_UMFOSS.Instance;
            cs.OnCurrencyEarned    += HandleEarned;
            cs.OnCurrencySpent     += HandleSpent;
            cs.OnInsufficientFunds += HandleInsufficient;
            cs.OnCapReached        += HandleCap;
            cs.OnAllBalancesReset  += HandleReset;
        }

        private void HandleEarned(CurrencyType_UMFOSS type, int amount, int newBalance)
        {
            Log($"OnCurrencyEarned — {type} +{amount} — Balance: {newBalance}");
            UpdateAllDisplays();
        }

        private void HandleSpent(CurrencyType_UMFOSS type, int amount, int newBalance)
        {
            Log($"OnCurrencySpent — {type} -{amount} — Balance: {newBalance}");
            UpdateAllDisplays();
        }

        private void HandleInsufficient(CurrencyType_UMFOSS type, int required, int available)
        {
            Log($"OnInsufficientFunds — {type} required: {required}, available: {available}");
        }

        private void HandleCap(CurrencyType_UMFOSS type, int cap)
        {
            Log($"OnCapReached — {type} capped at {cap}");
        }

        private void HandleReset()
        {
            Log("OnAllBalancesReset — all currencies back to starting values");
            UpdateAllDisplays();
        }

        private void UpdateAllDisplays()
        {
            var cs = CurrencySystem_UMFOSS.Instance;

            int goldMax = cs.GetMaxBalance(CurrencyType_UMFOSS.Gold);
            goldBalanceText.text = goldMax > 0
                ? $"Gold: {cs.GetBalance(CurrencyType_UMFOSS.Gold)} / {goldMax}"
                : $"Gold: {cs.GetBalance(CurrencyType_UMFOSS.Gold)}";

            int gemsMax = cs.GetMaxBalance(CurrencyType_UMFOSS.Gems);
            gemsBalanceText.text = gemsMax > 0
                ? $"Gems: {cs.GetBalance(CurrencyType_UMFOSS.Gems)} / {gemsMax}"
                : $"Gems: {cs.GetBalance(CurrencyType_UMFOSS.Gems)}";

            int xpMax = cs.GetMaxBalance(CurrencyType_UMFOSS.XP);
            xpBalanceText.text = xpMax > 0
                ? $"XP: {cs.GetBalance(CurrencyType_UMFOSS.XP)} / {xpMax}"
                : $"XP: {cs.GetBalance(CurrencyType_UMFOSS.XP)}";
        }

        private void Log(string entry)
        {
            eventLog.Add(entry);
            while (eventLog.Count > MAX_LOG_ENTRIES)
                eventLog.RemoveAt(0);
            eventLogText.text = string.Join("\n", eventLog);
        }

        private void Save()
        {
            var state = (CurrencySystem_UMFOSS.CurrencySaveData)CurrencySystem_UMFOSS.Instance.CaptureState();
            PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(state));
            PlayerPrefs.Save();
            Log("SAVED — balances written to PlayerPrefs");
        }

        private void Load()
        {
            if (!PlayerPrefs.HasKey(SAVE_KEY))
            {
                Log("LOAD FAILED — no save data found");
                return;
            }

            var data = JsonUtility.FromJson<CurrencySystem_UMFOSS.CurrencySaveData>(PlayerPrefs.GetString(SAVE_KEY));
            if (data == null || data.entries == null) return;

            CurrencySystem_UMFOSS.Instance.RestoreState(data);
            Log("LOADED — balances restored from PlayerPrefs");
            UpdateAllDisplays();
        }
    }
}
