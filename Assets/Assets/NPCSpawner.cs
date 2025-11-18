using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NPCSpawner : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject npcPrefab;
    public Tilemap groundTilemap;

    [Header("Параметры")]
    public int npcCount = 1;
    public float minRadius = 3f;
    public float maxRadius = 8f;

    public void SpawnNPCNear(GameObject player)
    {
        Debug.Log("🤖 NPCSpawner: Получена команда спавнить NPC рядом с " + player.name);

        if (npcPrefab == null || groundTilemap == null)
        {
            Debug.LogError("❌ NPCSpawner: Не привязан префаб или тайлмап!");
            return;
        }

        // --- ВАЖНО: Принудительно обновляем коллайдер земли ПЕРЕД поиском ---
        // Это гарантирует, что Unity знает о твердости сгенерированных тайлов
        groundTilemap.RefreshAllTiles();
        TilemapCollider2D col = groundTilemap.GetComponent<TilemapCollider2D>();
        if (col != null) col.ProcessTilemapChanges();
        // -------------------------------------------------------------------

        Vector3 playerPos = player.transform.position;
        Vector3Int playerCell = groundTilemap.WorldToCell(playerPos);
        List<Vector3> validSpawns = new List<Vector3>();

        int range = Mathf.CeilToInt(maxRadius) + 1;

        // Поиск точек
        for (int x = -range; x <= range; x++)
        {
            for (int y = -range; y <= range; y++)
            {
                Vector3Int checkCell = playerCell + new Vector3Int(x, y, 0);

                if (IsGoodSpawnPoint(checkCell))
                {
                    Vector3 worldPos = groundTilemap.GetCellCenterWorld(checkCell);
                    float dist = Vector3.Distance(worldPos, playerPos);

                    if (dist >= minRadius && dist <= maxRadius)
                    {
                        validSpawns.Add(worldPos);
                    }
                }
            }
        }

        if (validSpawns.Count > 0)
        {
            for(int i = 0; i < npcCount; i++)
            {
                if (validSpawns.Count == 0) break;
                int rnd = Random.Range(0, validSpawns.Count);
                
                // Поднимаем на 2.5, чтобы точно не застрять в полу при спавне
                Vector3 pos = validSpawns[rnd] + Vector3.up * 2.5f;
                
                Instantiate(npcPrefab, pos, Quaternion.identity);
                validSpawns.RemoveAt(rnd);
                Debug.Log($"✅ NPC успешно создан в {pos}");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ NPCSpawner: Не нашел места рядом с игроком.");
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