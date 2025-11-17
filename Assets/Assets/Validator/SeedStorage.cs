using System.IO;
using UnityEngine;
using System.Collections.Generic;

public static class SeedStorage
{
    private static string Path => Application.dataPath + "/StreamingAssets/Seeds/";

    public static void SaveSeed(int seed)
    {
        if (!Directory.Exists(Path))
            Directory.CreateDirectory(Path);

        string file = Path + "seed_" + System.DateTime.Now.Ticks + ".txt";
        File.WriteAllText(file, seed.ToString());

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log("💾 Saved seed: " + seed);
    }

    public static List<int> LoadAllSeeds()
    {
        List<int> seeds = new List<int>();

        if (!Directory.Exists(Path)) return seeds;

        string[] files = Directory.GetFiles(Path, "*.txt");

        foreach (var file in files)
        {
            string content = File.ReadAllText(file);
            if (int.TryParse(content, out int seed))
                seeds.Add(seed);
        }

        return seeds;
    }
}
