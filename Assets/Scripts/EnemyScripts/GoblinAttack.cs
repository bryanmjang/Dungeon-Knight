using UnityEngine;

public class GoblinAttack : MonoBehaviour
{
    public int damage = 1;
    public Transform attackPoint;
    public float weaponRange = 0.8f;
    public LayerMask playerLayer;
    public float knockbackForce = 55f;
    public float StunTime = 0.5f;

    private GoblinEnemy goblinEnemy;

    [SerializeField] private AudioSource attackSoundPlayer;

    private void Awake()
    {
        goblinEnemy = GetComponentInParent<GoblinEnemy>();
    }

    public void Attack()
    {
        attackSoundPlayer.Play();
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);
        if (hits.Length > 0)
        {
            hits[0].GetComponent<HealthTracker>().GiveDamage(damage);
            hits[0].GetComponent<PlayerMovement>().Knockback(transform, knockbackForce, StunTime);
            goblinEnemy?.StartPostHitIdle();
        }
    }
}
