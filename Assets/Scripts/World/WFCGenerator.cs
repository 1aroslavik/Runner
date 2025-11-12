using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFCGenerator : MonoBehaviour
{
    [Header("Размер сетки уровня")]
    public int gridWidth = 5;
    public int gridHeight = 5;
    public float roomSize = 15f;

    [Header("Ссылки")]
    public RoomTemplates templates;
    public GameObject playerPrefab;     // префаб игрока

    private Room[,] grid;

    void Start()
    {
        StartCoroutine(GenerateThenSpawnPlayer());
    }

    IEnumerator GenerateThenSpawnPlayer()
    {
        // Создаём сетку комнат
        grid = new Room[gridWidth, gridHeight];
        GenerateLevel();

        yield return new WaitForSeconds(0.1f);

        // Позиция спавна игрока
        Vector3 playerSpawn = new Vector3(
            (gridWidth / 2) * roomSize - 5f,
            (gridHeight / 2) * roomSize + 2f,
            0
        );

        // Создаём игрока
        GameObject player = Instantiate(playerPrefab, playerSpawn, Quaternion.identity);

        // Перемещаем камеру к игроку
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            Vector3 camPos = player.transform.position;
            camPos.z = -10f;
            mainCam.transform.position = camPos;
        }

        // Если есть скрипт CameraFollow — подключаем игрока
        CameraFollow follow = Camera.main.GetComponent<CameraFollow>();
        if (follow != null)
        {
            follow.target = player.transform;
        }

        Debug.Log($"🎮 Игрок появился в {playerSpawn}");
    }

    void GenerateLevel()
    {
        Vector2Int start = new Vector2Int(gridWidth / 2, gridHeight / 2);
        PlaceRoom(start, GetRandomRoom());

        Queue<Vector2Int> frontier = new Queue<Vector2Int>();
        frontier.Enqueue(start);

        while (frontier.Count > 0)
        {
            Vector2Int pos = frontier.Dequeue();
            Room current = grid[pos.x, pos.y];

            TryPlaceNeighbor(pos, Vector2Int.up, current.up, frontier);
            TryPlaceNeighbor(pos, Vector2Int.down, current.down, frontier);
            TryPlaceNeighbor(pos, Vector2Int.left, current.left, frontier);
            TryPlaceNeighbor(pos, Vector2Int.right, current.right, frontier);
        }
    }

    void TryPlaceNeighbor(Vector2Int pos, Vector2Int dir, bool hasExit, Queue<Vector2Int> frontier)
    {
        if (!hasExit) return;

        Vector2Int newPos = pos + dir;
        if (OutOfBounds(newPos) || grid[newPos.x, newPos.y] != null)
            return;

        List<GameObject> candidates = new List<GameObject>();
        foreach (GameObject prefab in templates.allRooms)
        {
            Room r = prefab.GetComponent<Room>();
            if (dir == Vector2Int.up && r.down ||
                dir == Vector2Int.down && r.up ||
                dir == Vector2Int.left && r.right ||
                dir == Vector2Int.right && r.left)
            {
                int score = r.weight;
                for (int i = 0; i < score; i++)
                    candidates.Add(prefab);
            }
        }

        if (candidates.Count == 0) return;

        GameObject chosen = candidates[Random.Range(0, candidates.Count)];
        PlaceRoom(newPos, chosen.GetComponent<Room>());
        frontier.Enqueue(newPos);
    }

    void PlaceRoom(Vector2Int gridPos, Room roomPrefab)
    {
        Vector3 worldPos = new Vector3(gridPos.x * roomSize, gridPos.y * roomSize, 0);
        Room newRoom = Instantiate(roomPrefab, worldPos, Quaternion.identity);
        grid[gridPos.x, gridPos.y] = newRoom;
    }

    bool OutOfBounds(Vector2Int p)
    {
        return p.x < 0 || p.y < 0 || p.x >= gridWidth || p.y >= gridHeight;
    }

    Room GetRandomRoom()
    {
        if (templates == null || templates.allRooms == null || templates.allRooms.Length == 0)
        {
            Debug.LogError("❌ Нет ни одной комнаты в RoomTemplates! Добавь хотя бы одну.");
            return null;
        }

        GameObject prefab = templates.allRooms[Random.Range(0, templates.allRooms.Length)];
        return prefab.GetComponent<Room>();
    }
}
