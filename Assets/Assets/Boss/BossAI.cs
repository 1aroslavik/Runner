using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Настройки фаз")]
    public int currentPhase = 1;

    [Header("Атаки")]
    public GameObject fireballPrefab;
    public Transform shootPoint;

    [Header("Фаза 1 — одиночный шар")]
    public float phase1ShootRate = 2f;

    [Header("Фаза 2 — регенерация")]
    public float regenAmount = 3f;
    public float regenDuration = 5f;
    private float regenTimer;

    [Header("Фаза 3 — тройной залп")]
    public float phase3ShootRate = 1.2f;

    private float nextShotTime;

    private BossHealth health;
    private Transform player;

    void Start()
    {
        health = GetComponent<BossHealth>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentPhase = 1;
    }

    void Update()
    {
        if (player == null) return;

        switch (currentPhase)
        {
            case 1:
                Phase1_Attack();
                break;

            case 2:
                Phase2_Regenerate();
                break;

            case 3:
                Phase3_Attack();
                break;
        }
    }

    // =====================================================================
    //  ФАЗА 1 — одиночный шар
    // =====================================================================
    void Phase1_Attack()
    {
        if (Time.time < nextShotTime) return;

        nextShotTime = Time.time + phase1ShootRate;

        ShootFireball(0f);
    }

    // =====================================================================
    //  ФАЗА 2 — регенерация
    // =====================================================================
    void Phase2_Regenerate()
    {
        regenTimer += Time.deltaTime;

        if (regenTimer <= regenDuration)
        {
            health.Heal(regenAmount * Time.deltaTime);
        }
        else
        {
            regenTimer = 0f;
            currentPhase = 3; // Переход в фазу 3
        }
    }

    // =====================================================================
    //  ФАЗА 3 — тройной залп
    // =====================================================================
    void Phase3_Attack()
    {
        if (Time.time < nextShotTime) return;

        nextShotTime = Time.time + phase3ShootRate;

        ShootFireball(0f);
        ShootFireball(15f);
        ShootFireball(-15f);
    }

    // =====================================================================
    //  ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ
    // =====================================================================
    void ShootFireball(float angleOffset)
    {
        Vector3 dir = (player.position - shootPoint.position).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle += angleOffset;

        Quaternion rot = Quaternion.Euler(0, 0, angle);

        Instantiate(fireballPrefab, shootPoint.position, rot);
    }

    // =====================================================================
    //  Вызывается из BossHealth
    // =====================================================================
    public void UpdatePhase(int currentHP, int maxHP)
    {
        float hpPercent = (float)currentHP / maxHP;

        if (hpPercent > 0.66f)
            currentPhase = 1;
        else if (hpPercent > 0.33f)
            currentPhase = 2;
        else
            currentPhase = 3;
    }
}
