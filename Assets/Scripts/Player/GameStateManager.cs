using UnityEngine;
using WFC; // Добавляем, чтобы видеть WFCTilemapGenerator

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private int deathCount = 0;
    public int DeathCount => deathCount;

    [Header("Ссылки Менеджеров")]
    public PlayerSpawn playerSpawner; 
    public WFCTilemapGenerator levelGenerator; 
    public DialogueManager dialogueSystem;

    void Awake()
    {
        Debug.Log("▶️ GameStateManager.Awake() вызван у: " + gameObject.name);

        // Проверка на дубликаты
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("⚠️ Обнаружен второй GameStateManager! Удаляю объект: " + gameObject.name);
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Debug.Log("✔️ GameStateManager установлен как Singleton.");
        Debug.Log("🔍 levelGenerator в Awake = " + (levelGenerator ? levelGenerator.name : "NULL"));
    }

    void Start()
    {
        Debug.Log("▶️ GameStateManager.Start()");
        Debug.Log("🔍 levelGenerator в Start = " + (levelGenerator ? levelGenerator.name : "NULL"));
    }

    public void HandlePlayerDeath(GameObject playerObject)
    {
        Debug.Log($"💀 Игрок умер. Текущий счетчик смертей: {deathCount}");
        Debug.Log("🔍 levelGenerator при смерти игрока = " + (levelGenerator ? levelGenerator.name : "NULL"));

        if (playerObject != null)
        {
            Debug.Log("🗑 Уничтожаю объект игрока: " + playerObject.name);
            Destroy(playerObject);
        }

        deathCount++;
        Debug.Log($"📈 Счетчик смертей увеличен: {deathCount}");

        Invoke("RestartGame", 2f);
    }

    void RestartGame()
    {
        Debug.Log("🔄 RestartGame начат.");
        Debug.Log("🔍 levelGenerator в RestartGame = " + (levelGenerator ? levelGenerator.name : "NULL"));

        if (levelGenerator != null)
        {
            Debug.Log("🧱 Запускаю генерацию нового уровня через WFC.");
            levelGenerator.GenerateNewLevel();
        }
        else
        {
            Debug.LogError("❌ GameStateManager: LevelGenerator не назначен! Респавн невозможен!");
        }
    }

    public void CompleteLevelGeneration()
    {
        Debug.Log("🏁 WFCTilemapGenerator сообщил: генерация завершена.");
        Debug.Log("🔍 levelGenerator в CompleteLevelGeneration = " + (levelGenerator ? levelGenerator.name : "NULL"));

        DeathScreenUI.Instance?.HideDeathScreen();

        Debug.Log("👤 Спавню игрока...");
        SpawnPlayer();

        Debug.Log("💬 Запускаю диалог (если требуется)...");
        StartDialogueOnRespawn();
    }

    void SpawnPlayer()
    {
        Debug.Log("▶️ SpawnPlayer()");
        Debug.Log("🔍 playerSpawner = " + (playerSpawner ? playerSpawner.name : "NULL"));

        if (playerSpawner != null)
        {
            playerSpawner.SpawnPlayer();
            Debug.Log("✔️ Игрок успешно заспавнен.");
        }
        else
        {
            Debug.LogError("❌ GameStateManager: Player Spawner не назначен!");
        }
    }

    void StartDialogueOnRespawn()
    {
        Debug.Log("▶️ StartDialogueOnRespawn()");
    }

    public void ResetGameProgress()
    {
        deathCount = 0;
        Debug.Log("🔄 Прогресс игры сброшен.");
    }
}
