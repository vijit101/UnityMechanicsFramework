using UnityEngine;

namespace GameplayMechanicsUMFOSS.Inventory
{
    // the blueprint for an item type - one asset per item in the whole game.
    // create new items from the project window: Create > UMFOSS > Inventory > ItemData
    [CreateAssetMenu(fileName = "NewItem", menuName = "UMFOSS/Inventory/ItemData")]
    public class ItemData_UMFOSS : ScriptableObject
    {
        [Header("Identity")]
        public string itemName;

        [TextArea(2, 4)]
        public string description;

        public Sprite icon;
        public ItemCategory category;

        [Header("Stack Settings")]
        public bool isStackable;

        [Min(1)]
        public int maxStackSize = 1; // only matters if isStackable is true

        [Header("Value")]
        [Min(0)]
        public int baseValue; // base currency value for shops

        [Header("Weight")]
        [Min(0f)]
        public float weight; // per unit, leave at 0 if your game doesn't use weight
    }
}
