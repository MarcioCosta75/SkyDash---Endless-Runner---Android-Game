using UnityEngine;

/// <summary>
/// The shield bubble around the player. Breaks obstacles on contact.
/// The fade-out flashing is handled by <see cref="BlinkEffect"/>.
/// </summary>
public class MagneticField : MonoBehaviour
{
    [SerializeField]
    private AudioClip collisionSound;
    [SerializeField]
    private GameObject particleEffect;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Obstacle"))
        {
            return;
        }

        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, collision.transform.position);
        }

        if (particleEffect != null)
        {
            Instantiate(particleEffect, collision.transform.position, Quaternion.identity);
        }

        Destroy(collision.gameObject);
    }
}
