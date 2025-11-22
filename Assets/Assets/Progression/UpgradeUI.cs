using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public GameObject root;

    public UpgradeOptionUI option1;
    public UpgradeOptionUI option2;
    public UpgradeOptionUI option3;

    public PlayerStats playerStats;
    public PlayerStatsUI statsUI;

    void Start()
    {
        if (statsUI == null)
            statsUI = FindObjectOfType<PlayerStatsUI>();

        Hide();
    }

    public void Show(UpgradeData[] upgrades, PlayerStats stats)
    {
        playerStats = stats;

        Time.timeScale = 0f;
        root.SetActive(true);

        option1.SetOption(upgrades[0], this);
        option2.SetOption(upgrades[1], this);
        option3.SetOption(upgrades[2], this);
    }

    public void ChooseUpgrade(UpgradeData u)
    {
        ApplyUpgrade(u);

        // ⬇ Сохраняем прокачанные статы
        PermanentStats.Instance.SaveFrom(playerStats);

        // ⬇ Обновляем UI
        if (statsUI != null)
            statsUI.UpdateUI(playerStats);

        Hide();
    }

    private void ApplyUpgrade(UpgradeData u)
    {
        switch (u.type)
        {
            case UpgradeType.MaxHP:
                playerStats.AddHP((int)u.value);
                break;

            case UpgradeType.MeleeDamage:
                playerStats.AddMeleeDamage(u.value);
                break;

            case UpgradeType.ArrowDamage:
                playerStats.AddArrowDamage(u.value);
                break;

            case UpgradeType.MoveSpeed:
                playerStats.AddMoveSpeed(u.value);
                break;

            case UpgradeType.SprintSpeed:
                playerStats.AddSprintSpeed(u.value);
                break;

            case UpgradeType.JumpForce:
                playerStats.AddJumpForce(u.value);
                break;

            case UpgradeType.Defence:
                playerStats.AddDefence(u.value);
                break;
        }
    }

    public void Hide()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
    }
}
