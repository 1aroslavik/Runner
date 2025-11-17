using System;
using System.Collections;
using System.Collections.Generic;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class WFCTilemapGenerator : MonoBehaviour
    {
        [Header("WFC")]
        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;          // ставь здесь size = (1000, 70, 1) в инспекторе
        public bool ShowSuperposition = false;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        public Action OnGenerationComplete;
        public Tilemap Tilemap => _tilemap;
        public BoundsInt GenerationBounds => Bounds;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        /// <summary>
        /// Установить сид извне (LevelBuilder)
        /// </summary>
        public void SetSeed(int seed)
        {
            Seed = seed;
        }

        /// <summary>
        /// Запустить генерацию с текущим Seed (не меняя его)
        /// </summary>
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

            foreach (var t in _stateMaps)
                t.ClearAllTiles();

            StartCoroutine(GenerateAndThenSpawnPlayer());
        }

        private IEnumerator GenerateAndThenSpawnPlayer()
        {
            yield return StartCoroutine(GenerateProgressive());

            Debug.Log("✔ WFC generation completed with Seed = " + Seed);
            OnGenerationComplete?.Invoke();
        }

        private IEnumerator GenerateProgressive()
        {
            // Один раз сбрасываем генератор с текущим Seed
            _generator.Reset(Seed);

            // Можно немного поднять FPS, чтобы генерация шла быстрее
            Application.targetFrameRate = 200;

            yield return null;

            int step = 0;

            foreach (var collapsedChunk in _generator.RunProgressive())
            {
                Vector3Int pos = Bounds.min + collapsedChunk;
                TileBase tile = _generator
                    .ChunkStates[collapsedChunk.x, collapsedChunk.y, collapsedChunk.z]
                    .Pattern.Chunk;

                _tilemap.SetTile(pos, tile);

                if (ShowSuperposition)
                    DrawSuperposition();

                // Для большой карты 1000×70 — иногда отдаём кадр Unity
                step++;
                if (step % 50 == 0)
                    yield return null;
            }
        }

        private void DrawSuperposition()
        {
            if (_stateMaps.Count < _generator.Patterns.Count)
            {
                for (var i = _stateMaps.Count; i < _generator.Patterns.Count; i++)
                {
                    var obj = new GameObject("StateMap_" + i);
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
                    var idx = 0;
                    foreach (var pattern in _generator.ChunkStates[x, y, 0].Compatibles)
                    {
                        _stateMaps[idx++].SetTile(p, pattern.Chunk);
                    }
                }
        }

        /// <summary>
        /// Если нужен случайный сид – вызываешь это ИЗВНЕ, а потом Generate()
        /// </summary>
        public void RandomSeed()
        {
            Seed = new System.Random().Next();
        }
    }
}
