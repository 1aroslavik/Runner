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
    public GameObject winPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("⚠️ Обнаружен второй GameStateManager! Удаляю объект: " + gameObject.name);
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
            Debug.LogError("❌ GameStateManager: LevelGenerator не назначен! Респавн невозможен!");
        }
    }

    public void CompleteLevelGeneration()
    {
        Debug.Log("🏁 WFCTilemapGenerator сообщил: генерация завершена.");

        DeathScreenUI.Instance?.HideDeathScreen();

        SpawnPlayer();

        StartDialogueOnRespawn();
    }

    // ===============================
    //       РЕСПАВН ИГРОКА
    // ===============================
    void SpawnPlayer()
    {
        Debug.Log("▶️ SpawnPlayer()");
        Debug.Log("🔍 playerSpawner = " + (playerSpawner ? playerSpawner.name : "NULL"));

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

        Time.timeScale = 0f;
    }

    public void ResetGameProgress()
    {
        deathCount = 0;
        Debug.Log("Прогресс игры сброшен.");
    }
}
