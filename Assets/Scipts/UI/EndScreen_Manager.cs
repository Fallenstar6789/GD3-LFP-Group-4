using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen_Manager : MonoBehaviour
{
    public GameObject startscreen;//vuyi edit
    public static bool startActive = true;//vuyi edit

     private void Start() // vuyi edit
    {
        startscreen.SetActive(startActive); // vuyi edit
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

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;

        Application.Quit();
    }
}