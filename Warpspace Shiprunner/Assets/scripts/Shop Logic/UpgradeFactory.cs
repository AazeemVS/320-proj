// UpgradeFactory.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public static class UpgradeFactory
{
    // Types that take a Rarity in the ctor:
    private static readonly List<Type> TieredTypes = new()
    {
        typeof(EngineUpgrade),
        typeof(DamageUpgrade),
        typeof(AttackUpgrade),
        typeof(BulletSpeedUpgrade),
        typeof(BulletPierceUpgrade),
        typeof(PlayerRecoveryUpgrade)
    };

    // Types with fixed ctor (no tier param):
    private static readonly List<Type> FixedTypes = new()
    {
        typeof(DashUpgrade),
        typeof(SuperDashUpgrade),
        typeof(ExtraCannonUpgrade)
    };

    // Adjust to taste
    private static readonly Dictionary<Rarity, int> RarityWeights = new()
    {
        { Rarity.Junk,      2 },
        { Rarity.Common,   55 },
        { Rarity.Uncommon, 28 },
        { Rarity.Rare,     12 },
        { Rarity.Legendary, 3 }
    };

    public static Upgrade CreateRandomUpgrade()
    {
        // 70% chance choose a tiered upgrade, else fixed (tweak if you like)
        bool chooseTiered = UnityEngine.Random.value < 0.7f;

        if (chooseTiered)
        {
            var t = TieredTypes[UnityEngine.Random.Range(0, TieredTypes.Count)];
            var r = RollRarity();
            return CreateTiered(t, r);
        }
        else
        {
            var t = FixedTypes[UnityEngine.Random.Range(0, FixedTypes.Count)];
            return (Upgrade)Activator.CreateInstance(t);
        }
    }

    private static Upgrade CreateTiered(Type t, Rarity rarity)
    {
        var ctor = t.GetConstructor(new[] { typeof(Rarity) });
        if (ctor == null)
        {
            Debug.LogError($"No (Rarity) ctor for {t.Name}");
            return null;
        }
        return (Upgrade)ctor.Invoke(new object[] { rarity });
    }

    private static Rarity RollRarity()
    {
        int total = 0;
        foreach (var kv in RarityWeights) total += kv.Value;
        int roll = UnityEngine.Random.Range(0, total);
        foreach (var kv in RarityWeights)
        {
            if (roll < kv.Value) return kv.Key;
            roll -= kv.Value;
        }
        return Rarity.Common;
    }
}
