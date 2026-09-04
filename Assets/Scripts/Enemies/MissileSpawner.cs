using System.Collections;
using UnityEngine;

/// <summary>
/// The alien: it flies in from above the screen, drifts across the top
/// dropping missiles, and leaves again when it is killed or when its time is
/// up. Hit points live in <see cref="EnemyHealth"/>, which is the single place
/// that decides when it dies.
///
/// The fly-in matters: switching a shooting enemy on in place gives the player
/// no warning. Coming down from off screen, without firing, does.
///
/// The retreat matters too. Without it an alien the player cannot kill would
/// sit there forever with the obstacle spawner switched off, and the run would
/// stall with nothing to do.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class MissileSpawner : MonoBehaviour
{
    [Header("Missiles")]
    [SerializeField]
    private GameObject missilePrefab;
    [Tooltip("Seconds between missiles on the first wave.")]
    [SerializeField]
    private float spawnInterval = 1f;
    [Tooltip("Downward speed of a missile on the first wave, in units per second.")]
    [SerializeField]
    private float spawnSpeed = 0.5f;
    [Tooltip("Vertical offset from the alien where missiles appear.")]
    [SerializeField]
    private float spawnOffsetY = -0.5f;

    [Header("Movement")]
    [Tooltip("Sideways speed on the first wave, in world units per second.")]
    [SerializeField]
    private float movementSpeed = 0.5f;

    [Header("Arrival and retreat")]
    [Tooltip("How far above its fighting position the alien starts.")]
    [SerializeField]
    private float entryHeight = 7f;
    [Tooltip("Seconds the fly-in takes. It does not shoot during this.")]
    [SerializeField]
    private float entryDuration = 1.3f;
    [Tooltip("Seconds it stays before giving up and leaving.")]
    [SerializeField]
    private float visitDuration = 26f;
    [Tooltip("Seconds the retreat takes.")]
    [SerializeField]
    private float exitDuration = 1f;

    [Header("Wave scaling")]
    [Tooltip("Extra hit points added per wave after the first.")]
    [SerializeField]
    private float healthPerWave = 20f;
    [Tooltip("Extra sideways speed added per wave.")]
    [SerializeField]
    private float speedPerWave = 0.35f;
    [Tooltip("Extra missile speed added per wave.")]
    [SerializeField]
    private float missileSpeedPerWave = 0.4f;
    [Tooltip("Missile interval is multiplied by this each wave, so they come faster.")]
    [SerializeField]
    private float intervalFactorPerWave = 0.84f;
    [Tooltip("Missiles never come closer together than this, in seconds.")]
    [SerializeField]
    private float minimumInterval = 0.35f;
    [Tooltip("Stars awarded for killing it, multiplied by the wave number.")]
    [SerializeField]
    private int starsPerWaveKilled = 10;

    [Header("Audio and effects")]
    [SerializeField]
    private AudioClip activeSound;
    [SerializeField]
    private AudioClip hitSound;
    [SerializeField]
    private AudioClip destructionSound;
    [SerializeField]
    private GameObject explosionEffectPrefab;

    private float direction = 1f;
    private Camera mainCamera;
    private AudioSource audioSource;
    private EnemyHealth health;
    private Coroutine visitRoutine;

    private Vector3 fightingPosition;
    private bool fightingPositionKnown;

    private int wave = 1;
    private float currentMovementSpeed;
    private float currentMissileSpeed;
    private float currentInterval;
    private bool firing;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        health = GetComponent<EnemyHealth>();
        mainCamera = Camera.main;

        RememberFightingPosition();
        ApplyWave(1);
    }

    /// <summary>
    /// The alien is a child of the moving rig, so its local position is the
    /// spot it should fight from. Captured once, before anything moves it.
    /// </summary>
    private void RememberFightingPosition()
    {
        if (!fightingPositionKnown)
        {
            fightingPosition = transform.localPosition;
            fightingPositionKnown = true;
        }
    }

    /// <summary>Sets the difficulty of this visit. Wave 1 is the first alien.</summary>
    public void ConfigureForWave(int waveNumber)
    {
        ApplyWave(Mathf.Max(1, waveNumber));
    }

    private void ApplyWave(int waveNumber)
    {
        wave = waveNumber;
        int extra = wave - 1;

        currentMovementSpeed = movementSpeed + speedPerWave * extra;
        currentMissileSpeed = spawnSpeed + missileSpeedPerWave * extra;
        currentInterval = Mathf.Max(minimumInterval,
                                    spawnInterval * Mathf.Pow(intervalFactorPerWave, extra));
    }

    /// <summary>Hit points for this wave, read by EnemyHealth.</summary>
    public int HealthForWave(int baseHealth)
    {
        return baseHealth + Mathf.RoundToInt(healthPerWave * (wave - 1));
    }

    private void OnEnable()
    {
        RememberFightingPosition();

        if (health != null)
        {
            health.Died += OnDied;
        }

        firing = false;
        visitRoutine = StartCoroutine(Visit());
    }

    private void OnDisable()
    {
        if (health != null)
        {
            health.Died -= OnDied;
        }

        if (visitRoutine != null)
        {
            StopCoroutine(visitRoutine);
            visitRoutine = null;
        }

        firing = false;

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Back to the fighting spot, ready for the next wave.
        if (fightingPositionKnown)
        {
            transform.localPosition = fightingPosition;
        }
    }

    private void Update()
    {
        if (firing)
        {
            MoveSpawner();
        }
    }

    /// <summary>Fly in, fight, then leave.</summary>
    private IEnumerator Visit()
    {
        yield return FlyTo(fightingPosition + Vector3.up * entryHeight, fightingPosition, entryDuration);

        if (audioSource != null && activeSound != null)
        {
            audioSource.clip = activeSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        firing = true;
        Coroutine missiles = StartCoroutine(SpawnMissiles());

        yield return new WaitForSeconds(visitDuration);

        // Time is up, so it gives up and climbs away.
        firing = false;
        StopCoroutine(missiles);

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        yield return FlyTo(transform.localPosition, fightingPosition + Vector3.up * entryHeight, exitDuration);

        visitRoutine = null;
        gameObject.SetActive(false);
    }

    private IEnumerator FlyTo(Vector3 from, Vector3 to, float seconds)
    {
        transform.localPosition = from;

        if (seconds <= 0f)
        {
            transform.localPosition = to;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / seconds));
            transform.localPosition = Vector3.Lerp(from, to, t);
            yield return null;
        }

        transform.localPosition = to;
    }

    private IEnumerator SpawnMissiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(minimumInterval, currentInterval));
            SpawnMissile();
        }
    }

    private void SpawnMissile()
    {
        if (missilePrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + new Vector3(0f, spawnOffsetY, 0f);
        GameObject missile = Instantiate(missilePrefab, spawnPosition, Quaternion.identity);

        Rigidbody2D body = missile.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            // Gravity would add to this and make the configured speed, and the
            // per-wave ramp, meaningless.
            body.gravityScale = 0f;
            body.linearVelocity = Vector2.down * currentMissileSpeed;
        }
    }

    private void MoveSpawner()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                return;
            }
        }

        transform.Translate(Vector3.right * (direction * currentMovementSpeed * Time.deltaTime));

        Vector3 viewportPosition = mainCamera.WorldToViewportPoint(transform.position);

        // Turn around at the screen edges.
        if (viewportPosition.x <= 0f || viewportPosition.x >= 1f)
        {
            viewportPosition.x = Mathf.Clamp01(viewportPosition.x);
            transform.position = mainCamera.ViewportToWorldPoint(viewportPosition);
            direction *= -1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Damage is applied by the projectile itself. This is only the
        // audible feedback for a hit that did not kill.
        if (collision.CompareTag("ProjectileSharp")
            && health != null && health.IsAlive
            && audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private void OnDied()
    {
        // Paying out gives a reason to shoot rather than just survive.
        ScoreManager score = FindAnyObjectByType<ScoreManager>();
        if (score != null && starsPerWaveKilled > 0)
        {
            score.AddStars(starsPerWaveKilled * wave);
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Played at a point, because EnemyHealth is about to disable this
        // object and a disabled AudioSource cannot play.
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }
    }
}
