using System.Collections;
using UnityEngine;

/// <summary>
/// Flashes a sprite near the end of a power-up so the player can see it is
/// about to run out. The timing comes from the power-up itself.
/// </summary>
public class BlinkEffect : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer targetSprite;
    [Tooltip("Seconds of flashing before the power-up ends.")]
    [SerializeField]
    private float warningTime = 3f;
    [Tooltip("Seconds between flashes.")]
    [SerializeField]
    private float blinkInterval = 0.25f;
    [Tooltip("Used when no power-up reports a duration.")]
    [SerializeField]
    private float blinkDuration = 10f;

    private Coroutine routine;

    private void Start()
    {
        if (targetSprite == null)
        {
            targetSprite = GetComponent<SpriteRenderer>();
        }

        if (routine == null)
        {
            BeginFor(blinkDuration);
        }
    }

    /// <summary>Schedules the warning flash for a power-up of this length.</summary>
    public void BeginFor(float totalDuration)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        routine = StartCoroutine(BlinkRoutine(totalDuration));
    }

    private IEnumerator BlinkRoutine(float totalDuration)
    {
        float quietTime = Mathf.Max(0f, totalDuration - warningTime);
        yield return new WaitForSeconds(quietTime);

        float elapsed = 0f;
        while (elapsed < warningTime)
        {
            if (targetSprite != null)
            {
                targetSprite.enabled = !targetSprite.enabled;
            }

            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }

        if (targetSprite != null)
        {
            targetSprite.enabled = true;
        }

        routine = null;
    }
}
