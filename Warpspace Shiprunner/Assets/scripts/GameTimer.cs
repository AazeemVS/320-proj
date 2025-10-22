using UnityEngine;
using UnityEngine.SceneManagement; // Only needed if you use a separate scene

public class GameTimer : MonoBehaviour
{
    public float timeLimit = 30f; // seconds
    private float elapsedTime = 0f;
    private bool gameEnded = false;

    void Update()
    {
        if (gameEnded) return;

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= timeLimit)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        gameEnded = true;
        Debug.Log("Game Over! Time’s up.");

        // Loads the win scene
        SceneManager.LoadScene("WinScene");

        // Stops all gameplay logic
        Time.timeScale = 0f;
    }
}
