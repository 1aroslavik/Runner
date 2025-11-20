using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public string gameSceneName = "SampleScene";

    public void Play()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}
