using UnityEngine;

public class DeathScreenUI : MonoBehaviour
{
    public static DeathScreenUI Instance;

    [Header("Root object of death screen")]
    public GameObject root;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideDeathScreen();
    }

    public void ShowDeathScreen()
    {
        if (root != null)
        {
            Debug.Log("🔥 DeathScreen SHOWN");
            root.SetActive(true);
        }
        else Debug.LogError("❌ DeathScreenUI: root NOT ASSIGNED!");
    }

    public void HideDeathScreen()
    {
        if (root != null)
        {
            Debug.Log("🟢 DeathScreen HIDDEN");
            root.SetActive(false);
        }
    }
}
