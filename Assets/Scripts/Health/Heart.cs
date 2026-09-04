using UnityEngine;

/// <summary>The health pickup. Gives one heart back.</summary>
public class Heart : MonoBehaviour
{
    [SerializeField]
    private AudioClip collisionSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
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

        playerHealth.RestoreHealth();

        if (collisionSound != null)
        {
            AudioSource.PlayClipAtPoint(collisionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
