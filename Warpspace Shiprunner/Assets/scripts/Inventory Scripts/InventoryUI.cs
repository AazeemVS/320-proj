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
        if (selected == null) return;

        var mgr = InventoryManager.Instance;
        if (mgr == null) return;

        var g = selected.group;
        var idx = selected.index;

        // Safety checks against shifting/empty slots
        if (g == SlotGroup.Inventory)
        {
            if (idx < 0 || idx >= mgr.Inventory.Count || mgr.Inventory[idx] == null) return;
            mgr.RemoveFromInventoryAt(idx);
        }
        else // SlotGroup.Active
        {
            if (idx < 0 || idx >= mgr.Active.Count || mgr.Active[idx] == null) return;
            mgr.RemoveFromActiveAt(idx);
        }

        // OnChanged from the manager will queue a repaint.
        // Clear selection + hide sell immediately for snappy UX.
        selected = null;
        if (sellButton) sellButton.gameObject.SetActive(false);
        if (detailsPanel) detailsPanel.Clear();
    }

    // Rebuilds both grids (active and inventory grids)
    public void Rebuild()
    {
        // Clear old slots
        foreach (Transform t in inventoryGrid) Destroy(t.gameObject);
        foreach (Transform t in activeGrid) Destroy(t.gameObject);

        var mgr = InventoryManager.Instance;
        var inv = mgr.Inventory;
        var act = mgr.Active;

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

        // After a full rebuild, clear selection + hide Sell
        selected = null;
        if (sellButton) sellButton.gameObject.SetActive(false);
    }

    // Called when a slot is clicked — shows details in the info panel
    private void HandleSlotClicked(UpgradeItem item, UISlot slot)
    {
        selected = slot;

        if (detailsPanel)
        {
            if (item != null)
            {
                detailsPanel.Show(item);      // fills icon/title/description
                detailsPanel.ShowSellOnly();  // your panel-specific visibility logic
            }
            else
            {
                detailsPanel.Clear();
            }
        }

        // Show Sell only if there is an item in this slot
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
