using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour
{
    public void ButtonPressed()
    {
        // Loads the Inventory UI Page scene
        SceneManager.LoadScene("Player_movement");
    }
}
