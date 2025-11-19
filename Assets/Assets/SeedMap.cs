using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "SavedMap", menuName = "WFC/Saved Map")]
public class SavedMap : ScriptableObject
{
    public int seed;

    public List<Vector3Int> mainTunnelPath = new List<Vector3Int>();

    public Vector3 startRoomPos;
    public Vector3 endRoomPos;

    public string mapName;
}
