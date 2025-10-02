using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    private GameStateManager stateManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stateManager = GameStateManager.Instance;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
