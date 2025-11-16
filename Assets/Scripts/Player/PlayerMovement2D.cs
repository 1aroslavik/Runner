using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    // ... (Все твои [Header] остаются без изменений)
    [Header("Стрельба")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

    [Header("Параметры движения")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 12f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private Animator animator; // <--- Ссылка на твой Аниматор

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // <--- Находим Аниматор при старте

        // ... (Твой код для groundCheck)
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = gc.transform;
        }
    }

    void Update()
    {
        // Горизонтальное движение (A/D или стрелки)
        moveInput = Input.GetAxisRaw("Horizontal");

        // Разворот спрайта
        if (moveInput > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        // --- 1. ОТПРАВЛЯЕМ "Speed" В BLEND TREE ---
        float horizontalMove = Mathf.Abs(moveInput); // 0 (стоим) или 1 (движемся)
        bool isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (horizontalMove > 0) // Если движемся
        {
            animator.SetFloat("Speed", isSprinting ? 1f : 0.5f);
        }
        else // Если стоим
        {
            animator.SetFloat("Speed", 0f);
        }

        // --- 2. ОТПРАВЛЯЕМ "isGrounded" ---
        animator.SetBool("isGrounded", isGrounded);

        // --- 3. ПРЫЖОК (Триггер "Jump") ---
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // --- 4. АТАКА (Триггер "Attack") ---
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Attack");
            ShootArrow(); // <--- Теперь эта функция "видна"
        }
    }

    void FixedUpdate()
    {
        // Проверка касания земли
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Спринт (Shift)
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        // Применяем движение
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    // Визуализация зоны проверки земли
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // 👇👇👇 ВОТ ОН, ТЕПЕРЬ ВНУТРИ КЛАССА 👇👇👇
    void ShootArrow()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("⚠️ Префаб стрелы не назначен!");
            return;
        }

        // Позиция выстрела — чуть впереди игрока
        Vector3 spawnPos = transform.position + new Vector3(transform.localScale.x * 0.6f, 0f, 0f);

        // Создаём стрелу
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        // Получаем Rigidbody2D стрелы
        Rigidbody2D rbArrow = arrow.GetComponent<Rigidbody2D>();

        // Направление полета зависит от того, куда смотрит игрок
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        rbArrow.linearVelocity = new Vector2(direction * arrowSpeed, 0f);

        // Немного повернём стрелу по направлению движения (необязательно)
        arrow.transform.localScale = new Vector3(direction, 1f, 1f);

        // Уничтожаем стрелу через arrowLifetime секунд
        Destroy(arrow, arrowLifetime);
    }

} // <--- ВОТ ЭТА СКОБКА ТЕПЕРЬ ПОСЛЕДНЯЯ