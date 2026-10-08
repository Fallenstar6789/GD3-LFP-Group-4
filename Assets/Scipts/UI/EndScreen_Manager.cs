using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen_Manager : MonoBehaviour
{
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