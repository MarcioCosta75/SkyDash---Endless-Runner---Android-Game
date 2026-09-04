using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Countdown bar for a power-up.
/// The power-up itself announces how long it lasts, so the bar can never
/// disagree with the effect it is showing.
/// </summary>
public abstract class PowerUpTimerBar : MonoBehaviour
{
    [SerializeField]
    private Image timer_linear_image;
    [Tooltip("Shown while the power-up is running.")]
    [SerializeField]
    private GameObject objectToActivate;
    [Tooltip("Used only if the power-up does not report its own duration.")]
    [SerializeField]
    private float fillDuration = 5f;

    private Coroutine countdown;

    protected virtual void Awake()
    {
        ResetBar();
        Subscribe();
    }

    protected virtual void OnDestroy()
    {
        Unsubscribe();
    }

    protected abstract void Subscribe();
    protected abstract void Unsubscribe();

    /// <summary>Starts, or restarts, the bar for the given duration.</summary>
    protected void StartCountdown(float duration)
    {
        if (duration <= 0f)
        {
            duration = fillDuration;
        }

        if (countdown != null)
        {
            StopCoroutine(countdown);
        }

        countdown = StartCoroutine(RunCountdown(duration));
    }

    private IEnumerator RunCountdown(float duration)
    {
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            if (timer_linear_image != null)
            {
                // Straight line from full to empty.
                timer_linear_image.fillAmount = Mathf.Clamp01(1f - elapsed / duration);
            }

            yield return null;
        }

        countdown = null;
        ResetBar();
    }

    private void ResetBar()
    {
        if (timer_linear_image != null)
        {
            timer_linear_image.fillAmount = 0f;
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }
}
