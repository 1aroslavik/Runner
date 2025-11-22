using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 2000;
    public int currentHealth;

    private BossAI bossAI;

    void Start()
    {
        currentHealth = maxHealth;
        bossAI = GetComponent<BossAI>();
    }

    public void TakeDamage(int dmg)
    {
        // --- Фаза 2: босс защищён ---
        if (bossAI.isInvulnerable)
        {
            int reduced = Mathf.RoundToInt(dmg * 0.2f); // 20% урона
            currentHealth -= reduced;

            Debug.Log($"🛡 Босс защищён! Получил только {reduced} урона вместо {dmg}. HP: {currentHealth}");

            bossAI.UpdatePhase(currentHealth, maxHealth);

            if (currentHealth <= 0)
                Die();

            return;
        }

        // --- Обычный урон ---
        currentHealth -= dmg;

        Debug.Log($"🔥 Босс получил урон: {dmg}. Осталось HP: {currentHealth}");

        bossAI.UpdatePhase(currentHealth, maxHealth);

        if (currentHealth <= 0)
            Die();
    }


    public void Heal(float amount)
    {
        currentHealth += Mathf.RoundToInt(amount);
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    void Die()
    {
        Debug.Log("💀 БОСС УМЕР");
        Destroy(gameObject);
    }
}
