using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen_Manager : MonoBehaviour
{
    public GameObject startscreen;//vuyi edit
    public static bool startActive = true;//vuyi edit

    [Header("Scenes")]
    [SerializeField] private string level1SceneName = "Level1";

    private void Start() // vuyi edit
    {
        if (startscreen != null) startscreen.SetActive(startActive); // vuyi edit
    }// vuyi edit

    public void StartGame()//vuyi edit
    {
        Time.timeScale = 1f;
        startscreen.SetActive(false);
        startActive = false;
    }//vuyi edit

    public void RestartGame()
    {
        // Restore normal time before reloading
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Reloads whichever scene you're currently in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // vuyi edit:win screen
    public void RestartFromLevel1()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(level1SceneName);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}