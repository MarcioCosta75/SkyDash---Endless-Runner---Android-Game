using UnityEngine;

/// <summary>Drops collectable stars and owns the pickup sound.</summary>
public class StarSpawner : FallingItemSpawner
{
    [Header("Star")]
    [SerializeField]
    private GameObject starPrefab;
    [SerializeField]
    private AudioClip collectSound;

    protected override GameObject GetItemPrefab() => starPrefab;

    public void PlayCollectSound()
    {
        // Played at a point rather than through a source on this object: this
        // spawner is switched off at game over, and a disabled AudioSource
        // cannot play, so the last star collected would be silent.
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, Camera.main != null
                ? Camera.main.transform.position
                : transform.position);
        }
    }
}
