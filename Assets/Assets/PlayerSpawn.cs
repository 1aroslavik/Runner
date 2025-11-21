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
    [Tooltip("Тэг объекта внутри стартовой комнаты, который задаёт точку спавна игрока.")]
    public string spawnPointTag = "PlayerSpawnPoint";

    // ПРИМЕЧАНИЕ: Здесь нет функций Awake/Start, что хорошо, 
    // так как это предотвращает автоматический спавн без GameStateManager.

    // ==========================================================
    //                        ОЧИСТКА СТАРЫХ ИГРОКОВ
    // ==========================================================
    /// <summary>
    /// Удаляет всех существующих игроков, чтобы гарантировать, что останется только один.
    /// Вызывается перед SpawnPlayer.
    /// </summary>
    private void ClearOldPlayer()
    {
        // Ищем всех игроков по тегу.
        var oldPlayers = GameObject.FindGameObjectsWithTag("Player");
        if (oldPlayers.Length > 0)
        {
            Debug.Log($"Очистка: Найдено {oldPlayers.Length} старых игроков. Удаляем.");
        }
        
        foreach (var player in oldPlayers)
        {
            // Убеждаемся, что мы не удаляем сам Game Manager или другие важные объекты.
            if (player.GetComponent<PlayerHealth>() != null) 
            {
                Destroy(player);
            }
        }
    }
    
    // ==========================================================
    //                        СПАВН ИГРОКА (ВЫЗЫВАЕТСЯ GAMEMANAGER)
    // ==========================================================
    public void SpawnPlayer()
    {
        // 1. ГАРАНТИРУЕМ, ЧТО НЕТ ДУБЛИКАТОВ
        ClearOldPlayer(); 

        // 2. ПОЛУЧАЕМ ПОЗИЦИЮ СПАВНА
        Vector3 spawnPos = GetPlayerSpawnPosition();
        
        if (spawnPos == Vector3.zero)
        {
             Debug.LogError("❌ Не удалось найти точку спавна. Проверьте groundTilemap.");
             return;
        }


        // создаём игрока
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        
        // 3. ДОБАВЛЯЕМ ТЕГ "Player" (если префаб не имеет его)
        if (player.tag != "Player")
        {
             player.tag = "Player";
        }

        // подключаем статы
        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (upgradeUI != null)
        {
            upgradeUI.playerStats = stats;
            // Debug.Log("✔ PlayerStats connected to UpgradeUI");
        }
        else
        {
            Debug.LogError("❌ upgradeUI НЕ ПОДКЛЮЧЕН В PlayerSpawn!!!");
        }

        // камера
        if (cameraFollow != null)
            cameraFollow.SetTarget(player.transform);

        Debug.Log("✔ Player spawned at " + spawnPos);

        // NPC
        if (npcSpawner != null)
            npcSpawner.SpawnNPCNear(player);
    }

    // ==========================================================
    //             ПОЛУЧЕНИЕ ПОЗИЦИИ (ДЛЯ GAMEMANAGER)
    // ==========================================================
    /// <summary>
    /// Возвращает координаты точки спавна, используя логику поиска тега или тайлмапа.
    /// Используется GameStateManager.
    /// </summary>
    public Vector3 GetPlayerSpawnPosition()
    {
        if (!playerPrefab) return Vector3.zero;
        if (!groundTilemap) return Vector3.zero;

        BoundsInt bounds = groundTilemap.cellBounds;

        GameObject spawnObj = GameObject.FindWithTag(spawnPointTag);
        Vector3 spawnPos;

        if (spawnObj != null)
        {
            spawnPos = spawnObj.transform.position;
        }
        else
        {
            // Debug.LogWarning("⚠️ PlayerSpawnPoint не найден, ищу позицию по тайлмапу");
            spawnPos = FindSpawnPoint(bounds);
        }
        
        return spawnPos;
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