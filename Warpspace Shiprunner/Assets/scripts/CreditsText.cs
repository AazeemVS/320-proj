using UnityEngine;
using TMPro;

public class CreditsText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI roundCreditsText;
    [SerializeField] private player_movement player;

    void Update()
    {
        if (creditsText != null)
        {
            creditsText.text = "Total credits: " + player_movement.credits.ToString();
        }

        if (roundCreditsText != null)
        {
            roundCreditsText.text = "Credits this round: " + player_movement.roundCredits.ToString();
        }
        
    }
}
