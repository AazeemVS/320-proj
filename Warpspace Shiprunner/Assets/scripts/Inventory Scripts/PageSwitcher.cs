using UnityEngine;
using UnityEngine.UI;

public class PageSwitcher : MonoBehaviour
{
  [SerializeField] private GameObject defaultBg;
  [SerializeField] private GameObject gearBg;
  [SerializeField] private GameObject tradeBg;
  [SerializeField] private GameObject shopBg;

  void Start() { ShowDefault(); DisableRaycasts(); }

  void DisableRaycasts()
  {
    foreach (var go in new[] { defaultBg, gearBg, tradeBg, shopBg })
      if (go && go.TryGetComponent<Image>(out var img))
        img.raycastTarget = false;
  }

  public void ShowDefault() => SetOnly(defaultBg);
  public void ShowGear() => SetOnly(gearBg);
  public void ShowTrade() => SetOnly(tradeBg);
  public void ShowShop() => SetOnly(shopBg);

  void SetOnly(GameObject target)
  {
    defaultBg?.SetActive(false);
    gearBg?.SetActive(false);
    tradeBg?.SetActive(false);
    shopBg?.SetActive(false);
    target?.SetActive(true);
  }
}
