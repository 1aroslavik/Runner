using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 100;
    [HideInInspector] public int currentHealth;

    // Ссылку на PlayerController убираем, чтобы не было проблем с перетаскиванием!
    
    // Ссылка на компонент Rigidbody2D
    private Rigidbody2D rb;

    private Image healthFill;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Получаем Rigidbody
        
        // ИЩЕМ HealthFill АВТОМАТИЧЕСКИ
        var hud = GameObject.Find("HealthFill");
        if (hud != null)
            healthFill = hud.GetComponent<Image>();
        else
            Debug.LogError("❌ PlayerHealth: HealthFill не найден в сцене!");
        
        // Инициализируем здоровье
        Respawn(maxHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;
        
        UpdateUI();

        // 🛑 ГЛАВНАЯ ПРОВЕРКА: Если здоровье равно нулю или меньше, вызываем смерть!
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateUI()
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)currentHealth / maxHealth;
    }

    // --- ЛОГИКА СМЕРТИ ---
    void Die()
    {
        Debug.Log("💀 Игрок погиб! ЗАПУСКАЮ РЕСПАВН.");
        
        // Отключаем физику и скрываем игрока
        if (rb != null) 
        { 
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }
        gameObject.SetActive(false); 

        // Сообщаем менеджеру игры, что игрок умер (он его уничтожит и создаст новый уровень).
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.HandlePlayerDeath(gameObject); 
        }
    }

    // --- ЛОГИКА РЕСПАВНА (для GameStateManager) ---
    public void Respawn(int healthToRestore)
    {
        currentHealth = healthToRestore;
        UpdateUI();
        
        // Включаем видимость и физику
        gameObject.SetActive(true); 
        if (rb != null) { rb.bodyType = RigidbodyType2D.Dynamic; }
    }
}