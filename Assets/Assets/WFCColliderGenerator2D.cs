using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class WFCColliderGenerator2D : MonoBehaviour
{
    public Transform collidersParent;

    public void Generate2DColliders()
    {
        if (collidersParent == null)
        {
            Debug.LogError("❌ collidersParent is not assigned!");
            return;
        }

        // Удаляем старые коллайдеры
        foreach (Transform child in collidersParent)
            DestroyImmediate(child.gameObject);

        // Ищем MeshRenderer (твои WFC блоки)
        var blocks = GetComponentsInChildren<MeshRenderer>();

        foreach (var block in blocks)
        {
            Bounds b = block.bounds;

            GameObject col = new GameObject("Collider2D");
            col.transform.SetParent(collidersParent);
            col.transform.position = b.center;

            BoxCollider2D bc = col.AddComponent<BoxCollider2D>();
            bc.size = new Vector2(b.size.x, b.size.y);
        }

        Debug.Log("✔ 2D Colliders generated!");

        // Авто-спавн игрока
        var spawner = GetComponent<PlayerSpawn>();
        if (spawner != null)
        {
            Debug.Log("🔥 Calling AutoSpawnPlayer()...");
            spawner.SpawnPlayer();
        }
        else
        {
            Debug.LogWarning("⚠ PlayerSpawn component not found!");
        }
    }
}


#if UNITY_EDITOR
[CustomEditor(typeof(WFCColliderGenerator2D))]
public class WFCColliderGenerator2DEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var gen = (WFCColliderGenerator2D)target;

        if (GUILayout.Button("Generate 2D Colliders"))
        {
            gen.Generate2DColliders();
        }
    }
}
#endif
