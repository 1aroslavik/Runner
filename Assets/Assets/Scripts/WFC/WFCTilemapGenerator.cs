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
    public class WFCTilemapGenerator : MonoBehaviour
    {
        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        public bool ShowSuperposition = false;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        public Action OnGenerationComplete;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

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

            StartCoroutine(GenerateAndThenSpawnPlayer());
        }

        IEnumerator GenerateAndThenSpawnPlayer()
        {
            yield return StartCoroutine(GenerateProgressive());

            Debug.Log("✔ WFC generation completed");

            // вызываем событие – LevelBuilder или любой другой объект может подписаться
            OnGenerationComplete?.Invoke();
        }

        IEnumerator GenerateProgressive()
        {
            _generator.Reset(Seed);

            yield return null;

            foreach (var collapsedChunk in _generator.RunProgressive())
            {
                Vector3Int pos = Bounds.min + collapsedChunk;
                TileBase tile = _generator.ChunkStates[collapsedChunk.x, collapsedChunk.y, collapsedChunk.z].Pattern.Chunk;

                _tilemap.SetTile(pos, tile);

                if (ShowSuperposition)
                    DrawSuperposition();

                yield return null;
            }
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

        public void RandomSeed()
        {
            Seed = new System.Random().Next();
        }
    }
}
