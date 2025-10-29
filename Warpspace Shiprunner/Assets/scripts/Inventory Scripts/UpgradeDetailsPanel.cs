using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeDetailsPanel : MonoBehaviour
{
    // UI elements for the upgrade info
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    // NEW: buttons that sit on top of each other
    [Header("Actions")]
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;

    void Awake()
    {
        // Ensure clean default
        HideBothButtons();
        SetDefaultMessage();
    }

    void OnEnable()
    {
        // In case the panel is re-enabled later
        HideBothButtons();
    }

    // Displays the selected upgrade's details
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
        SetDefaultMessage();
        // When cleared, keep both buttons hidden
        HideBothButtons();
    }

    // --- helpers ---
    private void HideBothButtons()
    {
        if (buyButton) buyButton.gameObject.SetActive(false);
        if (sellButton) sellButton.gameObject.SetActive(false);
    }

    private void SetDefaultMessage()
    {
        if (description) description.text = "Select an upgrade to see details.";
    }

    // (We’ll use these in the next step)
    public void ShowSellOnly() { if (sellButton) sellButton.gameObject.SetActive(true); if (buyButton) buyButton.gameObject.SetActive(false); }
    public void ShowBuyOnly() { if (buyButton) buyButton.gameObject.SetActive(true); if (sellButton) sellButton.gameObject.SetActive(false); }
}
