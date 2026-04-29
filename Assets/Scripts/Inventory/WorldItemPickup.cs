using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WorldItemPickup : MonoBehaviour
{
    // World object that gives the player inventory items when touched.
    // item data asset that this pickup represents
    [SerializeField] private InventoryItemData itemData;
    // how many of that item this pickup gives
    [SerializeField] private int quantity = 1;
    // sprite renderer used to show the item icon in the world
    [SerializeField] private SpriteRenderer spriteRenderer;
    private bool wasCollected;

    [SerializeField] private AudioClip collisionSound;

    private void Awake()
    {
        // Pickups use trigger collisions so the player can walk over them.
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;

        // auto-find a sprite renderer if one was not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // set the visible world sprite to match the item icon
        if (spriteRenderer != null && itemData != null && itemData.icon != null && spriteRenderer.GetComponent<Animator>() == null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ignore anything that is not the player
        if (wasCollected || !other.CompareTag("Player"))
        {
            return;
        }
        
        // Only the player should be able to collect this pickup.
        // try to find the inventory on the player object that touched the pickup
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            playerInventory = other.GetComponentInParent<PlayerInventory>();
        }

        // stop if there is no player inventory to receive the item
        if (playerInventory == null)
        {
            return;
        }

        // add the item, then remove the pickup from the world if successful
        if (playerInventory.AddItem(itemData, quantity))
        {
            wasCollected = true;

            Collider2D pickupCollider = GetComponent<Collider2D>();
            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }

            AudioSource.PlayClipAtPoint(collisionSound, transform.position);

            // 
            var isPersistentItem = GetComponent<ItemPersistence>();
            if (isPersistentItem) isPersistentItem.MarkAsCollected();

            Destroy(gameObject);
        }
    }


    public static WorldItemPickup SpawnDroppedItem(InventoryItemData itemData, Vector3 worldPosition, int quantity = 1)
    {
        // do not create anything if there is no valid item to drop
        if (itemData == null)
        {
            return null;
        }

        if (itemData.pickupPrefab != null)
        {
            WorldItemPickup pickupInstance = Object.Instantiate(itemData.pickupPrefab, worldPosition, Quaternion.identity);
            pickupInstance.SetItemData(itemData, quantity);
            return pickupInstance;
        }

        // Build a simple pickup object in code when dropping items from the inventory.
        GameObject pickupObject = new GameObject(itemData.itemName + "_Pickup");
        pickupObject.transform.position = worldPosition;

        // render the dropped item in the world using the item icon
        SpriteRenderer renderer = pickupObject.AddComponent<SpriteRenderer>();
        renderer.sprite = itemData.icon;
        renderer.sortingOrder = 10;

        // create a trigger so the player can pick the dropped item back up
        CircleCollider2D circleCollider = pickupObject.AddComponent<CircleCollider2D>();
        circleCollider.radius = 0.35f;
        circleCollider.isTrigger = true;

        WorldItemPickup pickup = pickupObject.AddComponent<WorldItemPickup>();
        pickup.itemData = itemData;
        pickup.quantity = quantity;
        pickup.spriteRenderer = renderer;

        return pickup;
    }

    public void SetItemData(InventoryItemData newItemData, int newQuantity)
    {
        itemData = newItemData;
        quantity = newQuantity;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer != null && itemData != null && itemData.icon != null && spriteRenderer.GetComponent<Animator>() == null)
        {
            spriteRenderer.sprite = itemData.icon;
        }
    }
}
