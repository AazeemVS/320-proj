using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class DebugInventory : MonoBehaviour
{
    public PlayerInventory inventory;
    public player_movement playerLogic;
    [SerializeField] Button button;
    public List<Upgrade> playerUpgrades = new List<Upgrade>();


    private void Start() {
        inventory = gameObject.GetComponent<PlayerInventory>();
        playerLogic = gameObject.GetComponent<player_movement>();
        if (playerLogic == null) enabled = false;
        playerUpgrades.Add(new HealthOnKill());
        playerUpgrades.Add(new SuperDashUpgrade());
        playerUpgrades.Add(new PoisonUpgrade());
        playerUpgrades.Add(new DamageOnKill());
        playerUpgrades.Add(new ExtraKillTrigger());
        playerUpgrades.Add(new GattlingGunUpgrade());
        playerUpgrades.Add(new ExplosiveHitUpgrade(Rarity.Rare));
        playerUpgrades.Add(new DashUpgrade());
        playerUpgrades.Add(new ExtraCannonUpgrade());
        playerUpgrades.Add(new VirusDamageBoost());

        Canvas canvas = FindAnyObjectByType<Canvas> ();
        for(int i = 0; i <playerUpgrades.Count; i++) {
            Button newButton = Instantiate(button, canvas.transform);
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = playerUpgrades[i].name;
            newButton.transform.position = new Vector2(80, 15 + i * 30);
            Upgrade u = playerUpgrades[i];
            newButton.onClick.AddListener(() => inventory.EquipUpgrade(u, true));
            
            inventory.GainItem(playerUpgrades[i]);
        }
    }

    public void Update() {
        
    }
}
