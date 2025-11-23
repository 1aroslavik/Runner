using UnityEngine;
using WFC;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    private int deathCount = 0;
    public int DeathCount => deathCount;

    [Header("Ссылки Менеджеров")]
    public PlayerSpawn playerSpawner;
    public WFCTilemapGenerator levelGenerator;
    public DialogueManager dialogueSystem;

    [Header("Окно победы")]
    public GameObject winPanel; // ← новое поле

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Debug.Log("Game Manager инициализирован. Текущий счетчик смертей: " + deathCount);
    }

    // ===============================
    //      СМЕРТЬ ИГРОКА
    // ===============================
    public void HandlePlayerDeath(GameObject playerObject)
    {
        Debug.Log($"Игрок умер. Текущий счетчик: {deathCount}");

        if (playerObject != null)
            Destroy(playerObject);

        deathCount++;

        Invoke("RestartGame", 2f);
    }

    void RestartGame()
    {
        if (levelGenerator != null)
        {
            levelGenerator.GenerateNewLevel();
        }
        else
        {
            Debug.LogError("❌ GameStateManager: LevelGenerator не назначен. Респавн невозможен!");
        }
    }

    // =================================
    // Вызвается после генерации уровня
    // =================================
    public void CompleteLevelGeneration()
    {
        DeathScreenUI.Instance?.HideDeathScreen();

        SpawnPlayer();
        StartDialogueOnRespawn();
    }

    // ===============================
    //       РЕСПАВН ИГРОКА
    // ===============================
    void SpawnPlayer()
    {
        if (playerSpawner != null)
        {
            playerSpawner.SpawnPlayer();
        }
        else
        {
            Debug.LogError("❌ GameStateManager: Player Spawner не назначен!");
        }
    }

    void StartDialogueOnRespawn()
    {
        // Диалог запускается триггером
    }

    // ===============================
    //      ПОБЕДА НАД БОССОМ
    // ===============================
    public void HandleBossDeath()
    {
        Debug.Log("🏆 Победа! Босс уничтожен!");

        if (winPanel != null)
            winPanel.SetActive(true);
        else
            Debug.LogError("❌ WinPanel не назначен!");

        Time.timeScale = 0f; // пауза игры
    }

    public void ResetGameProgress()
    {
        deathCount = 0;
        Debug.Log("Прогресс игры сброшен.");
    }
}
