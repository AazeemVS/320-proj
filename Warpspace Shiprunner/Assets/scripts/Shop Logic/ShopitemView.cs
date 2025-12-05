using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
  [Header("Icon-only card UI")]
  [SerializeField] private Image icon;
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text priceText;
  [SerializeField] private Button buyButton;

  [Header("Details Panel")]
  [SerializeField] private UpgradeDetailsPanel detailsPanel;

  // runtime
  private UpgradeItem item;
  private int _price;
  private Action<UpgradeItem, ShopItemView> _onBuy;

  public UpgradeItem Item => item; // lets the shop look this view up

  /// <summary>
  /// Bind this card. If you buy only from the details panel, pass `null` for onBuy
  /// and leave `buyButton` unassigned on the prefab.
  /// </summary>
  public void Bind(UpgradeItem i, int price, Action<UpgradeItem, ShopItemView> onBuy)
  {
    item = i;
    _price = price;
    _onBuy = onBuy;

    // wire card-level Buy if a button exists, and a callback is supplied
    if (buyButton != null)
    {
      buyButton.onClick.RemoveAllListeners();
      buyButton.onClick.AddListener(() => _onBuy?.Invoke(item, this));
    }

    RefreshUI();
  }

  private void Awake()
  {
    // auto find the panel if not set
    if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);
  }

  private void OnValidate() => RefreshUI();

  private void RefreshUI()
  {
    if (icon)
    {
      bool hasSprite = (item != null && item.icon != null);
      icon.enabled = hasSprite;
      icon.sprite = hasSprite ? item.icon : null;
      icon.preserveAspect = true;
    }

    if (title) title.text = "";
    if (priceText) priceText.text = "";
  }

  // Clicking the card shows the details panel with the Buy button
  public void OnPointerClick(PointerEventData e)
  {
    if (!detailsPanel || item == null) return;

    detailsPanel.Show(item);
    detailsPanel.ShowBuyOnly();
  }
}
