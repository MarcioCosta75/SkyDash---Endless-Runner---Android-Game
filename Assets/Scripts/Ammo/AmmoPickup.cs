using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 10; // Quantidade de munição a ser adicionada
    public AudioClip pickupSound; // Som a ser reproduzido quando o pickup é coletado

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerShooting playerShooting = collision.gameObject.GetComponentInChildren<PlayerShooting>();
            if (playerShooting != null)
            {
                playerShooting.AcquireProjectiles(ammoAmount);
                PlayPickupSound();
                Destroy(gameObject);
            }
        }
    }

    private void PlayPickupSound()
    {
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}
