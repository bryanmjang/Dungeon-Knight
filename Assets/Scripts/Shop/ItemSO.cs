using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Shop/Item", order = 0)]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    [TextArea]
    public string description;
}