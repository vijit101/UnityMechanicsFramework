using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Inventory
{
    public class InventorySystem_UMFOSS : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int slotCount = 20; 
        [SerializeField] private float maxWeight;   
        [SerializeField] private bool allowOverflow; 

        [SerializeField, HideInInspector]
        private List<InventorySlot_UMFOSS> slots;
        private float currentWeight;

        // Events
        public event Action<ItemData_UMFOSS, int, int> OnItemAdded;
        public event Action<ItemData_UMFOSS, int, int> OnItemRemoved;
        public event Action OnInventoryFull;
        public event Action<float, float> OnWeightChanged;
        public event Action<int> OnSlotChanged;
        public event Action OnInventoryCleared;

        private void Awake()
        {
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            slots = new List<InventorySlot_UMFOSS>(slotCount);
            for (int i = 0; i < slotCount; i++)
            {
                slots.Add(new InventorySlot_UMFOSS());
            }
            RecalculateWeight();
        }

        public bool AddItem(ItemData_UMFOSS itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;

            if (!IsBelowWeightLimit(itemData.weight * quantity))
            {
                if (!allowOverflow) return false;
            }

            int remainingQuantity = quantity;

            // 1. Try to fill existing stacks if stackable
            if (itemData.isStackable)
            {
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i];
                    if (!slot.IsEmpty() && slot.item.data == itemData && !slot.item.IsFull())
                    {
                        int spaceInStack = itemData.maxStackSize - slot.item.quantity;
                        int amountToAdd = Mathf.Min(remainingQuantity, spaceInStack);

                        slot.item.quantity += amountToAdd;
                        remainingQuantity -= amountToAdd;

                        OnItemAdded?.Invoke(itemData, amountToAdd, i);
                        OnSlotChanged?.Invoke(i);
                        UpdateWeight(itemData.weight * amountToAdd);

                        if (remainingQuantity <= 0) return true;
                    }
                }
            }

            // 2. Open new slots for remaining quantity
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i];
                if (slot.IsEmpty())
                {
                    int amountToAdd = itemData.isStackable ? Mathf.Min(remainingQuantity, itemData.maxStackSize) : 1;

                    slot.item = new ItemInstance_UMFOSS(itemData, amountToAdd);
                    remainingQuantity -= amountToAdd;

                    OnItemAdded?.Invoke(itemData, amountToAdd, i);
                    OnSlotChanged?.Invoke(i);
                    UpdateWeight(itemData.weight * amountToAdd);

                    if (remainingQuantity <= 0) return true;
                }
            }

            // Inventory was full, couldn't fit everything
            if (remainingQuantity > 0)
            {
                OnInventoryFull?.Invoke();
                return false;
            }

            return true;
        }

        public bool RemoveItem(ItemData_UMFOSS itemData, int quantity = 1)
        {
            if (itemData == null || quantity <= 0) return false;
            if (!HasItem(itemData, quantity)) return false;

            int remainingQuantity = quantity;

            // Find all slots with this item
            var candidateSlots = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item.data == itemData)
                {
                    candidateSlots.Add(i);
                }
            }

            // Sort so we deplete lowest quantity stacks first
            candidateSlots.Sort((a, b) => slots[a].item.quantity.CompareTo(slots[b].item.quantity));

            foreach (int slotIndex in candidateSlots)
            {
                var slot = slots[slotIndex];
                int amountToRemove = Mathf.Min(remainingQuantity, slot.item.quantity);

                slot.item.quantity -= amountToRemove;
                remainingQuantity -= amountToRemove;

                UpdateWeight(-itemData.weight * amountToRemove);
                OnItemRemoved?.Invoke(itemData, amountToRemove, slotIndex);

                if (slot.item.quantity <= 0)
                {
                    slot.Clear();
                }

                OnSlotChanged?.Invoke(slotIndex);

                if (remainingQuantity <= 0) break;
            }

            return remainingQuantity <= 0;
        }

        public bool HasItem(ItemData_UMFOSS itemData, int quantity = 1)
        {
            return GetItemCount(itemData) >= quantity;
        }

        public int GetItemCount(ItemData_UMFOSS itemData)
        {
            if (itemData == null) return 0;
            return slots.Where(s => !s.IsEmpty() && s.item.data == itemData).Sum(s => s.item.quantity);
        }

        public List<InventorySlot_UMFOSS> GetSlotsByCategory(ItemCategory category)
        {
            return slots.Where(s => !s.IsEmpty() && s.item.data.category == category).ToList();
        }

        public InventorySlot_UMFOSS GetSlotByIndex(int index)
        {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        public List<InventorySlot_UMFOSS> GetAllSlots()
        {
            return new List<InventorySlot_UMFOSS>(slots);
        }

        public float GetCurrentWeight()
        {
            return currentWeight;
        }

        public float GetMaxWeight()
        {
            return maxWeight;
        }

        public bool IsInventoryFull()
        {
            return slots.All(s => !s.IsEmpty());
        }

        public bool IsBelowWeightLimit(float additionalWeight)
        {
            if (maxWeight <= 0) return true; // 0 = weight system disabled
            return (currentWeight + additionalWeight) <= maxWeight;
        }

        public void SwapSlots(int slotIndexA, int slotIndexB)
        {
            if (slotIndexA < 0 || slotIndexA >= slots.Count || slotIndexB < 0 || slotIndexB >= slots.Count) return;

            var itemA = slots[slotIndexA].item;
            slots[slotIndexA].item = slots[slotIndexB].item;
            slots[slotIndexB].item = itemA;

            OnSlotChanged?.Invoke(slotIndexA);
            OnSlotChanged?.Invoke(slotIndexB);
        }

        public void ClearInventory()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty())
                {
                    UpdateWeight(-slots[i].item.data.weight * slots[i].item.quantity);
                    slots[i].Clear();
                    OnSlotChanged?.Invoke(i);
                }
            }
            OnInventoryCleared?.Invoke();
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;
            
            var slot = slots[slotIndex];
            if (!slot.IsEmpty())
            {
                UpdateWeight(-slot.item.data.weight * slot.item.quantity);
                slot.Clear();
                OnSlotChanged?.Invoke(slotIndex);
            }
        }

        private void RecalculateWeight()
        {
            currentWeight = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty())
                {
                    currentWeight += slot.item.data.weight * slot.item.quantity;
                }
            }
            OnWeightChanged?.Invoke(currentWeight, maxWeight);
        }

        private void UpdateWeight(float weightDelta)
        {
            if (maxWeight > 0 || currentWeight > 0)
            {
                currentWeight += weightDelta;
                if (currentWeight < 0.001f) currentWeight = 0;
                OnWeightChanged?.Invoke(currentWeight, maxWeight);
            }
        }
    }
}
