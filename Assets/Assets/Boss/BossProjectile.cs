using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 6f;
    public int damage = 15;
    public float life = 4f;

    void Start()
    {
        Destroy(gameObject, life);
    }

    void Update()
    {
        transform.position += transform.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
