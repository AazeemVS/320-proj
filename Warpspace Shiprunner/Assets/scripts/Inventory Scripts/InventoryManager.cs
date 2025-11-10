using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  // Singleton instance (only one exists)
  public static InventoryManager Instance { get; private set; }

  [SerializeField] private int inventoryCapacity = 20; // Max number of inventory items
  [SerializeField] private int activeCapacity = 5;     // Max number of active items
   [SerializeField] private player_movement player;
   // Read-only access for UI
    public IReadOnlyList<UpgradeItem> Inventory => inventory;
  public IReadOnlyList<UpgradeItem> Active => active;

  // Internal item lists
  private readonly List<UpgradeItem> inventory = new();
  private readonly List<UpgradeItem> active = new();

    // Fill with mock data when game starts
    void Start()
    {
        foreach (var it in seedInventory)
        {
            if (it != null && it.tempUpgrades != null && it.tempUpgrades.Length > it.tempUpgradeID)
                it.upgrade = it.tempUpgrades[it.tempUpgradeID];
                it.EnsureId();
                TryAddToInventory(it);
        }

        foreach (var it in seedActive)
        {
            if (it != null && it.tempUpgrades != null && it.tempUpgrades.Length > it.tempUpgradeID)
                it.upgrade = it.tempUpgrades[it.tempUpgradeID];
                it.EnsureId();
                TryAddToActive(it);
        }
    }
    // Set up singleton and persists across scenes
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (player == null)
        {
            player = FindFirstObjectByType<player_movement>();
            if (player == null) Debug.LogWarning("InventoryManager: player not assigned.", this);
        }
    }


  // Adds an item to the inventory, if the inventory isn't not full
  public bool TryAddToInventory(UpgradeItem item)
  {
    if (inventory.Count >= inventoryCapacity) return false;
    item?.EnsureId();
    inventory.Add(item);
    OnChanged?.Invoke();
    return true;
  }

  // Adds an item to the active slots (if not full)
  public bool TryAddToActive(UpgradeItem item)
  {
    if (active.Count >= activeCapacity) return false;
    item?.EnsureId();
    active.Add(item);
    SafeEquip(item);
    OnChanged?.Invoke();
    return true;
  }





  // Removes an item from the inventory by index
  public void RemoveFromInventoryAt(int index)
  {
    if (index < 0 || index >= inventory.Count) return;
    inventory.RemoveAt(index);
    OnChanged?.Invoke();
  }

    // Removes an item from the active list by index
    public void RemoveFromActiveAt(int index)
    {
        if (index < 0 || index >= active.Count) return;
        SafeUnequip(active[index]);
        active.RemoveAt(index);
        OnChanged?.Invoke();
    }



    // for UI to refresh when data changes
    public System.Action OnChanged;

  // TEMP: seed a few items at start to see UI working
  [Header("Seed (Optional)")]
  public UpgradeItem[] seedInventory;
  public UpgradeItem[] seedActive;






    // Moves an item between inventory and active grids
    public bool MoveBetween(SlotGroup from, int fromIndex, SlotGroup to, int toIndex)
    {
        try
        {
            if (from == to) return false;

            var fromList = (from == SlotGroup.Inventory) ? inventory : active;
            var toList = (to == SlotGroup.Inventory) ? inventory : active;
            int toCap = (to == SlotGroup.Inventory) ? inventoryCapacity : activeCapacity;

            if (fromIndex < 0 || fromIndex >= fromList.Count) return false;

            var item = fromList[fromIndex];
            if (item == null) return false;

            bool invToAct = (from == SlotGroup.Inventory && to == SlotGroup.Active);
            bool actToInv = (from == SlotGroup.Active && to == SlotGroup.Inventory);

            UpgradeItem displaced = null;

            // place into target (replace or append)
            if (toIndex < toList.Count)
            {
                displaced = toList[toIndex];

                // if putting into Active, unequip what we’re replacing first
                if (to == SlotGroup.Active) SafeUnequip(displaced);

                toList[toIndex] = item;

                // if moved into Active, equip now
                if (invToAct) SafeEquip(item);
            }
            else
            {
                if (toList.Count >= toCap) return false;
                toList.Add(item);
                if (invToAct) SafeEquip(item);
            }

            // remove from source *after* placing in target
            fromList.RemoveAt(fromIndex);

            // if we displaced something, put it back into the source list
            if (displaced != null)
            {
                int insertAt = Mathf.Min(fromIndex, fromList.Count);
                fromList.Insert(insertAt, displaced);

                // If source was Active, we’re putting the displaced item back into Active → re-equip it.
                if (from == SlotGroup.Active) SafeEquip(displaced);
            }

            OnChanged?.Invoke();
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"MoveBetween exception: {ex.Message}\nFROM {from}[{fromIndex}] TO {to}[{toIndex}]", this);
            return false;
        }
    }


    // Reorders an item within the same list
    public bool ReorderWithin(SlotGroup group, int fromIndex, int toIndex)
  {
    var list = (group == SlotGroup.Inventory) ? inventory : active;
    if (list.Count == 0) return false;
    if (fromIndex < 0 || fromIndex >= list.Count) return false;

    // Clamp toIndex to “end” so dropping on empty placeholders works
    toIndex = Mathf.Clamp(toIndex, 0, list.Count - 1);

    if (fromIndex == toIndex) return false;

    var item = list[fromIndex];
    list.RemoveAt(fromIndex);

    // if we removed before the insertion point, the index shifts left by one
    if (toIndex > fromIndex) toIndex -= 1;

    list.Insert(toIndex, item);

    OnChanged?.Invoke();
    return true;
  }
    public bool TryPurchase(UpgradeItem shopItem, out UpgradeItem created)
    {
        created = null;
        if (shopItem == null || shopItem.upgrade == null) return false;

        int price = Mathf.Max(0, shopItem.upgrade.value);
        if (inventory.Count >= inventoryCapacity) return false;
        if (player_movement.credits < price) return false;

        // pay
        player_movement.credits -= price;

        // clone a new inventory item that mirrors the shop item
        var newItem = ScriptableObject.CreateInstance<UpgradeItem>();

        // fresh unique id so selection/sticky logic works reliably
        newItem.id = System.Guid.NewGuid().ToString("N");

        // copy visuals/text
        newItem.displayName = string.IsNullOrWhiteSpace(shopItem.displayName)
            ? (shopItem.upgrade != null ? shopItem.upgrade.name : "Upgrade")
            : shopItem.displayName;

        newItem.description = shopItem.description ?? "";

        newItem.icon = shopItem.icon; // ok to share Sprite refs

        // use the same upgrade logic object
        // (If upgrades ever hold mutable runtime state, consider cloning the Upgrade here)
        newItem.upgrade = shopItem.upgrade;

        inventory.Add(newItem);
        created = newItem;

        OnChanged?.Invoke();
        return true;
    }



    // Adds a newly purchased Upgrade (from the Shop) to the inventory
    public bool AddUpgrade(Upgrade upgrade)
  {
    if (upgrade == null)
    {
      Debug.LogWarning("Tried to add a null upgrade to inventory.");
      return false;
    }

    if (inventory.Count >= inventoryCapacity)
    {
      Debug.Log("Inventory full — cannot add upgrade: " + upgrade.name);
      return false;
    }

    // ✅ Create ScriptableObject instance
    var newItem = ScriptableObject.CreateInstance<UpgradeItem>();
    newItem.upgrade = upgrade;
    newItem.displayName = upgrade.name; // optional
    newItem.description = "";
    newItem.EnsureId();

    inventory.Add(newItem);
    OnChanged?.Invoke();

    Debug.Log($"Added upgrade to inventory: {upgrade.name}");
    return true;
  }

  private void SafeEquip(UpgradeItem it)
    {
        if (it?.upgrade == null || player == null) return;
        try { it.upgrade.OnEquip(player); }
        catch (System.Exception ex)
        {
            Debug.LogError($"OnEquip error on {it.upgrade.name}: {ex.Message}", this);
        }
    }

    private void SafeUnequip(UpgradeItem it)
    {
        if (it?.upgrade == null || player == null) return;
        try { it.upgrade.OnUnequip(player); }
        catch (System.Exception ex)
        {
            Debug.LogError($"OnUnequip error on {it.upgrade.name}: {ex.Message}", this);
        }
    }


  public Upgrade generateUpgradeFromID(int id, int rarity = 0) {
    Upgrade newUpgrade = null;
    Rarity tier = (Rarity)rarity;
    switch ((upgradeID)id) {
        case upgradeID.EngineUpgrade:
            newUpgrade = new EngineUpgrade(tier);
            break;
        case upgradeID.DamageUpgrade: 
            newUpgrade = new DamageUpgrade(tier);
            break;
        case upgradeID.AttackUpgrade: 
            newUpgrade = new AttackUpgrade(tier);
            break;
        case upgradeID.DashUpgrade:
            newUpgrade = new DashUpgrade();
            break;
        case upgradeID.SuperDashUpgrade: 
            newUpgrade = new SuperDashUpgrade();
            break;
        case upgradeID.BulletSpeedUpgrade: 
            newUpgrade = new BulletSpeedUpgrade(tier);
            break;
        case upgradeID.BulletPierceUpgrade: 
            newUpgrade = new BulletPierceUpgrade(tier);
            break;
        case upgradeID.PlayerRecoveryUpgrade: 
            newUpgrade = new PlayerRecoveryUpgrade(tier);
            break;
        case upgradeID.EnrageUpgrade: 
            newUpgrade = new EnrageUpgrade(tier);
            break;
        case upgradeID.ExtraCannonUpgrade:
            newUpgrade = new ExtraCannonUpgrade();
            break;
        }
        return newUpgrade;
    }

    public bool TrySell(SlotGroup group, int index, out int refund)
    {
        refund = 0;

        // choose list
        var list = (group == SlotGroup.Inventory) ? inventory : active;

        // guards
        if (index < 0 || index >= list.Count) return false;
        var item = list[index];
        if (item == null) return false;

        // if selling from Active, unequip first
        if (group == SlotGroup.Active) SafeUnequip(item);

        // compute refund = half the upgrade value (rounds down)
        int baseValue = (item.upgrade != null) ? item.upgrade.value : 0;
        refund = Mathf.Max(0, baseValue / 2);

        // credit the player
        player_movement.credits += refund;

        // remove item
        list.RemoveAt(index);

        // notify UI
        OnChanged?.Invoke();
        return true;
    }


}
