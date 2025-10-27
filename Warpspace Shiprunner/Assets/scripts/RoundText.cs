using TMPro;
using UnityEngine;

public class RoundText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private player_movement player;

    void Update()
    {
        if (roundText != null)
        {
            roundText.text = "Round " + player_movement.gameRound.ToString();
        }
    }
}
