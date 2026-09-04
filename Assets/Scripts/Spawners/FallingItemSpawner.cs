using System.Collections;
using UnityEngine;

/// <summary>
/// Shared behaviour for everything that drops in from above the screen.
/// Subclasses only say which prefab to drop; timing, placement and the
/// downward push all live here.
/// </summary>
public abstract class FallingItemSpawner : MonoBehaviour
{
    [Header("Timing")]
    [Tooltip("Seconds between spawns. Ignored when maxSpawnInterval is above zero.")]
    [SerializeField]
    protected float spawnInterval = 5f;
    [Tooltip("Lower bound for a random wait. Leave both at zero for a fixed interval.")]
    [SerializeField]
    protected float minSpawnInterval;
    [SerializeField]
    protected float maxSpawnInterval;
    [Tooltip("Seconds to wait before the first spawn. Leave at 0 for a random "
             + "part of the interval, which keeps the spawners out of step.")]
    [SerializeField]
    protected float firstSpawnDelay;

    [Header("Movement")]
    [Tooltip("Downward speed in world units per second.")]
    [SerializeField]
    protected float fallSpeed = 2f;

    protected Camera mainCamera;

    /// <summary>The prefab to drop next. Return null to skip this spawn.</summary>
    protected abstract GameObject GetItemPrefab();

    protected virtual void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(FirstDelay());

        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(NextInterval());
        }
    }

    /// <summary>
    /// How long to wait for the very first item.
    ///
    /// With no delay set, every spawner fired on the same frame, so a run
    /// opened with a star, a heart, a shield, a magnet and an ammo box all
    /// falling together, and then nothing for half a minute. A random part of
    /// each spawner's own interval staggers them without needing a value
    /// tuned per spawner, and it keeps the rarer power-ups from arriving
    /// before the player has done anything.
    /// </summary>
    private float FirstDelay()
    {
        if (firstSpawnDelay > 0f)
        {
            return firstSpawnDelay;
        }

        float interval = maxSpawnInterval > 0f
            ? (Mathf.Min(minSpawnInterval, maxSpawnInterval) + maxSpawnInterval) * 0.5f
            : spawnInterval;

        return Mathf.Max(0.5f, interval * Random.Range(0.4f, 0.9f));
    }

    private float NextInterval()
    {
        if (maxSpawnInterval > 0f)
        {
            return Random.Range(Mathf.Min(minSpawnInterval, maxSpawnInterval), maxSpawnInterval);
        }

        return Mathf.Max(0.05f, spawnInterval);
    }

    protected virtual void Spawn()
    {
        GameObject prefab = GetItemPrefab();
        if (prefab == null)
        {
            return;
        }

        GameObject item = Instantiate(prefab, GetSpawnPosition(), Quaternion.identity);

        Rigidbody2D body = item.GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = 0f;
            body.linearVelocity = new Vector2(0f, -fallSpeed);
        }

        OnSpawned(item);
    }

    /// <summary>Hook for subclasses that need to touch the new item.</summary>
    protected virtual void OnSpawned(GameObject item)
    {
    }

    /// <summary>A random spot along the top edge of the camera view.</summary>
    protected virtual Vector3 GetSpawnPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return transform.position;
        }

        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, 0f, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, mainCamera.nearClipPlane));

        return new Vector3(Random.Range(bottomLeft.x, topRight.x), topRight.y, transform.position.z);
    }
}
