using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 1.2f;
    public float attackRate = 1f;
    public int damage = 10; // Урон уже в int, это хорошо!

    private float nextAttackTime;
    private Transform player;
    private PlayerHealth playerHealth; // Будем искать динамически

    // Функция, которая находит нового игрока, если старый был уничтожен
    void FindPlayerAndHealth()
    {
        // Проверяем, потеряна ли ссылка на игрока.
        // (player == null) или (игрок мертв/неактивен)
        if (player == null || (player.gameObject.activeInHierarchy == false && player.gameObject.tag == "Player"))
        {
            // Ищем новый объект игрока по тегу
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
                playerHealth = playerGO.GetComponent<PlayerHealth>();
            }
            else
            {
                // Игрока нет на сцене, очищаем ссылку
                player = null;
                playerHealth = null;
            }
        }
    }

    void Update()
    {
        // 1. Динамически ищем игрока и его здоровье
        FindPlayerAndHealth(); 

        if (player == null || playerHealth == null) return; // Если игрока нет, выходим

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / attackRate;
            DoDamage();
        }
    }

    void DoDamage()
    {
        // Двойная проверка, что здоровье есть
        if (playerHealth == null) return;

        Debug.Log("💢 Enemy атакует! Урон: " + damage);
        
        // Поскольку damage уже int, нам не нужно MathF.RoundToInt
        if (damage > 0)
        {
             playerHealth.TakeDamage(damage);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}