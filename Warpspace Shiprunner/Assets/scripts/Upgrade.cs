using UnityEngine;

public abstract class Upgrade : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int rarity;
    public int value;
    protected player_movement player;
    public 
    void Start()
    {
        
    }
    public abstract void OnEquip();
    public abstract void OnUnequip();
}

public class EngineUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip() { player.moveSpeed += modifier; }
    public override void OnUnequip() { player.moveSpeed -= modifier; }
}

public class DamageUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip() { player.playerDamage += modifier; }
    public override void OnUnequip() { player.playerDamage -= modifier; }
}

public class AttackUpgrade : Upgrade {
    public float modifier;
    public override void OnEquip() { player.shootTimerMax += modifier; }
    public override void OnUnequip() { player.shootTimerMax -= modifier; }
}