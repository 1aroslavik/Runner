using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SavedMapList", menuName = "WFC/Saved Map List")]
public class SavedMapList : ScriptableObject
{
    public List<SavedMap> maps = new List<SavedMap>();
}
