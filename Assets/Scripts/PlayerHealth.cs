using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        Debug.Log($"❤️ Игрок получил урон: {amount}. Осталось HP: {currentHealth}");
    }

    void Die()
    {
        Debug.Log("💀 Игрок погиб!");
        // Здесь можно добавить перезапуск, анимацию и т.д.
    }
}
