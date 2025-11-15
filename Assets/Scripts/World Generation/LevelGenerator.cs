using UnityEngine;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Поиск пола для игрока")]
    public LayerMask groundMask;   // тут выберем слой, на котором лежит пол

    [Header("Игрок")]
    public GameObject playerPrefab;
    public CameraFollow cameraFollow;

    [Header("Размер комнаты в юнитах (логическая сетка)")]
    public int roomWidth = 20;
    public int roomHeight = 12;

    [Header("Комнаты")]
    public Room startRoomPrefab;
    public Room endRoomPrefab;

    [Tooltip("Комнаты для основного пути (движение вправо)")]
    public Room[] forwardRooms;

    [Tooltip("Комнаты для веток ВВЕРХ (вход снизу, выход сверху)")]
    public Room[] upRooms;

    [Tooltip("Комнаты для веток ВНИЗ (вход сверху, выход снизу)")]
    public Room[] downRooms;

    [Tooltip("Концевые комнаты веток (сундук, награда)")]
    public Room[] branchEndRooms;

    [Header("Структура уровня")]
    public int mainPathLength = 8;   // сколько комнат по основной линии
    public int minBranchLength = 2;
    public int maxBranchLength = 4;

    [Header("Шанс ответвлений от каждой комнаты")]
    [Range(0, 1)] public float chanceBranchUp = 0.35f;
    [Range(0, 1)] public float chanceBranchDown = 0.45f;

    [Header("Враги")]
    public Enemy enemyPrefab;
    public EnemyType[] enemyTypes;
    [Range(0, 1)] public float enemyRoomChance = 0.7f;
    public int minEnemiesPerRoom = 1;
    public int maxEnemiesPerRoom = 3;

    [Header("Сундуки")]
    public GameObject chestPrefab;
    [Range(0, 1)] public float chestChance = 0.3f;

    private List<Room> spawnedRooms = new List<Room>();
    private Dictionary<Vector2Int, Room> roomGrid = new Dictionary<Vector2Int, Room>();
    private GameObject playerInstance;

    // ===========================
    // START
    // ===========================
    private void Start()
    {
        GenerateLevel();
        SpawnPlayer();
    }

    // ===========================
    // ГЕНЕРАЦИЯ УРОВНЯ
    // ===========================
    private void GenerateLevel()
    {
        spawnedRooms.Clear();
        roomGrid.Clear();

        // Стартовая клетка
        Vector2Int currentCell = Vector2Int.zero;

        Room startRoom = PlaceRoomAtCell(startRoomPrefab, currentCell);
        spawnedRooms.Add(startRoom);

        // Основной путь вправо
        for (int i = 0; i < mainPathLength; i++)
        {
            Vector2Int nextCell = currentCell + Vector2Int.right;

            // если вдруг клетка занята — просто прерываем путь
            if (roomGrid.ContainsKey(nextCell))
            {
                Debug.LogWarning($"Клетка {nextCell} уже занята, основной путь обрывается.");
                break;
            }

            Room forwardPrefab = forwardRooms[Random.Range(0, forwardRooms.Length)];
            Room forwardRoom = PlaceRoomAtCell(forwardPrefab, nextCell);

            spawnedRooms.Add(forwardRoom);
            currentCell = nextCell;

            SpawnEnemiesInRoom(forwardRoom);
            SpawnChestsInRoom(forwardRoom);

            // Ветка вниз
            if (Random.value < chanceBranchDown)
            {
                CreateBranch(currentCell, Vector2Int.down, downRooms);
            }

            // Ветка вверх
            if (Random.value < chanceBranchUp)
            {
                CreateBranch(currentCell, Vector2Int.up, upRooms);
            }
        }

        // Конечная комната справа от последней клетки основного пути
        Vector2Int endCell = currentCell + Vector2Int.right;
        if (endRoomPrefab != null && !roomGrid.ContainsKey(endCell))
        {
            Room endRoom = PlaceRoomAtCell(endRoomPrefab, endCell);
            spawnedRooms.Add(endRoom);

            SpawnEnemiesInRoom(endRoom);
            SpawnChestsInRoom(endRoom);
        }
    }

    // ===========================
    // УСТАНОВКА КОМНАТЫ В КЛЕТКУ СЕТКИ
    // ===========================
    private Room PlaceRoomAtCell(Room prefab, Vector2Int cell)
    {
        Vector3 worldPos = new Vector3(cell.x * roomWidth, cell.y * roomHeight, 0f);
        Room room = Instantiate(prefab, worldPos, Quaternion.identity);
        roomGrid[cell] = room;
        return room;
    }

    // ===========================
    // СОЗДАНИЕ ВЕТКИ ВВЕРХ/ВНИЗ
    // ===========================
    private void CreateBranch(Vector2Int startCell, Vector2Int dir, Room[] pool)
    {
        if (pool == null || pool.Length == 0) return;

        int length = Random.Range(minBranchLength, maxBranchLength + 1);
        Vector2Int currentCell = startCell;

        for (int i = 0; i < length; i++)
        {
            Vector2Int nextCell = currentCell + dir;

            // если клетка занята — прекращаем ветку
            if (roomGrid.ContainsKey(nextCell))
                return;

            Room prefab = pool[Random.Range(0, pool.Length)];
            Room room = PlaceRoomAtCell(prefab, nextCell);

            spawnedRooms.Add(room);
            SpawnEnemiesInRoom(room);
            SpawnChestsInRoom(room);

            currentCell = nextCell;
        }

        // Конечная комната ветки
        if (branchEndRooms != null && branchEndRooms.Length > 0)
        {
            Vector2Int endCell = currentCell + dir;
            if (!roomGrid.ContainsKey(endCell))
            {
                Room endPrefab = branchEndRooms[Random.Range(0, branchEndRooms.Length)];
                Room endRoom = PlaceRoomAtCell(endPrefab, endCell);

                spawnedRooms.Add(endRoom);
                SpawnEnemiesInRoom(endRoom);
                SpawnChestsInRoom(endRoom);
            }
        }
    }

    // ===========================
    // СПАВН ИГРОКА
    // ===========================
  
