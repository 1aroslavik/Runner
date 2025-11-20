using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq; // Добавляем для сортировки

public class NPCSpawner : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject npcPrefab;
    public Tilemap groundTilemap;

    [Header("Параметры Спавна")]
    public int npcCount = 1;
    public float idealMinRadius = 3.5f; 
    public float idealMaxRadius = 8f;   
    
    [Header("Абсолютный Минимум")]
    // NPC не должен спавниться ближе этой дистанции.
    public float absoluteMinDistance = 1.5f; 

    public void SpawnNPCNear(GameObject player)
    {
        if (npcPrefab == null || groundTilemap == null)
        {
            Debug.LogError("❌ NPCSpawner: Не привязан префаб или тайлмап!");
            return;
        }

        // Обновление физики земли
        groundTilemap.RefreshAllTiles();
        TilemapCollider2D col = groundTilemap.GetComponent<TilemapCollider2D>();
        if (col != null) col.ProcessTilemapChanges();

        Vector3 playerPos = player.transform.position;
        Vector3Int playerCell = groundTilemap.WorldToCell(playerPos);
        List<Vector3> validSpawns = new List<Vector3>();

        // 1. ПОПЫТКА 1: Строгий поиск в ИДЕАЛЬНОЙ зоне (3.5 - 8.0 м), ТОЛЬКО СПРАВА
        validSpawns = FindSpawnsInRadius(playerPos, playerCell, idealMinRadius, idealMaxRadius, true);

        // 2. ПОПЫТКА 2: Аварийный поиск в БЛИЖНЕЙ зоне (1.5 - 3.5 м), ТОЛЬКО СПРАВА
        if (validSpawns.Count == 0)
        {
            Debug.LogWarning("NPCSpawner: Попытка 1 (справа) не удалась. Ищем в ближней зоне 1.5-3.5м.");
            validSpawns = FindSpawnsInRadius(playerPos, playerCell, absoluteMinDistance, idealMinRadius, true);
        }
        
        // 3. ПОПЫТКА 3: ПОИСК ПОСЛЕДНЕЙ НАДЕЖДЫ (Ищем ЛЮБОЕ МЕСТО на всей карте > 1.5м)
        if (validSpawns.Count == 0)
        {
            Debug.LogWarning("⚠️ NPCSpawner: Ближний поиск провален. Ищем ЛЮБОЙ доступный пол на карте дальше 1.5м.");
            
            // Ищем по всей карте, но требуем, чтобы место было ДОСТУПНО (IsGoodSpawnPoint)
            validSpawns = FindSpawnsAnywhere(playerPos, absoluteMinDistance);
            
            // Если нашли, сортируем по близости к игроку, чтобы взять ближайший
            if (validSpawns.Count > 1)
            {
                validSpawns = validSpawns.OrderBy(p => Vector3.Distance(p, playerPos)).ToList();
            }
        }
        
        // --- Логика Инстанцирования ---
        if (validSpawns.Count > 0)
        {
            for(int i = 0; i < npcCount; i++)
            {
                if (validSpawns.Count == 0) break;
                
                // В случае "Последней надежды" validSpawns[0] - это ближайшая точка.
                int rnd = (validSpawns.Count > 1 && validSpawns.Count < 5) ? 0 : Random.Range(0, validSpawns.Count); 
                
                Vector3 pos = validSpawns[rnd] + Vector3.up * 2.5f;
                
                Instantiate(npcPrefab, pos, Quaternion.identity);
                validSpawns.RemoveAt(rnd);
                Debug.Log($"✅ NPC ГАРАНТИРОВАНО создан в {pos}.");
            }
        }
        else
        {
            // Это сообщение означает, что на карте нет ни одного доступного пола.
            Debug.LogWarning("❌ NPCSpawner: Не найдено НИ ОДНОЙ ДОСТУПНОЙ КЛЕТКИ на карте дальше 1.5м. Спавн невозможен.");
        }
    }

    // --- Поиск по заданному радиусу ---
    private List<Vector3> FindSpawnsInRadius(Vector3 playerPos, Vector3Int playerCell, float minD, float maxD, bool mustBeRight)
    {
        List<Vector3> foundSpawns = new List<Vector3>();
        int maxSearchRange = Mathf.CeilToInt(maxD) + 1;

        for (int x = -maxSearchRange; x <= maxSearchRange; x++)
        {
            for (int y = -maxSearchRange; y <= maxSearchRange; y++)
            {
                Vector3Int checkCell = playerCell + new Vector3Int(x, y, 0);

                if (IsGoodSpawnPoint(checkCell)) // Требуем доступного места
                {
                    Vector3 worldPos = groundTilemap.GetCellCenterWorld(checkCell);
                    float dist = Vector3.Distance(worldPos, playerPos);

                    bool isRight = checkCell.x > playerCell.x;
                    
                    if (mustBeRight && !isRight) continue; 

                    if (dist >= minD && dist <= maxD)
                    {
                        foundSpawns.Add(worldPos);
                    }
                }
            }
        }
        return foundSpawns;
    }
    
    // --- Поиск по всей карте с минимальным отступом ---
    private List<Vector3> FindSpawnsAnywhere(Vector3 playerPos, float minD)
    {
        List<Vector3> foundSpawns = new List<Vector3>();
        BoundsInt bounds = groundTilemap.cellBounds;

        foreach (var pos in bounds.allPositionsWithin)
        {
             Vector3Int checkCell = new Vector3Int(pos.x, pos.y, 0);
             
             // KEY: Требуем, чтобы это был доступный пол, а не стена/потолок
             if (IsGoodSpawnPoint(checkCell)) 
             {
                Vector3 worldPos = groundTilemap.GetCellCenterWorld(checkCell);
                
                if (Vector3.Distance(worldPos, playerPos) >= minD)
                {
                   foundSpawns.Add(worldPos);
                }
             }
        }
        return foundSpawns;
    }
    
    // --- Смягченная проверка пола/потолка (позволяет спавн в туннелях высотой 2 юнита) ---
    private bool IsGoodSpawnPoint(Vector3Int cell)
    {
        // 1. Проверка: есть пол
        if (!groundTilemap.HasTile(cell)) return false; 
        
        // 2. Проверка: гарантированно пуста ТОЛЬКО 1 клетка над полом.
        // Мы убрали проверку второй клетки, чтобы спавн работал в узких туннелях.
        if (groundTilemap.HasTile(cell + Vector3Int.up)) return false; 
           
        return true;
    }
}