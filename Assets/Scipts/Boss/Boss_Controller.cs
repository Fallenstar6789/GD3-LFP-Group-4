
using UnityEngine;
using System.Collections;

public class Boss_Controller : MonoBehaviour
{
    public enum BossMode
    {
        Dash,
        Arrow,
        BulletHell
    }

    private enum MovementState
    {
        Idle,
        Chase,
        Dash,
        Retreat,
        Stopped
    }

    [Header("Player")]
    public Transform player;
    public Rigidbody2D rb;

    [Header("Detection and Movement")]
    public float detectionRange = 8f;
    public float lockOnTime = 0.5f;
    public float moveSpeed = 3f;

    [Header("Attack Ranges")]
    public float dashRange = 3f;
    public float arrowRange = 5f;
    public float bulletHellRange = 6f;

    [Header("Dash Attack")]
    public float dashWindup = 0.4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;

    [Header("Dash Active Object")]
    public GameObject dashActiveObject;

    [Header("Arrow Attack")]
    public GameObject arrowPrefab;
    public Transform arrowSpawnPoint;
    public float arrowWindup = 0.4f;
    public float arrowSpeed = 12f;

    [Header("Bullet Hell")]
    public BulletSpawner bulletSpawner;
    public float bulletHellWindup = 0.6f;

    [Header("Retreat")]
    public float retreatSpeed = 6f;
    public float retreatDistance = 4f;
    public float retreatMaxDuration = 2f;
    public float modeChangePause = 0.25f;

    [Header("Boss Visual GameObjects")]
    public GameObject bossBase;
    public GameObject bossDash;
    public GameObject bossDashWindup;
    public GameObject bossArrow;
    public GameObject bossArrowWindup;
    public GameObject bossBulletHell;
    public GameObject bossBulletHellWindup;

    [Header("Current Mode")]
    public BossMode currentMode = BossMode.Dash;

    private bool lockedOn = false;
    private bool isBusy = false;

    private float detectionTimer = 0f;
    private float originalRotation;

    private Vector2 moveDirection;
    private Vector2 attackDirection;
    private Vector2 retreatDirection;

    private MovementState movementState = MovementState.Idle;

