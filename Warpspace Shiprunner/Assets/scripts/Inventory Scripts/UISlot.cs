using UnityEngine;
using UnityEngine.UI;
using System;

public enum SlotGroup { Inventory, Active }

public class UISlot : MonoBehaviour
{
  [HideInInspector] public SlotGroup group;
  [HideInInspector] public int index;

  [SerializeField] private Image icon;
  public Action<UpgradeItem, UISlot> OnSlotClicked;

  private UpgradeItem item;

  public void Set(SlotGroup g, int idx, UpgradeItem i)
  {
    group = g; index = idx; item = i;

    if (icon)
    {
      if (i == null || i.icon == null) { icon.enabled = false; icon.sprite = null; }
      else { icon.enabled = true; icon.sprite = i.icon; }
    }
  }

  public void OnClick()
  {
    OnSlotClicked?.Invoke(item, this);
  }
}
