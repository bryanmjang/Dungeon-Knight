using UnityEngine;

[CreateAssetMenu(fileName = "NewResourceItem", menuName = "Inventory/Items/Resource Item")]
public class ResourceItemData : InventoryItemData
{
    [Header("Resource Info")]
    public string resourceType = "Material";
}