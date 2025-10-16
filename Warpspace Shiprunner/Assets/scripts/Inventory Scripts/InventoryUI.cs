using UnityEngine;

public class InventoryUI : MonoBehaviour
{
  [SerializeField] private Transform inventoryGrid;  // Parent for inventory slots
  [SerializeField] private Transform activeGrid;     // Parent for active slots
  [SerializeField] private UISlot slotPrefab;        // Prefab for each slot
  [SerializeField] private int inventorySlots = 20;  // Max inventory slots
  [SerializeField] private int activeSlots = 5;      // Max active slots

  [Header("Details")]
  [SerializeField] private UpgradeDetailsPanel detailsPanel; // Panel to show item info

  private UISlot selected;       // Currently selected slot
  private bool rebuildQueued;    // Prevents multiple rebuilds in one frame

  // Subscribe to inventory updates and build the UI
  void OnEnable()
  {
    InventoryManager.Instance.OnChanged += QueueRebuild;
    Rebuild();
  }

  // Rebuilds both grids (active and inventory grids)
  public void Rebuild()
  {
    // Clear old slots
    foreach (Transform t in inventoryGrid) Destroy(t.gameObject);
    foreach (Transform t in activeGrid) Destroy(t.gameObject);

    // Get current item lists
    var inv = InventoryManager.Instance.Inventory;
    var act = InventoryManager.Instance.Active;

    // Build inventory slots
    for (int i = 0; i < inventorySlots; i++)
    {
      var slot = Instantiate(slotPrefab, inventoryGrid);
      var item = i < inv.Count ? inv[i] : null;
      slot.Set(SlotGroup.Inventory, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }

    // Build active slots
    for (int i = 0; i < activeSlots; i++)
    {
      var slot = Instantiate(slotPrefab, activeGrid);
      var item = i < act.Count ? act[i] : null;
      slot.Set(SlotGroup.Active, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }
  }

  // Called when a slot is clicked — shows details in the info panel
  private void HandleSlotClicked(UpgradeItem item, UISlot slot)
  {
    selected = slot;
    if (detailsPanel) detailsPanel.Show(item);
  }

  // Unsubscribe from events when disabled
  void OnDisable()
  {
    if (InventoryManager.Instance != null)
      InventoryManager.Instance.OnChanged -= QueueRebuild;
  }

  // Queues a UI rebuild (so it waits one frame before running)
  private void QueueRebuild()
  {
    if (!rebuildQueued) StartCoroutine(RebuildNextFrame());
  }

  // Waits one frame before rebuilding UI (lets drag/drop finish)
  private System.Collections.IEnumerator RebuildNextFrame()
  {
    rebuildQueued = true;
    yield return null;   // wait 1 frame -> EndDrag executes this frame, we rebuild next
    Rebuild();
    rebuildQueued = false;
  }
}
