using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class GoblinEnemy : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform player;
    private int facingDirection = 1; // 1 for right, -1 for left
    private Animator animator;
    private Vector2 spawnPosition;
    private float lostPlayerTimer = 0f;
    public float lostPlayerDelay = 1f;
    public float attackRange = 1f;
    public GoblinEnemyState enemyState;
    public float postAttackStopTime = 0.5f;
    private Coroutine attackPauseCoroutine;
    public float attackCooldown = 1f;
    public float attackTimer = 0.5f;
    public float playerDetectionRange = 5f;
    public Transform detectionPoint;
    public LayerMask playerLayer;


    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spawnPosition = transform.position;
        ChangeState(GoblinEnemyState.Idle);
    }

    void Update()
    {
        if (enemyState == GoblinEnemyState.Knockback)
            return;
        CheckForPlayer();

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        if (enemyState == GoblinEnemyState.Run)
        {
            Chase();
        }
        else if (enemyState == GoblinEnemyState.ReturnToSpawn)
        {
            MoveToSpawn();
        }
        else if (enemyState == GoblinEnemyState.Attack_Right ||
                 enemyState == GoblinEnemyState.Attack_Down ||
                 enemyState == GoblinEnemyState.Attack_Up)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    private IEnumerator EndAttackPause(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        attackPauseCoroutine = null;
        attackTimer = attackCooldown;

        if (player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (dist > attackRange)
                ChangeState(GoblinEnemyState.Run);
            else
                ChangeState(GoblinEnemyState.Idle);
        }
        else
        {
            if (Vector2.Distance(transform.position, spawnPosition) > 0.1f)
                ChangeState(GoblinEnemyState.ReturnToSpawn);
            else
                ChangeState(GoblinEnemyState.Idle);
        }
    }

    public void StartPostHitIdle()
    {
        if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);

        animator.SetBool("isAttacking", false);
        animator.SetBool("isAttackDown", false);
        animator.SetBool("isAttackUp", false);
        animator.SetBool("isChasing", false);
        animator.SetBool("isIdle", true);
        enemyState = GoblinEnemyState.Idle;
        rb.linearVelocity = Vector2.zero;

        attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
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

    void MoveToSpawn()
    {
        if (spawnPosition.x > transform.position.x && facingDirection == -1)
            Flip();
        else if (spawnPosition.x < transform.position.x && facingDirection == 1)
            Flip();

        Vector2 direction = ((Vector3)spawnPosition - transform.position).normalized;
        rb.linearVelocity = direction * 3.5f;

        if (Vector2.Distance(transform.position, spawnPosition) <= 0.1f)
        {
            transform.position = spawnPosition;
            rb.linearVelocity = Vector2.zero;
            ChangeState(GoblinEnemyState.Idle);
        }
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
        if (attackPauseCoroutine != null)
            return;
        Collider2D[] hitColliders =
            Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectionRange, playerLayer);

        if (hitColliders.Length > 0)
        {
            player = hitColliders[0].transform;
            lostPlayerTimer = 0f;

            float dist = Vector2.Distance(transform.position, player.position);

            if (dist <= attackRange && attackTimer <= 0)
            {
                attackTimer = attackCooldown;

                if (player.position.y < transform.position.y - 0.2f)
                {
                    ChangeState(GoblinEnemyState.Attack_Down);
                }
                else if (player.position.y > transform.position.y + 0.2f)
                {
                    ChangeState(GoblinEnemyState.Attack_Up);
                }
                else
                {
                    ChangeState(GoblinEnemyState.Attack_Right);
                }
            }
            else if (dist > attackRange && (enemyState != GoblinEnemyState.Attack_Up || enemyState != GoblinEnemyState.Attack_Down || enemyState != GoblinEnemyState.Attack_Right))
            {
                ChangeState(GoblinEnemyState.Run);
            }
        }
        else
        {
            lostPlayerTimer += Time.deltaTime;
            if (lostPlayerTimer >= lostPlayerDelay)
            {
                player = null;
                if (Vector2.Distance(transform.position, spawnPosition) > 0.1f)
                    ChangeState(GoblinEnemyState.ReturnToSpawn);
                else
                {
                    rb.linearVelocity = Vector2.zero;
                    ChangeState(GoblinEnemyState.Idle);
                }
            }
        }
    }

    public void ChangeState(GoblinEnemyState state)
    {
        if (enemyState == GoblinEnemyState.Idle)
            animator.SetBool("isIdle", false);
        else if (enemyState == GoblinEnemyState.Run || enemyState == GoblinEnemyState.ReturnToSpawn)
            animator.SetBool("isChasing", false);
        else if (enemyState == GoblinEnemyState.Attack_Right)
            animator.SetBool("isAttacking", false);
        else if (enemyState == GoblinEnemyState.Attack_Down)
            animator.SetBool("isAttackDown", false);
        else if (enemyState == GoblinEnemyState.Attack_Up)
            animator.SetBool("isAttackUp", false);

        enemyState = state;

        if (enemyState == GoblinEnemyState.Idle)
            animator.SetBool("isIdle", true);
        else if (enemyState == GoblinEnemyState.Run || enemyState == GoblinEnemyState.ReturnToSpawn)
            animator.SetBool("isChasing", true);
        else if (enemyState == GoblinEnemyState.Attack_Right)
        {
            animator.SetBool("isAttacking", true);
            rb.linearVelocity = Vector2.zero;
            if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);
            attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
        }
        else if (enemyState == GoblinEnemyState.Attack_Down)
        {
            animator.SetBool("isAttackDown", true);
            rb.linearVelocity = Vector2.zero;
            if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);
            attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
        }
        else if (enemyState == GoblinEnemyState.Attack_Up)
        {
            animator.SetBool("isAttackUp", true);
            rb.linearVelocity = Vector2.zero;
            if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);
            attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
        }
    }
}

public enum GoblinEnemyState
{
    Idle,
    Run,
    ReturnToSpawn,
    Attack_Right,
    Attack_Down,
    Attack_Up,
    Knockback
}