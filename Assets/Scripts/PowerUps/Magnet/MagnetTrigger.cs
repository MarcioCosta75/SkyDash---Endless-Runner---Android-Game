using System.Collections.Generic;
using UnityEngine;

public class MagnetTrigger : MonoBehaviour
{
    private Transform playerTransform;
    private PlayerController playerController;

    private List<Transform> starTransforms = new List<Transform>();

    [SerializeField]
    private float attractionForce = 5f;

    private void Start()
    {
        GameObject playerObject = GameObject.Find("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.GetComponent<Transform>();
            playerController = playerObject.GetComponent<PlayerController>();
        }

        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Star"))
            {
                starTransforms.Add(obj.transform);
            }
        }
    }

    private void Update()
    {
        if (playerController != null && playerController.isMagnet)
        {
            foreach (Transform starTransform in starTransforms)
            {
                if (starTransform != null)
                {
                    Vector3 direction = (playerTransform.position - starTransform.position).normalized;
                    float distance = Vector3.Distance(playerTransform.position, starTransform.position);
                    float force = attractionForce / distance;

                    starTransform.position += direction * force * Time.deltaTime;
                }
            }
        }
    }
}
