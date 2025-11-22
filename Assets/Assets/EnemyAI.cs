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
        
        // --- ИСПРАВЛЕННАЯ ЛОГИКА AWAKE ---
        // Используем GetComponentInChildren(true), чтобы найти на самом объекте ИЛИ дочернем, 
        // даже если он неактивен.
        _animator = GetComponentInChildren<Animator>(true); 
        
        // if (_animator == null)
        // {
        //      Debug.LogError("❌ Animator не найден на враге или его потомках. Анимация работать не будет!");
        // }
    }
    
    // МЕТОД START ДЛЯ ПРИНУДИТЕЛЬНОГО ЗАПУСКА АНИМАЦИИ
    void Start()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>(true);
        }

        if (_animator != null)
        {
            // ПРИНУДИТЕЛЬНЫЙ СТАРТ:
            // Убедитесь, что "EnemySlimeWalk" ТОЧНО совпадает с именем вашего состояния ходьбы!
            _animator.Play("EnemySlimeWalk"); 
            _animator.SetFloat("Speed", 1f); 
            // Debug.Log("✅ Аниматор принудительно запущен в состояние WALK."); // Убираем лишний Debug.Log из Start
        } else {
            Debug.LogError("❌ Animator не найден на враге или его потомках. Анимация работать не будет!");
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

        // -------- 1. Игрок рядом → атакуем (ATTACK) --------
        if (distance <= attackRange)
        {
            AttackPlayer();
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
        // Debug.Log($"[EnemyAI]: Текущая скорость врага: {actualSpeed}"); // Убираем лишний Debug.Log из Move

        if (_animator != null && _animator.runtimeAnimatorController == null)
            Debug.LogError("❌ AnimatorController не назначен в Animator врага. Анимация работать не будет!");

        if (_animator != null && _animator.runtimeAnimatorController != null)
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