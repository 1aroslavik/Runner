using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Основное")]
    public GameObject enemyPrefab;
    public Tilemap tilemap;
    // Используем spawnStep для определения, как часто спавнить врагов (каждый N-й шаг пути)
    public int spawnStep = 70; // <-- Это поле, которое вы сохранили

    [Header("Не спавнить около комнат")]
    public Vector3 startRoomPos;
    public Vector3 endRoomPos;
    public float safeRadius = 12f;

    [Header("Типы врагов")]
    public EnemyType[] enemyTypes;

    // ИСПРАВЛЕНО: Теперь функция принимает 'path' (путь туннеля), а не 'tunnelPath'
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
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ EnemySpawner: Путь туннеля пуст. Невозможно спавнить врагов!");
            return;
        }

        int pathLength = path.Count; // <--- Используем длину пути для прогресса
        int enemiesSpawned = 0; // <--- Счетчик для лога

        // ИСПРАВЛЕНО: Заменяем цикл while на цикл for, чтобы использовать 'i' и длину пути.
        // Спавним врага каждые 'spawnStep' клеток пути.
        for (int i = 0; i < pathLength; i += spawnStep)
        {
            // 1. Получаем позицию
            Vector3Int cell = path[i]; // <--- ИСПОЛЬЗУЕМ 'path' и 'i'
            Vector3 worldPos = tilemap.CellToWorld(cell) + new Vector3(0.5f, 1.2f, 0f);

            // 2. Избегаем старт/энда
            if (Vector3.Distance(worldPos, startRoomPos) < safeRadius) continue;
            if (Vector3.Distance(worldPos, endRoomPos) < safeRadius) continue;

            // 3. Создаём врага
            GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

            // 4. Настраиваем врага
            Enemy enemy = enemyObj.GetComponent<Enemy>();

            if (enemy == null)
            {
                Debug.LogError("ПРЕДУПРЕЖДЕНИЕ: На префабе врага НЕТ компонента Enemy!");
                Destroy(enemyObj); // Удаляем врага, если он не настроен
                continue;
            }
            
            // 5. Выбираем и применяем тип врага
            EnemyType chosenType = enemyTypes[Random.Range(0, enemyTypes.Length)];
            enemy.ApplyType(chosenType);

            // 6. Прогресс для усиления врага (0.0 на старте, 1.0 в конце)
            float progress = (float)i / pathLength; // <--- ИСПОЛЬЗУЕМ 'i' и 'pathLength'

            // 7. Усиление врага
            enemy.ApplyDifficulty(progress);
            
            enemiesSpawned++; // Увеличиваем счетчик
        }

        Debug.Log($"👹 Спавнер врагов: создано {enemiesSpawned} врагов.");
    }
}