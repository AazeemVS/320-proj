using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryButton : MonoBehaviour
{
    public void ButtonPressed()
    {
        // Loads the Inventory UI Page scene
        SceneManager.LoadScene("UpgradeUIPage");
    }
}
