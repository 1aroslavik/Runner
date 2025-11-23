using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    // === Настройки ИИ ===
    public float detectionRange = 6f;
    public float attackRange = 1.2f;
    public float attackCooldown = 1f;

    // === Прыжки ===
    [Header("Jump Settings")]
    public float jumpForce = 20f;
    public float jumpCooldown = 1.5f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private bool isGrounded = false;
    private float lastJumpTime = 0;

    // === Приватные ссылки ===
    private Transform player;
    private Enemy enemy;
    private Rigidbody2D rb;
    private Animator _animator;
    private float lastAttackTime;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        _animator = GetComponentInChildren<Animator>(true);
    }

    void Start()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>(true);
        }

        if (_animator != null)
        {
            _animator.Play("EnemySlimeWalk");
            _animator.SetFloat("Speed", 1f);
        }
        else
        {
            Debug.LogError("❌ Animator не найден на враге или его потомках.");
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
        CheckGround();   // ← добавлено

        FindPlayer();
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            AttackPlayer();
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
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

        // === Попытка прыгнуть ===
        TryJump(direction);

        float currentMoveSpeed = direction * enemy.moveSpeed;
        rb.linearVelocity = new Vector2(currentMoveSpeed, rb.linearVelocity.y);

        float actualSpeed = Mathf.Abs(currentMoveSpeed);

        if (_animator != null && _animator.runtimeAnimatorController == null)
            Debug.LogError("❌ AnimatorController не назначен в Animator врага!");

        if (_animator != null && _animator.runtimeAnimatorController != null)
            _animator.SetFloat("Speed", actualSpeed);

        transform.localScale = new Vector3(direction, 1, 1);
    }

    void AttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        if (_animator != null)
        {
            _animator.SetFloat("Speed", 0f);
            _animator.SetTrigger("Attack");
        }

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

    // ==========================
    //     НОВЫЕ МЕТОДЫ
    // ==========================

    void CheckGround()
    {
        if (groundCheck == null) return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        if (_animator != null)
            _animator.SetBool("Grounded", isGrounded);
    }

    void TryJump(float direction)
    {
        if (!isGrounded) return;
        if (Time.time - lastJumpTime < jumpCooldown) return;

        // 1. Прыгаем если игрок выше
        if (player.position.y > transform.position.y + 0.6f)
        {
            DoJump();
            return;
        }

        // 2. Прыжок через препятствие
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position + Vector3.up * 0.2f,
            Vector2.right * direction,
            0.5f,
            groundLayer
        );

        if (hit.collider != null)
        {
            DoJump();
        }
    }

    void DoJump()
    {
        lastJumpTime = Time.time;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        if (_animator != null)
            _animator.SetTrigger("Jump");
    }
}
