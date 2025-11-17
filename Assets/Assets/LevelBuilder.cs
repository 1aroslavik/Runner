using UnityEngine;
using WFC;

public class LevelBuilder : MonoBehaviour
{
    public WFCTilemapGenerator wfc;
    public PlayerSpawn spawner;
    public EnemySpawner enemySpawner;

    public int maxAttempts = 10;

    private void Start()
    {
        // 1) Загружаем список сохранённых сидов
        var seeds = SeedStorage.LoadAllSeeds();
        LoadingScreenController.Instance.Show();

        if (seeds.Count > 0)
        {
            int seed = seeds[Random.Range(0, seeds.Count)];
            Debug.Log("📥 Loading saved seed: " + seed);

            wfc.SetSeed(seed);
            wfc.Generate();

            wfc.OnGenerationComplete = () =>
            {
                spawner.SpawnPlayer();
                LoadingScreenController.Instance.Hide();
                enemySpawner?.SpawnEnemies();
            };

            return;
        }

        // Если нет ни одного сида — генерируем новый
        Debug.Log("🆕 No saved seeds. Generating new world...");
        GenerateNewWorld();
    }
    public void BuildLevel()
    {
        // например:
        wfc.RandomSeed();
        wfc.Generate();
    }

    void GenerateNewWorld()
    {
        int attempts = 0;

        wfc.OnGenerationComplete = () =>
        {
            var validator = GetComponent<LevelValidator>();

            if (validator != null && !validator.Validate())
            {
                attempts++;
                if (attempts >= maxAttempts)
                {
                    Debug.LogError("❌ Failed to create valid world");
                    return;
                }

                // генерируем новый seed
                wfc.RandomSeed();
                wfc.Generate();
                return;
            }

            // Если карта валидная → сохраняем seed
            SeedStorage.SaveSeed(wfc.Seed);

            spawner.SpawnPlayer();
            LoadingScreenController.Instance.Hide();

            enemySpawner?.SpawnEnemies();
        };

        // первый запуск генерации
        wfc.RandomSeed();
        wfc.Generate();
    }
}
