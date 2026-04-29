using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerMovement : MonoBehaviour
{
    // Basic Items for Player Movement
    [SerializeField] private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;

    // Dash variables
    private Vector2 moveDirection;
    private bool isDashing = false;
    [SerializeField] private float dashSpeed = 2.0f;
    [SerializeField] private float dashTime = 0.3f;

    private bool isKnockedback = false;
    private Coroutine knockbackCoroutine;

    public bool canMove = true;

    public PlayerCombat playerCombat;

    // Inventory variables
    private PlayerInventory playerInventory;
    private Keyboard keyboard;

    [SerializeField] private AudioSource dashSoundPlayer;

    public StatsUI statsui;

    private void Start()
    {
         // Spawn player at door if moving between scenes
        if(PlayerPrefs.HasKey("SpawnX") && PlayerPrefs.HasKey("SpawnY"))
        {
            float x = PlayerPrefs.GetFloat("SpawnX");
            float y = PlayerPrefs.GetFloat("SpawnY");
            transform.position = new Vector3(x, y, transform.position.z);

            PlayerPrefs.DeleteKey("SpawnX");
            PlayerPrefs.DeleteKey("SpawnY");
        }
        animator = GetComponent<Animator>();
        playerInventory = GetComponent<PlayerInventory>();
        keyboard = Keyboard.current;
    }

    // Update is called once per frame
    void Update()
    {
        if (keyboard != null && (keyboard.iKey.wasPressedThisFrame || keyboard.tabKey.wasPressedThisFrame))
        {
            playerInventory.ToggleInventory();
        }

        HandleHotbarNumberKeys();

        if (!canMove || (playerInventory != null && playerInventory.IsInventoryOpen)) 
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }
        if (Input.GetButtonDown("Attack"))
        {
            playerCombat.Attack();
        }
        if (!isDashing && !isKnockedback)
            rb.linearVelocity = moveInput * Statsmanager.instance.speed;

        if (Input.GetButtonDown("OpenStats"))
        {
            statsui.ToggleStats();
        }

    }

    public void Move(InputAction.CallbackContext context)
    {
        // disable movement when inventory is open
        if (playerInventory != null && playerInventory.IsInventoryOpen)
        {
            moveInput = Vector2.zero;
            animator.SetBool("isWalking", false);
            return;
        }

        moveInput = context.ReadValue<Vector2>();

        // determine if moving
        if (moveInput == Vector2.zero)
        {
            animator.SetBool("isWalking", false);
        }
        else
        {
            moveDirection = moveInput;
            animator.SetBool("isWalking", true);

            // determine direction
            gameObject.GetComponent<SpriteRenderer>().flipX = moveInput.x < 0;
        }
    }

    // disables player update loop, and adds a constant velocity for a time. 
    public void Dash(InputAction.CallbackContext context)
    {
        // Disable is inventory is open
        if (playerInventory != null && playerInventory.IsInventoryOpen)
        {
            return;
        }

        isDashing = true;
        rb.linearVelocity += moveDirection * Statsmanager.instance.speed * dashSpeed;
        if (dashSoundPlayer & dashSoundPlayer.clip) dashSoundPlayer.Play();
        StartCoroutine(EndDashInSeconds(dashTime));
    }

    // terminates dashing state within the provided number of seconds.
    private IEnumerator EndDashInSeconds(float delayTime)
    {
        // Wait for the specified amount of time
        yield return new WaitForSeconds(delayTime);

        // This code will run after the delay
        isDashing = false;
    }

    public void Knockback(Transform enemy, float force, float stunTime)
    {
        Vector2 direction = (transform.position - enemy.position).normalized;
        if (knockbackCoroutine != null) StopCoroutine(knockbackCoroutine);
        knockbackCoroutine = StartCoroutine(ApplyKnockback(direction * force, stunTime));
    }

    private IEnumerator ApplyKnockback(Vector2 velocity, float duration)
    {
        isKnockedback = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            rb.linearVelocity = velocity;
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        rb.linearVelocity = Vector2.zero;
        isKnockedback = false;
        knockbackCoroutine = null;
    }

    private void HandleHotbarNumberKeys()
    {
        if (keyboard == null || playerInventory == null)
        {
            return;
        }

        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(0);
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(1);
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(2);
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(3);
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(4);
        }
        else if (keyboard.digit6Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(5);
        }
        else if (keyboard.digit7Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(6);
        }
        else if (keyboard.digit8Key.wasPressedThisFrame)
        {
            playerInventory.SetSelectedSlot(7);
        }
    }
}
