using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // for LayoutRebuilder

public class ShopGridController : MonoBehaviour
{
    [Header("UI/Scene")]
    [SerializeField] private Transform gridParent; // Parent with GridLayoutGroup
    [SerializeField] private ShopItemView itemPrefab; // Icon-only prefab
    [SerializeField] private InventoryManager inventory; // InventoryManager in scene
    [SerializeField] private UpgradeDetailsPanel detailsPanel; // Right-side panel
    [SerializeField] private int defaultPrice = 100;

    [Header("Roll Settings")]
    [SerializeField, Min(1)] private int itemsToShow = 5;
    [SerializeField] private bool rerollOnEnable = true;

    [Header("Mock Catalog (ScriptableObjects)")]
    [Tooltip("Assign your UpgradeItem assets here")]
    //[SerializeField] private List<UpgradeItem> mockCatalog = new();
    [SerializeField] UpgradeItem templateItem;
    private GridLayoutGroup _grid;
    // Track spawned cards so we can destroy the one the player buys
    private readonly Dictionary<string, ShopItemView> _viewsById = new();

    Type[] upgradeList = { typeof(EngineUpgrade), typeof(DamageUpgrade), typeof(AttackUpgrade), typeof(DashUpgrade), typeof(SuperDashUpgrade), typeof(BulletSpeedUpgrade),
    typeof(BulletPierceUpgrade), typeof(PlayerRecoveryUpgrade), typeof(EnrageUpgrade), typeof(ExplosiveHitUpgrade), typeof(ExplosiveKillUpgrade), typeof(ExtraCannonUpgrade),
    typeof(ExtraKillTrigger), typeof(DamageOnKill), typeof(CreditsOnKill), typeof(HealthOnKill), typeof(VirusDamageBoost), typeof(PoisonUpgrade), typeof(HealthUpgrade),
    typeof(CreditsWhenHit), typeof(RailgunUpgrade), typeof(GattlingGunUpgrade)};

    [SerializeField] List<Sprite> upgradeSprites;

    private void Awake()
    {
        _grid = gridParent ? gridParent.GetComponent<GridLayoutGroup>() : null;
    }

    private void OnEnable()
    {
        // Auto find missing refs
        if (!_grid && gridParent) _grid = gridParent.GetComponent<GridLayoutGroup>();
        if (!inventory) inventory = FindObjectOfType<InventoryManager>(true);
        if (!detailsPanel) detailsPanel = FindObjectOfType<UpgradeDetailsPanel>(true);

        if (detailsPanel) detailsPanel.OnBuyRequested += HandlePanelBuy;

        if (rerollOnEnable) BuildShopFromMock();
        else ForceGridRebuild(); // Ensures layout is correct
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
        if (!itemPrefab || !gridParent)
        {
            Debug.LogWarning("[Shop] Missing catalog or bindings.");
            return;
        }
        
        int take = itemsToShow;
        List<int> usedItems = new List<int>();

        for (int k = 0; k < take; k++) {
            //int swap = UnityEngine.Random.Range(k, pool.Count);
            //(pool[k], pool[swap]) = (pool[swap], pool[k]);

            //var mock = mockCatalog[pool[k]];
            //if (!mock) continue;

            //// 4) Clone the ScriptableObject so each card has its own instance/state
            //var item = ScriptableObject.Instantiate(mock);
            //item.EnsureId(); // stable id at runtime

            //// 5) Hydrate concrete Upgrade (if using tempUpgrades)
            //if (item.upgrade == null && item.tempUpgrades != null && item.tempUpgrades.Length > 0) {
            //    int id = Mathf.Clamp(item.tempUpgradeID, 0, item.tempUpgrades.Length - 1);
            //    item.upgrade = item.tempUpgrades[id];
            //}
            int toSpawnID = UnityEngine.Random.Range(0, upgradeList.Length);
            //ensure we spawn unique upgrades
            while (usedItems.Contains(toSpawnID)) { toSpawnID = UnityEngine.Random.Range(0, upgradeList.Length); }
            usedItems.Add(toSpawnID);
            UpgradeItem item = Instantiate(templateItem);
            Type[] filterType = { typeof(Rarity) };
            //if upgrade has a constructor that accepts a rarity, roll for rarity
            if (upgradeList[toSpawnID].GetConstructor(filterType) != null) {
                int rarityRoll = UnityEngine.Random.Range(0, 10);
                Rarity itemRarity;
                //rarity checks, ratios could be adjusted for balance
                if (rarityRoll < 6) itemRarity = Rarity.Common;
                else if (rarityRoll < 9) itemRarity = Rarity.Uncommon;
                else itemRarity = Rarity.Rare;
                item.upgrade = (Upgrade)Activator.CreateInstance(upgradeList[toSpawnID], itemRarity);
            } else item.upgrade = (Upgrade)Activator.CreateInstance(upgradeList[toSpawnID]);
            //update data object's parameters to match the created upgrade
            item.name = item.upgrade.name;
            item.displayName = item.upgrade.name;
            item.description = item.upgrade.description;
            item.icon = upgradeSprites[item.upgrade.spriteID];

            // 7) Spawn icon card; click shows panel
            var view = Instantiate(itemPrefab, gridParent, false);
            SnapChildToGridCell(view.transform as RectTransform);
            int price = (item.upgrade != null) ? item.upgrade.value : defaultPrice;
            view.Bind(item, price, null);
            var grid = gridParent.GetComponent<GridLayoutGroup>();
            ConformToGrid(view.transform as RectTransform, grid);

            // 8) Track for removal after purchase
            if (!string.IsNullOrEmpty(item.id))
                _viewsById[item.id] = view;
        }

        // 9) Force a reflow so icons lock into grid slots
        ReflowGridNow();
    }

