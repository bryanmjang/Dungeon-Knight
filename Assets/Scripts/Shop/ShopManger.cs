using System.Collections.Generic;
using UnityEngine;

public class ShopManger : MonoBehaviour
{
    [SerializeField] private List<ShopItems> shopItems;
    [SerializeField] private List<ShopBuilding> shopBuildings;
    [SerializeField] private List<ShopItems> shopTroops;
    [SerializeField] private ShopSlot[] shopSlots;
    [SerializeField] private InventoryItemData coinItem;

    public ShopTrigger shopTrigger;
    public GameObject placementZoneVisual;
    public Collider2D placementZoneCollider;

    string currentTab = "Items";
    public bool isViewingZone = false;
    public GameObject previewObject;
    private ShopBuilding pendingBuilding;

    private void Start()
    {
        if (placementZoneVisual != null)
            placementZoneVisual.SetActive(false);
        populateShopItems();
    }


    public void shopUpdate(string tab)
    {
        currentTab = tab;
        if (currentTab == "Items")
            populateShopItems();
        else if (currentTab == "Buildings")
            populateShopBuildings();
        else if (currentTab == "Troops")
            populateShopTroops();
    }

    public void populateShopItems()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (i < shopItems.Count)
            {
                shopSlots[i].slotIndex = i;
                shopSlots[i].SetItem(shopItems[i].item, shopItems[i].price);
                shopSlots[i].gameObject.SetActive(true);
            }
        }
        for (int i = shopItems.Count; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] != null)
                shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void populateShopBuildings()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (i < shopBuildings.Count)
            {
                shopSlots[i].slotIndex = i;
                shopSlots[i].SetItem(shopBuildings[i].item, shopBuildings[i].price);
                shopSlots[i].gameObject.SetActive(true);
            }
        }
        for (int i = shopBuildings.Count; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] != null)
                shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void populateShopTroops()
    {
        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (i < shopTroops.Count)
            {
                shopSlots[i].slotIndex = i;
                shopSlots[i].SetItem(shopTroops[i].item, shopTroops[i].price);
                shopSlots[i].gameObject.SetActive(true);
            }
        }
        for (int i = shopTroops.Count; i < shopSlots.Length; i++)
        {
            if (shopSlots[i] != null)
                shopSlots[i].gameObject.SetActive(false);
        }
    }

    public void UpdatePreview(Vector3 worldPos)
    {
        if (previewObject == null) return;

        previewObject.transform.position = worldPos;

        bool valid = IsValidPlacement(worldPos);
        Color c = valid ? new Color(1f, 1f, 1f, 0.4f) : new Color(1f, 0f, 0f, 0.4f);
        foreach (var sr in previewObject.GetComponentsInChildren<SpriteRenderer>())
            sr.color = c;
    }

    private bool IsValidPlacement(Vector3 worldPos)
    {
        Vector2 pos2D = new Vector2(worldPos.x, worldPos.y);

        if (placementZoneCollider != null && !placementZoneCollider.OverlapPoint(pos2D))
            return false;

        int enemyLayer = LayerMask.NameToLayer("Enemy");
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos2D, 0.3f);
        foreach (var hit in hits)
        {
            if (hit == placementZoneCollider) continue;
            if (hit is CircleCollider2D && hit.gameObject.layer == enemyLayer) continue;
            return false;
        }

        return true;
    }

    public void BuySelected(int slotIndex)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        if (inventory == null) return;

        if (currentTab == "Buildings")
        {
            if (slotIndex < 0 || slotIndex >= shopBuildings.Count) return;

            ShopBuilding building = shopBuildings[slotIndex];
            if (inventory.GetItemCount(coinItem) < building.price)
            {
                Debug.Log("Not enough coins.");
                return;
            }

            inventory.RemoveItem(coinItem, building.price);
            ShowPlacementZone(slotIndex);
        }
        else if (currentTab == "Items")
        {
            if (slotIndex < 0 || slotIndex >= shopItems.Count) return;

            ShopItems selectedItem = shopItems[slotIndex];
            if (inventory.GetItemCount(coinItem) < selectedItem.price)
            {
                Debug.Log("Not enough coins.");
                return;
            }

            if (selectedItem.rewardItem == null)
            {
                Debug.LogWarning("Shop item has no reward item assigned");
                return;
            }

            inventory.RemoveItem(coinItem, selectedItem.price);
            inventory.AddItem(selectedItem.rewardItem, 1);
        }
    }

    private void ShowPlacementZone(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= shopBuildings.Count) return;

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        var building = shopBuildings[slotIndex];

        if (shopTrigger != null)
        {
            shopTrigger.CloseShopTemp();
            shopTrigger.gotoPreviewMode();
        }

        if (placementZoneVisual != null)
            placementZoneVisual.SetActive(true);

        pendingBuilding = building;

        if (building.prefab != null)
        {
            previewObject = Instantiate(building.prefab);
            previewObject.SetActive(true);

            foreach (var sr in previewObject.GetComponentsInChildren<SpriteRenderer>())
            {
                Color c = sr.color;
                c.a = 0.4f;
                sr.color = c;
            }
            foreach (var col in previewObject.GetComponentsInChildren<Collider2D>())
                col.enabled = false;
            foreach (var mb in previewObject.GetComponentsInChildren<MonoBehaviour>())
                if (mb != this) mb.enabled = false;
        }

        isViewingZone = true;
    }

    public void TryPlace(Vector3 worldPos)
    {
        if (!IsValidPlacement(worldPos)) return;

        if (pendingBuilding != null && pendingBuilding.prefab != null)
            Instantiate(pendingBuilding.prefab, worldPos, Quaternion.identity);

        FinishPlacement();
    }

    private void FinishPlacement()
    {
        isViewingZone = false;
        pendingBuilding = null;

        if (placementZoneVisual != null)
            placementZoneVisual.SetActive(false);

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        if (shopTrigger != null)
        {
            shopTrigger.gotoNormalMode();
            shopTrigger.CloseShop();
        }
    }

    public void CancelZoneView()
    {
        isViewingZone = false;
        pendingBuilding = null;

        if (placementZoneVisual != null)
            placementZoneVisual.SetActive(false);

        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        if (shopTrigger != null)
        {
            shopTrigger.gotoNormalMode();
            shopTrigger.OpenShop();
        }

        shopUpdate("Buildings");
    }
}

[System.Serializable]
public class ShopItems
{
    public ItemSO item;
    public InventoryItemData rewardItem;
    public int price;
}

[System.Serializable]
public class ShopBuilding
{
    public ItemSO item;
    public int price;
    public GameObject prefab;
}
