using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDetailsPanel : MonoBehaviour
{
  // UI elements for the upgrade info
  [SerializeField] private Image icon;          // Upgrade icon image
  [SerializeField] private TMP_Text title;      // Upgrade name text
  [SerializeField] private TMP_Text description; // Upgrade description text

  // Displays the selected upgrade's details
  public void Show(UpgradeItem item)
  {
    if (item == null) { Clear(); return; } // If no item, clear the panel

    // Update icon, title, and description
    if (icon) { icon.enabled = item.icon != null; icon.sprite = item.icon; }
    if (title) title.text = item.displayName;
    if (description) description.text = item.description;

    // Make sure the panel is visible
    gameObject.SetActive(true);
  }

  // Clears the panel text and icon
  public void Clear()
  {
    if (icon) { icon.enabled = false; icon.sprite = null; }
    if (title) title.text = "";
    if (description) description.text = "Select an upgrade to see details.";
  }
}
