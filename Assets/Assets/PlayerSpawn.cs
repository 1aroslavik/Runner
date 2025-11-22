using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerSpawn : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Tilemap groundTilemap;

    [Header("UPGRADES")]
    public UpgradeUI upgradeUI;

    [Header("Camera")]
    public CameraFollow cameraFollow;

    [Header("NPC Spawner")]
    public NPCSpawner npcSpawner;

    [Header("Start Room Spawn Point")]
    public string spawnPointTag = "PlayerSpawnPoint";

    private void ClearOldPlayer()
    {
        var oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in oldPlayers)
        {
            if (p.GetComponent<PlayerHealth>() != null)
                Destroy(p);
        }
    }

    // ==========================================================
    //                       СПАВН ИГРОКА
    // ==========================================================
    public void SpawnPlayer()
    {
        ClearOldPlayer();

        Vector3 spawnPos = GetPlayerSpawnPosition();
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        PlayerStats stats = player.GetComponent<PlayerStats>();

        // === Загружаем постоянные статы ===
        PermanentStats.Instance.ApplyTo(stats);

        // === Передаём UI выбора апгрейда ===
        if (upgradeUI != null)
            upgradeUI.playerStats = stats;

        // === Главный фикс: передать UI статов НОВОМУ игроку ===
        var statsUI = FindObjectOfType<PlayerStatsUI>();
        if (statsUI != null)
        {
            statsUI.SetStats(stats);   // <<< ВАЖНО
            statsUI.UpdateUI(stats);
        }

        // Камера
        if (cameraFollow != null)
            cameraFollow.SetTarget(player.transform);

        // NPC
        npcSpawner?.SpawnNPCNear(player);
    }

    // ==========================================================
    //                  ПОЗИЦИЯ СПАВНА
    // ==========================================================
    public Vector3 GetPlayerSpawnPosition()
    {
        if (!playerPrefab || !groundTilemap) return Vector3.zero;

        GameObject spawnObj = GameObject.FindWithTag(spawnPointTag);

        if (spawnObj != null)
            return spawnObj.transform.position;

        return FindSpawnPoint(groundTilemap.cellBounds);
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
            return Vector3.zero;

        float centerX = (bounds.xMin + bounds.xMax) / 2f;

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
