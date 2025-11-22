using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    public TMP_Text hpText;
    public TMP_Text damageText;
    public TMP_Text speedText;
    public TMP_Text defenceText;
    public PlayerStats stats;

    void Start()
    {
        Debug.Log("PlayerStatsUI STARTED");

        var player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            Debug.LogError("PlayerStatsUI: Player НЕ найден на сцене!");
            return;
        } else {
            stats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        }

        if (stats != null)
            UpdateUI(stats);
        else
            Debug.LogError("stats == NULL !");
    }
    public void SetStats(PlayerStats newStats)
    {
        stats = newStats;
        UpdateUI(stats);
    }


    void Update()
    {
        if (stats != null)
            UpdateUI(stats);
    }

    public void UpdateUI(PlayerStats stats)
    {
        hpText.text = $"HP: {stats.currentHealth}/{stats.maxHealth}";
        damageText.text = $"Melle dmg: {stats.meleeDamage} / Arrow: {stats.arrowDamage}";
        speedText.text = $"Speed: {stats.moveSpeed}";
        defenceText.text = $"Armor: {stats.defence}";
        Debug.Log($"[UI DEBUG] DmgText = '{damageText.text}'");

    }
}
