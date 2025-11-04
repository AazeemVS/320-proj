using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
  [SerializeField] private Image icon;
  [SerializeField] private TMP_Text title;      // optional (can be left unassigned)
  [SerializeField] private TMP_Text priceText;  // optional (can be left unassigned)

  private UpgradeItem item;
  private UpgradeDetailsPanel detailsPanel;

  public UpgradeItem Item => item; // so the shop can look this up

  public void Bind(UpgradeItem i, UpgradeDetailsPanel panel)
  {
    item = i;
    detailsPanel = panel;
    RefreshUI();
  }

  private void RefreshUI()
  {
    if (icon)
    {
      bool hasSprite = item != null && item.icon != null;
      icon.enabled = hasSprite;
      icon.sprite = hasSprite ? item.icon : null;
      icon.preserveAspect = true;  // <-- keeps the icon nicely scaled
    }

    if (title) title.text = ""; // not displaying titles in the grid
    if (priceText) priceText.text = ""; // and not showing price in the grid
  }

  public void OnPointerClick(PointerEventData e)
  {
    if (!detailsPanel || item == null) return;
    detailsPanel.Show(item);
    detailsPanel.ShowBuyOnly(); // <- panel shows Buy button
  }
}
