using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInventory))]
public class PlayerConsumables : MonoBehaviour
{
    [SerializeField] private Key useConsumableKey = Key.Q;

    private PlayerInventory playerInventory;
    private PlayerCombat playerCombat;
    private HealthTracker healthTracker;
    private Keyboard keyboard;
    private Coroutine activeBuffCoroutine;

    private void Awake()
    {
        playerInventory = GetComponent<PlayerInventory>();
        playerCombat = GetComponent<PlayerCombat>();
        healthTracker = GetComponent<HealthTracker>();
    }

    private void Start()
    {
        keyboard = Keyboard.current;
    }

    private void Update()
    {
        if (keyboard == null) keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (playerInventory != null && playerInventory.IsInventoryOpen) return;

        if (keyboard[useConsumableKey].wasPressedThisFrame) TryUseSelectedConsumable();
    }

    private void TryUseSelectedConsumable()
    {
        if (playerInventory == null) return;

        InventorySlot selectedSlot = playerInventory.GetSelectedSlot();
        if (selectedSlot == null || selectedSlot.IsEmpty) return;

        ConsumableItem consumable = selectedSlot.item as ConsumableItem;
        if (consumable == null) return;

        if (consumable.effectType == ConsumableItem.ConsumableEffectType.Heal && healthTracker.isMax()) return;

        ApplyConsumable(consumable);
        playerInventory.RemoveFromSlot(playerInventory.SelectedSlotIndex, 1);
    }

    private void ApplyConsumable(ConsumableItem consumable)
    {
        if (consumable == null)
        {
            return;
        }

        switch (consumable.effectType)
        {
            case ConsumableItem.ConsumableEffectType.Heal:
                ApplyHealing(consumable.amount);
                break;

            case ConsumableItem.ConsumableEffectType.DamageBuff:
                ApplyDamageBuff(consumable.amount, consumable.duration);
                break;

        }
    }

    private void ApplyHealing(int amount)
    {
        if (healthTracker == null || amount <= 0) return;

        healthTracker.GiveHealth(amount);
    }

    private void ApplyDamageBuff(int amount, float duration)
    {
        if (playerCombat == null || amount == 0 || duration <= 0f) return;

        if (activeBuffCoroutine != null) StopCoroutine(activeBuffCoroutine);
        activeBuffCoroutine = StartCoroutine(DamageBuffRoutine(amount, duration)); 
    }

    private IEnumerator DamageBuffRoutine(int amount, float duration)
    {
        playerCombat.baseDamage += amount;
        playerCombat.RefreshEquippedWeaponDamage();

        yield return new WaitForSeconds(duration);

        playerCombat.baseDamage -= amount;
        playerCombat.RefreshEquippedWeaponDamage();
        activeBuffCoroutine = null;
    }
}