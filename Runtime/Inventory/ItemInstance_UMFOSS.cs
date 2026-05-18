using UnityEngine;

namespace GameplayMechanicsUMFOSS.Inventory
{
    // one actual item sitting in a slot. ItemData is the shared blueprint,
    // this is the per-slot copy with its own quantity (and later, durability etc)
    [System.Serializable]
    public class ItemInstance_UMFOSS
    {
        public ItemData_UMFOSS data;
        public int quantity;

        public ItemInstance_UMFOSS(ItemData_UMFOSS data, int quantity = 1)
        {
            this.data = data;
            this.quantity = Mathf.Clamp(quantity, 1, data.isStackable ? data.maxStackSize : 1);
        }

        public bool IsEmpty() => data == null || quantity <= 0;
        public bool IsFull()  => !data.isStackable || quantity >= data.maxStackSize;
    }
}
