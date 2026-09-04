using UnityEngine;

/// <summary>
/// Anything that costs the player a heart on contact: asteroids, clouds,
/// enemy missiles. The border cleans up whatever the player dodges.
/// </summary>
public class DamagingHazard : MonoBehaviour
{
    [SerializeField]
    private GameObject particleEffectPrefab;
    [SerializeField]
    private AudioClip collisionSound;
    [Tooltip("Hearts removed on contact.")]
    [SerializeField]
    private int damage = 1;
    [Tooltip("Destroyed by the shield bubble as well as by the player.")]
    [SerializeField]
    private bool destroyedByMagneticField = true;
    [Tooltip("Destroyed by the player's shots.")]
    [SerializeField]
    private bool destroyedByShots = true;

    private bool spent;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (spent)
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth == null)
            {
                playerHealth = collision.GetComponentInParent<Health>();
            }

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Impact();
            return;
        }

        if (destroyedByMagneticField && collision.CompareTag("MagneticField"))
        {
            Impact();
            return;
        }

        if (destroyedByShots && collision.CompareTag("ProjectileSharp"))
        {
            Impact();
        }
    }

    private void Impact()
    {
        spent = true;

        if (particleEffectPrefab != null)
        {
            Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
        }

        if (collisionSound != null)
        {
            SoundPlayer.Play(collisionSound, 0.85f);
        }

        Destroy(gameObject);
    }
}
