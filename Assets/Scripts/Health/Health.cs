using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player hit points and the heart row that shows them.
///
/// A hit grants a short spell of invulnerability. Without it, two obstacles
/// arriving close together take two hearts for what the player reads as a
/// single mistake, and a full run can end in half a second.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 3;
    [Tooltip("Seconds of invulnerability after a hit.")]
    [SerializeField]
    private float invulnerabilityDuration = 1.2f;

    [Tooltip("Width the heart row may use, in canvas units, measured from the "
             + "first heart's left edge. The score sits at the centre, so this "
             + "has to stop short of it.")]
    [SerializeField]
    private float rowWidth = 300f;

    [Header("Heart display")]
    [SerializeField]
    private Image[] hearts;
    [SerializeField]
    private Sprite fullHeart;
    [SerializeField]
    private Sprite emptyHeart;

    /// <summary>Raised once, when health reaches zero.</summary>
    public event Action Died;

    /// <summary>Raised when a hit lands, with the seconds of grace that follow.</summary>
    public event Action<float> Hurt;

    /// <summary>Raised when a heart is gained.</summary>
    public event Action Healed;

    private int health;
    private bool isDead;
    private float invulnerableUntil;

    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private void Awake()
    {
        // A max of zero would mean a player that can never be hurt and never
        // dies, so the run would have no end.
        maxHealth = Mathf.Max(1, maxHealth) + PlayerUpgrades.ExtraHearts;
        invulnerabilityDuration += PlayerUpgrades.ExtraGraceSeconds;
        health = maxHealth;
    }

    private void Start()
    {
        EnsureHeartSlots();
        RefreshHearts();
    }

    /// <summary>
    /// Builds any heart icons the upgrades have earned beyond the three the
    /// scene provides, then lays the row out so it still fits.
    ///
    /// The row only has room for three at their authored size, so adding a
    /// fourth or fifth means tightening the spacing and shrinking them a
    /// little. Nothing moves while the player has the base three.
    /// </summary>
    private void EnsureHeartSlots()
    {
        if (hearts == null || hearts.Length == 0)
        {
            return;
        }

        if (maxHealth > hearts.Length)
        {
            Image template = hearts[hearts.Length - 1];
            if (template == null)
            {
                return;
            }

            Image[] grown = new Image[maxHealth];
            for (int i = 0; i < hearts.Length; i++)
            {
                grown[i] = hearts[i];
            }

            for (int i = hearts.Length; i < maxHealth; i++)
            {
                Image copy = Instantiate(template, template.transform.parent);
                copy.name = "Heart (" + i + ")";
                grown[i] = copy;
            }

            hearts = grown;
        }

        // Always, not only after an upgrade: the scene positions the first
        // heart and this owns the spacing of the whole row.
        LayOutHearts();
    }

    /// <summary>
    /// Lays the row out inside the space it actually has, shrinking the icons
    /// when there are more of them.
    ///
    /// The first attempt spread them across the width the three authored
    /// hearts spanned, 692 units, which reaches well past the score and put
    /// the fourth and fifth hearts on top of it. The row gets the strip from
    /// the left margin to just clear of the score instead.
    /// </summary>
    private void LayOutHearts()
    {
        RectTransform first = hearts[0] != null ? hearts[0].rectTransform : null;
        if (first == null)
        {
            return;
        }

        float authoredSize = first.sizeDelta.x;
        float step = rowWidth / hearts.Length;
        float size = Mathf.Min(authoredSize, step * 0.92f);

        // Positions are relative to the first heart's anchor, so the strip is
        // measured from where that heart already sits.
        float rowStart = first.anchoredPosition.x - authoredSize * 0.5f;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
            {
                continue;
            }

            RectTransform rect = hearts[i].rectTransform;
            rect.anchorMin = first.anchorMin;
            rect.anchorMax = first.anchorMax;
            rect.pivot = first.pivot;
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(rowStart + step * (i + 0.5f),
                                                first.anchoredPosition.y);
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead || amount <= 0 || IsInvulnerable)
        {
            return;
        }

        SetHealth(health - amount);

        bool fatal = health <= 0;

        // Raised for the fatal hit as well. Skipping it left the one hit that
        // ends the run with no flash and no camera kick, while every hit the
        // player survives had both.
        invulnerableUntil = Time.time + invulnerabilityDuration;
        Hurt?.Invoke(fatal ? 0f : invulnerabilityDuration);

        if (fatal)
        {
            isDead = true;
            Died?.Invoke();
        }
    }

    public void RestoreHealth(int amount = 1)
    {
        if (isDead || amount <= 0 || health >= maxHealth)
        {
            return;
        }

        SetHealth(health + amount);
        Healed?.Invoke();
    }

    private void SetHealth(int value)
    {
        int clamped = Mathf.Clamp(value, 0, maxHealth);
        if (clamped == health)
        {
            return;
        }

        health = clamped;
        RefreshHearts();
    }

    private void RefreshHearts()
    {
        if (hearts == null)
        {
            return;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i] == null)
            {
                continue;
            }

            bool slotExists = i < maxHealth;
            hearts[i].enabled = slotExists;

            if (slotExists)
            {
                hearts[i].sprite = i < health ? fullHeart : emptyHeart;
            }
        }
    }
}
