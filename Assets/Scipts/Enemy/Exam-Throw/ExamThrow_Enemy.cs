using UnityEngine;
using System.Collections;

public class ExamThrow_Enemy : MonoBehaviour




{
    [Header("Player")]
    public Transform player;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public Rigidbody2D rb;

    [Header("Detection")]
    public float detectionRange = 8f;
    public float lockOnTime = 0.5f;

    [Header("Attack")]
    public Transform weapon;
    public float attackRange = 4f;
    public float weaponDistance = 5f;
    public float weaponSpeed = 12f;
    public float returnSpeed = 10f;
    public float attackWindup = 0.4f;
    public float attackRecovery = 0.5f;
    public float attackCooldown = 1.5f;

    [Header("Sound")]
    public AudioSource audioSource;

    [Header("Enemy Visual States")]
    public GameObject baseState;
    public GameObject windupState;
    public GameObject attackState;

    [Header("Rotation")]
    public float rotationOffset = -90f;

    private bool lockedOn = false;
    private bool isAttacking = false;
    private bool canAttack = true;

    private float detectionTimer = 0f;

    private Vector2 moveDirection;
    private Vector2 attackDirection;

    private Vector3 weaponStartPosition;

    // =====================================
    // INITIAL SETUP
    // =====================================

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.freezeRotation = true;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (weapon != null)
            weaponStartPosition = weapon.localPosition;

        // Start with base design active
        SetEnemyState(baseState);
    }

    // =====================================
    // PLAYER DETECTION AND CHASE
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

        // CHASE AND ATTACK
        if (lockedOn && !isAttacking)
        {
            moveDirection =
                ((Vector2)player.position - rb.position).normalized;

            if (distanceToPlayer <= attackRange && canAttack)
            {
                StartCoroutine(Attack());
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

        // Stop while attacking
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Chase player
        if (lockedOn)
        {
            rb.linearVelocity = moveDirection * moveSpeed;

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
    // THROW ATTACK
    // =====================================

    private IEnumerator Attack()
    {
        if (weapon == null || player == null)
            yield break;

        canAttack = false;
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        // =================================
        // 1. LOCK ATTACK DIRECTION
        // =================================

        attackDirection =
            ((Vector2)player.position - rb.position).normalized;

        if (attackDirection.sqrMagnitude < 0.001f)
            attackDirection = Vector2.up;

        // =================================
        // 2. WINDUP
        // =================================

        SetEnemyState(windupState);

        yield return new WaitForSeconds(attackWindup);

        // =================================
        // 3. LAUNCH WEAPON
        // =================================

        SetEnemyState(attackState);

        if (audioSource != null)
            audioSource.Play();

        Vector3 startWorldPosition = weapon.position;

        Vector3 targetPosition =
            startWorldPosition +
            (Vector3)(attackDirection * weaponDistance);

        // Weapon travels forward
        while (Vector3.Distance(
            weapon.position,
            targetPosition
        ) > 0.05f)
        {
            weapon.position = Vector3.MoveTowards(
                weapon.position,
                targetPosition,
                weaponSpeed * Time.deltaTime
            );

            yield return null;
        }

        weapon.position = targetPosition;

        // =================================
        // 4. RETURN WEAPON
        // =================================

        while (Vector3.Distance(
            weapon.position,
            startWorldPosition
        ) > 0.05f)
        {
            weapon.position = Vector3.MoveTowards(
                weapon.position,
                startWorldPosition,
                returnSpeed * Time.deltaTime
            );

            yield return null;
        }

        weapon.localPosition = weaponStartPosition;

        // =================================
        // 5. RECOVERY
        // =================================

        // Return to base design
        SetEnemyState(baseState);

        yield return new WaitForSeconds(attackRecovery);

        isAttacking = false;

        // =================================
        // 6. COOLDOWN
        // =================================

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    // =====================================
    // VISUAL STATE SWITCHING
    // =====================================

    private void SetEnemyState(GameObject activeState)
    {
        if (baseState != null)
            baseState.SetActive(baseState == activeState);

        if (windupState != null)
            windupState.SetActive(windupState == activeState);

        if (attackState != null)
            attackState.SetActive(attackState == activeState);
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
            attackRange
        );
    }
}

