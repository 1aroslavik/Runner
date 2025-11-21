using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 7f;
    public int damage = 10;
    public float lifeTime = 4f;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += transform.right * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            var hp = col.GetComponent<PlayerHealth>();
            if (hp != null)
                hp.TakeDamage(damage);

            Destroy(gameObject);
        }
    }
}
