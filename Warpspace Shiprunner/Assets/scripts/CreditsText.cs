using UnityEngine;
using TMPro;

public class CreditsText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText; // Assign this in the Inspector
    [SerializeField] private player_movement player; // Drag your player object here

    void Update()
    {
        if (player != null && creditsText != null)
        {
            creditsText.text = "Credits: " + Mathf.FloorToInt(player.credits).ToString();
        }
    }
}
