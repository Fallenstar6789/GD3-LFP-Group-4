
using UnityEngine;
using System.Collections;

public class Boss_Health : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Damage Settings")]
    public float damageCooldown = 0.3f;

    [Header("Damage Feedback")]
    public Color damageColor = Color.white;
    public float damageFlashDuration = 0.1f;

    private bool canTakeDamage = true;
    private bool isDead = false;

    private SpriteRenderer[] allRenderers;
    private Color[] originalColors;

    private void Start()
    {
        currentHealth = maxHealth;

        allRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        originalColors = new Color[allRenderers.Length];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            originalColors[i] = allRenderers[i].color;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage)
            return;

        currentHealth = Mathf.Max(
            0,
            currentHealth - damage
        );

        Debug.Log("Boss Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFeedback());
        }
    }

    private IEnumerator DamageFeedback()
    {
        canTakeDamage = false;

        // Flash all visual states
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
                allRenderers[i].color = damageColor;
        }

        yield return new WaitForSeconds(
            damageFlashDuration
        );

        // Restore original colours
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
                allRenderers[i].color = originalColors[i];
        }

        float remainingCooldown = Mathf.Max(
            0f,
            damageCooldown - damageFlashDuration
        );

        yield return new WaitForSeconds(
            remainingCooldown
        );

        canTakeDamage = true;
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("Boss Defeated!");

        Destroy(gameObject);
    }
}
