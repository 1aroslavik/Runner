using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Base Stats")]
    public int maxHP = 100;
    public int currentHP;
    public float damage = 10f;
    public float moveSpeed = 5f;
    public float attackSpeed = 1f;

    [Header("Advanced (optional)")]
    public float critChance = 0f;
    public float defence = 0f;

    void Start()
    {
        currentHP = maxHP;
    }

    // ------------------------------------
    //           APPLY UPGRADES
    // ------------------------------------
    public void AddHP(int amount)
    {
        maxHP += amount;
        currentHP += amount;
    }

    public void AddDamage(float amount)
    {
        damage += amount;
    }

    public void AddMoveSpeed(float amount)
    {
        moveSpeed += amount;
    }

    public void AddAttackSpeed(float amount)
    {
        attackSpeed -= amount;     // меньше — быстрее атака
        if (attackSpeed < 0.1f) attackSpeed = 0.1f;
    }

    public void AddCritChance(float amount)
    {
        critChance += amount;
    }

    public void AddDefence(float amount)
    {
        defence += amount;
    }

    // ------------------------------------
    //           DAMAGE SYSTEM
    // ------------------------------------
    public void TakeDamage(int dmg)
    {
        dmg -= Mathf.RoundToInt(defence);
        if (dmg < 1) dmg = 1;

        currentHP -= dmg;

        if (currentHP <= 0)
        {
            currentHP = 0;
            Die();
        }
    }

    void Die()
    {
        Debug.Log("☠ Игрок умер");
        // тут можно сделать рестарт карты
    }
}
