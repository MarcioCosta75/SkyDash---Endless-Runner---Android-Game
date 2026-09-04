using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Hit points for the alien enemy, with its health bar.</summary>
public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private int maxHealth = 100;
    [SerializeField]
    private Slider healthSlider;

    /// <summary>Raised when this enemy is killed.</summary>
    public event Action Died;

    private int currentHealth;

    public bool IsAlive => currentHealth > 0;

    private void OnEnable()
    {
        ResetHealth();
    }

    /// <summary>Refills health, so the enemy can be reused on a later wave.</summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        UpdateSlider();
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
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }
}
