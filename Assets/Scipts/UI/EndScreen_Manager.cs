using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen_Manager : MonoBehaviour
{
    public GameObject startscreen;
    public static bool startActive = true;

    private void Start() 
    {
        if (startscreen != null) startscreen.SetActive(startActive); 
    }

    public void StartGame()
    {
        Time.timeScale = 1f;
        startscreen.SetActive(false);
        startActive = false;
    }

    public void RestartGame()
    {
        // Restore normal time before reloading
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        // Reloads whichever scene you're currently in
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

   //win screen
    public void RestartFromLevel1()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(0);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}