using UnityEngine;

/// <summary>Refills the player's ammo.</summary>
public class AmmoPickup : MonoBehaviour
{
    [SerializeField]
    private int ammoAmount = 2;
    [SerializeField]
    private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
        {
            return;
        }

        PlayerShooting playerShooting = collision.GetComponentInChildren<PlayerShooting>();
        if (playerShooting == null)
        {
            playerShooting = collision.GetComponentInParent<PlayerShooting>();
        }

        if (playerShooting == null)
        {
            return;
        }

        playerShooting.AcquireProjectiles(ammoAmount);

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        Destroy(gameObject);
    }
}
