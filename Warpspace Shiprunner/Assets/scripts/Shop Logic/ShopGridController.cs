using System.Collections.Generic;
using UnityEngine;

public class ShopGridController : MonoBehaviour
{
  [Header("UI/Scene")]
  [SerializeField] private Transform gridParent;       // GridLayoutGroup parent
  [SerializeField] private ShopItemView itemPrefab;    // prefab that only shows an icon
  [SerializeField] private InventoryManager inventory; // InventoryManager in scene
  [SerializeField] private UpgradeDetailsPanel detailsPanel; // <- panel with Buy/Sell

  [Header("Roll Settings")]
  [SerializeField, Min(1)] private int itemsToShow = 5;
  [SerializeField] private bool rerollOnEnable = true;

  [Header("Mock Catalog (ScriptableObjects)")]
  [SerializeField] private List<UpgradeItem> mockCatalog = new();

  // Track spawned cards so we can destroy the one the player buys
  private readonly Dictionary<string, ShopItemView> _viewsById = new();

  void OnEnable()
  {
    if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);
    if (detailsPanel) detailsPanel.OnBuyRequested += HandlePanelBuy;

    if (rerollOnEnable) BuildShopFromMock();
  }

  void OnDisable()
  {
    if (detailsPanel) detailsPanel.OnBuyRequested -= HandlePanelBuy;
  }

  [ContextMenu("Reroll")]
  public void BuildShopFromMock()
  {
    // Clear old cards
    for (int i = gridParent.childCount - 1; i >= 0; i--)
      Destroy(gridParent.GetChild(i).gameObject);
    _viewsById.Clear();

    if (mockCatalog == null || mockCatalog.Count == 0 || !itemPrefab || !gridParent)
    {
      Debug.LogWarning("[Shop] Missing catalog or bindings.");
      return;
    }

    // Choose up to itemsToShow unique entries
    var pool = new List<int>(mockCatalog.Count);
    for (int i = 0; i < mockCatalog.Count; i++) pool.Add(i);
    int take = Mathf.Min(itemsToShow, pool.Count);

    for (int k = 0; k < take; k++)
    {
      int swap = Random.Range(k, pool.Count);
      (pool[k], pool[swap]) = (pool[swap], pool[k]);

      var mock = mockCatalog[pool[k]];
      if (!mock) continue;

      // Clone the ScriptableObject so each card has its own instance/state
      var item = ScriptableObject.Instantiate(mock);
      item.EnsureId();


      if (item.upgrade == null && item.tempUpgrades != null && item.tempUpgrades.Length > 0)
      {
        int id = Mathf.Clamp(item.tempUpgradeID, 0, item.tempUpgrades.Length - 1);
        item.upgrade = item.tempUpgrades[id];
      }

      // keep/repair visuals (optional defaults)
      if (string.IsNullOrWhiteSpace(item.displayName) && item.upgrade != null)
        item.displayName = item.upgrade.name;


      // Spawn the icon card
      var view = Instantiate(itemPrefab, gridParent, false);
      view.Bind(item, detailsPanel);

      // Track by id so we can destroy this view after purchase
      if (!string.IsNullOrEmpty(item.id))
        _viewsById[item.id] = view;
    }
  }

  // Fired when the panel Buy button is pressed (we subscribed in OnEnable)
  private void HandlePanelBuy(UpgradeItem item)
  {
    if (item == null || item.upgrade == null || inventory == null)
      return;

    // Deduct credits + add new UpgradeItem to the inventory list
    if (inventory.TryPurchase(item.upgrade, out var created))
    {
      // Remove the corresponding shop card
      if (!string.IsNullOrEmpty(item.id) && _viewsById.TryGetValue(item.id, out var view))
      {
        if (view) Destroy(view.gameObject);
        _viewsById.Remove(item.id);
      }

      // Hide panel buttons after purchase
      detailsPanel.Clear();
    }
    else
    {
      Debug.Log("[Shop] Purchase failed (credits or capacity).");
    }
  }
}
