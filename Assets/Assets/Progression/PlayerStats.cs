using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Основные параметры")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;

    public float meleeDamage = 20f;
    public float arrowDamage = 30f;

    public float attackSpeed = 1f;   // чем больше — тем быстрее
    public float defence = 0f;       // броня уменьшает урон

    [Header("UI")]
    public PlayerStatsUI statsUI;


    private void Start()
    {
        currentHealth = maxHealth;

        if (statsUI != null)
            statsUI.UpdateUI(this);
    }


    // ===============================
    //   Методы прокачки
    // ===============================

    public void AddMaxHP(float value)
    {
        maxHealth += value;
        currentHealth = maxHealth;
        statsUI.UpdateUI(this);
    }

    public void AddMeleeDamage(float value)
    {
        meleeDamage += value;
        statsUI.UpdateUI(this);
    }

    public void AddArrowDamage(float value)
    {
        arrowDamage += value;
        statsUI.UpdateUI(this);
    }

    public void AddMoveSpeed(float value)
    {
        moveSpeed += value;
        statsUI.UpdateUI(this);
    }

    public void AddAttackSpeed(float value)
    {
        attackSpeed += value;
        statsUI.UpdateUI(this);
    }

    public void AddDefence(float value)
    {
        defence += value;
        statsUI.UpdateUI(this);
    }
}
