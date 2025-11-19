using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Background Theme", menuName = "Game/Background Theme")]
public class BackgroundTheme : ScriptableObject
{
    [Header("Спрайты для слоев")]
    // ЭТИ ИМЕНА ТРЕБУЕТ BackgroundManager!
    public Sprite BackLayerSprite;   
    public Sprite MidLayerSprite;   
    public Sprite FrontLayerSprite; 

    [Header("Цвета")]
    // ЭТО ИМЯ ТРЕБУЕТ BackgroundManager!
    public Color cameraBackgroundColor = Color.cyan; 

    // Временные поля для декораций мы удалили, чтобы не было ошибок
}