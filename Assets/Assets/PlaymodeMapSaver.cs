#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class PlaymodeMapSaver
{
    private static System.Action saveAction;

    static PlaymodeMapSaver()
    {
        EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
    }

    public static void RequestSave(System.Action action)
    {
        saveAction = action;
        Debug.Log("⏳ Сохранение карты отложено — выйдите из Play Mode.");
    }

    private static void OnPlaymodeStateChanged(PlayModeStateChange state)
    {
        // Когда игра остановилась
        if (state == PlayModeStateChange.EnteredEditMode && saveAction != null)
        {
            Debug.Log("💾 Выполняю отложенное сохранение карты...");
            saveAction.Invoke();
            saveAction = null;
        }
    }
}
#endif
