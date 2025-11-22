using UnityEngine;

public class PlayerMovement2D : MonoBehaviour
{
    [Header("Стрельба (Q)")]
    public GameObject arrowPrefab;
    public float arrowSpeed = 10f;
    public float arrowLifetime = 3f;

    [Header("Ближний бой (E)")]
    public Transform attackPoint;
    public float attackRadius = 0.6f;
    public LayerMask enemyLayer;

    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private Animator animator;

    [Header("Проверка земли")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;

    private PlayerStats stats;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        stats = GetComponent<PlayerStats>();

        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0, -0.8f, 0);
            groundCheck = gc.transform;
        }

        if (attackPoint == null)
        {
            GameObject ap = new GameObject("AttackPoint");
            ap.transform.SetParent(transform);
            ap.transform.localPosition = new Vector3(0.6f, 0, 0);
            attackPoint = ap.transform;
        }
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput > 0)
            transform.localScale = Vector3.one;
        else if (moveInput < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("isGrounded", isGrounded);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
            animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Attack");
            ShootArrow();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetTrigger("Attack_2");
            MeleeAttack();
        }
    }

    void FixedUpdate()
    {
        Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
        isGrounded = hit != null;

        float currentSpeed = Input.GetKey(KeyCode.LeftShift)
            ? stats.sprintSpeed
            : stats.moveSpeed;

        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);
    }

    void ShootArrow()
    {
        if (arrowPrefab == null) return;

        float dir = transform.localScale.x > 0 ? 1f : -1f;

        Vector3 spawnPos = transform.position + new Vector3(0.6f * dir, 0, 0);
        GameObject arrow = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);

        Rigidbody2D rbArrow = arrow.GetComponent<Rigidbody2D>();
        rbArrow.linearVelocity = new Vector2(dir * arrowSpeed, 0);

        arrow.transform.localScale = new Vector3(dir, 1, 1);

        // ✅ Передаём урон из PlayerStats
        Arrow a = arrow.GetComponent<Arrow>();
        a.damage = stats.arrowDamage;

        Destroy(arrow, arrowLifetime);
    }


    void MeleeAttack()
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            Enemy e = enemy.GetComponent<Enemy>();
            if (e != null)
            {
                e.TakeDamage(stats.meleeDamage);
                continue;
            }

            BossHealth boss = enemy.GetComponent<BossHealth>();
            if (boss != null)
            {
                boss.TakeDamage((int)stats.meleeDamage);
            }
        }
    }
}
