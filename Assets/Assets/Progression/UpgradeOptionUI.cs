using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeOptionUI : MonoBehaviour
{
    public Image icon;
    public TMP_Text title;
    public TMP_Text description;

    private UpgradeData upgrade;
    private UpgradeUI ui;

    public void SetOption(UpgradeData data, UpgradeUI parentUI)
    {
        upgrade = data;
        ui = parentUI;

        icon.sprite = data.icon;
        title.text = data.upgradeName;
        description.text = data.description;
    }

    public void OnClick()
    {
        ui.ChooseUpgrade(upgrade);
    }
}
