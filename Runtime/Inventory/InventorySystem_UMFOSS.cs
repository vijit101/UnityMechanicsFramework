using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameplayMechanicsUMFOSS.Inventory
{
    // core inventory manager. handles slots, items, weight, and fires events.
    // doesn't know about UI or any external system - everything talks through events.
    public class InventorySystem_UMFOSS : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int slotCount = 20;
        [SerializeField] private float maxWeight = 0f;    // 0 = weight system off
        [SerializeField] private bool allowOverflow = false;

        private List<InventorySlot_UMFOSS> slots;
        private float currentWeight;

        // events - subscribe to these from UI, audio, save system, whatever
        public event Action<ItemData_UMFOSS, int, int> OnItemAdded;      // (item, qty, slotIndex)
        public event Action<ItemData_UMFOSS, int, int> OnItemRemoved;    // (item, qty, slotIndex)
        public event Action OnInventoryFull;
        public event Action<float, float> OnWeightChanged;               // (current, max)
        public event Action<int> OnSlotChanged;                          // (slotIndex)
        public event Action OnInventoryCleared;

        public int SlotCount => slotCount;
        public float MaxWeight => maxWeight;

        private void Awake()
        {
            InitializeSlots();
        }

        // --- ADD ---

        // tries to add items. fills existing stacks first, then uses empty slots.
        // won't do partial adds - either the full quantity goes in or nothing does.
        public bool AddItem(ItemData_UMFOSS item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            // weight check
            if (maxWeight > 0f && item.weight > 0f)
            {
                float additionalWeight = item.weight * quantity;
                if (!IsBelowWeightLimit(additionalWeight))
                {
                    OnInventoryFull?.Invoke();
                    return false;
                }
            }

            // can we actually fit all of it?
            if (!CanFitItem(item, quantity))
            {
                OnInventoryFull?.Invoke();
                return false;
            }

            int remaining = quantity;

            if (item.isStackable)
            {
                // first pass: top up existing stacks of this item
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (!slots[i].IsEmpty() &&
                        slots[i].item.data == item &&
                        !slots[i].item.IsFull())
                    {
                        int space = item.maxStackSize - slots[i].item.quantity;
                        int toAdd = Mathf.Min(remaining, space);

                        slots[i].item.quantity += toAdd;
                        remaining -= toAdd;

                        OnSlotChanged?.Invoke(i);
                        OnItemAdded?.Invoke(item, toAdd, i);
                    }
                }

                // second pass: put the rest in empty slots
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (slots[i].IsEmpty())
                    {
                        int toAdd = Mathf.Min(remaining, item.maxStackSize);
                        slots[i].item = new ItemInstance_UMFOSS(item, toAdd);
                        remaining -= toAdd;

                        OnSlotChanged?.Invoke(i);
                        OnItemAdded?.Invoke(item, toAdd, i);
                    }
                }
            }
            else
            {
                // non-stackable: one item per slot
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (slots[i].IsEmpty())
                    {
                        slots[i].item = new ItemInstance_UMFOSS(item, 1);
                        remaining--;

                        OnSlotChanged?.Invoke(i);
                        OnItemAdded?.Invoke(item, 1, i);
                    }
                }
            }

            if (maxWeight > 0f && item.weight > 0f)
            {
                currentWeight += item.weight * quantity;
                OnWeightChanged?.Invoke(currentWeight, maxWeight);
            }

            return true;
        }

        // --- REMOVE ---

        // removes items starting from the smallest stacks first.
        // if we don't have enough, nothing gets removed at all.
        public bool RemoveItem(ItemData_UMFOSS item, int quantity = 1)
        {
            if (item == null || quantity <= 0) return false;

            int totalCount = GetItemCount(item);
            if (totalCount < quantity) return false;

            // grab all slots with this item, sorted smallest stack first
            List<int> matchingSlots = new List<int>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item.data == item)
                    matchingSlots.Add(i);
            }

            matchingSlots.Sort((a, b) =>
                slots[a].item.quantity.CompareTo(slots[b].item.quantity));

            int remaining = quantity;

            foreach (int idx in matchingSlots)
            {
                if (remaining <= 0) break;

                int available = slots[idx].item.quantity;
                int toRemove = Mathf.Min(remaining, available);

                slots[idx].item.quantity -= toRemove;
                remaining -= toRemove;

                OnItemRemoved?.Invoke(item, toRemove, idx);

                if (slots[idx].item.quantity <= 0)
                    slots[idx].Clear();

                OnSlotChanged?.Invoke(idx);
            }

            if (maxWeight > 0f && item.weight > 0f)
            {
                currentWeight -= item.weight * quantity;
                currentWeight = Mathf.Max(0f, currentWeight);
                OnWeightChanged?.Invoke(currentWeight, maxWeight);
            }

            return true;
        }

        // --- QUERY ---

        public bool HasItem(ItemData_UMFOSS item, int quantity = 1)
        {
            return GetItemCount(item) >= quantity;
        }

        public int GetItemCount(ItemData_UMFOSS item)
        {
            if (item == null) return 0;

            int count = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item.data == item)
                    count += slots[i].item.quantity;
            }
            return count;
        }

        public List<InventorySlot_UMFOSS> GetSlotsByCategory(ItemCategory category)
        {
            List<InventorySlot_UMFOSS> result = new List<InventorySlot_UMFOSS>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty() && slots[i].item.data.category == category)
                    result.Add(slots[i]);
            }
            return result;
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

        public bool IsInventoryFull()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].IsEmpty()) return false;
            }
            return true;
        }

        public bool IsBelowWeightLimit(float additionalWeight)
        {
            if (maxWeight <= 0f) return true;
            return (currentWeight + additionalWeight) <= maxWeight;
        }

        // --- SLOT MANAGEMENT ---

        // swap is the atomic operation everything else builds on (drag-drop, hotbar, etc)
        public void SwapSlots(int slotIndexA, int slotIndexB)
        {
            if (slotIndexA < 0 || slotIndexA >= slots.Count) return;
            if (slotIndexB < 0 || slotIndexB >= slots.Count) return;
            if (slotIndexA == slotIndexB) return;

            ItemInstance_UMFOSS temp = slots[slotIndexA].item;
            slots[slotIndexA].item = slots[slotIndexB].item;
            slots[slotIndexB].item = temp;

            OnSlotChanged?.Invoke(slotIndexA);
            OnSlotChanged?.Invoke(slotIndexB);
        }

        public void ClearInventory()
        {
            for (int i = 0; i < slots.Count; i++)
                slots[i].Clear();

            currentWeight = 0f;

            if (maxWeight > 0f)
                OnWeightChanged?.Invoke(currentWeight, maxWeight);

            OnInventoryCleared?.Invoke();
        }

        public void ClearSlot(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= slots.Count) return;

            if (!slots[slotIndex].IsEmpty())
            {
                ItemData_UMFOSS itemData = slots[slotIndex].item.data;
                int qty = slots[slotIndex].item.quantity;

                if (maxWeight > 0f && itemData != null && itemData.weight > 0f)
                {
                    currentWeight -= itemData.weight * qty;
                    currentWeight = Mathf.Max(0f, currentWeight);
                    OnWeightChanged?.Invoke(currentWeight, maxWeight);
                }

                OnItemRemoved?.Invoke(itemData, qty, slotIndex);
            }

            slots[slotIndex].Clear();
            OnSlotChanged?.Invoke(slotIndex);
        }

        // --- INTERNAL ---

        private void InitializeSlots()
        {
            slots = new List<InventorySlot_UMFOSS>(slotCount);
            for (int i = 0; i < slotCount; i++)
                slots.Add(new InventorySlot_UMFOSS());

            currentWeight = 0f;
        }

        // dry-run check: can we fit this many items without actually modifying anything?
        private bool CanFitItem(ItemData_UMFOSS item, int quantity)
        {
            if (allowOverflow) return true;

            int remaining = quantity;

            if (item.isStackable)
            {
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (!slots[i].IsEmpty() &&
                        slots[i].item.data == item &&
                        !slots[i].item.IsFull())
                    {
                        remaining -= (item.maxStackSize - slots[i].item.quantity);
                    }
                }

                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (slots[i].IsEmpty())
                        remaining -= item.maxStackSize;
                }
            }
            else
            {
                for (int i = 0; i < slots.Count && remaining > 0; i++)
                {
                    if (slots[i].IsEmpty())
                        remaining--;
                }
            }

            return remaining <= 0;
        }
    }
}
