using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Grids/Prefab")]
    [SerializeField] private Transform inventoryGrid;  // Parent for inventory slots
    [SerializeField] private Transform activeGrid;     // Parent for active slots
    [SerializeField] private UISlot slotPrefab;        // Prefab for each slot
    [SerializeField] private int inventorySlots = 20;  // Max inventory slots
    [SerializeField] private int activeSlots = 5;      // Max active slots

    [Header("Details")]
    [SerializeField] private UpgradeDetailsPanel detailsPanel; // Panel to show item info

    [Header("Actions")]
    [SerializeField] private Button sellButton;        // <-- drag your Sell button here

    private UISlot selected;       // Currently selected slot
    private bool rebuildQueued;    // Prevents multiple rebuilds in one frame
    private SlotGroup? stickyGroup;
    private int stickyIndex;
  private string stickyItemId;
  // Subscribe to inventory updates and build the UI
  void OnEnable()
    {
        InventoryManager.Instance.OnChanged += QueueRebuild;

        // clean panel + hide sell button when opening
        if (detailsPanel) detailsPanel.Clear();
        if (sellButton) sellButton.gameObject.SetActive(false);

        Rebuild();
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnChanged -= QueueRebuild;
    }

  // Called by the Sell button (wire this in the button's OnClick)
  public void OnSellButtonPressed()
  {
    var mgr = InventoryManager.Instance;
    if (mgr == null) return;

    // Prefer the currently selected slot if it's valid
    if (selected != null)
    {
      var g = selected.group;
      var idx = selected.index;

      if (g == SlotGroup.Inventory)
      {
        if (idx >= 0 && idx < mgr.Inventory.Count && mgr.Inventory[idx] != null)
        {
          mgr.RemoveFromInventoryAt(idx);
          stickyItemId = null; // sold
          selected = null;
          if (sellButton) sellButton.gameObject.SetActive(false);
          if (detailsPanel) detailsPanel.Clear();
          return;
        }
      }
      else // Active
      {
        if (idx >= 0 && idx < mgr.Active.Count && mgr.Active[idx] != null)
        {
          mgr.RemoveFromActiveAt(idx);
          stickyItemId = null; // sold
          selected = null;
          if (sellButton) sellButton.gameObject.SetActive(false);
          if (detailsPanel) detailsPanel.Clear();
          return;
        }
      }
    }

    // If we get here, the selected slot was stale. Use stickyItemId to find the item.
    if (!string.IsNullOrEmpty(stickyItemId))
    {
      // search Inventory
      for (int i = 0; i < mgr.Inventory.Count; i++)
      {
        var it = mgr.Inventory[i];
        if (it != null && it.id == stickyItemId)
        {
          mgr.RemoveFromInventoryAt(i);
          stickyItemId = null;
          selected = null;
          if (sellButton) sellButton.gameObject.SetActive(false);
          if (detailsPanel) detailsPanel.Clear();
          return;
        }
      }
      // search Active
      for (int i = 0; i < mgr.Active.Count; i++)
      {
        var it = mgr.Active[i];
        if (it != null && it.id == stickyItemId)
        {
          mgr.RemoveFromActiveAt(i);
          stickyItemId = null;
          selected = null;
          if (sellButton) sellButton.gameObject.SetActive(false);
          if (detailsPanel) detailsPanel.Clear();
          return;
        }
      }
    }

    // Nothing to sell—clear UI gracefully
    selected = null;
    if (sellButton) sellButton.gameObject.SetActive(false);
    if (detailsPanel) detailsPanel.Clear();
  }


  // Rebuilds both grids (active and inventory grids)
  public void Rebuild()
  {
    var mgr = InventoryManager.Instance;

    // Remember current selection hints before we kill/recreate slots
    if (selected != null)
    {
      stickyGroup = selected.group;
      stickyIndex = selected.index;
    }

    // Clear old slots
    foreach (Transform t in inventoryGrid) Destroy(t.gameObject);
    foreach (Transform t in activeGrid) Destroy(t.gameObject);

    // Rebuild inventory slots
    var inv = mgr.Inventory;
    for (int i = 0; i < inventorySlots; i++)
    {
      var slot = Instantiate(slotPrefab, inventoryGrid);
      var item = i < inv.Count ? inv[i] : null;
      // If you added EnsureId() on UpgradeItem, you can do: item?.EnsureId();
      slot.Set(SlotGroup.Inventory, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }

    // Rebuild active slots
    var act = mgr.Active;
    for (int i = 0; i < activeSlots; i++)
    {
      var slot = Instantiate(slotPrefab, activeGrid);
      var item = i < act.Count ? act[i] : null;
      // item?.EnsureId();
      slot.Set(SlotGroup.Active, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }

    // --- Restore selection by prior group/index FIRST (most precise for drop target) ---
    bool restored = false;
    if (stickyGroup.HasValue)
    {
      var g = stickyGroup.Value;
      int idx = Mathf.Max(0, stickyIndex);

      if (g == SlotGroup.Inventory && idx < inv.Count && inv[idx] != null)
      {
        var slot = inventoryGrid.GetChild(idx).GetComponent<UISlot>();
        HandleSlotClicked(inv[idx], slot);
        restored = true;
      }
      else if (g == SlotGroup.Active && idx < act.Count && act[idx] != null)
      {
        var slot = activeGrid.GetChild(idx).GetComponent<UISlot>();
        HandleSlotClicked(act[idx], slot);
        restored = true;
      }
    }

    // --- If that failed (e.g., index shifted), fall back to restoring by item ID ---
    if (!restored && !string.IsNullOrEmpty(stickyItemId))
    {
      // search Inventory
      for (int i = 0; i < inv.Count && !restored; i++)
      {
        var it = inv[i];
        if (it != null && it.id == stickyItemId)
        {
          var slot = inventoryGrid.GetChild(i).GetComponent<UISlot>();
          HandleSlotClicked(it, slot);
          restored = true;
        }
      }

      // search Active
      for (int i = 0; i < act.Count && !restored; i++)
      {
        var it = act[i];
        if (it != null && it.id == stickyItemId)
        {
          var slot = activeGrid.GetChild(i).GetComponent<UISlot>();
          HandleSlotClicked(it, slot);
          restored = true;
        }
      }
    }

    // Nothing to restore — clear UI
    if (!restored)
    {
      selected = null;
      if (sellButton) sellButton.gameObject.SetActive(false);
      if (detailsPanel) detailsPanel.Clear();
    }
  }


  // Called when a slot is clicked — shows details in the info panel
  private void HandleSlotClicked(UpgradeItem item, UISlot slot)
  {
    selected = slot;
    stickyGroup = slot.group;
    stickyIndex = slot.index;
    stickyItemId = item != null ? item.id : null;   // <-- NEW

    if (detailsPanel)
    {
      if (item != null) { detailsPanel.Show(item); detailsPanel.ShowSellOnly(); }
      else { detailsPanel.Clear(); }
    }
    if (sellButton) sellButton.gameObject.SetActive(item != null);
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
