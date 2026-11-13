
using UnityEngine;
using TMPro;
using System.Collections;

public class Boss_Health : MonoBehaviour
{
    [Header("Boss Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Boss Health UI")]
    public TMP_Text bossHealthText;

    [Header("Damage Settings")]
    public float damageCooldown = 0.3f;

    [Header("Damage Feedback")]
    public Color damageColor = Color.white;
    public float damageFlashDuration = 0.1f;

    private bool canTakeDamage = true;
    private bool isDead = false;

    private SpriteRenderer[] allRenderers;
    private Color[] originalColors;

    // =====================================
    // INITIAL SETUP
    // =====================================

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

        UpdateHealthUI();
    }

    // =====================================
    // DAMAGE DETECTION
    // =====================================

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            TakeDamage(1);
        }
    }

    // =====================================
    // TAKE DAMAGE
    // =====================================

    public void TakeDamage(int damage)
    {
        if (isDead || !canTakeDamage)
            return;

        currentHealth = Mathf.Max(
            0,
            currentHealth - damage
        );

        Debug.Log("Boss Health: " + currentHealth);

        // Update UI whenever health changes
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(DamageFeedback());
        }
    }

    // =====================================
    // UPDATE HEALTH UI
    // =====================================

    private void UpdateHealthUI()
    {
        if (bossHealthText != null)
        {
            bossHealthText.text =
                "BOSS HEALTH: " +
                currentHealth +
                " / " +
                maxHealth;
        }
    }

    // =====================================
    // DAMAGE FEEDBACK
    // =====================================

    private IEnumerator DamageFeedback()
    {
        canTakeDamage = false;

        // Flash all boss visual states
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
            {
                allRenderers[i].color = damageColor;
            }
        }

        yield return new WaitForSeconds(
            damageFlashDuration
        );

        // Restore original colours
        for (int i = 0; i < allRenderers.Length; i++)
        {
            if (allRenderers[i] != null)
            {
                allRenderers[i].color = originalColors[i];
            }
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

    // =====================================
    // BOSS DEATH
    // =====================================

    private void Die()
    {
        isDead = true;

        Debug.Log("Boss Defeated!");

        // UI shows 0 / 10 before boss is destroyed
        UpdateHealthUI();

        Destroy(gameObject);
    }
}
