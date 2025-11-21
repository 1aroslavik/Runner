using System.Linq;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public UpgradeData[] allUpgrades;
    public UpgradeUI upgradeUI;

    public void ShowRandom(PlayerStats stats)
    {
        UpgradeData[] set =
            allUpgrades.OrderBy(a => Random.value).Take(3).ToArray();

        upgradeUI.Show(set, stats);
    }
}
