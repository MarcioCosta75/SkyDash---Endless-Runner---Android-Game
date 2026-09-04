using UnityEngine;

/// <summary>
/// Homing shot fired by the player. It only reacts to the enemy and to the
/// play area border, so falling pickups no longer swallow the shot.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField]
    private GameObject shootEffect;
    [SerializeField]
    private GameObject hitEffect;
    [SerializeField]
    private AudioClip activationSound;
    [SerializeField]
    private AudioClip destructionSound;

    [Header("Flight")]
    [SerializeField]
    private float moveSpeed = 10f;
    [Tooltip("Seconds between re-aiming at the enemy.")]
    [SerializeField]
    private float directionUpdateDelay = 0.1f;
    [Tooltip("The shot disappears after this many seconds.")]
    [SerializeField]
    private float lifetime = 5f;
    [SerializeField]
    private int damageAmount = 10;

    private GameObject alienEnemy;
    private Vector3 direction;
    private float directionUpdateTimer;

    private void Start()
    {
        if (shootEffect != null)
        {
            Instantiate(shootEffect, transform.position, Quaternion.identity);
        }

        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }

        direction = transform.up;
        UpdateProjectileDirection();

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        directionUpdateTimer -= Time.deltaTime;

        if (directionUpdateTimer <= 0f)
        {
            UpdateProjectileDirection();
            directionUpdateTimer = directionUpdateDelay;
        }

        // World space, so a rotated sprite cannot bend the flight path.
        transform.position += direction * (moveSpeed * Time.deltaTime);
    }

    private void UpdateProjectileDirection()
    {
        if (alienEnemy == null || !alienEnemy.activeInHierarchy)
        {
            alienEnemy = GameObject.FindGameObjectWithTag("AlienEnemy");
        }

        if (alienEnemy != null && alienEnemy.activeInHierarchy)
        {
            direction = (alienEnemy.transform.position - transform.position).normalized;
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("AlienEnemy"))
        {
            EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
            {
                enemyHealth = col.GetComponentInParent<EnemyHealth>();
            }

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
            }

            Explode();
            return;
        }

        if (col.CompareTag("Border"))
        {
            Destroy(gameObject);
        }
    }

    private void Explode()
    {
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }

        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

        Destroy(gameObject);
    }
}
