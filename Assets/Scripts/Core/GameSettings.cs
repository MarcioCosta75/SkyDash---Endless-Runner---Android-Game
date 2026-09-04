using UnityEngine;

/// <summary>
/// Player-facing options, stored on the device.
/// The settings scene writes them and the game reads them, so neither needs a
/// reference to the other.
/// </summary>
public static class GameSettings
{
    private const string TouchSensitivityKey = "touchSensitivity";
    private const string MusicVolumeKey = "musicVolume";
    private const string SfxVolumeKey = "sfxVolume";

    /// <summary>Slider range in the settings scene.</summary>
    public const float SliderMinimum = 0f;
    public const float SliderMaximum = 10f;

    private const float LowestMultiplier = 0.5f;
    private const float HighestMultiplier = 1.5f;

    /// <summary>Middle of the slider, which means "unchanged".</summary>
    public const float DefaultSliderValue = 5f;

    /// <summary>
    /// How far one button press moves the astronaut, as a multiplier of the value
    /// set on the player. 1 is the tuned default.
    /// </summary>
    public static float TouchSensitivity
    {
        get => SliderToMultiplier(SliderValue);
    }

    /// <summary>The raw slider position, which is what the settings UI shows.</summary>
    public static float SliderValue
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(TouchSensitivityKey, DefaultSliderValue),
                           SliderMinimum, SliderMaximum);
        set
        {
            // No Save here: this is written on every frame of a slider drag,
            // and PlayerPrefs.Save blocks on a disk write.
            PlayerPrefs.SetFloat(TouchSensitivityKey, Mathf.Clamp(value, SliderMinimum, SliderMaximum));
        }
    }

    /// <summary>Flushes pending changes to disk. Call when leaving settings.</summary>
    public static void Save()
    {
        PlayerPrefs.Save();
    }

    /// <summary>Music level, 0 to 1.</summary>
    public static float MusicVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.7f));
        set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
    }

    /// <summary>Sound effect level, 0 to 1.</summary>
    public static float SfxVolume
    {
        get => Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, 0.8f));
        set => PlayerPrefs.SetFloat(SfxVolumeKey, Mathf.Clamp01(value));
    }

    public static float SliderToMultiplier(float sliderValue)
    {
        float t = Mathf.InverseLerp(SliderMinimum, SliderMaximum, sliderValue);
        return Mathf.Lerp(LowestMultiplier, HighestMultiplier, t);
    }
}
