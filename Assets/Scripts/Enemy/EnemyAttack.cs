using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 1.2f;
    public float attackRate = 1f;
    public int damage = 10;

    private float nextAttackTime;
    private Transform player;
    private PlayerHealth playerHealth;

    // === ИСПРАВЛЕННЫЙ МЕТОД ===
    void FindPlayerAndHealth()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");

        if (playerGO == null)
        {
            player = null;
            playerHealth = null;
            return;
        }

        player = playerGO.transform;
        playerHealth = playerGO.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // всегда обновляем ссылку, вдруг игрок умер/заспавнился
        if (player == null || playerHealth == null)
            FindPlayerAndHealth();

        if (player == null || playerHealth == null)
            return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + 1f / attackRate;
            DoDamage();
        }
    }

    void DoDamage()
    {
        if (playerHealth == null) return;

        Debug.Log("💢 Enemy атакует игрока! dmg: " + damage);

        playerHealth.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
