using UnityEngine;
using UnityEngine.SceneManagement;

public class EndScreen_Manager : MonoBehaviour
{
    public GameObject startscreen;//vuyi edit
    public static bool startActive = true;//vuyi edit

    [Header("Levels")]
    public GameObject level1;   
    public GameObject level2;   
    public Transform player;    

    // Static = survives scene reloads
    public static int currentLevel = 1;
    public static Vector2 level2Spawn;

    private void Start() // vuyi edit
    {
        startscreen.SetActive(startActive); // vuyi edit

        // Re-apply the level the player was on before the reload
        if (level1 != null) level1.SetActive(currentLevel == 1);
        if (level2 != null) level2.SetActive(currentLevel == 2);

        // Put the player back at the level 2 spawn
        if (currentLevel == 2 && player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = level2Spawn;
                rb.linearVelocity = Vector2.zero;
            }
            player.position = level2Spawn;
        }
    }// vuyi edit

    public void StartGame()//vuyi edit
    {
        Time.timeScale = 1f;
        startscreen.SetActive(false);
        startActive = false;
    }//vuyi edit

    // vuyi edit: Called by DoorTrigger when the player enters level 2
    public static void SetLevel2(Vector2 spawn)
    {
        currentLevel = 2;
        level2Spawn = spawn;
    }

    public void RestartGame()
    {
        // Restore normal time before reloading
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // vuyi edit:win screen
    public void RestartFromLevel1()
    {
        currentLevel = 1;
        RestartGame();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        currentLevel = 1;
        Application.Quit();
    }
}