using UnityEngine;

public class DoorTrigger : MonoBehaviour//vuyi edit
{
    [Header("Unlock Check")]
    [SerializeField] private Door_Unlocker doorUnlocker;

    [Header("Door Type")]
    [Tooltip("Tick for final door")]
    [SerializeField] private bool isFinalDoor = false;

    [Header("Levels")]
    [SerializeField] private GameObject level1;
    [SerializeField] private GameObject level2;

    [Header("Player")]
    [SerializeField] private Vector2 startPosition;

    [Header("Win Screen (on final door only)")]
    [SerializeField] private GameObject winScreen;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!doorUnlocker.IsComplete) return;//checks if the door is unlocked(green)

        if (isFinalDoor)
        {
            if (winScreen != null) winScreen.SetActive(true);
            Time.timeScale = 0f;
            return;
        }

        // Level 1 door: switch levels
        level1.SetActive(false);
        level2.SetActive(true);

        // Remember that we're on level 2 so a restart returns here
        EndScreen_Manager.SetLevel2(startPosition);

        // Move the player back to the start position
        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.position = startPosition;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            other.transform.position = startPosition;
        }
    }
}