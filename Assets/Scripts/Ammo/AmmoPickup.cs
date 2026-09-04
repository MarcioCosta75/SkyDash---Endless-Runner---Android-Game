using UnityEngine;

/// <summary>Refills the player's ammo.</summary>
public class AmmoPickup : MonoBehaviour
{
    private bool used;
    [SerializeField]
    private int ammoAmount = 2;
    [SerializeField]
    private AudioClip pickupSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used || !collision.CompareTag("Player"))
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

        used = true;
        playerShooting.AcquireProjectiles(ammoAmount);

        if (pickupSound != null)
        {
            SoundPlayer.Play(pickupSound, 0.7f);
        }

        Destroy(gameObject);
    }
}
