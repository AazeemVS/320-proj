using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    //currently assumes no slot specific/incompatible upgrades
    public List<Upgrade> inventory;
    public List<Upgrade> activeUpgrades;
    private player_movement player;
    private int maxInventory;
    private int maxUpgrades;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponentInParent<player_movement>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Add item/upgrade to player inventory
    public bool GainItem(Upgrade newUpgrade) {
        if(inventory.Count < maxInventory) {
            inventory.Add(newUpgrade);
            return true;
        } 
        return false;
    }
    //Remove item/upgrade from player inventory
    public void RemoveItem(Upgrade newUpgrade) {
        inventory.Remove(newUpgrade);
    }
    //Equip an upgrade from the player inventory
    public bool EquipUpgrade(Upgrade newUpgrade) {
        if (activeUpgrades.Count < maxUpgrades) {
            activeUpgrades.Add(newUpgrade);
            newUpgrade.OnEquip(player);
            return true;
        }
        return false;
    }
    //Unequip a currently active upgrade
    public void UnequipUpgrade(Upgrade newUpgrade) {
        //Make sure were not calling an unequipped item and nerfing the player
        if (activeUpgrades.Contains(newUpgrade)) {
            activeUpgrades.Remove(newUpgrade);
            newUpgrade.OnUnequip(player);
        }
    }
}
