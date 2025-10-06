using UnityEngine;

public class StartMenuManager : MonoBehaviour
{
    //temporary class to instantiate important persistent manager scripts 
    private GameStateManager stateManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //instantiate game state manager
        stateManager = GameStateManager.Instance;
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
