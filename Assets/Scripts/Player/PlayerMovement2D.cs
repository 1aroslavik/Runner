using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Параметры движения")]
    public float moveSpeed = 5f;          // обычная скорость
    public float sprintSpeed = 8f;        // скорость во время спринта
    public float jumpForce = 12f;         // сила прыжка
    public LayerMask groundLayer;         // слой земли

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;

    [Header("Проверка земли")]
    public Transform groundCheck;         // точка проверки земли
    public float groundCheckRadius = 0.2f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Если GroundCheck не создан — создаём автоматически
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -1f, 0); // чуть ниже ног
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

        // Прыжок
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
}
