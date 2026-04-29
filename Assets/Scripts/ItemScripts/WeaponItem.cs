using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Inventory/Items/Weapon Item")]
public class WeaponItem : InventoryItemData
{
    [Header("Weapon Info")]
    public int damageBonus = 1;
}