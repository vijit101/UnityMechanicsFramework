using UnityEngine;
using System.Collections.Generic;
using GameplayMechanicsUMFOSS.Inventory;

namespace GameplayMechanicsUMFOSS.Samples.Inventory
{
    // demo controller - uses OnGUI so there's no canvas/TMP dependency.
    // just shows the API working with buttons and a slot grid.
    public class InventoryDemo_UMFOSS : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventorySystem_UMFOSS inventory;

        [Header("Demo Items")]
        [SerializeField] private ItemData_UMFOSS healthPotion;
        [SerializeField] private ItemData_UMFOSS ironSword;
        [SerializeField] private ItemData_UMFOSS goldCoin;

        private const int MAX_LOG_LINES = 12;
        private List<string> eventLog = new List<string>();

        private void OnEnable()
        {
            if (inventory == null) return;
            inventory.OnItemAdded += (item, qty, slot) => Log($"+ {qty}x {item.itemName} -> slot {slot}");
            inventory.OnItemRemoved += (item, qty, slot) => Log($"- {qty}x {item.itemName} <- slot {slot}");
            inventory.OnInventoryFull += () => Log("INVENTORY FULL");
            inventory.OnWeightChanged += (cur, max) => Log($"Weight: {cur:F1}/{max:F1}");
            inventory.OnInventoryCleared += () => Log("Inventory cleared");
        }

        private void OnGUI()
        {
            GUI.skin.button.fontSize = 14;
            GUI.skin.label.fontSize = 13;
            GUI.skin.box.fontSize = 12;

            GUI.Label(new Rect(20, 10, 500, 30), "<size=20><b>Inventory System Demo</b></size>");

            // action buttons
            float y = 50;
            if (GUI.Button(new Rect(20, y, 200, 35), "Add Health Potion"))
                TryAdd(healthPotion, 1);
            y += 40;

            if (GUI.Button(new Rect(20, y, 200, 35), "Add Iron Sword"))
                TryAdd(ironSword, 1);
            y += 40;

            if (GUI.Button(new Rect(20, y, 200, 35), "Add Gold Coin x10"))
                TryAdd(goldCoin, 10);
            y += 40;

            if (GUI.Button(new Rect(20, y, 200, 35), "Remove Health Potion"))
            {
                bool ok = inventory.RemoveItem(healthPotion, 1);
                if (!ok) Log("Can't remove - not in inventory");
            }
            y += 40;

            if (GUI.Button(new Rect(20, y, 200, 35), "Swap Slot 0 \u2194 1"))
            {
                inventory.SwapSlots(0, 1);
                Log("Swapped slot 0 and 1");
            }
            y += 40;

            if (GUI.Button(new Rect(20, y, 200, 35), "Clear Inventory"))
                inventory.ClearInventory();

            // slot grid
            DrawSlotGrid(250, 50);

            // stats
            DrawStats(250, 450);

            // event log
            DrawLog(20, 320);
        }

        private void TryAdd(ItemData_UMFOSS item, int qty)
        {
            bool ok = inventory.AddItem(item, qty);
            if (!ok) Log($"Failed to add {item.itemName}");
        }

        private void DrawSlotGrid(float startX, float startY)
        {
            GUI.Label(new Rect(startX, startY, 500, 25), "<b>Slots:</b>");
            startY += 25;

            List<InventorySlot_UMFOSS> allSlots = inventory.GetAllSlots();

            for (int i = 0; i < allSlots.Count; i++)
            {
                int col = i % 5;
                int row = i / 5;
                float x = startX + col * 95;
                float slotY = startY + row * 95;

                GUI.Box(new Rect(x, slotY, 90, 90), "");

                if (!allSlots[i].IsEmpty())
                {
                    var item = allSlots[i].item;

                    if (item.data.icon != null && item.data.icon.texture != null)
                        GUI.DrawTexture(new Rect(x + 5, slotY + 5, 80, 50), item.data.icon.texture);

                    string name = item.data.itemName;
                    if (name.Length > 10) name = name.Substring(0, 9) + "..";

                    GUI.Label(new Rect(x + 3, slotY + 55, 84, 18), $"<size=10>{name}</size>");
                    GUI.Label(new Rect(x + 3, slotY + 70, 84, 18), $"<size=11><b>x{item.quantity}</b></size>");
                }
                else
                {
                    GUI.Label(new Rect(x + 15, slotY + 35, 70, 20),
                        $"<color=#888>[{i}]</color>");
                }
            }
        }

        private void DrawStats(float x, float y)
        {
            GUI.Label(new Rect(x, y, 300, 20), "<b>Stats:</b>");
            y += 20;

            string full = inventory.IsInventoryFull() ? "<color=red>YES</color>" : "<color=green>No</color>";
            GUI.Label(new Rect(x, y, 300, 20), $"Full: {full}");
            y += 18;

            if (inventory.MaxWeight > 0f)
            {
                float w = inventory.GetCurrentWeight();
                string wc = w >= inventory.MaxWeight ? "red" : "white";
                GUI.Label(new Rect(x, y, 300, 20), $"Weight: <color={wc}>{w:F1}/{inventory.MaxWeight:F1}</color>");
                y += 18;
            }

            if (healthPotion) GUI.Label(new Rect(x, y, 300, 20), $"Potions: {inventory.GetItemCount(healthPotion)}");
            y += 18;
            if (ironSword) GUI.Label(new Rect(x, y, 300, 20), $"Swords: {inventory.GetItemCount(ironSword)}");
            y += 18;
            if (goldCoin) GUI.Label(new Rect(x, y, 300, 20), $"Gold: {inventory.GetItemCount(goldCoin)}");
        }

        private void DrawLog(float x, float y)
        {
            GUI.Label(new Rect(x, y, 200, 20), "<b>Log:</b>");
            y += 20;
            GUI.Box(new Rect(x, y, 210, 200), "");

            for (int i = 0; i < eventLog.Count; i++)
                GUI.Label(new Rect(x + 5, y + 5 + i * 15, 200, 15), $"<size=10>{eventLog[i]}</size>");
        }

        private void Log(string msg)
        {
            eventLog.Insert(0, msg);
            if (eventLog.Count > MAX_LOG_LINES)
                eventLog.RemoveAt(eventLog.Count - 1);
            Debug.Log($"[InventoryDemo] {msg}");
        }
    }
}
