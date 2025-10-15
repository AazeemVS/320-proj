using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class UpgradeBox : MonoBehaviour
{
    [System.Serializable]
    public class UpgradeItem
    {
        public string itemName;
        public string description;
    }

    [Header("Upgrade Data")]
    public List<UpgradeItem> allUpgrades = new List<UpgradeItem>();
    public int numberOfChoices = 5;

    [Header("UI Elements")]
    public List<TextMeshProUGUI> upgradeTextSlots; // Assign in inspector

    private void Start()
    {
        // Dummy upgrades
        allUpgrades.Add(new UpgradeItem { itemName = "Improved Engines", description = "+?% Movement Speed" });
        allUpgrades.Add(new UpgradeItem { itemName = "Improved Guns", description = "+?% Damage" });
        allUpgrades.Add(new UpgradeItem { itemName = "Gun Efficiency", description = "+?% Attack Speed" });
        allUpgrades.Add(new UpgradeItem { itemName = "Bullet Speed", description = "+?% Player Bullet Speed" });
        allUpgrades.Add(new UpgradeItem { itemName = "Scrapping Bay", description = "+? Credits on enemy kill" });
        allUpgrades.Add(new UpgradeItem { itemName = "Multi-Gun", description = "Adds second player projectile" });
        allUpgrades.Add(new UpgradeItem { itemName = "Nano-Bot Hull", description = "+? Health Regen After Level Completion" });
    }

    public void GenerateUpgradeChoices()
    {
        // Safety checks
        if (upgradeTextSlots.Count < numberOfChoices)
        {
            Debug.LogWarning("Not enough UI text slots for upgrade choices.");
            return;
        }

        // Picks random upgrades
        List<UpgradeItem> tempList = new List<UpgradeItem>(allUpgrades);
        for (int i = 0; i < numberOfChoices; i++)
        {
            if (tempList.Count == 0) break;
            int index = Random.Range(0, tempList.Count);
            UpgradeItem chosen = tempList[index];
            tempList.RemoveAt(index);

            // Displays it
            upgradeTextSlots[i].text = $"{chosen.itemName}\n<color=#AAAAAA>{chosen.description}</color>";
        }
    }
}
