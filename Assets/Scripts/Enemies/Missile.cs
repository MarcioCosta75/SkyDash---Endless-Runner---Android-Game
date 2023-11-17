using UnityEngine;

public class Missile : MonoBehaviour
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

        if (collision.tag == "MagneticField")
        {
            Destroy(this.gameObject);
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity); // Instancia o efeito de part�culas no local da colis�o
            PlayCollisionSound();
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