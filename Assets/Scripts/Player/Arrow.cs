using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float damage = 40f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("🎯 Попадание по врагу: " + other.name);

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log("💥 Урон нанесён: " + damage);
            }

            Destroy(gameObject);
        }
    }
}
