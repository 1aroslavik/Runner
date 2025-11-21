using UnityEngine;

public class Chest : MonoBehaviour
{
    private Animator anim;
    private bool opened = false;

    private UpgradeManager upgradeManager;

    void Start()
    {
        anim = GetComponent<Animator>();

        upgradeManager = FindObjectOfType<UpgradeManager>();
        if (upgradeManager == null)
            Debug.LogError("❌ Chest: UpgradeManager не найден!");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (opened) return;
        if (!col.CompareTag("Player")) return;

        PlayerStats stats = col.GetComponent<PlayerStats>();
        if (stats == null) return;

        opened = true;

        if (anim != null)
            anim.SetTrigger("Open");

        upgradeManager.TriggerUpgrade(stats);
    }

    public void CleanUp()
    {
        Destroy(gameObject);
    }
}
