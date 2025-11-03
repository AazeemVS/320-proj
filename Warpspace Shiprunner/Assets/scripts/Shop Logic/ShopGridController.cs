// ShopGridController.cs
using System.Collections.Generic;
using UnityEngine;

public class ShopGridController : MonoBehaviour
{
    [Header("UI/Scene")]
    [SerializeField] private Transform gridParent;      // GridLayoutGroup parent
    [SerializeField] private ShopItemView itemPrefab;   // prefab with ShopItemView
    [SerializeField] private InventoryManager inventory; // in-scene InventoryManager

    [Header("Roll Settings")]
    [SerializeField, Min(1)] private int itemsToShow = 5;
    [SerializeField] private bool rerollOnEnable = true;

    [Header("Mock Catalog (ScriptableObjects)")]
    [Tooltip("Drop your MockData/MockItem assets here")]
    [SerializeField] private List<UpgradeItem> mockCatalog = new();
    [SerializeField] private int defaultPrice = 100; // used if item.upgrade is null

    private readonly List<UpgradeItem> _currentRuntime = new();

    void OnEnable()
    {
        if (rerollOnEnable) BuildShopFromMock();
    }

    [ContextMenu("Reroll")]
    public void BuildShopFromMock()
    {
        // 1) clear old UI
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);
        _currentRuntime.Clear();

        if (mockCatalog == null || mockCatalog.Count == 0 || itemPrefab == null || gridParent == null)
        {
            Debug.LogWarning("[Shop] Missing mock catalog or bindings.");
            return;
        }

        // 2) choose up to itemsToShow unique entries
        var pool = new List<int>(mockCatalog.Count);
        for (int i = 0; i < mockCatalog.Count; i++) pool.Add(i);

        int take = Mathf.Min(itemsToShow, pool.Count);
        for (int k = 0; k < take; k++)
        {
            int swap = Random.Range(k, pool.Count);
            (pool[k], pool[swap]) = (pool[swap], pool[k]);

            var mock = mockCatalog[pool[k]];
            if (mock == null) continue;

            var item = ScriptableObject.Instantiate(mock);
            item.EnsureId(); // makes sure it has a unique stable id

            // 4) hydrate concrete Upgrade so shop displays/uses it
            if (item.upgrade == null &&
                item.tempUpgrades != null &&
                item.tempUpgradeID >= 0 &&
                item.tempUpgradeID < item.tempUpgrades.Length)
            {
                item.upgrade = item.tempUpgrades[item.tempUpgradeID];
            }

            _currentRuntime.Add(item);

            // 5) spawn a card and BIND data (this sets icon/title/price and wires Buy)
            var view = Instantiate(itemPrefab, gridParent, false);
            int price = (item.upgrade != null) ? item.upgrade.value : defaultPrice;
            view.Bind(item, price, OnBuyClicked);
        }
    }

    private void OnBuyClicked(UpgradeItem item)
    {
        if (item == null || item.upgrade == null) { Debug.Log("[Shop] Nothing to buy."); return; }

        if (inventory != null && inventory.TryPurchase(item.upgrade, out var created))
        {
            Debug.Log($"[Shop] Purchased {item.displayName}");
        }
        else
        {
            Debug.Log("[Shop] Purchase failed (capacity or credits).");
        }
    }
}
