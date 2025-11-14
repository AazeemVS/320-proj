using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade Item")]
public class UpgradeItem : ScriptableObject
{
  [SerializeField] public string id;   // <-- unique identifier for tracking items
  public string displayName;
  [TextArea] public string description;
  public Sprite icon;

  public int tempUpgradeID;
  public Upgrade[] tempUpgrades =
  {
        new EngineUpgrade(),        // 0
        new DamageUpgrade(),        // 1
        new AttackUpgrade(),        // 2
        new DashUpgrade(),          // 3
        new SuperDashUpgrade(),     // 4
        new BulletSpeedUpgrade(),   // 5
        new BulletPierceUpgrade(),  // 6
        new PlayerRecoveryUpgrade(),// 7
        new EnrageUpgrade(),        // 8
        new ExtraCannonUpgrade(),   // 9
        new ExplosiveHitUpgrade(),  // 10
        new ExplosiveKillUpgrade(), // 11
        new ExtraKillTrigger(),     // 12
        new DamageOnKill(),         // 13
        new CreditsOnKill(),        // 14
        new HealthOnKill(),         // 15
        new VirusDamageBoost(),     // 16
        new PoisonUpgrade(),        // 17
        new HealthUpgrade(),        // 18
        new CreditsWhenHit(),       // 19
        new RailgunUpgrade(),
        new GattlingGunUpgrade(),
    };

  public Upgrade upgrade;

  // Ensures every item has a persistent, unique ID for inventory tracking
  public void EnsureId()
  {
    if (string.IsNullOrWhiteSpace(id))
      id = Guid.NewGuid().ToString("N");
  }

  // These automatically make sure IDs exist even if you forget manually
  private void OnValidate() => EnsureId();
  private void OnEnable() => EnsureId();
}
