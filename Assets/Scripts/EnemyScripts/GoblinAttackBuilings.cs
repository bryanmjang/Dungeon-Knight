using UnityEngine;

public class GoblinAttackBulidings : MonoBehaviour
{
    public int damage = 1;
    public Transform attackPoint;
    public float weaponRange = 0.8f;
    public LayerMask playerLayer;
    public float knockbackForce = 55f;
    public float StunTime = 0.5f;

    private SpawnedGoblinEnemy goblinEnemy;

    private void Awake()
    {
        goblinEnemy = GetComponentInParent<SpawnedGoblinEnemy>();
    }

    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);
        if (hits.Length > 0)
        {
            hits[0].GetComponent<BuildingHealth>()?.TakeDamage(damage);
            goblinEnemy?.StartPostHitIdle();
        }
    }
}
