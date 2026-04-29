using UnityEngine;

[CreateAssetMenu(fileName = "NewConsumableItem", menuName = "Inventory/Items/Consumable Item")]
public class ConsumableItem: InventoryItemData
{
    public enum ConsumableEffectType
    {
        Heal,
        DamageBuff,
        SpeedBuff,
    }   // If you want to add buff types, add to enum then add a new case to
        // ApplyConsumable() logic in PlayerConsumables.cs script

    [Header("Consumable Info")]
    public ConsumableEffectType effectType = ConsumableEffectType.Heal;
    public int amount = 1;
    public float duration = 5f;
}