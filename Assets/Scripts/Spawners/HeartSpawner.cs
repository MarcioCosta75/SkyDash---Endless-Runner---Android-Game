using UnityEngine;

/// <summary>Drops the health pickup.</summary>
public class HeartSpawner : FallingItemSpawner
{
    [Header("Heart")]
    [SerializeField]
    private GameObject heartPrefab;

    protected override GameObject GetItemPrefab() => heartPrefab;
}
