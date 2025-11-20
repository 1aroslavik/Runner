using UnityEngine;
using System.Linq;

public class UpgradeManager : MonoBehaviour
{
    public UpgradeData[] allUpgrades;
    public UpgradeUI upgradeUI;

    public void TriggerUpgrade()
    {
        // берём 3 случайных
        UpgradeData[] chosen =
            allUpgrades.OrderBy(x => Random.value).Take(3).ToArray();

        upgradeUI.Show(chosen);
    }
}
