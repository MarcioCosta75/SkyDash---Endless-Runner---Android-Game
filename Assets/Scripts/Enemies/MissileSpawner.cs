using System.Collections;
using UnityEngine;

/// <summary>
/// The alien enemy: drifts across the top of the screen dropping missiles,
/// and breaks after enough hits.
/// </summary>
public class MissileSpawner : MonoBehaviour
{
    [Header("Missiles")]
    [SerializeField]
    private GameObject missilePrefab;
    [Tooltip("Seconds between missiles.")]
    [SerializeField]
    private float spawnInterval = 1f;
    [Tooltip("Downward speed of a missile, in world units per second.")]
    [SerializeField]
    private float spawnSpeed = 0.5f;
    [Tooltip("Vertical offset from the enemy where missiles appear.")]
    [SerializeField]
    private float spawnOffsetY = -0.5f;

    [Header("Movement")]
    [Tooltip("Sideways speed of the enemy, in world units per second.")]
    [SerializeField]
    private float movementSpeed = 0.5f;

    [Header("Damage")]
    [SerializeField]
    private int projectileSharpHitsToDestroy = 10;

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
    private int projectileSharpHits;
    private Camera mainCamera;
    private AudioSource audioSource;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        projectileSharpHits = 0;

        if (audioSource != null && activeSound != null)
        {
            audioSource.clip = activeSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        spawnRoutine = StartCoroutine(SpawnMissiles());
    }

    private void OnDisable()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private void Update()
    {
        MoveSpawner();
    }

    private IEnumerator SpawnMissiles()
    {
        while (true)
        {
            yield return new WaitForSeconds(Mathf.Max(0.05f, spawnInterval));
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
            body.linearVelocity = Vector2.down * spawnSpeed;
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

        transform.Translate(Vector3.right * (direction * movementSpeed * Time.deltaTime));

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
        if (!collision.CompareTag("ProjectileSharp"))
        {
            return;
        }

        projectileSharpHits++;

        if (projectileSharpHits >= projectileSharpHitsToDestroy)
        {
            DestroySpawner();
        }
        else if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
    }

    private void DestroySpawner()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // Played at a point, because a disabled object cannot play audio.
        if (destructionSound != null)
        {
            AudioSource.PlayClipAtPoint(destructionSound, transform.position);
        }

        gameObject.SetActive(false);
    }
}