// ===========================
// СПАВН ИГРОКА
// ===========================
private void SpawnPlayer()
{
    if (spawnedRooms.Count == 0)
    {
        Debug.LogError("Нет сгенерированных комнат, некуда ставить игрока.");
        return;
    }

    Room startRoom = spawnedRooms[0];

    if (startRoom.playerSpawnPoint == null)
    {
        Debug.LogError("В стартовой комнате не назначен playerSpawnPoint.");
        return;
    }

    // сначала ставим чуть выше
    Vector3 spawnPos = startRoom.playerSpawnPoint.position + Vector3.up * 1.5f;
    playerInstance = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

    // ищем пол ПОД игроком
    RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 20f, groundMask);

    if (hit.collider != null)
    {
        float halfHeight = playerInstance.GetComponent<Collider2D>().bounds.size.y / 2f;
        playerInstance.transform.position = new Vector3(
            spawnPos.x,
            hit.point.y + halfHeight + 0.05f,
            spawnPos.z
        );
    }
    else
    {
        Debug.LogWarning("Пол под игроком не найден!");
    }

    // подключаем камеру
    if (cameraFollow != null)
        cameraFollow.target = playerInstance.transform;
}






    // ===========================
    // ВРАГИ
    // ===========================
    private void SpawnEnemiesInRoom(Room room)
    {
        if (enemyPrefab == null) return;
        if (room.enemySpawnPoints == null || room.enemySpawnPoints.Length == 0) return;
        if (Random.value > enemyRoomChance) return;

        int maxCount = Mathf.Min(maxEnemiesPerRoom, room.enemySpawnPoints.Length);
        int count = Random.Range(minEnemiesPerRoom, maxCount + 1);

        // перемешиваем точки
        List<Transform> points = new List<Transform>();
        foreach (var p in room.enemySpawnPoints)
            if (p != null) points.Add(p);

        if (points.Count == 0) return;

        for (int i = 0; i < count && i < points.Count; i++)
        {
            Transform t = points[i];
            Enemy e = Instantiate(enemyPrefab, t.position, Quaternion.identity);
            e.transform.SetParent(room.transform);

            if (enemyTypes != null && enemyTypes.Length > 0)
            {
                e.type = enemyTypes[Random.Range(0, enemyTypes.Length)];
                // Enemy сам подхватит type в Start()
            }
        }
    }

    // ===========================
    // СУНДУКИ
    // ===========================
    private void SpawnChestsInRoom(Room room)
    {
        if (chestPrefab == null) return;
        if (room.chestSpawnPoints == null || room.chestSpawnPoints.Length == 0) return;
        if (Random.value > chestChance) return;

        // выбираем случайную точку
        List<Transform> points = new List<Transform>();
        foreach (var p in room.chestSpawnPoints)
            if (p != null) points.Add(p);

        if (points.Count == 0) return;

        Transform t = points[Random.Range(0, points.Count)];
        GameObject chest = Instantiate(chestPrefab, t.position, Quaternion.identity);
        chest.transform.SetParent(room.transform);
    }
}
