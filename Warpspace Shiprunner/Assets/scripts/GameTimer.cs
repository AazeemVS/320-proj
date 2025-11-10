using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private player_movement player;
    [SerializeField] private EnemySpawner spawner;     // 👈 add this
    [SerializeField] public float timeLimit = 40f;     // rounds last 40s before flee
    private float remaining;
    private bool gameEnded = false;
    private bool waitingForClear = false;

    public float Remaining => Mathf.Max(0f, remaining);
    public float Duration => timeLimit;

    void Start()
    {
        remaining = timeLimit;

        // Safe fallback if not wired in inspector
        if (spawner == null)
            spawner = FindAnyObjectByType<EnemySpawner>();
    }

    void Update()
    {
        if (gameEnded) return;

        // If we are in the post-timer phase, wait for board clear
        if (waitingForClear)
        {
            if (spawner == null || spawner.IsFieldClear())
            {
                EndGame();
            }
            return;
        }

        // Normal countdown
        remaining -= Time.deltaTime;

        if (remaining <= 0f)
        {
            remaining = 0f;

            // Enter flee phase once
            if (spawner != null && !spawner.InFleePhase)
            {
                spawner.EnterFleePhaseExternal();
            }

            // Now we wait until enemies are gone/fled
            waitingForClear = true;
        }
    }

    void EndGame()
    {
        gameEnded = true;
        player_movement.gameRound++;

        // If you instead use a GameStateManager, call that here.
        SceneManager.LoadScene("WinScene");
    }
}
