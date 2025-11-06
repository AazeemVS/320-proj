using System;                      // for Action<>
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeDetailsPanel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    [Header("Action Buttons")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    [Header("Price Labels (optional)")]
    [SerializeField] private TMP_Text buyText;   // e.g. “Buy: 300”
    [SerializeField] private TMP_Text sellText;  // e.g. “Sell: 150”

    // Events (if you’ve been using them already)
    public event Action<UpgradeItem> OnBuyRequested;
    public event Action<UpgradeItem> OnSellRequested;

    // Keep track of which item the panel is showing
    public UpgradeItem CurrentItem { get; private set; }

    void Awake()
    {
        HideBothButtons();
        HidePrices();
        SetDefaultMessage();

        if (buyButton)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() =>
            {
                if (CurrentItem == null) return;

                // 1) Try to purchase directly (robust across rounds)
                var inv = InventoryManager.Instance ?? FindObjectOfType<InventoryManager>(true);
                if (inv != null && inv.TryPurchase(CurrentItem, out var created))
                {
                    // 2) Ask whatever ShopGridController is active to remove that card
                    var shop = FindObjectOfType<ShopGridController>(true);
                    if (shop != null) shop.RemoveCardFor(CurrentItem.id);

                    // 3) Reset panel UI
                    Clear();
                }
                else
                {
                    Debug.Log("[Panel/Buy] Purchase failed (credits or capacity).");
                }
            });
        }

        if (sellButton)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(() =>
            {
                if (CurrentItem != null) OnSellRequested?.Invoke(CurrentItem);
            });
        }
    }

    void OnEnable()
    {
        // Don’t flash the wrong controls when re-enabled
        HideBothButtons();
        HidePrices();
    }

    // ---------- Public API (backwards compatible) ----------

    // Your existing generic Show
    public void Show(UpgradeItem item)
    {
        CurrentItem = item;
        if (item == null) { Clear(); return; }

        if (icon) { icon.enabled = item.icon != null; icon.sprite = item.icon; }
        if (title) title.text = item.displayName;
        if (description) description.text = item.description;

        gameObject.SetActive(true);
        // Do not force buttons; caller decides (ShowBuyOnly / ShowSellOnly)
    }

    public void Clear()
    {
        CurrentItem = null;
        if (icon) { icon.enabled = false; icon.sprite = null; }
        if (title) title.text = "";
        SetDefaultMessage();
        HideBothButtons();
        HidePrices();
    }

    public void ShowBuyOnly()
    {
        if (buyButton) buyButton.gameObject.SetActive(true);
        if (sellButton) sellButton.gameObject.SetActive(false);

        // If we know the item, populate Buy label
        if (CurrentItem != null)
        {
            int price = SafeBuyValue(CurrentItem);
            SetBuyText(price);
            ClearSellText();
        }
        else
        {
            HidePrices();
        }
    }

    public void ShowSellOnly()
    {
        if (sellButton) sellButton.gameObject.SetActive(true);
        if (buyButton) buyButton.gameObject.SetActive(false);

        if (CurrentItem != null)
        {
            int sell = SafeSellValue(CurrentItem);
            SetSellText(sell);
            ClearBuyText();
        }
        else
        {
            HidePrices();
        }
    }

    // ---------- Convenience helpers (optional to use) ----------

    // Call this from shop item clicks when you already know the price shown in the shop
    public void ShowShop(UpgradeItem item, int price)
    {
        Show(item);
        if (buyButton) buyButton.gameObject.SetActive(true);
        if (sellButton) sellButton.gameObject.SetActive(false);

        SetBuyText(price);
        ClearSellText();
    }

    // Call this from inventory selection if you want it in one line
    public void ShowInventory(UpgradeItem item)
    {
        Show(item);
        if (sellButton) sellButton.gameObject.SetActive(true);
        if (buyButton) buyButton.gameObject.SetActive(false);

        int sell = SafeSellValue(item);
        SetSellText(sell);
        ClearBuyText();
    }

    // ---------- Internal helpers ----------

    private void HideBothButtons()
    {
        if (buyButton) buyButton.gameObject.SetActive(false);
        if (sellButton) sellButton.gameObject.SetActive(false);
    }

    private void HidePrices()
    {
        if (buyText) buyText.gameObject.SetActive(false);
        if (sellText) sellText.gameObject.SetActive(false);
    }

    private void ClearBuyText()
    {
        if (buyText) { buyText.text = ""; buyText.gameObject.SetActive(false); }
    }

    private void ClearSellText()
    {
        if (sellText) { sellText.text = ""; sellText.gameObject.SetActive(false); }
    }

    private void SetBuyText(int price)
    {
        if (!buyText) return;
        buyText.text = $"Buy: {price}";
        buyText.gameObject.SetActive(true);
    }

    private void SetSellText(int value)
    {
        if (!sellText) return;
        sellText.text = $"Sell: {value}";
        sellText.gameObject.SetActive(true);
    }

    private void SetDefaultMessage()
    {
        if (description) description.text = "Select an upgrade to see details.";
    }

    private static int SafeBuyValue(UpgradeItem item)
    {
        // Falls back to 0 if no upgrade/value
        return (item != null && item.upgrade != null) ? item.upgrade.value : 0;
    }

    private static int SafeSellValue(UpgradeItem item)
    {
        int buy = SafeBuyValue(item);
        return Mathf.FloorToInt(buy * 0.5f);
    }
}
