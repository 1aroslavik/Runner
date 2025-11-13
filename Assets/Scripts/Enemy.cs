using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Параметры здоровья")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI здоровья")]
    public GameObject healthBarPrefab;  // Префаб полоски HP
    private Image healthFill;           // Внутренний элемент (заливка)
    private Canvas hpCanvas;            // Канвас, прикреплённый над врагом

    void Start()
    {
        currentHealth = maxHealth;

        if (healthBarPrefab == null)
        {
            Debug.LogError("❌ Префаб полоски здоровья не назначен в инспекторе у врага!");
            return;
        }

        // Создаём UI полоску
        GameObject bar = Instantiate(healthBarPrefab, transform.position + Vector3.up * 1.2f, Quaternion.identity);
        hpCanvas = bar.GetComponentInChildren<Canvas>();

        // 🔹 Универсальный поиск HealthFill (где бы он ни находился)
        healthFill = FindHealthFill(bar.transform);
        if (healthFill == null)
        {
            Debug.LogError("❌ Не найден элемент HealthFill в EnemyHealthCanvas!");
        }
        else
        {
            Debug.Log("✅ HealthFill найден успешно: " + healthFill.name);
        }

        bar.transform.SetParent(transform);
    }

    void Update()
    {
        if (hpCanvas != null)
        {
            hpCanvas.transform.position = transform.position + Vector3.up * 1.2f;
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0, currentHealth - amount);

        if (healthFill != null)
            healthFill.fillAmount = currentHealth / maxHealth;

        if (currentHealth <= 0)
            Destroy(gameObject);
    }

    // 🔍 Универсальный рекурсивный поиск HealthFill
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
}
