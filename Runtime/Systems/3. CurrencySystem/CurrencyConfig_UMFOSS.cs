using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    [CreateAssetMenu(fileName = "CurrencyConfig", menuName = "UMFOSS/Economy/CurrencyConfig")]
    public class CurrencyConfig_UMFOSS : ScriptableObject
    {
        [System.Serializable]
        public class CurrencyEntry
        {
            public CurrencyType_UMFOSS type;
            public string              displayName;
            public Sprite              icon;
            public int                 startingBalance;
            public int                 maxBalance;
            public bool                canGoNegative;
        }

        public CurrencyEntry[] currencies;

        public CurrencyEntry GetEntry(CurrencyType_UMFOSS type) =>
            System.Array.Find(currencies, c => c.type == type);
    }
}
