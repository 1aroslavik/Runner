using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(WFC_ColliderBaker))]
public class WFC_ColliderBakerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Bake Colliders"))
        {
            ((WFC_ColliderBaker)target).Bake();
        }
    }
}
#endif

public class WFC_ColliderBaker : MonoBehaviour
{
    public Transform collidersParent;

    public void Bake()
    {
        if (collidersParent == null)
        {
            Debug.LogError("❌ collidersParent is not assigned!");
            return;
        }

        // Удаляем старые коллайдеры
        foreach (Transform c in collidersParent)
            DestroyImmediate(c.gameObject);

        // Находим все SpriteRenderer в сгенерированной WFC карте
        var renderers = GetComponentsInChildren<SpriteRenderer>();

        foreach (var r in renderers)
        {
            GameObject g = new GameObject("COL");
            g.transform.parent = collidersParent;

            var bc = g.AddComponent<BoxCollider2D>();

            bc.size = r.bounds.size;
            bc.transform.position = r.bounds.center;
        }

        Debug.Log("✔ Colliders baked!");
    }
}
