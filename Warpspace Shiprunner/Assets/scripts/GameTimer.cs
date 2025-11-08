using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private player_movement player;
    [SerializeField] public float timeLimit = 30f; // total seconds for the round
    private float remaining;                       // starts equal to timeLimit
    private bool gameEnded = false;

    // expose for UI
    public float Remaining => Mathf.Max(0f, remaining);
    public float Duration => timeLimit;

    void Start()
    {
        remaining = timeLimit; // start full countdown
    }

    void Update()
    {
        if (gameEnded) return;

        remaining -= Time.deltaTime; // 👈 count down

        if (remaining <= 0f)
        {
            remaining = 0f;
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        player_movement.gameRound++;
        Debug.Log("Game Over! Time’s up.");

        // optional: stop time AFTER loading next scene
        SceneManager.LoadScene("WinScene");
    }
}
