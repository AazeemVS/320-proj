using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
public enum GameState {
    Start,
    Playing,
    GameOver,
    Inventory,
    Menu,
    Pause
}
public sealed class GameStateManager : MonoBehaviour
{
    private static GameStateManager instance = null;
    private static readonly object instanceLock = new object();
    [SerializeField] private GameState state = GameState.Start;
    private GameState targetState = GameState.Start;

    public static GameStateManager Instance {
        get {
            lock (instanceLock) {
                if (instance == null) {
                    GameObject StateManagerObject = new GameObject();
                    instance = StateManagerObject.AddComponent<GameStateManager>();
                }
                return instance;
            }
        }
    }
    private void Awake() {
        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        TryGetState();
    }

    // Update is called once per frame
    void Update()
    {
        switch (state) {
            case GameState.Start:
                if (Input.GetKey(KeyCode.Return)) {
                    LoadInventory();
                }
                break;
            case GameState.Playing:
                break;
            case GameState.GameOver:
                if (Input.GetKey(KeyCode.Return)) {
                    ChangeState(GameState.Playing);
                }
                if (Input.GetKey(KeyCode.Escape)) {
                    ChangeState(GameState.Start);
                }
                break;
            case GameState.Inventory:
                if (Input.GetKey(KeyCode.Return)) {
                    ChangeState(GameState.Playing);
                } 
                break;
        }
    }
    public void RequestSceneChange(GameState senderState, GameState newState) {
        if (state != senderState) return;
        ChangeState(newState);
    }
    private void ChangeState(GameState newState) {
        switch (newState) {
            case GameState.Start:
                LoadStart();
                break;
            case GameState.Playing:
                LoadPlaying();
                break;
            case GameState.GameOver:
                LoadGameOver();
                break;
        }
    }
    private void LoadPlaying() {
        SceneManager.LoadScene("player_movement");
        state = GameState.Playing;
    }

    private void LoadGameOver() {
        SceneManager.LoadScene("GameOverScreen");
        state = GameState.GameOver;
    }
    private void LoadInventory() {
        SceneManager.LoadScene("UpgradeUIPage");
        state = GameState.Inventory;
    }
    private void LoadStart() {
        SceneManager.LoadScene("StartMenu");
        state = GameState.Start;
    }

    private void TryGetState() {
        string sceneName = SceneManager.GetActiveScene().name;
        switch (sceneName) {
            case "player_movement":
                state = GameState.Playing;
                break;
            case "StartMenu":
                state = GameState.Start;
                break;
            case "GameOverScene":
                state = GameState.GameOver;
                break;
        }
    }
}
