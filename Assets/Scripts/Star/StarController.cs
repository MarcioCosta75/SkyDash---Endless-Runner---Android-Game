using UnityEngine;

/// <summary>
/// A collectable star. While the magnet power-up is running the star pulls
/// itself towards the player, which keeps the cost at one check per star
/// instead of every star scanning every other star.
/// </summary>
public class StarController : MonoBehaviour
{
    [Tooltip("How strongly the magnet pulls this star, in units per second.")]
    [SerializeField]
    private float attractionSpeed = 6f;
    [Tooltip("The magnet only reaches stars closer than this, in world units.")]
    [SerializeField]
    private float attractionRange = 6f;

    private static ScoreManager cachedScoreManager;
    private static StarSpawner cachedSpawner;

    private void Update()
    {
        PlayerController player = PlayerController.Instance;
        if (player == null || !player.IsMagnetActive)
        {
            return;
        }

        Vector3 target = player.Position;
        float distance = Vector3.Distance(target, transform.position);
        if (distance > attractionRange)
        {
            return;
        }

        // Closer stars accelerate, which reads as a magnet rather than a tow.
        float pull = attractionSpeed * Mathf.Clamp01(1f - distance / attractionRange) + attractionSpeed * 0.25f;
        transform.position = Vector3.MoveTowards(transform.position, target, pull * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (cachedScoreManager == null)
        {
            cachedScoreManager = FindAnyObjectByType<ScoreManager>();
        }

        if (cachedScoreManager != null)
        {
            cachedScoreManager.AddStar();
        }

        if (cachedSpawner == null)
        {
            cachedSpawner = FindAnyObjectByType<StarSpawner>();
        }

        if (cachedSpawner != null)
        {
            cachedSpawner.PlayCollectSound();
        }

        Destroy(gameObject);
    }
}
