using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 500;
    public int currentHealth;

    private BossAI bossAI;

    void Start()
    {
        currentHealth = maxHealth;
        bossAI = GetComponent<BossAI>();
    }

    public void TakeDamage(int dmg)
    {
        currentHealth -= dmg;
        if (currentHealth < 0)
            currentHealth = 0;

        bossAI.UpdatePhase(currentHealth, maxHealth);

        if (currentHealth == 0)
            Die();
    }

    public void Heal(float amount)
    {
        currentHealth += Mathf.RoundToInt(amount);
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }

    void Die()
    {
        Debug.Log("BOSS DEAD!");
        Destroy(gameObject);
    }
}
