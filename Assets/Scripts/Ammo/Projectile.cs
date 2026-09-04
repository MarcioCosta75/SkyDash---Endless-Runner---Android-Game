using UnityEngine;

/// <summary>
/// Homing shot fired by the player. It only reacts to the enemy and to the
/// play area border, so falling pickups no longer swallow the shot.
/// Movement goes through the Rigidbody2D alone, so the physics velocity and
/// the homing cannot fight each other.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
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
    [Tooltip("Speed in world units per second. The shooter can override this.")]
    [SerializeField]
    private float moveSpeed = 50f;
    [Tooltip("Seconds between re-aiming at the enemy.")]
    [SerializeField]
    private float directionUpdateDelay = 0.1f;
    [Tooltip("The shot disappears after this many seconds.")]
    [SerializeField]
    private float lifetime = 5f;
    [SerializeField]
    private int damageAmount = 10;

    private Rigidbody2D body;
    private GameObject alienEnemy;
    private Vector2 direction;
    private float directionUpdateTimer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        direction = transform.up;
    }

    /// <summary>
    /// Sets the shot on its way. Called by the shooter right after spawning,
    /// so speed lives in one place instead of being applied twice.
    /// </summary>
    public void Launch(Vector2 startDirection, float speed)
    {
        if (startDirection.sqrMagnitude > 0f)
        {
            direction = startDirection.normalized;
        }

        if (speed > 0f)
        {
            moveSpeed = speed;
        }

        ApplyVelocity();
    }

    private void Start()
    {
        if (shootEffect != null)
        {
            Instantiate(shootEffect, transform.position, Quaternion.identity);
        }

        if (activationSound != null)
        {
            SoundPlayer.Play(activationSound, 0.45f);
        }

        UpdateProjectileDirection();
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        directionUpdateTimer -= Time.fixedDeltaTime;

        if (directionUpdateTimer <= 0f)
        {
            directionUpdateTimer = directionUpdateDelay;
            UpdateProjectileDirection();
        }
    }

    private void UpdateProjectileDirection()
    {
        if (alienEnemy == null || !alienEnemy.activeInHierarchy)
        {
            alienEnemy = GameObject.FindGameObjectWithTag("AlienEnemy");
        }

        if (alienEnemy != null && alienEnemy.activeInHierarchy)
        {
            direction = ((Vector2)(alienEnemy.transform.position - transform.position)).normalized;
        }

        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        if (body != null)
        {
            body.linearVelocity = direction * moveSpeed;
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

        // Spent on an obstacle, but the obstacle plays its own effect and
        // sound, so this one does not add a second explosion on top.
        if (col.CompareTag("Obstacle") || col.CompareTag("Missile"))
        {
            Destroy(gameObject);
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
            SoundPlayer.Play(destructionSound, 0.6f);
        }

        Destroy(gameObject);
    }
}
