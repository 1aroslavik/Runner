using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string path = Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
        Debug.Log("✔ Сохранение выполнено: " + path);
    }

    public static SaveData Load()
    {
        if (!File.Exists(path))
        {
            Debug.Log("⚠ Сейв не найден. Создаю новый.");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool SaveExists()
    {
        return File.Exists(path);
    }
}
