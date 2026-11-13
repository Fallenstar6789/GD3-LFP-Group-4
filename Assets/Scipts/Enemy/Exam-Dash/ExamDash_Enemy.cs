using UnityEngine;
using System.Collections;

public class ExamDash_Enemy : MonoBehaviour
{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Rigidbody2D rb;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float lockOnTime = 0.5f;

    [Header("Dash")]
    public float dashRange = 3f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;

    [Tooltip("How long the enemy stops before dashing")]
    public float dashWindup = 0.4f;

    [Tooltip("How long the enemy stops after finishing a dash")]
    public float dashRecoveryTime = 0.75f;

    [Tooltip("Time before the enemy can dash again")]
    public float dashCooldown = 1.5f;

    // CHANGED: replaced the three GameObject fields (baseState, windupState,
    // attackState) with one SpriteRenderer and three Sprite assets.
    [Header("Enemy Visual States (Sprites)")]
    [Tooltip("The SpriteRenderer whose sprite gets swapped. Auto-found if left empty.")]
    public SpriteRenderer enemyRenderer;

    public Sprite baseSprite;
    public Sprite windupSprite;
    public Sprite attackSprite;

    [Header("Rotation")]
    public float rotationOffset = -90f;

    private bool lockedOn = false;

    private bool isDashing = false;
    private bool isPreparingDash = false;
    private bool isRecovering = false;

    private bool canDash = true;

    private float detectionTimer = 0f;

    private Vector2 moveDirection;
    private Vector2 dashDirection;

    // =====================================
    // INITIAL SETUP
    // =====================================

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        // Prevent physics collisions from spinning enemy
        if (rb != null)
            rb.freezeRotation = true;

        // NEW: find the SpriteRenderer automatically if it wasn't assigned
        if (enemyRenderer == null)
            enemyRenderer = GetComponentInChildren<SpriteRenderer>();

        // CHANGED: start with the base sprite (was SetEnemyState(baseState))
        SetEnemySprite(baseSprite);
    }

    // =====================================
    // DETECTION AND CHASE
    // =====================================

    private void Update()
    {
        if (player == null || rb == null)
            return;

        float distanceToPlayer = Vector2.Distance(
            rb.position,
            player.position
        );

        // PLAYER DETECTION
        if (!lockedOn)
        {
            if (distanceToPlayer <= detectionRange)
            {
                detectionTimer += Time.deltaTime;

                if (detectionTimer >= lockOnTime)
                {
                    lockedOn = true;
                }
            }
            else
            {
                detectionTimer = 0f;
            }
        }

        // NORMAL CHASE
        if (lockedOn &&
            !isDashing &&
            !isPreparingDash &&
            !isRecovering)
        {
            moveDirection =
                ((Vector2)player.position - rb.position).normalized;

            // Start dash when close enough
            if (distanceToPlayer <= dashRange && canDash)
            {
                StartCoroutine(Dash());
            }
        }
    }

    // =====================================
    // MOVEMENT
    // =====================================

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // DASH
        if (isDashing)
        {
            rb.linearVelocity =
                dashDirection * dashSpeed;

            return;
        }

        // WINDUP
        if (isPreparingDash)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // RECOVERY
        if (isRecovering)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // NORMAL CHASE
        if (lockedOn)
        {
            rb.linearVelocity =
                moveDirection * moveSpeed;

            // Rotate toward player
            float angle = Mathf.Atan2(
                moveDirection.y,
                moveDirection.x
            ) * Mathf.Rad2Deg + rotationOffset;

            rb.rotation = angle;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    // =====================================
    // DASH ATTACK
    // =====================================

    private IEnumerator Dash()
    {
        canDash = false;

        // =================================
        // 1. WINDUP
        // =================================

        isPreparingDash = true;
        rb.linearVelocity = Vector2.zero;

        // CHANGED: swap to the windup sprite (was SetEnemyState(windupState))
        SetEnemySprite(windupSprite);

        yield return new WaitForSeconds(dashWindup);

        // =================================
        // 2. LOCK DASH DIRECTION
        // =================================

        dashDirection = moveDirection.normalized;

        // =================================
        // 3. DASH
        // =================================

        isPreparingDash = false;
        isDashing = true;

        // CHANGED: swap to the attack sprite (was SetEnemyState(attackState))
        SetEnemySprite(attackSprite);

        yield return new WaitForSeconds(dashDuration);

        // =================================
        // 4. END DASH
        // =================================

        isDashing = false;
        rb.linearVelocity = Vector2.zero;

        // CHANGED: back to the base sprite (was SetEnemyState(baseState))
        SetEnemySprite(baseSprite);

        // =================================
        // 5. RECOVERY
        // =================================

        isRecovering = true;

        yield return new WaitForSeconds(dashRecoveryTime);

        isRecovering = false;

        // =================================
        // 6. CHASE AGAIN
        // =================================

        if (player != null)
        {
            moveDirection =
                ((Vector2)player.position - rb.position).normalized;
        }

        // =================================
        // 7. DASH COOLDOWN
        // =================================

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    // =====================================
    // VISUAL STATE SWITCHING
    // =====================================

    // CHANGED: replaced SetEnemyState(GameObject), which toggled three
    // GameObjects on/off, with a single sprite swap on one SpriteRenderer.
    // If a sprite slot is left empty, the current sprite is kept so the
    // enemy never turns invisible by accident.
    private void SetEnemySprite(Sprite newSprite)
    {
        if (enemyRenderer == null || newSprite == null)
            return;

        enemyRenderer.sprite = newSprite;
    }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    // =====================================
    // DEBUG GIZMOS
    // =====================================

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            dashRange
        );
    }
}