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


    private Vector2 moveDirection;
    private Vector2 mousePosition;

    public bool isDashing = false;
    private bool canDash = true;

    private Vector2 dashDirection;

    private void Start()
    {
        weapon.SetActive(false);
    }


    void Update()
    {
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

            // Normal SUPERHOT-style time system

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

            float targetTimeScale =
                Mathf.Lerp(
                    minTimeScale,
                    maxTimeScale,
                    actionIntensity
                );

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