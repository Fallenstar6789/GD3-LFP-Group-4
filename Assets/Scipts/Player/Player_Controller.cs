using UnityEngine;
using System.Collections;

public class Player_Controller : MonoBehaviour
{
    [Header("Movement & Combat")]
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    [Header("Weapon")]
    public GameObject weapon;

    [Header("Dash Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    [Header("Time Dilation Settings")]
    [Tooltip("Time scale while dilation is active")]
    public float minTimeScale = 0.05f;

    [Tooltip("Normal gameplay time scale")]
    public float maxTimeScale = 1f;

    [Tooltip("Speed of time scale transitions")]
    public float timeChangeSpeed = 10f;

    [Header("Time Dilation Ability")]
    public KeyCode dilationKey = KeyCode.LeftShift;

    public float dilationDuration = 4f;
    public float dilationCooldown = 5f;

    public SpriteRenderer playerRenderer;
    public Color dilationColor = Color.cyan;

    private Vector2 moveDirection;
    private Vector2 mousePosition;

    public bool isDashing = false;
    private bool canDash = true;

    private Vector2 dashDirection;

    // Time dilation state
    private bool dilationActive = false;
    private float dilationTimer = 0f;
    private float cooldownTimer = 0f;

    private Color originalColor = Color.white;

    // UI cooldown bar values
    public bool IsDilationActive => dilationActive;
    public float DilationTimeRemaining => dilationTimer;
    public float DilationCooldownRemaining => cooldownTimer;
    public bool CanUseDilation =>
        !dilationActive && cooldownTimer <= 0f;

    private void Start()
    {
        Time.timeScale = 0f;

        if (EndScreen_Manager.startActive == false)
        {
            Time.timeScale = 1f;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        weapon.SetActive(false);

        if (playerRenderer == null)
        {
            playerRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (playerRenderer != null)
        {
            originalColor = playerRenderer.color;
        }
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return;

        // -----------------------------
        // PLAYER INPUT
        // -----------------------------

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection =
            new Vector2(moveX, moveY).normalized;

        // -----------------------------
        // MOUSE POSITION
        // -----------------------------

        mousePosition =
            Camera.main.ScreenToWorldPoint(
                Input.mousePosition
            );

        // -----------------------------
        // DASH INPUT
        // -----------------------------

        if (Input.GetMouseButtonDown(0) && canDash)
        {
            StartCoroutine(Dash());
        }

        // -----------------------------
        // TIME DILATION ABILITY
        // -----------------------------

        float realDelta = Time.unscaledDeltaTime;

        // Cooldown timer
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= realDelta;
            cooldownTimer = Mathf.Max(0f, cooldownTimer);
        }

        // Active ability timer
        if (dilationActive)
        {
            dilationTimer -= realDelta;

            if (dilationTimer <= 0f)
            {
                dilationActive = false;
                dilationTimer = 0f;

                cooldownTimer = dilationCooldown;
            }
        }

        // Activate ONLY when Shift is pressed
        if (Input.GetKeyDown(dilationKey) && CanUseDilation)
        {
            dilationActive = true;
            dilationTimer = dilationDuration;
        }

        // -----------------------------
        // PLAYER COLOUR
        // -----------------------------

        if (playerRenderer != null)
        {
            playerRenderer.color =
                dilationActive
                ? dilationColor
                : originalColor;
        }

        // -----------------------------
        // TIME SYSTEM
        // -----------------------------

        if (isDashing)
        {
            // Dashing always runs at normal speed
            Time.timeScale = 1f;

            weapon.SetActive(true);
        }
        else
        {
            weapon.SetActive(false);

            // Time dilation is now ONLY based
            // on whether Shift activated the ability

            float targetTimeScale =
                dilationActive
                ? minTimeScale
                : maxTimeScale;

            Time.timeScale = Mathf.Lerp(
                Time.timeScale,
                targetTimeScale,
                Time.unscaledDeltaTime * timeChangeSpeed
            );
        }

        // Keep physics synchronized
        Time.fixedDeltaTime =
            0.02f * Time.timeScale;
    }

    private void FixedUpdate()
    {
        // -----------------------------
        // MOVEMENT / DASH
        // -----------------------------

        if (isDashing)
        {
            rb.linearVelocity =
                dashDirection * dashSpeed;
        }
        else
        {
            rb.linearVelocity =
                moveDirection * moveSpeed;
        }

        // -----------------------------
        // PLAYER ROTATION
        // -----------------------------

        Vector2 aimDirection =
            mousePosition - rb.position;

        float aimAngle =
            Mathf.Atan2(
                aimDirection.y,
                aimDirection.x
            ) * Mathf.Rad2Deg - 90f;

        rb.rotation = aimAngle;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        // -----------------------------
        // DASH TOWARD MOUSE
        // -----------------------------

        dashDirection =
            (mousePosition - rb.position).normalized;

        // Restore normal time during dash
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // -----------------------------
        // DASH
        // -----------------------------

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        // -----------------------------
        // DASH COOLDOWN
        // -----------------------------

        yield return new WaitForSecondsRealtime(
            dashCooldown
        );

        canDash = true;
    }

    private void OnDisable()
    {
        // Prevent slow motion persisting
        // if the player controller is disabled
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}