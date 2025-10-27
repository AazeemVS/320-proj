// In ShopItemView.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public partial class ShopItemView : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI rarityText;
    [SerializeField] private Button buyButton;

    public void BindForMock(Sprite icon, string name, string cost, string rarityText, Action onBuyClicked)
    {
        if (this.icon) this.icon.sprite = icon;
        if (nameText) nameText.text = name;
        if (costText) costText.text = cost;
        if (this.rarityText) this.rarityText.text = rarityText;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuyClicked?.Invoke());
    }

    public void SetPurchased()
    {
        if (buyButton)
        {
            buyButton.interactable = false;
            buyButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText("Bought");
        }
    }
}
