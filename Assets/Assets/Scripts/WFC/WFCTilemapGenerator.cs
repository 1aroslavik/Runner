using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WFC
{
    [RequireComponent(typeof(Tilemap))]
    public class WFCTilemapGenerator : MonoBehaviour, ICustomEditorEX
    {
        [Header("Player Spawner")]
        public PlayerSpawn playerSpawn;

        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        public bool ShowSuperposition = false;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        public TunnelGenerator tunnelGenerator;
        public SavedMapList savedMaps;

        const int STEPS_PER_FRAME = 256; // ← ускоренная генерация

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        // ==========================================================
        //                           GENERATE
        // ==========================================================
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

            foreach (var tm in _stateMaps)
                tm.ClearAllTiles();

            RandomSeed();
            StartCoroutine(GenerateProgressive());
        }

        IEnumerator GenerateProgressive()
        {
            _generator.Reset(Seed);
            yield return null;

            var enumerator = _generator.RunProgressive().GetEnumerator();
            bool finished = false;

            // ------------------------- FAST WFC -------------------------
            while (!finished)
            {
                for (int i = 0; i < STEPS_PER_FRAME; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        finished = true;
                        break;
                    }

                    var chunk = enumerator.Current;
                    var state = _generator.ChunkStates[chunk.x, chunk.y, chunk.z];

                    // Ставим тайлы ТОЛЬКО если state.Definite = true
                    if (state.Definite && state.Pattern != null && state.Pattern.Chunk != null)
                    {
                        _tilemap.SetTile(Bounds.min + chunk, state.Pattern.Chunk);
                    }
                }

                yield return null;
            }

            // -------------------- DO FINAL FILL -------------------------
            ForceFillMap();

            // --------------------- GENERATE TUNNEL ----------------------
            if (tunnelGenerator != null)
                tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);
        }

        // ==========================================================
        //                 IMMEDIATE WFC (NO COROUTINE)
        // ==========================================================
        private void GenerateImmediateWFC()
        {
            _tilemap.ClearAllTiles();
            TilemapPattern.ExtractPatterns();

            _generator = new WFCGenerator<TileBase>(
                Bounds.size.ToVector2Int().ToVector3Int(1),
                TilemapPattern.NeighborOffset,
                TilemapPattern.Patterns
            );

            _generator.Reset(Seed);

            foreach (var chunk in _generator.RunProgressive())
            {
                var state = _generator.ChunkStates[chunk.x, chunk.y, chunk.z];

                if (state.Definite && state.Pattern != null && state.Pattern.Chunk != null)
                {
                    _tilemap.SetTile(Bounds.min + chunk, state.Pattern.Chunk);
                }
            }

            ForceFillMap();
        }

        // ==========================================================
        //                      FORCE FILL (FIXED)
        // ==========================================================
        private void ForceFillMap()
        {
            var size = _generator.Size;
            System.Random rnd = new System.Random();

            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var state = _generator.ChunkStates[x, y, 0];
                    var pos = Bounds.min + new Vector3Int(x, y, 0);

                    // Уже готово — ставим и пропускаем
                    if (state.Definite)
                    {
                        _tilemap.SetTile(pos, state.Pattern.Chunk);
                        continue;
                    }

                    TileBase tile;
                    var compat = state.Compatibles.ToArray();

                    // Выбор по совместимым
                    if (compat.Length > 0)
                    {
                        var chosen = compat[rnd.Next(compat.Length)];
                        state.CollapseTo(chosen);
                        tile = chosen.Chunk;
                    }
                    else
                    {
                        // Полный тупик — берём случайный валидный паттерн
                        var all = _generator.Patterns.ToArray();
                        var chosen = all[rnd.Next(all.Length)];
                        state.CollapseTo(chosen);
                        tile = chosen.Chunk;
                    }

                    _generator.ChunkStates[x, y, 0] = state;
                    _tilemap.SetTile(pos, tile);
                }
            }
        }

        // ==========================================================
        //                           SAVE MAP
        // ==========================================================
        [EditorButton]
        public void SaveMap()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
                PlaymodeMapSaver.RequestSave(() => SaveMap_Editor());
                return;
            }
            SaveMap_Editor();
#endif
        }

#if UNITY_EDITOR
        private void SaveMap_Editor()
        {
            if (tunnelGenerator == null || savedMaps == null)
            {
                Debug.LogError("❌ Cannot save map — missing refs.");
                return;
            }

            SavedMap map = ScriptableObject.CreateInstance<SavedMap>();

            map.seed = Seed;
            map.mainTunnelPath = new List<Vector3Int>(tunnelGenerator.mainTunnelPath);
            map.startRoomPos = tunnelGenerator.startRoomWorldPos;
            map.endRoomPos = tunnelGenerator.endRoomWorldPos;
            map.mapName = "Map_" + Seed;

            if (!AssetDatabase.IsValidFolder("Assets/SavedMaps"))
                AssetDatabase.CreateFolder("Assets", "SavedMaps");

            string path = $"Assets/SavedMaps/{map.mapName}.asset";

            AssetDatabase.CreateAsset(map, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            savedMaps.maps.Add(map);

            Debug.Log("✔ Map saved at " + path);
        }
#endif

        // ==========================================================
        //                           LOAD MAP
        // ==========================================================
        [EditorButton]
        public void LoadMap()
        {
#if UNITY_EDITOR
            if (savedMaps == null || savedMaps.maps.Count == 0)
            {
                Debug.LogError("❌ No saved maps!");
                return;
            }

            SavedMap map = savedMaps.maps[0];
            Seed = map.seed;

            GenerateImmediateWFC();

            if (tunnelGenerator != null)
                tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);
#endif
        }

        [EditorButton]
        public void RandomSeed()
        {
            Seed = new System.Random().Next();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(transform.position + Bounds.center, Bounds.size);
        }
    }
}
