using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    [SerializeField] private UpgradeItem item;

    [Header("Optional Card UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text priceText;

    [Header("References")]
    [SerializeField] private UpgradeDetailsPanel detailsPanel; // <- expects the component

    // Call this if you spawn items at runtime:
    public void Init(UpgradeItem i, UpgradeDetailsPanel panel)
    {
        item = i;
        detailsPanel = panel;
        RefreshUI();
    }

    private void Awake()
    {
        // Fallback: auto-find in scene if not set in Inspector
        if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);
    }

    private void RefreshUI()
    {
        if (!item) return;
        if (icon) { icon.enabled = item.icon; icon.sprite = item.icon; }
        if (title) title.text = item.displayName;
    }

    private void OnValidate() { if (item) RefreshUI(); }

    public void OnPointerClick(PointerEventData e)
    {
        if (!detailsPanel) { Debug.LogWarning($"{name}: detailsPanel not assigned."); return; }
        detailsPanel.Show(item);
    }
}
