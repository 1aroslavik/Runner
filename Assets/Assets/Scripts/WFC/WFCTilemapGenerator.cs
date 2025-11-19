using System.Collections;
using System.Collections.Generic;
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

        // Туннельный генератор
        public TunnelGenerator tunnelGenerator;

        // Список сохранённых карт
        public SavedMapList savedMaps;

        // --------- ПОЛЯ ДЛЯ БЕЗОПАСНОЙ ПАКЕТНОЙ ГЕНЕРАЦИИ ----------
#if UNITY_EDITOR
        private bool batchRunning = false;
        private int batchTarget = 100;
        private int batchIndex = 0;
        private int batchSaved = 0;
        private System.Random batchRnd;
#endif

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
        }

        // -------------------------------------------------------
        // -------------------- GENERATE -------------------------
        // -------------------------------------------------------
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

            if (tunnelGenerator != null)
                tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);
        }

        // -------------------------------------------------------
        // ------------------ SUPERPOSITION ----------------------
        // -------------------------------------------------------
        void DrawSuperposition()
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
                    int idx = 0;

                    foreach (var pattern in _generator.ChunkStates[x, y, 0].Compatibles)
                        _stateMaps[idx++].SetTile(p, pattern.Chunk);
                }
        }

        // -------------------------------------------------------
        // -------------------- SAVE MAP -------------------------
        // -------------------------------------------------------
        [EditorButton]
        public void SaveMap()
        {
#if UNITY_EDITOR
            if (tunnelGenerator == null)
            {
                Debug.LogError("❌ TunnelGenerator не назначен!");
                return;
            }
            if (savedMaps == null)
            {
                Debug.LogError("❌ SavedMapList не назначен!");
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

            AssetDatabase.CreateAsset(map, $"Assets/SavedMaps/{map.mapName}.asset");
            AssetDatabase.SaveAssets();

            savedMaps.maps.Add(map);

            Debug.Log("✔ Карта сохранена: " + map.mapName);
#endif
        }

        // -------------------------------------------------------
        // -------------------- LOAD MAP -------------------------
        // -------------------------------------------------------
        [EditorButton]
        public void LoadMap()
        {
#if UNITY_EDITOR
            if (savedMaps == null || savedMaps.maps.Count == 0)
            {
                Debug.LogError("❌ Нет сохранённых карт!");
                return;
            }

            // 🔹 пока берём первую карту, потом можно сделать выбор по индексу или по seed
            SavedMap map = savedMaps.maps[0];

            Debug.Log("✔ Загружаем карту по сиду: " + map.mapName + " (Seed = " + map.seed + ")");

            // 1) Ставим сид, чтобы WFC + туннели сгенерировались так же
            Seed = map.seed;

            // 2) Полностью регенерируем WFC-карту (быстрая, без корутин)
            GenerateImmediateWFC();   // <- у тебя этот метод уже есть

            // 3) Генерируем всю тоннельную систему поверх WFC
            if (tunnelGenerator != null)
            {
                tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);
            }

            // 4) Спавним игрока (внутри GenerateTunnel уже должен вызываться SpawnPlayer,
            //    но если хочешь — можешь продублировать)
            
#endif
        }



        // -------------------------------------------------------
        // ------------ GENERATE IMMEDIATE (no coroutine) --------
        // -------------------------------------------------------
        private void GenerateImmediateWFC()
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

            _generator.Reset(Seed);

            // Используем тот же RunProgressive, но без yield
            foreach (var collapsedChunk in _generator.RunProgressive())
            {
                var pos = Bounds.min + collapsedChunk;

                var tile = _generator
                    .ChunkStates[collapsedChunk.x, collapsedChunk.y, collapsedChunk.z]
                    .Pattern
                    .Chunk;

                _tilemap.SetTile(pos, tile);
            }
        }

        // -------------------------------------------------------
        // -------------------- VALIDATION -----------------------
        // -------------------------------------------------------
        private bool ValidateMap()
        {
            // Простейшая проверка — можно потом усложнить
            if (tunnelGenerator.mainTunnelPath == null ||
                tunnelGenerator.mainTunnelPath.Count < 20)
                return false;

            if (tunnelGenerator.startRoomWorldPos == Vector3.zero ||
                tunnelGenerator.endRoomWorldPos == Vector3.zero)
                return false;

            if (tunnelGenerator.mainTunnelPath.Count < 60)
                return false;

            return true;
        }

        // -------------------------------------------------------
        // --------- БЕЗОПАСНАЯ ГЕНЕРАЦИЯ 100 КАРТ ---------------
        // -------------------------------------------------------
        [EditorButton]
        public void Generate100Maps()
        {
#if UNITY_EDITOR
            if (batchRunning)
            {
                Debug.LogWarning("⚠ Пакетная генерация уже запущена!");
                return;
            }

            if (tunnelGenerator == null)
            {
                Debug.LogError("❌ TunnelGenerator не назначен!");
                return;
            }

            if (savedMaps == null)
            {
                Debug.LogError("❌ SavedMapList не назначен!");
                return;
            }

            batchTarget = 100;
            batchIndex = 0;
            batchSaved = 0;
            batchRnd = new System.Random();
            batchRunning = true;

            EditorApplication.update += BatchStep;
            EditorUtility.DisplayProgressBar("Batch map generation", "Starting…", 0f);

            Debug.Log("▶ Запущена безопасная генерация 100 карт");
#endif
        }

#if UNITY_EDITOR
        private void BatchStep()
        {
            if (!batchRunning)
            {
                EditorApplication.update -= BatchStep;
                EditorUtility.ClearProgressBar();
                return;
            }

            if (batchIndex >= batchTarget)
            {
                batchRunning = false;
                EditorApplication.update -= BatchStep;
                EditorUtility.ClearProgressBar();
                Debug.Log($"🏁 Пакетная генерация завершена. Сохранено {batchSaved} карт из {batchTarget}.");
                return;
            }

            // Новый сид
            Seed = batchRnd.Next();

            float progress = (float)batchIndex / batchTarget;
            bool cancel = EditorUtility.DisplayCancelableProgressBar(
                "Batch map generation",
                $"Генерация карты {batchIndex + 1}/{batchTarget} (Seed = {Seed})",
                progress
            );

            if (cancel)
            {
                batchRunning = false;
                EditorApplication.update -= BatchStep;
                EditorUtility.ClearProgressBar();
                Debug.LogWarning($"⏹ Пакетная генерация остановлена пользователем. Сохранено {batchSaved} карт.");
                return;
            }

            // 1) Генерация WFC (полностью, но только ОДНОЙ карты за кадр)
            GenerateImmediateWFC();

            // 2) Генерация тоннеля
            tunnelGenerator.GenerateTunnel(_tilemap, Seed, Bounds, playerSpawn);

            // 3) Валидация
            if (ValidateMap())
            {
                SaveMap();
                batchSaved++;
                Debug.Log($"✔ Карта #{batchIndex + 1} (Seed={Seed}) сохранена. Всего сохранено: {batchSaved}");
            }
            else
            {
                Debug.Log($"⚠ Карта #{batchIndex + 1} (Seed={Seed}) отклонена валидатором");
            }

            batchIndex++;
        }
#endif

        // -------------------------------------------------------
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
