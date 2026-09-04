using UnityEngine;

/// <summary>Drops the magnet power-up.</summary>
public class PowerUpSpawner : FallingItemSpawner
{
    [Header("Power-up")]
    [SerializeField]
    private GameObject powerUpPrefab;

    protected override GameObject GetItemPrefab() => powerUpPrefab;
}
