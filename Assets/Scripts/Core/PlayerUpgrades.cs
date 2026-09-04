using UnityEngine;

/// <summary>
/// Permanent upgrades, bought with the stars collected across every run.
///
/// This replaces the milestone version, where upgrades arrived on their own at
/// fixed star totals. Spending is the more interesting shape: the player picks
/// what to improve, an early heart or a bigger magazine, and a run that ends
/// badly still pays into something they chose.
///
/// Stars earned are never taken away. What is stored is how many have been
/// spent, so the balance is earned minus spent and the run counter on the HUD
/// stays honest.
/// </summary>
public static class PlayerUpgrades
{
    private const string TotalStarsKey = "totalStarCounter";
    private const string SpentStarsKey = "starsSpent";
    private const string LevelKeyPrefix = "upgrade_";

    /// <summary>One thing that can be bought, possibly more than once.</summary>
    public readonly struct Upgrade
    {
        public readonly string Id;
        public readonly string Name;
        public readonly string Effect;
        public readonly int MaxLevel;
        public readonly int FirstCost;
        public readonly int CostStep;

        public Upgrade(string id, string name, string effect, int maxLevel, int firstCost, int costStep)
        {
            Id = id;
            Name = name;
            Effect = effect;
            MaxLevel = maxLevel;
            FirstCost = firstCost;
            CostStep = costStep;
        }

        /// <summary>What the next level costs, given how many are owned.</summary>
        public int CostAt(int owned)
        {
            return FirstCost + CostStep * owned;
        }
    }

    /// <summary>
    /// Costs rise per level, so the first heart is cheap enough to reach in a
    /// few runs and the last one is something to work towards.
    /// </summary>
    public static readonly Upgrade[] Catalogue =
    {
        new Upgrade("heart", "Extra heart", "+1 heart", 2, 40, 90),
        new Upgrade("ammo", "Bigger magazine", "+6 rounds", 3, 30, 40),
        new Upgrade("magnet", "Longer magnet", "+8 seconds", 3, 35, 45),
        new Upgrade("shield", "Longer shield", "+4 seconds", 3, 35, 45),
        new Upgrade("grace", "Tougher suit", "+0.3s invulnerable", 2, 60, 80),
    };

    /// <summary>Stars collected across every run, before spending.</summary>
    public static int TotalStarsEarned => PlayerPrefs.GetInt(TotalStarsKey, 0);

    /// <summary>Stars already spent in the shop.</summary>
    public static int StarsSpent => PlayerPrefs.GetInt(SpentStarsKey, 0);

    /// <summary>What the player can spend right now.</summary>
    public static int AvailableStars => Mathf.Max(0, TotalStarsEarned - StarsSpent);

    /// <summary>How many levels of an upgrade the player owns.</summary>
    public static int LevelOf(string id)
    {
        return PlayerPrefs.GetInt(LevelKeyPrefix + id, 0);
    }

    /// <summary>The upgrade with this id, or false if there is none.</summary>
    public static bool TryGet(string id, out Upgrade upgrade)
    {
        for (int i = 0; i < Catalogue.Length; i++)
        {
            if (Catalogue[i].Id == id)
            {
                upgrade = Catalogue[i];
                return true;
            }
        }

        upgrade = default;
        return false;
    }

    /// <summary>True when the player owns every level of everything.</summary>
    public static bool EverythingOwned
    {
        get
        {
            for (int i = 0; i < Catalogue.Length; i++)
            {
                if (LevelOf(Catalogue[i].Id) < Catalogue[i].MaxLevel)
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Buys one level, if it is not already maxed and the stars are there.
    /// Writes straight to disk: a purchase is worth a flush.
    /// </summary>
    public static bool TryBuy(string id)
    {
        Upgrade upgrade;
        if (!TryGet(id, out upgrade))
        {
            return false;
        }

        int owned = LevelOf(id);
        if (owned >= upgrade.MaxLevel)
        {
            return false;
        }

        int cost = upgrade.CostAt(owned);
        if (AvailableStars < cost)
        {
            return false;
        }

        PlayerPrefs.SetInt(LevelKeyPrefix + id, owned + 1);
        PlayerPrefs.SetInt(SpentStarsKey, StarsSpent + cost);
        PlayerPrefs.Save();
        return true;
    }

    // What the game reads. Each is the owned level times its step.

    /// <summary>Extra hearts on top of the base three.</summary>
    public static int ExtraHearts => LevelOf("heart");

    /// <summary>Extra rounds on top of the base magazine.</summary>
    public static int ExtraAmmo => LevelOf("ammo") * 6;

    /// <summary>Extra seconds the magnet lasts.</summary>
    public static float ExtraMagnetSeconds => LevelOf("magnet") * 8f;

    /// <summary>Extra seconds the shield lasts.</summary>
    public static float ExtraShieldSeconds => LevelOf("shield") * 4f;

    /// <summary>Extra seconds of invulnerability after a hit.</summary>
    public static float ExtraGraceSeconds => LevelOf("grace") * 0.3f;
}
