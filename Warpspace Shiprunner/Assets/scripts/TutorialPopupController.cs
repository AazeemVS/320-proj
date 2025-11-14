using UnityEngine;

public class TutorialPopupController : MonoBehaviour
{
  [SerializeField] private GameObject popupOverlay;
  [SerializeField] private GameObject popupPanel;

  public void ShowPopup()
  {
    popupOverlay.SetActive(true);
    popupPanel.SetActive(true);
  }

  public void HidePopup()
  {
    popupOverlay.SetActive(false);
    popupPanel.SetActive(false);
  }
}
