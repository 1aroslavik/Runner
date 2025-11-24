using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SardineFish.Utils;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

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

        [Header("Спавнеры")]
        public EnemySpawner enemySpawner;

        public int Seed;
        public TilemapPattern TilemapPattern;
        public BoundsInt Bounds;
        public bool ShowSuperposition = false;

        private Tilemap _tilemap;
        private WFCGenerator<TileBase> _generator;
        private List<Tilemap> _stateMaps = new List<Tilemap>();

        public TunnelGenerator tunnelGenerator;
        public SavedMapList savedMaps;

        const int STEPS_PER_FRAME = 256;

        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // ТОЛЬКО ЭТА СТРОКА — решение WebGL проблемы
        private TileBase[] allTiles;
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();

            // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
            // Грузим ВСЕ тайлы из Resources/Tiles
            allTiles = Resources.LoadAll<TileBase>("Tiles");

            if (allTiles == null || allTiles.Length == 0)
                Debug.LogError("❌ WebGL: Не найдено ТАЙЛОВ в Resources/Tiles !");
            else
                Debug.Log($"✔ WebGL: Загружено тайлов: {allTiles.Length}");
            // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
        }

        private void Start()
        {
            Generate();
        }

        public void GenerateNewLevel()
        {
            Debug.Log("WFC Генератор: Запуск цикла очистки и генерации нового уровня.");
            ClearGeneratedObjects();
            Generate();
        }

        private void ClearGeneratedObjects()
        {
            var oldPlayers = GameObject.FindGameObjectsWithTag("Player");
            foreach (var player in oldPlayers)
            {
                if (player.GetComponent<PlayerHealth>() != null)
                    Destroy(player);
            }

            var enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
                Destroy(enemy);

            var npcs = GameObject.FindGameObjectsWithTag("NPC");
            foreach (var npc in npcs)
                Destroy(npc);
        }

        [EditorButton]
        public void Generate()
        {
            Debug.Log("[WFC Генератор]: Запуск генерации уровня...");
            LoadingScreenUI.Instance?.Show();

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

            StopAllCoroutines();
            StartCoroutine(GenerateProgressive());
        }

        IEnumerator GenerateProgressive()
        {
            Debug.Log("[WFC Генератор]: Запуск прогрессивной генерации...");

            _generator.Reset(Seed);
            yield return null;

            var enumerator = _generator.RunProgressive().GetEnumerator();
            bool finished = false;

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

                    if (state.Definite && state.Pattern != null && state.Pattern.Chunk != null)
                        _tilemap.SetTile(Bounds.min + chunk, state.Pattern.Chunk);
                }

                yield return null;
            }

            ForceFillMap();

            if (tunnelGenerator != null)
                tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);

            if (enemySpawner != null && tunnelGenerator != null)
                enemySpawner.SpawnEnemiesAlongTunnel(tunnelGenerator.mainTunnelPath);

            DeathScreenUI.Instance?.HideDeathScreen();
            LoadingScreenUI.Instance?.Hide();
        }

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
                    _tilemap.SetTile(Bounds.min + chunk, state.Pattern.Chunk);
            }

            ForceFillMap();
        }

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

                    if (state.Definite)
                    {
                        _tilemap.SetTile(pos, state.Pattern.Chunk);
                        continue;
                    }

                    TileBase tile;
                    var compat = state.Compatibles.ToArray();

                    if (compat.Length > 0)
                    {
                        var chosen = compat[rnd.Next(compat.Length)];
                        state.CollapseTo(chosen);
                        tile = chosen.Chunk;
                    }
                    else
                    {
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

        [EditorButton]
        public void LoadMap()
        {
#if UNITY_EDITOR
            if (savedMaps == null || savedMaps.maps.Count == 0)
            {
                Debug.LogError("❌ No saved maps!");
                return;
            }

            ClearGeneratedObjects();

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
