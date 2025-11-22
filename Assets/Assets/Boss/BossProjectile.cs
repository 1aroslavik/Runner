using UnityEngine;

public class BossProjectile : MonoBehaviour
{
    public float speed = 6f;

    private Vector3 direction;

    public void SetDirection(Vector3 dir)
    {
        direction = dir.normalized;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            col.GetComponent<PlayerHealth>()?.TakeDamage(15);
            Destroy(gameObject);
        }
    }
}
