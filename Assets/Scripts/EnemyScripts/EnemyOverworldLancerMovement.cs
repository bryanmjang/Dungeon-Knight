using UnityEngine;
using UnityEngine.XR;

public class EnemyOverworldLancerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform player;
    private int facingDirection = 1; // 1 for right, -1 for left
    private Animator animator;
    public float attackRange = 0.8f;
    public EnemyState enemyState;
    public float attackCooldown = 1f;
    public float attackTimer = 0.5f;
    public float playerDetectionRange = 5f;
    public Transform detectionPoint;
    public LayerMask playerLayer;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); 
        ChangeState(EnemyState.Idle);

    }

    void Update()
    {
        CheckForPlayer();
        if (attackTimer > 0) {
            attackTimer -= Time.deltaTime;
        }
        if (enemyState == EnemyState.Chasing)
        {
            Chase();
        }
        else if (enemyState == EnemyState.Attacking)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    void Chase()
    {

        if (player.position.x > transform.position.x && facingDirection == -1)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * 3.5f;
    }

    void Flip()
    {
        facingDirection *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void CheckForPlayer()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectionRange, playerLayer);

        if (hitColliders.Length > 0)
        {
            player = hitColliders[0].transform;
            if (Vector2.Distance(transform.position, player.transform.position) <= attackRange && attackTimer <= 0)
            {
                attackTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
            }
            else if (Vector2.Distance(transform.position, player.position) > attackRange)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }
    }

    void ChangeState(EnemyState state)
    {
        if(enemyState == EnemyState.Idle)
            animator.SetBool("isIdle", false);
        else if (enemyState == EnemyState.Chasing)
            animator.SetBool("isChasing", false);
        else if (enemyState == EnemyState.Attacking)
            animator.SetBool("isAttacking", false);
        enemyState = state;
        if (enemyState == EnemyState.Idle)
            animator.SetBool("isIdle", true);
        else if (enemyState == EnemyState.Chasing)
            animator.SetBool("isChasing", true);
        else if (enemyState == EnemyState.Attacking)
            animator.SetBool("isAttacking", true);
    }


}

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
}