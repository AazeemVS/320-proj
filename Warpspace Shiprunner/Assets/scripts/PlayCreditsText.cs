using TMPro;
using UnityEngine;

public class PlayCreditsText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI creditsText;
    [SerializeField] private player_movement player;

    void Update()
    {
        if (creditsText != null)
        {
            creditsText.text = "Credits: " + player_movement.credits.ToString();
        }
    }
}
