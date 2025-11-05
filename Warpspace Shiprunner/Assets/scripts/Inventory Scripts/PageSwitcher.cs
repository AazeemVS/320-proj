using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
  [SerializeField] private GameObject gearBg;

  void Start()
  {
    // Always show the gear page
    ShowGear();
    DisableRaycasts();
  }

  private void DisableRaycasts()
  {
    if (gearBg && gearBg.TryGetComponent<Image>(out var img))
      img.raycastTarget = false;
  }

  public void ShowGear()
  {
    if (gearBg) gearBg.SetActive(true);
  }
}
