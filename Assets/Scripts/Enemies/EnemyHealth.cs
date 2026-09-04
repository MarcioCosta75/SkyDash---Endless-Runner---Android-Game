using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Hit points for the alien enemy, with its health bar.</summary>
public class EnemyHealth : MonoBehaviour
{
    [Tooltip("Hit points on the first wave. Later waves add more.")]
    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private Slider healthSlider;

    /// <summary>Raised when this enemy is killed.</summary>
    public event Action Died;

    private int currentHealth;
    private int waveHealth;

    public bool IsAlive => currentHealth > 0;
    public int MaxHealth => waveHealth > 0 ? waveHealth : maxHealth;

    private void OnEnable()
    {
        ResetHealth();
        ShowBar(true);
    }

    private void OnDisable()
    {
        ShowBar(false);
    }

    /// <summary>
    /// The bar lives on the HUD, not on this object, so it does not switch
    /// off with the alien. A full health bar sitting there with no enemy on
    /// screen reads as a bug.
    /// </summary>
    private void ShowBar(bool visible)
    {
        if (healthSlider != null && healthSlider.gameObject.activeSelf != visible)
        {
            healthSlider.gameObject.SetActive(visible);
        }
    }

    /// <summary>Refills health, so the enemy can be reused on a later wave.</summary>
    public void ResetHealth()
    {
        currentHealth = MaxHealth;
        UpdateSlider();
    }

    /// <summary>
    /// Sets the hit points for this visit. Later waves are tougher, but only
    /// modestly: most of the extra difficulty comes from the alien being
    /// quicker and firing more, not from being a bigger bag of health that
    /// the player has no ammunition to empty.
    /// </summary>
    public void ConfigureForWave(int waveNumber)
    {
        MissileSpawner spawner = GetComponent<MissileSpawner>();
        waveHealth = spawner != null
            ? spawner.HealthForWave(maxHealth)
            : maxHealth;

        ResetHealth();
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        UpdateSlider();

        if (currentHealth == 0)
        {
            Died?.Invoke();
            gameObject.SetActive(false);
        }
    }

    private void UpdateSlider()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = MaxHealth;
            healthSlider.value = currentHealth;
        }
    }
}
