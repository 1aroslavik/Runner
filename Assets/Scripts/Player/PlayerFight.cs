using UnityEngine;

public class PlayerFight : MonoBehaviour
{
    // --- Общие компоненты ---
    private Animator animator;

    // --- БЛИЖНИЙ БОЙ (Атака E) ---
    [Header("Ближний бой (Атака E)")]
    public Transform attackPoint; // Точка, откуда бьем
    public float attackRadius = 0.6f;
    public float meleeDamage = 25f; // Урон мечом (больше)
    public LayerMask enemyLayer;    // Какие слои считать врагами


    void Start()
    {
        // Получаем компонент Аниматора
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Атака E (Ближний бой)
        if (Input.GetKeyDown(KeyCode.E))
        {
            // "AttackE" - это имя твоего НОВОГО триггера в Аниматоре
            animator.SetTrigger("AttackE");
            MeleeAttack();
        }
    }

    // --- Логика Атаки E ---
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

    // --- Вспомогательное ---
    // Рисует красный круг в редакторе, чтобы ты видела зону атаки E
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}