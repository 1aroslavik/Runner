using UnityEngine;
using System.Collections.Generic;

public class LevelValidator
{
    public bool Validate(int seed)
    {
        Random.InitState(seed);

        // 1. Генерируем структуру уровня (только логику, не объекты)
        var layout = GenerateTestLayout(seed);

        if (!layout.success)
            return false;

        // 2. Проверяем путь Start -> End
        if (!HasPath(layout))
            return false;

        // 3. Нет перекрытий?
        if (HasOverlaps(layout))
            return false;

        // 4. Нет ли вертикальных скачков
        if (HasImpossibleVerticals(layout))
            return false;

        // 5. Нет ли горизонтальных прыжков слишком длинных
        if (HasImpossibleGaps(layout))
            return false;

        return true;
    }

    // -------------------------
    // Генерация упрощённого layout
    // (ИМИТАЦИЯ работы LevelGenerator,
    //  но без спавна комнат)
    // -------------------------
    private Layout GenerateTestLayout(int seed)
    {
        Layout layout = new Layout();
        layout.rooms = new Dictionary<Vector2Int, RoomStub>();

        Vector2Int pos = Vector2Int.zero;
        layout.start = pos;
        layout.rooms[pos] = new RoomStub(pos);

        int rooms = Random.Range(7, 15);

        for (int i = 0; i < rooms; i++)
        {
            AnchorType dir = ChooseDirection();

            Vector2Int next = pos + DirectionToOffset(dir);

            // вертикальные ограничения
            if (next.y > 3 || next.y < -3)
                dir = AnchorType.Right;

            next = pos + DirectionToOffset(dir);

            if (layout.rooms.ContainsKey(next))
                return layout.Fail();

            layout.rooms[next] = new RoomStub(next);

            pos = next;
        }

        layout.end = pos;

        return layout;
    }

    private AnchorType ChooseDirection()
    {
        float r = Random.value;

        if (r < 0.7f) return AnchorType.Right;
        if (r < 0.85f) return AnchorType.Top;
        return AnchorType.Bottom;
    }

    private Vector2Int DirectionToOffset(AnchorType t)
    {
        switch (t)
        {
            case AnchorType.Right: return new Vector2Int(1, 0);
            case AnchorType.Left: return new Vector2Int(-1, 0);
            case AnchorType.Top: return new Vector2Int(0, 1);
            case AnchorType.Bottom: return new Vector2Int(0, -1);
        }
        return Vector2Int.zero;
    }

    // -------------------------
    // Проверки
    // -------------------------
    private bool HasPath(Layout layout)
    {
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        q.Enqueue(layout.start);

        while (q.Count > 0)
        {
            var c = q.Dequeue();
            if (c == layout.end)
                return true;

            visited.Add(c);

            foreach (var d in DirectionList())
            {
                var n = c + DirectionToOffset(d);
                if (layout.rooms.ContainsKey(n) && !visited.Contains(n))
                    q.Enqueue(n);
            }
        }

        return false;
    }

    private IEnumerable<AnchorType> DirectionList()
    {
        yield return AnchorType.Right;
        yield return AnchorType.Left;
        yield return AnchorType.Top;
        yield return AnchorType.Bottom;
    }

    private bool HasOverlaps(Layout layout)
    {
        // если количество ключей ≠ количеству комнат → был дубликат
        return layout.rooms.Count != new HashSet<Vector2Int>(layout.rooms.Keys).Count;
    }

    private bool HasImpossibleVerticals(Layout layout)
    {
        foreach (var r in layout.rooms.Keys)
        {
            foreach (var d in new[] { AnchorType.Top, AnchorType.Bottom })
            {
                Vector2Int n = r + DirectionToOffset(d);
                if (layout.rooms.ContainsKey(n))
                {
                    // если подъем > 1 клетки — слишком круто
                    if (Mathf.Abs(n.y - r.y) > 1)
                        return true;
                }
            }
        }
        return false;
    }

    private bool HasImpossibleGaps(Layout layout)
    {
        foreach (var r in layout.rooms.Keys)
        {
            Vector2Int right = r + Vector2Int.right;

            if (layout.rooms.ContainsKey(right))
            {
                // Гэп нормальный — 1 клетка
                continue;
            }

            // Если нет комнаты справа, но есть через одну — это плохой прыжок
            Vector2Int far = r + new Vector2Int(2, 0);
            if (layout.rooms.ContainsKey(far))
                return true;
        }
        return false;
    }

    // -------------------------
    // Вспомогательные классы
    // -------------------------

    public class Layout
    {
        public Dictionary<Vector2Int, RoomStub> rooms;
        public Vector2Int start;
        public Vector2Int end;
        public bool success = true;

        public Layout Fail()
        {
            success = false;
            return this;
        }
    }

    public class RoomStub
    {
        public Vector2Int pos;
        public RoomStub(Vector2Int p)
        {
            pos = p;
        }
    }
}
