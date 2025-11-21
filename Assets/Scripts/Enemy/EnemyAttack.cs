using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public float attackRange = 1.2f;
    public float attackRate = 1f;
    public int damage = 10;

    private float nextAttackTime;
    private Transform player;
    private PlayerHealth playerHealth;

    void Update()
    {
        if (player == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null)
            {
                player = go.transform;
                playerHealth = go.GetComponent<PlayerHealth>();
            }
            return;
        }

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

        Debug.Log("💢 Enemy атакует! Урон: " + damage);
        playerHealth.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
