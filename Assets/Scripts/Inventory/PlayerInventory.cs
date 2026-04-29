using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    // lightweight runtime copy of the inventory used when moving between scenes
    [Serializable]
    private class SavedSlotData
    {
        public InventoryItemData item;
        public int quantity;
    }

    private static List<SavedSlotData> savedSlots = new List<SavedSlotData>();
    private static int savedSelectedSlotIndex;
    private static bool hasSavedInventory;

    // Hotbar slots stay visible, extra rows only show when the inventory is open.
    [SerializeField] private int hotbarSize = 8;
    [SerializeField] private int inventoryRows = 3;
    [SerializeField] private int inventoryColumns = 8;
    [SerializeField] private float interactRange = 1.5f;

    // main inventory storage list
    private List<InventorySlot> slots = new List<InventorySlot>();
    // reference to the UI that displays this inventory
    private InventoryUI inventoryUI;

    // events used so the UI can update whenever something changes
    public event Action InventoryChanged;
    public event Action<int> SelectedSlotChanged;


    [Header("Item Scriptable Objects - not scallable, but it works")]
    [SerializeField] private CoinItem coinItemScriptable;
    [SerializeField] private WeaponItem ironSwordScriptable;

    [Header("Consumables")]
    [SerializeField] private ConsumableItem steakScriptable;
    [SerializeField] private ConsumableItem healthPotionScriptable;
    [SerializeField] private ConsumableItem maxHealthPotionScriptable;
    [SerializeField] private ConsumableItem strengthPotionScriptable;

    [Header("Keys")]
    [SerializeField] private KeyItem finalDungeonKeyScriptable;
    [SerializeField] private KeyItem goldenKeyScriptable;
    [SerializeField] private KeyItem d1k1Scriptable;
    [SerializeField] private KeyItem d1k2Scriptable;
    [SerializeField] private KeyItem d1k3Scriptable;
    [SerializeField] private KeyItem d1k4Scriptable;
    [SerializeField] private KeyItem d1k5Scriptable;
    [SerializeField] private KeyItem d2k1Scriptable;
    [SerializeField] private KeyItem d2k2Scriptable;
    [SerializeField] private KeyItem d2k3Scriptable;
    [SerializeField] private KeyItem d2k4Scriptable;
    [SerializeField] private KeyItem d2k5Scriptable;

    public IReadOnlyList<InventorySlot> Slots
    {
        get { return slots; }
    }

    public int HotbarSize
    {
        get { return hotbarSize; }
    }

    public int TotalSlotCount
    {
        get { return hotbarSize + (inventoryRows * inventoryColumns); }
    }

    public bool IsInventoryOpen { get; private set; }
    public int SelectedSlotIndex { get; private set; }
    public int DraggedSlotIndex { get; private set; } = -1;


    private void Awake()
    {
        // Create the slot list and connect it to the runtime inventory UI.
        EnsureSlotCapacity();
        // restore the previous scene's inventory before the UI draws the slots
        RestoreSavedInventory();

        inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI == null)
        {
            GameObject inventoryUiObject = new GameObject("RuntimeInventoryUI");
            inventoryUI = inventoryUiObject.AddComponent<InventoryUI>();
        }

        inventoryUI.Initialize(this);
        SetSelectedSlot(hasSavedInventory ? savedSelectedSlotIndex : 0);
    }

    private void OnDestroy()
    {
        // keep the current inventory data available for the next scene's player
        SaveInventoryState();
    }

    public bool AddItem(InventoryItemData itemData, int quantity = 1)
    {
        // reject invalid add requests
        if (itemData == null || quantity <= 0)
        {
            return false;
        }

        // tracks how many items still need to be placed into slots
        int remaining = quantity;

        // First try to add onto existing stacks of the same item.
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot.item != itemData || slot.quantity >= itemData.maxStackSize)
            {
                continue;
            }

            int spaceLeft = itemData.maxStackSize - slot.quantity;
            int amountToAdd = Mathf.Min(spaceLeft, remaining);

            slot.quantity += amountToAdd;
            remaining -= amountToAdd;

            if (remaining <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }

        // If no matching stack has room, use the first empty slot.
        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (!slot.IsEmpty)
            {
                continue;
            }

            int amountToAdd = Mathf.Min(itemData.maxStackSize, remaining);
            slot.item = itemData;
            slot.quantity = amountToAdd;
            remaining -= amountToAdd;

            if (remaining <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }

        NotifyInventoryChanged();
        return false;
    }

    public bool RemoveFromSlot(int slotIndex, int quantity = 1)
    {
        // reject invalid slot indexes or quantities
        if (!IsValidSlot(slotIndex) || quantity <= 0)
        {
            return false;
        }

        // do nothing if the target slot is already empty
        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty)
        {
            return false;
        }

        // subtract from the stack
        slot.quantity -= quantity;

        // fully clear the slot if the stack reaches zero
        if (slot.quantity <= 0)
        {
            slot.Clear();
        }

        NotifyInventoryChanged();
        return true;
    }

    public InventorySlot GetSelectedSlot()
    {
        // returns the slot currently highlighted by the player
        if (!IsValidSlot(SelectedSlotIndex))
        {
            return null;
        }

        return slots[SelectedSlotIndex];
    }

    public void ToggleInventory()
    {
        // swap between open and closed inventory states
        SetInventoryOpen(!IsInventoryOpen);
    }

    public void SetInventoryOpen(bool isOpen)
    {
        if (IsInventoryOpen == isOpen)
        {
            return;
        }

        // The UI decides whether only the hotbar or the full inventory should be shown.
        IsInventoryOpen = isOpen;
        inventoryUI.SetInventoryVisible(isOpen);
        NotifyInventoryChanged();
    }

    public void SetSelectedSlot(int slotIndex)
    {
        // ignore invalid slot numbers
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        // store the selected slot and notify the UI
        SelectedSlotIndex = slotIndex;
        // remember the selected hotbar slot across scene loads
        savedSelectedSlotIndex = SelectedSlotIndex;
        SelectedSlotChanged?.Invoke(SelectedSlotIndex);
        NotifyInventoryChanged();
    }

    public void HandleLeftClick(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        // When the inventory is closed, left click just changes the selected hotbar slot.
        if (!IsInventoryOpen)
        {
            SetSelectedSlot(slotIndex);
            return;
        }

        // First click chooses which slot is being moved.
        if (DraggedSlotIndex < 0)
        {
            if (slots[slotIndex].IsEmpty)
            {
                SetSelectedSlot(slotIndex);
                return;
            }

            DraggedSlotIndex = slotIndex;
            SetSelectedSlot(slotIndex);
            NotifyInventoryChanged();
            return;
        }

        // Clicking the same slot again cancels the move.
        if (DraggedSlotIndex == slotIndex)
        {
            DraggedSlotIndex = -1;
            SetSelectedSlot(slotIndex);
            NotifyInventoryChanged();
            return;
        }

        MoveOrSwapSlots(DraggedSlotIndex, slotIndex);
        DraggedSlotIndex = -1;
        SetSelectedSlot(slotIndex);
        NotifyInventoryChanged();
    }

    public void DropOneFromSlot(int slotIndex)
    {
        if (!IsValidSlot(slotIndex))
        {
            return;
        }

        InventorySlot slot = slots[slotIndex];
        if (slot.IsEmpty)
        {
            return;
        }

        // Remove one item from this slot and spawn it back into the world.
        InventoryItemData itemToDrop = slot.item;
        RemoveFromSlot(slotIndex, 1);

        Vector3 dropPosition = transform.position + Vector3.right * 0.75f;
        WorldItemPickup.SpawnDroppedItem(itemToDrop, dropPosition, 1);
    }

    private void EnsureSlotCapacity()
    {
        // Fill the list with empty slots until it matches the configured size.
        while (slots.Count < TotalSlotCount)
        {
            slots.Add(new InventorySlot());
        }
    }

    private void MoveOrSwapSlots(int fromIndex, int toIndex)
    {
        if (!IsValidSlot(fromIndex) || !IsValidSlot(toIndex))
        {
            return;
        }

        InventorySlot fromSlot = slots[fromIndex];
        InventorySlot toSlot = slots[toIndex];

        if (fromSlot.IsEmpty)
        {
            return;
        }

        // Move directly into empty slots.
        if (toSlot.IsEmpty)
        {
            toSlot.item = fromSlot.item;
            toSlot.quantity = fromSlot.quantity;
            fromSlot.Clear();
            return;
        }

        // Merge stacks when the same item is clicked together.
        if (toSlot.item == fromSlot.item && toSlot.quantity < toSlot.item.maxStackSize)
        {
            int availableSpace = toSlot.item.maxStackSize - toSlot.quantity;
            int amountToMove = Mathf.Min(availableSpace, fromSlot.quantity);

            toSlot.quantity += amountToMove;
            fromSlot.quantity -= amountToMove;

            if (fromSlot.quantity <= 0)
            {
                fromSlot.Clear();
            }

            return;
        }

        // Otherwise swap the two slot contents.
        InventoryItemData tempItem = toSlot.item;
        int tempQuantity = toSlot.quantity;

        toSlot.item = fromSlot.item;
        toSlot.quantity = fromSlot.quantity;

        fromSlot.item = tempItem;
        fromSlot.quantity = tempQuantity;
    }

    private bool IsValidSlot(int slotIndex)
    {
        // makes sure the requested slot exists in the list
        return slotIndex >= 0 && slotIndex < slots.Count;
    }

    private void NotifyInventoryChanged()
    {
        // save the latest slot data, then tell the UI it needs a redraw
        SaveInventoryState();
        if (InventoryChanged != null)
        {
            InventoryChanged.Invoke();
        }
    }

    private void SaveInventoryState()
    {
        // rebuild the saved slot list from the current live inventory contents
        savedSlots.Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];
            savedSlots.Add(new SavedSlotData
            {
                item = slot.item,
                quantity = slot.quantity
            });
        }

        savedSelectedSlotIndex = SelectedSlotIndex;
        hasSavedInventory = true;
    }

    private void RestoreSavedInventory()
    {
        // do nothing until a previous scene has actually stored inventory data
        if (!hasSavedInventory)
        {
            HardLoad();
            return;
        }

        // copy the saved slot contents into this new scene's player inventory
        for (int i = 0; i < slots.Count && i < savedSlots.Count; i++)
        {
            SavedSlotData savedSlot = savedSlots[i];
            slots[i].item = savedSlot.item;
            slots[i].quantity = savedSlot.quantity;
        }
    }

    public int GetItemCount(InventoryItemData itemData)
    {
        int total = 0;

        for (int i = 0; i < slots.Count; i++ )
        {
            if (slots[i].item == itemData) total += slots[i].quantity;
        }

        return total;
    }

    public bool RemoveItem(InventoryItemData itemData, int quantity)
    {
        if (itemData == null || quantity <= 0) return false;

        if (GetItemCount(itemData) < quantity) return false;

        int remaining = quantity;

        for (int i = 0; i < slots.Count; i ++)
        {
            InventorySlot slot = slots[i];

            if (slot.item != itemData || slot.IsEmpty) continue;

            int amountToRemove = Mathf.Min(slot.quantity, remaining);
            slot.quantity -= amountToRemove;
            remaining -= amountToRemove;

            if (slot.quantity <= 0) slot.Clear();
            if (remaining <= 0)
            {
                NotifyInventoryChanged();
                return true;
            }
        }

        NotifyInventoryChanged();
        return true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryUnlockDoor();
        }
    }

    void TryUnlockDoor()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange);
    
        foreach (Collider2D hit in hits)
        {
            LockedDoor door = hit.GetComponent<LockedDoor>();
    
            if (door != null)
            {
                InventorySlot selectedSlot = GetSelectedSlot();
    
                if (selectedSlot == null || selectedSlot.IsEmpty)
                {
                    door.TryUnlock(null);
                    return;
                }
    
                KeyItem key = selectedSlot.item as KeyItem;
    
                door.TryUnlock(key);
    
                if (key != null && key.keyID == door.requiredKeyID)
                {
                    RemoveFromSlot(SelectedSlotIndex, 1);
                }
    
                return;
            }
        }
}



    // saves slots to player prefs
    public void HardSave()
    {
        string items_saved = "";

        foreach (var slot in savedSlots)
        {
            if (slot == null || slot.item == null) continue;
            string itemName = $"{slot.item.itemName}";
            int itemAmount = slot.quantity;
            PlayerPrefs.SetInt(itemName, itemAmount);
            items_saved += $"{itemName}_";
        }

        Debug.Log(items_saved);
        PlayerPrefs.SetString("Inventory_Keys", items_saved);

    }
    // Need to figure out how to load properly 
    public void HardLoad()
    {
        Debug.Log("loading");
        string keyString = PlayerPrefs.GetString("Inventory_Keys");
        List<string> keys = keyString.Split('_').ToList<string>();
        keys.Remove("");

        foreach (string key in keys)
        {
            int quanity = PlayerPrefs.GetInt(key);
            Debug.Log(key);
            InventoryItemData newItem = longSwitchStatementSorry(key);

            // this case ain't doing it's job


            Debug.Log(newItem == null);
            AddItem(newItem, quanity);
        }
    }

    private InventoryItemData longSwitchStatementSorry(string key)
    {
        switch (key)
        {
            case "Coin":
                return coinItemScriptable;

            case "Iron Sword":
                return ironSwordScriptable;

            // consumables
            case "Steak":
                return steakScriptable;

            case "Health Potion":
                return healthPotionScriptable;

            case "Max Health Potion":
                return maxHealthPotionScriptable;


            // keys
            case "Golden Key":
                return goldenKeyScriptable;

            case "Exit Key":
                return finalDungeonKeyScriptable;

            case "Dungeon1Key1":
                return d1k1Scriptable;

            case "Dungeon1Key2":
                return d1k2Scriptable;

            case "Dungeon1Key3":
                return d1k3Scriptable;

            case "Dungeon1Key4":
                return d1k4Scriptable;

            case "Dungeon1Key5":
                return d1k5Scriptable;

            case "Dungeon2Key1":
                return d2k1Scriptable;

            case "Dungeon2Key2":
                return d2k2Scriptable;

            case "Dungeon2Key3":
                return d2k3Scriptable;

            case "Dungeon2Key4":
                return d2k4Scriptable;

            case "Dungeon2Key5":
                return d2k5Scriptable;

            default:
                return null;
        }

    }

    public static void dumpInventory()
    {
        savedSlots.Clear();
    }
}
