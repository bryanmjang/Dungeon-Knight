using System.Collections;
using UnityEngine;

public class GoblinBulidingAttack : MonoBehaviour
{
    private Rigidbody2D rb;
    private Transform target;           // current chase/attack target (building or player)
    private bool targetingPlayer = false;
    private int facingDirection = 1;
    private Animator animator;
    private Vector2 spawnPosition;
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
    public int damage = 1;
    public float attackDamageDelay = 0.2f;
    public float knockbackForce = 55f;
    public float StunTime = 0.5f;

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

        FindTarget();

        if (attackTimer > 0)
            attackTimer -= Time.deltaTime;

        if (enemyState == GoblinEnemyState.Run)
            Chase();
        else if (enemyState == GoblinEnemyState.ReturnToSpawn)
            MoveToSpawn();
        else if (enemyState == GoblinEnemyState.Attack_Right ||
                 enemyState == GoblinEnemyState.Attack_Down ||
                 enemyState == GoblinEnemyState.Attack_Up)
            rb.linearVelocity = Vector2.zero;
    }

    // Finds the closest building; if none, switches to targeting the player.
    private void FindTarget()
    {
        GameObject[] buildings = GameObject.FindGameObjectsWithTag("Player Building");

        if (buildings.Length > 0)
        {
            targetingPlayer = false;
            float closestDist = float.MaxValue;
            foreach (GameObject b in buildings)
            {
                float dist = Vector2.Distance(transform.position, b.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    target = b.transform;
                }
            }

            if (enemyState != GoblinEnemyState.Attack_Right &&
                enemyState != GoblinEnemyState.Attack_Down &&
                enemyState != GoblinEnemyState.Attack_Up)
                ChangeState(GoblinEnemyState.Run);
        }
        else
        {
            // All buildings destroyed — switch to chasing the player.
            if (!targetingPlayer)
            {
                targetingPlayer = true;
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    target = playerObj.transform;
                    Debug.Log("[GoblinBulidingAttack] Buildings gone — switching to player.");
                    ChangeState(GoblinEnemyState.Run);
                }
                else
                {
                    target = null;
                }
            }

            // Distance-based attack on player (no collision needed).
            if (target != null && attackTimer <= 0 &&
                enemyState != GoblinEnemyState.Attack_Right &&
                enemyState != GoblinEnemyState.Attack_Down &&
                enemyState != GoblinEnemyState.Attack_Up)
            {
                float dist = Vector2.Distance(transform.position, target.position);
                if (dist <= attackRange)
                {
                    attackTimer = attackCooldown;
                    if (target.position.y < transform.position.y - 0.2f)
                        ChangeState(GoblinEnemyState.Attack_Down);
                    else if (target.position.y > transform.position.y + 0.2f)
                        ChangeState(GoblinEnemyState.Attack_Up);
                    else
                        ChangeState(GoblinEnemyState.Attack_Right);
                }
            }
        }
    }

    private IEnumerator DealDamage()
    {
        yield return new WaitForSeconds(attackDamageDelay);
        if (target == null) yield break;

        if (targetingPlayer)
        {
            HealthTracker ht = target.GetComponent<HealthTracker>();
            if (ht != null)
                ht.GiveDamage(damage);
            else
                Debug.Log($"[GoblinBulidingAttack] {target.name} has no HealthTracker!");

            PlayerMovement pm = target.GetComponent<PlayerMovement>();
            if (pm != null)
                pm.Knockback(transform, knockbackForce, StunTime);
        }
        else
        {
            BuildingHealth bh = target.GetComponent<BuildingHealth>();
            if (bh != null)
                bh.TakeDamage(damage);
            else
                Debug.Log($"[GoblinBulidingAttack] {target.name} has no BuildingHealth!");
        }
    }

    private IEnumerator EndAttackPause(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        attackPauseCoroutine = null;
        attackTimer = attackCooldown;

        if (target != null)
        {
            float dist = Vector2.Distance(transform.position, target.position);
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
        if (target == null) return;

        if (target.position.x > transform.position.x && facingDirection == -1)
            Flip();
        else if (target.position.x < transform.position.x && facingDirection == 1)
            Flip();

        Vector2 direction = (target.position - transform.position).normalized;
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (targetingPlayer) return; // player attacks handled by distance in FindTarget()

        if (!collision.gameObject.CompareTag("Player Building"))
            return;

        target = collision.transform;

        if (attackTimer <= 0 &&
            enemyState != GoblinEnemyState.Attack_Right &&
            enemyState != GoblinEnemyState.Attack_Down &&
            enemyState != GoblinEnemyState.Attack_Up)
        {
            attackTimer = attackCooldown;

            if (target.position.y < transform.position.y - 0.2f)
                ChangeState(GoblinEnemyState.Attack_Down);
            else if (target.position.y > transform.position.y + 0.2f)
                ChangeState(GoblinEnemyState.Attack_Up);
            else
                ChangeState(GoblinEnemyState.Attack_Right);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!targetingPlayer &&
            collision.gameObject.CompareTag("Player Building") &&
            enemyState != GoblinEnemyState.Attack_Right &&
            enemyState != GoblinEnemyState.Attack_Down &&
            enemyState != GoblinEnemyState.Attack_Up)
            ChangeState(GoblinEnemyState.Run);
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
            StartCoroutine(DealDamage());
        }
        else if (enemyState == GoblinEnemyState.Attack_Down)
        {
            animator.SetBool("isAttackDown", true);
            rb.linearVelocity = Vector2.zero;
            if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);
            attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
            StartCoroutine(DealDamage());
        }
        else if (enemyState == GoblinEnemyState.Attack_Up)
        {
            animator.SetBool("isAttackUp", true);
            rb.linearVelocity = Vector2.zero;
            if (attackPauseCoroutine != null) StopCoroutine(attackPauseCoroutine);
            attackPauseCoroutine = StartCoroutine(EndAttackPause(postAttackStopTime));
            StartCoroutine(DealDamage());
        }
    }
}
