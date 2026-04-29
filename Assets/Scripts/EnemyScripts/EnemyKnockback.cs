using System.Collections;
using UnityEngine;

public class EnemyKnockback : MonoBehaviour
{
    private Rigidbody2D rb;
    private GoblinEnemy goblinEnemy;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        goblinEnemy = GetComponent<GoblinEnemy>();
    }
    public void Knockback(Transform playerTransform, float knockbackForce, float knockTime, float stunTime)
    {
        goblinEnemy.ChangeState(GoblinEnemyState.Knockback);
        StartCoroutine(StunTimer(knockTime, stunTime));
        Vector2 direction = (transform.position - playerTransform.position).normalized;
        rb.linearVelocity = direction * knockbackForce;

    }

    IEnumerator StunTimer(float knockTime, float stunTime)
    {
        yield return new WaitForSeconds(knockTime);
        rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(stunTime);
        goblinEnemy.ChangeState(GoblinEnemyState.Idle);
    }
}
