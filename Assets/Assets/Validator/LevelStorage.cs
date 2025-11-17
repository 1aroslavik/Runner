using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public static class LevelStorage
{
    // 📌 Сохраняем прямо в проект: Assets/StreamingAssets/Levels/
    private static string SavePath => Application.dataPath + "/StreamingAssets/Levels/";

    public static void SaveLevel(Tilemap map, BoundsInt bounds)
    {
        Debug.Log("💾 SaveLevel() STARTED");

        if (!Directory.Exists(SavePath))
            Directory.CreateDirectory(SavePath);

        SavedLevelData data = new SavedLevelData();

        // 🟢 Используем реальные границы Tilemap — ЭТО ГЛАВНЫЙ ФИКС
        BoundsInt real = map.cellBounds;

        data.width = real.size.x;
        data.height = real.size.y;

        int savedTiles = 0;

        foreach (var pos in real.allPositionsWithin)
        {
            TileBase tile = map.GetTile(pos);
            if (tile == null) continue;

            savedTiles++;

            data.tiles.Add(new TileRecord()
            {
                x = pos.x - real.min.x,
                y = pos.y - real.min.y,
                tileName = tile.name
            });
        }

        string json = JsonUtility.ToJson(data, true);
        string file = SavePath + "level_" + System.DateTime.Now.Ticks + ".json";

        File.WriteAllText(file, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log($"💾 SAVED LEVEL: {savedTiles} tiles at {file}");
    }


    public static string[] GetSavedLevels()
    {
        if (!Directory.Exists(SavePath))
            return new string[0];

        return Directory.GetFiles(SavePath, "*.json");
    }

    public static SavedLevelData LoadLevel(string path)
    {
        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SavedLevelData>(json);
    }

    public static void ApplyLevelToTilemap(SavedLevelData data, Tilemap map, BoundsInt bounds)
    {
        Debug.Log("📥 APPLY LEVEL:");
        Debug.Log("Bounds min=" + bounds.min + " max=" + bounds.max);
        Debug.Log("Tiles count=" + data.tiles.Count);

        map.ClearAllTiles();

        foreach (var record in data.tiles)
        {
            Debug.Log("Trying tile: " + record.tileName);

            TileBase tile = Resources.Load<TileBase>("Tiles/" + record.tileName);

            if (tile == null)
            {
                Debug.LogError("❌ NOT FOUND in Resources/Tiles/: " + record.tileName);
                continue;
            }

            Vector3Int pos = new Vector3Int(
                bounds.min.x + record.x,
                bounds.min.y + record.y,
                0
            );

            Debug.Log("✔ Set tile at " + pos + " = " + record.tileName);

            map.SetTile(pos, tile);
        }

        Debug.Log("🎯 Done applying tiles!");
    }

}
