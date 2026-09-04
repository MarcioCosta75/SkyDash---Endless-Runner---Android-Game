using UnityEngine;

/// <summary>Drops the shield power-up.</summary>
public class ShieldSpawner : FallingItemSpawner
{
    [Header("Shield")]
    [SerializeField]
    private GameObject shieldPrefab;

    protected override GameObject GetItemPrefab() => shieldPrefab;
}
