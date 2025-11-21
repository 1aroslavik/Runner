using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public GameObject root;
    public UpgradeOptionUI option1;
    public UpgradeOptionUI option2;
    public UpgradeOptionUI option3;

    public PlayerStats playerStats;

    public void Show(UpgradeData[] options, PlayerStats stats)
    {
        playerStats = stats;

        Time.timeScale = 0f;
        root.SetActive(true);

        option1.SetOption(options[0], this);
        option2.SetOption(options[1], this);
        option3.SetOption(options[2], this);
    }

    public void ChooseUpgrade(UpgradeData u)
    {
        ApplyUpgrade(u);

        root.SetActive(false);
        Time.timeScale = 1f;
    }

    private void ApplyUpgrade(UpgradeData u)
    {
        switch (u.type)
        {
            case UpgradeType.MaxHP: playerStats.AddMaxHP(u.value); break;
            case UpgradeType.MeleeDamage: playerStats.AddMeleeDamage(u.value); break;
            case UpgradeType.ArrowDamage: playerStats.AddArrowDamage(u.value); break;
            case UpgradeType.MoveSpeed: playerStats.AddMoveSpeed(u.value); break;
            case UpgradeType.SprintSpeed: playerStats.sprintSpeed += u.value; break;
            case UpgradeType.AttackSpeed: playerStats.AddAttackSpeed(u.value); break;
            case UpgradeType.Defence: playerStats.AddDefence(u.value); break;
            case UpgradeType.JumpForce: FindObjectOfType<PlayerMovement2D>().jumpForce += u.value; break;
            case UpgradeType.Regeneration: /* добавим позже */ break;
            case UpgradeType.CritChance: /* можно добавить */ break;
        }
    }
}
