using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject playerPrefab;
    public Tilemap groundTilemap;

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("❌ playerPrefab is not assigned!");
            return;
        }

        if (groundTilemap == null)
        {
            Debug.LogError("❌ groundTilemap is not assigned!");
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;

        List<Vector3> topTiles = new List<Vector3>();

        // 1) Собираем все верхние плитки (тайлы, у которых сверху нет другой тайлы)
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);

                if (groundTilemap.HasTile(cell))
                {
                    // проверяем, нет ли тайлы выше
                    Vector3Int above = new Vector3Int(x, y + 1, 0);

                    if (!groundTilemap.HasTile(above))
                    {
                        topTiles.Add(groundTilemap.GetCellCenterWorld(cell));
                    }
                }
            }
        }

        if (topTiles.Count == 0)
        {
            Debug.LogError("❌ No valid platforms found!");
            return;
        }

        // 2) Ищем плитку, которая ближе всего к центру карты
        float centerX = (bounds.xMin + bounds.xMax) * 0.5f;

        Vector3 bestTile = topTiles[0];
        float bestDist = Mathf.Abs(bestTile.x - centerX);

        foreach (var t in topTiles)
        {
            float dist = Mathf.Abs(t.x - centerX);

            if (dist < bestDist)
            {
                bestDist = dist;
                bestTile = t;
            }
        }

        // 3) Спавним игрока над этой плиткой
        Vector3 spawnPos = bestTile + Vector3.up * 1.2f;

        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        // 4) Камера на игрока
        if (Camera.main != null)
        {
            Camera.main.transform.position =
                new Vector3(spawnPos.x, spawnPos.y, Camera.main.transform.position.z);
        }

        Debug.Log("✔ Player spawned on top platform " + spawnPos);
    }
}
