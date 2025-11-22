using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;

    public GameObject pauseUI; // сам Canvas PauseMenu
    private bool isPaused = false;

    void Awake()
    {
        Instance = this;
        pauseUI.SetActive(false); // скрыть меню при старте
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        pauseUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Resume()
    {
        pauseUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void SaveGame()
    {
        PermanentStats.Instance.SavePermanentData();
        Debug.Log("✔ Игра сохранена");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
