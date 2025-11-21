using UnityEngine;

public class ParallaxLooper : MonoBehaviour
{
    [Tooltip("Скорость движения слоя. Для дальних слоев - низкая, для ближних - высокая.")]
    public float scrollSpeed = 1f; 

    private Vector3 startPosition;
    private float layerWidth; // Ширина спрайта

    void Start()
    {
        // Запоминаем начальную позицию контейнера (родителя)
        startPosition = transform.position;

        // Находим ширину, используя ПЕРВЫЙ дочерний спрайт (Child(0)).
        SpriteRenderer spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            // Получаем ширину в игровых единицах
            layerWidth = spriteRenderer.bounds.size.x;
        }
        else
        {
            Debug.LogError("Скрипт ParallaxLooper должен быть прикреплен к родителю, содержащему два спрайта.");
            enabled = false;
        }
    }

    void Update()
    {
        // 1. Движение всего контейнера (двух спрайтов)
        // Двигаем влево (если хотите вправо, замените Vector3.left на Vector3.right)
        transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime); 

        // 2. Проверка зацикливания.
        // Если контейнер сдвинулся влево на расстояние, равное ширине одного спрайта.
        // То есть, Mountain_A полностью ушел за границу.
        if (transform.position.x < startPosition.x - layerWidth)
        {
            // Сбрасываем позицию вправо, чтобы он появился после второго спрайта.
            // Это мгновенный "прыжок", который не виден, потому что его скрывает второй спрайт.
            transform.position = new Vector3(
                transform.position.x + layerWidth, 
                transform.position.y, 
                transform.position.z
            );
        }
    }
}