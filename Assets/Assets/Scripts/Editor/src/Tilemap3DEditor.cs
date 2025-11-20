using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCSpawner : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject npcPrefab;
    public Tilemap groundTilemap;

    [Header("Параметры Спавна")]
    public int npcCount = 1;
    public float minRadius = 3f; // Строгая минимальная дистанция
    public float maxRadius = 8f; // Строгая максимальная дистанция

    public void SpawnNPCNear(GameObject player)
    {
        Debug.Log("🤖 NPCSpawner: Получена команда спавнить NPC рядом с " + player.name);

        if (npcPrefab == null || groundTilemap == null)
        {
            Debug.LogError("❌ NPCSpawner: Не привязан префаб или тайлмап!");
            return;
        }

        // --- ОБНОВЛЕНИЕ ФИЗИКИ ЗЕМЛИ (WFC) ---
        groundTilemap.RefreshAllTiles();
        TilemapCollider2D col = groundTilemap.GetComponent<TilemapCollider2D>();
        if (col != null) col.ProcessTilemapChanges();
        // ------------------------------------

        Vector3 playerPos = player.transform.position;
        Vector3Int playerCell = groundTilemap.WorldToCell(playerPos);
        List<Vector3> validSpawns = new List<Vector3>();
        int maxRange = Mathf.CeilToInt(maxRadius) + 1;

        // ===================================================
        // ПОПЫТКА 1: Строгий поиск слева (в радиусе min-max)
        // ===================================================

        for (int x = -maxRange; x <= maxRange; x++)
        {
            for (int y = -maxRange; y <= maxRange; y++)
            {
                Vector3Int checkCell = playerCell + new Vector3Int(x, y, 0);

                if (IsGoodSpawnPoint(checkCell))
                {
                    Vector3 worldPos = groundTilemap.GetCellCenterWorld(checkCell);
                    float dist = Vector3.Distance(worldPos, playerPos);

                    // Условия: слева И в заданном радиусе
                    if (checkCell.x < playerCell.x && dist >= minRadius && dist <= maxRadius)
                    {
                        validSpawns.Add(worldPos);
                    }
                }
            }
        }
        
        // ===================================================
        // АВАРИЙНЫЙ ПОИСК (Если Попытка 1 провалилась)
        // ===================================================

        if (validSpawns.Count == 0)
        {
            Debug.LogWarning("NPCSpawner: Строгий радиус слева заблокирован. Ищем везде на левой половине карты.");
            
            // Расширяем поиск на всю карту (bounds)
            BoundsInt bounds = groundTilemap.cellBounds;
            
            foreach (var pos in bounds.allPositionsWithin)
            {
                 Vector3Int checkCell = new Vector3Int(pos.x, pos.y, 0);
                
                 // Условие: слева от игрока (даже если он очень далеко)
                 if (checkCell.x < playerCell.x && IsGoodSpawnPoint(checkCell))
                 {
                    Vector3 worldPos = groundTilemap.GetCellCenterWorld(checkCell);
                    validSpawns.Add(worldPos);
                 }
            }

            // После аварийного поиска, если нашлось много точек, выбираем ту, что ближе к игроку:
            if (validSpawns.Count > 1)
            {
                // Сортируем по дистанции и берем ближайшую (первую)
                validSpawns.Sort((a, b) => Vector3.Distance(a, playerPos).CompareTo(Vector3.Distance(b, playerPos)));
            }
        }
        
        // ===================================================
        // СПАВН
        // ===================================================

        if (validSpawns.Count > 0)
        {
            for(int i = 0; i < npcCount; i++)
            {
                if (validSpawns.Count == 0) break;
                
                // В аварийном режиме мы уже отсортировали, поэтому берем первый элемент.
                // В строгом режиме берем случайный.
                int rnd = (validSpawns.Count == 1 || validSpawns.Count > 1 && validSpawns[0] == validSpawns[1]) ? 0 : Random.Range(0, validSpawns.Count);
                
                // Высота спавна с запасом для платформера
                Vector3 pos = validSpawns[rnd] + Vector3.up * 2.5f;
                
                Instantiate(npcPrefab, pos, Quaternion.identity);
                validSpawns.RemoveAt(rnd);
                Debug.Log($"✅ NPC успешно создан слева от игрока в {pos}");
            }
        }
        else
        {
            // Если даже расширенный поиск не помог
            Debug.LogError("❌ NPCSpawner: Не найдено НИ ОДНОГО места слева от игрока. Карта, вероятно, полностью заблокирована.");
        }
    }

    private bool IsGoodSpawnPoint(Vector3Int cell)
    {
        // Проверка: есть пол И нет блоков в 2 клетках над ним
        return groundTilemap.HasTile(cell) && 
               !groundTilemap.HasTile(cell + Vector3Int.up) && 
               !groundTilemap.HasTile(cell + Vector3Int.up * 2);
    }
}