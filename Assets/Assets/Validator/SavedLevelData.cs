using System;
using System.Collections.Generic;

[Serializable]
public class SavedLevelData
{
    public int width;
    public int height;

    // тайлы в координатах от (0..width-1, 0..height-1)
    public List<TileRecord> tiles = new List<TileRecord>();
}

[Serializable]
public class TileRecord
{
    public int x;
    public int y;
    public string tileName;
}
