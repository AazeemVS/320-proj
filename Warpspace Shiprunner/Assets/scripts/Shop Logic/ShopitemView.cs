using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopItemView : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;

    private System.Action onClick;

    public void SetIcon(Sprite sprite)
    {
        icon.sprite = sprite;
        icon.preserveAspect = true;
    }

    public void SetOnClick(System.Action click)
    {
        onClick = click;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick?.Invoke();
    }
}
