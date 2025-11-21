using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Параметры здоровья")]
    // Изменим на int, чтобы было проще, но float тоже можно
    public int maxHealth = 100; 
    private int currentHealth;

    [Header("UI")]
    public Image healthFill; // сюда перетащи HealthFill из Canvas

    // Ссылка на компонент управления игроком (например, PlayerMovement)
    // Перетащите сюда ваш скрипт управления в Инспекторе
    public MonoBehaviour playerController; 
    
    // Ссылка на компонент Rigidbody2D
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Автоматический поиск UI, если не перетащили
        if (healthFill == null)
        {
            var obj = GameObject.Find("HealthFill");
            if (obj != null)
                healthFill = obj.GetComponent<Image>();
        }
    }

    void Start()
    {
        // Инициализация при старте (первом запуске)
        Respawn(maxHealth); 
    }

    // Универсальная функция для получения урона
    public void TakeDamage(int amount)
    {
        // Используем (int) для приведения, если урон приходит как float
        currentHealth = Mathf.Max(0, currentHealth - amount); 

        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Универсальная функция для лечения
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
    }
    
    // Функция обновления шкалы здоровья
    void UpdateHealthUI()
    {
        if (healthFill != null)
        {
            // Здесь используем (float) для корректного деления
            healthFill.fillAmount = (float)currentHealth / maxHealth; 
        }
    }

    // --- ЛОГИКА СМЕРТИ ---
    void Die()
    {
        Debug.Log("💀 Игрок погиб! Уведомляю GameStateManager.");
        
        // 1. Отключаем управление игроком
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // 2. Отключаем физику и/или коллизии, чтобы игрок не мешал
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // <-- ИСПРАВЛЕНО: вместо rb.velocity
            rb.bodyType = RigidbodyType2D.Kinematic; // <-- ИСПРАВЛЕНО: вместо rb.isKinematic = true
        }
        
        // 3. Отключаем видимость объекта (или запускаем анимацию смерти)
        // Если вы хотите сделать эффект "исчезновения":
        gameObject.SetActive(false); 

        // 4. Сообщаем менеджеру игры, что игрок умер.
        // ЭТОТ ВЫЗОВ ПРАВИЛЬНЫЙ (HandlePlayerDeath с 1 аргументом)
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.HandlePlayerDeath(gameObject);
        }
    }

    // --- ЛОГИКА РЕСПАВНА (вызывается GameStateManager'ом) ---
    public void Respawn(int healthToRestore)
    {
        // 1. Возвращаем HP
        currentHealth = healthToRestore;
        UpdateHealthUI();

        // 2. Включаем видимость
        gameObject.SetActive(true); 

        // 3. Включаем управление и физику
        if (playerController != null)
        {
            playerController.enabled = true;
        }
        
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; // <-- ИСПРАВЛЕНО: вместо rb.isKinematic = false
        }
        
        // Примечание: Перемещение игрока на точку спавна
        // будет происходить в GameStateManager, но мы можем 
        // добавить сюда публичную функцию для удобства:
        // public void Teleport(Vector3 position) { transform.position = position; }
    }
}