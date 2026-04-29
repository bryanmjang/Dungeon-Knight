using UnityEngine;
using UnityEngine.EventSystems;

public class UpdateShopTab : MonoBehaviour, IPointerClickHandler
{
    public ShopManger shopManager;
    public string tab;

    public void OnPointerClick(PointerEventData eventData)
    {
        shopManager.shopUpdate(tab);
    }
}
