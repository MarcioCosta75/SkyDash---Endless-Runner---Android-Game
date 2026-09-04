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

    [Header("Heart display")]
    [SerializeField]
    private Image[] hearts;
    [SerializeField]
    private Sprite fullHeart;
    [SerializeField]
    private Sprite emptyHeart;

    /// <summary>Raised once, when health reaches zero.</summary>
    public event Action Died;

    /// <summary>Raised whenever the current value changes.</summary>
    public event Action<int, int> Changed;

    /// <summary>Raised when a hit lands, with the seconds of grace that follow.</summary>
    public event Action<float> Hurt;

    /// <summary>Raised when a heart is gained.</summary>
    public event Action Healed;

    private int health;
    private bool isDead;
    private float invulnerableUntil;

    public int Current => health;
    public int Max => maxHealth;
    public bool IsInvulnerable => Time.time < invulnerableUntil;

    private void Awake()
    {
        // A max of zero would mean a player that can never be hurt and never
        // dies, so the run would have no end.
        maxHealth = Mathf.Max(1, maxHealth);
        health = maxHealth;
    }

    private void Start()
    {
        RefreshHearts();
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead || amount <= 0 || IsInvulnerable)
        {
            return;
        }

        SetHealth(health - amount);

        if (health <= 0)
        {
            isDead = true;
            Died?.Invoke();
            return;
        }

        invulnerableUntil = Time.time + invulnerabilityDuration;
        Hurt?.Invoke(invulnerabilityDuration);
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
        Changed?.Invoke(health, maxHealth);
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
