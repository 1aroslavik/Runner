using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "SampleScene";

    public GameObject creditsPanel;

    // ==============================
    //         NEW GAME
    // ==============================
    public void PlayNewGame()
    {
        Debug.Log("🔥 NEW GAME — удаляем сохранения и запускаем заново!");

        // 1. Полностью очистить PlayerPrefs (если есть — удалит, если нет — просто ничего)
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 2. Сбросить PermanentStats в значения по умолчанию
        if (PermanentStats.Instance != null)
            PermanentStats.Instance.ResetAllData();

        // 3. Загрузить сцену игры
        SceneManager.LoadScene(gameSceneName);
    }

    // ==============================
    //         CONTINUE
    // ==============================
    public void ContinueGame()
    {
        Debug.Log("▶ CONTINUE — загрузка игры");

        // Продолжение просто загружает сцену с уже сохранёнными статами
        SceneManager.LoadScene(gameSceneName);
    }

    // ==============================
    //         CREDITS
    // ==============================
    public void ShowCredits()
    {
        creditsPanel.SetActive(true);
    }

    public void HideCredits()
    {
        creditsPanel.SetActive(false);
    }

    // ==============================
    //         EXIT GAME
    // ==============================
    public void QuitGame()
    {
        Debug.Log("❌ Игра закрыта");
        Application.Quit();
    }
}
