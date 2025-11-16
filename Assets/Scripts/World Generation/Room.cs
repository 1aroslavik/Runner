using UnityEngine;

public class Room : MonoBehaviour
{
    public bool exitLeft;
    public bool exitRight;
    public bool exitUp;
    public bool exitDown;

    public Transform playerSpawnPoint;
    public Transform[] enemySpawnPoints;
    public Transform[] chestSpawnPoints;
}
