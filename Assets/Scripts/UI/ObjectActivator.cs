using System.Collections;
using UnityEngine;

/// <summary>
/// Shows an object for a moment. Used for the level banner, which appears
/// when the run reaches a new level rather than on a fixed timer that would
/// drift out of step with the real level.
/// </summary>
public class ObjectActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToActivate;
    [Tooltip("Show the object when the run reaches a new level.")]
    [SerializeField]
    private bool showOnLevelChange = true;
    [Tooltip("Seconds the object stays visible.")]
    [SerializeField]
    private float activationDuration = 4f;

    [Header("Fixed timer, used only when showOnLevelChange is off")]
    [Tooltip("Seconds before the first appearance.")]
    [SerializeField]
    private float initialDelay;
    [Tooltip("Seconds from one appearance to the next.")]
    [SerializeField]
    private float activationInterval = 100f;

    private Coroutine showRoutine;

    private void Awake()
    {
        // Hidden here rather than in Start: Start order between this and
        // ScoreManager is undefined, and ScoreManager.Start raises the first
        // LevelChanged. Hiding later would swallow the level 1 banner.
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (showOnLevelChange)
        {
            ScoreManager.LevelChanged += OnLevelChanged;
        }
    }

    private void OnDisable()
    {
        ScoreManager.LevelChanged -= OnLevelChanged;
    }

    private void Start()
    {
        if (objectToActivate == null)
        {
            enabled = false;
            return;
        }

        if (!showOnLevelChange)
        {
            StartCoroutine(RepeatOnTimer());
        }
    }

    private void OnLevelChanged(int level)
    {
        if (objectToActivate == null)
        {
            return;
        }

        if (showRoutine != null)
        {
            StopCoroutine(showRoutine);
        }

        showRoutine = StartCoroutine(ShowOnce());
    }

    private IEnumerator ShowOnce()
    {
        objectToActivate.SetActive(true);
        yield return new WaitForSeconds(activationDuration);
        objectToActivate.SetActive(false);
        showRoutine = null;
    }

    private IEnumerator RepeatOnTimer()
    {
        yield return new WaitForSeconds(initialDelay);

        // Never let a short interval turn the gap into a negative wait.
        WaitForSeconds hidden = new WaitForSeconds(Mathf.Max(0.1f, activationInterval - activationDuration));

        while (true)
        {
            yield return ShowOnce();
            yield return hidden;
        }
    }
}
