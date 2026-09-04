using System.Collections;
using UnityEngine;

/// <summary>
/// Brings the alien enemy in after a delay, and brings it back for another
/// pass once the player has killed it.
/// </summary>
public class EnemyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject alienEnemy;
    [Tooltip("Obstacle spawner that runs while no enemy is on screen.")]
    [SerializeField]
    private GameObject spawnPoint;
    [Tooltip("Seconds before the enemy first appears.")]
    [SerializeField]
    private float activationDelay = 100f;
    [Tooltip("Seconds between one enemy dying and the next arriving.")]
    [SerializeField]
    private float respawnDelay = 90f;

    private void Start()
    {
        SetSpawnPointActive(true);

        if (alienEnemy != null)
        {
            alienEnemy.SetActive(false);
            StartCoroutine(EnemyCycle());
        }
    }

    private IEnumerator EnemyCycle()
    {
        yield return new WaitForSeconds(activationDelay);

        while (true)
        {
            ActivateAlienEnemy();

            // Wait out the whole visit before counting down to the next one.
            while (alienEnemy != null && alienEnemy.activeSelf)
            {
                yield return null;
            }

            SetSpawnPointActive(true);

            if (alienEnemy == null)
            {
                yield break;
            }

            yield return new WaitForSeconds(respawnDelay);
        }
    }

    private void ActivateAlienEnemy()
    {
        if (alienEnemy == null)
        {
            return;
        }

        EnemyHealth health = alienEnemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.ResetHealth();
        }

        alienEnemy.SetActive(true);
        SetSpawnPointActive(false);
    }

    private void SetSpawnPointActive(bool active)
    {
        if (spawnPoint != null && spawnPoint.activeSelf != active)
        {
            spawnPoint.SetActive(active);
        }
    }
}
