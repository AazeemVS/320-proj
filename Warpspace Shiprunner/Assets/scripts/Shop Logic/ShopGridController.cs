// ShopGridController.cs
using System.Collections.Generic;
using UnityEngine;

public class ShopGridController : MonoBehaviour
{
    [Header("UI/Scene")]
    [SerializeField] private Transform gridParent;   // GridLayoutGroup parent
    [SerializeField] private ShopItemView itemPrefab;
    [SerializeField] private InventoryManager inventory; // InventoryManager in scene

    [Header("Roll Settings")]
    [SerializeField, Min(1)] private int itemsToShow = 5;
    [SerializeField] private bool rerollOnEnable = true;

    [Header("Mock Catalog (ScriptableObjects)")]
    [Tooltip("Drop your MockData/MockItem assets here")]
    [SerializeField] private List<UpgradeItem> mockCatalog = new();
    [SerializeField] private int defaultPrice = 100; // used if item.upgrade is null

    private readonly List<UpgradeItem> _current = new();

    void OnEnable()
    {
        if (rerollOnEnable) BuildShopFromMock();
    }

    public void BuildShopFromMock()
    {
        // clear old
        for (int i = gridParent.childCount - 1; i >= 0; i--)
            Destroy(gridParent.GetChild(i).gameObject);

        _current.Clear();

        if (mockCatalog == null || mockCatalog.Count == 0 || itemPrefab == null || gridParent == null)
        {
            Debug.LogWarning("[Shop] Missing mock catalog or bindings.");
            return;
        }

        // choose unique random items
        var pool = new List<int>();
        for (int i = 0; i < mockCatalog.Count; i++) pool.Add(i);
        int take = Mathf.Min(itemsToShow, pool.Count);

        for (int k = 0; k < take; k++)
        {
            int swap = Random.Range(k, pool.Count);
            (pool[k], pool[swap]) = (pool[swap], pool[k]);

            var mock = mockCatalog[pool[k]];
            _current.Add(mock);

            // Ensure the Upgrade reference is set from your temp fields if needed
            if (mock.upgrade == null && mock.tempUpgrades != null &&
                mock.tempUpgradeID >= 0 && mock.tempUpgradeID < mock.tempUpgrades.Length)
            {
                mock.upgrade = mock.tempUpgrades[mock.tempUpgradeID];
            }

            // Build UI card
            var view = Instantiate(itemPrefab, gridParent);

            string displayName = !string.IsNullOrEmpty(mock.displayName) ? mock.displayName
                                 : mock.upgrade != null ? mock.upgrade.name
                                 : $"Item {k + 1}";

            int price = (mock.upgrade != null) ? mock.upgrade.value : defaultPrice;
            string rarityLabel = (mock.upgrade != null) ? ((Rarity)mock.upgrade.rarity).ToString() : "Common";

            view.BindForMock(
                icon: mock.icon,
                name: displayName,
                cost: price.ToString(),
                rarityText: rarityLabel,
                onBuyClicked: () => OnBuyMock(mock, price, view)
            );
        }
    }

    private void OnBuyMock(UpgradeItem mock, int price, ShopItemView view)
    {
        if (player_movement.credits < price)
        {
            Debug.Log("Not enough credits.");
            return;
        }

        // Pay and add to inventory
        player_movement.credits -= price;
        if (mock.upgrade == null && mock.tempUpgrades != null &&
            mock.tempUpgradeID >= 0 && mock.tempUpgradeID < mock.tempUpgrades.Length)
        {
            mock.upgrade = mock.tempUpgrades[mock.tempUpgradeID];
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.AddUpgrade(mock.upgrade);

        view.SetPurchased();
    }
}
