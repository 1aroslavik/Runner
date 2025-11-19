using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public static class MapSaveSystem
{
    public static void SaveMap(SavedMapList list, TunnelGenerator tunnel, int seed)
    {
        SavedMap map = ScriptableObject.CreateInstance<SavedMap>();

        map.seed = seed;
        map.mainTunnelPath = new List<Vector3Int>(tunnel.mainTunnelPath);
        map.startRoomPos = tunnel.startRoomWorldPos;
        map.endRoomPos = tunnel.endRoomWorldPos;
        map.mapName = "Map_" + seed;

        list.maps.Add(map);

        AssetDatabase.CreateAsset(map, "Assets/SavedMaps/" + map.mapName + ".asset");
        AssetDatabase.SaveAssets();

        Debug.Log("✔ Карта сохранена: " + map.mapName);
    }

    public static SavedMap LoadMap(SavedMapList list, int index)
    {
        if (index < 0 || index >= list.maps.Count)
        {
            Debug.LogError("❌ Индекс карты вне диапазона!");
            return null;
        }

        return list.maps[index];
    }
}
