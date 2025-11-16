using UnityEngine;

[CreateAssetMenu(menuName = "Level/Room Type")]
public class RoomType : ScriptableObject
{
    public string typeName;

    [Tooltip(" омнаты, которые могут идти по этому типу.")]
    public Room[] rooms;

    [Range(0, 1)]
    public float rarity = 1f;
}
