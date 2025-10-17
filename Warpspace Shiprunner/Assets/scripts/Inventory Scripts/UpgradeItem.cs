using UnityEngine;

[CreateAssetMenu(fileName = "NewUpgrade", menuName = "Upgrades/Upgrade Item")]
public class UpgradeItem : ScriptableObject
{
  public string id;
  public string displayName;
  [TextArea] public string description;
  public Sprite icon;
    public int tempUpgradeID;
    public Upgrade[] tempUpgrades = { new AttackUpgrade(), new EngineUpgrade(), new DamageUpgrade(), new DashUpgrade(), new BulletSpeedUpgrade(), new ExtraCannonUpgrade() };
    public Upgrade upgrade;
}
