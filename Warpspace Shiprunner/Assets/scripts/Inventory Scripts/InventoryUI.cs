using UnityEngine;

public class InventoryUI : MonoBehaviour
{
  [SerializeField] private Transform inventoryGrid;
  [SerializeField] private Transform activeGrid;
  [SerializeField] private UISlot slotPrefab;
  [SerializeField] private int inventorySlots = 20;
  [SerializeField] private int activeSlots = 5;

  [Header("Details")]
  [SerializeField] private UpgradeDetailsPanel detailsPanel;

  private UISlot selected;

  void OnEnable()
  {
    InventoryManager.Instance.OnChanged += Rebuild;
    Rebuild();
    if (detailsPanel) detailsPanel.Clear();
  }

  public void Rebuild()
  {
    foreach (Transform t in inventoryGrid) Destroy(t.gameObject);
    foreach (Transform t in activeGrid) Destroy(t.gameObject);

    var inv = InventoryManager.Instance.Inventory;
    var act = InventoryManager.Instance.Active;

    for (int i = 0; i < inventorySlots; i++)
    {
      var slot = Instantiate(slotPrefab, inventoryGrid);
      var item = i < inv.Count ? inv[i] : null;
      slot.Set(SlotGroup.Inventory, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }

    for (int i = 0; i < activeSlots; i++)
    {
      var slot = Instantiate(slotPrefab, activeGrid);
      var item = i < act.Count ? act[i] : null;
      slot.Set(SlotGroup.Active, i, item);
      slot.OnSlotClicked = HandleSlotClicked;
    }
  }

  private void HandleSlotClicked(UpgradeItem item, UISlot slot)
  {
    selected = slot;
    if (detailsPanel) detailsPanel.Show(item);
  }
}
