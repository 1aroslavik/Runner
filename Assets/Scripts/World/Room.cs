using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Выходы комнаты (направления)")]
    public bool up;
    public bool down;
    public bool left;
    public bool right;

    [Header("Вес появления (чем выше, тем чаще)")]
    public int weight = 1;
}
