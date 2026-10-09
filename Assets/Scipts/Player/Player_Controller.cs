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
    [Tooltip("The lowest time scale when standing perfectly still")]
    public float minTimeScale = 0.05f;

    [Tooltip("Normal time scale when moving")]
    public float maxTimeScale = 1.0f;

    [Tooltip("How quickly time transitions between slow-mo and normal speed")]
    public float timeChangeSpeed = 10f;

    [Tooltip("How much moving the mouse speeds up time")]
    public float mouseSensitivityForTime = 0.75f;

    [Header("Time Dilation Ability")] // vuyi edit
    [Tooltip("Press the shift key to activate time dilation")] // vuyi edit
    public KeyCode dilationKey = KeyCode.LeftShift; // vuyi edit

    [Tooltip("How long time dilation stays active once activated")] // vuyi edit
    public float dilationDuration = 4f; // vuyi edit

    [Tooltip("Time dilation cooldown period")] // vuyi edit
    public float dilationCooldown = 5f; // vuyi edit

    public SpriteRenderer playerRenderer; // vuyi edit

    [Tooltip("Colour the player turns while time dilation is active")] // vuyi edit
    public Color dilationColor = Color.cyan; // vuyi edit


    private Vector2 moveDirection;
    private Vector2 mousePosition;

    public bool isDashing = false;
    private bool canDash = true;

    private Vector2 dashDirection;

    // Time dilation ability state // vuyi edit
    private bool dilationActive = false; // vuyi edit
    private float dilationTimer = 0f; // vuyi edit
    private float cooldownTimer = 0f; // vuyi edit

    private Color originalColor = Color.white; // vuyi edit

    // values for cooldown bar) // vuyi edit
    public bool IsDilationActive => dilationActive; // vuyi edit
    public float DilationTimeRemaining => dilationTimer; // vuyi edit
    public float DilationCooldownRemaining => cooldownTimer; // vuyi edit
    public bool CanUseDilation => !dilationActive && cooldownTimer <= 0f; // vuyi edit

    private void Start()
    {
        weapon.SetActive(false);

        if (playerRenderer == null) // vuyi edit
        {
            playerRenderer = GetComponentInChildren<SpriteRenderer>(); // vuyi edit
        }

        if (playerRenderer != null) // vuyi edit
        {
            originalColor = playerRenderer.color; // vuyi edit
        }
    }


    void Update()
    {
       
        if (Time.timeScale == 0f) return; // vuyi edit

        // -----------------------------
        // PLAYER INPUT
        // -----------------------------

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;


        // -----------------------------
        // MOUSE POSITION
        // -----------------------------

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);


        // -----------------------------
        // DASH INPUT
        // -----------------------------

        if (Input.GetMouseButtonDown(0) && canDash)
        {
            StartCoroutine(Dash());
        }


        // -----------------------------
        // TIME DILATION ABILITY// vuyi edit
        // -----------------------------

        float realDelta = Time.unscaledDeltaTime; // vuyi edit

        if (cooldownTimer > 0f) // vuyi edit
        {
            cooldownTimer -= realDelta; // vuyi edit
        }

        if (dilationActive) // vuyi edit
        {
            dilationTimer -= realDelta; // vuyi edit

            if (dilationTimer <= 0f) // vuyi edit
            {
                // Ability ends, cooldown starts // vuyi edit
                dilationActive = false; // vuyi edit
                dilationTimer = 0f; // vuyi edit
                cooldownTimer = dilationCooldown; // vuyi edit
            }
        }

        if (Input.GetKeyDown(dilationKey) && CanUseDilation) // vuyi edit
        {
            dilationActive = true; // vuyi edit
            dilationTimer = dilationDuration; // vuyi edit
        }

        // Turn the player cyan while time dilation is active // vuyi edit
        if (playerRenderer != null) // vuyi edit
        {
            playerRenderer.color = dilationActive ? dilationColor : originalColor; // vuyi edit
        }


        // -----------------------------
        // TIME SYSTEM
        // -----------------------------

        if (isDashing)
        {
            // DASHING = FULL SPEED
            Time.timeScale = 1f;

            //sets weapon active
            weapon.SetActive(true);
        }
        else
        {

            //sets weapon inactive
            weapon.SetActive(false);

            float targetTimeScale = maxTimeScale; 

           
            if (dilationActive) // vuyi edit
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                float mouseMagnitude =
                    new Vector2(mouseX, mouseY).magnitude
                    * mouseSensitivityForTime;

                float actionIntensity =
                    Mathf.Clamp01(
                        moveDirection.magnitude +
                        mouseMagnitude
                    );

                targetTimeScale = 
                    Mathf.Lerp(
                        minTimeScale,
                        maxTimeScale,
                        actionIntensity
                    );
            } 

            Time.timeScale =
                Mathf.Lerp(
                    Time.timeScale,
                    targetTimeScale, 
                    Time.unscaledDeltaTime * timeChangeSpeed
                );
        }


        // Keep physics synchronized with time scale
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }


    private void FixedUpdate()
    {
        // -----------------------------
        // MOVEMENT / DASH
        // -----------------------------

        if (isDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
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


        // Immediately restore normal time
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

        yield return new WaitForSecondsRealtime(dashCooldown);

        canDash = true;
    }
}