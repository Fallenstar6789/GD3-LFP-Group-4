using UnityEngine;
using UnityEngine.UI;

public class Dilation_Bar : MonoBehaviour 
{
    [Header("References")]
    public Player_Controller player; 
    public Image fillImage;            

    public Color activeColor = Color.cyan;                 // bar colour while dilation is active
    public Color rechargingColor = new Color(0.4f, 0.4f, 0.4f); // bar colour while on cooldown
    public Color readyColor = Color.white;                 // bar colour when ready to use

    [Header("Music")]
    public AudioSource[] songs;

    [Tooltip("Pitch while time dilation is active")]
    public float slowPitch = 0.5f;

    [Tooltip("Normal pitch")]
    public float normalPitch = 1f;

    [Tooltip("How fast the pitch changes")]
    public float pitchChangeSpeed = 5f;

    private void Update()
    {
        if (player == null || fillImage == null) return;

        float fill;

        if (player.IsDilationActive)
        {
            // Full to empty over the dilation duration
            fill = player.DilationTimeRemaining / player.dilationDuration;
            fillImage.color = activeColor;
        }
        else if (player.DilationCooldownRemaining > 0f)
        {
            // Empty to full over the cooldown
            fill = 1f - (player.DilationCooldownRemaining / player.dilationCooldown);
            fillImage.color = rechargingColor;
        }
        else
        {
            // Ready to use
            fill = 1f;
            fillImage.color = readyColor;
        }

        fillImage.fillAmount = Mathf.Clamp01(fill);

        // Slow the music down while dilation is active
        float targetPitch = player.IsDilationActive ? slowPitch : normalPitch;

        foreach (AudioSource song in songs)
        {
            if (song == null) continue;

            song.pitch = Mathf.Lerp(
                song.pitch,
                targetPitch,
                Time.unscaledDeltaTime * pitchChangeSpeed
            );
        }
    }
}