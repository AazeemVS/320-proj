using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public enum SlotGroup { Inventory, Active }

[RequireComponent(typeof(RectTransform))]
public class UISlot : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public SlotGroup group;
    [HideInInspector] public int index;

    [SerializeField] private Image icon;        // item icon (child)
    [SerializeField] private Image frame;       // hover highlight (optional)
    [SerializeField] private Canvas dragCanvas; // top-most canvas (assign in Inspector or auto-find)

    private static UISlot dragSource;
    private static UpgradeItem dragItem;
    private static Image dragGhost;
  

    public Action<UpgradeItem, UISlot> OnSlotClicked;

    private UpgradeItem item;

    void Awake()
    {
        // Ensure the slot itself can receive drops (must have a raycastable Graphic)
        var g = GetComponent<Graphic>();
        if (g == null)
        {
            var slotImg = gameObject.AddComponent<Image>();
            slotImg.color = new Color(0, 0, 0, 0.001f); // nearly transparent but still hit-testable
            slotImg.raycastTarget = true;
        }
        else
        {
            g.raycastTarget = true;
        }

        // Auto-find a top canvas if not assigned
        if (dragCanvas == null)
        {
            var root = GetComponentInParent<Canvas>();
            dragCanvas = root != null ? root.rootCanvas : null;
        }
    }

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

    public void OnClick() => OnSlotClicked?.Invoke(item, this);

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || icon == null) return;

        dragSource = this;
        dragItem = item;

        // 🔹 Show the details for the item being dragged
        OnSlotClicked?.Invoke(item, this);

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


    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost != null)
            ((RectTransform)dragGhost.transform).position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDrag();
        SetHighlight(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (dragSource == null || dragItem == null) return;

        var mgr = InventoryManager.Instance;
        if (mgr == null) return;

        bool ok = (dragSource.group != this.group)
            ? mgr.MoveBetween(dragSource.group, dragSource.index, this.group, this.index)
            : mgr.ReorderWithin(this.group, dragSource.index, this.index);

        if (ok)
        {
            // Select the destination slot so details + Sell show for the dropped item
            OnSlotClicked?.Invoke(dragItem, this);
        }

        SetHighlight(false);
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (dragSource != null) SetHighlight(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetHighlight(false);
    }

    private void SetHighlight(bool on)
    {
        if (frame != null) frame.enabled = on;
    }

  private static void CleanupDrag()
  {
    if (dragGhost != null) Destroy(dragGhost.gameObject);
    dragGhost = null;

    if (dragSource != null)
    {
      // restore icon visuals
      if (dragSource.icon != null)
      {
        var c = dragSource.icon.color; c.a = 1f;
        dragSource.icon.color = c;
        dragSource.icon.raycastTarget = true;
      }

      // ✅ restore raycast blocking on the source slot
      var cg = dragSource.GetComponent<CanvasGroup>();
      if (cg) cg.blocksRaycasts = true;
    }

    dragItem = null;
    dragSource = null;
  }


  private static RectTransform GetOrCreateDragLayer(Canvas topCanvas)
    {
        if (topCanvas == null)
        {
            // last resort: make a screen-space overlay canvas
            var fallback = new GameObject("DragCanvas(Fallback)", typeof(Canvas), typeof(GraphicRaycaster));
            var c = fallback.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            topCanvas = c;
        }

        var t = topCanvas.transform.Find("DragLayer") as RectTransform;
        if (t != null) return t;

        var go = new GameObject("DragLayer", typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(topCanvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }
}
