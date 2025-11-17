using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSpawn : MonoBehaviour
{
    public GameObject playerPrefab;
    public Tilemap groundTilemap;

    public CameraFollow cameraFollow; // ⬅ добавили ссылку на скрипт камеры

    public void SpawnPlayer()
    {
        if (!playerPrefab)
        {
            Debug.LogError("❌ Player prefab is missing!");
            return;
        }

        if (!groundTilemap)
        {
            Debug.LogError("❌ groundTilemap is missing!");
            return;
        }

        BoundsInt bounds = groundTilemap.cellBounds;

        List<Vector3> validSpawns = new List<Vector3>();

        foreach (var pos in bounds.allPositionsWithin)
        {
            Vector3Int cell = new Vector3Int(pos.x, pos.y, 0);

            if (IsGoodSpawnPoint(cell))
                validSpawns.Add(groundTilemap.GetCellCenterWorld(cell));
        }

        if (validSpawns.Count == 0)
        {
            Debug.LogError("❌ No valid spawn points found!");
            return;
        }

        float centerX = (bounds.xMin + bounds.xMax) * 0.5f;

        Vector3 best = validSpawns[0];
        float bestDist = Mathf.Abs(best.x - centerX);

        foreach (var v in validSpawns)
        {
            float dist = Mathf.Abs(v.x - centerX);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = v;
            }
        }

        Vector3 spawnPos = best + Vector3.up * 1.2f;
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        CameraFollow cf = Camera.main.GetComponent<CameraFollow>();
        if (cf != null)
            cf.SetTarget(player.transform);

        Debug.Log("✔ Player spawned at " + spawnPos);
    }


    private Vector3 FindSpawnPoint(BoundsInt bounds)
    {
        List<Vector3> validSpawns = new List<Vector3>();

        foreach (var pos in bounds.allPositionsWithin)
        {
            Vector3Int cell = new Vector3Int(pos.x, pos.y, 0);

            if (IsGoodSpawnPoint(cell))
                validSpawns.Add(groundTilemap.GetCellCenterWorld(cell));
        }

        if (validSpawns.Count == 0)
        {
            Debug.LogError("❌ No valid spawn points found!");
            return Vector3.zero;
        }

        float centerX = (bounds.xMin + bounds.xMax) * 0.5f;

        Vector3 best = validSpawns[0];
        float bestDist = Mathf.Abs(best.x - centerX);

        foreach (var v in validSpawns)
        {
            float dist = Mathf.Abs(v.x - centerX);
            if (dist < bestDist)
            {
                bestDist = dist;
                best = v;
            }
        }

        return best + Vector3.up * 1.2f;
    }

    private bool IsGoodSpawnPoint(Vector3Int cell)
    {
        if (!groundTilemap.HasTile(cell)) return false;

        if (groundTilemap.HasTile(cell + Vector3Int.up)) return false;
        if (groundTilemap.HasTile(cell + Vector3Int.up * 2)) return false;

        if (groundTilemap.HasTile(cell + new Vector3Int(1, 1, 0))) return false;
        if (groundTilemap.HasTile(cell + new Vector3Int(-1, 1, 0))) return false;

        if (!groundTilemap.HasTile(cell + new Vector3Int(1, -1, 0))) return false;
        if (!groundTilemap.HasTile(cell + new Vector3Int(-1, -1, 0))) return false;

        return true;
    }
}
