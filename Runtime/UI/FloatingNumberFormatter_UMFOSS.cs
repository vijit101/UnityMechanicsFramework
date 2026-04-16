using UnityEngine;

namespace GameplayMechanicsUMFOSS.UI
{
    /// <summary>
    /// Centralizes text formatting rules for floating numbers and keeps them easy to test.
    /// </summary>
    public static class FloatingNumberFormatter_UMFOSS
    {
        public static string Format(float amount, NumberType type, int decimalPlaces)
        {
            string formattedValue = decimalPlaces <= 0
                ? Mathf.RoundToInt(amount).ToString()
                : amount.ToString("F" + decimalPlaces);

            switch (type)
            {
                case NumberType.Miss:
                    return "MISS";
                case NumberType.CriticalHit:
                    return formattedValue + "!";
                case NumberType.Experience:
                    return "+" + formattedValue + " XP";
                default:
                    return formattedValue;
            }
        }
    }
}
