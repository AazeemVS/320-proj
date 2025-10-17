using UnityEngine;
public enum Rarity {
    Junk = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Legendary = 4
}

public abstract class Upgrade
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int rarity;
    public int value;
    public string name;
    public Upgrade(Rarity rarity = Rarity.Junk, int value = 0, string name = "Junk") {
        this.rarity = (int)rarity;
        this.value = value;
        this.name = name;
    }
    public abstract void OnEquip(player_movement player);
    public abstract void OnUnequip(player_movement player);
}

//Movespeed
public class EngineUpgrade : Upgrade {
    public float modifier;
    public EngineUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier*100, tier + " Engine Upgrade") { modifier = (int)tier * 2; }
    public override void OnEquip(player_movement player) { player.moveSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.moveSpeed -= modifier; }
}

//Damage
public class DamageUpgrade : Upgrade {
    public float modifier;
    public DamageUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Improved Munitions") { modifier = (int)tier * .5f; }
    public override void OnEquip(player_movement player) { player.playerDamage += modifier; }
    public override void OnUnequip(player_movement player) { player.playerDamage -= modifier; }
}

//Attack Rate
public class AttackUpgrade : Upgrade {
    public float modifier;
    public AttackUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Cannon Loading") { modifier = (int)tier * -.05f; }
    public override void OnEquip(player_movement player) { player.shootTimerMax += modifier; }
    public override void OnUnequip(player_movement player) { player.shootTimerMax -= modifier; }
}

//Basic Dash
public class DashUpgrade : Upgrade {
    public DashUpgrade() : base(Rarity.Uncommon, 250, "Dash Module") { }
    public override void OnEquip(player_movement player) { player.dashEnabled = true; }
    public override void OnUnequip(player_movement player) { player.dashEnabled = false;  }
}

//Bullet Velocity
public class BulletSpeedUpgrade : Upgrade {
    public float modifier;
    public BulletSpeedUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier, tier + " Shot Velocity") { modifier = (int)tier * 5f; }
    public override void OnEquip(player_movement player) { player.bulletSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.bulletSpeed -= modifier; }
}

//Second projectile for player
public class ExtraCannonUpgrade : Upgrade {
    private float damageReduction = .75f;
    private float sizeReduction = .8f;
    public ExtraCannonUpgrade() : base(Rarity.Rare,  400, "Extra Cannon") {}
    public override void OnEquip(player_movement player) { 
        player.projectileAmt += 1;
        player.playerDamageMult *= damageReduction;
        player.bulletSize *= sizeReduction;
    }
    public override void OnUnequip(player_movement player) {
        player.projectileAmt -= 1;
        player.playerDamageMult /= damageReduction;
        player.bulletSize /= sizeReduction;
    }
}