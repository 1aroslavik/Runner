using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Основное")]
    public GameObject enemyPrefab;
    public Tilemap tilemap;
    public int enemiesToSpawn = 15;

    [Header("Не спавнить около комнат")]
    public Vector3 startRoomPos;
    public Vector3 endRoomPos;
    public float safeRadius = 12f;

    [Header("Типы врагов")]
    public EnemyType[] enemyTypes; // ← вот они

    public void SpawnEnemiesAlongTunnel(List<Vector3Int> tunnelPath)
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
        if (tunnelPath == null || tunnelPath.Count == 0)
        {
            Debug.LogError("❌ EnemySpawner: Путь туннеля пуст. Невозможно спавнить врагов!");
            return;
        }

        int spawned = 0;
        int safety = 0;

        while (spawned < enemiesToSpawn && safety < 5000)
        {
            safety++;

            // случайная точка туннеля
            Vector3Int cell = tunnelPath[Random.Range(0, tunnelPath.Count)];
            
            // NOTE: Мы используем Vector3(0.5f, 1.2f, 0f) для позиционирования
            Vector3 worldPos = tilemap.CellToWorld(cell) + new Vector3(0.5f, 1.2f, 0f);

            // избегаем старт/энда
            if (Vector3.Distance(worldPos, startRoomPos) < safeRadius) continue;
            if (Vector3.Distance(worldPos, endRoomPos) < safeRadius) continue;

            // создаём врага
            GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);
            enemyObj.name = $"Enemy_{spawned}"; // <--- ДЛЯ ОТЛАДКИ: даем имя

            // Добавляем тег, если его нет
            if (enemyObj.tag != "Enemy")
            {
                 enemyObj.tag = "Enemy";
            }
            
            // выбираем случайный EnemyType
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.type = enemyTypes[Random.Range(0, enemyTypes.Length)];
                enemy.ApplyType();
            }

            spawned++;
        }

        Debug.Log($"👹 Спавнер врагов: создано {spawned} врагов.");
    }
}