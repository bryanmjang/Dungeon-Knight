using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewKeyItem", menuName = "Inventory/Items/Key Item")]
public class KeyItem : InventoryItemData
{
    [Header("Key Info")]
    public string keyID;
    
}