using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
  // References to each background/page object
  [SerializeField] private GameObject defaultBg;
  [SerializeField] private GameObject gearBg;
  [SerializeField, CanBeNull] private GameObject tradeBg;
  [SerializeField, CanBeNull] private GameObject shopBg;

    // Called when the scene starts
    void Start()
  {
    ShowDefault();     // Start with the default page visible
    DisableRaycasts(); // Prevent background images from blocking clicks
  }

  // Turns off raycast blocking for all background images
  void DisableRaycasts()
  {
    foreach (var go in new[] { defaultBg, gearBg, tradeBg, shopBg })
      if (go && go.TryGetComponent<Image>(out var img))
        img.raycastTarget = false;
  }

  // Functions to show each page
  public void ShowDefault() => SetOnly(defaultBg);
  public void ShowGear() => SetOnly(gearBg);
  public void ShowTrade() => SetOnly(tradeBg);
  public void ShowShop() => SetOnly(shopBg);

  // Activates one page and hides all others
  void SetOnly(GameObject target)
  {
    defaultBg?.SetActive(false);
    gearBg?.SetActive(true);
    tradeBg?.SetActive(false);
    shopBg?.SetActive(false);
    target?.SetActive(true);
  }
}
