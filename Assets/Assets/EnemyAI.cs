using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // === Настройки ИИ ===
    public float detectionRange = 6f;   
    public float attackRange = 1.2f;    
    public float attackCooldown = 1f;   

    // === Приватные ссылки ===
    private Transform player;
    private Enemy enemy;    
    private Rigidbody2D rb;
    private Animator _animator; // <-- Ссылка на Animator
    private float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // 1. УСТОЙЧИВАЯ ИНИЦИАЛИЗАЦИЯ ANIMATOR: ищем на самом объекте
        _animator = GetComponent<Animator>(); 
        
        if (_animator == null)
        {
             // Если не нашли на себе, ищем на дочерних объектах (более надежный способ)
             _animator = GetComponentInChildren<Animator>(); 
             if (_animator == null)
             {
                 Debug.LogError("❌ Animator не найден. Анимация работать не будет!");
             }
        }
    }
    
    // МЕТОД START ДЛЯ ПРИНУДИТЕЛЬНОГО ЗАПУСКА АНИМАЦИИ
    void Start()
    {
        if (_animator != null)
        {
            // ПРИНУДИТЕЛЬНЫЙ СТАРТ: Убедитесь, что "EnemySlimeWalk" (или "EnemySkeletonWalk")
            // ТОЧНО совпадает с именем вашего состояния ходьбы на карте!
            _animator.Play("EnemySlimeWalk"); 
            // Установка скорости > 0 гарантирует, что анимация Walk запустится
            _animator.SetFloat("Speed", 1f); 
            Debug.Log("✅ Аниматор принудительно запущен в состояние WALK.");
        }
    }
    
    void FindPlayer()
    {
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
        FindPlayer(); 
        if (player == null) return; 

        float distance = Vector2.Distance(transform.position, player.position);

        // 🛑 УДАЛЕНА ЛОГИКА IDLE: Враг всегда либо атакует, либо двигается
        
        // -------- 1. Игрок рядом → атакуем (ATTACK) --------
        if (distance <= attackRange)
        {
            AttackPlayer();
            // Враг останавливается для удара (ВАЖНО для синхронизации анимации)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }

        // -------- 2. Игрок вне зоны атаки → идём/бежим (WALK/RUN) --------
        else 
        {
            MoveTowardPlayer();
        }
    }

    void MoveTowardPlayer()
    {
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null) return; 
        }
        
        float direction = player.position.x > transform.position.x ? 1 : -1;

        // ВАША ОРИГИНАЛЬНАЯ ЛОГИКА ДВИЖЕНИЯ
        float currentMoveSpeed = direction * enemy.moveSpeed;
        rb.linearVelocity = new Vector2(currentMoveSpeed, rb.linearVelocity.y);

        // 2. АНИМАЦИЯ WALK/RUN: Передаем абсолютное значение скорости
        float actualSpeed = Mathf.Abs(currentMoveSpeed);
        if (_animator != null)
            _animator.SetFloat("Speed", actualSpeed); 

        // поворот спрайта
        transform.localScale = new Vector3(direction, 1, 1);
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;
        
        // 3. АНИМАЦИЯ ATTACK: Запуск триггера
        if (_animator != null)
            _animator.SetTrigger("Attack"); // Запуск клипа атаки
            
        // ... (Остальная логика нанесения урона) ...
        if (enemy == null)
        {
            enemy = GetComponent<Enemy>();
            if (enemy == null) return;
        }

        PlayerHealth hp = player.GetComponent<PlayerHealth>();

        if (hp != null)
        {
            int damageInt = Mathf.RoundToInt(enemy.damage);
            
            if (damageInt > 0)
            {
                hp.TakeDamage(damageInt);
            }
        }
    }
}