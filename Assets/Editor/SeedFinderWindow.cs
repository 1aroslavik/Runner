using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class SeedFinderWindow : EditorWindow
{
    private int seedsToTest = 500;
    private int maxGoodSeeds = 100;

    private List<int> goodSeeds = new List<int>();

    [MenuItem("Tools/Level Generator/Find Good Seeds")]
    public static void ShowWindow()
    {
        GetWindow<SeedFinderWindow>("Seed Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Seed Generator & Validator", EditorStyles.boldLabel);

        seedsToTest = EditorGUILayout.IntField("Seeds to test:", seedsToTest);
        maxGoodSeeds = EditorGUILayout.IntField("Max good seeds:", maxGoodSeeds);

        if (GUILayout.Button("Start Testing"))
        {
            goodSeeds.Clear();
            TestSeeds();
        }

        if (goodSeeds.Count > 0 && GUILayout.Button("Save to JSON"))
        {
            SaveSeedsToJson();
        }

        GUILayout.Space(10);
        GUILayout.Label("Good seeds found: " + goodSeeds.Count);
    }

    private void TestSeeds()
    {
        LevelValidator validator = new LevelValidator();

        for (int i = 0; i < seedsToTest; i++)
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);

            bool ok = validator.Validate(seed);

            if (ok)
            {
                goodSeeds.Add(seed);
                Debug.Log($"👍 Good seed found: {seed}");

                if (goodSeeds.Count >= maxGoodSeeds)
                {
                    Debug.Log("Reached desired amount of good seeds.");
                    break;
                }
            }
        }
    }

    private void SaveSeedsToJson()
    {
        string json = JsonUtility.ToJson(new SeedList(goodSeeds), true);
        File.WriteAllText(Application.dataPath + "/GoodSeeds.json", json);

        Debug.Log("Saved good seeds to Assets/GoodSeeds.json");
        AssetDatabase.Refresh();
    }

    [System.Serializable]
    private class SeedList
    {
        public List<int> seeds;

        public SeedList(List<int> list)
        {
            seeds = list;
        }
    }
}
