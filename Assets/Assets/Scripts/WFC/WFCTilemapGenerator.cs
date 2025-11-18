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
        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        public bool ShowSuperposition = false;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        private CoroutineRunner CoroutineRunner;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        [EditorButton]
        public void Generate()
        {
            if (!TilemapPattern)
                return;

            _tilemap.ClearAllTiles();
            TilemapPattern.ExtractPatterns();

            _generator = new WFCGenerator<TileBase>(
                Bounds.size.ToVector2Int().ToVector3Int(1),
                TilemapPattern.NeighborOffset,
                TilemapPattern.Patterns
            );

            foreach (var tilemap in _stateMaps)
                tilemap.ClearAllTiles();

            RandomSeed();
            StartCoroutine(GenerateProgressive());
        }



        // ====================================================================  
        //                       WFC GENERATION (НЕ ТРОГАЕМ)
        // ====================================================================
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

            // 🟦 Путь после генерации
            GeneratePath();
        }



        void DrawSuperposition()
        {
            if (_stateMaps.Count < _generator.Patterns.Count)
            {
                for (var i = _stateMaps.Count; i < _generator.Patterns.Count; i++)
                {
                    var obj = new GameObject();
                    obj.transform.parent = transform;
                    obj.transform.position = transform.position + Vector3.forward * (i + 1);
                    var tilemap = obj.AddComponent<Tilemap>();
                    var renderer = obj.AddComponent<TilemapRenderer>();
                    tilemap.color = Color.white.WithAlpha(0.7f);
                    _stateMaps.Add(tilemap);
                }
            }

            for (var x = 0; x < _generator.Size.x; x++)
                for (var y = 0; y < _generator.Size.y; y++)
                {
                    var p = Bounds.min + new Vector3Int(x, y, 0);
                    var idx = 0;
                    foreach (var pattern in _generator.ChunkStates[x, y, 0].Compatibles)
                    {
                        _stateMaps[idx++].SetTile(p, pattern.Chunk);
                    }
                }
        }



        [EditorButton()]
        public void RandomSeed()
        {
            Seed = new System.Random().Next();
        }

        bool InsideBounds(Vector3Int p)
        {
            return p.x >= Bounds.xMin && p.x < Bounds.xMax &&
                   p.y >= Bounds.yMin && p.y < Bounds.yMax;
        }


        // ====================================================================  
        //                             ПУТЬ
        // ====================================================================  


        // ─────────────────────────────────────────────
        // Генерация ветки
        // ─────────────────────────────────────────────
        List<Vector3Int> GenerateBranch(Vector3Int start, System.Random rnd)
        {
            List<Vector3Int> branch = new List<Vector3Int>();
            Vector3Int p = start;

            int length = rnd.Next(5, 20);

            for (int i = 0; i < length; i++)
            {
                int dir = rnd.Next(0, 4);

                switch (dir)
                {
                    case 0: p += Vector3Int.up; break;
                    case 1: p += Vector3Int.down; break;
                    case 2: p += Vector3Int.left; break;
                    case 3: p += Vector3Int.right; break;
                }

                if (!InsideBounds(p)) break;

                branch.Add(p);
            }

            return branch;
        }



        // ─────────────────────────────────────────────
        // Основной путь + ветвления
        // ─────────────────────────────────────────────
        List<Vector3Int> BuildOrganicBranchedPath(Vector3Int start)
        {
            List<Vector3Int> mainPath = new List<Vector3Int>();
            List<List<Vector3Int>> branches = new List<List<Vector3Int>>();

            Vector3Int p = start;

            System.Random rnd = new System.Random(Seed);

            int maxMainSteps = Mathf.Max(40, Bounds.size.x * 2);

            for (int i = 0; i < maxMainSteps; i++)
            {
                p += Vector3Int.right;

                int r = rnd.Next(0, 100);

                if (r < 20 && p.y < Bounds.yMax - 3)
                    p += Vector3Int.up;
                else if (r < 40 && p.y > Bounds.yMin + 3)
                    p += Vector3Int.down;

                if (!InsideBounds(p)) break;

                mainPath.Add(p);

                // 🔥 Ветки
                if (rnd.Next(0, 100) < 25)
                {
                    var branch = GenerateBranch(p, rnd);
                    if (branch.Count > 0)
                        branches.Add(branch);
                }

                if (p.x >= Bounds.xMax - 4)
                    break;
            }

            // Объединяем путь и ветви
            List<Vector3Int> full = new List<Vector3Int>(mainPath);
            foreach (var b in branches)
                full.AddRange(b);

            return full;
        }



        // ─────────────────────────────────────────────
        // Выкапывание
        // ─────────────────────────────────────────────
        void CarveOrganicPath(List<Vector3Int> path)
        {
            if (path == null || path.Count == 0)
                return;

            var firstPattern = TilemapPattern.Patterns.FirstOrDefault();
            TileBase groundTile = firstPattern != null ? firstPattern.Chunk : null;

            foreach (var p in path)
            {
                if (!InsideBounds(p))
                    continue;

                // пространство для игрока
                ClearIfInside(p);
                ClearIfInside(p + Vector3Int.up);

                // расширение
                ClearIfInside(p + Vector3Int.left);
                ClearIfInside(p + Vector3Int.right);

                // пол
                if (groundTile != null)
                {
                    SetGroundIfInside(p + Vector3Int.down, groundTile);
                    SetGroundIfInside(p + Vector3Int.down * 2, groundTile);
                }
            }
        }



        void ClearIfInside(Vector3Int pos)
        {
            if (InsideBounds(pos))
                _tilemap.SetTile(pos, null);
        }

        void SetGroundIfInside(Vector3Int pos, TileBase groundTile)
        {
            if (InsideBounds(pos))
                _tilemap.SetTile(pos, groundTile);
        }



        // ─────────────────────────────────────────────
        // Главный запуск пути
        // ─────────────────────────────────────────────
        void GeneratePath()
        {
            Vector3Int start = new Vector3Int(
                Bounds.xMin + 2,
                Bounds.yMin + Bounds.size.y / 2,
                0
            );

            var fullPath = BuildOrganicBranchedPath(start);

            CarveOrganicPath(fullPath);

            Debug.Log("Generated path nodes: " + fullPath.Count);
        }



        // ====================================================================  
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}
