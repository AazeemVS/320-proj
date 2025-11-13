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
    ExplosiveHitUpgrade = 10,
    ExplosiveKillUpgrade = 11,

    ExtraKillTrigger = 12,
    DamageOnKill = 13,
    CreditsOnKill = 14,
    HealthOnKill = 15,
    VirusDamageBoost = 16,

    PoisonUpgrade = 17,
    HealthUpgrade = 18,
    CreditsWhenHit = 19,

    RailgunUpgrade = 20,
    GattlingGunUpgrade = 21
}

public abstract class Upgrade
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int rarity;
    public int value;
    public string name;
    public Upgrade(Rarity rarity = Rarity.Junk, int value = 0, string name = "Junk", string description = "Does Nothing?") {
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
    public EngineUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier*50, tier + " Fuel Pumps", "Increases Base Movement Speed.") { modifier = (int)tier * 2; }
    public override void OnEquip(player_movement player) { player.moveSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.moveSpeed -= modifier; }
}

//Damage
public class DamageUpgrade : Upgrade {
    public float modifier;
    public DamageUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Improved Munitions", "Increases Attack Damage.") { modifier = (int)tier * .5f; }
    public override void OnEquip(player_movement player) { player.playerDamage += modifier; }
    public override void OnUnequip(player_movement player) { player.playerDamage -= modifier; }
}

//Attack Rate
public class AttackUpgrade : Upgrade {
    public float modifier;
    public AttackUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Cannon Loading", "Reduces Time Between Attacks.") { modifier = 1 - (int)tier * .2f; }
    public override void OnEquip(player_movement player) { player.shootTimerMax *= modifier; }
    public override void OnUnequip(player_movement player) { player.shootTimerMax /= modifier; }
}

//Basic Dash
public class DashUpgrade : Upgrade {
    public DashUpgrade() : base(Rarity.Uncommon, 125, "Dash Module", "Press [SHIFT] To Dash In Movement Direction") { }
    public override void OnEquip(player_movement player) { player.dashEnabled = true; }
    public override void OnUnequip(player_movement player) { player.dashEnabled = false;  }
}

//Dark Souls Dodge Roll
public class SuperDashUpgrade : Upgrade {
    public SuperDashUpgrade() : base(Rarity.Rare, 250, "Phasing Dash Module", "Press [SHIFT] To Dash In Movement Direction. You Are Invulnerable While Dashing.") { }
    public override void OnEquip(player_movement player) { player.dashEnabled = true; player.dashHasDodge = true; }
    public override void OnUnequip(player_movement player) { player.dashEnabled = false; player.dashHasDodge = false; }
}

//Bullet Velocity
public class BulletSpeedUpgrade : Upgrade {
    public float modifier;
    public BulletSpeedUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 20, tier + " Cannon Rifling", "Increases Velocity Of Attacks.") { modifier = (int)tier * 5f; }
    public override void OnEquip(player_movement player) { player.bulletSpeed += modifier; }
    public override void OnUnequip(player_movement player) { player.bulletSpeed -= modifier; }
}

//Piercing Upgrade
public class BulletPierceUpgrade : Upgrade {
    public BulletPierceUpgrade(Rarity tier = Rarity.Common):base(tier, (int)tier*50, tier + " Armor Piercing", "Increases Amount Of Enemies Pierced By Attacks.") { }
    public override void OnEquip(player_movement player) { player.piercing += rarity; }
    public override void OnUnequip(player_movement player) { player.piercing -= rarity; }
}
//I-Frame Upgrade
public class PlayerRecoveryUpgrade : Upgrade {
    public float modifier;
    public PlayerRecoveryUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Reactive Shielding", "Increases Amount Time Invulnerable After Taking Damage.") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.iFrameMax += modifier; }
    public override void OnUnequip(player_movement player) { player.iFrameMax -= modifier; }
}
//Damage after taking hit
public class EnrageUpgrade : Upgrade {
    public float modifier;
    public EnrageUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Defense Protocols", "Briefly Increases Attack Damage After Taking Damage.") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.enragesOnHit = true; player.enrageDamage += modifier; }
    public override void OnUnequip(player_movement player) { player.enragesOnHit = false; player.enrageDamage -= modifier; }
}
//Converts bullet hits to explosions
public class ExplosiveHitUpgrade : Upgrade {
    public float modifier;
    public ExplosiveHitUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Explosive Shells", "Attacks Explode When Hitting Enemies.") { modifier = (rarity - 1) / 2; }
    public override void OnEquip(player_movement player) { player.explodeOnHit = true; player.hitExplosionScale += modifier; }
    public override void OnUnequip(player_movement player) { player.explodeOnHit = false; player.hitExplosionScale -= modifier; }
}

//Explosion on kill
public class ExplosiveKillUpgrade : Upgrade {
    public float modifier;
    public ExplosiveKillUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, "Virus: " + tier + " Reactor Meltdown Override", "Enemies Explode When Killed.") { modifier = (rarity - 1) / 2; }
    public override void OnEquip(player_movement player) { player.explodeOnKill = true; player.killExplosionScale += modifier; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.explodeOnKill = false; player.killExplosionScale -= modifier; player.virusBonus -= 1; }
}

