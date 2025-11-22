using UnityEngine;
using UnityEngine.UI;

public class BossHPSpriteBarUI : MonoBehaviour
{
    public BossHealth health;
    public RectTransform fill;  // HPFill (UI)

    private float maxWidth;

    void Start()
    {
        maxWidth = fill.sizeDelta.x;
    }

    void Update()
    {
        float t = (float)health.currentHealth / health.maxHealth;

        fill.sizeDelta = new Vector2(maxWidth * t, fill.sizeDelta.y);

        // Полоска смотрит на камеру
        transform.rotation = Camera.main.transform.rotation;
    }
}
