using System.Collections;
using UnityEngine;

public class ObjectActivator : MonoBehaviour
{
    public GameObject objectToActivate;
    public float initialDelay;
    public float activationInterval;
    public float activationDuration;

    private WaitForSeconds activationWait;

    private void Start()
    {
        activationWait = new WaitForSeconds(activationDuration);
        StartCoroutine(ActivateObjectRoutine());
    }

    private IEnumerator ActivateObjectRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            objectToActivate.SetActive(true);
            yield return activationWait;
            objectToActivate.SetActive(false);
            yield return new WaitForSeconds(activationInterval - activationDuration);
        }
    }
}
