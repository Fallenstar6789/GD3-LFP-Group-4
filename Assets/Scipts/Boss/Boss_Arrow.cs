
using UnityEngine;

public class Boss_Arrow : MonoBehaviour
{
    [Header("Arrow Settings")]
    public float speed = 12f;
    public float lifetime = 5f;
    public float rotationOffset = 0f;

    private Vector2 moveDirection = Vector2.right;
    private bool launched = false;
    private float lifeTimer = 0f;

    public void Launch(Vector2 direction)
    {
        moveDirection = direction.normalized;

        if (moveDirection.sqrMagnitude < 0.001f)
            moveDirection = Vector2.right;

        float angle = Mathf.Atan2(
            moveDirection.y,
            moveDirection.x
        ) * Mathf.Rad2Deg + rotationOffset;

        transform.rotation = Quaternion.Euler(
            0f, 0f, angle
        );

        launched = true;
    }

    private void Update()
    {
        if (!launched)
            return;

        // SUPERHOT time dilation compatible
        transform.position +=
            (Vector3)(moveDirection * speed * Time.deltaTime);

        lifeTimer += Time.deltaTime;

        if (lifeTimer >= lifetime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
