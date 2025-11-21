using UnityEngine;

public class MenuParallax : MonoBehaviour
{
    [Header("Слой фона")]
    public float parallaxMultiplier = 0.1f; // Множитель: 0.1f - медленно, 1.0f - быстро

    private Vector2 startPosition;

    void Start()
    {
        // Запоминаем начальную позицию объекта
        startPosition = transform.position;
    }

    void Update()
    {
        // 1. Получаем позицию мыши в мировых координатах (от 0 до Screen.width/height)
        Vector2 mousePosition = Input.mousePosition;

        // 2. Нормализуем позицию мыши (от 0 до 1)
        // Для удобства, сделаем так, чтобы центр экрана был (0.5, 0.5)
        Vector2 normalizedMouse = new Vector2(
            mousePosition.x / Screen.width - 0.5f,
            mousePosition.y / Screen.height - 0.5f
        );

        // 3. Вычисляем смещение
        Vector2 offset = normalizedMouse * parallaxMultiplier;

        // 4. Применяем смещение к начальной позиции
        transform.position = startPosition + offset;
    }
}