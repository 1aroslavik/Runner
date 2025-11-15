using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Выходы комнаты (где есть проходы)")]
    public bool exitLeft;
    public bool exitRight;
    public bool exitUp;
    public bool exitDown;

    [Header("Точки спавна игрока / врагов / сундуков")]
    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints;
    public Transform[] chestSpawnPoints;

    [Header("Размер комнаты (если нужно для визуализации)")]
    public Vector2Int size = new Vector2Int(20, 12);

    // Опционально: можно добавить имя комнаты
    public string roomName;
}
