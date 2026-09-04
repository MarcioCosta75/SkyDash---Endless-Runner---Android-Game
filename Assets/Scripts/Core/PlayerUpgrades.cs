using UnityEngine;

/// <summary>
/// Permanent upgrades earned by the running star total.
///
/// Stars were being counted and saved for the life of the project and never
/// spent on anything, so a run had nothing to show for itself but a number.
/// Passing a milestone now improves the astronaut for good, which gives a
/// reason to collect stars on a run that is already going badly, and a reason
/// to come back after one that ended early.
///
/// Milestones rather than a shop: the reward arrives on its own, with no menu
/// to build and no currency to spend or regret spending.
/// </summary>
public static class PlayerUpgrades
{
    private const string TotalStarCounterKey = "totalStarCounter";

    /// <summary>A milestone and what it grants.</summary>
    public readonly struct Milestone
    {
        public readonly int Stars;
        public readonly string Description;

        public Milestone(int stars, string description)
        {
            Stars = stars;
            Description = description;
        }
    }

    /// <summary>
    /// In order. Each is reached once and kept, so a player who has collected
    /// 400 stars has everything up to 300.
    /// </summary>
    public static readonly Milestone[] Milestones =
    {
        new Milestone(50, "Extra heart"),
        new Milestone(150, "Bigger magazine"),
        new Milestone(300, "Longer magnet"),
        new Milestone(500, "Longer shield"),
        new Milestone(800, "Fourth heart"),
    };

    /// <summary>Stars collected across every run.</summary>
    public static int TotalStars => PlayerPrefs.GetInt(TotalStarCounterKey, 0);

    /// <summary>Extra hearts on top of the base three.</summary>
    public static int ExtraHearts
    {
        get
        {
            int extra = 0;
            if (Reached(0)) extra++;   // 50 stars
            if (Reached(4)) extra++;   // 800 stars
            return extra;
        }
    }

    /// <summary>Extra rounds on top of the base magazine.</summary>
    public static int ExtraAmmo => Reached(1) ? 6 : 0;

    /// <summary>Extra seconds the magnet lasts.</summary>
    public static float ExtraMagnetSeconds => Reached(2) ? 10f : 0f;

    /// <summary>Extra seconds the shield lasts.</summary>
    public static float ExtraShieldSeconds => Reached(3) ? 5f : 0f;

    /// <summary>True once the star total has passed this milestone.</summary>
    public static bool Reached(int index)
    {
        return index >= 0
               && index < Milestones.Length
               && TotalStars >= Milestones[index].Stars;
    }

    /// <summary>
    /// The next milestone the player has not reached, or false when they have
    /// all of them.
    /// </summary>
    public static bool TryGetNext(out Milestone next)
    {
        int total = TotalStars;

        for (int i = 0; i < Milestones.Length; i++)
        {
            if (total < Milestones[i].Stars)
            {
                next = Milestones[i];
                return true;
            }
        }

        next = default;
        return false;
    }
}
