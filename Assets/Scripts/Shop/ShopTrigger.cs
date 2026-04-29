using UnityEngine;
using UnityEngine.InputSystem;

public class ShopTrigger : MonoBehaviour
{
    public GameObject shopCanvas;
    public ShopManger shopManger;

    private PlayerMovement playerMovement;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private TextMesh interactPrompt;
    private bool isPlayerInRange;
    private bool isSubscribed;

    private void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
        }

        if (playerInput != null)
        {
            interactAction = playerInput.actions.FindActionMap("Player").FindAction("Interact");
        }

        CreateInteractPrompt();
        SetPromptVisible(false);
    }

    public void OpenShop()
    {
        shopCanvas.SetActive(true);
        SetPromptVisible(false);

        if (playerMovement != null)
        {
            playerMovement.canMove = false;
        }
    }

    public void CloseShop()
    {
        shopCanvas.SetActive(false);
        SetPromptVisible(isPlayerInRange && !IsPromptBlocked());

        if (playerMovement != null)
        {
            playerMovement.canMove = true;
        }
    }

    public void CloseShopTemp()
    {
        shopCanvas.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        playerMovement = collision.GetComponent<PlayerMovement>();
        isPlayerInRange = true;
        SetPromptVisible(!IsPromptBlocked());

        if (interactAction != null && !isSubscribed)
        {
            interactAction.performed += HandleInteract;
            isSubscribed = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = false;
        SetPromptVisible(false);

        if (interactAction != null && isSubscribed)
        {
            interactAction.performed -= HandleInteract;
            isSubscribed = false;
        }

        playerMovement = null;
    }

    public void gotoPreviewMode()
    {
        SetPromptVisible(false);
    }

    public void gotoNormalMode()
    {
        SetPromptVisible(isPlayerInRange && !IsPromptBlocked());
    }

    private void Update()
    {
        if (shopManger != null && shopManger.isViewingZone && shopManger.previewObject != null)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            shopManger.UpdatePreview(mouseWorld);

            if (Input.GetMouseButtonDown(0))
                shopManger.TryPlace(mouseWorld);
        }

        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        if (shopManger != null && shopManger.isViewingZone)
            shopManger.CancelZoneView();
    }

    private void OnDestroy()
    {
        if (interactAction != null && isSubscribed)
        {
            interactAction.performed -= HandleInteract;
        }
    }

    private void HandleInteract(InputAction.CallbackContext context)
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (shopManger != null && shopManger.isViewingZone)
        {
            return;
        }

        if (shopCanvas != null && shopCanvas.activeSelf)
        {
            CloseShop();
            return;
        }

        OpenShop();
    }

    private bool IsPromptBlocked()
    {
        return (shopCanvas != null && shopCanvas.activeSelf)
            || (shopManger != null && shopManger.isViewingZone);
    }

    private void CreateInteractPrompt()
    {
        GameObject promptObject = new GameObject("InteractPrompt");
        promptObject.transform.SetParent(transform);
        promptObject.transform.localPosition = new Vector3(0f, -0.62f, 100f);

        interactPrompt = promptObject.AddComponent<TextMesh>();
        interactPrompt.text = "Press E";
        interactPrompt.characterSize = 0.12f;
        interactPrompt.fontSize = 28;
        interactPrompt.anchor = TextAnchor.MiddleCenter;
        interactPrompt.alignment = TextAlignment.Center;
        interactPrompt.color = Color.white;
    }

    private void SetPromptVisible(bool visible)
    {
        if (interactPrompt != null)
        {
            interactPrompt.gameObject.SetActive(visible);
        }
    }
}
