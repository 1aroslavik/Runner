using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    private Rigidbody2D rb;
    private Image healthFill;

    private PlayerStats stats;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();

        var hud = GameObject.Find("HealthFill");
        if (hud != null)
            healthFill = hud.GetComponent<Image>();
        else
            Debug.LogError("❌ PlayerHealth: HealthFill НЕ найден!");

        if (stats != null)
        {
            maxHealth = (int)stats.maxHealth;
            currentHealth = (int)stats.currentHealth;
        }
        else
        {
            currentHealth = maxHealth;
        }

        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        float reduced = amount * (1f - stats.defence / 100f);
        int finalDamage = Mathf.RoundToInt(reduced);

        Debug.Log($"🛡 DEF={stats.defence} → входящий {amount} → итог {finalDamage}");

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        stats.currentHealth = currentHealth;

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

        // Сохраняем статы чтобы не потерялись
        PermanentStats.Instance.SaveFrom(GetComponent<PlayerStats>());

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        GameStateManager.Instance?.HandlePlayerDeath(gameObject);
    }




    public void Respawn(int healthToRestore)
    {
        currentHealth = healthToRestore;
        stats.currentHealth = currentHealth;

        UpdateUI();
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
}
