using UnityEngine;

[CreateAssetMenu(fileName = "RoomSet", menuName = "Generation/RoomSet")]
public class RoomSet : ScriptableObject
{
    public Room startRoom;
    public Room exitRoom;
    public Room[] horizontalRooms;
    public Room[] verticalRooms;
}
