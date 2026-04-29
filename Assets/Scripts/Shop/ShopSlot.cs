using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public TMP_Text itemNameText;
    public TMP_Text priceText;
    public Image itemIcon;
    public ShopManger shopManager;

    [HideInInspector] public int slotIndex;
    private int price;

    private void Start()
    {
        if (itemSO != null)
            SetItem(itemSO, price);
    }

    public void SetItem(ItemSO item, int price)
    {
        this.itemSO = item;
        itemNameText.text = this.itemSO.itemName;
        itemIcon.sprite = this.itemSO.icon;
        this.price = price;
        priceText.text = price.ToString();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        shopManager.BuySelected(slotIndex);
    }
}
