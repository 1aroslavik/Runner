using UnityEngine;

public class UpgradeUI : MonoBehaviour
{
    public GameObject root;

    public UpgradeOptionUI option1;
    public UpgradeOptionUI option2;
    public UpgradeOptionUI option3;

    [HideInInspector]
    public PlayerStats playerStats;

    void Start()
    {
        Hide();
    }

    public void Show(UpgradeData[] upgrades)
    {
        Time.timeScale = 0f; // пауза игры
        root.SetActive(true);

        option1.SetOption(upgrades[0], this);
        option2.SetOption(upgrades[1], this);
        option3.SetOption(upgrades[2], this);
    }

    // 🔥 Метод выбора улучшения
    public void ChooseUpgrade(UpgradeData data)
    {
        // Лог в консоль
        Debug.Log($"[UPGRADE] Игрок выбрал: {data.upgradeName} — {data.description}");

        // Применяем апгрейд
        ApplyUpgrade(data);

        // Закрываем меню
        Hide();
    }

    void ApplyUpgrade(UpgradeData u)
    {
        switch (u.type)
        {
            case UpgradeType.MaxHP: playerStats.AddHP((int)u.value); break;
            case UpgradeType.Damage: playerStats.AddDamage(u.value); break;
            case UpgradeType.MoveSpeed: playerStats.AddMoveSpeed(u.value); break;
            case UpgradeType.AttackSpeed: playerStats.AddAttackSpeed(u.value); break;
            case UpgradeType.CritChance: playerStats.AddCritChance(u.value); break;
            case UpgradeType.Defence: playerStats.AddDefence(u.value); break;
        }
    }

    public void Hide()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
    }
}
