using UnityEngine;

public class Camera_Follow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followSpeed = 2f;
    public float yOffset = 1f;
    public Transform target;

    [Header("Zoom Settings")]
    public float normalSize = 5f;
    public float movingSize = 7f;
    public float zoomSpeed = 3f;

    private Camera cam;
    private Player_Controller playerController;

    private void Start()
    {
        cam = GetComponent<Camera>();

        // Get the Player_Controller from the target
        playerController = target.GetComponent<Player_Controller>();
    }

    void Update()
    {
        // -----------------------------
        // CAMERA FOLLOW
        // -----------------------------

        Vector3 newPos = new Vector3(
            target.position.x,
            target.position.y + yOffset,
            -10f
        );

        transform.position = Vector3.Slerp(
            transform.position,
            newPos,
            followSpeed * Time.deltaTime
        );


        // -----------------------------
        // CHECK PLAYER MOVEMENT
        // -----------------------------

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        bool isMoving = horizontal != 0 || vertical != 0;


        // -----------------------------
        // CHECK PLAYER DASH
        // -----------------------------

        bool isDashing = playerController != null &&
                         playerController.isDashing;


        // -----------------------------
        // CAMERA ZOOM
        // -----------------------------

        // Zoom out if walking OR dashing
        bool shouldZoomOut = isMoving || isDashing;

        float targetSize = shouldZoomOut
            ? movingSize
            : normalSize;

        cam.orthographicSize = Mathf.Lerp(
            cam.orthographicSize,
            targetSize,
            zoomSpeed * Time.deltaTime
        );
    }
}