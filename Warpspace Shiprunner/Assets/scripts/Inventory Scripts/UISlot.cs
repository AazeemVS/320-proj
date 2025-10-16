using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using Mono.Cecil.Cil;

// Identifies which grid this slot belongs to
public enum SlotGroup { Inventory, Active }

public class UISlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
  [HideInInspector] public SlotGroup group;  // Which grid this slot is in
  [HideInInspector] public int index;        // Slot index in its grid

  [SerializeField] private Image icon;       // The item's icon image
  [SerializeField] private Image frame;      // Optional highlight border

  // Static drag context (shared during dragging)
  private static UISlot dragSource;          // Slot being dragged from
  private static UpgradeItem dragItem;       // Item being dragged
  private static Image dragGhost;            // Floating image under cursor

  public Action<UpgradeItem, UISlot> OnSlotClicked; // Callback for click

  private UpgradeItem item;                  // Item currently in this slot

  // Public API from UI builder 
  public void Set(SlotGroup g, int idx, UpgradeItem i)
  {
    group = g; index = idx; item = i;

    if (icon)
    {
      if (i == null || i.icon == null) { icon.enabled = false; icon.sprite = null; }
      else { icon.enabled = true; icon.sprite = i.icon; }
    }
    SetHighlight(false);
  }

  // Handles button click
  public void OnClick() => OnSlotClicked?.Invoke(item, this);

  // Start dragging
  public void OnBeginDrag(PointerEventData eventData)
  {
    if (item == null || icon == null) return;

    dragSource = this;
    dragItem = item;

    // Ensure CanvasGroup exists, then disable raycast blocking
    if (!TryGetComponent<CanvasGroup>(out var cg))
      cg = gameObject.AddComponent<CanvasGroup>();
      cg.blocksRaycasts = false;

    // Create ghost icon in topmost DragLayer
    var rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    var dragLayer = GetOrCreateDragLayer(rootCanvas);

    var go = new GameObject("DragGhost", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
    go.transform.SetParent(dragLayer, false);
    var img = go.GetComponent<Image>();
    img.sprite = icon.sprite;
    img.raycastTarget = false;
    img.color = new Color(1, 1, 1, 1);
    dragGhost = img;

    var rtGhost = (RectTransform)go.transform;
    var rtIcon = (RectTransform)icon.transform;
    rtGhost.sizeDelta = rtIcon.rect.size;
    rtGhost.position = eventData.position;

    // visual dim on source
    icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, 0.6f);
  }

  // While dragging, moves the ghost to mouse position
  public void OnDrag(PointerEventData eventData)
  {
    if (dragGhost != null)
      ((RectTransform)dragGhost.transform).position = eventData.position;
  }

  // When drag ends (The mouse is released)
  public void OnEndDrag(PointerEventData eventData)
  {
    CleanupDrag();       // your helper that destroys ghost + restores alpha + blocksRaycasts true
    SetHighlight(false);
  }


  // When something is dropped on this slot
  public void OnDrop(PointerEventData eventData)
  {
    if (dragSource == null || dragItem == null) return;

    // Move between or reorder within grids
    bool ok = (dragSource.group != this.group)
        ? InventoryManager.Instance.MoveBetween(dragSource.group, dragSource.index, this.group, this.index)
        : InventoryManager.Instance.ReorderWithin(this.group, dragSource.index, this.index);

    if (!ok) Debug.Log("Drop failed (capacity/index).");
    SetHighlight(false);
  }


  // Highlight slot when hovered over during drag
  public void OnPointerEnter(PointerEventData eventData)
  {
    if (dragSource != null) SetHighlight(true);
  }

  // Remove highlight when cursor leaves
  public void OnPointerExit(PointerEventData eventData)
  {
    SetHighlight(false);
  }

  // Toggle frame highlight visibility
  private void SetHighlight(bool on)
  {
    if (frame != null)
    {
      frame.enabled = on;
    }
  }

  // Clean up after drag ends
  private static void CleanupDrag()
  {
    if (dragGhost != null) Destroy(dragGhost.gameObject);
    dragGhost = null;

    if (dragSource != null && dragSource.icon != null)
      dragSource.icon.color = new Color(
          dragSource.icon.color.r,
          dragSource.icon.color.g,
          dragSource.icon.color.b,
          1f);

    // restore raycast blocking on the source
    var cg = dragSource ? dragSource.GetComponent<CanvasGroup>() : null;
    if (cg) cg.blocksRaycasts = true;

    dragItem = null;
    dragSource = null;
  }


  // Finds or creates a top-level canvas layer for dragging
  private static RectTransform GetOrCreateDragLayer(Canvas root)
  {
    var t = root.transform.Find("DragLayer") as RectTransform;
    if (t != null) return t;

    var go = new GameObject("DragLayer", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
    var rt = go.GetComponent<RectTransform>();
    rt.SetParent(root.transform, false);
    rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
    rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

    var c = go.GetComponent<Canvas>();
    c.overrideSorting = true;
    c.sortingOrder = 9999; // always on top

    return rt;
  }
}
