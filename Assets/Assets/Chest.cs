using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpened = false;

    // Ссылка на менеджер
    private UpgradeManager upgradeManager; 

    void Start()
    {
        _animator = GetComponent<Animator>();

        // Находим менеджер (используем рекомендуемый метод)
        upgradeManager = FindFirstObjectByType<UpgradeManager>();
        
        if (upgradeManager == null)
        {
            Debug.LogError("UpgradeManager не найден.");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 1. Проверки
        if (_isOpened || !col.CompareTag("Player")) return;

        _isOpened = true; // Блокируем повторное открытие
        
        // 2. ЗАПУСК АНИМАЦИИ
        if (_animator != null)
        {
            // Здесь анимация запускается
            _animator.SetTrigger("Open"); 
        }

        // 3. ЛОГИКА ЛУТА (Как было раньше, но с исправлением ошибки)
        // Находим PlayerStats и передаем его в TriggerUpgrade (для устранения CS7036)
        PlayerStats playerStats = col.gameObject.GetComponent<PlayerStats>();
        
        if (playerStats != null)
        {
            // Вызываем ваш Canvas с лутом немедленно
            upgradeManager.TriggerUpgrade(playerStats); 
        }
        else
        {
            Debug.LogError("PlayerStats не найден на игроке! Не могу вызвать TriggerUpgrade.");
        }

        // 4. УДАЛЕНИЕ СУНДУКА
        // Сундук удаляется немедленно, поэтому анимация не будет доиграна до конца!
        Destroy(gameObject);
    }
}