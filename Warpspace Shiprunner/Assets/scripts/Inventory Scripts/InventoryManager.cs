using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  // Singleton instance (only one exists)
  public static InventoryManager Instance { get; private set; }

  [SerializeField] private int inventoryCapacity = 20; // Max number of inventory items
  [SerializeField] private int activeCapacity = 5;     // Max number of active items

  // Read-only access for UI
  public IReadOnlyList<UpgradeItem> Inventory => inventory;
  public IReadOnlyList<UpgradeItem> Active => active;

  // Internal item lists
  private readonly List<UpgradeItem> inventory = new();
  private readonly List<UpgradeItem> active = new();

  // Set up singleton and persists across scenes
  void Awake()
  {
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  // Adds an item to the inventory, if the inventory isn't not full
  public bool TryAddToInventory(UpgradeItem item)
  {
    if (inventory.Count >= inventoryCapacity) return false;
    inventory.Add(item);
    OnChanged?.Invoke(); // Notifies the UI
    return true;
  }

  // Adds an item to the active slots (if not full)
  public bool TryAddToActive(UpgradeItem item)
  {
    if (active.Count >= activeCapacity) return false;
    active.Add(item);
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
    active.RemoveAt(index);
    OnChanged?.Invoke();
  }

  // for UI to refresh when data changes
  public System.Action OnChanged;

  // TEMP: seed a few items at start to see UI working
  [Header("Seed (Optional)")]
  public UpgradeItem[] seedInventory;
  public UpgradeItem[] seedActive;


  // Fill with mock data when game starts
  void Start()
  {
        foreach (var it in seedInventory) { 
            it.upgrade = it.tempUpgrades[it.tempUpgradeID];
            TryAddToInventory(it); };
        foreach (var it in seedActive) {
            TryAddToActive(it);
            it.upgrade = it.tempUpgrades[it.tempUpgradeID];

        }
        ;
  }

  // Moves an item between inventory and active grids
  public bool MoveBetween(SlotGroup from, int fromIndex, SlotGroup to, int toIndex)
  {
    if (from == to) return false; // tonight: only cross-grid moves

    var fromList = (from == SlotGroup.Inventory) ? inventory : active;
    var toList = (to == SlotGroup.Inventory) ? inventory : active;

    int toCap = (to == SlotGroup.Inventory) ? inventoryCapacity : activeCapacity;

    if (fromIndex < 0 || fromIndex >= fromList.Count) return false;

    var item = fromList[fromIndex];
    if (item == null) return false;

    UpgradeItem displaced = null;

    // Replace or add item in target list
    if (toIndex < toList.Count)
    {
      displaced = toList[toIndex];
      toList[toIndex] = item;             // replace target
    }
    else
    {
      // Dropped on an "empty" slot beyond current count → try to append
      if (toList.Count >= toCap) return false;
      toList.Add(item);
    }

    // Remove from source *after* placing in target
    fromList.RemoveAt(fromIndex);

    // If we displaced someone, put them back into the source list near the original index
    if (displaced != null)
    {
      // now fromList is one shorter, so clamp the insert index
      int insertAt = Mathf.Min(fromIndex, fromList.Count);
      fromList.Insert(insertAt, displaced);
    }

    OnChanged?.Invoke();
    return true;
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

}
