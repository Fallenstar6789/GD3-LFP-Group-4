using UnityEngine;
using System.Collections;

public class Enemy_Controller : MonoBehaviour
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

    [Header("Enemy Colors")]
    public SpriteRenderer cubeRenderer;
    public SpriteRenderer triRenderer;

    // Base Color = #1600FF
    public Color baseColor = new Color32(22, 0, 255, 255);

    // Attack Color = Red
    public Color attackColor = Color.red;

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


    private void Start()
    {
        // Set both parts to their normal color
        SetEnemyColor(baseColor);
    }


    void Update()
    {
        float distanceToPlayer = Vector2.Distance(
            transform.position,
            player.position
        );


        // --------------------------------
        // PLAYER DETECTION
        // --------------------------------

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


        // --------------------------------
        // NORMAL CHASE
        // --------------------------------

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


    private void FixedUpdate()
    {
        // --------------------------------
        // DASH
        // --------------------------------

        if (isDashing)
        {
            rb.linearVelocity =
                dashDirection * dashSpeed;

            return;
        }


        // --------------------------------
        // WINDUP
        // --------------------------------

        if (isPreparingDash)
        {
            rb.linearVelocity = Vector2.zero;

            return;
        }


        // --------------------------------
        // RECOVERY
        // --------------------------------

        if (isRecovering)
        {
            rb.linearVelocity = Vector2.zero;

            return;
        }


        // --------------------------------
        // NORMAL CHASE
        // --------------------------------

        if (lockedOn)
        {
            rb.linearVelocity =
                moveDirection * moveSpeed;


            // Rotate toward player
            float angle =
                Mathf.Atan2(
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


    private IEnumerator Dash()
    {
        canDash = false;


        // =================================
        // 1. WINDUP
        // =================================

        isPreparingDash = true;

        rb.linearVelocity = Vector2.zero;


        // TURN BOTH PARTS RED
        SetEnemyColor(attackColor);


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

        // Enemy stays red during dash

        yield return new WaitForSeconds(dashDuration);


        // =================================
        // 4. END DASH
        // =================================

        isDashing = false;

        rb.linearVelocity = Vector2.zero;


        // RETURN BOTH PARTS TO PURPLE
        SetEnemyColor(baseColor);


        // =================================
        // 5. RECOVERY
        // =================================

        isRecovering = true;

        yield return new WaitForSeconds(dashRecoveryTime);

        isRecovering = false;


        // =================================
        // 6. CHASE AGAIN
        // =================================

        moveDirection =
            ((Vector2)player.position - rb.position).normalized;


        // =================================
        // 7. DASH COOLDOWN
        // =================================

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }


    // =====================================
    // COLOR CONTROL
    // =====================================

    private void SetEnemyColor(Color newColor)
    {
        if (cubeRenderer != null)
        {
            cubeRenderer.color = newColor;
        }

        if (triRenderer != null)
        {
            triRenderer.color = newColor;
        }
    }


    private void OnDrawGizmosSelected()
    {
        // Detection Range
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Dash Range
        Gizmos.DrawWireSphere(
            transform.position,
            dashRange
        );
    }
}