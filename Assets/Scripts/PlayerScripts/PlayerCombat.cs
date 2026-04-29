using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public float knockbackForce = 50f;
    public Transform attackPoint;
    public float weaponRange = 1f;
    public LayerMask enemyLayer;
    public int baseDamage = 1;
    public int damage = 1;
    public float knockTime = 0.5f;
    private float timer;
    private PlayerInventory playerInventory;
    [SerializeField] private AudioSource swordPlayer;

    private void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
    }

    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    

    public void Attack()
    {
        if (playerInventory != null && playerInventory.IsInventoryOpen)
        {
            return;
        }
        if(timer <= 0)
        {
            animator.SetBool("isAttacking", true);
            timer = Statsmanager.instance.attackSpeed;

            if (swordPlayer) swordPlayer.Play();
        }
    //    animator.SetBool("isAttacking", true);
    }

    public void DealDamage()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, enemyLayer);
        if (hitColliders.Length > 0)
        {
            hitColliders[0].GetComponent<EnemyHealth>().ChangeHealth(-damage);
            hitColliders[0].GetComponent<EnemyKnockback>().Knockback(transform, knockbackForce, knockTime, Statsmanager.instance.knockbackStun);
        }

    }

    public void finishAttack()
    {
        animator.SetBool("isAttacking", false);
    }

    private void OnEnable()
    {
        // reconnect to the inventory when this component becomes active
        if (playerInventory == null) playerInventory = GetComponent<PlayerInventory>();

        if (playerInventory != null)
        {
            // keep damage in sync when the selected slot or its contents change
            playerInventory.SelectedSlotChanged += HandleSelectedSlotChanged;
            playerInventory.InventoryChanged += HandleInventoryChanged;
        }

        // update damage immediately in case a weapon is already selected
        RefreshEquippedWeaponDamage();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            // stop listening once this component is disabled to avoid duplicate event hooks
            playerInventory.SelectedSlotChanged -= HandleSelectedSlotChanged;
            playerInventory.InventoryChanged -= HandleInventoryChanged;
        }
    }

    private void HandleSelectedSlotChanged(int _)
    {
        // switching hotbar slots can change which weapon is equipped
        RefreshEquippedWeaponDamage();
    }

    private void HandleInventoryChanged()
    {
        // moving or removing the equipped item should also refresh damage
        RefreshEquippedWeaponDamage();
    }

    public void RefreshEquippedWeaponDamage()
    {
        // start from the player's normal damage before applying any weapon bonus
        damage = baseDamage;

        if (playerInventory == null) return;

        // only the currently selected hotbar item can act as the equipped weapon
        InventorySlot selectedSlot = playerInventory.GetSelectedSlot();
        if (selectedSlot == null || selectedSlot.IsEmpty)
        {
            return;
        }

        // weapon items add bonus damage while they are selected
        WeaponItem equippedWeapon = selectedSlot.item as WeaponItem;
        if (equippedWeapon == null) return;

        damage += equippedWeapon.damageBonus;
    }
}
