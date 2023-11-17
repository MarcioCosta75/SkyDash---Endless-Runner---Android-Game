using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    public int health;
    public int maxHealth; // Nova vari�vel para armazenar a quantidade m�xima de vidas
    public int numOfHearts;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    private void Start()
    {
        health = maxHealth; // Definir a vida inicial para o valor m�ximo de vidas
    }

    void Update()
    {
        if (health > maxHealth)
        {
            health = maxHealth;
        }

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < health)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }

            if (i < maxHealth)
            {
                hearts[i].enabled = true;
            }
            else
            {
                hearts[i].enabled = false;
            }
        }
    }

    public void TakeDamage()
    {
        health--;
        if (health <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth()
    {
        health++;
        if (health > maxHealth)
        {
            health = maxHealth;
        }
    }

    private void Die()
    {
        // Aqui voc� pode adicionar o c�digo para tratar o caso de morte do jogador
        // Exemplo: Reiniciar o n�vel, exibir um painel de game over, etc.
        Debug.Log("Player morreu");
    }
}
