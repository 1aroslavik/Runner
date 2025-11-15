using UnityEngine;

public enum AnchorType
{
    Left,
    Right,
    Top,
    Bottom
}

public class Anchor : MonoBehaviour
{
    public AnchorType type;
}
