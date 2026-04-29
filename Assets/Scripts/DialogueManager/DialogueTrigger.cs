using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using XNode;

public class DialogueTrigger : MonoBehaviour
{
    // could've probably done better by adding a supermanager, but this is already a stitching job, and it doesn't matter for now
    public DialogueTree tree;
    public Button interactButton;
    public GameObject dialogue_pop_up;
    public PlayerInput playerInput;

    private TextMesh interactPrompt;
    private InputAction interactAction;
    private bool isPlayerInRange;
    private bool isSubscribed;

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        CreateInteractPrompt();
        SetPromptVisible(false);

        if (player == null)
        {
            Debug.Log("Couldn't find a player");
            return;
        }

        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.Log("Couldn't find a player");
            return;
        }

        interactAction = playerInput.actions.FindActionMap("Player").FindAction("Interact");
    }

    public void TriggerDialog(InputAction.CallbackContext context)
    {
        if (!isPlayerInRange)
        {
            return;
        }

        if (dialogue_pop_up != null)
        {
            dialogue_pop_up.SetActive(false);
        }

        SetPromptVisible(false);
        FindAnyObjectByType<DialogueManager>().StartDialogue(tree.nodes[0]);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        isPlayerInRange = true;
        SetPromptVisible(true);

        if (interactAction != null && !isSubscribed)
        {
            interactAction.performed += TriggerDialog;
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

        if (dialogue_pop_up != null)
        {
            dialogue_pop_up.SetActive(false);
        }

        if (interactAction != null && isSubscribed)
        {
            interactAction.performed -= TriggerDialog;
            isSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (interactAction != null && isSubscribed)
        {
            interactAction.performed -= TriggerDialog;
        }
    }

    private void CreateInteractPrompt()
    {
        // create a simple text prompt above the NPC without needing extra UI setup
        GameObject promptObject = new GameObject("InteractPrompt");
        promptObject.transform.SetParent(transform);
        promptObject.transform.localPosition = new Vector3(0f, 0.3f, 100f);

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
