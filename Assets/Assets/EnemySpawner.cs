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
        int lastIndex = path.Count - 1;

        for (int i = 0; i < path.Count; i += spawnStep)
        {
            Vector3 worldPos = tilemap.CellToWorld(path[i]);

            if (Vector3.Distance(worldPos, startRoomPos) < safeRadius) continue;
            if (Vector3.Distance(worldPos, endRoomPos) < safeRadius) continue;

            // создаём врага
            GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);

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