//Second projectile for player
public class ExtraCannonUpgrade : Upgrade {
    private float damageReduction = .75f;
    private float sizeReduction = .8f;
    public ExtraCannonUpgrade() : base(Rarity.Rare,  350, "Extra Cannon", "Gain A Second Cannon, But Reduces Attack Damage.") {}
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


//Kill Retrigger (Reactivates all upgrades that do something on kill
public class ExtraKillTrigger : Upgrade {
    public ExtraKillTrigger() : base(Rarity.Rare, 200, "Virus: Trojan Payload", "Doubles The Effects Of Other Virus (On Kill) Upgrades.") { }
    public override void OnEquip(player_movement player) { player.killTriggers += 1; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.killTriggers -= 1; player.virusBonus -= 1; }
}
//Player gains temporary damage bonus after a kill
public class DamageOnKill : Upgrade {
    private float modifier;
    public DamageOnKill(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, "Virus: " + tier + " Defense Exploit", "Briefly Increases Attack Damage On Enemy Kill.") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.killBoost = true; player.killBoostDamage += modifier; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.killBoost = false; player.killBoostDamage -= modifier; player.virusBonus -= 1; }
}
//Player gains extra money for each kill
public class CreditsOnKill : Upgrade {
    private int modifier;
    public CreditsOnKill(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, "Virus: " + tier + " Credit Scraper", "Gain Extra Credits On Enemy Kill.") { modifier = rarity; }
    public override void OnEquip(player_movement player) { player.extraKillCredits += modifier; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.extraKillCredits -= modifier; player.virusBonus -= 1; }
}
//Player gains health on kill
public class HealthOnKill : Upgrade {
    public HealthOnKill() : base(Rarity.Rare, 1000, "Virus: Shielding Data Extraction", "Gain A Small Amount Of Health On Enemy Kill.") { }
    public override void OnEquip(player_movement player) { player.hasHealthSteal = true; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.hasHealthSteal = false; player.virusBonus -= 1; }

}
//Player gains extra damage proportional to amount of virus (on kill) upgrades equipped
public class VirusDamageBoost : Upgrade {
    public VirusDamageBoost() : base(Rarity.Rare, 200, "Virus: Fatal Error", "Increasse Attack Damage For Each Equipped Virus.") { }
    public override void OnEquip(player_movement player) { player.virusBoost += 1; player.virusBonus += 1; }
    public override void OnUnequip(player_movement player) { player.virusBoost -= 1; player.virusBonus -= 1; }
}
//Player inflicts DoT on hit
public class PoisonUpgrade : Upgrade {
    float modifier;
    public PoisonUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Corrosive Rounds", "Attacks Inflict Damaging Poison.") { modifier = rarity + 1; }
    public override void OnEquip(player_movement player) { player.hasPoison = true; player.poisonLength += modifier; }
    public override void OnUnequip(player_movement player) { player.hasPoison = false; player.poisonLength -= modifier; }
}
//Player has more max health
public class HealthUpgrade : Upgrade {
    float modifier;
    public HealthUpgrade(Rarity tier = Rarity.Common) : base(tier, (int)tier * 50, tier + " Backup Shields", "Increases Maximum Health.") { modifier = rarity * 2; }
    public override void OnEquip(player_movement player) { player.maxHealth += modifier; player.ChangeHealth(modifier); }
    public override void OnUnequip(player_movement player) { player.maxHealth -= modifier; player.ChangeHealth(0); }
}
//Player gains credits on taking damage
public class CreditsWhenHit : Upgrade {
    float modifier;
    public CreditsWhenHit(Rarity tier = Rarity.Common) : base(tier, (int)tier*25, tier + " Insurance Plan", "Gain Credits After Taking Damage.") { modifier = rarity * 5; }
    public override void OnEquip(player_movement player) { player.insuranceCreditsScalar += modifier; }
    public override void OnUnequip(player_movement player) { player.insuranceCreditsScalar -= modifier; }
}
//Railcannon Upgrade (Shuffles Gun Stats)
public class RailgunUpgrade : Upgrade {
    public RailgunUpgrade() : base(Rarity.Uncommon, 100, "Railcannon Adapter", "Greatly Increases Attack Damage And Velocity. Increases Time Between Attacks.") { }
    public override void OnEquip(player_movement player) { player.playerDamageMult *= 3; player.bulletSpeed += 20; player.shootTimerMax *= 2.5f; }
    public override void OnUnequip(player_movement player) { player.playerDamageMult /= 3; player.bulletSpeed -= 20; player.shootTimerMax /= 2.5f; }
}
//+Attack Speed, +Spread
public class GattlingGunUpgrade : Upgrade {
    public GattlingGunUpgrade(): base(Rarity.Uncommon, 100, "Gattling Adapter", "Greatly Decreases Time Between And Accuracy Of Attacks.") { }
    public override void OnEquip(player_movement player) { player.shootTimerMax *= .5f; player.spread += 12f; player.bulletSize *= .8f; }
    public override void OnUnequip(player_movement player) { player.shootTimerMax /= .5f; player.spread -= 12f; player.bulletSize /= .8f; }
}


