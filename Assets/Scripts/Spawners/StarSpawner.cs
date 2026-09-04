using UnityEngine;

/// <summary>Drops collectable stars and owns the pickup sound.</summary>
public class StarSpawner : FallingItemSpawner
{
    [Header("Star")]
    [SerializeField]
    private GameObject starPrefab;
    [SerializeField]
    private AudioClip collectSound;

    private AudioSource audioSource;

    protected override void Start()
    {
        audioSource = GetComponent<AudioSource>();
        base.Start();
    }

    protected override GameObject GetItemPrefab() => starPrefab;

    public void PlayCollectSound()
    {
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
    }
}
