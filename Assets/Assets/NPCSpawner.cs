using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq; 
using Random = UnityEngine.Random; 

public class NPCSpawner : MonoBehaviour
{
    // Убираем старую заглушку и используем DialogueTrigger
    
    [Header("Настройки")]
    public GameObject npcPrefab;
    public Tilemap groundTilemap;

    [Header("Параметры Спавна")]
    public int npcCount = 1;
    public float idealMinRadius = 3.5f; 
    public float idealMaxRadius = 8f;   
    
    [Header("Абсолютный Минимум")]
    public float absoluteMinDistance = 1.5f; 
    
    // ==========================================================
    //                        ОЧИСТКА СТАРЫХ NPC 
    // ==========================================================
    public void ClearOldNPCs()
    {
        var oldNpcs = GameObject.FindGameObjectsWithTag("NPC");
        if (oldNpcs.Length > 0)
        {
            Debug.Log($"Очистка NPC: Найдено {oldNpcs.Length} старых NPC. Удаляем.");
        }
        
        foreach (var npc in oldNpcs)
        {
            Destroy(npc);
        }
    }

    public void SpawnNPCNear(GameObject player)
    {
        // 1. ГАРАНТИРУЕМ ОЧИСТКУ ПЕРЕД СПАВНОМ НОВЫХ
        ClearOldNPCs();
        
        if (npcPrefab == null || groundTilemap == null)
        {
            Debug.LogError("❌ NPCSpawner: Не привязан префаб или тайлмап!");
            return;
        }

        groundTilemap.RefreshAllTiles();
        TilemapCollider2D col = groundTilemap.GetComponent<TilemapCollider2D>();
        if (col != null) col.ProcessTilemapChanges();

        Vector3 playerPos = player.transform.position;
        Vector3Int playerCell = groundTilemap.WorldToCell(playerPos);
        
        List<Vector3> validSpawns = new List<Vector3>();

        validSpawns = FindSpawnsInRadius(playerPos, playerCell, idealMinRadius, idealMaxRadius, true);

        if (validSpawns.Count == 0)
        {
            Debug.LogWarning("NPCSpawner: Попытка 1 (справа) не удалась. Ищем в ближней зоне 1.5-3.5м.");
            validSpawns = FindSpawnsInRadius(playerPos, playerCell, absoluteMinDistance, idealMinRadius, true);
        }
        
        if (validSpawns.Count == 0)
        {
            Debug.LogWarning("⚠️ NPCSpawner: Ближний поиск провален. Ищем ЛЮБОЙ доступный пол на карте дальше 1.5м.");
            
            validSpawns = FindSpawnsAnywhere(playerPos, absoluteMinDistance);
            
            if (validSpawns.Count > 1)
            {
                validSpawns = validSpawns.OrderBy(p => Vector3.Distance(p, playerPos)).ToList();
            }
        }
        
        // --- Логика Инстанцирования ---
        if (validSpawns.Count > 0)
        {
            // УДАЛЕНА ЛОГИКА РАСЧЕТА targetNodeName, т.к. DialogueTrigger делает это сам.
            // УДАЛЕНА ЛОГИКА ЗАГРУЗКИ И УСТАНОВКИ ДИАЛОГОВОГО НОДА, т.к. она вызывала ошибки.


            for(int i = 0; i < npcCount; i++)
            {
                if (validSpawns.Count == 0) break;
                
                int rnd = (validSpawns.Count > 1 && validSpawns.Count < 5) ? 0 : Random.Range(0, validSpawns.Count); 
                
                Vector3 pos = validSpawns[rnd] + Vector3.up * 2.5f;
                
                GameObject newNpc = Instantiate(npcPrefab, pos, Quaternion.identity);

                // *** ВЕСЬ БЛОК НАСТРОЙКИ ДИАЛОГА УДАЛЕН ***
                // Теперь NPC будет использовать свой DialogueTrigger, 
                // который сам считает прогресс через GameStateManager.

                if (newNpc.tag != "NPC")
                {
                    newNpc.tag = "NPC";
                }
                
                validSpawns.RemoveAt(rnd);
                // Убрали Debug.Log, который ссылался на targetNodeName
                Debug.Log($"✅ NPC создан в {pos}.");
            }
        }
        else
        {
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

                if (IsGoodSpawnPoint(checkCell)) 
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
        if (!groundTilemap.HasTile(cell)) return false; 
        
        if (groundTilemap.HasTile(cell + Vector3Int.up)) return false; 
           
        return true;
    }
}