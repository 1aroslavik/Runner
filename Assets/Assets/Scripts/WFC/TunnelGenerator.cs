using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TunnelGenerator : MonoBehaviour
{

    public List<Vector3Int> mainTunnelPath = new List<Vector3Int>();
    public Vector3 startRoomWorldPos;
    public Vector3 endRoomWorldPos;

    [Header("Rooms")]
    public GameObject StartRoomPrefab;
    public GameObject EndRoomPrefab;
    public float StartRoomOffset = 3f;
    public float EndRoomOffset = 3f;

    [Header("Decorations")]
    public GameObject[] decorations;
    public float decorChance = 0.12f;

    // внутренние ссылки
    [HideInInspector] public Tilemap map;
    [HideInInspector] public BoundsInt bounds;
    private int seed;
    private PlayerSpawn playerSpawn;

    private int startRoomBottomY = -99999;

    // ================================================================
    //   ПУБЛИЧНЫЙ ИНТЕРФЕЙС — вызывает WFCTilemapGenerator
    // ================================================================
    public void GenerateTunnel(Tilemap tilemap, int seed, BoundsInt bounds, PlayerSpawn playerSpawn)
    {
        this.map = tilemap;
        this.seed = seed;
        this.bounds = bounds;
        this.playerSpawn = playerSpawn;

        System.Random rnd = new System.Random(seed);

        // ---------------- 1. СТАРТОВАЯ КОМНАТА ----------------
        Vector3Int startCell = new Vector3Int(
            bounds.xMin + 5,
            Mathf.FloorToInt(bounds.center.y),
            0
        );
        PlaceStartRoom(startCell);

        // ---------------- 2. ПЕРЛИН ПУТЬ ----------------
        List<Vector3Int> path = BuildMainTunnelPerlin(
            new Vector3Int(bounds.xMin, startRoomBottomY, 0)
        );
        mainTunnelPath = new List<Vector3Int>(path);

        // ---------------- 3. Ступени ----------------
        ApplyHeightSteps(path, rnd);

        // ---------------- 4. Вертикальные шахты ----------------
        AddVerticalShafts(path, rnd);

        // ---------------- 5. Основной туннель ----------------
        CarveMainTunnel(path);

        // ---------------- 6. Боковые ветви ----------------
        for (int i = 15; i < path.Count - 20;)
        {
            if (rnd.Next(0, 100) < 45)
            {
                var br = GrowSideBranch(path[i], rnd);
                CarveBranchWithChambers(br, rnd);
            }

            if (rnd.Next(0, 100) < 30)
            {
                CarveMiniChamber(path[i], rnd);
            }

            i += rnd.Next(18, 28);
        }

        // ---------------- 7. Декорации ----------------
        PlaceDecorations(path, rnd);

        // ---------------- 8. END ROOM ----------------
        Vector3Int end = path[path.Count - 1];
        CutRect(end.x - 2, end.y - 2, end.x + 2, end.y + 2);
        PlaceEndRoom(end);

        // ---------------- 9. Spawn Player ----------------
        if (playerSpawn != null)
            playerSpawn.SpawnPlayer();
    }

    // ================================================================
    //   ВСЕ ФУНКЦИИ ТОННЕЛЕЙ (подняты из твоего большого класса)
    // ================================================================

    void PlaceStartRoom(Vector3Int tunnelCell)
    {
        if (StartRoomPrefab == null) return;

        Vector3 tunnelWorld = map.CellToWorld(tunnelCell);
        Tilemap roomMap = StartRoomPrefab.GetComponentInChildren<Tilemap>();
        BoundsInt rb = roomMap.cellBounds;

        float cell = roomMap.cellSize.x;
        float rightEdge = rb.xMax * cell;

        Vector3 spawnPos = tunnelWorld;
        spawnPos.x -= Mathf.Abs(rightEdge) + StartRoomOffset;

        GameObject inst = Instantiate(StartRoomPrefab, spawnPos, Quaternion.identity, transform);

        Transform spawnPoint = inst.transform.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            Vector3 wp = spawnPoint.position;
            Vector3Int c = map.WorldToCell(wp);
            startRoomBottomY = c.y - 1;
        }
        startRoomWorldPos = inst.transform.position;

    }

    void PlaceEndRoom(Vector3Int tunnelCell)
    {
        if (EndRoomPrefab == null) return;

        Vector3 tunnelWorld = map.CellToWorld(tunnelCell);
        GameObject inst = Instantiate(EndRoomPrefab, tunnelWorld, Quaternion.identity, transform);
        endRoomWorldPos = inst.transform.position;

    }

    // ---------------- ВЫРЕЗАТЕЛИ ----------------
    public void CutRect(int x1, int y1, int x2, int y2)
    {
        if (x2 < x1) (x1, x2) = (x2, x1);
        if (y2 < y1) (y1, y2) = (y2, y1);

        for (int x = x1; x <= x2; x++)
            for (int y = y1; y <= y2; y++)
                map.SetTile(new Vector3Int(x, y, 0), null);
    }

    void CutBlob(Vector3Int center, int rx, int ry)
    {
        for (int dx = -rx; dx <= rx; dx++)
            for (int dy = -ry; dy <= ry; dy++)
                if ((dx * dx) / (float)(rx * rx) + (dy * dy) / (float)(ry * ry) <= 1f)
                    map.SetTile(center + new Vector3Int(dx, dy, 0), null);
    }

    // ---------------- ТОННЕЛЬ ----------------
    List<Vector3Int> BuildMainTunnelPerlin(Vector3Int start, int width = 0)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        float scale = 0.015f;
        float amp = 12f;
        float offset = seed % 9999;

        for (int x = bounds.xMin; x <= bounds.xMax; x++)
        {
            float nx = x * scale + offset;
            float noise = Mathf.PerlinNoise(nx, 0f);
            int y = start.y + Mathf.RoundToInt((noise - 0.5f) * amp);
            y = Mathf.Clamp(y, bounds.yMin + 8, bounds.yMax - 8);

            path.Add(new Vector3Int(x, y, 0));
        }

        return path;
    }

    void ApplyHeightSteps(List<Vector3Int> path, System.Random rnd)
    {
        for (int i = 10; i < path.Count - 10; i++)
        {
            int r = rnd.Next(0, 100);

            if (r < 5) path[i] += new Vector3Int(0, +rnd.Next(1, 3), 0);
            else if (r < 10) path[i] += new Vector3Int(0, -rnd.Next(1, 3), 0);
        }
    }

    void AddVerticalShafts(List<Vector3Int> path, System.Random rnd)
    {
        for (int i = 20; i < path.Count - 20; i += rnd.Next(25, 40))
        {
            if (rnd.Next(0, 100) > 45)
                continue;

            Vector3Int p = path[i];

            int height = rnd.Next(6, 18);
            int dir = rnd.Next(0, 2) == 0 ? -1 : +1;

            for (int dy = 0; dy < height; dy++)
            {
                CutRect(p.x - 2, p.y + dy * dir - 2, p.x + 2, p.y + dy * dir + 2);
            }
        }
    }

    void CarveMainTunnel(List<Vector3Int> path)
    {
        foreach (var p in path)
            CutRect(p.x - 3, p.y - 2, p.x + 3, p.y + 2);
    }

    List<Vector3Int> GrowSideBranch(Vector3Int from, System.Random rnd)
    {
        List<Vector3Int> b = new List<Vector3Int>();

        int length = rnd.Next(10, 25);
        int dirX = rnd.Next(0, 2) == 0 ? -1 : +1;
        int x = from.x;
        int y = from.y;

        for (int i = 0; i < length; i++)
        {
            x += dirX;

            int r = rnd.Next(0, 100);
            if (r < 30) y++;
            else if (r < 60) y--;

            b.Add(new Vector3Int(x, y, 0));
        }

        return b;
    }

    void CarveBranchWithChambers(List<Vector3Int> br, System.Random rnd)
    {
        foreach (var p in br)
        {
            CutBlob(p, 2, 1);

            if (rnd.Next(0, 100) < 25)
                CutBlob(p + new Vector3Int(rnd.Next(-2, 3), rnd.Next(-2, 3), 0), 3, 3);
        }
    }

    void CarveMiniChamber(Vector3Int p, System.Random rnd)
    {
        CutBlob(p, 2, 1);
        CutBlob(p + new Vector3Int(rnd.Next(-2, 3), rnd.Next(-2, 3), 0), 3, 2);
    }

    void PlaceDecorations(List<Vector3Int> path, System.Random rnd)
    {
        if (decorations == null || decorations.Length == 0)
            return;

        foreach (var p in path)
        {
            if (rnd.NextDouble() > decorChance)
                continue;

            Vector3 world = map.CellToWorld(p);
            Instantiate(decorations[rnd.Next(decorations.Length)], world, Quaternion.identity);
        }

    }
}
