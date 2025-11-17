using UnityEngine;

public class LoadingScreenController : MonoBehaviour
{
    public static LoadingScreenController Instance;

    public GameObject loadingUI;

    private void Awake()
    {
        Instance = this;
    }

    public void Show()
    {
        loadingUI.SetActive(true);
    }

    public void Hide()
    {
        loadingUI.SetActive(false);
    }
}
