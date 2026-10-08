
using UnityEngine;
using System.Collections;

public class Enemy_Thrower : MonoBehaviour
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

    [Header("Enemy Colors")]
    public SpriteRenderer cubeRenderer;
    public SpriteRenderer triRenderer;

    public Color baseColor = new Color32(22, 0, 255, 255);
    public Color attackColor = Color.red;

    [Header("Rotation")]
    public float rotationOffset = -90f;

    private bool lockedOn = false;
    private bool isAttacking = false;
    private bool canAttack = true;

    private float detectionTimer = 0f;
    private Vector2 moveDirection;

    private Vector3 weaponStartPosition;
    private Vector2 attackDirection;

    private void Start()
    {
        SetEnemyColor(baseColor);

        if (weapon != null)
        {
            weaponStartPosition = weapon.localPosition;
        }
    }

    private void Update()
    {
        if (player == null) return;

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

    private void FixedUpdate()
    {
        if (isAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

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


    private IEnumerator Attack()
    {
        if (weapon == null || player == null)
            yield break;

        canAttack = false;
        isAttacking = true;

        rb.linearVelocity = Vector2.zero;

        // --------------------------------
        // 1. LOCK DIRECTION IMMEDIATELY
        // --------------------------------

        // Calculate the direction only once.
        attackDirection =
            ((Vector2)player.position - rb.position).normalized;

        // The enemy will not update this direction
        // during windup, launch, or return.

        // --------------------------------
        // 2. WINDUP
        // --------------------------------

        SetEnemyColor(attackColor);

        yield return new WaitForSeconds(attackWindup);

        // --------------------------------
        // 3. LAUNCH WEAPON
        // --------------------------------

        Vector3 startWorldPosition = weapon.position;

        // Fixed destination in a straight line
        Vector3 targetPosition =
            startWorldPosition +
            (Vector3)(attackDirection * weaponDistance);

        while (Vector3.Distance(
            weapon.position, targetPosition) > 0.05f)
        {
            weapon.position = Vector3.MoveTowards(
                weapon.position,
                targetPosition,
                weaponSpeed * Time.deltaTime
            );

            yield return null;
        }

        weapon.position = targetPosition;

        // --------------------------------
        // 4. RETURN WEAPON
        // --------------------------------

        while (Vector3.Distance(
            weapon.position, startWorldPosition) > 0.05f)
        {
            weapon.position = Vector3.MoveTowards(
                weapon.position,
                startWorldPosition,
                returnSpeed * Time.deltaTime
            );

            yield return null;
        }

        weapon.localPosition = weaponStartPosition;

        // --------------------------------
        // 5. RECOVERY
        // --------------------------------

        SetEnemyColor(baseColor);

        yield return new WaitForSeconds(attackRecovery);

        isAttacking = false;

        // --------------------------------
        // 6. COOLDOWN
        // --------------------------------

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }


    private void SetEnemyColor(Color newColor)
    {
        if (cubeRenderer != null)
            cubeRenderer.color = newColor;

        if (triRenderer != null)
            triRenderer.color = newColor;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position, detectionRange
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position, attackRange
        );
    }
}
