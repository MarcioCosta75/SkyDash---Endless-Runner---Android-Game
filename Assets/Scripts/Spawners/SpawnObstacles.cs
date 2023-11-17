using UnityEngine;

public class SpawnObstacles : MonoBehaviour
{
    public GameObject[] obstacles;
    public float maxX;
    public float maxY;
    public float minX;
    public float minY;
    public float minTimeBetweenSpawn;
    public float maxTimeBetweenSpawn;

    private float spawnTime;
    private float timeBetweenSpawn;

    private void Start()
    {
        SetNextSpawnTime();
    }

    private void Update()
    {
        if (Time.time >= spawnTime)
        {
            Spawn();
            SetNextSpawnTime();
        }
    }

    private void SetNextSpawnTime()
    {
        timeBetweenSpawn = Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn);
        spawnTime = Time.time + timeBetweenSpawn;
    }

    private void Spawn()
    {
        int obstacleIndex = Random.Range(0, obstacles.Length);
        float randomX = Random.Range(minX, maxX);
        float randomY = Random.Range(minY, maxY);

        Vector3 spawnPosition = transform.position + new Vector3(randomX, randomY, 0f);
        Quaternion spawnRotation = Quaternion.identity;

        if (obstacleIndex == 0 || obstacleIndex == 1 || obstacleIndex == 3 || obstacleIndex == 4 || obstacleIndex == 6 || obstacleIndex == 7 || obstacleIndex == 9)
        {
            float randomRotationZ = Random.Range(0f, 360f);
            spawnRotation = Quaternion.Euler(0f, 0f, randomRotationZ);
        }

        GameObject obstacle = obstacles[obstacleIndex];
        Instantiate(obstacle, spawnPosition, spawnRotation);
    }
}
