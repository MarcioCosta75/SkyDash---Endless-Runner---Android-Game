using UnityEngine;

/// <summary>
/// Drops ammo pickups, picking between prefabs by weight.
/// </summary>
public class AmmoSpawner : FallingItemSpawner
{
    [Header("Ammo")]
    [SerializeField]
    private GameObject[] ammoPickupPrefabs;
    [Tooltip("One weight per prefab. Higher means more common.")]
    [SerializeField]
    private float[] spawnProbabilities;

    protected override GameObject GetItemPrefab()
    {
        if (ammoPickupPrefabs == null || ammoPickupPrefabs.Length == 0)
        {
            return null;
        }

        if (spawnProbabilities == null || spawnProbabilities.Length != ammoPickupPrefabs.Length)
        {
            return ammoPickupPrefabs[Random.Range(0, ammoPickupPrefabs.Length)];
        }

        float total = 0f;
        for (int i = 0; i < spawnProbabilities.Length; i++)
        {
            total += Mathf.Max(0f, spawnProbabilities[i]);
        }

        if (total <= 0f)
        {
            return ammoPickupPrefabs[Random.Range(0, ammoPickupPrefabs.Length)];
        }

        float roll = Random.Range(0f, total);
        float cumulative = 0f;
        for (int i = 0; i < ammoPickupPrefabs.Length; i++)
        {
            cumulative += Mathf.Max(0f, spawnProbabilities[i]);
            if (roll <= cumulative)
            {
                return ammoPickupPrefabs[i];
            }
        }

        return ammoPickupPrefabs[ammoPickupPrefabs.Length - 1];
    }
}
