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
        playerUpgrades.Add(new EngineUpgrade());
        playerUpgrades.Add(new EngineUpgrade(Rarity.Rare));
        playerUpgrades.Add(new DamageUpgrade());
        playerUpgrades.Add(new AttackUpgrade());
        playerUpgrades.Add(new BulletSpeedUpgrade());
        playerUpgrades.Add(new DashUpgrade());
        playerUpgrades.Add(new ExtraCannonUpgrade());

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
