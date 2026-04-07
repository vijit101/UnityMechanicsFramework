namespace GameplayMechanicsUMFOSS.Inventory
{
    [System.Serializable]
    public class InventorySlot_UMFOSS
    {
        public ItemInstance_UMFOSS item; 

        public bool IsEmpty() => item == null || item.IsEmpty();
        
        public void Clear()
        {
            item = null;
        }
    }
}
