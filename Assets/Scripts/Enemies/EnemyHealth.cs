using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 100; // Vida máxima do inimigo
    public Slider healthSlider; // Referência ao componente Slider para exibir a barra de vida

    private int currentHealth; // Vida atual do inimigo

    private void Start()
    {
        currentHealth = maxHealth;

        // Atualiza o valor máximo da barra de vida
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;

        // Atualiza o valor atual da barra de vida
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Lógica de morte do inimigo
        gameObject.SetActive(false);
    }
}
