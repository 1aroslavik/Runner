using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Параметры здоровья")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("UI")]
    public Image healthFill; // сюда перетащи HealthFill из Canvas

    void Start()
    {
        currentHealth = maxHealth;

        if (healthFill == null)
        {
            var obj = GameObject.Find("HealthFill");
            if (obj != null)
                healthFill = obj.GetComponent<Image>();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;
    }

    void Die()
    {
        Debug.Log("💀 Игрок погиб!");
        // Здесь можешь добавить перезапуск сцены или экран смерти
    }
}
