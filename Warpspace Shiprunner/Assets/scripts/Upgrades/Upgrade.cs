using UnityEngine;
public enum Rarity {
    Junk = 0,
    Common = 1,
    Uncommon = 2,
    Rare = 3,
    Legendary = 4
}
public enum upgradeID {
    EngineUpgrade = 0, //increases move speed
    DamageUpgrade = 1, //increases bullet damage
    AttackUpgrade = 2, //increases player attack speed
    DashUpgrade = 3, //enables player dash
    SuperDashUpgrade = 4, //enables player dash with Iframe
    BulletSpeedUpgrade = 5, //increases player bullet velocity
    BulletPierceUpgrade = 6, //increases bullet piercing
    PlayerRecoveryUpgrade = 7, //increases I-frames after being hit
    EnrageUpgrade = 8, //increases damage after being hit
    ExtraCannonUpgrade = 9, //1 extra projectile
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

//Dark Souls Dodge Roll
public class SuperDashUpgrade : Upgrade {
    public SuperDashUpgrade() : base(Rarity.Rare, 500, "Phasing Dash Module") { }
    public override void OnEquip(player_movement player) { player.dashEnabled = true; player.dashHasDodge = true; }
    public override void OnUnequip(player_movement player) { player.dashEnabled = false; player.dashHasDodge = false; }
}

//Bullet Velocity
public class BulletSpeedUpgrade : Upgrade {
    public float modifier;
    public BulletSpeedUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier, tier + " Shot Velocity") { modifier = (int)tier * 5f; }
    public override void OnEquip(player_movement player) { player.bulletSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.bulletSpeed -= modifier; }
}

//Piercing Upgrade
public class BulletPierceUpgrade : Upgrade {
    public BulletPierceUpgrade(Rarity tier = Rarity.Common):base(tier, (int)tier*100, tier + " Armor Piercing") { }
    public override void OnEquip(player_movement player) { player.piercing += rarity; }
    public override void OnUnequip(player_movement player) { player.piercing -= rarity; }
}
//I-Frame Upgrade
public class PlayerRecoveryUpgrade : Upgrade {
    public float modifier;
    public PlayerRecoveryUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Reactive Shielding") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.iFrameMax += modifier; }
    public override void OnUnequip(player_movement player) { player.iFrameMax -= modifier; }
}
//Damage after taking hit
public class EnrageUpgrade : Upgrade {
    public float modifier;
    public EnrageUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Reactive Munitions") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.enragesOnHit = true; player.enrageDamage += modifier; }
    public override void OnUnequip(player_movement player) { player.enragesOnHit = false; player.enrageDamage -= modifier; }
}
//Converts bullet hits to explosions
public class ExplosiveHitUpgrade : Upgrade {
    public float modifier;
    public ExplosiveHitUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Explosive Shells") { modifier = (rarity - 1) / 2; }
    public override void OnEquip(player_movement player) { player.explodeOnHit = true; player.hitExplosionScale += modifier; }
    public override void OnUnequip(player_movement player) { player.explodeOnHit = false; player.hitExplosionScale -= modifier; }
}

//Explosion on kill
public class ExplosiveKillUpgrade : Upgrade {
    public float modifier;
    public ExplosiveKillUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 100, tier + " Reactor Destabilization") { modifier = (rarity - 1) / 2; }
    public override void OnEquip(player_movement player) { player.explodeOnKill = true; player.killExplosionScale += modifier; }
    public override void OnUnequip(player_movement player) { player.explodeOnKill = false; player.killExplosionScale -= modifier; }
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