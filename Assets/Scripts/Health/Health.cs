using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player hit points and the heart row that shows them.
/// The UI is refreshed only when the value changes, and death is announced
/// through an event so nothing has to poll for it every frame.
/// </summary>
public class Health : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 3;

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

    private int health;
    private bool isDead;

    public int Current => health;
    public int Max => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        health = maxHealth;
    }

    private void Start()
    {
        RefreshHearts();
    }

    public void TakeDamage(int amount = 1)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        SetHealth(health - amount);

        if (health <= 0)
        {
            isDead = true;
            Died?.Invoke();
        }
    }

    public void RestoreHealth(int amount = 1)
    {
        if (isDead || amount <= 0)
        {
            return;
        }

        SetHealth(health + amount);
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
