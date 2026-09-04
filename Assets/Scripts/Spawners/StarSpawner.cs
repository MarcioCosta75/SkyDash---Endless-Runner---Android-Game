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
        // Through the shared player, not a source on this object: this spawner
        // is switched off at game over and a disabled AudioSource cannot play,
        // so the last star of a run would be silent. Low in the mix, because
        // it fires every few seconds for the whole run.
        SoundPlayer.Play(collectSound, 0.5f);
    }
}
