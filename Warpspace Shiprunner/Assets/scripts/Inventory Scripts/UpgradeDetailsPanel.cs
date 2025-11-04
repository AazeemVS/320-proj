using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDetailsPanel : MonoBehaviour
{
  // UI elements for the upgrade info
  [SerializeField] private Image icon;
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text description;

  // Buttons that sit on top of each other
  [Header("Actions")]
  [SerializeField] private Button buyButton;
  [SerializeField] private Button sellButton;

  // NEW: remember what we're showing + raise an event when Buy is pressed
  private UpgradeItem _current;
  public event Action<UpgradeItem> OnBuyRequested; // ShopGridController subscribes

  void Awake()
  {
    // Wire the Buy button to raise an event with the currently-shown item
    if (buyButton != null)
    {
      buyButton.onClick.RemoveAllListeners();
      buyButton.onClick.AddListener(() =>
      {
        if (_current != null) OnBuyRequested?.Invoke(_current);
      });
    }

    HideBothButtons();
    SetDefaultMessage();
  }

  void OnEnable()
  {
    // In case the panel is re-enabled later
    HideBothButtons();
  }

  // Displays the selected upgrade's details
  public void Show(UpgradeItem item)
  {
    _current = item; // <-- keep track of what's selected

    if (item == null) { Clear(); return; }

    if (icon) { icon.enabled = item.icon != null; icon.sprite = item.icon; }
    if (title) title.text = item.displayName;
    if (description) description.text = item.description;

    gameObject.SetActive(true);
  }

  public void Clear()
  {
    _current = null; // <-- clear selection

    if (icon) { icon.enabled = false; icon.sprite = null; }
    if (title) title.text = "";
    SetDefaultMessage();
    HideBothButtons();
  }

  // --- helpers ---
  private void HideBothButtons()
  {
    if (buyButton) buyButton.gameObject.SetActive(false);
    if (sellButton) sellButton.gameObject.SetActive(false);
  }

  private void SetDefaultMessage()
  {
    if (description) description.text = "Select an upgrade to see details.";
  }

  // These are called by your other scripts (keep existing usage)
  public void ShowSellOnly()
  {
    if (sellButton) sellButton.gameObject.SetActive(true);
    if (buyButton) buyButton.gameObject.SetActive(false);
  }

  public void ShowBuyOnly()
  {
    if (buyButton) buyButton.gameObject.SetActive(true);
    if (sellButton) sellButton.gameObject.SetActive(false);
  }
}
