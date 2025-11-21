using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    public Slider volumeSlider;
    public Toggle fullscreenToggle;

    [Header("Root panel")]
    public GameObject root; // сама панель настроек

    private void Start()
    {
        LoadSettings();

        root.SetActive(false);
    }

    // Открыть меню
    public void Open()
    {
        root.SetActive(true);
        Time.timeScale = 0f;
    }

    // Закрыть меню
    public void Close()
    {
        root.SetActive(false);
        Time.timeScale = 1f;
        SaveSettings();
    }

    // -------------------------------
    //      Загрузка настроек
    // -------------------------------
    private void LoadSettings()
    {
        float volume = PlayerPrefs.GetFloat("volume", 1f);
        bool fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;

        // Обновляем UI
        volumeSlider.value = volume;
        fullscreenToggle.isOn = fullscreen;

        // Применяем
        AudioListener.volume = volume;
        Screen.fullScreen = fullscreen;
    }

    // -------------------------------
    //      Сохранение настроек
    // -------------------------------
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("volume", volumeSlider.value);
        PlayerPrefs.SetInt("fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    // -------------------------------
    //      UI CALLBACKS
    // -------------------------------
    public void OnVolumeChanged(float v)
    {
        AudioListener.volume = v;
    }

    public void OnFullscreenChanged(bool state)
    {
        Screen.fullScreen = state;
    }
}
