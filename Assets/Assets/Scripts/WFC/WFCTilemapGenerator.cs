using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class WFCTilemapGenerator : MonoBehaviour, ICustomEditorEX
    {
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

        // === ГЛОБАЛЬНАЯ ВЫСОТА ПУТИ (клетка пола, по которой идём) ===
        private int startRoomBottomY = -99999;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

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
                spawnPos.x += Mathf.Abs(leftEdge);
                spawnPos.x += EndRoomOffset;
            }

            // создаём комнату
            GameObject instance = Instantiate(prefab, spawnPos, Quaternion.identity, transform);

            // === ЕСЛИ ЭТО START ROOM → ИЩЕМ PlayerSpawnPoint ===
            if (prefab == StartRoomPrefab)
            {
                Transform spawnPoint = instance.transform.Find("PlayerSpawnPoint");

                if (spawnPoint != null)
                {
                    // мировая позиция спавна игрока
                    Vector3 wp = spawnPoint.position;

                    // переводим в клетку тайлмапа
                    Vector3Int cellPos = _tilemap.WorldToCell(wp);

                    playerSpawnY = cellPos.y;

                    // Путь идёт по клетке пола под игроком
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

        // ============================================================
        //         ГЛАВНЫЙ ПУТЬ — ПРЯМОЙ ТУННЕЛЬ
        // ============================================================
        void CarveMainTunnel()
        {
            int y = startRoomBottomY;   // уровень пола
            int h = 4;                  // высота туннеля

            CutRect(
                Bounds.xMin,
                y - 1,
                Bounds.xMax,
                y + h
            );
        }

        // ============================================================
        //                        ЯМА ВНИЗ
        // ============================================================
        void CarvePit(int x, int depth, int width = 4)
        {
            int y = startRoomBottomY;

            CutRect(
                x - width,
                y - depth - 4,
                x + width,
                y
            );
        }

        // ============================================================
        //                      ВЕРТИКАЛЬНЫЙ ТУННЕЛЬ ВВЕРХ
        // ============================================================
        void CarveUp(int x, int height, int width = 3)
        {
            int y = startRoomBottomY;

            CutRect(
                x - width,
                y,
                x + width,
                y + height
            );
        }

        // ============================================================
        //                          КОМНАТА
        // ============================================================
        void CarveRoom(int x, int y, int w = 6, int h = 4)
        {
            CutRect(
                x - w,
                y - h,
                x + w,
                y + h
            );
        }

        // ============================================================
        //                 ОСНОВНАЯ ГЕНЕРАЦИЯ ТОННЕЛЕЙ
        // ============================================================
        void GenerateAntTunnelNetwork()
        {
            // 1. ставим комнату старта
            Vector3Int tmpStartCell = new Vector3Int(
                Bounds.xMin + 5,
                Mathf.FloorToInt(Bounds.center.y),
                0
            );

            PlaceRoomPrefab(StartRoomPrefab, tmpStartCell, true);

            if (startRoomBottomY == -99999)
            {
                Debug.LogError("StartRoomBottomY NOT SET");
                return;
            }

            // 2. главный туннель
            CarveMainTunnel();

            System.Random rnd = new System.Random(Seed);

            // 3. добавляем ямы и верхние тоннели каждые ~40 клеток
            for (int x = Bounds.xMin + 20; x < Bounds.xMax - 20; x += rnd.Next(35, 50))
            {
                // яма вниз
                if (rnd.Next(0, 100) < 60)
                    CarvePit(x, rnd.Next(8, 15));

                // тоннель вверх
                if (rnd.Next(0, 100) < 40)
                    CarveUp(x, rnd.Next(6, 12));

                // комнатка внизу
                if (rnd.Next(0, 100) < 30)
                    CarveRoom(x, startRoomBottomY - rnd.Next(10, 20));

                // комнатка вверху
                if (rnd.Next(0, 100) < 30)
                    CarveRoom(x, startRoomBottomY + rnd.Next(8, 18));
            }

            // 4. ставим конец
            PlaceRoomPrefab(EndRoomPrefab, new Vector3Int(Bounds.xMax - 5, startRoomBottomY, 0), false);

            // 5. спавн
            if (playerSpawn != null)
                playerSpawn.SpawnPlayer();
        }

        // ============================================================
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}
