using UnityEngine;

namespace GameplayMechanicsUMFOSS.Utils
{
    /// <summary>
    /// Utility methods for working with Unity LayerMasks.
    /// </summary>
    public static class LayerMaskUtils
    {
        /// <summary>
        /// Checks whether a given layer index is included in a LayerMask.
        /// </summary>
        /// <param name="layer">The layer index (0-31) from gameObject.layer.</param>
        /// <param name="mask">The LayerMask to check against.</param>
        public static bool IsInMask(int layer, LayerMask mask)
        {
            return (mask.value & (1 << layer)) != 0;
        }
    }
}
