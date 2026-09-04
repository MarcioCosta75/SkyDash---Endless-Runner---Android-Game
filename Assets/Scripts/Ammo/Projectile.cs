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
    [Tooltip("How far the shot looks for an obstacle when no alien is out.")]
    [SerializeField]
    private float obstacleSearchRadius = 9f;

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
        Vector3? target = FindTarget();

        if (target.HasValue)
        {
            direction = ((Vector2)(target.Value - transform.position)).normalized;
        }

        ApplyVelocity();
    }

    /// <summary>
    /// What to steer at: the alien if one is out, otherwise the nearest
    /// obstacle ahead.
    ///
    /// Shots used to fly straight up when there was no alien, which wasted
    /// most of them. Homing on the nearest asteroid means a shot fired to
    /// clear the way actually clears something.
    /// </summary>
    private Vector3? FindTarget()
    {
        if (alienEnemy == null || !alienEnemy.activeInHierarchy)
        {
            alienEnemy = GameObject.FindGameObjectWithTag("AlienEnemy");
        }

        if (alienEnemy != null && alienEnemy.activeInHierarchy)
        {
            return alienEnemy.transform.position;
        }

        return NearestObstacleAhead();
    }

    private Vector3? NearestObstacleAhead()
    {
        // Only what is in front, so a shot never turns back on itself.
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, obstacleSearchRadius);

        float bestDistance = float.MaxValue;
        Vector3? best = null;

        for (int i = 0; i < nearby.Length; i++)
        {
            Collider2D candidate = nearby[i];
            if (candidate == null)
            {
                continue;
            }

            if (!candidate.CompareTag("Obstacle") && !candidate.CompareTag("Missile"))
            {
                continue;
            }

            Vector3 position = candidate.transform.position;
            if (position.y < transform.position.y)
            {
                continue;
            }

            float distance = ((Vector2)(position - transform.position)).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                best = position;
            }
        }

        return best;
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
