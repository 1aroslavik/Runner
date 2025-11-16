using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Стрельба")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

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

        // ПРЫЖОК — теперь БЕЗ LayerMask, он всегда работает
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetTrigger("Jump");
        }

        // Атака
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Attack");
            ShootArrow();
        }
    }

    void FixedUpdate()
    {
        // ПРОВЕРКА ЗЕМЛИ — ТЕПЕРЬ ГАРАНТИРОВАННО РАБОТАЕТ
        // использует OverlapCircle без layerMask
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);

        isGrounded = hit != null;

        float currentSpeed =
            Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    // СТРЕЛЬБА
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
}
