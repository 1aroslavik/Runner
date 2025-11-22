using UnityEngine;

public class LoadingScreenUI : MonoBehaviour
{
    public static LoadingScreenUI Instance;

    [Header("Корневой объект UI (панель загрузки)")]
    public GameObject root;

    void Awake()
    {
        Instance = this;
        Hide(); // чтобы при старте не видно
    }

    public void Show()
    {
        if (root != null)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}
