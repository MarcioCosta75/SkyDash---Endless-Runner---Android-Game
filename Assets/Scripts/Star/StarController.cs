using System.Collections;
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

    [Header("Collect")]
    [Tooltip("Seconds the star takes to pop and vanish when collected.")]
    [SerializeField]
    private float popDuration = 0.16f;
    [Tooltip("How much it grows while popping.")]
    [SerializeField]
    private float popScale = 1.9f;

    private static ScoreManager cachedScoreManager;
    private static StarSpawner cachedSpawner;

    private bool collected;

    private void Update()
    {
        PlayerController player = PlayerController.Instance;
        if (collected || player == null || !player.IsMagnetActive)
        {
            return;
        }

        // The player sits on a different z plane to the falling items, so
        // range and movement are measured on the screen plane only. Using a
        // 3D distance here would never be under the range.
        Vector3 position = transform.position;
        Vector2 target = player.Position;
        Vector2 flat = new Vector2(position.x, position.y);

        float distance = Vector2.Distance(target, flat);
        if (distance > attractionRange)
        {
            return;
        }

        // Closer stars accelerate, which reads as a magnet rather than a tow.
        float pull = attractionSpeed * Mathf.Clamp01(1f - distance / attractionRange)
                     + attractionSpeed * 0.25f;

        Vector2 moved = Vector2.MoveTowards(flat, target, pull * Time.deltaTime);

        // z is left alone so the star keeps its render depth.
        transform.position = new Vector3(moved.x, moved.y, position.z);
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
        // The player has three trigger colliders, so this can fire twice.
        if (collected)
        {
            return;
        }

        collected = true;

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

        StartCoroutine(Pop());
    }

    /// <summary>
    /// Flares out and fades as it disappears, so collecting a star is
    /// something you see and not only something you hear.
    /// </summary>
    private IEnumerator Pop()
    {
        // Stop it being collected again, and stop it drifting on the magnet
        // while it plays out.
        Collider2D[] colliders = GetComponents<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Vector3 startScale = transform.localScale;
        Color startColour = sprite != null ? sprite.color : Color.white;

        float elapsed = 0f;
        while (elapsed < popDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popDuration);

            transform.localScale = startScale * Mathf.Lerp(1f, popScale, t);

            if (sprite != null)
            {
                sprite.color = new Color(startColour.r, startColour.g, startColour.b,
                                         startColour.a * (1f - t));
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}
