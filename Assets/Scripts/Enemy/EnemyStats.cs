using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    [Header("Базовые значения")]
    public float baseHP = 20f;
    public float baseDamage = 5f;
    public float baseSpeed = 3f;

    [Header("Текущие значения")]
    public float hp;
    public float damage;
    public float speed;

    /// difficulty = 0..1 (0 = начало карты, 1 = конец)
    public void ApplyDifficulty(float difficulty)
    {
        // HP растёт сильно
        hp = baseHP * Mathf.Lerp(1f, 3f, difficulty);

        // урон растёт умеренно
        damage = baseDamage * Mathf.Lerp(1f, 2f, difficulty);

        // скорость почти не растёт
        speed = baseSpeed * Mathf.Lerp(1f, 1.3f, difficulty);
    }
}
