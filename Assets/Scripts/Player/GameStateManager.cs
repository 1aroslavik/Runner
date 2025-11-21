using UnityEngine; 
using WFC; // <--- ДОБАВЛЕНО для доступа к WFCTilemapGenerator (исправляет CS0246)

public class GameStateManager : MonoBehaviour
{
    // Паттерн Одиночка (Singleton) для легкого доступа из других скриптов
    public static GameStateManager Instance { get; private set; }

    // Переменная для отслеживания, какой диалог запускать
    private int deathCount = 0;

    // Ссылка на объект игрока, который нужно удалить после смерти (старая ссылка)
    // Эта переменная используется для временного хранения ссылки, но мы ее не используем в SpawnPlayer.
    private GameObject playerObject; 

    // НОВАЯ ССЫЛКА: Используем ваш существующий скрипт PlayerSpawn для создания нового игрока
    public PlayerSpawn playerSpawner; 

    // Ссылка на ваш скрипт диалоговой системы.
    public DialogueManager dialogueSystem; 

    // Ссылка на ваш скрипт Генератора Уровня. (ИСПРАВЛЕНО имя класса и добавлен using WFC)
    public WFCTilemapGenerator levelGenerator; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // --- ЛОГИКА СМЕРТИ ---
    // Эта функция вызывается из PlayerHealth.Die().
    // ИСПРАВЛЕНО: Теперь функция принимает ОДИН АРГУМЕНТ (GameObject playerObject)! (Исправляет CS1061)
    public void HandlePlayerDeath(GameObject playerObject) 
    {
        // УДАЛЯЕМ старый объект игрока, чтобы PlayerSpawn мог создать новый
        Destroy(playerObject); 
        this.playerObject = null; // Очищаем ссылку
        
        deathCount++; 
        
        // Запускаем процесс перезапуска игры через 2 секунды
        Invoke("RestartGame", 2f); 
    }

    void RestartGame()
    {
        // 1. Генерируем новый уровень
        if (levelGenerator != null)
        {
            // Убедитесь, что в WFCTilemapGenerator.cs есть функция public void GenerateNewLevel()
            levelGenerator.Generate(); 
        }

        // 2. Респавн игрока
        SpawnPlayer(); 

        // 3. Запускаем диалог
        StartDialogueOnRespawn();
    }

    // --- ЛОГИКА СПАВНА ---
    void SpawnPlayer()
    {
        if (playerSpawner != null)
        {
             // ВЫЗЫВАЕМ ВАШУ УЖЕ РАБОЧУЮ ФУНКЦИЮ, КОТОРАЯ СОЗДАЕТ НОВОГО ИГРОКА!
             playerSpawner.SpawnPlayer(); 
        }
        else
        {
             Debug.LogError("❌ Player Spawner (PlayerSpawn.cs) не назначен в GameStateManager!");
        }
    }

    // --- ЛОГИКА ДИАЛОГА ---
    void StartDialogueOnRespawn()
    {
        if (dialogueSystem != null)
        {
            string nodeToStart;

            if (deathCount == 1)
            {
                nodeToStart = "RespawnNode_1"; 
            }
            else if (deathCount == 2)
            {
                nodeToStart = "RespawnNode_2"; 
            }
            else
            {
                nodeToStart = "DefaultRespawnNode"; 
            }

            // Убедитесь, что в DialogueManager.cs есть функция public void StartDialogueNode(string nodeName)
            dialogueSystem.StartDialogueNode(nodeToStart);
        }
    }
}