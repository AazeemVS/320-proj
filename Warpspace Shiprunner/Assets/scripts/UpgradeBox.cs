using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class UpgradeBox : MonoBehaviour
{
    public enum Rarity { Common, Rare, Epic }
    public enum BoxType { Small, Medium, Large }

    [System.Serializable]
    public class UpgradeItem
    {
        public string itemName;
        public string description;
        public Rarity rarity;
    }

    [Header("Upgrade Data")]
    public List<UpgradeItem> allUpgrades = new List<UpgradeItem>();
    public List<TextMeshProUGUI> upgradeTextSlots;

    private void Start()
    {
        // Add dummy items with rarities

        // Commons
        allUpgrades.Add(new UpgradeItem { itemName = "Speed Boost", description = "Move 10% faster.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Extra Health", description = "Gain +1 max HP.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Quick Shot", description = "Reduce shoot cooldown by 10%.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Accuracy", description = "Bullet spread reduced slightly.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Dash Fuel", description = "Dash cooldown reduced by 15%.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Regen I", description = "Regenerate 0.5 HP per 10 seconds.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Reload Speed", description = "Faster reload between shots.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Armor Shard", description = "Reduce damage taken by 5%.", rarity = Rarity.Common });
        allUpgrades.Add(new UpgradeItem { itemName = "Sprint Control", description = "Smoother movement while dashing.", rarity = Rarity.Common });

        // Rares
        allUpgrades.Add(new UpgradeItem { itemName = "Double Shot", description = "Fire 2 bullets instead of 1.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Chain Reaction", description = "Killed enemies explode.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Critical Hits", description = "10% chance to deal double damage.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Regen II", description = "Regenerate 1 HP per 10 seconds.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Bouncy Bullets", description = "Bullets bounce once.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Auto Target", description = "Slight homing toward enemies.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Shield Burst", description = "Brief invincibility on low health.", rarity = Rarity.Rare });
        allUpgrades.Add(new UpgradeItem { itemName = "Piercing Rounds", description = "Bullets go through 1 enemy.", rarity = Rarity.Rare });

        // Epics
        allUpgrades.Add(new UpgradeItem { itemName = "Explosion Rounds", description = "Bullets explode on impact.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Fire Trail", description = "Leave fire behind while moving.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Bullet Storm", description = "Shoot 3 bullets in spread pattern.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Time Warp", description = "Brief slow-mo after dashing.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Lightning Chain", description = "Bullets chain to nearby enemies.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Health Bloom", description = "Heals +2 HP when opening a chest.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Second Wind", description = "Survive a killing blow once.", rarity = Rarity.Epic });
        allUpgrades.Add(new UpgradeItem { itemName = "Gravity Bomb", description = "Bullets pull enemies inward.", rarity = Rarity.Epic });
    }


    public void GenerateUpgradeChoices(BoxType boxType)
    {
        List<UpgradeItem> selectedUpgrades = new List<UpgradeItem>();

        // Define rarity distribution per box type
        int commons = 0, rares = 0, epics = 0;

        switch (boxType)
        {
            case BoxType.Small:
                commons = 5;
                break;
            case BoxType.Medium:
                commons = 3;
                rares = 2;
                break;
            case BoxType.Large:
                commons = 2;
                rares = 2;
                epics = 1;
                break;
        }

        selectedUpgrades.AddRange(PickRandomUpgrades(Rarity.Common, commons));
        selectedUpgrades.AddRange(PickRandomUpgrades(Rarity.Rare, rares));
        selectedUpgrades.AddRange(PickRandomUpgrades(Rarity.Epic, epics));

        // Display in UI
        for (int i = 0; i < upgradeTextSlots.Count; i++)
        {
            if (i < selectedUpgrades.Count)
            {
                var item = selectedUpgrades[i];
                upgradeTextSlots[i].text = $"{item.itemName} [{item.rarity}]\n<color=#AAAAAA>{item.description}</color>";
            }
            else
            {
                upgradeTextSlots[i].text = "";
            }
        }
    }

    private List<UpgradeItem> PickRandomUpgrades(Rarity rarity, int amount)
    {
        List<UpgradeItem> pool = allUpgrades.Where(i => i.rarity == rarity).ToList();
        List<UpgradeItem> picked = new List<UpgradeItem>();

        for (int i = 0; i < amount && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            picked.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return picked;
    }

    public void OpenSmallBox()
    {
        GenerateUpgradeChoices(BoxType.Small);
    }

    public void OpenMediumBox()
    {
        GenerateUpgradeChoices(BoxType.Medium);
    }

    public void OpenLargeBox()
    {
        GenerateUpgradeChoices(BoxType.Large);
    }
}
