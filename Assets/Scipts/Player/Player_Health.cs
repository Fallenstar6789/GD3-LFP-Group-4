using UnityEngine;

public class Player_Health : MonoBehaviour
{
    [Header("Death Screen")]
    public GameObject endScreen;

    private bool isDead = false;

    private void Start()
    {
        // Make sure end screen starts hidden
        if (endScreen != null)
        {
            endScreen.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Enemy weapon hit player
        if (collision.CompareTag("Enemy_weapon") && !isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // Stop the game
        Time.timeScale = 0f;

        // Display end screen
        if (endScreen != null)
        {
            endScreen.SetActive(true);
        }
    }
}