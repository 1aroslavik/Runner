using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 6f;   // расстояние, с которого враг видит игрока
    public float attackRange = 1.2f;    // расстояние атаки
    public float attackCooldown = 1f;   // время между ударами

    private Transform player;
    private Enemy enemy;                // доступ к статам врага
    private Rigidbody2D rb;
    private float lastAttackTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemy = GetComponent<Enemy>();
        rb = GetComponent<Rigidbody2D>();

        if (player == null)
            Debug.LogError("❌ Player not found! Убедись, что у игрока стоит TAG = Player");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // -------- 1. Игрок далеко → стоим --------
        if (distance > detectionRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // -------- 2. Игрок в зоне видимости → идём к игроку --------
        if (distance > attackRange)
        {
            MoveTowardPlayer();
        }

        // -------- 3. Игрок рядом → атакуем --------
        else
        {
            AttackPlayer();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // не дрожим
        }
    }

    void MoveTowardPlayer()
    {
        float direction = player.position.x > transform.position.x ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * enemy.moveSpeed, rb.linearVelocity.y);

        // поворот спрайта
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        // ищем компонент здоровья у игрока
        PlayerHealth hp = player.GetComponent<PlayerHealth>();

        if (hp != null)
        {
            // ИСПРАВЛЕНИЕ ОШИБКИ CS1503:
            // Преобразуем урон (float) в целое число (int), округляя его.
            int damageInt = Mathf.RoundToInt(enemy.damage);
            
            hp.TakeDamage(damageInt);
        }
    }
}
