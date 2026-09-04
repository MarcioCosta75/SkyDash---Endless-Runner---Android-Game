using System.Collections;
using UnityEngine;

/// <summary>
/// Shows an object for a moment, over and over. Used for the level banner.
/// </summary>
public class ObjectActivator : MonoBehaviour
{
    [SerializeField]
    private GameObject objectToActivate;
    [Tooltip("Seconds before the first appearance.")]
    [SerializeField]
    private float initialDelay;
    [Tooltip("Seconds from one appearance to the next.")]
    [SerializeField]
    private float activationInterval = 100f;
    [Tooltip("Seconds the object stays visible.")]
    [SerializeField]
    private float activationDuration = 4f;

    private void Start()
    {
        if (objectToActivate == null)
        {
            enabled = false;
            return;
        }

        StartCoroutine(ActivateObjectRoutine());
    }

    private IEnumerator ActivateObjectRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        // Never let a short interval turn the gap into a negative wait.
        WaitForSeconds visible = new WaitForSeconds(activationDuration);
        WaitForSeconds hidden = new WaitForSeconds(Mathf.Max(0.1f, activationInterval - activationDuration));

        while (true)
        {
            objectToActivate.SetActive(true);
            yield return visible;
            objectToActivate.SetActive(false);
            yield return hidden;
        }
    }
}
