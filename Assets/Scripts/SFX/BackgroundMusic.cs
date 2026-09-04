using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One music player that survives scene loads.
/// A second copy loaded with a new scene destroys itself, so restarting the
/// game never stacks two tracks or leaves a half-built duplicate behind.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    [Tooltip("Scenes whose load should start the music.")]
    [SerializeField]
    private string[] scenesWithMusic = { SceneNames.Game };
    [Tooltip("Where the track sits in the mix, before the player's volume setting.")]
    [Range(0f, 1f)]
    [SerializeField]
    private float mixLevel = 0.5f;

    private static BackgroundMusic instance;

    private AudioSource audioSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        audioSource = GetComponent<AudioSource>();

        // 2D, so the track does not fade as the camera climbs away from this
        // object, which never moves.
        audioSource.spatialBlend = 0f;
        ApplyVolume();

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayForScene(scene.name);
    }

    /// <summary>Starts the music if the named scene should have it, else stops it.</summary>
    public static void PlayForScene(string sceneName)
    {
        if (instance == null)
        {
            return;
        }

        bool wanted = false;
        for (int i = 0; i < instance.scenesWithMusic.Length; i++)
        {
            if (instance.scenesWithMusic[i] == sceneName)
            {
                wanted = true;
                break;
            }
        }

        if (wanted)
        {
            PlayMusic();
        }
        else
        {
            StopMusic();
        }
    }

    /// <summary>Re-reads the volume setting. Call after changing it.</summary>
    public static void RefreshVolume()
    {
        if (instance != null)
        {
            instance.ApplyVolume();
        }
    }

    private void ApplyVolume()
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(GameSettings.MusicVolume * mixLevel);
        }
    }

    public static void PlayMusic()
    {
        if (instance == null || instance.audioSource == null || instance.audioSource.isPlaying)
        {
            return;
        }

        instance.audioSource.enabled = true;
        instance.ApplyVolume();
        instance.audioSource.Play();
    }

    public static void StopMusic()
    {
        if (instance != null && instance.audioSource != null && instance.audioSource.isPlaying)
        {
            instance.audioSource.Stop();
        }
    }
}
