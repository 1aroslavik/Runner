using UnityEngine;

public class InfiniteParallax : MonoBehaviour
{
    public Camera cam;
    [Range(0f, 1f)]
    public float parallaxEffect = 0.5f; 
    
    // ЭТО ПОЛЕ НУЖНО ДЛЯ BackgroundManager, чтобы он мог менять спрайты!
    [HideInInspector] 
    public SpriteRenderer spriteRenderer; 
    
    // public List<GameObject> generatedDecorations = new List<GameObject>(); // Это пригодится для генерации

    private float length; 
    private float startpos; 

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        
        // ПОЛУЧАЕМ КОМПОНЕНТ SpriteRenderer ПРИ СТАРТЕ
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            // Здесь не должно быть ошибки, так как менеджер будет назначать спрайты.
            // Но мы оставим проверку на всякий случай.
            Debug.LogWarning($"InfiniteParallax на {gameObject.name} не нашел SpriteRenderer.", this);
            return;
        }
    }

    void Start()
    {
        // Перенесено из Awake, чтобы избежать ошибок, если WFC меняет размер спрайта
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            startpos = transform.position.x;
            // Получаем ширину спрайта для зацикливания
            length = spriteRenderer.bounds.size.x;
        }
    }

    void FixedUpdate()
    {
        // Проверяем, что камера и длина существуют
        if (cam == null || length == 0) return;

        // Вычисляем, насколько мы сместились относительно камеры
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);

        // Двигаем фон
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // Магия бесконечности: если камера ушла далеко вправо, "прыгаем" фоном вперед
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
    
    // Этот метод вам понадобится, когда вы вернете генерацию декораций
    /*
    public void ClearDecorations()
    {
        foreach (GameObject deco in generatedDecorations)
        {
            Destroy(deco);
        }
        generatedDecorations.Clear();
    }
    */
}