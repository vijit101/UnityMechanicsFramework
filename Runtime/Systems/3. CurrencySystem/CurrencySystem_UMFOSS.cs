using System;
using System.Collections.Generic;
using UnityEngine;
using GameplayMechanicsUMFOSS.Core;

namespace GameplayMechanicsUMFOSS.Systems
{
    public class CurrencySystem_UMFOSS : MonoSingletongeneric<CurrencySystem_UMFOSS>, ISaveable_UMFOSS
    {
        [Header("Configuration")]
        [SerializeField] private CurrencyConfig_UMFOSS config;

        private Dictionary<CurrencyType_UMFOSS, int> balances = new Dictionary<CurrencyType_UMFOSS, int>();

        public event Action<CurrencyType_UMFOSS, int, int> OnCurrencyEarned;
        public event Action<CurrencyType_UMFOSS, int, int> OnCurrencySpent;
        public event Action<CurrencyType_UMFOSS, int, int> OnBalanceChanged;
        public event Action<CurrencyType_UMFOSS, int, int> OnInsufficientFunds;
        public event Action<CurrencyType_UMFOSS, int> OnCapReached;
        public event Action OnAllBalancesReset;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this) return;
            InitializeBalances();
        }

        public void Earn(CurrencyType_UMFOSS type, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencySystem] Earn called with non-positive amount ({amount}) for {type}.");
                return;
            }

            if (!ValidateType(type, "Earn")) return;

            int oldBalance = balances[type];
            int newBalance = oldBalance + amount;
            bool capped = false;

            var entry = config.GetEntry(type);
            if (entry != null && entry.maxBalance > 0 && newBalance > entry.maxBalance)
            {
                newBalance = entry.maxBalance;
                capped = true;
            }

            balances[type] = newBalance;

            OnBalanceChanged?.Invoke(type, oldBalance, newBalance);
            OnCurrencyEarned?.Invoke(type, newBalance - oldBalance, newBalance);

            if (capped)
                OnCapReached?.Invoke(type, entry.maxBalance);
        }

        public bool Spend(CurrencyType_UMFOSS type, int amount)
        {
            if (amount <= 0)
            {
                Debug.LogWarning($"[CurrencySystem] Spend called with non-positive amount ({amount}) for {type}.");
                return false;
            }

            if (!ValidateType(type, "Spend")) return false;

            var entry = config.GetEntry(type);
            bool canGoNegative = entry != null && entry.canGoNegative;

            if (!canGoNegative && balances[type] < amount)
            {
                OnInsufficientFunds?.Invoke(type, amount, balances[type]);
                return false;
            }

            int oldBalance = balances[type];
            balances[type] = oldBalance - amount;

            OnBalanceChanged?.Invoke(type, oldBalance, balances[type]);
            OnCurrencySpent?.Invoke(type, amount, balances[type]);

            return true;
        }

        public void SetBalance(CurrencyType_UMFOSS type, int amount)
        {
            if (!ValidateType(type, "SetBalance")) return;

            int oldBalance = balances[type];
            balances[type] = amount;
            OnBalanceChanged?.Invoke(type, oldBalance, amount);
        }

        public void ResetBalance(CurrencyType_UMFOSS type)
        {
            if (!ValidateType(type, "ResetBalance")) return;

            var entry = config.GetEntry(type);
            int oldBalance = balances[type];
            int newBalance = entry != null ? entry.startingBalance : 0;

            balances[type] = newBalance;
            OnBalanceChanged?.Invoke(type, oldBalance, newBalance);
        }

        public void ResetAll()
        {
            if (config == null || config.currencies == null) return;

            foreach (var entry in config.currencies)
                balances[entry.type] = entry.startingBalance;

            OnAllBalancesReset?.Invoke();
        }

        public bool Transaction(CurrencyType_UMFOSS earnType, int earnAmount,
                                CurrencyType_UMFOSS spendType, int spendAmount)
        {
            if (!balances.ContainsKey(earnType) || !balances.ContainsKey(spendType))
            {
                Debug.LogWarning($"[CurrencySystem] Transaction failed — {earnType} or {spendType} not configured.");
                return false;
            }

            if (!Spend(spendType, spendAmount))
                return false;

            Earn(earnType, earnAmount);
            return true;
        }

        public int GetBalance(CurrencyType_UMFOSS type) =>
            balances.TryGetValue(type, out int bal) ? bal : 0;

        public bool HasEnough(CurrencyType_UMFOSS type, int amount) =>
            GetBalance(type) >= amount;

        public bool IsAtCap(CurrencyType_UMFOSS type)
        {
            var entry = config.GetEntry(type);
            if (entry == null || entry.maxBalance <= 0) return false;
            return GetBalance(type) >= entry.maxBalance;
        }

        public int GetMaxBalance(CurrencyType_UMFOSS type)
        {
            var entry = config.GetEntry(type);
            return entry != null ? entry.maxBalance : 0;
        }

        public string GetDisplayName(CurrencyType_UMFOSS type)
        {
            var entry = config.GetEntry(type);
            return entry != null ? entry.displayName : string.Empty;
        }

        public Sprite GetIcon(CurrencyType_UMFOSS type)
        {
            var entry = config.GetEntry(type);
            return entry != null ? entry.icon : null;
        }

        // --- ISaveable_UMFOSS ---

        public string GetSaveID() => "CurrencySystem";

        public object CaptureState()
        {
            var data = new CurrencySaveData
            {
                entries = new List<CurrencySaveEntry>(balances.Count)
            };

            foreach (var kvp in balances)
            {
                data.entries.Add(new CurrencySaveEntry
                {
                    type    = (int)kvp.Key,
                    balance = kvp.Value
                });
            }

            return data;
        }

        public void RestoreState(object state)
        {
            if (!(state is CurrencySaveData data) || data.entries == null) return;

            foreach (var entry in data.entries)
            {
                var type = (CurrencyType_UMFOSS)entry.type;
                if (!balances.ContainsKey(type)) continue;

                int oldBalance = balances[type];
                balances[type] = entry.balance;
                OnBalanceChanged?.Invoke(type, oldBalance, entry.balance);
            }
        }

        [Serializable]
        public class CurrencySaveData
        {
            public List<CurrencySaveEntry> entries;
        }

        [Serializable]
        public struct CurrencySaveEntry
        {
            public int type;
            public int balance;
        }

        // --- Private ---

        private bool ValidateType(CurrencyType_UMFOSS type, string caller)
        {
            if (balances.ContainsKey(type)) return true;
            Debug.LogWarning($"[CurrencySystem] {caller} called with unconfigured type {type}.");
            return false;
        }

        private void InitializeBalances()
        {
            if (config == null)
            {
                Debug.LogError("[CurrencySystem] CurrencyConfig not assigned!");
                return;
            }

            if (config.currencies == null) return;

            balances.Clear();
            foreach (var entry in config.currencies)
            {
                if (balances.ContainsKey(entry.type))
                {
                    Debug.LogWarning($"[CurrencySystem] Duplicate type {entry.type} in config. Skipping.");
                    continue;
                }
                balances[entry.type] = entry.startingBalance;
            }
        }
    }
}
