using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Здоровье")]
    public float maxHealth = 100;
    public float currentHealth = 100;

    [Header("Урон")]
    public float meleeDamage = 25f;
    public float arrowDamage = 40f;

    [Header("Характеристики")]
    public float moveSpeed = 5;
    public float sprintSpeed = 8;
    public float jumpForce = 30;
    public float defence = 0;

    // ========= Методы улучшений =========
    public void AddHP(int amount)
    {
        maxHealth += amount;
        currentHealth += amount;
    }

    public void AddMeleeDamage(float value)
    {
        meleeDamage += value;
    }

    public void AddArrowDamage(float value)
    {
        arrowDamage += value;
    }

    public void AddMoveSpeed(float value)
    {
        moveSpeed += value;
    }

    public void AddSprintSpeed(float value)
    {
        sprintSpeed += value;
    }

    public void AddJumpForce(float value)
    {
        jumpForce += value;
    }

    public void AddDefence(float value)
    {
        defence += value;
    }
}
