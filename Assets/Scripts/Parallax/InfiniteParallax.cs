using UnityEngine;
using System.Collections.Generic; // Не используется, но можно оставить

public class InfiniteParallax : MonoBehaviour
{
    public Camera cam;
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f; 
    
    // Поле для BackgroundManager
    [HideInInspector] 
    public SpriteRenderer spriteRenderer; 
    
    // Публичное поле для декораций (можно временно закомментировать)
    // public List<GameObject> generatedDecorations = new List<GameObject>(); 

    private float length; 
    private float startpos; 

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        
        // Получаем SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>(); 

        if (spriteRenderer == null)
        {
            Debug.LogError($"[ПАРАЛЛАКС]: На объекте {gameObject.name} отсутствует компонент SpriteRenderer! Фон не будет работать.", this);
        }
    }

    void Start()
    {
        // Выполняется после Awake и после того, как BackgroundManager назначит спрайт
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            startpos = transform.position.x;
            // Получаем ширину спрайта для зацикливания
            length = spriteRenderer.bounds.size.x;
        } else {
            // Если спрайт не назначен, значит, менеджер не сработал или спрайт не загружен
            Debug.LogWarning($"InfiniteParallax на {gameObject.name}: Спрайт отсутствует при старте. Движение не будет работать.", this);
        }
    }

    void FixedUpdate()
    {
        // Проверяем, что камера и длина существуют
        if (cam == null || length == 0) return;

        // Вычисляем смещение и двигаем фон
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Магия бесконечности
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
    
    // Метод для очистки декораций (когда понадобится)
    /*
    public void ClearDecorations()
    {
        // ...
    }
    */
}