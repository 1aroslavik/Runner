using UnityEngine; 
using WFC; // Добавляем, чтобы видеть WFCTilemapGenerator

public class GameStateManager : MonoBehaviour
{
    // 1. Singleton Instance 
    public static GameStateManager Instance { get; private set; }

    // ==========================================================
    //                        СОСТОЯНИЕ ИГРЫ
    // ==========================================================
    
    private int deathCount = 0; 

    public int DeathCount => deathCount; 

    // ==========================================================
    //                      ССЫЛКИ МЕНЕДЖЕРОВ
    // ==========================================================
    
    [Header("Ссылки Менеджеров")]
    public PlayerSpawn playerSpawner; 
    public WFCTilemapGenerator levelGenerator; // Ссылка на ваш генератор WFC
    public DialogueManager dialogueSystem; 

    void Awake()
    {
        // Логика Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        
        // Объект не уничтожается при перезагрузке/рестарте, чтобы сохранить deathCount
        DontDestroyOnLoad(this.gameObject); 
        
        Debug.Log("Game Manager инициализирован. Текущий счетчик смертей (запусков): " + deathCount);
    }
    
    // ==========================================================
    //                        ОБРАБОТКА СМЕРТИ
    // ==========================================================
    
    public void HandlePlayerDeath(GameObject playerObject)
    {
        Debug.Log($"Игрок умер. Текущий счетчик: {deathCount}");
        
        // 1. Удаляем старый объект игрока
        if (playerObject != null)
        {
            Destroy(playerObject);
        }
        
        // 2. Увеличиваем счетчик
        deathCount++;
        
        Debug.Log($"Счетчик после смерти (для следующего цикла): {deathCount}");
        
        // Запускаем процесс перезапуска через 2 секунды ожидания
        Invoke("RestartGame", 2f); 
    }

    void RestartGame()
    {
        // 1. Генерируем новый уровень (запускает асинхронный процесс в WFCTilemapGenerator)
        if (levelGenerator != null)
        {
            levelGenerator.GenerateNewLevel(); 
        }
        else
        {
            Debug.LogError("❌ GameStateManager: LevelGenerator не назначен. Респавн невозможен!");
        }

        // SpawnPlayer() и StartDialogueOnRespawn() будут вызваны после завершения генерации.
    }

    // ==========================================================
    //                    ФИНАЛИЗАЦИЯ (ВЫЗЫВАЕТСЯ WFC)
    // ==========================================================
    /// <summary>
    /// Вызывается WFCTilemapGenerator после того, как корутина генерации полностью завершится.
    /// </summary>
    public void CompleteLevelGeneration() 
    {
        // 1. Респавн игрока (вызывает спавн NPC)
        SpawnPlayer(); 

        // 2. Запускаем диалог
        StartDialogueOnRespawn();
    }
    
    // ==========================================================
    //                    ФУНКЦИИ РЕСПАВНА
    // ==========================================================

    void SpawnPlayer()
    {
        if (playerSpawner != null)
        {
             // ПРОСТО ВЫЗЫВАЕМ ФУНКЦИЮ СПАВНА
             playerSpawner.SpawnPlayer(); 
        }
        else
        {
             Debug.LogError("❌ GameStateManager: Player Spawner не назначен!");
        }
    }

    void StartDialogueOnRespawn()
    {
        // ОСТАВЛЯЕМ ФУНКЦИЮ ПУСТОЙ, потому что DialogueTrigger.cs теперь сам считывает DeathCount
        // и запускает нужный Conversation при нажатии кнопки "Talk".
    }

    public void ResetGameProgress()
    {
        deathCount = 0;
        Debug.Log("Прогресс игры сброшен к началу.");
    }
}