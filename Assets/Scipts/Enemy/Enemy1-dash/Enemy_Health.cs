using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player weapon hit enemy
        if (collision.CompareTag("Weapon"))
        {
            Destroy(gameObject);
        }
    }
}