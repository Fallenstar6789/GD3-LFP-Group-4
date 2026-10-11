using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Header("Unlock Check")]
    [SerializeField] private Door_Unlocker doorUnlocker;

    [Header("Levels")]
    [SerializeField] private GameObject level1;
    [SerializeField] private GameObject level2;

    [Header("Player")]
    [SerializeField] private Vector2 startPosition;

   

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (!doorUnlocker.IsComplete) return;//checks if the door is unlocked(green)

        level1.SetActive(false);
        level2.SetActive(true);

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