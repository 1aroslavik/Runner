using UnityEngine;
using System.Collections.Generic;

public class BackgroundManager : MonoBehaviour
{
    [Header("Ссылки на компоненты слоев")]
    // Обновленные имена переменных для Инспектора
    public InfiniteParallax BackLayer;
    public InfiniteParallax MidLayer;
    public InfiniteParallax FrontLayer;

    [Header("Параметры уровня")]
    public Transform playerTransform; 
    public float levelWidth = 50f; 

    [Header("Список всех возможных тем")]
    public List<BackgroundTheme> themes;

    private BackgroundTheme currentTheme;

    void Start()
    {
        // Проверяем, есть ли менеджер на сцене, и назначаем камеру для параллакс-скриптов
        if (Camera.main != null)
        {
            Camera cam = Camera.main;
            if(BackLayer != null) BackLayer.cam = cam;
            if(MidLayer != null) MidLayer.cam = cam;
            if(FrontLayer != null) FrontLayer.cam = cam;
        }

        ChangeBackground();
    }

    public void ChangeBackground()
    {
        if (themes.Count == 0) 
        {
            Debug.LogError("Список тем пуст!");
            return;
        }

        // 1. Выбираем случайную тему
        int randomIndex = Random.Range(0, themes.Count);
        currentTheme = themes[randomIndex];

        Debug.Log($"Применена тема: {currentTheme.name}");

        // 2. ПРИМЕНЯЕМ СЛОИ, ИСПОЛЬЗУЯ КОРРЕКТНЫЕ ИМЕНА ПОЛЕЙ ИЗ BackgroundTheme.cs
        // Мы используем BackLayerSprite, MidLayerSprite и FrontLayerSprite
        
        ApplyThemeToLayer(BackLayer, currentTheme.BackLayerSprite, 0.9f, false); // Дальний (Back)
        ApplyThemeToLayer(MidLayer, currentTheme.MidLayerSprite, 0.5f, false);  // Средний
        ApplyThemeToLayer(FrontLayer, currentTheme.FrontLayerSprite, 0.2f, false); // Ближний (Front)
        
        // 3. Меняем цвет фона камеры
        if (Camera.main != null) 
            Camera.main.backgroundColor = currentTheme.cameraBackgroundColor;

        // ВРЕМЕННО ОТКЛЮЧАЕМ ГЕНЕРАЦИЮ ДЕКОРАЦИЙ
    }

    void ApplyThemeToLayer(InfiniteParallax layer, Sprite sprite, float parallax, bool canClear)
    {
        if (layer == null) return;
        
        // Временно пропускаем очистку декораций
        // if (canClear) { layer.ClearDecorations(); } 

        // Применяем спрайт и скорость
        if (layer.spriteRenderer != null) 
        {
            layer.spriteRenderer.sprite = sprite;
        } else {
             Debug.LogWarning($"SpriteRenderer не найден на {layer.gameObject.name}. Параллакс не будет работать корректно.");
        }
        
        layer.parallaxEffect = parallax; 
    }
}