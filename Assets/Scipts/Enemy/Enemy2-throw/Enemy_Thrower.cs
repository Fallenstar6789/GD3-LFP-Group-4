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

    
    [Header("Line of Sight")]
    [Tooltip("Layersthat block attacks: walls, pillars, closed doors. ")]
    public LayerMask wallLayer;

    [Tooltip("Approx. radius of the weapon, used so it stops at walls instead of clipping into them")]
    public float weaponRadius = 0.2f;

    [Tooltip("How long to wait before trying again if the attack was cancelled by a wall")]
    public float blockedRetryDelay = 0.3f;

    [Header("Sound")]
    public AudioSource audioSource;

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
        rb.freezeRotation = true; //stop enemy from rotating after touching walls

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

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

            
            if (distanceToPlayer <= attackRange && canAttack && HasLineOfSight())
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

        
        if (!HasLineOfSight())
        {
            SetEnemyColor(baseColor);
            isAttacking = false;

            yield return new WaitForSeconds(blockedRetryDelay);

            canAttack = true;
            yield break;
        }

        // --------------------------------
        // 3. LAUNCH WEAPON
        // --------------------------------

        if (audioSource != null)
            audioSource.Play();

        Vector3 startWorldPosition = weapon.position;

        // Fixed destination in a straight line
        Vector3 targetPosition =
            startWorldPosition +
            (Vector3)(attackDirection * weaponDistance);

       
        bool hitWall = false;

        while (Vector3.Distance(
            weapon.position, targetPosition) > 0.05f)
        {
            
            Vector3 nextPosition = Vector3.MoveTowards(
                weapon.position,
                targetPosition,
                weaponSpeed * Time.deltaTime
            );

            
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

        
        if (!hitWall)
        {
            weapon.position = targetPosition;
        }

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