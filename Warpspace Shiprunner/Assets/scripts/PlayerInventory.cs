using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;


public class PlayerInventory : MonoBehaviour
{
    //currently assumes no slot specific/incompatible upgrades
    public List<Upgrade> inventory = new List<Upgrade>();
    public List<Upgrade> activeUpgrades = new List<Upgrade>();
    private player_movement player;
    private int maxInventory =20;
    private int maxUpgrades = 5;

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
    public bool EquipUpgrade(Upgrade newUpgrade, bool removeIfActive = false) {
        if (activeUpgrades.Count < maxUpgrades && !activeUpgrades.Contains(newUpgrade)) {
            activeUpgrades.Add(newUpgrade);
            newUpgrade.OnEquip(player);
            return true;
        } else if (removeIfActive && activeUpgrades.Contains(newUpgrade)) {
            UnequipUpgrade(newUpgrade);
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
