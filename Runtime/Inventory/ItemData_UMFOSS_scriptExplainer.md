Line 1: `using UnityEngine;`
Explanation: Imports Unity types such as `ScriptableObject`, `Sprite`, and Inspector attributes used by this script.

Line 2: blank line
Explanation: Separates the `using` directive from the namespace declaration for readability.

Line 3: `namespace GameplayMechanicsUMFOSS.Inventory`
Explanation: Places the item data script inside the inventory namespace.

Line 4: `{`
Explanation: Opens the namespace block.

Line 5: `[CreateAssetMenu(fileName = "NewItem", menuName = "UMFOSS/Inventory/ItemData")]`
Explanation: Adds a right-click Project menu entry so designers can create new item assets without writing code.

Line 6: `public class ItemData_UMFOSS : ScriptableObject`
Explanation: Declares the item definition class and makes it a `ScriptableObject` so each item type exists as an asset.

Line 7: `{`
Explanation: Opens the class block.

Line 8: `[Header("Identity")]`
Explanation: Groups the next set of fields under an `Identity` heading in the Unity Inspector.

Line 9: `public string itemName;`
Explanation: Stores the display name of the item type.

Line 10: `[TextArea(3, 5)]`
Explanation: Makes the description field show as a multi-line text area in the Inspector.

Line 11: `public string description;`
Explanation: Stores the item's description text for UI or tooltips.

Line 12: `public Sprite icon;`
Explanation: Stores the sprite used by the UI to represent the item visually.

Line 13: `public ItemCategory category;`
Explanation: Stores which category this item belongs to so other systems can filter or route it later.

Line 14: blank line
Explanation: Separates the identity fields from the stack settings section.

Line 15: `[Header("Stack Settings")]`
Explanation: Groups the stacking-related fields under a `Stack Settings` heading in the Inspector.

Line 16: `public bool isStackable;`
Explanation: Determines whether multiple copies of this item can share one slot.

Line 17: `public int maxStackSize;`
Explanation: Stores the maximum quantity allowed in one stack when the item is stackable.

Line 18: blank line
Explanation: Separates the stack settings from the value section.

Line 19: `[Header("Value")]`
Explanation: Groups value-related fields in the Inspector.

Line 20: `public int baseValue;`
Explanation: Stores the base currency value of the item for future systems like shops.

Line 21: blank line
Explanation: Separates the value section from the optional weight section.

Line 22: `[Header("Weight (Optional)")]`
Explanation: Groups the optional weight field under its own Inspector heading.

Line 23: `public float weight;`
Explanation: Stores how much one unit of this item weighs when a game uses a weight limit.

Line 24: `}`
Explanation: Closes the class block.

Line 25: `}`
Explanation: Closes the namespace block.
