using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Снаряды")]
    public GameObject fireballPrefab;
    public Transform shootPoint;

    [Header("Фаза 1 — одиночный выстрел")]
    public float phase1ShootRate = 2f;

    [Header("Фаза 2 — круговой залп")]
    public int phase2Projectiles = 12;
    public float phase2ShootRate = 3f;
    public bool isInvulnerable = false;

    [Header("Фаза 3 — регенерация")]
    public float regenPerSecond = 5f;
    public float regenDuration = 5f;

    [Header("Дистанция атаки")]
    public float attackRange = 10f;  // <<< ДОБАВИЛ ЭТО

    private float nextShotTime;
    private float regenTimer;

    private BossHealth health;
    private Transform player;

    public int currentPhase = 1;

    void Start()
    {
        health = GetComponent<BossHealth>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void Update()
    {
        // Игрок мог умереть или исчезнуть — ищем снова
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                player = p.transform;

            return; // без игрока ничего не делаем
        }

        switch (currentPhase)
        {
            case 1:
                Phase1_Attack();
                break;

            case 2:
                Phase2_Attack();
                break;

            case 3:
                Phase3_Regenerate();
                break;
        }
    }

    // =====================================================
    // ФАЗА 1 — одиночный выстрел в игрока
    // =====================================================
    void Phase1_Attack()
    {
        if (Time.time < nextShotTime) return;

        // ---- проверка дистанции ----
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        nextShotTime = Time.time + phase1ShootRate;
        ShootSingleAtPlayer();
    }

    void ShootSingleAtPlayer()
    {
        if (player == null) return;

        Vector3 dir = (player.position - shootPoint.position).normalized;

        GameObject fb = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);
        fb.GetComponent<Fireball>().SetDirection(dir);
    }


    // =====================================================
    // ФАЗА 2 — круговой залп
    // =====================================================
    void Phase2_Attack()
    {
        isInvulnerable = true;

        if (Time.time < nextShotTime) return;

        // ---- проверка дистанции ----
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > attackRange) return;

        nextShotTime = Time.time + phase2ShootRate;

        float angleStep = 360f / phase2Projectiles;

        for (int i = 0; i < phase2Projectiles; i++)
        {
            float angle = i * angleStep;

            Vector3 dir = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            );

            GameObject fb = Instantiate(fireballPrefab, shootPoint.position, Quaternion.identity);
            fb.GetComponent<Fireball>().SetDirection(dir);
        }
    }

    // =====================================================
    // ФАЗА 3 — регенерация
    // =====================================================
    void Phase3_Regenerate()
    {
        isInvulnerable = false;

        regenTimer += Time.deltaTime;
        health.Heal(regenPerSecond * Time.deltaTime);

        if (regenTimer >= regenDuration)
        {
            regenTimer = 0;
        }
    }

    // =====================================================
    // Обновление фазы
    // =====================================================
    public void UpdatePhase(int currentHP, int maxHP)
    {
        float hpPercent = (float)currentHP / maxHP;

        if (hpPercent > 0.7f)
            currentPhase = 1;
        else if (hpPercent > 0.4f)
            currentPhase = 2;
        else
            currentPhase = 3;
    }
}
