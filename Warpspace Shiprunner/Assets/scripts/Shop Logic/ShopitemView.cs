using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    [SerializeField] private UpgradeItem item;

    [Header("Card UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("References")]
    [SerializeField] private UpgradeDetailsPanel detailsPanel; // expects the component in scene

    // runtime state
    private int _price;
    private Action<UpgradeItem> _onBuy;

    // ----- Controller calls this -----
    public void Bind(UpgradeItem i, int price, Action<UpgradeItem> onBuy)
    {
        item = i;
        _price = price;
        _onBuy = onBuy;

        // wire button
        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => _onBuy?.Invoke(item));
        }

        RefreshUI();
    }

    // Optional: if you spawn manually elsewhere and only need details hookup
    public void Init(UpgradeItem i, UpgradeDetailsPanel panel)
    {
        item = i;
        detailsPanel = panel;
        _price = (item != null && item.upgrade != null) ? item.upgrade.value : _price;
        RefreshUI();
    }

    private void Awake()
    {
        if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);
    }

    private void RefreshUI()
    {
        if (icon != null)
        {
            var hasSprite = (item != null && item.icon != null);
            icon.enabled = hasSprite;
            icon.sprite = hasSprite ? item.icon : null;
            icon.preserveAspect = true;
        }

        if (title != null)
            title.text = item != null ? item.displayName : "";

        if (priceText != null)
            priceText.text = _price > 0 ? $"Price: {_price}" : "";
    }

    private void OnValidate()
    {
        // keep prefab preview in sync in editor
        RefreshUI();
    }

    // Click anywhere on the card to show details (Buy button still works separately)
    public void OnPointerClick(PointerEventData e)
    {
        if (!detailsPanel)
        {
            Debug.LogWarning($"{name}: detailsPanel not assigned.");
            return;
        }
        if (!item)
        {
            Debug.LogWarning($"{name}: item is null on click.");
            return;
        }

        detailsPanel.Show(item);     // fill icon/title/description
        detailsPanel.ShowBuyOnly();  // show only the Buy button in the panel
    }
}
