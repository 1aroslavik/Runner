using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Стрельба (Атака Q)")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

    // --- НОВОЕ: Переменные для ближнего боя ---
    [Header("Ближний бой (Атака E)")]
    public Transform attackPoint; // Точка, откуда бьем
    public float attackRadius = 0.6f;
    public float meleeDamage = 25f; // Урон 
    public LayerMask enemyLayer;    // Какие слои считать врагами
    // --- Конец нового ---

    [Header("Параметры движения")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 12f;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private Animator animator;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // Создаём точку groundCheck, если её нет
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.8f, 0);
            groundCheck = gc.transform;
        }

        // --- НОВОЕ: Создаем attackPoint, если его нет ---
        if (attackPoint == null)
        {
            GameObject ap = new GameObject("AttackPoint");
            ap.transform.SetParent(transform);
            ap.transform.localPosition = new Vector3(0.6f, 0, 0); // Позиция по умолчанию
            attackPoint = ap.transform;
        }
        // --- Конец нового ---
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        // Разворот
        if (moveInput > 0)
            transform.localScale = Vector3.one;
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // Анимации
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("isGrounded", isGrounded);

        // ПРЫЖОК
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // Атака Q (Стрельба)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Attack"); // Триггер для лука
            ShootArrow();
        }

        // --- НОВОЕ: Атака E (Ближний бой) ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Attack_2"); // НОВЫЙ триггер для меча
            MeleeAttack();
        }
        // --- Конец нового ---
    }

    void FixedUpdate()
    {
        // ПРОВЕРКА ЗЕМЛИ
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        isGrounded = hit != null;

        float currentSpeed =
            Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        // Гизмо для земли
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // --- НОВОЕ: Гизмо для зоны атаки ---
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
        // --- Конец нового ---
    }

    // СТРЕЛЬБА (Атака Q)
    void ShootArrow()
    {
        if (arrowPrefab == null) return;

        Vector3 spawnPos = transform.position + new Vector3(transform.localScale.x * 0.6f, 0, 0);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rbArrow = arrow.GetComponent<Rigidbody2D>();
        float dir = transform.localScale.x > 0 ? 1f : -1f;

        rbArrow.linearVelocity = new Vector2(dir * arrowSpeed, 0);
        arrow.transform.localScale = new Vector3(dir, 1, 1);

        Destroy(arrow, arrowLifetime);
    }

    // --- НОВОЕ: Функция Ближнего Боя (Атака E) ---
    void MeleeAttack()
    {
        if (attackPoint == null) return;

        // 1. Находим всех врагов в круге атаки
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        // 2. Наносим урон каждому
        foreach (Collider2D enemy in hitEnemies)
        {
            Debug.Log("Удар по: " + enemy.name + ", Урон: " + meleeDamage);
            
            // Тут будет код нанесения урона врагу
            // enemy.GetComponent<EnemyHealth>().TakeDamage(meleeDamage);
        }
    }
    // --- Конец нового ---
}