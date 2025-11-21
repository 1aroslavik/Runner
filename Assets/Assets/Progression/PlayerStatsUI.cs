using TMPro;
using UnityEngine;

public class PlayerStatsUI : MonoBehaviour
{
    public TMP_Text hpText;
    public TMP_Text damageText;
    public TMP_Text speedText;
    public TMP_Text defenceText;

    public void UpdateUI(PlayerStats stats)
    {
        hpText.text = $"HP: {stats.currentHealth}/{stats.maxHealth}";
        damageText.text = $"Меч: {stats.meleeDamage} / Лук: {stats.arrowDamage}";
        speedText.text = $"Скорость: {stats.moveSpeed}";
        defenceText.text = $"Броня: {stats.defence}";
    }
}
