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

        // EQUIP effect if possible
        item?.upgrade?.OnEquip(player);

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

        // UNEQUIP effect before removal
        active[index]?.upgrade?.OnUnequip(player);

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

        // if we’re moving between Inventory and Active, handle equip/unequip
        bool invToAct = (from == SlotGroup.Inventory && to == SlotGroup.Active);
        bool actToInv = (from == SlotGroup.Active && to == SlotGroup.Inventory);

        UpgradeItem displaced = null;

        // Place in target
        if (toIndex < toList.Count)
        {
            displaced = toList[toIndex];

            // If target is Active and we’re replacing, unequip the displaced first
            if (to == SlotGroup.Active) displaced?.upgrade?.OnUnequip(player);

            toList[toIndex] = item;

            // If we just moved into Active, equip it now
            if (invToAct) item.upgrade?.OnEquip(player);
        }
        else
        {
            if (toList.Count >= toCap) return false;
            toList.Add(item);

            if (invToAct) item.upgrade?.OnEquip(player);
        }

        // Remove from source AFTER placing in target
        fromList.RemoveAt(fromIndex);

        // If we displaced someone, put them back into the source list
        if (displaced != null)
        {
            // If we moved an Active item back to Inventory, it needs to be unequipped already (handled above).
            // If source was Active and we took something out, and we're putting displaced back into Active, equip it again:
            bool puttingBackIntoActive = (from == SlotGroup.Active);
            if (puttingBackIntoActive) displaced.upgrade?.OnEquip(player);

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
    public bool TryPurchase(Upgrade upgrade, out UpgradeItem created)
    {
        created = null;
        if (upgrade == null) return false;

        // capacity & funds
        if (inventory.Count >= inventoryCapacity) { Debug.Log("Inventory full."); return false; }
        if (player_movement.credits < upgrade.value) { Debug.Log("Not enough credits."); return false; }

        // pay
        player_movement.credits -= upgrade.value;

        // create a new item that wraps the Upgrade you rolled in the shop
        var item = new UpgradeItem
        {
            upgrade = upgrade
        };

        inventory.Add(item);
        created = item;

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

        // Check if there's space
        if (inventory.Count >= inventoryCapacity)
        {
            Debug.Log("Inventory full — cannot add upgrade: " + upgrade.name);
            return false;
        }

        // Wrap the Upgrade inside an UpgradeItem
        var newItem = new UpgradeItem();
        newItem.upgrade = upgrade;

        // Add it to the inventory
        inventory.Add(newItem);

        // Notify any UI listeners
        OnChanged?.Invoke();

        Debug.Log($"Added upgrade to inventory: {upgrade.name}");
        return true;
    }


}
