using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public GameObject particleEffectPrefab; // Prefab do efeito de part�culas
    public AudioClip collisionSound; // Som de colis�o

    private Health playerHealth;

    private void Start()
    {
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Border")
        {
            Destroy(this.gameObject);
        }
        else if (collision.tag == "Player")
        {
            if (collision.gameObject.GetComponent<Heart>() != null)
            {
                playerHealth.RestoreHealth();
            }
            else
            {
                playerHealth.TakeDamage();
            }

            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity); // Instancia o efeito de part�culas no local da colis�o
            PlayCollisionSound();
            Destroy(this.gameObject);
        }
    }

    private void PlayCollisionSound()
    {
        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }
    }
}
