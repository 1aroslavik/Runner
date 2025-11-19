using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class WFCTilemapGenerator : MonoBehaviour, ICustomEditorEX
    {
        [Header("Decorations")]
        public GameObject[] decorations;
        public float decorChance = 0.12f;

        [Header("Player Spawner")]
        public PlayerSpawn playerSpawn;

        public int Seed;
        public int SavedSeed;

        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        public bool ShowSuperposition = false;

        // Y клетки, где стоит игрок в стартовой комнате
        private int playerSpawnY = -99999;

        [Header("Start / End Rooms (Tilemap prefabs)")]
        public GameObject StartRoomPrefab;
        public GameObject EndRoomPrefab;

        [Header("Room Offsets")]
        public float StartRoomOffset = 3f;
        public float EndRoomOffset = 3f;

        // глобальная высота пола основного пути
        private int startRoomBottomY = -99999;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        // путь основного туннеля
        private List<Vector3Int> mainTunnelPath = new List<Vector3Int>();

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        // ============================================================
        [EditorButton]
        public void RandomSeed()
        {
            Seed = new System.Random().Next();
            Debug.Log($"[WFC] New random Seed = {Seed}");
        }

        [EditorButton]
        public void RandomSeedAndGenerate()
        {
            RandomSeed();
            Generate();
        }

        [EditorButton]
        public void SaveCurrentSeed()
        {
            SavedSeed = Seed;
            Debug.Log($"[WFC] Saved Seed = {SavedSeed}");
        }

        [EditorButton]
        public void LoadSavedSeed()
        {
            if (SavedSeed == 0)
            {
                Debug.LogWarning("SavedSeed = 0 — сначала сохрани сид");
                return;
            }

            Seed = SavedSeed;
            Debug.Log($"[WFC] Loaded saved Seed = {Seed}");
            Generate();
        }

        // ============================================================
        [EditorButton]
        public void Generate()
        {
            if (!TilemapPattern)
                return;

            startRoomBottomY = -99999;
            playerSpawnY = -99999;
            mainTunnelPath.Clear();

            _tilemap.ClearAllTiles();
            TilemapPattern.ExtractPatterns();

            _generator = new WFCGenerator<TileBase>(
                Bounds.size.ToVector2Int().ToVector3Int(1),
                TilemapPattern.NeighborOffset,
                TilemapPattern.Patterns
            );

            foreach (var tm in _stateMaps)
                tm.ClearAllTiles();

            StartCoroutine(GenerateProgressive());
        }

        IEnumerator GenerateProgressive()
        {
            _generator.Reset(Seed);
            yield return null;

            foreach (var collapsedChunk in _generator.RunProgressive())
            {
                var pos = Bounds.min + collapsedChunk;
                var tile = _generator
                    .ChunkStates[collapsedChunk.x, collapsedChunk.y, collapsedChunk.z]
                    .Pattern
                    .Chunk;

                _tilemap.SetTile(pos, tile);

                if (ShowSuperposition)
                    DrawSuperposition();

                yield return null;
            }

            // после WFC — вырезаем систему тоннелей
            GenerateAntTunnelNetwork();
        }

        void DrawSuperposition()
        {
            if (_stateMaps.Count < _generator.Patterns.Count)
            {
                for (var i = _stateMaps.Count; i < _generator.Patterns.Count; i++)
                {
                    var obj = new GameObject($"StateMap_{i}");
                    obj.transform.parent = transform;
                    obj.transform.position = transform.position + Vector3.forward * (i + 1);

                    var tilemap = obj.AddComponent<Tilemap>();
                    obj.AddComponent<TilemapRenderer>();
                    tilemap.color = Color.white.WithAlpha(0.7f);

                    _stateMaps.Add(tilemap);
                }
            }

            for (var x = 0; x < _generator.Size.x; x++)
                for (var y = 0; y < _generator.Size.y; y++)
                {
                    var p = Bounds.min + new Vector3Int(x, y, 0);
                    int idx = 0;

                    foreach (var pat in _generator.ChunkStates[x, y, 0].Compatibles)
                        _stateMaps[idx++].SetTile(p, pat.Chunk);
                }
        }

        // ============================================================
        bool InsideBounds(Vector3Int p)
        {
            return p.x >= Bounds.xMin && p.x < Bounds.xMax &&
                   p.y >= Bounds.yMin && p.y < Bounds.yMax;
        }

        // ============================================================
        //     ROOM SPAWN — ОПРЕДЕЛЯЕМ ВЫСОТУ ПУТИ ОТ START ROOM
        // ============================================================
        void PlaceRoomPrefab(GameObject prefab, Vector3Int tunnelCell, bool isLeft)
        {
            if (prefab == null) return;

            Vector3 tunnelWorld = _tilemap.CellToWorld(tunnelCell);

            Tilemap roomMap = prefab.GetComponentInChildren<Tilemap>();
            if (roomMap == null)
            {
                Debug.LogError("❌ В префабе комнаты нет Tilemap!");
                return;
            }

            BoundsInt rb = roomMap.cellBounds;

            float cell = roomMap.cellSize.x;
            float leftEdge = rb.xMin * cell;
            float rightEdge = rb.xMax * cell;

            Vector3 spawnPos = tunnelWorld;

            if (isLeft)
            {
                spawnPos.x -= Mathf.Abs(rightEdge);
                spawnPos.x -= StartRoomOffset;
            }
            else
            {
                // конечная комната ближе к тоннелю
                spawnPos.x += Mathf.Abs(leftEdge) - 1f;
            }


            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            // стартовая комната → ищем PlayerSpawnPoint
            if (prefab == StartRoomPrefab)
            {
                Transform spawnPoint = instance.transform.Find("PlayerSpawnPoint");

                if (spawnPoint != null)
                {
                    Vector3 wp = spawnPoint.position;
                    Vector3Int cellPos = _tilemap.WorldToCell(wp);

                    playerSpawnY = cellPos.y;
                    startRoomBottomY = playerSpawnY - 1;

                    Debug.Log($"✔ PlayerSpawn Y = {playerSpawnY}, путь по Y = {startRoomBottomY}");
                }
                else
                {
                    Debug.LogError("❌ В стартовой комнате нет PlayerSpawnPoint");
                }
            }
        }

        // ============================================================
        //            УНИВЕРСАЛЬНЫЙ ВЫРЕЗАТЕЛЬ ПРЯМОУГОЛЬНИКА
        // ============================================================
        void CutRect(int x1, int y1, int x2, int y2)
        {
            if (x2 < x1) { int tmp = x1; x1 = x2; x2 = tmp; }
            if (y2 < y1) { int tmp = y1; y1 = y2; y2 = tmp; }

            for (int x = x1; x <= x2; x++)
            {
                for (int y = y1; y <= y2; y++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    if (InsideBounds(cell))
                        _tilemap.SetTile(cell, null);
                }
            }
        }

        // округлая "капля" / камера
        void CutBlob(Vector3Int center, int radiusX, int radiusY)
        {
            for (int dx = -radiusX; dx <= radiusX; dx++)
            {
                for (int dy = -radiusY; dy <= radiusY; dy++)
                {
                    float nx = (float)dx / radiusX;
                    float ny = (float)dy / radiusY;
                    if (nx * nx + ny * ny <= 1.0f)
                    {
                        Vector3Int c = new Vector3Int(center.x + dx, center.y + dy, 0);
                        if (InsideBounds(c))
                            _tilemap.SetTile(c, null);
                    }
                }
            }
        }

        // ============================================================
        //           L-SYSTEM ПРОФИЛЬ (пока не используется)
        // ============================================================
        string BuildLSystemProfile(int length, System.Random rnd)
        {
            // S = ровно, U = вверх, D = вниз
            string axiom = "S";
            Dictionary<char, string> rules = new Dictionary<char, string>()
            {
                { 'S', "SUD" },
                { 'U', "SU"  },
                { 'D', "SD"  }
            };

            string current = axiom;

            while (current.Length < length)
            {
                StringBuilder sb = new StringBuilder(current.Length * 3);
                foreach (char c in current)
                {
                    if (rules.TryGetValue(c, out string prod))
                        sb.Append(prod);
                    else
                        sb.Append(c);
                }
                current = sb.ToString();
            }

            char[] arr = current.ToCharArray();

            // небольшая мутация для разнообразия
            for (int i = 0; i < arr.Length; i++)
            {
                int r = rnd.Next(0, 100);
                if (r < 8) arr[i] = 'S';
                else if (r < 16) arr[i] = 'U';
                else if (r < 24) arr[i] = 'D';
            }

            if (arr.Length > length)
                current = new string(arr, 0, length);
            else
                current = new string(arr);

            return current;
        }

        // ============================================================
        //           ПЕРЛИН-ПРОФИЛЬ ОСНОВНОГО ТОННЕЛЯ
        // ============================================================
        List<Vector3Int> BuildMainAntTunnelPerlin(Vector3Int startCell, int width)
        {
            List<Vector3Int> path = new List<Vector3Int>();
            float scale = 0.015f;        // частота шума
            float amp = 12f;             // амплитуда (высота колебаний)
            float offset = Seed % 9999;  // случайный оффсет

            for (int x = Bounds.xMin; x <= Bounds.xMax; x++)
            {
                float nx = x * scale + offset;
                float noise = Mathf.PerlinNoise(nx, 0f);

                int y = startCell.y + Mathf.RoundToInt((noise - 0.5f) * amp);

                y = Mathf.Clamp(y, Bounds.yMin + 8, Bounds.yMax - 8);

                path.Add(new Vector3Int(x, y, 0));
            }

            return path;
        }

        // ============================================================
        //           ВЕРТИКАЛЬНЫЕ ШАХТЫ
        // ============================================================
        void AddVerticalShafts(List<Vector3Int> path, System.Random rnd)
        {
            for (int i = 20; i < path.Count - 20; i += rnd.Next(25, 40))
            {
                if (rnd.Next(0, 100) > 45)
                    continue;

                Vector3Int p = path[i];

                // насколько падаем/поднимаемся
                int height = rnd.Next(6, 18);
                int dir = rnd.Next(0, 2) == 0 ? -1 : +1; // вверх или вниз

                for (int dy = 0; dy < height; dy++)
                {
                    Vector3Int v = new Vector3Int(p.x, p.y + dy * dir, 0);

                    if (!InsideBounds(v))
                        break;

                    CutRect(v.x - 2, v.y - 2, v.x + 2, v.y + 2);
                }
            }
        }

        // ============================================================
        //           СТУПЕНИ / МНОГО УРОВНЕЙ ВЫСОТЫ
        // ============================================================
        void ApplyHeightSteps(List<Vector3Int> path, System.Random rnd)
        {
            for (int i = 10; i < path.Count - 10; i++)
            {
                int r = rnd.Next(0, 100);

                if (r < 4)
                {
                    // ступень вверх
                    path[i] = new Vector3Int(path[i].x, path[i].y + rnd.Next(1, 3), 0);
                }
                else if (r < 8)
                {
                    // ступень вниз
                    path[i] = new Vector3Int(path[i].x, path[i].y - rnd.Next(1, 3), 0);
                }
            }
        }

        // ============================================================
        //      ОСНОВНОЙ ТОЛСТЫЙ ТОННЕЛЬ
        // ============================================================
        void CarveMainAntTunnel(List<Vector3Int> path)
        {
            foreach (var p in path)
            {
                // ширина 6 тайлов, высота 4 тайла
                CutRect(p.x - 3, p.y - 2, p.x + 3, p.y + 2);
            }
        }

        // боковой ход (как корень)
        List<Vector3Int> GrowSideBranch(Vector3Int from, System.Random rnd)
        {
            List<Vector3Int> branch = new List<Vector3Int>();

            int length = rnd.Next(10, 25);
            int dirX = rnd.Next(0, 2) == 0 ? -1 : +1;
            int x = from.x;
            int y = from.y;

            for (int i = 0; i < length; i++)
            {
                x += dirX;
                int r = rnd.Next(0, 100);
                if (r < 30) y += 1;
                else if (r < 60) y -= 1;

                y = Mathf.Clamp(y, Bounds.yMin + 4, Bounds.yMax - 4);

                Vector3Int c = new Vector3Int(x, y, 0);
                if (!InsideBounds(c)) break;

                branch.Add(c);
            }

            return branch;
        }

        void CarveBranchWithChambers(List<Vector3Int> branch, System.Random rnd)
        {
            if (branch.Count == 0) return;

            for (int i = 0; i < branch.Count; i++)
            {
                Vector3Int p = branch[i];

                // узкий коридор
                CutBlob(p, 2, 1);

                // маленькие изгибы и комнаты
                if (i > 3 && i < branch.Count - 3 && rnd.Next(0, 100) < 25)
                {
                    Vector3Int chamberCenter = p + new Vector3Int(
                        rnd.Next(-2, 3),
                        rnd.Next(-2, 3),
                        0
                    );

                    int rx = rnd.Next(3, 5);
                    int ry = rnd.Next(2, 4);
                    CutBlob(chamberCenter, rx, ry);
                }
            }
        }

        // маленькая камера прямо от основного туннеля
        void CarveMiniChamber(Vector3Int center, System.Random rnd)
        {
            // вход
            CutBlob(center, 2, 1);

            // сама камера (капля)
            Vector3Int chamberCenter = center + new Vector3Int(
                rnd.Next(-2, 3),
                rnd.Next(-2, 3),
                0
            );

            int rx = rnd.Next(3, 5);
            int ry = rnd.Next(2, 3);
            CutBlob(chamberCenter, rx, ry);
        }

        // ============================================================
        //      ДЕКОРАЦИИ
        // ============================================================
        void PlaceDecorations(List<Vector3Int> path, System.Random rnd)
        {
            if (decorations == null || decorations.Length == 0)
                return;

            foreach (var p in path)
            {
                if (rnd.NextDouble() > decorChance)
                    continue;

                Vector3 world = _tilemap.CellToWorld(p);
                world += new Vector3(rnd.Next(-1, 2), rnd.Next(-1, 2), 0);

                GameObject prefab = decorations[rnd.Next(decorations.Length)];
                Instantiate(prefab, world, Quaternion.identity, transform);
            }
        }

        // ============================================================
        //                 ОСНОВНАЯ ГЕНЕРАЦИЯ ТОННЕЛЕЙ
        // ============================================================
        void GenerateAntTunnelNetwork()
        {
            System.Random rnd = new System.Random(Seed);

            // ------- 1. СПАВНИМ СТАРТОВУЮ КОМНАТУ -------
            Vector3Int tmpStart = new Vector3Int(
                Bounds.xMin + 5,
                Mathf.FloorToInt(Bounds.center.y),
                0
            );

            PlaceRoomPrefab(StartRoomPrefab, tmpStart, true);
            if (startRoomBottomY == -99999)
            {
                Debug.LogError("StartRoomBottomY NOT SET");
                return;
            }

            // ------- 2. ПЕРЛИН-ТОННЕЛЬ -------
            mainTunnelPath = BuildMainAntTunnelPerlin(
                new Vector3Int(Bounds.xMin, startRoomBottomY),
                Bounds.size.x
            );

            // ------- 3. Ступени, несколько уровней -------
            ApplyHeightSteps(mainTunnelPath, rnd);

            // ------- 4. Вертикальные шахты -------
            AddVerticalShafts(mainTunnelPath, rnd);

            // ------- 5. ВЫРЕЗАЕМ ТОННЕЛЬ -------
            CarveMainAntTunnel(mainTunnelPath);

            // ------- 6. Боковые ветви + мини-камеры -------
            for (int i = 15; i < mainTunnelPath.Count - 20;)
            {
                if (rnd.Next(0, 100) < 45)
                {
                    var br = GrowSideBranch(mainTunnelPath[i], rnd);
                    CarveBranchWithChambers(br, rnd);
                }

                if (rnd.Next(0, 100) < 30)
                {
                    CarveMiniChamber(mainTunnelPath[i], rnd);
                }

                i += rnd.Next(18, 28);
            }

            // ------- 7. Декорации -------
            PlaceDecorations(mainTunnelPath, rnd);

            // ------- 8. КОНЕЧНАЯ КОМНАТА -------
            Vector3Int end = mainTunnelPath[mainTunnelPath.Count - 1];
            CutRect(end.x - 2, end.y - 2, end.x + 2, end.y + 2);
            PlaceRoomPrefab(EndRoomPrefab, end, false);

            // ------- 9. СПАВН ИГРОКА -------
            if (playerSpawn != null)
                playerSpawn.SpawnPlayer();

            Debug.Log("✔ Перлин-тоннель сгенерирован");
        }

        // ============================================================
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}
