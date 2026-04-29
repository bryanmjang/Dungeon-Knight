using System;

[Serializable]
public class InventorySlot
{
    // Stores the item type in this slot and how many are stacked there.
    public InventoryItemData item;
    public int quantity;

    // checks whether the slot currently has anything stored in it
    public bool IsEmpty
    {
        get { return item == null || quantity <= 0; }
    }

    // resets the slot back to an empty state
    public void Clear()
    {
        item = null;
        quantity = 0;
    }
}
