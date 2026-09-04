using UnityEngine;

/// <summary>
/// Drops obstacles at random spots inside a rectangle above the player.
/// </summary>
public class SpawnObstacles : MonoBehaviour
{
    [SerializeField]
    private GameObject[] obstacles;

    [Header("Spawn area, relative to this object")]
    [SerializeField]
    private float minX = -2.3f;
    [SerializeField]
    private float maxX = 2.3f;
    [SerializeField]
    private float minY = -3.4f;
    [SerializeField]
    private float maxY = 3.4f;

    [Header("Timing")]
    [SerializeField]
    private float minTimeBetweenSpawn = 0.7f;
    [SerializeField]
    private float maxTimeBetweenSpawn = 1f;

    [Header("Look")]
    [Tooltip("Obstacles spawn at a random angle. Turn off for upright sprites.")]
    [SerializeField]
    private bool randomiseRotation = true;

    private float spawnTime;

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
        spawnTime = Time.time + Random.Range(minTimeBetweenSpawn, maxTimeBetweenSpawn);
    }

    private void Spawn()
    {
        if (obstacles == null || obstacles.Length == 0)
        {
            return;
        }

        GameObject prefab = obstacles[Random.Range(0, obstacles.Length)];
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPosition = transform.position + new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
        Quaternion spawnRotation = randomiseRotation
            ? Quaternion.Euler(0f, 0f, Random.Range(0f, 360f))
            : Quaternion.identity;

        Instantiate(prefab, spawnPosition, spawnRotation);
    }
}
