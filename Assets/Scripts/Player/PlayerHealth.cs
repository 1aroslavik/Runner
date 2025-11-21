using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    private Image healthFill;

    void Awake()
    {
        currentHealth = maxHealth;

        // ИЩЕМ HealthFill АВТОМАТИЧЕСКИ
        var hud = GameObject.Find("HealthFill");
        if (hud != null)
            healthFill = hud.GetComponent<Image>();
        else
            Debug.LogError("❌ PlayerHealth: HealthFill не найден в сцене!");
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }
}
