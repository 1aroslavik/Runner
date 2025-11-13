using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 1.4f;   // радиус удара
    public float attackRate = 1f;      // раз/сек
    public float damage = 15f;         // урон
    public LayerMask playerLayer;      // можно оставить пустым — подставим дефолт

    Transform player;
    float nextAttackTime;

    void Update()
    {
        // ЛЕНИВАЯ ПОИСК-ЛОГИКА: игрок спавнится позже — ждём и находим
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) player = go.transform;
            else return; // игрока ещё нет — выходим
        }

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / attackRate;
            Attack();
        }
    }

    void Attack()
    {
        // Если маска не задана в инспекторе — используем слой по имени "Player", иначе — все слои
        int mask = (playerLayer.value != 0) ? playerLayer.value : LayerMask.GetMask("Player");
        if (mask == 0) mask = ~0; // если слоя "Player" нет — бьём по всем слоям, дальше проверим тег

        var hit = Physics2D.OverlapCircle(transform.position, attackRange, mask);
        if (hit != null)
        {
            // Доп защита: проверяем тег
            if (!hit.CompareTag("Player")) return;

            var hp = hit.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                // Debug.Log("💢 Враг ударил игрока на " + damage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
