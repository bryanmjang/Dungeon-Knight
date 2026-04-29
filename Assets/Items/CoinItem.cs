using UnityEngine;

[CreateAssetMenu(fileName = "NewCoinItem", menuName = "Inventory/Items/Coin Item")]
public class CoinItem : InventoryItemData
{
    [Header("Coin Info")]
    public int coinValue = 1;
}