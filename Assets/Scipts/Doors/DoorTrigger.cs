using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTrigger : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private Door_Unlocker doorUnlocker;

    [Header("Door Type")]
    [Tooltip("Tick for final door")]
    [SerializeField] private bool isFinalDoor = false;

    // [Header("Win Screen")]
    // [SerializeField] private GameObject winScreen;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!doorUnlocker.IsComplete) return;//checks if the door is unlocked(green)

        if (isFinalDoor)
        {
            //if (winScreen != null) winScreen.SetActive(true);
            //Time.timeScale = 0f;
            //return;
         SceneManager.LoadScene(2);
         return;

        }

        // Level 1 door: load the next scene
        SceneManager.LoadScene(1);
    }
}