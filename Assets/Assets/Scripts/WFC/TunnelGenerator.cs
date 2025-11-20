using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TunnelGenerator : MonoBehaviour
{
    [Header("Loot / Chests")]
    public GameObject chestPrefab;
    public int chestDistance = 200;

    [Header("Rooms")]
    public GameObject StartRoomPrefab;
    public GameObject EndRoomPrefab;
    public float StartRoomOffset = 3f;
    public float EndRoomOffset = 3f;

    [Header("Decorations")]
    public GameObject[] decorations;
    public float decorChance = 0.12f;

    public List<Vector3Int> mainTunnelPath = new List<Vector3Int>();
    public Vector3 startRoomWorldPos;
    public Vector3 endRoomWorldPos;

    // ‚ÌÛÚÂÌÌËÂ ‰‡ÌÌ˚Â
    [HideInInspector] public Tilemap map;
    [HideInInspector] public BoundsInt bounds;

    private int seed;
    private PlayerSpawn playerSpawn;
    private int startRoomBottomY = -99999;

    // ================================================================
    //   œ”¡À»◊Õ€… «¿œ”—  »« WFC GENERATOR
    // ================================================================
    public void GenerateTunnel(Tilemap tilemap, int seed, BoundsInt bounds, PlayerSpawn playerSpawn)
    {
        this.map = tilemap;
        this.seed = seed;
        this.bounds = bounds;
        this.playerSpawn = playerSpawn;

        System.Random rnd = new System.Random(seed);

        // ---------------- START ROOM ----------------
        Vector3Int startCell = new Vector3Int(
            bounds.xMin + 5,
            Mathf.FloorToInt(bounds.center.y),
            0
        );
        PlaceStartRoom(startCell);

        // ---------------- MAIN PATH ----------------
        var path = BuildMainTunnel(startCell, rnd);
        mainTunnelPath = new List<Vector3Int>(path);

        // ---------------- ADD EXTRA SHAPING ----------------
        ApplyHeightSteps(path, rnd);
        AddVerticalShafts(path, rnd);
        AddLargeCaves(path, rnd);
        void CarveMainTunnel(List<Vector3Int> path)
        {
            foreach (var p in path)
            {
                // ÓÒÌÓ‚ÌÓÈ ÍÓË‰Ó 7 Ú‡ÈÎÓ‚ ¯ËËÌÓÈ (ÔÓ X) Ë 5 ÔÓ ‚˚ÒÓÚÂ (ÔÓ Y)
                CutRect(p.x - 3, p.y - 2, p.x + 3, p.y + 2);
            }
        }

        // ---------------- CARVE MAIN TUNNEL ----------------
        CarveMainTunnel(path);

        // ---------------- SIDE BRANCHES ----------------
        for (int i = 15; i < path.Count - 25;)
        {
            if (rnd.Next(0, 100) < 55)
            {
                var br = GrowSideBranch(path[i], rnd);
                CarveBranch(br, rnd);
            }

            if (rnd.Next(0, 100) < 35)
                CarveMiniChamber(path[i], rnd);

            i += rnd.Next(16, 30);
        }

        // ---------------- DECORATIONS + LOOT ----------------
        PlaceDecorations(path, rnd);
        PlaceChests(path);

        // ---------------- END ROOM ----------------
        Vector3Int end = path[path.Count - 1];
        CutRect(end.x - 2, end.y - 2, end.x + 2, end.y + 2);
        PlaceEndRoom(end);

        // ---------------- PLAYER SPAWN ----------------
        if (playerSpawn != null)
            playerSpawn.SpawnPlayer();
    }

    // ================================================================
    //   –¿«Ã≈Ÿ≈Õ»≈  ŒÃÕ¿“
    // ================================================================

    void PlaceStartRoom(Vector3Int tunnelCell)
    {
        if (StartRoomPrefab == null) return;

        Vector3 world = map.CellToWorld(tunnelCell);
        Tilemap roomMap = StartRoomPrefab.GetComponentInChildren<Tilemap>();
        BoundsInt rb = roomMap.cellBounds;

        float cell = roomMap.cellSize.x;
        float rightEdge = rb.xMax * cell;

        Vector3 pos = world;
        pos.x -= Mathf.Abs(rightEdge) + StartRoomOffset;

        GameObject inst = Instantiate(StartRoomPrefab, pos, Quaternion.identity, transform);

        Transform spawnPoint = inst.transform.Find("PlayerSpawnPoint");
        if (spawnPoint != null)
        {
            Vector3 w = spawnPoint.position;
            Vector3Int c = map.WorldToCell(w);
            startRoomBottomY = c.y - 1;
        }

        startRoomWorldPos = inst.transform.position;
    }

    void PlaceEndRoom(Vector3Int tunnelCell)
    {
        if (EndRoomPrefab == null) return;

        Vector3 world = map.CellToWorld(tunnelCell);
        GameObject inst = Instantiate(EndRoomPrefab, world, Quaternion.identity, transform);

        endRoomWorldPos = inst.transform.position;
    }

    // ================================================================
    //   "–≈«¿ »"
    // ================================================================

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

    // ================================================================
    //   Œ—ÕŒ¬ÕŒ… œ”“‹ (ÕŒ¬€… »«¬»À»—“€… ¿À√Œ–»“Ã)
    // ================================================================

    List<Vector3Int> BuildMainTunnel(Vector3Int start, System.Random rnd)
    {
        List<Vector3Int> path = new List<Vector3Int>();

        int x = bounds.xMin;
        int y = start.y;

        float dirY = 0f;
        float momentum = 0f;
        float noiseOffset = (seed % 9999);

        for (; x <= bounds.xMax; x++)
        {
            float n = Mathf.PerlinNoise(x * 0.015f + noiseOffset, 0f);
            float targetY = start.y + (n - 0.5f) * 18f;
            float chaos = (float)(rnd.NextDouble() - 0.5) * 4f;

            momentum += (targetY - y) * 0.05f;
            momentum += chaos * 0.3f;
            momentum = Mathf.Clamp(momentum, -3f, 3f);

            y += Mathf.RoundToInt(momentum);
            y = Mathf.Clamp(y, bounds.yMin + 8, bounds.yMax - 8);

            path.Add(new Vector3Int(x, y, 0));
        }

        return path;
    }

    // ================================================================
    //   ƒŒœŒÀÕ»“≈À‹Õ¿ﬂ ‘Œ–Ã¿ “”ÕÕ≈Àﬂ
    // ================================================================

    void ApplyHeightSteps(List<Vector3Int> path, System.Random rnd)
    {
        for (int i = 12; i < path.Count - 12; i++)
        {
            int r = rnd.Next(0, 100);
            if (r < 5) path[i] += new Vector3Int(0, rnd.Next(1, 3), 0);
            else if (r < 10) path[i] += new Vector3Int(0, -rnd.Next(1, 3), 0);
        }
    }

    void AddVerticalShafts(List<Vector3Int> path, System.Random rnd)
    {
        for (int i = 25; i < path.Count - 25; i += rnd.Next(30, 45))
        {
            if (rnd.Next(0, 100) > 45) continue;

            Vector3Int p = path[i];
            int height = rnd.Next(6, 14);
            int dir = rnd.Next(0, 2) == 0 ? -1 : 1;

            for (int dy = 0; dy < height; dy++)
                CutRect(p.x - 2, p.y + dy * dir - 2, p.x + 2, p.y + dy * dir + 2);
        }
    }

    void AddLargeCaves(List<Vector3Int> path, System.Random rnd)
    {
        for (int i = 30; i < path.Count - 30; i += rnd.Next(35, 60))
        {
            Vector3Int c = path[i];

            int rx = rnd.Next(5, 12);
            int ry = rnd.Next(4, 10);

            for (int x = -rx; x <= rx; x++)
                for (int y = -ry; y <= ry; y++)
                    if ((x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry) <= 1f)
                        map.SetTile(c + new Vector3Int(x, y, 0), null);

            // ‰ÓÔÓÎÌËÚÂÎ¸Ì˚Â ÏËÌË-ÔÂ˘Â˚
            for (int j = 0; j < rnd.Next(2, 5); j++)
            {
                CutBlob(c + new Vector3Int(rnd.Next(-rx, rx), rnd.Next(-ry, ry), 0),
                        rnd.Next(2, 4), rnd.Next(2, 4));
            }
        }
    }

    // ================================================================
    //   ¬≈“¬»
    // ================================================================

    List<Vector3Int> GrowSideBranch(Vector3Int from, System.Random rnd)
    {
        List<Vector3Int> b = new();

        int length = rnd.Next(15, 40);
        int x = from.x;
        int y = from.y;

        float dirY = 0;
        int dirX = rnd.Next(0, 2) == 0 ? -1 : +1;

        for (int i = 0; i < length; i++)
        {
            x += dirX;

            float n = Mathf.PerlinNoise((x + seed) * 0.05f, 0f);
            float targetY = from.y + (n - 0.5f) * 10f;
            float chaos = (float)(rnd.NextDouble() - 0.5) * 4f;

            dirY += (targetY - y) * 0.1f;
            dirY += chaos * 0.4f;

            dirY = Mathf.Clamp(dirY, -2f, 2f);
            y += Mathf.RoundToInt(dirY);

            y = Mathf.Clamp(y, bounds.yMin + 6, bounds.yMax - 6);

            b.Add(new Vector3Int(x, y, 0));
        }

        return b;
    }

    void CarveBranch(List<Vector3Int> br, System.Random rnd)
    {
        foreach (var p in br)
        {
            CutBlob(p, 2, 1);

            if (rnd.Next(0, 100) < 30)
                CutBlob(p + new Vector3Int(rnd.Next(-2, 3), rnd.Next(-2, 3), 0),
                        3, 2);
        }
    }

    void CarveMiniChamber(Vector3Int p, System.Random rnd)
    {
        CutBlob(p, 2, 1);
        CutBlob(p + new Vector3Int(rnd.Next(-2, 3), rnd.Next(-2, 3), 0), 3, 2);
    }

    // ================================================================
    //   ƒ≈ Œ–¿÷»» / À”“
    // ================================================================

    void PlaceChests(List<Vector3Int> path)
    {
        if (chestPrefab == null) return;

        for (int i = chestDistance; i < path.Count - chestDistance; i += chestDistance)
        {
            Vector3Int p = path[i];
            Vector3 world = map.CellToWorld(new Vector3Int(p.x, p.y + 2, 0));
            Instantiate(chestPrefab, world, Quaternion.identity);
        }
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

    // ================================================================
}
