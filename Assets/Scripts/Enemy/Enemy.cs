using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Тип врага (ScriptableObject)")]
    public EnemyType type;

    [Header("Параметры врага")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float moveSpeed = 2f;
    public float damage = 10f;

    private SpriteRenderer sr;

    [Header("UI здоровья")]
    public GameObject healthBarPrefab;
    private Image healthFill;
    private Canvas hpCanvas;


    // ================================
    //  START
    // ================================
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        ApplyType();            // базовые данные из ScriptableObject
        SetupHealthBar();       // полоса HP
    }


    // ================================
    //  ПРИМЕНИТЬ EnemyType
    // ================================
    public void ApplyType()
    {
        if (type == null)
        {
            Debug.LogError("❌ EnemyType не назначен!");
            return;
        }

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        sr.sprite = type.sprite;
        maxHealth = type.maxHealth;
        currentHealth = maxHealth;
        moveSpeed = type.moveSpeed;
        damage = type.damage;
    }

    // Перегрузка с параметром — для спавнера
    public void ApplyType(EnemyType newType)
    {
        type = newType;   // назначаем новый тип
        ApplyType();      // используем уже существующую логику
    }


    // ================================
    //  УСИЛЕНИЕ ПО ТУННЕЛЮ
    // ================================
    public void ApplyDifficulty(float progress)
    {
        // progress = 0..1

        // Увеличиваем ХП
        maxHealth *= Mathf.Lerp(1f, 3f, progress);
        currentHealth = maxHealth;

        // Увеличиваем урон
        damage *= Mathf.Lerp(1f, 2f, progress);

        // Скорость растёт слегка
        moveSpeed *= Mathf.Lerp(1f, 1.25f, progress);

        // Обновляем полоску HP
        if (healthFill != null)
            healthFill.fillAmount = 1f;
    }


    // ================================
    //  СОЗДАТЬ ПОЛОСУ ЗДОРОВЬЯ
    // ================================
    private void SetupHealthBar()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogError("❌ HealthBarPrefab не назначен!");
            return;
        }

        GameObject bar = Instantiate(
            healthBarPrefab,
            transform.position + Vector3.up * 1.2f,
            Quaternion.identity
        );

        hpCanvas = bar.GetComponentInChildren<Canvas>();

        healthFill = FindHealthFill(bar.transform);

        if (healthFill == null)
        {
            Debug.LogError("❌ Не найден объект HealthFill в префабе HP!");
            return;
        }

        bar.transform.SetParent(transform);
    }


    // ================================
    //  РЕКУРСИВНЫЙ ПОИСК HealthFill
    // ================================
    private Image FindHealthFill(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name == "HealthFill")
                return child.GetComponent<Image>();

            Image result = FindHealthFill(child);
            if (result != null)
                return result;
        }

        return null;
    }


    // ================================
    //  ОБНОВЛЕНИЕ ПОЗИЦИИ HP BAR
    // ================================
    void Update()
    {
        if (hpCanvas != null)
            hpCanvas.transform.position = transform.position + Vector3.up * 1.2f;
    }


    // ================================
    //  ПОЛУЧЕНИЕ УРОНА
    // ================================
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
            Destroy(gameObject);
    }
}
