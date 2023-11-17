using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public GameObject alienEnemy;
    public GameObject spawnPoint;
    public float activationDelay = 300f;

    private float elapsedTime = 0f;
    private bool alienEnemyActivated = false;

    private void Start()
    {
        ActivateSpawnPoint();
    }

    private void Update()
    {
        // Ativa o AlienEnemy após o tempo de delay
        if (!alienEnemyActivated)
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= activationDelay)
            {
                ActivateAlienEnemy();
            }
        }
        else
        {
            // Verifica se o AlienEnemy está desativado e ativa o spawnPoint
            if (alienEnemy == null || !alienEnemy.activeSelf)
            {
                ActivateSpawnPoint();
            }
        }
    }

    private void ActivateAlienEnemy()
    {
        alienEnemy.SetActive(true);
        alienEnemyActivated = true;
        spawnPoint.SetActive(false);
    }

    private void ActivateSpawnPoint()
    {
        spawnPoint.SetActive(true);
    }
}
