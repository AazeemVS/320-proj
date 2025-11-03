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
        new AttackUpgrade(), new EngineUpgrade(), new DamageUpgrade(),
        new DashUpgrade(), new BulletSpeedUpgrade(), new ExtraCannonUpgrade()
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
