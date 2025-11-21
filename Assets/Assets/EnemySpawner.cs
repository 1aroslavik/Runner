using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Основное")]
    public GameObject enemyPrefab;
    public Tilemap tilemap;
    public int spawnStep = 70;

    [Header("Не спавнить около комнат")]
    public Vector3 startRoomPos;
    public Vector3 endRoomPos;
    public float safeRadius = 12f;

    [Header("Типы врагов")]
    public EnemyType[] enemyTypes;

    public void SpawnEnemiesAlongTunnel(List<Vector3Int> path)
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ EnemySpawner: enemyPrefab не назначен!");
            return;
        }
        if (tilemap == null)
        {
            Debug.LogError("❌ EnemySpawner: tilemap не назначен!");
            return;
        }
        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("❌ EnemySpawner: enemyTypes пустой! Назначи EnemyType");
            return;
        }

        int spawned = 0;
        int safety = 0;

        while (spawned < enemiesToSpawn && safety < 5000)
        {
            safety++;

            // случайная точка туннеля
            Vector3Int cell = tunnelPath[Random.Range(0, tunnelPath.Count)];
            Vector3 worldPos = tilemap.CellToWorld(cell) + new Vector3(0.5f, 1.2f, 0f);

            if (Vector3.Distance(worldPos, startRoomPos) < safeRadius) continue;
            if (Vector3.Distance(worldPos, endRoomPos) < safeRadius) continue;

            // создаём врага
            GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

            // выбираем случайный EnemyType
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError("ПРЕДУПРЕЖДЕНИЕ: На префабе врага НЕТ компонента Enemy!");
                continue;
            }

            // выбираем случайный тип врага
            EnemyType chosenType = enemyTypes[Random.Range(0, enemyTypes.Length)];

            // применяем тип
            enemy.ApplyType(chosenType);

            // прогресс 0..1
            float progress = (float)i / lastIndex;

            // усиление врага
            enemy.ApplyDifficulty(progress);
        }
    }
}