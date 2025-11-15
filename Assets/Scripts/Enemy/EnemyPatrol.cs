using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 2f;

    private Transform target;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();

        target = pointB; // начинаем движение к точке B
    }

    private void Update()
    {
        if (target == null) return;

        // Движение к цели
        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        // Проверка достижения точки
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
        {
            target = (target == pointA) ? pointB : pointA; // меняем направление
        }

        // Флип спрайта (чтобы смотрел в сторону движения)
        if (sr != null)
        {
            sr.flipX = target.position.x < transform.position.x;
        }
    }
}
