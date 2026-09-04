using UnityEngine;

/// <summary>The health pickup. Gives one heart back.</summary>
public class Heart : MonoBehaviour
{
    private bool used;
    [SerializeField]
    private AudioClip collisionSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (used || !collision.CompareTag("Player"))
        {
            return;
        }

        Health playerHealth = collision.GetComponent<Health>();
        if (playerHealth == null)
        {
            playerHealth = collision.GetComponentInParent<Health>();
        }

        if (playerHealth == null)
        {
            return;
        }

        used = true;
        playerHealth.RestoreHealth();

        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
