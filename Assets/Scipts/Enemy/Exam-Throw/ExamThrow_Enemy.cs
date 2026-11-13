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

    // NEW: line of sight / wall collision settings
    [Header("Line of Sight")]
    [Tooltip("Layer(s) that block attacks: walls, pillars, closed doors. Do NOT include the Player or enemies.")]
    public LayerMask wallLayer;

    [Tooltip("Approx. radius of the weapon, used so it stops at walls instead of clipping into them")]
    public float weaponRadius = 0.2f;

    [Tooltip("How long to wait before trying again if the attack was cancelled by a wall")]
    public float blockedRetryDelay = 0.3f;

    [Header("Sound")]
    public AudioSource audioSource;

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

        // NEW: find the SpriteRenderer automatically if it wasn't assigned.
        // The weapon is excluded, because it may have its own SpriteRenderer
        // and we don't want to swap the weapon's sprite by accident.
        if (enemyRenderer == null)
        {
            foreach (SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
            {
                if (weapon != null && sr.transform.IsChildOf(weapon)) continue;
                enemyRenderer = sr;
                break;
            }
        }

        // CHANGED: start with the base sprite (was SetEnemyState(baseState))
        SetEnemySprite(baseSprite);
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

            // CHANGED: added HasLineOfSight() so the wind-up never starts
            // when a wall is between the enemy and the player.
            // The enemy keeps chasing until it can see the player.
            if (distanceToPlayer <= attackRange && canAttack && HasLineOfSight())
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

    // NEW: returns true only if no wall lies on the straight line
    // between this enemy and the player.
    private bool HasLineOfSight()
    {
        if (player == null) return false;

        RaycastHit2D hit = Physics2D.Linecast(
            rb.position,
            player.position,
            wallLayer
        );

        return hit.collider == null;
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

        // CHANGED: swap to the windup sprite (was SetEnemyState(windupState))
        SetEnemySprite(windupSprite);

        yield return new WaitForSeconds(attackWindup);

        // NEW: second line-of-sight check right before firing.
        // The player may have ducked behind a wall during the wind-up.
        // If so, cancel the attack instead of firing into the wall.
        if (!HasLineOfSight())
        {
            SetEnemySprite(baseSprite);
            isAttacking = false;

            yield return new WaitForSeconds(blockedRetryDelay);

            canAttack = true;
            yield break;
        }

        // =================================
        // 3. LAUNCH WEAPON
        // =================================

        // CHANGED: swap to the attack sprite (was SetEnemyState(attackState))
        SetEnemySprite(attackSprite);

        if (audioSource != null)
            audioSource.Play();

        Vector3 startWorldPosition = weapon.position;

        Vector3 targetPosition =
            startWorldPosition +
            (Vector3)(attackDirection * weaponDistance);

        // NEW: tracks whether the weapon hit a wall on the way out
        bool hitWall = false;

        // Weapon travels forward
        while (Vector3.Distance(
            weapon.position,
            targetPosition
        ) > 0.05f)
        {
            // CHANGED: work out where the weapon WOULD move this frame first
            Vector3 nextPosition = Vector3.MoveTowards(
                weapon.position,
                targetPosition,
                weaponSpeed * Time.deltaTime
            );

            // NEW: sweep a circle along this frame's movement.
            // If it touches a wall, stop the weapon at the point of contact.
            Vector2 travel = nextPosition - weapon.position;

            RaycastHit2D wallHit = Physics2D.CircleCast(
                weapon.position,
                weaponRadius,
                travel.normalized,
                travel.magnitude,
                wallLayer
            );

            if (wallHit.collider != null)
            {
                // centroid = where the centre of the circle was at impact
                weapon.position = new Vector3(
                    wallHit.centroid.x,
                    wallHit.centroid.y,
                    weapon.position.z
                );

                hitWall = true;
                break; // stop flying outward, go straight to return phase
            }

            weapon.position = nextPosition;

            yield return null;
        }

        // CHANGED: only snap to the full target if the weapon did NOT hit a wall.
        // Previously this always ran, which teleported the weapon through walls.
        if (!hitWall)
        {
            weapon.position = targetPosition;
        }

        // =================================
        // 4. RETURN WEAPON
        // =================================

        // Unchanged. The weapon returns along the path it came from,
        // which is clear, so it works for both full and wall-stopped throws.
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

        // CHANGED: back to the base sprite (was SetEnemyState(baseState))
        SetEnemySprite(baseSprite);

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
            attackRange
        );
    }
}