using UnityEngine;
using UnityEngine.UI;

public class Dilation_Bar : MonoBehaviour // vuyi edit
{
    [Header("References")]
    public Player_Controller player; 
    public Image fillImage;            

    public Color activeColor = Color.cyan;                 // bar colour while dilation is active
    public Color rechargingColor = new Color(0.4f, 0.4f, 0.4f); // bar colour while on cooldown
    public Color readyColor = Color.white;                 // bar colour when ready to use

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
    }
}