using UnityEngine;
using UnityEngine.InputSystem;

public class DungeonChest : MonoBehaviour
{
    // Different reward behaviors a dungeon chest can use.
    public enum ChestRewardType
    {
        Coins,
        Weapon,
        KeyItem,
        Trap
    }

    [SerializeField] private ChestRewardType rewardType;
    [SerializeField] private int rewardAmount = 1;
    [SerializeField] private InventoryItemData rewardItem;
    [SerializeField] private string rewardItemResourcePath;
    [SerializeField] private string trapPrefabResourcePath;
    [SerializeField] private Color openedTint = new Color(0.75f, 0.75f, 0.75f, 1f);

    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip weaponSound;
    [SerializeField] private AudioClip keyItemSound;
    [SerializeField] private AudioClip trapSound;
    [SerializeField] private AudioSource audioPlayer;

    private bool isOpened;
    private SpriteRenderer spriteRenderer;
    private Color closedTint = Color.white;
    private PlayerInventory playerInRange;
    private TextMesh interactPrompt;

    private void Awake()
    {
        // cache the chest sprite so opened and closed visuals can be swapped quickly
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            closedTint = spriteRenderer.color;
        }

        // chests use a trigger so the player can stand nearby and press E
        Collider2D chestCollider = GetComponent<Collider2D>();
        if (chestCollider != null)
        {
            chestCollider.isTrigger = true;
        }

        // build the small world-space prompt shown while the player is in range
        CreateInteractPrompt();
        SetPromptVisible(false);
    }

    private void Update()
    {
        // only allow interaction while the player is nearby and the chest is unopened
        if (isOpened || playerInRange == null)
        {
            return;
        }

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenChest(playerInRange);
        }
    }

    public void Configure(ChestRewardType newRewardType, int newRewardAmount, string itemPath = "", string trapPath = "")
    {
        // runtime setup uses this to quickly assign chest behavior by scene object name
        rewardType = newRewardType;
        rewardAmount = Mathf.Max(1, newRewardAmount);
        rewardItemResourcePath = itemPath;
        trapPrefabResourcePath = trapPath;
    }

    public void SetRewardItem(InventoryItemData newRewardItem)
    {
        // lets runtime setup provide a direct asset reference for chest rewards
        rewardItem = newRewardItem;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpened || !other.CompareTag("Player"))
        {
            return;
        }

        // store the nearby player's inventory so the chest can open on key press
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            playerInventory = other.GetComponentInParent<PlayerInventory>();
        }

        playerInRange = playerInventory;
        SetPromptVisible(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // clear the nearby player reference once they leave the interaction zone
        PlayerInventory playerInventory = other.GetComponent<PlayerInventory>();
        if (playerInventory == null)
        {
            playerInventory = other.GetComponentInParent<PlayerInventory>();
        }

        if (playerInRange == playerInventory)
        {
            playerInRange = null;
            SetPromptVisible(false);
        }
    }

    public void OpenChest(PlayerInventory playerInventory)
    {
        // opened chests can only be used once
        isOpened = true;
        playerInRange = null;
        SetPromptVisible(false);
        ApplyOpenedVisual();

        // disable the trigger so the chest cannot be reopened
        Collider2D chestCollider = GetComponent<Collider2D>();
        if (chestCollider != null)
        {
            chestCollider.enabled = false;
        }

        // added this for ease of chest reloading (i.e. after saving)
        if (!playerInventory) return;


        AudioClip soundChoice = null;

        switch (rewardType)
        {
            case ChestRewardType.Coins:
                GiveReward(playerInventory, rewardItem, rewardItemResourcePath, rewardAmount);
                Debug.Log($"{name} opened: awarded {rewardAmount} coins.");
                soundChoice = coinSound;
                break;

            case ChestRewardType.Weapon:
                GiveReward(playerInventory, rewardItem, rewardItemResourcePath, rewardAmount);
                Debug.Log($"{name} opened: awarded weapon item.");
                soundChoice = weaponSound;
                break;

            case ChestRewardType.KeyItem:
                GiveReward(playerInventory, rewardItem, rewardItemResourcePath, rewardAmount);
                Debug.Log($"{name} opened: awarded key item.");
                soundChoice = keyItemSound;
                break;

            case ChestRewardType.Trap:
                SpawnTrap();
                Debug.Log($"{name} opened: trap activated.");
                soundChoice = trapSound;
                break;
        }

        gameObject.GetComponent<ChestSaver>().MarkAsOpen();

        if (soundChoice != null && audioPlayer != null)
        {
            audioPlayer.PlayOneShot(soundChoice);
        }    

    }

    private void GiveReward(PlayerInventory playerInventory, InventoryItemData directItem, string resourcePath, int quantity)
    {
        // prefer an item assigned directly in the inspector, then fall back to a Resources path
        InventoryItemData itemData = directItem;

        if (itemData == null && !string.IsNullOrWhiteSpace(resourcePath))
        {
            itemData = Resources.Load<InventoryItemData>(resourcePath);
        }

        if (itemData == null)
        {
            Debug.LogWarning($"{name} has no valid reward item assigned.");
            return;
        }

        // if the inventory is full, drop the reward into the world near the chest
        bool addedToInventory = playerInventory != null && playerInventory.AddItem(itemData, quantity);
        if (!addedToInventory)
        {
            Vector3 dropPosition = transform.position + Vector3.down * 0.5f;
            WorldItemPickup.SpawnDroppedItem(itemData, dropPosition, quantity);
        }
    }

    private void SpawnTrap()
    {
        // fake chests can spawn a prefab instead of giving loot
        if (string.IsNullOrWhiteSpace(trapPrefabResourcePath))
        {
            return;
        }

        GameObject trapPrefab = Resources.Load<GameObject>(trapPrefabResourcePath);
        if (trapPrefab == null)
        {
            Debug.LogWarning($"{name} could not load trap prefab from Resources/{trapPrefabResourcePath}.");
            return;
        }

        Vector3 spawnPosition = transform.position + Vector3.right * 1.25f;
        Instantiate(trapPrefab, spawnPosition, Quaternion.identity);
    }

    private void ApplyOpenedVisual()
    {
        // tint and flip the chest sprite to make the opened state obvious
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            return;
        }

        spriteRenderer.color = openedTint;
        spriteRenderer.flipY = true;
    }

    public void ResetVisual()
    {
        // restore the chest to its unopened state when the scene is initialized
        isOpened = false;
        playerInRange = null;
        SetPromptVisible(false);

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = closedTint;
            spriteRenderer.flipY = false;
        }

        Collider2D chestCollider = GetComponent<Collider2D>();
        if (chestCollider != null)
        {
            chestCollider.enabled = true;
        }
    }

    private void CreateInteractPrompt()
    {
        // create a simple text prompt above the chest without needing extra UI setup
        GameObject promptObject = new GameObject("InteractPrompt");
        promptObject.transform.SetParent(transform);
        promptObject.transform.localPosition = new Vector3(0f, -0.12f, 0f);

        interactPrompt = promptObject.AddComponent<TextMesh>();
        interactPrompt.text = "Press E";
        interactPrompt.characterSize = 0.12f;
        interactPrompt.fontSize = 32;
        interactPrompt.anchor = TextAnchor.MiddleCenter;
        interactPrompt.alignment = TextAlignment.Center;
        interactPrompt.color = Color.white;
    }

    private void SetPromptVisible(bool visible)
    {
        // hide the prompt after the chest has already been opened
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(visible && !isOpened);
        }
    }
}
