using UnityEngine;

public class ButtonFunctionality : MonoBehaviour
{
    public GameObject gearPage;
    public GameObject tradePage;
    public GameObject shopPage;
    public GameObject inventoryMenu;


    void Start()
    {
        ShowPage(gearPage); // Inventory page starts on Gear page
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            inventoryMenu.SetActive(!inventoryMenu.activeSelf);
    }

    public void ShowPage(GameObject pageToShow)
    {
        gearPage.SetActive(false);
        tradePage.SetActive(false);
        shopPage.SetActive(false);
        pageToShow.SetActive(true);
    }
}
