namespace GameplayMechanicsUMFOSS.Inventory
{
    // one slot in the inventory, holds a single ItemInstance (or null if empty)
    [System.Serializable]
    public class InventorySlot_UMFOSS
    {
        public ItemInstance_UMFOSS item;

        public bool IsEmpty() => item == null || item.IsEmpty();
        public void Clear()   { item = null; }
    }
}
