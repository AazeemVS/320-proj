using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDetailsPanel : MonoBehaviour
{
  [SerializeField] private Image icon;
  [SerializeField] private TMP_Text title;
  [SerializeField] private TMP_Text description;

  public void Show(UpgradeItem item)
  {
    if (item == null) { Clear(); return; }
    if (icon) { icon.enabled = item.icon != null; icon.sprite = item.icon; }
    if (title) title.text = item.displayName;
    if (description) description.text = item.description;
    gameObject.SetActive(true);
  }

  public void Clear()
  {
    if (icon) { icon.enabled = false; icon.sprite = null; }
    if (title) title.text = "";
    if (description) description.text = "Select an upgrade to see details.";
  }
}
