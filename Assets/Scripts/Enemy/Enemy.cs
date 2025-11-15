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

        ApplyType();            // применяем ScriptableObject
        SetupHealthBar();       // создаем полосу HP
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

        // ИСПОЛЬЗУЕМ рекурсивный поиск
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
    //  ОБНОВЛЕНИЕ CANVAS ПОЗИЦИИ
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
