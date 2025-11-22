using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 15;
    public float lifeTime = 4f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    // Устанавливаем направление из BossAI
    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // Попадание в игрока
        if (col.CompareTag("Player"))
        {
            PlayerHealth hp = col.GetComponent<PlayerHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damage);
                Debug.Log("🔥 Босс нанёс урон игроку: " + damage);
            }

            Destroy(gameObject);
        }

        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        if (col.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }

    }
}
