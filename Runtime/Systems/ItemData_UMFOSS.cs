using UnityEngine;

namespace GameplayMechanicsUMFOSS.Systems
{
    /// <summary>
    /// Minimal item definition for quest rewards. Extend in your project or replace with your inventory item type.
    /// </summary>
    [CreateAssetMenu(fileName = "NewItem", menuName = "UMFOSS/Quest/ItemData")]
    public class ItemData_UMFOSS : ScriptableObject
    {
        [Header("Identity")]
        public string itemID;

        public string displayName;
        public Sprite icon;
    }
}
