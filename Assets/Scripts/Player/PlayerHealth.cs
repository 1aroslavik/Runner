using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    private Rigidbody2D rb;
    private Image healthFill;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        var hud = GameObject.Find("HealthFill");
        if (hud != null)
            healthFill = hud.GetComponent<Image>();
        else
            Debug.LogError("❌ PlayerHealth: HealthFill НЕ найден!");

        Respawn(maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        UpdateUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log("💀 Игрок погиб!");

        // Вырубаем управление и физику, но НЕ отключаем объект!
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        // Сообщаем менеджеру игры
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.HandlePlayerDeath(gameObject);
        }
    }

    public void Respawn(int healthToRestore)
    {
        currentHealth = healthToRestore;
        UpdateUI();

        // Возвращаем физику
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
