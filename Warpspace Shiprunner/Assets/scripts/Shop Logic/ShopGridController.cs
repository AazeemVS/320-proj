using System.Collections.Generic;
using UnityEngine;

public class ShopGridController : MonoBehaviour
{
  [Header("UI/Scene")]
  [SerializeField] private Transform gridParent;          // Parent with GridLayoutGroup
  [SerializeField] private ShopItemView itemPrefab;       // Prefab that shows ONLY an icon
  [SerializeField] private InventoryManager inventory;    // Your InventoryManager in scene
  [SerializeField] private UpgradeDetailsPanel detailsPanel; // The right-side details panel
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
    if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);
    if (detailsPanel) detailsPanel.OnBuyRequested += HandlePanelBuy;

    if (rerollOnEnable) BuildShopFromMock();
  }

  private void OnDisable()
  {
    if (detailsPanel) detailsPanel.OnBuyRequested -= HandlePanelBuy;
  }

  [ContextMenu("Reroll")]
  public void BuildShopFromMock()
  {
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
      item.EnsureId(); // make sure we have a stable id at runtime

      // 5) Hydrate its concrete Upgrade for price/effects (if using tempUpgrades)
      if (item.upgrade == null && item.tempUpgrades != null && item.tempUpgrades.Length > 0)
      {
        int id = Mathf.Clamp(item.tempUpgradeID, 0, item.tempUpgrades.Length - 1);
        item.upgrade = item.tempUpgrades[id];
      }

      // 6) Optional: fill visuals if authoring left them blank
      if (string.IsNullOrWhiteSpace(item.displayName) && item.upgrade != null)
        item.displayName = item.upgrade.name;

      // 7) Spawn the icon card and bind (icon only; click = show details on panel)
      var view = Instantiate(itemPrefab, gridParent, false);
      int price = (item.upgrade != null) ? item.upgrade.value : defaultPrice;
      view.Bind(item, price, null);
      // 8) Track by id so we can remove this card after purchase
      if (!string.IsNullOrEmpty(item.id))
        _viewsById[item.id] = view;
    }
  }

  // Called when the DETAILS PANEL Buy button is pressed
  private void HandlePanelBuy(UpgradeItem item)
  {
    if (item == null || item.upgrade == null || inventory == null)
      return;

    // Attempt purchase (deduct credits, create inventory item)
    if (inventory.TryPurchase(item, out var created))
    {
      // Remove the corresponding shop card
      if (!string.IsNullOrEmpty(item.id) && _viewsById.TryGetValue(item.id, out var view))
      {
        if (view) Destroy(view.gameObject);
        _viewsById.Remove(item.id);
      }

      // Reset panel UI after success
      detailsPanel.Clear();
    }
    else
    {
      Debug.Log("[Shop] Purchase failed (credits or capacity).");
    }
  }
}
