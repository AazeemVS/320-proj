using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for LayoutRebuilder

public class ShopGridController : MonoBehaviour
{
    [Header("UI/Scene")]
    [SerializeField] private Transform gridParent;                // Parent with GridLayoutGroup
    [SerializeField] private ShopItemView itemPrefab;             // Icon-only prefab
    [SerializeField] private InventoryManager inventory;          // InventoryManager in scene
    [SerializeField] private UpgradeDetailsPanel detailsPanel;    // Right-side panel
    [SerializeField] private int defaultPrice = 100;

    [Header("Roll Settings")]
    [SerializeField, Min(1)] private int itemsToShow = 5;
    [SerializeField] private bool rerollOnEnable = true;

    [Header("Mock Catalog (ScriptableObjects)")]
    [Tooltip("Assign your UpgradeItem assets here")]
    [SerializeField] private List<UpgradeItem> mockCatalog = new();

    // Track spawned cards so we can destroy the one the player buys
    private readonly Dictionary<string, ShopItemView> _viewsById = new();

    private void OnEnable()
    {
        // Auto-find missing refs (helps after scene/page swaps)
        if (!inventory) inventory = FindObjectOfType<InventoryManager>(true);
        if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);

        if (detailsPanel) detailsPanel.OnBuyRequested += HandlePanelBuy;

        if (rerollOnEnable) BuildShopFromMock();
        else ForceGridRebuild(); // still ensure layout is correct
    }

    private void OnDisable()
    {
        if (detailsPanel) detailsPanel.OnBuyRequested -= HandlePanelBuy;
    }

    /// <summary>
    /// Call this when returning from a round or reopening the shop UI.
    /// </summary>
    public void RefreshShop()
    {
        BuildShopFromMock();
    }

    [ContextMenu("Reroll")]
    public void BuildShopFromMock()
    {
        if (detailsPanel) detailsPanel.Clear();

        // 1) Clear old cards
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
        _viewsById.Clear();

        // 2) Validate
        if (mockCatalog == null || mockCatalog.Count == 0 || !itemPrefab || !gridParent)
        {
            Debug.LogWarning("[Shop] Missing catalog or bindings.");
            return;
        }

        // 3) Choose up to itemsToShow unique entries
        var pool = new List<int>(mockCatalog.Count);
        for (int i = 0; i < mockCatalog.Count; i++) pool.Add(i);
        int take = Mathf.Min(itemsToShow, pool.Count);

        for (int k = 0; k < take; k++)
        {
            int swap = Random.Range(k, pool.Count);
            (pool[k], pool[swap]) = (pool[swap], pool[k]);

            var mock = mockCatalog[pool[k]];
            if (!mock) continue;

            // 4) Clone the ScriptableObject so each card has its own instance/state
            var item = ScriptableObject.Instantiate(mock);
            item.EnsureId(); // stable id at runtime

            // 5) Hydrate concrete Upgrade (if using tempUpgrades)
            if (item.upgrade == null && item.tempUpgrades != null && item.tempUpgrades.Length > 0)
            {
                int id = Mathf.Clamp(item.tempUpgradeID, 0, item.tempUpgrades.Length - 1);
                item.upgrade = item.tempUpgrades[id];
            }

            // 6) Optional: fill visuals if left blank
            if (string.IsNullOrWhiteSpace(item.displayName) && item.upgrade != null)
                item.displayName = item.upgrade.name;

            // 7) Spawn icon card; click shows panel (Buy happens from the panel)
            var view = Instantiate(itemPrefab, gridParent, false);
            int price = (item.upgrade != null) ? item.upgrade.value : defaultPrice;
            view.Bind(item, price, null);       // null => buying is done from the details panel

            // 8) Track for removal after purchase
            if (!string.IsNullOrEmpty(item.id))
                _viewsById[item.id] = view;
        }

        // 9) Force a reflow so icons lock into grid slots
        ForceGridRebuild();
    }

    // Called when the DETAILS PANEL Buy button is pressed
    private void HandlePanelBuy(UpgradeItem item)
    {
        if (item == null || item.upgrade == null || inventory == null)
            return;

        if (inventory.TryPurchase(item, out var created))
        {
            // Remove the corresponding shop card
            if (!string.IsNullOrEmpty(item.id) && _viewsById.TryGetValue(item.id, out var view))
            {
                if (view) Destroy(view.gameObject);
                _viewsById.Remove(item.id);
            }

            detailsPanel.Clear();   // reset panel
            ForceGridRebuild();     // keep grid tidy after removal
        }
        else
        {
            Debug.Log("[Shop] Purchase failed (credits or capacity).");
        }
    }

    private void ForceGridRebuild()
    {
        if (gridParent && gridParent is RectTransform rt)
            LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    public void RemoveCardFor(string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return;
        if (_viewsById.TryGetValue(itemId, out var view) && view)
        {
            Destroy(view.gameObject);
        }
        _viewsById.Remove(itemId);
    }

}
