using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    private PlayerStats stats;
    private Rigidbody2D rb;
    private Image healthFill;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();

        if (stats == null)
        {
            Debug.LogError("❌ PlayerHealth: PlayerStats не найден!");
        }

        // Находим полосу HP
        healthFill = GameObject.Find("HealthFill")?.GetComponent<Image>();
        if (healthFill == null)
            Debug.LogError("❌ PlayerHealth: объект HealthFill не найден!");

        Respawn();
    }

    public void TakeDamage(int amount)
    {
        stats.currentHealth -= amount;
        if (stats.currentHealth < 0) stats.currentHealth = 0;

        UpdateUI();

        if (stats.currentHealth <= 0)
            Die();
    }

    public void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = stats.currentHealth / stats.maxHealth;
    }

    void Die()
    {
        Debug.Log("💀 Игрок погиб!");

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        if (GameStateManager.Instance != null)
            GameStateManager.Instance.HandlePlayerDeath(gameObject);
    }

    public void Respawn()
    {
        stats.currentHealth = stats.maxHealth;
        UpdateUI();

        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
