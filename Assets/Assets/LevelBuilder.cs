using UnityEngine;
using WFC;

public class LevelBuilder : MonoBehaviour
{
    public WFCTilemapGenerator wfc;
    public PlayerSpawn spawner;

    private void Start()
    {
        // подписываемся на событие завершения генерации
        wfc.OnGenerationComplete = () =>
        {
            spawner.SpawnPlayer();
        };
    }

    public void BuildLevel()
    {
        Debug.Log("🔥 Building Level...");
        wfc.Generate();
    }
}
