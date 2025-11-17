using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Ссылки")]
    public Tilemap groundTilemap;              // tilemap WFC Output
    public GameObject enemyPrefab;             // враг
    public EnemyType[] enemyTypes;             // <--- разные типы врагов

    [Header("Параметры спавна")]
    public int enemiesToSpawn = 10;            // сколько врагов генерировать
    public int minDistanceBetween = 5;         // чтобы они не были в одной клетке

    public void SpawnEnemies()
    {
        if (groundTilemap == null || enemyPrefab == null)
        {
            Debug.LogWarning("⚠ Нет Tilemap или EnemyPrefab");
            return;
        }

        if (enemyTypes == null || enemyTypes.Length == 0)
        {
            Debug.LogError("❌ enemyTypes пустой — назначи ScriptableObject!");
            return;
        }

        Debug.Log("👹 Генерируем врагов...");

        BoundsInt bounds = groundTilemap.cellBounds;
        int spawned = 0;

        int safety = 0;

        while (spawned < enemiesToSpawn && safety < 5000)
        {
            safety++;

            // случайная точка на карте
            int x = Random.Range(bounds.xMin, bounds.xMax);
            int y = Random.Range(bounds.yMin, bounds.yMax);
            Vector3Int cell = new Vector3Int(x, y, 0);

            // проверяем, есть ли земля
            if (!groundTilemap.HasTile(cell))
                continue;

            // проверяем, что над тайлом есть воздух (место для врага)
            if (groundTilemap.HasTile(cell + Vector3Int.up))
                continue;

            // финальная позиция врага
            Vector3 spawnPos = groundTilemap.CellToWorld(cell) + new Vector3(0.5f, 1f, 0f);

            // создаём врага
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

            // НАЗНАЧАЕМ тип врага
            Enemy e = enemy.GetComponent<Enemy>();
            e.type = enemyTypes[Random.Range(0, enemyTypes.Length)];
            e.ApplyType(); // вручную применяем параметры

            spawned++;
        }

        Debug.Log($"✔ Спавн врагов завершён: {spawned}");
    }
}
