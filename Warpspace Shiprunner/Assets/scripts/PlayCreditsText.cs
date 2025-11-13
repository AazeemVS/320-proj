using TMPro;
using UnityEngine;

public class PlayCreditsText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private TextMeshProUGUI creditsInventoryText;
    [SerializeField] private player_movement player;

    void Update()
    {
        if (creditsText != null)
        {
            creditsText.text = player_movement.credits.ToString();
        }
        if (creditsInventoryText != null)
        {
            creditsInventoryText.text = player_movement.credits.ToString();
        }
        else
        {
                creditsInventoryText.text = "0";
        }
    }
}
