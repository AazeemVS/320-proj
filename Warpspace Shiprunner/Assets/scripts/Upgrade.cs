using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int rarity;
    public int value;
    public 
    void Start()
    {
        
    }
    public abstract void OnEquip(player_movement player);
    public abstract void OnUnequip(player_movement player);
}

public class EngineUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip(player_movement player) { player.moveSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.moveSpeed -= modifier; }
}

public class DamageUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip(player_movement player) { player.playerDamage += modifier; }
    public override void OnUnequip(player_movement player) { player.playerDamage -= modifier; }
}

public class AttackUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip(player_movement player) { player.shootTimerMax += modifier; }
    public override void OnUnequip(player_movement player) { player.shootTimerMax -= modifier; }
}