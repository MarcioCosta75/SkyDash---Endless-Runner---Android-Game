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
    [Tooltip("Seconds to wait before the first spawn.")]
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
        if (firstSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(firstSpawnDelay);
        }

        while (true)
        {
            Spawn();
            yield return new WaitForSeconds(NextInterval());
        }
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
