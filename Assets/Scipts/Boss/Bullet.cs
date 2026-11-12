
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float bulletLife = 4f;
    public float speed = 5f;

    private float timer = 0f;

    private void Update()
    {
        // Movement affected by SUPERHOT time dilation
        transform.position +=
            transform.right * speed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= bulletLife)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignore the boss
        if (collision.GetComponentInParent<Boss_Controller>() != null)
            return;

        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore the boss
        if (collision.collider.GetComponentInParent<Boss_Controller>() != null)
            return;

        Destroy(gameObject);
    }
}
