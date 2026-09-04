using UnityEngine;

/// <summary>
/// Plays the game's one-shot sounds through a small pool of 2D sources.
///
/// It replaces AudioSource.PlayClipAtPoint, which had two problems. It creates
/// a 3D source, and gameplay sits ten units from the camera on the z axis, so
/// every pickup, hit and explosion was playing at roughly a tenth of its
/// volume while the music played at full. And it allocates a GameObject per
/// sound, which during an alien wave means one or two a second thrown away.
///
/// It also varies the pitch a little. The star chime fires every few seconds
/// for a whole run, and an identical sample repeated that often reads as a
/// stuck machine.
/// </summary>
public class SoundPlayer : MonoBehaviour
{
    /// <summary>How many sounds can overlap before the oldest is reused.</summary>
    private const int Voices = 12;

    /// <summary>Random pitch spread, so repeats do not sound identical.</summary>
    private const float PitchSpread = 0.06f;

    private static SoundPlayer instance;

    private AudioSource[] voices;
    private int next;

    /// <summary>
    /// Plays a clip. The scale is a per-sound mix level, applied on top of the
    /// player's effects volume.
    /// </summary>
    public static void Play(AudioClip clip, float scale = 1f)
    {
        if (clip == null || scale <= 0f)
        {
            return;
        }

        SoundPlayer player = Ensure();
        if (player == null)
        {
            return;
        }

        player.PlayInternal(clip, scale, PitchSpread);
    }

    /// <summary>
    /// Plays a clip without pitch variation, for sounds where a wobble would
    /// be noticeable, such as a long explosion.
    /// </summary>
    public static void PlayFlat(AudioClip clip, float scale = 1f)
    {
        if (clip == null || scale <= 0f)
        {
            return;
        }

        SoundPlayer player = Ensure();
        if (player == null)
        {
            return;
        }

        player.PlayInternal(clip, scale, 0f);
    }

    private void PlayInternal(AudioClip clip, float scale, float spread)
    {
        AudioSource source = voices[next];
        next = (next + 1) % voices.Length;

        source.clip = clip;
        source.volume = Mathf.Clamp01(GameSettings.SfxVolume * scale);
        source.pitch = spread > 0f ? 1f + Random.Range(-spread, spread) : 1f;
        source.Play();
    }

    /// <summary>
    /// Builds the pool on first use and keeps it across scene loads, so a
    /// sound started as the run ends is not cut off by the reload.
    /// </summary>
    private static SoundPlayer Ensure()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject host = new GameObject("SoundPlayer");
        DontDestroyOnLoad(host);

        instance = host.AddComponent<SoundPlayer>();
        instance.Build();
        return instance;
    }

    private void Build()
    {
        voices = new AudioSource[Voices];

        for (int i = 0; i < voices.Length; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;

            // 2D. This is the whole point: no distance falloff, so a sound
            // plays at the volume it was mixed at.
            source.spatialBlend = 0f;

            voices[i] = source;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
