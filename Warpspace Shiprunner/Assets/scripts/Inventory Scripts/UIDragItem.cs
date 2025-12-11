using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDragItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform OriginalParent;
    [HideInInspector] public int OriginalIndex;
    [SerializeField] private Canvas dragCanvas;
    private CanvasGroup cg;
    private RectTransform rt;
    private LayoutElement layoutElement;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        layoutElement = GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OriginalParent = transform.parent;
        OriginalIndex = transform.GetSiblingIndex();

        // Make sure this image can be picked up by raycasts on slots
        cg.blocksRaycasts = false;
        cg.alpha = 0.9f;

        // Prevent layout from fighting while dragging
        layoutElement.ignoreLayout = true;

        // Move to a top-level canvas so it follows the mouse cleanly
        if (dragCanvas == null)
            dragCanvas = GetComponentInParent<Canvas>();

        transform.SetParent(dragCanvas.transform, worldPositionStays: true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Follow mouse in screen space
        rt.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;
        cg.alpha = 1f;
        layoutElement.ignoreLayout = false;

        // If we didn’t get dropped on a slot, snap back
        if (transform.parent == dragCanvas.transform)
        {
            transform.SetParent(OriginalParent, worldPositionStays: false);
            transform.SetSiblingIndex(OriginalIndex);
        }
    }
}
