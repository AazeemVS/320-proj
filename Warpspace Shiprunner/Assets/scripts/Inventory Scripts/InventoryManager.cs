using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
  public static InventoryManager Instance { get; private set; }

  [SerializeField] private int inventoryCapacity = 20;
  [SerializeField] private int activeCapacity = 5;

  public IReadOnlyList<UpgradeItem> Inventory => inventory;
  public IReadOnlyList<UpgradeItem> Active => active;

  private readonly List<UpgradeItem> inventory = new();
  private readonly List<UpgradeItem> active = new();

  void Awake()
  {
    if (Instance != null && Instance != this) { Destroy(gameObject); return; }
    Instance = this;
    DontDestroyOnLoad(gameObject);
  }

  public bool TryAddToInventory(UpgradeItem item)
  {
    if (inventory.Count >= inventoryCapacity) return false;
    inventory.Add(item);
    OnChanged?.Invoke();
    return true;
  }

  public bool TryAddToActive(UpgradeItem item)
  {
    if (active.Count >= activeCapacity) return false;
    active.Add(item);
    OnChanged?.Invoke();
    return true;
  }

  public void RemoveFromInventoryAt(int index)
  {
    if (index < 0 || index >= inventory.Count) return;
    inventory.RemoveAt(index);
    OnChanged?.Invoke();
  }

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

  void Start()
  {
    foreach (var it in seedInventory) TryAddToInventory(it);
    foreach (var it in seedActive) TryAddToActive(it);
  }
}