    // Called when the DETAILS PANEL Buy btn is pressed
    private void HandlePanelBuy(UpgradeItem item)
    {
        if (item == null || item.upgrade == null || inventory == null)
            return;

        // Attempt purchase (deduct credits, create inventory item)
        if (inventory.TryPurchase(item, out var created))
        {
            detailsPanel.Clear();
            ReflowGridNow();
            Debug.Log($"[Shop] Purchased {created.displayName}. Item remains available in shop.");
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

    private void ReflowGridNow()
    {
        if (!gridParent) return;
        var rt = gridParent as RectTransform;
        if (!rt) return;

        if (_grid == null) _grid = gridParent.GetComponent<GridLayoutGroup>();
        if (_grid == null) return;

        for (int i = 0; i < gridParent.childCount; i++)
            SnapChildToGridCell(gridParent.GetChild(i) as RectTransform);

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);


        // normalize every existing child
        for (int i = 0; i < gridParent.childCount; i++)
        {
            var crt = gridParent.GetChild(i) as RectTransform;
            ConformToGrid(crt, _grid);
        }

        // force a layout pass
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
    }

    private void ConformToGrid(RectTransform cardRt, GridLayoutGroup grid)
    {
        if (!cardRt || !grid) return;

        // kill any drifting
        cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
        cardRt.pivot = new Vector2(0.5f, 0.5f);
        cardRt.localScale = Vector3.one;
        cardRt.anchoredPosition = Vector2.zero;
        cardRt.offsetMin = cardRt.offsetMax = Vector2.zero;
        cardRt.sizeDelta = grid.cellSize;

        // ensure no other layout system fights the grid
        var vlg = cardRt.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
        if (vlg) Destroy(vlg);
        var hlg = cardRt.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
        if (hlg) Destroy(hlg);
        var csf = cardRt.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (csf) Destroy(csf);

        // optional but helps: pin a LayoutElement to cell size
        var le = cardRt.GetComponent<UnityEngine.UI.LayoutElement>();
        if (!le) le = cardRt.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
        le.minWidth = le.preferredWidth = grid.cellSize.x;
        le.minHeight = le.preferredHeight = grid.cellSize.y;
    }
    private void SnapChildToGridCell(RectTransform rt)
    {
        if (!rt || _grid == null) return;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.localScale = Vector3.one;

        // Size exactly to the grid’s cell
        rt.sizeDelta = _grid.cellSize;

        // If the prefab has a LayoutElement, set preferred size too
        var le = rt.GetComponent<LayoutElement>();
        if (le)
        {
            le.preferredWidth = _grid.cellSize.x;
            le.preferredHeight = _grid.cellSize.y;
            le.minWidth = le.minHeight = -1;
            le.flexibleWidth = le.flexibleHeight = 0f;
            le.ignoreLayout = false;
        }
    }


}
