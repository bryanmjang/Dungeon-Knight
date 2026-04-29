using UnityEngine;

[CreateAssetMenu(fileName = "InventoryItem", menuName = "Inventory/Item Data")]
public class InventoryItemData : ScriptableObject
{
    // Shared item info used by both world pickups and inventory slots.
    [Header("Display")]
    // name shown in the inventory or other future UI
    public string itemName = "New Item";
    // short description for the item
    [TextArea]
    public string description;
    // image shown in the slot icon
    public Sprite icon;

    [Header("Stacking")]
    // maximum number of this item allowed in one slot
    [Min(1)]
    public int maxStackSize = 99;

    [Header("World Pickup")]
    public WorldItemPickup pickupPrefab;
}
