using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelValidator : MonoBehaviour
{
    [Header("Основное")]
    public Tilemap tilemap;

    [Header("Порог количества тайлов")]
    public int minGroundTiles = 200;      // минимум тайлов вообще

    [Header("Связная область")]
    public int minConnectedTiles = 300;   // минимум тайлов в одной связной области (отсекает 1 кубик)

    [Header("Горизонтальные дыры")]
    public int maxGapSize = 8;            // макс. длина пустоты по X

    [Header("Проходимый путь")]
    public int minWalkPath = 40;          // минимум клеток, достижимых BFS

    public bool Validate()
    {
        if (tilemap == null)
        {
            Debug.LogError("❌ LevelValidator: tilemap is not assigned!");
            return false;
        }

        if (!HasEnoughGround())
        {
            Debug.Log("❌ Validator: not enough ground tiles");
            return false;
        }

        if (!HasLargeConnectedArea())
        {
            Debug.Log("❌ Validator: connected area too small");
            return false;
        }

        if (HasLargeGaps())
        {
            Debug.Log("❌ Validator: too large horizontal gaps");
            return false;
        }

        if (!HasLongWalkablePath())
        {
            Debug.Log("❌ Validator: walkable path too short");
            return false;
        }

        Debug.Log("✔ Level is VALID!");
        return true;
    }

    bool HasEnoughGround()
    {
        int count = 0;
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                count++;
        }

        return count >= minGroundTiles;
    }

    // 🔥 Главное: чтобы не было карты из 1 кубика
    bool HasLargeConnectedArea()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        Vector3Int? start = FindFirstGround();
        if (start == null) return false;

        queue.Enqueue(start.Value);
        visited.Add(start.Value);

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();

            foreach (var d in new Vector3Int[]
            {
                Vector3Int.right, Vector3Int.left,
                Vector3Int.up, Vector3Int.down
            })
            {
                var n = p + d;

                if (!tilemap.HasTile(n) || visited.Contains(n))
                    continue;

                visited.Add(n);
                queue.Enqueue(n);
            }
        }

        // если связная область меньше порога — карта плохая
        return visited.Count >= minConnectedTiles;
    }

    bool HasLargeGaps()
    {
        int gap = 0;
        var bounds = tilemap.cellBounds;

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            bool any = false;

            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                if (tilemap.HasTile(new Vector3Int(x, y, 0)))
                {
                    any = true;
                    break;
                }
            }

            if (!any)
            {
                gap++;
                if (gap > maxGapSize)
                    return true;
            }
            else
            {
                gap = 0;
            }
        }

        return false;
    }

    bool HasLongWalkablePath()
    {
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();
        Queue<Vector3Int> queue = new Queue<Vector3Int>();

        Vector3Int? start = FindFirstGround();
        if (start == null) return false;

        queue.Enqueue(start.Value);
        visited.Add(start.Value);

        int steps = 0;

        while (queue.Count > 0)
        {
            var p = queue.Dequeue();
            steps++;

            foreach (var d in new Vector3Int[]
            {
                Vector3Int.right, Vector3Int.left,
                Vector3Int.up, Vector3Int.down
            })
            {
                var n = p + d;

                if (!tilemap.HasTile(n) || visited.Contains(n))
                    continue;

                visited.Add(n);
                queue.Enqueue(n);
            }
        }

        return steps >= minWalkPath;
    }

    Vector3Int? FindFirstGround()
    {
        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
                return pos;
        }

        return null;
    }
}
