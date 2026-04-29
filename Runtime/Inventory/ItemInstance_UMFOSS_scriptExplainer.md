Line 1: `using UnityEngine;`
Explanation: Imports Unity utilities, specifically `Mathf`, which is used to clamp quantity values safely.

Line 2: blank line
Explanation: Separates the `using` directive from the namespace declaration.

Line 3: `namespace GameplayMechanicsUMFOSS.Inventory`
Explanation: Places the runtime item instance class inside the inventory namespace.

Line 4: `{`
Explanation: Opens the namespace block.

Line 5: `[System.Serializable]`
Explanation: Marks this plain C# class as serializable so Unity can show and store it inside other serialized objects.

Line 6: `public class ItemInstance_UMFOSS`
Explanation: Declares the class that represents one mutable item stack stored at runtime.

Line 7: `{`
Explanation: Opens the class block.

Line 8: `public ItemData_UMFOSS data;`
Explanation: Stores a reference to the shared `ItemData_UMFOSS` blueprint that tells this instance what kind of item it is.

Line 9: `public int quantity;`
Explanation: Stores the mutable amount of this item in the current slot.

Line 10: blank line
Explanation: Separates the fields from the constructor.

Line 11: `public ItemInstance_UMFOSS(ItemData_UMFOSS data, int quantity = 1)`
Explanation: Declares a constructor that creates a new runtime item instance from an item definition and an optional starting quantity.

Line 12: `{`
Explanation: Opens the constructor block.

Line 13: `this.data = data;`
Explanation: Saves the incoming item definition reference into the instance.

Line 14: `this.quantity = Mathf.Clamp(quantity, 1, data.isStackable ? data.maxStackSize : 1);`
Explanation: Forces quantity to stay valid by clamping stackable items to their max stack size and non-stackable items to exactly 1.

Line 15: `}`
Explanation: Closes the constructor block.

Line 16: blank line
Explanation: Separates the constructor from the helper methods.

Line 17: `public bool IsEmpty() => data == null || quantity <= 0;`
Explanation: Returns `true` when the instance does not point to a valid item or has no usable quantity left.

Line 18: `public bool IsFull() => data.isStackable && quantity >= data.maxStackSize;`
Explanation: Returns `true` when the instance is stackable and has already reached its allowed stack limit.

Line 19: `}`
Explanation: Closes the class block.

Line 20: `}`
Explanation: Closes the namespace block.
