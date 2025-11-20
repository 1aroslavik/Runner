using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Roguelike/Upgrade")]
public class UpgradeData : ScriptableObject
{
    public string upgradeName;
    public string description;

    public Sprite icon;

    public UpgradeType type;
    public float value;
}

public enum UpgradeType
{
    MaxHP,
    Damage,
    MoveSpeed,
    AttackSpeed,
    CritChance,
    Defence
}
