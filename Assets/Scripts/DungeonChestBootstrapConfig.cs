using UnityEngine;

public class DungeonChestBootstrapConfig : MonoBehaviour
{
    // direct references used by the runtime bootstrap so it does not need duplicate Resources assets
    [SerializeField] private InventoryItemData coinItem;

    public InventoryItemData CoinItem
    {
        get { return coinItem; }
    }
}
