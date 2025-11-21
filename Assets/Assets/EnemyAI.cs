using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float detectionRange = 6f;   
    public float attackRange = 1.2f;    
    public float attackCooldown = 1f;   

    private Transform player;
    private Enemy enemy;    // Мы не будем инициализировать это в Awake
    private Rigidbody2D rb;
    private float lastAttackTime;

    void Awake()
    {
        // Только то, что нужно для самого врага сразу:
        rb = GetComponent<Rigidbody2D>();
        
        // ВНИМАНИЕ: Ссылку на "enemy" (статы) убрали из Awake!
        // Она будет искаться в момент атаки, когда спавнер ее точно настроит.
    }
    
    // Новая функция для поиска игрока.
    void FindPlayer()
    {
        // Если ссылка на игрока потеряна (уничтожена), ищем ее заново.
        if (player == null || (player.gameObject.activeInHierarchy == false && player.gameObject.tag == "Player"))
        {
            GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
        }
    }

    void Update()
    {
        FindPlayer(); // <--- Сначала находим игрока
        
        if (player == null) return; // Если игрока все еще нет, выходим

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
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void MoveTowardPlayer()
    {
        // ГАРАНТИЯ СТАТОВ: Получаем статы, если их еще нет
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null) return; // Если статов нет, не двигаемся
        }
        
        float direction = player.position.x > transform.position.x ? 1 : -1;

        rb.linearVelocity = new Vector2(direction * enemy.moveSpeed, rb.linearVelocity.y);

        // поворот спрайта
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        
        // ГАРАНТИЯ СТАТОВ: Получаем статы, если их еще нет (Ленивая инициализация)
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null)
            {
                 Debug.LogWarning("❌ Enemy stats (Enemy component) not found!");
                 return;
            }
        }

        // ищем компонент здоровья у игрока
        PlayerHealth hp = player.GetComponent<PlayerHealth>();

        if (hp != null)
        {
            // Преобразуем урон (float) в целое число (int), округляя его.
            int damageInt = Mathf.RoundToInt(enemy.damage);
            
            // Если урон > 0, наносим его.
            if (damageInt > 0)
            {
                hp.TakeDamage(damageInt);
            }
            else
            {
                // Это сработает, если damage в инспекторе = 0.0 или очень мало.
                Debug.LogWarning($"Урон врага равен 0! Текущий damage: {enemy.damage}");
            }
        }
    }
}