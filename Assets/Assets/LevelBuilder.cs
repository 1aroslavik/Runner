using UnityEngine;
using WFC;

public class LevelBuilder : MonoBehaviour
{
    public WFCTilemapGenerator wfc;
    public PlayerSpawn spawner;

    public Transform enemySpawnParent;
    public GameObject enemyPrefab;
    public EnemySpawner enemySpawner;

    private void Start()
    {
        // Когда WFC закончит генерацию — вызываем ВСЁ остальное
        wfc.OnGenerationComplete = () =>
        {
            Debug.Log("🎉 WFC generation complete!");

            // 1. Спавним игрока
            spawner.SpawnPlayer();

            if (enemySpawner != null)
                enemySpawner.SpawnEnemies();
        };
    }

    void SpawnEnemies()
    {
        if (enemyPrefab == null || enemySpawnParent == null)
        {
            Debug.LogWarning("❗ Enemy prefab or spawn parent is missing!");
            return;
        }

        foreach (Transform point in enemySpawnParent)
        {
            Vector3 pos = point.position;

            // Ставим врага НА землю с +1 по Y
            Vector3 spawnPos = new Vector3(pos.x, pos.y + 1f, pos.z);

            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }

        Debug.Log("✔ Enemies spawned!");
    }

    public void BuildLevel()
    {
        Debug.Log("🔥 Building Level...");
        wfc.Generate();   // WFC начнёт генерацию
        // ВСЁ остальное произойдёт только POСLE OnGenerationComplete()
    }
}
