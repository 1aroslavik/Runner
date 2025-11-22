using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 40f;

    void OnTriggerEnter2D(Collider2D other)
    {
        // ------------------------------
        // Урон обычным врагам
        // ------------------------------
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy != null)
        {
            Debug.Log("🎯 Попадание по врагу: " + other.name);
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // ------------------------------
        // Урон боссу
        // ------------------------------
        BossHealth boss = other.GetComponent<BossHealth>();
        if (boss != null)
        {
            Debug.Log("🔥 Попадание по БОССУ!");
            boss.TakeDamage((int)damage);
            Destroy(gameObject);
            return;
        }
    }
}
