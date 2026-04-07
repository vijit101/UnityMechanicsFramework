using UnityEngine;

namespace GameplayMechanicsUMFOSS.Inventory
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "UMFOSS/Inventory/ItemData")]
    public class ItemData_UMFOSS : ScriptableObject
    {
        [Header("Identity")]
        public string itemName;
        [TextArea(3, 5)]
        public string description;
        public Sprite icon;
        public ItemCategory category;

        [Header("Stack Settings")]
        public bool isStackable;
        public int maxStackSize; 

        [Header("Value")]
        public int baseValue;

        [Header("Weight (Optional)")]
        public float weight; 
    }
}