    // =====================================
    // INITIAL SETUP
    // =====================================

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            originalRotation = rb.rotation;
    }

    private void Start()
    {
        currentMode = BossMode.Dash;

        if (bulletSpawner != null)
            bulletSpawner.automaticFire = false;

        // Start with the base boss design
        SetBossVisual(bossBase);

        // Dash object must start inactive
        if (dashActiveObject != null)
            dashActiveObject.SetActive(false);
    }

    // =====================================
    // PLAYER DETECTION AND CHASE
    // =====================================

    private void Update()
    {
        if (player == null || rb == null)
            return;

        float distance = Vector2.Distance(
            rb.position,
            player.position
        );

        if (!lockedOn)
        {
            if (distance <= detectionRange)
            {
                detectionTimer += Time.deltaTime;

                if (detectionTimer >= lockOnTime)
                {
                    lockedOn = true;
                    movementState = MovementState.Chase;
                    SetModeVisual();
                }
            }
            else
            {
                detectionTimer = 0f;
            }

            return;
        }

        if (isBusy)
            return;

        // Follow player without rotating
        moveDirection =
            ((Vector2)player.position - rb.position).normalized;

        movementState = MovementState.Chase;

        if (distance <= GetAttackRange())
        {
            StartCoroutine(AttackCycle());
        }
    }

    // =====================================
    // MOVEMENT (FIXED ROTATION)
    // =====================================

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        switch (movementState)
        {
            case MovementState.Chase:
                rb.linearVelocity = moveDirection * moveSpeed;
                break;

            case MovementState.Dash:
                rb.linearVelocity = attackDirection * dashSpeed;
                break;

            case MovementState.Retreat:
                rb.linearVelocity =
                    retreatDirection * retreatSpeed;
                break;

            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }

        // Maintain original boss rotation
        rb.MoveRotation(originalRotation);
    }

    // =====================================
    // ATTACK RANGE
    // =====================================

    private float GetAttackRange()
    {
        switch (currentMode)
        {
            case BossMode.Dash:
                return dashRange;

            case BossMode.Arrow:
                return arrowRange;

            case BossMode.BulletHell:
                return bulletHellRange;
        }

        return dashRange;
    }

    // =====================================
    // ATTACK CYCLE
    // =====================================

    private IEnumerator AttackCycle()
    {
        isBusy = true;
        movementState = MovementState.Stopped;
        rb.linearVelocity = Vector2.zero;

        // Lock direction before attack windup
        attackDirection =
            ((Vector2)player.position - rb.position).normalized;

        if (attackDirection.sqrMagnitude < 0.001f)
            attackDirection = Vector2.up;

        switch (currentMode)
        {
            case BossMode.Dash:
                yield return StartCoroutine(DashAttack());
                break;

            case BossMode.Arrow:
                yield return StartCoroutine(ArrowAttack());
                break;

            case BossMode.BulletHell:
                yield return StartCoroutine(BulletHellAttack());
                break;
        }

        // Retreat after attacking
        yield return StartCoroutine(Retreat());

        // Switch to next attack
        ChangeMode();

        movementState = MovementState.Stopped;

        yield return new WaitForSeconds(modeChangePause);

        if (player != null)
        {
            moveDirection =
                ((Vector2)player.position - rb.position).normalized;
        }

        movementState = MovementState.Chase;
        isBusy = false;
    }

    // =====================================
    // DASH ATTACK
    // =====================================

    private IEnumerator DashAttack()
    {
        // Windup
        SetBossVisual(bossDashWindup);
        movementState = MovementState.Stopped;

        if (dashActiveObject != null)
            dashActiveObject.SetActive(false);

        yield return new WaitForSeconds(dashWindup);

        // Begin dash
        SetBossVisual(bossDash);
        movementState = MovementState.Dash;

        if (dashActiveObject != null)
            dashActiveObject.SetActive(true);

        yield return new WaitForSeconds(dashDuration);

        // End dash
        movementState = MovementState.Stopped;
        rb.linearVelocity = Vector2.zero;

        if (dashActiveObject != null)
            dashActiveObject.SetActive(false);
    }

    // =====================================
    // ARROW ATTACK
    // =====================================

    private IEnumerator ArrowAttack()
    {
        SetBossVisual(bossArrowWindup);
        movementState = MovementState.Stopped;

        yield return new WaitForSeconds(arrowWindup);

        SetBossVisual(bossArrow);

        if (arrowPrefab == null || arrowSpawnPoint == null)
        {
            Debug.LogWarning(
                "Boss arrow prefab or spawn point is missing."
            );

            yield break;
        }

        GameObject spawnedArrow = Instantiate(
            arrowPrefab,
            arrowSpawnPoint.position,
            Quaternion.identity
        );

        Boss_Arrow arrowScript =
            spawnedArrow.GetComponent<Boss_Arrow>();

        if (arrowScript != null)
        {
            arrowScript.speed = arrowSpeed;
            arrowScript.Launch(attackDirection);
        }

        // Prevent arrow colliding with boss
        Collider2D[] arrowColliders =
            spawnedArrow.GetComponentsInChildren<Collider2D>();

        Collider2D[] bossColliders =
            GetComponentsInChildren<Collider2D>();

        foreach (Collider2D arrowCollider in arrowColliders)
        {
            foreach (Collider2D bossCollider in bossColliders)
            {
                Physics2D.IgnoreCollision(
                    arrowCollider,
                    bossCollider
                );
            }
        }

        // Arrow moves independently
    }

    // =====================================
    // BULLET HELL ATTACK
    // =====================================

    private IEnumerator BulletHellAttack()
    {
        SetBossVisual(bossBulletHellWindup);
        movementState = MovementState.Stopped;

        yield return new WaitForSeconds(bulletHellWindup);

        SetBossVisual(bossBulletHell);

        if (bulletSpawner != null)
        {
            yield return StartCoroutine(
                bulletSpawner.FireOneSpiral()
            );
        }
    }

    // =====================================
    // RETREAT
    // =====================================

    private IEnumerator Retreat()
    {
        if (player == null)
            yield break;

        retreatDirection =
            (rb.position - (Vector2)player.position).normalized;

        if (retreatDirection.sqrMagnitude < 0.001f)
            retreatDirection = -attackDirection;

        if (retreatDirection.sqrMagnitude < 0.001f)
            retreatDirection = Vector2.up;

        Vector2 startPosition = rb.position;
        float elapsed = 0f;

        movementState = MovementState.Retreat;

        while (
            Vector2.Distance(startPosition, rb.position)
                < retreatDistance
            && elapsed < retreatMaxDuration
        )
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        movementState = MovementState.Stopped;
        rb.linearVelocity = Vector2.zero;
    }

    // =====================================
    // MODE CHANGES
    // =====================================

    private void ChangeMode()
    {
        switch (currentMode)
        {
            case BossMode.Dash:
                currentMode = BossMode.Arrow;
                break;

            case BossMode.Arrow:
                currentMode = BossMode.BulletHell;
                break;

            case BossMode.BulletHell:
                currentMode = BossMode.Dash;
                break;
        }

        SetModeVisual();
    }

    private void SetModeVisual()
    {
        switch (currentMode)
        {
            case BossMode.Dash:
                SetBossVisual(bossDash);
                break;

            case BossMode.Arrow:
                SetBossVisual(bossArrow);
                break;

            case BossMode.BulletHell:
                SetBossVisual(bossBulletHell);
                break;
        }
    }

    // =====================================
    // VISUAL STATE SWITCHING
    // =====================================

    private void SetBossVisual(GameObject activeVisual)
    {
        GameObject[] visuals =
        {
            bossBase,
            bossDash,
            bossDashWindup,
            bossArrow,
            bossArrowWindup,
            bossBulletHell,
            bossBulletHellWindup
        };

        foreach (GameObject visual in visuals)
        {
            if (visual != null)
            {
                visual.SetActive(
                    visual == activeVisual
                );
            }
        }
    }

    // =====================================
    // CLEANUP
    // =====================================

    private void OnDisable()
    {
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (dashActiveObject != null)
            dashActiveObject.SetActive(false);
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            arrowRange
        );

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            bulletHellRange
        );
    }
}
