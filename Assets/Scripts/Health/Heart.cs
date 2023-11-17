using UnityEngine;

public class Heart : MonoBehaviour
{
    public AudioClip collisionSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Health playerHealth = collision.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.RestoreHealth();
                Destroy(gameObject);

                if (collisionSound != null)
                {
                    AudioSource.PlayClipAtPoint(collisionSound, transform.position);
                }
            }
        }
    }
}
